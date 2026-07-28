// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;

using static ReactiveUI.Binding.SourceGenerators.CodeGeneration.GeneratedTypeNames;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Shared code generation logic for property observation APIs (WhenChanged, WhenChanging, WhenAnyValue).
/// Generates concrete typed extension method overloads and per-invocation observation methods.
/// Uses the plugin system to emit platform-specific observation code.
/// </summary>
internal static class ObservationCodeGenerator
{
    /// <summary>
    /// The maximum number of property expressions for which a runtime affinity check is emitted.
    /// This matches the available <c>RuntimeObservationFallback</c> method signatures.
    /// </summary>
    private const int MaxAffinityFallbackPropertyCount = 3;

    /// <summary>
    /// Returns the fully qualified type name for casting the observer parameter back to the
    /// concrete source type. Falls back to <c>"object"</c> when <paramref name="classInfo"/>
    /// is null — a branch that is unreachable in normal generator pipelines but testable directly.
    /// </summary>
    /// <param name="classInfo">The class binding info, or null.</param>
    /// <returns>The fully qualified type name string.</returns>
    internal static string GetTypeCastName(ClassBindingInfo? classInfo) =>
        classInfo?.FullyQualifiedName ?? "object";

    /// <summary>
    /// Determines whether the given class supports after-change property observation
    /// via <see cref="System.ComponentModel.INotifyPropertyChanged"/> (either directly
    /// or through <c>IReactiveObject</c>).
    /// Returns <see langword="false"/> when <paramref name="classInfo"/> is null.
    /// </summary>
    /// <param name="classInfo">The class binding info, or null.</param>
    /// <returns><see langword="true"/> if INPC observation is supported.</returns>
    internal static bool IsINPC(ClassBindingInfo? classInfo) =>
        classInfo is not null && (classInfo.ImplementsIReactiveObject || classInfo.ImplementsINPC);

    /// <summary>
    /// Determines whether the given class supports before-change property observation
    /// via <see cref="System.ComponentModel.INotifyPropertyChanging"/> (either directly
    /// or through <c>IReactiveObject</c>).
    /// Returns <see langword="false"/> when <paramref name="classInfo"/> is null.
    /// </summary>
    /// <param name="classInfo">The class binding info, or null.</param>
    /// <returns><see langword="true"/> if INPChanging observation is supported.</returns>
    internal static bool IsINPChanging(ClassBindingInfo? classInfo) =>
        classInfo is not null && (classInfo.ImplementsIReactiveObject || classInfo.ImplementsINPChanging);

    /// <summary>Generates concrete typed overloads and observation methods for property observation invocations.</summary>
    /// <param name="invocations">All detected invocations.</param>
    /// <param name="allClasses">All detected class binding info for type mechanism lookup.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <param name="methodPrefix">The method name prefix ("WhenChanged", "WhenChanging", or "WhenAnyValue").</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    internal static string? Generate(
        ImmutableArray<InvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        LanguageFeatures features,
        string methodPrefix)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return null;
        }

        var sb = new StringBuilder(invocations.Length * CodeGeneratorHelpers.PerInvocationBufferCapacity);
        var supportsCallerArgExpr = features.SupportsCallerArgExpr;
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb, features);
        _ = sb.AppendLine();

        // Track which plugins with helper classes are used, so we emit them once
        var usedPluginKinds = new HashSet<string>();

        // Group invocations by their method signature
        var groups = GroupByTypeSignature(invocations);

        for (var g = 0; g < groups.Count; g++)
        {
            GenerateGroup(sb, groups[g], allClasses, supportsCallerArgExpr, methodPrefix, usedPluginKinds);
        }

        EmitUsedHelperClasses(sb, usedPluginKinds);

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        _ = sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>Generates an observation method for a single invocation.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="suffix">The stable method name suffix (hex hash).</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change), false for WhenChanged (after-change).</param>
    /// <param name="prefix">The method name prefix ("WhenChanged", "WhenChanging", or "WhenAnyValue").</param>
    internal static void GenerateObservationMethod(
        StringBuilder sb,
        InvocationInfo inv,
        ClassBindingInfo? classInfo,
        string suffix,
        bool isBeforeChange,
        string prefix)
    {
        var selectorParam = inv.HasSelector ? $", {GetSelectorType(inv)} selector" : string.Empty;

        _ = sb.AppendLine($$"""
                                private static global::System.IObservable<{{inv.ReturnTypeFullName}}> __{{prefix}}_{{suffix}}({{inv.SourceTypeFullName}} obj{{selectorParam}})
                                {
                        """);

        if (inv.PropertyPaths.Length == 1)
        {
            var path = inv.PropertyPaths[0];

            if (path.Length > 1)
            {
                GenerateDeepChainObservation(sb, inv, classInfo, isBeforeChange);
            }
            else
            {
                var propertyAccessChain = CodeGeneratorHelpers.BuildPropertyAccessChain("obj", path);
                var leafPropertyName = path[0].PropertyName;

                if (inv.HasSelector)
                {
                    _ = sb.Append("            return global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select(");
                    GenerateShallowPathObservation(sb, path, classInfo, isBeforeChange);
                    _ = sb.AppendLine(", selector);");
                }
                else
                {
                    GenerateSinglePropertyObservation(
                        sb,
                        inv,
                        classInfo,
                        propertyAccessChain,
                        leafPropertyName,
                        isBeforeChange);
                }
            }
        }
        else
        {
            GenerateMultiPropertyObservation(sb, inv, classInfo, isBeforeChange);
        }

        _ = sb.AppendLine()
            .AppendLine("        }")
            .AppendLine();
    }

    /// <summary>Gets the Func type signature for a selector parameter.</summary>
    /// <param name="inv">The invocation info containing property path and return type information.</param>
    /// <returns>A fully qualified Func type string like <c>global::System.Func&lt;T1, T2, TReturn&gt;</c>.</returns>
    internal static string GetSelectorType(InvocationInfo inv)
    {
        var sb = new StringBuilder("global::System.Func<");
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            _ = sb.Append(path[path.Length - 1].PropertyTypeFullName).Append(", ");
        }

        _ = sb.Append(inv.ReturnTypeFullName).Append('>');
        return sb.ToString();
    }

    /// <summary>
    /// Generates a multi-property observation method body using CombineLatest.
    /// Each property path observable is pre-declared as a local variable with properly
    /// formatted multi-line code, then referenced by name inside CombineLatest.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change), false for WhenChanged (after-change).</param>
    internal static void GenerateMultiPropertyObservation(
        StringBuilder sb,
        InvocationInfo inv,
        ClassBindingInfo? classInfo,
        bool isBeforeChange)
    {
        // Pre-declare an observable variable for each property path.
        // Both shallow (single-segment) and deep (multi-segment) paths get their own
        // properly formatted local variable, then are referenced by name in CombineLatest.
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            var varName = $"__propObs{i}";

            if (path.Length > 1)
            {
                GenerateDeepChainVariable(sb, path, classInfo, isBeforeChange, varName);
            }
            else
            {
                GenerateShallowObservableVariable(sb, path, classInfo, isBeforeChange, varName);
            }

            // Blank line between variable declarations for readability
            _ = sb.AppendLine()
                .AppendLine();
        }

        _ = sb.AppendLine("            return global::ReactiveUI.Binding.Observables.CombineLatestObservable.Create(");
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            _ = sb.Append("                __propObs").Append(i);
            if (i < inv.PropertyPaths.Length - 1)
            {
                _ = sb.AppendLine(",");
            }
        }

        if (inv.HasSelector)
        {
            _ = sb.AppendLine(",")
                .Append("                selector);");
        }
        else
        {
            EmitCombineLatestTupleProjection(sb, inv.PropertyPaths.Length);
        }
    }

    /// <summary>
    /// Generates a shallow (single-segment) path observation as a single-line expression.
    /// Uses plugin dispatch to emit platform-specific observation code.
    /// Appended directly to <paramref name="sb"/> without a trailing newline.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="path">The single-segment property path.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change), false for WhenChanged (after-change).</param>
    internal static void GenerateShallowPathObservation(
        StringBuilder sb,
        EquatableArray<PropertyPathSegment> path,
        ClassBindingInfo? classInfo,
        bool isBeforeChange)
    {
        var segment = path[0];
        var plugin = classInfo is not null ? ObservationPluginRegistry.GetBestPlugin(classInfo) : null;

        if (plugin is not null)
        {
            plugin.EmitShallowObservation(sb, "obj", segment, GetTypeCastName(classInfo), isBeforeChange, true);
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            _ = sb
                .Append($"new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{segment.PropertyTypeFullName}>((")
                .Append($"""global::System.ComponentModel.INotifyPropertyChanging)obj, "{segment.PropertyName}", (global::System.ComponentModel.INotifyPropertyChanging __o) => (""")
                .Append($"({GetTypeCastName(classInfo)})__o).{segment.PropertyName})");
        }
        else
        {
            var propertyAccess = $"obj.{segment.PropertyName}";
            _ = sb.Append(
                $"new global::ReactiveUI.Binding.Observables.ReturnObservable<{segment.PropertyTypeFullName}>({propertyAccess})");
        }
    }

    /// <summary>
    /// Generates a shallow (single-segment) path observable as a properly formatted local variable
    /// declaration. Uses plugin dispatch to emit platform-specific observation code.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="path">The single-segment property path.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change), false for WhenChanged (after-change).</param>
    /// <param name="varName">The variable name to assign the observable to.</param>
    internal static void GenerateShallowObservableVariable(
        StringBuilder sb,
        EquatableArray<PropertyPathSegment> path,
        ClassBindingInfo? classInfo,
        bool isBeforeChange,
        string varName)
    {
        var segment = path[0];
        var plugin = classInfo is not null ? ObservationPluginRegistry.GetBestPlugin(classInfo) : null;

        if (plugin is not null)
        {
            plugin.EmitShallowObservationVariable(
                sb,
                "obj",
                segment,
                GetTypeCastName(classInfo),
                isBeforeChange,
                varName);
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            _ = sb.Append($"""
                                   var {varName} = new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{segment.PropertyTypeFullName}>(
                                       (global::System.ComponentModel.INotifyPropertyChanging)obj,
                                       "{segment.PropertyName}",
                                       (global::System.ComponentModel.INotifyPropertyChanging __o) => (({GetTypeCastName(classInfo)})__o).{segment.PropertyName});
                       """);
        }
        else
        {
            var propertyAccess = $"obj.{segment.PropertyName}";
            _ = sb.Append(
                $"            var {varName} = new global::ReactiveUI.Binding.Observables.ReturnObservable<{segment.PropertyTypeFullName}>({propertyAccess});");
        }
    }

    /// <summary>
    /// Generates a deep chain observable as a properly formatted local variable declaration.
    /// Uses plugin dispatch for the root segment and inner segments.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="path">The multi-segment property path.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change), false for WhenChanged (after-change).</param>
    /// <param name="varName">The variable name to assign the final observable to.</param>
    internal static void GenerateDeepChainVariable(
        StringBuilder sb,
        EquatableArray<PropertyPathSegment> path,
        ClassBindingInfo? classInfo,
        bool isBeforeChange,
        string varName)
    {
        // First segment: observe root object for first property
        var seg0 = path[0];
        var obs0Var = $"{varName}_s0";
        var rootPlugin = classInfo is not null ? ObservationPluginRegistry.GetBestPlugin(classInfo) : null;

        if (rootPlugin is not null)
        {
            rootPlugin.EmitDeepChainRootSegment(sb, "obj", seg0, GetTypeCastName(classInfo), isBeforeChange, obs0Var);
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            _ = sb.AppendLine($"""
            var {obs0Var} = (global::System.IObservable<{seg0.PropertyTypeFullName}>)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{seg0.PropertyTypeFullName}>(
                (global::System.ComponentModel.INotifyPropertyChanging)obj,
                "{seg0.PropertyName}",
                (global::System.ComponentModel.INotifyPropertyChanging __o) => (({GetTypeCastName(classInfo)})__o).{seg0.PropertyName});
""");
        }
        else
        {
            _ = sb
                .Append($"            var {obs0Var} = (global::System.IObservable<{seg0.PropertyTypeFullName}>")
                .AppendLine($")new global::ReactiveUI.Binding.Observables.ReturnObservable<{seg0.PropertyTypeFullName}>(obj.{seg0.PropertyName});");
        }

        EmitDeepChainInnerSegments(sb, path, classInfo, rootPlugin, isBeforeChange, varName);

        var lastObsVar = $"{varName}_s{path.Length - 1}";
        _ = sb.AppendLine(isBeforeChange
            ? $"            var {varName} = {lastObsVar};"
            : $"            var {varName} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.DistinctUntilChanged({lastObsVar});");
    }

    /// <summary>
    /// Groups invocations by their source type, return type, property count, and property types.
    /// Invocations sharing the same type signature share a concrete overload.
    /// </summary>
    /// <param name="invocations">All detected invocations.</param>
    /// <returns>A list of type groups, each containing invocations with the same signature.</returns>
    internal static List<TypeGroup> GroupByTypeSignature(ImmutableArray<InvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<InvocationInfo>>(invocations.Length);
        var keySb = new StringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            _ = keySb.Clear()
            .Append(inv.SourceTypeFullName).Append('|')
            .Append(inv.ReturnTypeFullName).Append('|')
            .Append(inv.PropertyPaths.Length).Append('|')
            .Append(inv.HasSelector);
            for (var p = 0; p < inv.PropertyPaths.Length; p++)
            {
                var path = inv.PropertyPaths[p];
                _ = keySb.Append('|').Append(path[path.Length - 1].PropertyTypeFullName);
            }

            var key = keySb.ToString();

            if (!groupMap.TryGetValue(key, out var list))
            {
                list = [];
                groupMap[key] = list;
            }

            list.Add(inv);
        }

        var result = new List<TypeGroup>();
        foreach (var kvp in groupMap)
        {
            result.Add(new(kvp.Value[0], [.. kvp.Value]));
        }

        return result;
    }

    /// <summary>
    /// Generates a concrete typed extension method overload with dispatch logic.
    /// When <paramref name="generatedAffinity"/> is non-negative, emits an affinity check
    /// before the dispatch table to allow user-registered plugins with higher affinity
    /// to override the source-generated observation at runtime.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    /// <param name="generatedAffinity">The affinity of the source generator's selected plugin, or -1 if unknown.</param>
    internal static void GenerateConcreteOverload(
    StringBuilder sb,
    TypeGroup group,
    bool supportsCallerArgExpr,
    string methodPrefix,
    int generatedAffinity = -1)
    {
        var first = group.First;
        var propCount = first.PropertyPaths.Length;
        var hasSelector = first.HasSelector;

        EmitOverloadSignature(sb, first, supportsCallerArgExpr, methodPrefix, propCount, hasSelector);
        EmitStaticPrefixNormalization(sb, supportsCallerArgExpr, propCount);

        // Emit runtime affinity check: allow user-registered plugins to override generated observation.
        // Only emit for overloads with <= 3 properties, matching RuntimeObservationFallback signatures.
        if (generatedAffinity >= 0 && propCount <= MaxAffinityFallbackPropertyCount)
        {
            EmitAffinityCheck(sb, first, methodPrefix, propCount, hasSelector, generatedAffinity);
        }

        EmitDispatchTable(sb, group, supportsCallerArgExpr, methodPrefix, propCount, hasSelector);

        GenerateRuntimeFallback(sb, methodPrefix, propCount, hasSelector);

        _ = sb.AppendLine("        }");
    }

    /// <summary>
    /// Generates the throw path for when no generated dispatch match is found.
    /// Since the source generator matched all invocations at compile time, an unmatched
    /// dispatch indicates a caching issue — never falls back to runtime reflection.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    /// <param name="propCount">The number of property expressions (unused, kept for API compatibility).</param>
    /// <param name="hasSelector">Whether a selector function is present (unused, kept for API compatibility).</param>
    internal static void GenerateRuntimeFallback(
    StringBuilder sb,
    string methodPrefix,
    int propCount,
    bool hasSelector) =>
    sb.AppendLine(
    $"            throw new global::System.InvalidOperationException(\"No generated {methodPrefix} dispatch matched. Ensure the expression is an inline lambda for compile-time optimization.\");");

    /// <summary>
    /// Emits a runtime affinity check at the top of a concrete overload method body.
    /// If a user-registered <c>ICreatesObservableForProperty</c> implementation has higher
    /// affinity than the source generator's plugin, delegates to <c>RuntimeObservationFallback</c>.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="first">The first invocation in the group, used for type information.</param>
    /// <param name="methodPrefix">The method name prefix ("WhenChanged", "WhenChanging", or "WhenAnyValue").</param>
    /// <param name="propCount">The number of property expressions.</param>
    /// <param name="hasSelector">Whether a selector function is present.</param>
    /// <param name="generatedAffinity">The affinity of the source generator's selected plugin.</param>
    internal static void EmitAffinityCheck(
    StringBuilder sb,
    InvocationInfo first,
    string methodPrefix,
    int propCount,
    bool hasSelector,
    int generatedAffinity)
    {
        var isBeforeChange = methodPrefix == "WhenChanging";

        _ = sb.AppendLine(
        "            // Allow user-registered plugins with higher affinity to override generated observation")
        .AppendLine(
        $"            if ({ObservationAffinityChecker}.HasHigherAffinityPlugin(typeof({first.SourceTypeFullName}), {generatedAffinity}, {(isBeforeChange ? "true" : "false")}))")
        .AppendLine("            {");

        EmitAffinityFallbackReturn(sb, first, methodPrefix, propCount, hasSelector);

        _ = sb.AppendLine("            }")
        .AppendLine();
    }

    /// <summary>
    /// Emits the return statement inside the affinity check block, delegating to
    /// <c>RuntimeObservationFallback</c> with the appropriate method signature.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="first">The first invocation in the group, used for type information.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    /// <param name="propCount">The number of property expressions.</param>
    /// <param name="hasSelector">Whether a selector function is present.</param>
    internal static void EmitAffinityFallbackReturn(
    StringBuilder sb,
    InvocationInfo first,
    string methodPrefix,
    int propCount,
    bool hasSelector)
    {
        // Determine the fallback method name: WhenAnyValue maps to WhenAnyValue, others stay as-is
        var fallbackMethod = methodPrefix;

        // Build the property arguments (property1, property2, ...)
        var propArgs = new StringBuilder();
        for (var i = 0; i < propCount; i++)
        {
            _ = propArgs.Append($", property{i + 1}");
        }

        if (!hasSelector)
        {
            // No selector: direct call to RuntimeObservationFallback
            _ = sb.AppendLine(
            $"                return global::ReactiveUI.Binding.Fallback.RuntimeObservationFallback.{fallbackMethod}(objectToMonitor{propArgs});");
        }
        else if (propCount == 1)
        {
            // Single property with selector: wrap fallback with SelectObservable
            var propType = first.PropertyPaths[0][first.PropertyPaths[0].Length - 1].PropertyTypeFullName;
            _ = sb.AppendLine(
            $"                return new global::ReactiveUI.Binding.Observables.SelectObservable<{propType}, {first.ReturnTypeFullName}>(")
            .AppendLine(
            $"                    global::ReactiveUI.Binding.Fallback.RuntimeObservationFallback.{fallbackMethod}(objectToMonitor{propArgs}),")
            .AppendLine("                    selector);");
        }
        else
        {
            // Multi-property with selector: wrap fallback tuple with selector decomposition
            var tupleType = new StringBuilder("global::System.ValueTuple<");
            for (var i = 0; i < propCount; i++)
            {
                var path = first.PropertyPaths[i];
                _ = tupleType.Append(path[path.Length - 1].PropertyTypeFullName);
                if (i < propCount - 1)
                {
                    _ = tupleType.Append(", ");
                }
            }

            _ = tupleType.Append('>');

            // Build the selector decomposition lambda: __t => selector(__t.Item1, __t.Item2, ...)
            var selectorArgs = new StringBuilder();
            for (var i = 0; i < propCount; i++)
            {
                _ = selectorArgs.Append("__t.Item").Append(i + 1);
                if (i < propCount - 1)
                {
                    _ = selectorArgs.Append(", ");
                }
            }

            _ = sb.AppendLine(
            $"                return new global::ReactiveUI.Binding.Observables.SelectObservable<{tupleType}, {first.ReturnTypeFullName}>(")
            .AppendLine(
            $"                    global::ReactiveUI.Binding.Fallback.RuntimeObservationFallback.{fallbackMethod}(objectToMonitor{propArgs}),")
            .AppendLine($"                    __t => selector({selectorArgs}));");
        }
    }

    /// <summary>Generates a single-property observation method body using plugin dispatch.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="propertyAccess">The dotted property access expression (e.g. "obj.Name").</param>
    /// <param name="propertyName">The leaf property name for event filtering.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change), false for WhenChanged (after-change).</param>
    internal static void GenerateSinglePropertyObservation(
    StringBuilder sb,
    InvocationInfo inv,
    ClassBindingInfo? classInfo,
    string propertyAccess,
    string propertyName,
    bool isBeforeChange)
    {
        var plugin = classInfo is not null ? ObservationPluginRegistry.GetBestPlugin(classInfo) : null;

        if (plugin is not null)
        {
            var segment = inv.PropertyPaths[0][0];
            _ = sb.Append("            return ");
            plugin.EmitShallowObservation(sb, "obj", segment, GetTypeCastName(classInfo), isBeforeChange, true);
            _ = sb.Append(';');
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            // INPChanging-only type (no INPC, no IReactiveObject) — can observe before-change
            _ = sb.Append($"""
            return new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{inv.ReturnTypeFullName}>(
                (global::System.ComponentModel.INotifyPropertyChanging)obj,
                "{propertyName}",
                (global::System.ComponentModel.INotifyPropertyChanging __o) => (({inv.SourceTypeFullName})__o).{propertyName});
""");
        }
        else
        {
            _ = sb.Append(
                $"            return new global::ReactiveUI.Binding.Observables.ReturnObservable<{inv.ReturnTypeFullName}>({propertyAccess});");
        }
    }

    /// <summary>Generates a deep chain observation method body using plugin dispatch for the root segment and inner segments.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change), false for WhenChanged (after-change).</param>
    internal static void GenerateDeepChainObservation(
        StringBuilder sb,
        InvocationInfo inv,
        ClassBindingInfo? classInfo,
        bool isBeforeChange)
    {
        var path = inv.PropertyPaths[0];
        var seg0 = path[0];
        var rootPlugin = classInfo is not null ? ObservationPluginRegistry.GetBestPlugin(classInfo) : null;

        // First segment: observe root object for first property
        if (rootPlugin is not null)
        {
            rootPlugin.EmitDeepChainRootSegment(sb, "obj", seg0, GetTypeCastName(classInfo), isBeforeChange, "__obs0");
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            _ = sb.AppendLine($"""
            var __obs0 = (global::System.IObservable<{seg0.PropertyTypeFullName}>)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{seg0.PropertyTypeFullName}>(
                (global::System.ComponentModel.INotifyPropertyChanging)obj,
                "{seg0.PropertyName}",
                (global::System.ComponentModel.INotifyPropertyChanging __o) => (({GetTypeCastName(classInfo)})__o).{seg0.PropertyName});
""");
        }
        else
        {
            _ = sb
                .Append($"            var __obs0 = (global::System.IObservable<{seg0.PropertyTypeFullName}>")
                .AppendLine($")new global::ReactiveUI.Binding.Observables.ReturnObservable<{seg0.PropertyTypeFullName}>(obj.{seg0.PropertyName});");
        }

        EmitObservationChainInnerSegments(sb, path, classInfo, rootPlugin, isBeforeChange);

        var lastObs = $"__obs{path.Length - 1}";
        _ = sb.Append(isBeforeChange
            ? $"            return {lastObs};"
            : $"            return global::ReactiveUI.Binding.Observables.RxBindingExtensions.DistinctUntilChanged({lastObs});");
    }

    /// <summary>
    /// Emits an inline observation expression as a variable assignment using plugin dispatch.
    /// Used by binding generators to emit direct observation code
    /// instead of delegating to WhenChanged dispatch.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The root variable name (e.g., "source", "target").</param>
    /// <param name="propertyPath">The property path segments.</param>
    /// <param name="propertyTypeFullName">The fully qualified type of the leaf property.</param>
    /// <param name="classInfo">The class binding info for the observed type, or null.</param>
    /// <param name="variableName">The name for the resulting observable variable (e.g., "sourceObs").</param>
    internal static void EmitInlineObservation(
        StringBuilder sb,
        string rootVar,
        EquatableArray<PropertyPathSegment> propertyPath,
        string propertyTypeFullName,
        ClassBindingInfo? classInfo,
        string variableName)
    {
        var plugin = classInfo is not null ? ObservationPluginRegistry.GetBestPlugin(classInfo) : null;

        if (propertyPath.Length == 1)
        {
            var segment = propertyPath[0];

            if (plugin is not null)
            {
                plugin.EmitInlineObservationVariable(sb, rootVar, segment, GetTypeCastName(classInfo), variableName);
            }
            else
            {
                var propertyAccess = $"{rootVar}.{segment.PropertyName}";
                _ = sb.AppendLine(
                    $"        var {variableName} = new global::ReactiveUI.Binding.Observables.ReturnObservable<{propertyTypeFullName}>({propertyAccess});");
            }
        }
        else
        {
            EmitInlineDeepChain(sb, rootVar, propertyPath, classInfo, plugin, variableName);
        }
    }

    /// <summary>
    /// Chains the segments after the root for the standalone observation method, which names its
    /// stages <c>__obsN</c> rather than deriving them from a caller-supplied prefix.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="path">The property path being observed.</param>
    /// <param name="classInfo">The root type's binding info, when known.</param>
    /// <param name="rootPlugin">
    /// The root type's observation plugin. Every plugin emits the same generic notification observable
    /// for inner segments, so reusing the root's plugin is safe whatever the segment declares.
    /// </param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    private static void EmitObservationChainInnerSegments(
        StringBuilder sb,
        EquatableArray<PropertyPathSegment> path,
        ClassBindingInfo? classInfo,
        IObservationPlugin? rootPlugin,
        bool isBeforeChange)
    {
        for (var s = 1; s < path.Length; s++)
        {
            var seg = path[s];
            var prevVar = $"__obs{s - 1}";
            var curVar = $"__obs{s}";
            var lambdaParam = $"__parent{s}";
            var segType = seg.PropertyTypeFullName;

            if (rootPlugin is not null)
            {
                rootPlugin.EmitDeepChainInnerSegment(sb, prevVar, curVar, lambdaParam, seg, isBeforeChange);
            }
            else if (IsINPChanging(classInfo) && isBeforeChange)
            {
                _ = sb.AppendLine()
                    .AppendLine($"""
                                         var {curVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                             global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevVar},
                                                 {lambdaParam} => {lambdaParam} != null
                                                     ? (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{segType}>(
                                                         (global::System.ComponentModel.INotifyPropertyChanging){lambdaParam},
                                                         "{seg.PropertyName}",
                                                         (global::System.ComponentModel.INotifyPropertyChanging __o) => (({seg.DeclaringTypeFullName})__o).{seg.PropertyName})
                                                     : (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(default({segType}))));
                                 """);
            }
            else
            {
                _ = sb.AppendLine()
                    .AppendLine($"""
                                         var {curVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                             global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevVar},
                                                 {lambdaParam} => (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(
                                                     {lambdaParam} != null ? (({seg.DeclaringTypeFullName}){lambdaParam}).{seg.PropertyName} : default({segType}))));
                                 """);
            }
        }
    }

    /// <summary>
    /// Chains the segments after the root with Select + Switch, so the observation re-subscribes when
    /// an intermediate value changes.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="path">The property path being observed.</param>
    /// <param name="classInfo">The root type's binding info, when known.</param>
    /// <param name="rootPlugin">
    /// The root type's observation plugin. Every plugin emits the same generic notification observable
    /// for inner segments, so reusing the root's plugin is safe whatever the segment declares.
    /// </param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="varName">The variable-name prefix for the emitted stages.</param>
    private static void EmitDeepChainInnerSegments(
        StringBuilder sb,
        EquatableArray<PropertyPathSegment> path,
        ClassBindingInfo? classInfo,
        IObservationPlugin? rootPlugin,
        bool isBeforeChange,
        string varName)
    {
        for (var s = 1; s < path.Length; s++)
        {
            var seg = path[s];
            var prevObsVar = $"{varName}_s{s - 1}";
            var curObsVar = $"{varName}_s{s}";
            var lambdaParam = $"{varName}_p{s}";
            var segType = seg.PropertyTypeFullName;

            if (rootPlugin is not null)
            {
                rootPlugin.EmitDeepChainInnerSegment(sb, prevObsVar, curObsVar, lambdaParam, seg, isBeforeChange);
            }
            else if (IsINPChanging(classInfo) && isBeforeChange)
            {
                _ = sb.AppendLine()
                    .AppendLine($"""
                                         var {curObsVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                             global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevObsVar},
                                                 {lambdaParam} => {lambdaParam} != null
                                                     ? (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{segType}>(
                                                         (global::System.ComponentModel.INotifyPropertyChanging){lambdaParam},
                                                         "{seg.PropertyName}",
                                                         (global::System.ComponentModel.INotifyPropertyChanging __o) => (({seg.DeclaringTypeFullName})__o).{seg.PropertyName})
                                                     : (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(default({segType}))));
                                 """);
            }
            else
            {
                _ = sb.AppendLine()
                    .AppendLine($"""
                  var {curObsVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                      global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevObsVar},
                          {lambdaParam} => (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(
                              {lambdaParam} != null ? (({seg.DeclaringTypeFullName}){lambdaParam}).{seg.PropertyName} : default({segType}))));
          """);
            }
        }
    }

    /// <summary>
    /// Emits the Select/Switch chain for a multi-segment property path, one stage per segment, and
    /// the distinct-until-changed gate that terminates it.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable holding the chain root.</param>
    /// <param name="propertyPath">The property path being observed.</param>
    /// <param name="classInfo">The root type's binding info, when known.</param>
    /// <param name="plugin">The observation plugin for the root type, when one matched.</param>
    /// <param name="variableName">The name of the variable the chain result is assigned to.</param>
    private static void EmitInlineDeepChain(
        StringBuilder sb,
        string rootVar,
        EquatableArray<PropertyPathSegment> propertyPath,
        ClassBindingInfo? classInfo,
        IObservationPlugin? plugin,
        string variableName)
    {
        var seg0 = propertyPath[0];

        if (plugin is not null)
        {
            plugin.EmitDeepChainRootSegment(sb, rootVar, seg0, GetTypeCastName(classInfo), false, $"__{variableName}_s0");
        }
        else
        {
            _ = sb
                .Append($"            var __{variableName}_s0 = (global::System.IObservable<{seg0.PropertyTypeFullName}>")
                .AppendLine($")new global::ReactiveUI.Binding.Observables.ReturnObservable<{seg0.PropertyTypeFullName}>({rootVar}.{seg0.PropertyName});");
        }

        for (var s = 1; s < propertyPath.Length; s++)
        {
            var seg = propertyPath[s];
            var prevVar = $"__{variableName}_s{s - 1}";
            var curVar = $"__{variableName}_s{s}";
            var lambdaParam = $"__p{s}";

            if (plugin is not null)
            {
                plugin.EmitDeepChainInnerSegment(sb, prevVar, curVar, lambdaParam, seg, false);
                continue;
            }

            var segType = seg.PropertyTypeFullName;
            var declType = seg.DeclaringTypeFullName;
            _ = sb.AppendLine()
                .AppendLine($"""
                                 var {curVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                     global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevVar},
                                         {lambdaParam} => (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(
                                             {lambdaParam} != null ? (({declType}){lambdaParam}).{seg.PropertyName} : default({segType}))));
                             """);
        }

        var lastSeg = $"__{variableName}_s{propertyPath.Length - 1}";
        _ = sb.AppendLine(
            $"        var {variableName} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.DistinctUntilChanged({lastSeg});");
    }

    /// <summary>
    /// Computes the worker-method suffix for an observation invocation, keyed only by the source type and
    /// property expression(s) — not the call site. Call sites that share the same type and expression(s)
    /// produce an identical worker, so they resolve to a single generated method.
    /// </summary>
    /// <param name="inv">The invocation info.</param>
    /// <returns>The stable, call-site-independent method-name suffix.</returns>
    private static string MethodSuffix(InvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.SourceTypeFullName,
            string.Empty,
            0,
            string.Join("|", inv.ExpressionTexts));

    /// <summary>
    /// Generates the concrete overload and per-invocation observation methods for a single type group,
    /// tracking which plugins require helper-class emission.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The type group to generate code for.</param>
    /// <param name="allClasses">All detected class binding info for type mechanism lookup.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    /// <param name="usedPluginKinds">Accumulates the observation kinds of plugins that require helper classes.</param>
    private static void GenerateGroup(
        StringBuilder sb,
        TypeGroup group,
        ImmutableArray<ClassBindingInfo> allClasses,
        bool supportsCallerArgExpr,
        string methodPrefix,
        HashSet<string> usedPluginKinds)
    {
        // Resolve the plugin affinity for the source type to emit the runtime override check
        var groupClassInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, group.SourceTypeFullName);
        var groupPlugin = groupClassInfo is not null
            ? ObservationPluginRegistry.GetBestPlugin(groupClassInfo)
            : null;
        var groupAffinity = groupPlugin is not null ? groupPlugin.Affinity : -1;

        // Generate the concrete typed extension method overload
        GenerateConcreteOverload(sb, group, supportsCallerArgExpr, methodPrefix, groupAffinity);
        _ = sb.AppendLine();

        // Generate the observation methods for each invocation in this group. Call sites that share the
        // same source type and property expression(s) produce an identical worker, so the method is keyed
        // by (type, expressions) and emitted only once — avoiding duplicate, identical methods.
        var emittedMethods = new HashSet<string>();
        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var suffix = MethodSuffix(inv);
            if (!emittedMethods.Add(suffix))
            {
                continue;
            }

            var classInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.SourceTypeFullName);

            // Track plugin usage for helper class emission
            if (classInfo is not null)
            {
                var plugin = ObservationPluginRegistry.GetBestPlugin(classInfo);
                if (plugin?.RequiresHelperClasses == true)
                {
                    _ = usedPluginKinds.Add(plugin.ObservationKind);
                }
            }

            GenerateObservationMethod(sb, inv, classInfo, suffix, inv.IsBeforeChange, methodPrefix);
        }
    }

    /// <summary>Emits helper classes for all used plugins that require them, sorted for deterministic output order.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="usedPluginKinds">The observation kinds of plugins requiring helper classes.</param>
    private static void EmitUsedHelperClasses(StringBuilder sb, HashSet<string> usedPluginKinds)
    {
        var sortedKinds = new List<string>(usedPluginKinds);
        sortedKinds.Sort(StringComparer.Ordinal);
        for (var k = 0; k < sortedKinds.Count; k++)
        {
            var plugin = ObservationPluginRegistry.GetPluginByKind(sortedKinds[k]);
            plugin?.EmitHelperClasses(sb);
        }
    }

    /// <summary>Emits the trailing named-tuple projection lambda for a selector-less <c>CombineLatest</c> call.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="propertyCount">The number of property path observables being combined.</param>
    private static void EmitCombineLatestTupleProjection(StringBuilder sb, int propertyCount)
    {
        _ = sb.AppendLine(",")
            .Append("                (");
        for (var i = 0; i < propertyCount; i++)
        {
            _ = sb.Append('p').Append(i + 1);
            if (i < propertyCount - 1)
            {
                _ = sb.Append(", ");
            }
        }

        _ = sb.Append(") => (");
        for (var i = 0; i < propertyCount; i++)
        {
            _ = sb.Append("property").Append(i + 1).Append(": p").Append(i + 1);
            if (i < propertyCount - 1)
            {
                _ = sb.Append(", ");
            }
        }

        _ = sb.Append("));");
    }

    /// <summary>
    /// Emits the XML doc comment, method signature, property expression parameters, optional selector,
    /// CallerArgumentExpression parameters, and the caller-info parameters that open the overload body.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="first">The first invocation in the group, used for type information.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    /// <param name="propCount">The number of property expressions.</param>
    /// <param name="hasSelector">Whether a selector function is present.</param>
    private static void EmitOverloadSignature(
        StringBuilder sb,
        InvocationInfo first,
        bool supportsCallerArgExpr,
        string methodPrefix,
        int propCount,
        bool hasSelector)
    {
        _ = sb.AppendLine($"""
                               /// <summary>
                               /// Concrete typed overload for {methodPrefix} on {first.SourceTypeFullName}.
                               /// </summary>
                               public static global::System.IObservable<{first.ReturnTypeFullName}> {methodPrefix}(
                                   this {first.SourceTypeFullName} objectToMonitor,
                       """);

        for (var i = 0; i < propCount; i++)
        {
            var type = first.PropertyPaths[i][first.PropertyPaths[i].Length - 1].PropertyTypeFullName;
            _ = sb.AppendLine(
                $"            global::System.Linq.Expressions.Expression<global::System.Func<{first.SourceTypeFullName}, {type}>> property{i + 1},");
        }

        if (hasSelector)
        {
            _ = sb.AppendLine($"            {GetSelectorType(first)} selector,");
        }

        if (supportsCallerArgExpr)
        {
            for (var i = 0; i < propCount; i++)
            {
                _ = sb.AppendLine(
                    $"            [global::System.Runtime.CompilerServices.CallerArgumentExpression(\"property{i + 1}\")] string property{i + 1}Expression = \"\",");
            }
        }

        _ = sb.AppendLine("""
                                  [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                                  [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                              {
                      """);
    }

    /// <summary>Emits normalization that strips the <c>static</c> prefix from CallerArgumentExpression values.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="propCount">The number of property expressions.</param>
    private static void EmitStaticPrefixNormalization(StringBuilder sb, bool supportsCallerArgExpr, int propCount)
    {
        if (!supportsCallerArgExpr)
        {
            return;
        }

        for (var i = 0; i < propCount; i++)
        {
            var paramName = $"property{i + 1}Expression";
            _ = sb.AppendLine(
                $"""            {paramName} = {paramName}.StartsWith("static ") ? {paramName}.Substring(7) : {paramName};""");
        }

        _ = sb.AppendLine();
    }

    /// <summary>Emits the if/else-if dispatch table that routes each matched invocation to its generated method.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    /// <param name="propCount">The number of property expressions.</param>
    /// <param name="hasSelector">Whether a selector function is present.</param>
    private static void EmitDispatchTable(
        StringBuilder sb,
        TypeGroup group,
        bool supportsCallerArgExpr,
        string methodPrefix,
        int propCount,
        bool hasSelector)
    {
        // CallerArgumentExpression dispatch keys solely on the expression text, so call sites that share
        // the same expression(s) collapse to one branch (duplicates would be identical and unreachable).
        // CallerFilePath dispatch keeps one branch per call site (distinct file/line) but routes to the
        // same expression-keyed worker. 'branchIndex' tracks emitted branches so the first uses "if".
        var emittedConditions = new HashSet<string>();
        var branchIndex = 0;
        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var keyword = CodeGeneratorHelpers.ConditionKeyword(branchIndex);

            if (supportsCallerArgExpr)
            {
                if (!emittedConditions.Add(string.Join("|", inv.ExpressionTexts)))
                {
                    continue;
                }

                EmitCallerArgExprCondition(sb, inv, keyword, propCount);
            }
            else
            {
                var suffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
                _ = sb
                    .Append($"""            {keyword} (callerLineNumber == {inv.CallerLineNumber} && callerFilePath.EndsWith("{CodeGeneratorHelpers.EscapeString(suffix)}",""")
                    .AppendLine(" global::System.StringComparison.OrdinalIgnoreCase))");
            }

            branchIndex++;
            _ = sb.AppendLine("            {");
            var selectorArg = hasSelector ? ", selector" : string.Empty;
            _ = sb.AppendLine($"                return __{methodPrefix}_{MethodSuffix(inv)}(objectToMonitor{selectorArg});")
                .AppendLine("            }");
        }
    }

    /// <summary>Emits the CallerArgumentExpression match condition for a single invocation in the dispatch table.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="condition">The conditional keyword (<c>"if"</c> or <c>"else if"</c>).</param>
    /// <param name="propCount">The number of property expressions.</param>
    private static void EmitCallerArgExprCondition(
        StringBuilder sb,
        InvocationInfo inv,
        string condition,
        int propCount)
    {
        _ = sb.Append($"            {condition} (");
        for (var p = 0; p < propCount; p++)
        {
            _ = sb.Append(
                $"property{p + 1}Expression == \"{CodeGeneratorHelpers.EscapeString(inv.ExpressionTexts[p])}\"");
            if (p < propCount - 1)
            {
                _ = sb.Append(" && ");
            }
        }

        _ = sb.AppendLine(")");
    }

    /// <summary>Groups invocations by source and return type signature for overload generation.</summary>
    /// <param name="First">The first invocation in the group, used for type information.</param>
    /// <param name="Invocations">All invocations sharing the same type signature.</param>
    internal sealed record TypeGroup(
        InvocationInfo First,
        InvocationInfo[] Invocations)
    {
        /// <summary>Gets the fully qualified name of the source type.</summary>
        internal string SourceTypeFullName => First.SourceTypeFullName;

        /// <summary>Gets the fully qualified name of the return type.</summary>
        internal string ReturnTypeFullName => First.ReturnTypeFullName;
    }
}

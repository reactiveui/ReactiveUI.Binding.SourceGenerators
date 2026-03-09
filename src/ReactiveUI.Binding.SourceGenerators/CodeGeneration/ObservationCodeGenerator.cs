// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Shared code generation logic for property observation APIs (WhenChanged, WhenChanging, WhenAnyValue).
/// Generates concrete typed extension method overloads and per-invocation observation methods.
/// Uses the plugin system to emit platform-specific observation code.
/// </summary>
internal static class ObservationCodeGenerator
{
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

    /// <summary>
    /// Generates concrete typed overloads and observation methods for property observation invocations.
    /// </summary>
    /// <param name="invocations">All detected invocations.</param>
    /// <param name="allClasses">All detected class binding info for type mechanism lookup.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression (C# 10+).</param>
    /// <param name="methodPrefix">The method name prefix ("WhenChanged", "WhenChanging", or "WhenAnyValue").</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    internal static string? Generate(
        ImmutableArray<InvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        bool supportsCallerArgExpr,
        string methodPrefix)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return null;
        }

        var sb = new StringBuilder(invocations.Length * 1024);
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb);
        sb.AppendLine();

        // Track which plugins with helper classes are used, so we emit them once
        var usedPluginKinds = new HashSet<string>();

        // Group invocations by their method signature
        var groups = GroupByTypeSignature(invocations);

        for (var g = 0; g < groups.Count; g++)
        {
            var group = groups[g];

            // Resolve the plugin affinity for the source type to emit the runtime override check
            var groupClassInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, group.SourceTypeFullName);
            var groupPlugin = groupClassInfo is not null ? ObservationPluginRegistry.GetBestPlugin(groupClassInfo) : null;
            var groupAffinity = groupPlugin is not null ? groupPlugin.Affinity : -1;

            // Generate the concrete typed extension method overload
            GenerateConcreteOverload(sb, group, supportsCallerArgExpr, methodPrefix, groupAffinity);
            sb.AppendLine();

            // Generate the observation methods for each invocation in this group
            for (var i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var classInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.SourceTypeFullName);
                var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, string.Join("|", inv.ExpressionTexts));

                // Track plugin usage for helper class emission
                if (classInfo is not null)
                {
                    var plugin = ObservationPluginRegistry.GetBestPlugin(classInfo);
                    if (plugin is not null && plugin.RequiresHelperClasses)
                    {
                        usedPluginKinds.Add(plugin.ObservationKind);
                    }
                }

                GenerateObservationMethod(sb, inv, classInfo, suffix, isBeforeChange: inv.IsBeforeChange, methodPrefix);
            }
        }

        // Emit helper classes for all used plugins that require them.
        // Sort kinds for deterministic output order.
        var sortedKinds = new List<string>(usedPluginKinds);
        sortedKinds.Sort(StringComparer.Ordinal);
        for (var k = 0; k < sortedKinds.Count; k++)
        {
            var plugin = ObservationPluginRegistry.GetPluginByKind(sortedKinds[k]);
            if (plugin is not null)
            {
                plugin.EmitHelperClasses(sb);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Generates an observation method for a single invocation.
    /// </summary>
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
        var selectorParam = inv.HasSelector ? ", " + GetSelectorType(inv) + " selector" : string.Empty;

        sb.AppendLine($$"""
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
                    sb.Append("            return global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select(");
                    GenerateShallowPathObservation(sb, path, classInfo, isBeforeChange);
                    sb.AppendLine(", selector);");
                }
                else
                {
                    GenerateSinglePropertyObservation(sb, inv, classInfo, propertyAccessChain, leafPropertyName, isBeforeChange);
                }
            }
        }
        else
        {
            GenerateMultiPropertyObservation(sb, inv, classInfo, isBeforeChange);
        }

        sb.AppendLine()
            .AppendLine("        }")
            .AppendLine();
    }

    /// <summary>
    /// Gets the Func type signature for a selector parameter.
    /// </summary>
    /// <param name="inv">The invocation info containing property path and return type information.</param>
    /// <returns>A fully qualified Func type string like <c>global::System.Func&lt;T1, T2, TReturn&gt;</c>.</returns>
    internal static string GetSelectorType(InvocationInfo inv)
    {
        var sb = new StringBuilder("global::System.Func<");
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            sb.Append(path[path.Length - 1].PropertyTypeFullName).Append(", ");
        }

        sb.Append(inv.ReturnTypeFullName).Append('>');
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
            var varName = "__propObs" + i;

            if (path.Length > 1)
            {
                GenerateDeepChainVariable(sb, path, classInfo, isBeforeChange, varName);
            }
            else
            {
                GenerateShallowObservableVariable(sb, path, classInfo, isBeforeChange, varName);
            }

            // Blank line between variable declarations for readability
            sb.AppendLine()
                .AppendLine();
        }

        sb.AppendLine("            return global::ReactiveUI.Binding.Observables.CombineLatestObservable.Create(");
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            sb.Append("                __propObs").Append(i);
            if (i < inv.PropertyPaths.Length - 1)
            {
                sb.AppendLine(",");
            }
        }

        if (inv.HasSelector)
        {
            sb.AppendLine(",")
                .Append("                selector);");
        }
        else
        {
            sb.AppendLine(",")
                .Append("                (");
            for (var i = 0; i < inv.PropertyPaths.Length; i++)
            {
                sb.Append('p').Append(i + 1);
                if (i < inv.PropertyPaths.Length - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append(") => (");
            for (var i = 0; i < inv.PropertyPaths.Length; i++)
            {
                sb.Append("property").Append(i + 1).Append(": p").Append(i + 1);
                if (i < inv.PropertyPaths.Length - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append("));");
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
            plugin.EmitShallowObservation(sb, "obj", segment, GetTypeCastName(classInfo), isBeforeChange, includeStartWith: true);
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            sb.Append($"""new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{segment.PropertyTypeFullName}>((global::System.ComponentModel.INotifyPropertyChanging)obj, "{segment.PropertyName}", (global::System.ComponentModel.INotifyPropertyChanging __o) => (({GetTypeCastName(classInfo)})__o).{segment.PropertyName})""");
        }
        else
        {
            var propertyAccess = "obj." + segment.PropertyName;
            sb.Append($"new global::ReactiveUI.Binding.Observables.ReturnObservable<{segment.PropertyTypeFullName}>({propertyAccess})");
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
            plugin.EmitShallowObservationVariable(sb, "obj", segment, GetTypeCastName(classInfo), isBeforeChange, varName);
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            sb.Append($"""
                            var {varName} = new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{segment.PropertyTypeFullName}>(
                                (global::System.ComponentModel.INotifyPropertyChanging)obj,
                                "{segment.PropertyName}",
                                (global::System.ComponentModel.INotifyPropertyChanging __o) => (({GetTypeCastName(classInfo)})__o).{segment.PropertyName});
                """);
        }
        else
        {
            var propertyAccess = "obj." + segment.PropertyName;
            sb.Append($"            var {varName} = new global::ReactiveUI.Binding.Observables.ReturnObservable<{segment.PropertyTypeFullName}>({propertyAccess});");
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
        var obs0Var = varName + "_s0";
        var rootPlugin = classInfo is not null ? ObservationPluginRegistry.GetBestPlugin(classInfo) : null;

        if (rootPlugin is not null)
        {
            rootPlugin.EmitDeepChainRootSegment(sb, "obj", seg0, GetTypeCastName(classInfo), isBeforeChange, obs0Var);
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            sb.AppendLine($"""
                            var {obs0Var} = (global::System.IObservable<{seg0.PropertyTypeFullName}>)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{seg0.PropertyTypeFullName}>(
                                (global::System.ComponentModel.INotifyPropertyChanging)obj,
                                "{seg0.PropertyName}",
                                (global::System.ComponentModel.INotifyPropertyChanging __o) => (({GetTypeCastName(classInfo)})__o).{seg0.PropertyName});
                """);
        }
        else
        {
            sb.AppendLine($"            var {obs0Var} = (global::System.IObservable<{seg0.PropertyTypeFullName}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{seg0.PropertyTypeFullName}>(obj.{seg0.PropertyName});");
        }

        // Chain remaining segments using Select + Switch for reactive re-subscription
        for (var s = 1; s < path.Length; s++)
        {
            var seg = path[s];
            var prevObsVar = varName + "_s" + (s - 1);
            var curObsVar = varName + "_s" + s;
            var lambdaParam = varName + "_p" + s;

            // All plugins emit generic INPC-based PropertyObservable for inner segments,
            // so reusing the root plugin is safe regardless of the inner segment's declaring type.
            var innerPlugin = rootPlugin;

            if (innerPlugin is not null)
            {
                innerPlugin.EmitDeepChainInnerSegment(sb, prevObsVar, curObsVar, lambdaParam, seg, isBeforeChange);
            }
            else if (IsINPChanging(classInfo) && isBeforeChange)
            {
                var segType = seg.PropertyTypeFullName;
                sb.AppendLine()
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
                var segType = seg.PropertyTypeFullName;
                var declType = seg.DeclaringTypeFullName;
                sb.AppendLine()
                    .AppendLine($"""
                            var {curObsVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevObsVar},
                                    {lambdaParam} => (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(
                                        {lambdaParam} != null ? (({declType}){lambdaParam}).{seg.PropertyName} : default({segType}))));
                    """);
            }
        }

        var lastObsVar = varName + "_s" + (path.Length - 1);
        if (isBeforeChange)
        {
            sb.AppendLine($"            var {varName} = {lastObsVar};");
        }
        else
        {
            sb.AppendLine($"            var {varName} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.DistinctUntilChanged({lastObsVar});");
        }
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
        var keySb = new StringBuilder(128);

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            keySb.Clear()
                .Append(inv.SourceTypeFullName).Append('|')
                .Append(inv.ReturnTypeFullName).Append('|')
                .Append(inv.PropertyPaths.Length).Append('|')
                .Append(inv.HasSelector);
            for (var p = 0; p < inv.PropertyPaths.Length; p++)
            {
                var path = inv.PropertyPaths[p];
                keySb.Append('|').Append(path[path.Length - 1].PropertyTypeFullName);
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
            result.Add(new TypeGroup(kvp.Value[0], kvp.Value.ToArray()));
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

        sb.AppendLine($"""
                /// <summary>
                /// Concrete typed overload for {methodPrefix} on {first.SourceTypeFullName}.
                /// </summary>
                public static global::System.IObservable<{first.ReturnTypeFullName}> {methodPrefix}(
                    this {first.SourceTypeFullName} objectToMonitor,
        """);

        for (var i = 0; i < propCount; i++)
        {
            var type = first.PropertyPaths[i][first.PropertyPaths[i].Length - 1].PropertyTypeFullName;
            sb.AppendLine($"            global::System.Linq.Expressions.Expression<global::System.Func<{first.SourceTypeFullName}, {type}>> property{i + 1},");
        }

        if (hasSelector)
        {
            sb.AppendLine($"            {GetSelectorType(first)} selector,");
        }

        if (supportsCallerArgExpr)
        {
            for (var i = 0; i < propCount; i++)
            {
                sb.AppendLine($"            [global::System.Runtime.CompilerServices.CallerArgumentExpression(\"property{i + 1}\")] string property{i + 1}Expression = \"\",");
            }
        }

        sb.AppendLine("""
                    [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                    [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                {
        """);

        // Emit normalization to strip "static " prefix from CallerArgumentExpression values
        if (supportsCallerArgExpr)
        {
            for (var i = 0; i < propCount; i++)
            {
                var paramName = $"property{i + 1}Expression";
                sb.AppendLine($"""            {paramName} = {paramName}.StartsWith("static ") ? {paramName}.Substring(7) : {paramName};""");
            }

            sb.AppendLine();
        }

        // Emit runtime affinity check: allow user-registered plugins to override generated observation.
        // Only emit for overloads with <= 3 properties, matching RuntimeObservationFallback signatures.
        if (generatedAffinity >= 0 && propCount <= 3)
        {
            EmitAffinityCheck(sb, first, methodPrefix, propCount, hasSelector, generatedAffinity);
        }

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);

            if (supportsCallerArgExpr)
            {
                sb.Append($"            {condition} (");
                for (var p = 0; p < propCount; p++)
                {
                    sb.Append($"property{p + 1}Expression == \"{CodeGeneratorHelpers.EscapeString(inv.ExpressionTexts[p])}\"");
                    if (p < propCount - 1)
                    {
                        sb.Append(" && ");
                    }
                }

                sb.AppendLine(")");
            }
            else
            {
                var suffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
                sb.AppendLine($"""            {condition} (callerLineNumber == {inv.CallerLineNumber} && callerFilePath.EndsWith("{CodeGeneratorHelpers.EscapeString(suffix)}", global::System.StringComparison.OrdinalIgnoreCase))""");
            }

            sb.AppendLine("            {");
            var selectorArg = hasSelector ? ", selector" : string.Empty;
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, string.Join("|", inv.ExpressionTexts));
            sb.AppendLine($"                return __{methodPrefix}_{methodSuffix}(objectToMonitor{selectorArg});")
                .AppendLine("            }");
        }

        GenerateRuntimeFallback(sb, methodPrefix, propCount, hasSelector);

        sb.AppendLine("        }");
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
        sb.AppendLine($"            throw new global::System.InvalidOperationException(\"No generated {methodPrefix} dispatch matched. Ensure the expression is an inline lambda for compile-time optimization.\");");

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

        sb.AppendLine("            // Allow user-registered plugins with higher affinity to override generated observation")
            .AppendLine($"            if (global::ReactiveUI.Binding.Fallback.ObservationAffinityChecker.HasHigherAffinityPlugin(typeof({first.SourceTypeFullName}), {generatedAffinity}, {(isBeforeChange ? "true" : "false")}))")
            .AppendLine("            {");

        EmitAffinityFallbackReturn(sb, first, methodPrefix, propCount, hasSelector);

        sb.AppendLine("            }")
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
            propArgs.Append($", property{i + 1}");
        }

        if (!hasSelector)
        {
            // No selector: direct call to RuntimeObservationFallback
            sb.AppendLine($"                return global::ReactiveUI.Binding.Fallback.RuntimeObservationFallback.{fallbackMethod}(objectToMonitor{propArgs});");
        }
        else if (propCount == 1)
        {
            // Single property with selector: wrap fallback with SelectObservable
            var propType = first.PropertyPaths[0][first.PropertyPaths[0].Length - 1].PropertyTypeFullName;
            sb.AppendLine($"                return new global::ReactiveUI.Binding.Observables.SelectObservable<{propType}, {first.ReturnTypeFullName}>(")
                .AppendLine($"                    global::ReactiveUI.Binding.Fallback.RuntimeObservationFallback.{fallbackMethod}(objectToMonitor{propArgs}),")
                .AppendLine("                    selector);");
        }
        else
        {
            // Multi-property with selector: wrap fallback tuple with selector decomposition
            var tupleType = new StringBuilder("global::System.ValueTuple<");
            for (var i = 0; i < propCount; i++)
            {
                var path = first.PropertyPaths[i];
                tupleType.Append(path[path.Length - 1].PropertyTypeFullName);
                if (i < propCount - 1)
                {
                    tupleType.Append(", ");
                }
            }

            tupleType.Append('>');

            // Build the selector decomposition lambda: __t => selector(__t.Item1, __t.Item2, ...)
            var selectorArgs = new StringBuilder();
            for (var i = 0; i < propCount; i++)
            {
                selectorArgs.Append("__t.Item").Append(i + 1);
                if (i < propCount - 1)
                {
                    selectorArgs.Append(", ");
                }
            }

            sb.AppendLine($"                return new global::ReactiveUI.Binding.Observables.SelectObservable<{tupleType}, {first.ReturnTypeFullName}>(")
                .AppendLine($"                    global::ReactiveUI.Binding.Fallback.RuntimeObservationFallback.{fallbackMethod}(objectToMonitor{propArgs}),")
                .AppendLine($"                    __t => selector({selectorArgs}));");
        }
    }

    /// <summary>
    /// Generates a single-property observation method body using plugin dispatch.
    /// </summary>
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
            sb.Append("            return ");
            plugin.EmitShallowObservation(sb, "obj", segment, GetTypeCastName(classInfo), isBeforeChange, includeStartWith: true);
            sb.Append(';');
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            // INPChanging-only type (no INPC, no IReactiveObject) — can observe before-change
            sb.Append($"""
                            return new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{inv.ReturnTypeFullName}>(
                                (global::System.ComponentModel.INotifyPropertyChanging)obj,
                                "{propertyName}",
                                (global::System.ComponentModel.INotifyPropertyChanging __o) => (({inv.SourceTypeFullName})__o).{propertyName});
                """);
        }
        else
        {
            sb.Append($"            return new global::ReactiveUI.Binding.Observables.ReturnObservable<{inv.ReturnTypeFullName}>({propertyAccess});");
        }
    }

    /// <summary>
    /// Generates a deep chain observation method body using plugin dispatch
    /// for the root segment and inner segments.
    /// </summary>
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
            sb.AppendLine($"""
                            var __obs0 = (global::System.IObservable<{seg0.PropertyTypeFullName}>)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{seg0.PropertyTypeFullName}>(
                                (global::System.ComponentModel.INotifyPropertyChanging)obj,
                                "{seg0.PropertyName}",
                                (global::System.ComponentModel.INotifyPropertyChanging __o) => (({GetTypeCastName(classInfo)})__o).{seg0.PropertyName});
                """);
        }
        else
        {
            sb.AppendLine($"            var __obs0 = (global::System.IObservable<{seg0.PropertyTypeFullName}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{seg0.PropertyTypeFullName}>(obj.{seg0.PropertyName});");
        }

        // Chain remaining segments using Select + Switch for reactive re-subscription
        for (var s = 1; s < path.Length; s++)
        {
            var seg = path[s];
            var prevVar = $"__obs{s - 1}";
            var curVar = $"__obs{s}";
            var lambdaParam = $"__parent{s}";

            // All plugins emit generic INPC-based PropertyObservable for inner segments,
            // so reusing the root plugin is safe regardless of the inner segment's declaring type.
            var innerPlugin = rootPlugin;

            if (innerPlugin is not null)
            {
                innerPlugin.EmitDeepChainInnerSegment(sb, prevVar, curVar, lambdaParam, seg, isBeforeChange);
            }
            else if (IsINPChanging(classInfo) && isBeforeChange)
            {
                var segType = seg.PropertyTypeFullName;
                sb.AppendLine()
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
                var segType = seg.PropertyTypeFullName;
                var declType = seg.DeclaringTypeFullName;
                sb.AppendLine()
                    .AppendLine($"""
                            var {curVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevVar},
                                    {lambdaParam} => (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(
                                        {lambdaParam} != null ? (({declType}){lambdaParam}).{seg.PropertyName} : default({segType}))));
                    """);
            }
        }

        var lastObs = $"__obs{path.Length - 1}";
        if (isBeforeChange)
        {
            sb.Append($"            return {lastObs};");
        }
        else
        {
            sb.Append($"            return global::ReactiveUI.Binding.Observables.RxBindingExtensions.DistinctUntilChanged({lastObs});");
        }
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
                sb.AppendLine($"        var {variableName} = new global::ReactiveUI.Binding.Observables.ReturnObservable<{propertyTypeFullName}>({propertyAccess});");
            }
        }
        else
        {
            // Deep chain: emit Select/Switch pattern using plugin dispatch
            var seg0 = propertyPath[0];

            if (plugin is not null)
            {
                plugin.EmitDeepChainRootSegment(sb, rootVar, seg0, GetTypeCastName(classInfo), isBeforeChange: false, $"__{variableName}_s0");
            }
            else
            {
                sb.AppendLine($"            var __{variableName}_s0 = (global::System.IObservable<{seg0.PropertyTypeFullName}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{seg0.PropertyTypeFullName}>({rootVar}.{seg0.PropertyName});");
            }

            for (var s = 1; s < propertyPath.Length; s++)
            {
                var seg = propertyPath[s];
                var prevVar = $"__{variableName}_s{s - 1}";
                var curVar = $"__{variableName}_s{s}";
                var lambdaParam = $"__p{s}";

                if (plugin is not null)
                {
                    plugin.EmitDeepChainInnerSegment(sb, prevVar, curVar, lambdaParam, seg, isBeforeChange: false);
                }
                else
                {
                    var segType = seg.PropertyTypeFullName;
                    var declType = seg.DeclaringTypeFullName;
                    sb.AppendLine()
                        .AppendLine($"""
                            var {curVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevVar},
                                    {lambdaParam} => (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(
                                        {lambdaParam} != null ? (({declType}){lambdaParam}).{seg.PropertyName} : default({segType}))));
                        """);
                }
            }

            var lastSeg = $"__{variableName}_s{propertyPath.Length - 1}";
            sb.AppendLine($"        var {variableName} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.DistinctUntilChanged({lastSeg});");
        }
    }

    /// <summary>
    /// Groups invocations by source and return type signature for overload generation.
    /// </summary>
    /// <param name="First">The first invocation in the group, used for type information.</param>
    /// <param name="Invocations">All invocations sharing the same type signature.</param>
    internal sealed record TypeGroup(
        InvocationInfo First,
        InvocationInfo[] Invocations)
    {
        /// <summary>
        /// Gets the fully qualified name of the source type.
        /// </summary>
        public string SourceTypeFullName => First.SourceTypeFullName;

        /// <summary>
        /// Gets the fully qualified name of the return type.
        /// </summary>
        public string ReturnTypeFullName => First.ReturnTypeFullName;
    }
}

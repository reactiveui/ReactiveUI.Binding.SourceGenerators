// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
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
    /// <summary>Opens the lambda that reads the property back off a before-change notification.</summary>
    private const string ChangingReaderLambdaOpen = "                (global::System.ComponentModel.INotifyPropertyChanging __o) => ((";

    /// <summary>Passes the observed object to a before-change observation.</summary>
    private const string ChangingSourceArgument = "                (global::System.ComponentModel.INotifyPropertyChanging)obj,";

    /// <summary>Opens the branch a chain stage takes while its parent is present.</summary>
    private const string ObservableTrueBranchOpen = "                ? (global::System.IObservable<";

    /// <summary>Opens the branch a chain stage takes while its parent is null.</summary>
    private const string ObservableFalseBranchOpen = "                : (global::System.IObservable<";

    /// <summary>Tests that a chain stage's parent is present before observing it.</summary>
    private const string ParentPresentTest = " != null";

    /// <summary>Passes the observed object as the first argument of a generated call.</summary>
    private const string MonitoredObjectArgument = "(objectToMonitor";

    /// <summary>Opens the observation a property that never notifies is read through.</summary>
    private const string UnchangingObservableOpen = ")new global::ReactiveUI.Binding.Observables.UnchangingPropertyObservable<";

    /// <summary>Opens a before-change observation cast to the interface the chain stage expects.</summary>
    private const string ChangingObservableOpen = ">)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<";

    /// <summary>Names the local holding the observation the generator's own mechanism builds.</summary>
    private const string MechanismVariableSuffix = "Mechanism";

    /// <summary>Names the local holding the registration that outranked that mechanism.</summary>
    private const string RegistrationVariableSuffix = "Registration";

    /// <summary>The operator every observation ends with, so consecutive equal values are suppressed.</summary>
    private const string DistinctUntilChangedCall = "global::ReactiveUI.Primitives.LinqExtensions.DistinctUntilChanged";

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? Generate(
        ImmutableArray<InvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features,
        string methodPrefix) =>
        CodeGeneratorHelpers.GenerateDispatchFile(
            invocations,
            features,
            GroupByTypeSignature,
            (sb, group, snapshot) => GenerateGroup(
                sb,
                group,
                allClasses,
                snapshot.SupportsCallerArgExpr,
                snapshot.StubHasExpressionParameters,
                methodPrefix));

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

        _ = sb.Append("        private static global::System.IObservable<").Append(inv.ReturnTypeFullName).Append("> __").Append(prefix).Append('_')
            .Append(suffix).Append('(').Append(inv.SourceTypeFullName).Append(" obj").Append(selectorParam).AppendLine(")").AppendLine("        {");

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
                    _ = sb.Append("            return global::ReactiveUI.Primitives.LinqExtensions.Select(");
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
        var sb = new PooledStringBuilder().Append("global::System.Func<");
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            _ = sb.Append(path[path.Length - 1].PropertyTypeFullName).Append(", ");
        }

        _ = sb.Append(inv.ReturnTypeFullName).Append('>');
        return sb.ToStringAndReturn();
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

        _ = sb.AppendLine("            return global::ReactiveUI.Primitives.LinqExtensions.CombineLatest(");
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
            EmitCombineLatestValuesProjection(sb, inv);
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
        var plugin = ResolveRootPlugin(classInfo, segment);

        if (plugin is not null)
        {
            plugin.EmitShallowObservation(sb, "obj", segment, GetTypeCastName(classInfo), isBeforeChange, true);
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            _ = sb.Append("new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<").Append(segment.PropertyTypeFullName).Append(">((")
                .Append("global::System.ComponentModel.INotifyPropertyChanging)obj, \"").Append(segment.PropertyName)
                .Append("\", (global::System.ComponentModel.INotifyPropertyChanging __o) => (").Append('(').Append(GetTypeCastName(classInfo))
                .Append(GeneratedSyntax.ObserverCastClose).Append(segment.PropertyName).Append(')');
        }
        else
        {
            var propertyAccess = $"obj.{segment.PropertyName}";
            _ = sb.Append("new global::ReactiveUI.Binding.Observables.UnchangingPropertyObservable<").Append(segment.PropertyTypeFullName)
                .Append(">(").Append(propertyAccess).Append(')');
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
        var plugin = ResolveRootPlugin(classInfo, segment);
        var mechanismVariable = varName + MechanismVariableSuffix;

        if (plugin is not null)
        {
            plugin.EmitShallowObservationVariable(
                sb,
                "obj",
                segment,
                GetTypeCastName(classInfo),
                isBeforeChange,
                mechanismVariable);
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            _ = sb.Append(GeneratedSyntax.BodyLocalDeclaration).Append(mechanismVariable).Append(" = new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<")
                .Append(segment.PropertyTypeFullName).AppendLine(">(")
                .AppendLine(ChangingSourceArgument).Append(GeneratedSyntax.QuotedArgumentOpen)
                .Append(segment.PropertyName).AppendLine("\",")
                .Append(ChangingReaderLambdaOpen).Append(GetTypeCastName(classInfo))
                .Append(GeneratedSyntax.ObserverCastClose).Append(segment.PropertyName).Append(");");
        }
        else
        {
            var propertyAccess = $"obj.{segment.PropertyName}";
            _ = sb.Append(GeneratedSyntax.BodyLocalDeclaration).Append(mechanismVariable).Append(" = new global::ReactiveUI.Binding.Observables.UnchangingPropertyObservable<")
                .Append(segment.PropertyTypeFullName).Append(">(").Append(propertyAccess).Append(");");
        }

        _ = sb.AppendLine();

        EmitInlinePluginChoice(
            sb,
            "obj",
            segment,
            plugin?.Affinity ?? 0,
            isBeforeChange,
            new(GeneratedSyntax.BodyLocalDeclaration, "                ", mechanismVariable, varName));
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
        var rootPlugin = ResolveRootPlugin(classInfo, seg0);

        if (rootPlugin is not null)
        {
            rootPlugin.EmitDeepChainRootSegment(sb, "obj", seg0, GetTypeCastName(classInfo), isBeforeChange, obs0Var);
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            _ = sb.Append(GeneratedSyntax.BodyLocalDeclaration).Append(obs0Var).Append(" = (global::System.IObservable<").Append(seg0.PropertyTypeFullName)
                .Append(ChangingObservableOpen).Append(seg0.PropertyTypeFullName).AppendLine(">(")
                .AppendLine(ChangingSourceArgument).Append(GeneratedSyntax.QuotedArgumentOpen)
                .Append(seg0.PropertyName).AppendLine("\",").Append(ChangingReaderLambdaOpen)
                .Append(GetTypeCastName(classInfo)).Append(GeneratedSyntax.ObserverCastClose).Append(seg0.PropertyName).AppendLine(");");
        }
        else
        {
            _ = sb.Append(GeneratedSyntax.BodyLocalDeclaration).Append(obs0Var).Append(" = (global::System.IObservable<").Append(seg0.PropertyTypeFullName).Append('>')
                .Append(UnchangingObservableOpen).Append(seg0.PropertyTypeFullName).Append(">(obj.")
                .Append(seg0.PropertyName).AppendLine(");");
        }

        EmitDeepChainInnerSegments(sb, path, isBeforeChange, varName);

        var lastObsVar = $"{varName}_s{path.Length - 1}";

        // Distinct on both timings. The runtime engine asks for it whichever way it observes, so a
        // before-change stream that repeated a value would emit where the runtime engine stayed quiet.
        _ = sb.Append(GeneratedSyntax.BodyLocalDeclaration).Append(varName)
            .Append(" = ").Append(DistinctUntilChangedCall).Append('(')
            .Append(lastObsVar).AppendLine(");");
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
        var keySb = new PooledStringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);

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

        keySb.Return();

        var result = new List<TypeGroup>();
        foreach (var kvp in groupMap)
        {
            result.Add(new(kvp.Value[0], [.. kvp.Value]));
        }

        return result;
    }

    /// <summary>Generates a concrete typed extension method overload with its dispatch table.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    internal static void GenerateConcreteOverload(
        StringBuilder sb,
        TypeGroup group,
        bool supportsCallerArgExpr,
        bool stubHasExpressionParameters,
        string methodPrefix)
    {
        var first = group.First;
        var propCount = first.PropertyPaths.Length;
        var hasSelector = first.HasSelector;

        EmitOverloadSignature(sb, first, supportsCallerArgExpr, stubHasExpressionParameters, methodPrefix, propCount, hasSelector);
        CodeGeneratorHelpers.AppendIndexedStaticPrefixNormalization(sb, supportsCallerArgExpr, "property", propCount);

        // A registration that outranks the generated mechanism is honoured where the observation is built,
        // one property at a time, so the dispatch itself has nothing to decide.
        EmitDispatchTable(sb, group, supportsCallerArgExpr, methodPrefix, propCount, hasSelector);

        GenerateRuntimeFallback(sb, methodPrefix);

        _ = sb.AppendLine("        }");
    }

    /// <summary>
    /// Generates the throw path for when no generated dispatch match is found.
    /// Since the source generator matched all invocations at compile time, an unmatched
    /// dispatch indicates a caching issue — never falls back to runtime reflection.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void GenerateRuntimeFallback(StringBuilder sb, string methodPrefix) =>
    sb.Append("            throw new global::System.InvalidOperationException(\"No generated ").Append(methodPrefix)
        .AppendLine(" dispatch matched. Ensure the expression is an inline lambda for compile-time optimization.\");");

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
        var plugin = classInfo is not null
            ? ObservationPluginRegistry.GetBestPlugin(classInfo, propertyName)
            : null;

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
            _ = sb.Append("            return new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<")
                .Append(inv.ReturnTypeFullName).AppendLine(">(").AppendLine(ChangingSourceArgument)
                .Append(GeneratedSyntax.QuotedArgumentOpen).Append(propertyName).AppendLine("\",")
                .Append(ChangingReaderLambdaOpen).Append(inv.SourceTypeFullName)
                .Append(GeneratedSyntax.ObserverCastClose).Append(propertyName).Append(");");
        }
        else
        {
            _ = sb.Append("            return new global::ReactiveUI.Binding.Observables.UnchangingPropertyObservable<")
                .Append(inv.ReturnTypeFullName).Append(">(").Append(propertyAccess).Append(");");
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
        var rootPlugin = ResolveRootPlugin(classInfo, seg0);

        // First segment: observe root object for first property
        if (rootPlugin is not null)
        {
            rootPlugin.EmitDeepChainRootSegment(sb, "obj", seg0, GetTypeCastName(classInfo), isBeforeChange, "__obs0");
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            _ = sb.Append("            var __obs0 = (global::System.IObservable<").Append(seg0.PropertyTypeFullName)
                .Append(ChangingObservableOpen).Append(seg0.PropertyTypeFullName).AppendLine(">(")
                .AppendLine(ChangingSourceArgument).Append(GeneratedSyntax.QuotedArgumentOpen)
                .Append(seg0.PropertyName).AppendLine("\",").Append(ChangingReaderLambdaOpen)
                .Append(GetTypeCastName(classInfo)).Append(GeneratedSyntax.ObserverCastClose).Append(seg0.PropertyName).AppendLine(");");
        }
        else
        {
            _ = sb.Append("            var __obs0 = (global::System.IObservable<").Append(seg0.PropertyTypeFullName).Append('>')
                .Append(UnchangingObservableOpen).Append(seg0.PropertyTypeFullName).Append(">(obj.")
                .Append(seg0.PropertyName).AppendLine(");");
        }

        EmitObservationChainInnerSegments(sb, path, isBeforeChange);

        var lastObs = $"__obs{path.Length - 1}";
        _ = sb.Append("            return ").Append(DistinctUntilChangedCall).Append('(')
            .Append(lastObs).Append(");");
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
            var mechanismVariable = variableName + MechanismVariableSuffix;

            if (plugin is not null)
            {
                plugin.EmitInlineObservationVariable(sb, rootVar, segment, GetTypeCastName(classInfo), mechanismVariable);
            }
            else
            {
                var propertyAccess = $"{rootVar}.{segment.PropertyName}";
                _ = sb.Append(GeneratedSyntax.InlineLocalDeclaration).Append(mechanismVariable)
                    .Append(" = new global::ReactiveUI.Binding.Observables.UnchangingPropertyObservable<").Append(propertyTypeFullName).Append(">(")
                    .Append(propertyAccess).AppendLine(");");
            }

            EmitInlinePluginChoice(
                sb,
                rootVar,
                segment,
                plugin?.Affinity ?? 0,
                false,
                new(GeneratedSyntax.InlineLocalDeclaration, "            ", mechanismVariable, variableName));
            _ = sb.AppendLine();
        }
        else
        {
            EmitInlineDeepChain(sb, rootVar, propertyPath, classInfo, plugin, variableName);
        }
    }

    /// <summary>Renders a flag as the generated output spells it.</summary>
    /// <param name="value">The flag to render.</param>
    /// <returns>The literal a generated argument carries.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string BooleanLiteral(bool value) => value ? "true" : "false";

    /// <summary>Emits the choice between the mechanism the generator picked and a registration that outranks it.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable holding the observed object.</param>
    /// <param name="segment">The property being observed.</param>
    /// <param name="generatedAffinity">The affinity of the mechanism the generator picked.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="layout">Where the choice is written and what it names the locals it declares.</param>
    /// <remarks>
    /// The registration is resolved where the observation is built rather than at the call site's dispatch, so a
    /// binding keeps its generated write and only its reading changes. The property is named as a literal and
    /// read through an emitted accessor, and the expression handed to the registration is a lambda the compiler
    /// built, so nothing on this path is resolved by name and an ahead-of-time consumer carries no expression
    /// engine for it.
    /// </remarks>
    private static void EmitInlinePluginChoice(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        int generatedAffinity,
        bool isBeforeChange,
        in PluginChoiceLayout layout)
    {
        var declaringType = segment.DeclaringTypeFullName;
        var valueType = segment.PropertyTypeFullName;
        var pluginVariable = layout.VariableName + RegistrationVariableSuffix;
        var argumentIndent = $"{layout.ContinuationIndent}    ";
        const string observableOpen = "? (global::System.IObservable<";

        _ = sb.Append(layout.DeclarationPrefix).Append(pluginVariable).Append(" = ").Append(ObservationAffinityChecker)
            .Append(".FindHigherAffinityPlugin(typeof(").Append(declaringType).Append("), \"").Append(segment.PropertyName)
            .Append("\", ").Append(generatedAffinity).Append(", ").Append(BooleanLiteral(isBeforeChange)).AppendLine(");")
            .Append(layout.DeclarationPrefix).Append(layout.VariableName).Append(" = ").Append(pluginVariable).AppendLine(" == null")
            .Append(layout.ContinuationIndent).Append(observableOpen).Append(valueType).Append(">)").AppendLine(layout.MechanismVariable)
            .Append(layout.ContinuationIndent).Append(": (global::System.IObservable<").Append(valueType).Append(">)new ")
            .Append(PluginPropertyObservable).Append('<').Append(valueType).AppendLine(">(")
            .Append(argumentIndent).Append(pluginVariable).AppendLine(",")
            .Append(argumentIndent).Append(rootVar).AppendLine(",")
            .Append(argumentIndent).Append("((global::System.Linq.Expressions.Expression<global::System.Func<").Append(declaringType).Append(", ")
            .Append(valueType).Append(">>)(__e => __e.").Append(segment.PropertyName).AppendLine(")).Body,")
            .Append(argumentIndent).Append('"').Append(segment.PropertyName).AppendLine("\",")
            .Append(argumentIndent).Append("(object __o) => ((").Append(declaringType).Append(")__o).").Append(segment.PropertyName).AppendLine(",")
            .Append(argumentIndent).Append(BooleanLiteral(isBeforeChange)).AppendLine(",")
            .Append(argumentIndent).Append("true);");
    }

    /// <summary>Picks the observation plugin for the type that declares a chain segment's property.</summary>
    /// <param name="segment">The chain segment, which carries how its declaring type notifies.</param>
    /// <returns>The plugin for that type, or null to fall back to reading the property.</returns>
    /// <remarks>
    /// Each link of a chain is declared by its own type and notifies - or does not - on its own terms, so the
    /// mechanism is resolved per segment. Reusing the root's plugin emits its cast against whatever the segment
    /// declares, which is a build error where the two are unrelated and a failed cast at runtime where they are
    /// merely different.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IObservationPlugin? ResolveSegmentPlugin(PropertyPathSegment segment) =>
        segment.DeclaringTypeInfo is null
            ? null
            : ObservationPluginRegistry.GetBestPlugin(segment.DeclaringTypeInfo, segment.PropertyName);

    /// <summary>Resolves the mechanism for a property observed directly on the type a call site names.</summary>
    /// <param name="classInfo">The type the call site observes, or null when it was never detected.</param>
    /// <param name="segment">The property being observed.</param>
    /// <returns>The mechanism to observe it through, or null when nothing reaches it.</returns>
    /// <remarks>
    /// The type a call site names advertises the mechanism, but only the type that declares the property knows
    /// whether that property takes part in it - and the two differ for an inherited property, which the named
    /// type's own member list does not mention. Answering from the named type alone treats every inherited
    /// property as participating, which for a dependency object means emitting a companion field that an
    /// inherited plain property does not have, and the consumer's build is what discovers it.
    /// </remarks>
    private static IObservationPlugin? ResolveRootPlugin(ClassBindingInfo? classInfo, PropertyPathSegment segment)
    {
        if (classInfo is null)
        {
            return null;
        }

        return ObservedProperties.IsDeclaredByConsumer(classInfo, segment.PropertyName)
               || segment.DeclaringTypeInfo is null
            ? ObservationPluginRegistry.GetBestPlugin(classInfo, segment.PropertyName)
            : ObservationPluginRegistry.GetBestPlugin(segment.DeclaringTypeInfo, segment.PropertyName);
    }

    /// <summary>
    /// Chains the segments after the root for the standalone observation method, which names its
    /// stages <c>__obsN</c> rather than deriving them from a caller-supplied prefix.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="path">The property path being observed.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <remarks>
    /// Missing parents propagate through intermediate stages so downstream subscriptions detach,
    /// while the leaf suppresses the default value to match the runtime expression-chain fallback.
    /// </remarks>
    private static void EmitObservationChainInnerSegments(
        StringBuilder sb,
        EquatableArray<PropertyPathSegment> path,
        bool isBeforeChange)
    {
        for (var s = 1; s < path.Length; s++)
        {
            var seg = path[s];
            var prevVar = $"__obs{s - 1}";
            var curVar = $"__obs{s}";
            var lambdaParam = $"__parent{s}";
            var segType = seg.PropertyTypeFullName;
            var segInfo = seg.DeclaringTypeInfo;
            var segPlugin = ResolveSegmentPlugin(seg);

            // Only the leaf suppresses. Inner segments keep pushing the null downstream so the
            // stage below re-parents onto null and drops its subscription on the detached subtree.
            var nullParentBehavior = s == path.Length - 1
                ? NullParentObservationBehavior.SuppressEmission
                : NullParentObservationBehavior.EmitDefault;
            var nullParentObservable = nullParentBehavior == NullParentObservationBehavior.EmitDefault
                ? $"new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{segType}>(default({segType}))"
                : $"global::ReactiveUI.Primitives.Advanced.ImmutableEmptySignal<{segType}>.Instance";

            if (segPlugin is not null)
            {
                segPlugin.EmitDeepChainInnerSegment(
                    sb,
                    prevVar,
                    curVar,
                    lambdaParam,
                    seg,
                    isBeforeChange,
                    nullParentBehavior);
            }
            else if (IsINPChanging(segInfo) && isBeforeChange)
            {
                _ = sb.AppendLine().Append(GeneratedSyntax.InlineLocalDeclaration).Append(curVar).Append(" = ").Append(OpenChainSwitchMap(seg, segType, prevVar)).AppendLine()
                    .Append("            ").Append(lambdaParam).Append(" => ").Append(lambdaParam).AppendLine(ParentPresentTest)
                    .Append(ObservableTrueBranchOpen).Append(segType)
                    .Append(ChangingObservableOpen).Append(segType).AppendLine(">(")
                    .Append("                    (global::System.ComponentModel.INotifyPropertyChanging)").Append(lambdaParam).AppendLine(",")
                    .Append("                    \"").Append(seg.PropertyName).AppendLine("\",")
                    .Append("                    (global::System.ComponentModel.INotifyPropertyChanging __o) => ((").Append(seg.DeclaringTypeFullName)
                    .Append(GeneratedSyntax.ObserverCastClose).Append(seg.PropertyName).AppendLine(")").Append(ObservableFalseBranchOpen).Append(segType)
                    .Append(">)").Append(nullParentObservable).AppendLine(");");
            }
            else
            {
                _ = sb.AppendLine().Append(GeneratedSyntax.InlineLocalDeclaration).Append(curVar).Append(" = ").Append(OpenChainSwitchMap(seg, segType, prevVar)).AppendLine()
                    .Append("            ").Append(lambdaParam).Append(" => ").Append(lambdaParam).AppendLine(ParentPresentTest)
                    .Append(ObservableTrueBranchOpen).Append(segType).AppendLine(">)")
                    .Append("                    new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<").Append(segType).Append(">(((")
                    .Append(seg.DeclaringTypeFullName).Append(')').Append(lambdaParam).Append(").").Append(seg.PropertyName).AppendLine(")")
                    .Append(ObservableFalseBranchOpen).Append(segType).Append(">)").Append(nullParentObservable).AppendLine(");");
            }
        }
    }

    /// <summary>
    /// Chains the segments after the root with Select + Switch, so the observation re-subscribes when
    /// an intermediate value changes.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="path">The property path being observed.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="varName">The variable-name prefix for the emitted stages.</param>
    /// <remarks>
    /// Missing parents propagate through intermediate stages so downstream subscriptions detach,
    /// while the leaf suppresses the default value to match the runtime expression-chain fallback.
    /// </remarks>
    private static void EmitDeepChainInnerSegments(
        StringBuilder sb,
        EquatableArray<PropertyPathSegment> path,
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
            var segInfo = seg.DeclaringTypeInfo;
            var segPlugin = ResolveSegmentPlugin(seg);

            var nullParentBehavior = s == path.Length - 1
                ? NullParentObservationBehavior.SuppressEmission
                : NullParentObservationBehavior.EmitDefault;
            var nullParentObservable = nullParentBehavior == NullParentObservationBehavior.EmitDefault
                ? $"new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{segType}>(default({segType}))"
                : $"global::ReactiveUI.Primitives.Advanced.ImmutableEmptySignal<{segType}>.Instance";

            if (segPlugin is not null)
            {
                segPlugin.EmitDeepChainInnerSegment(
                    sb,
                    prevObsVar,
                    curObsVar,
                    lambdaParam,
                    seg,
                    isBeforeChange,
                    nullParentBehavior);
            }
            else if (IsINPChanging(segInfo) && isBeforeChange)
            {
                _ = sb.AppendLine().Append(GeneratedSyntax.InlineLocalDeclaration).Append(curObsVar).Append(" = ").Append(OpenChainSwitchMap(seg, segType, prevObsVar))
                    .AppendLine().Append("            ").Append(lambdaParam).Append(" => ").Append(lambdaParam).AppendLine(ParentPresentTest)
                    .Append(ObservableTrueBranchOpen).Append(segType)
                    .Append(ChangingObservableOpen).Append(segType).AppendLine(">(")
                    .Append("                    (global::System.ComponentModel.INotifyPropertyChanging)").Append(lambdaParam).AppendLine(",")
                    .Append("                    \"").Append(seg.PropertyName).AppendLine("\",")
                    .Append("                    (global::System.ComponentModel.INotifyPropertyChanging __o) => ((").Append(seg.DeclaringTypeFullName)
                    .Append(GeneratedSyntax.ObserverCastClose).Append(seg.PropertyName).AppendLine(")").Append(ObservableFalseBranchOpen).Append(segType)
                    .Append(">)").Append(nullParentObservable).AppendLine(");");
            }
            else
            {
                _ = sb.AppendLine().Append(GeneratedSyntax.InlineLocalDeclaration).Append(curObsVar).Append(" = ").Append(OpenChainSwitchMap(seg, segType, prevObsVar))
                    .AppendLine().Append("            ").Append(lambdaParam).Append(" => ").Append(lambdaParam).AppendLine(ParentPresentTest)
                    .Append(ObservableTrueBranchOpen).Append(segType).AppendLine(">)")
                    .Append("                    new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<").Append(segType).Append(">(((")
                    .Append(seg.DeclaringTypeFullName).Append(')').Append(lambdaParam).Append(").").Append(seg.PropertyName).AppendLine(")")
                    .Append(ObservableFalseBranchOpen).Append(segType).Append(">)").Append(nullParentObservable).AppendLine(");");
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
    /// <remarks>Missing parents emit defaults here so binding consumers can clear their targets.</remarks>
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
            _ = sb.Append("            var __").Append(variableName).Append("_s0 = (global::System.IObservable<").Append(seg0.PropertyTypeFullName)
                .Append('>').Append(UnchangingObservableOpen).Append(seg0.PropertyTypeFullName)
                .Append(">(").Append(rootVar).Append('.').Append(seg0.PropertyName).AppendLine(");");
        }

        for (var s = 1; s < propertyPath.Length; s++)
        {
            var seg = propertyPath[s];
            var prevVar = $"__{variableName}_s{s - 1}";
            var curVar = $"__{variableName}_s{s}";
            var lambdaParam = $"__p{s}";
            var segPlugin = ResolveSegmentPlugin(seg);

            if (segPlugin is not null)
            {
                segPlugin.EmitDeepChainInnerSegment(
                    sb,
                    prevVar,
                    curVar,
                    lambdaParam,
                    seg,
                    isBeforeChange: false,
                    nullParentBehavior: NullParentObservationBehavior.EmitDefault);
                continue;
            }

            var segType = seg.PropertyTypeFullName;
            var declType = seg.DeclaringTypeFullName;
            _ = sb.AppendLine().Append("    var ").Append(curVar).Append(" = ").Append(OpenChainSwitchMap(seg, segType, prevVar)).AppendLine()
                .Append("        ").Append(lambdaParam).Append(" => ").Append(lambdaParam).AppendLine(ParentPresentTest)
                .Append("            ? (global::System.IObservable<").Append(segType).AppendLine(">)")
                .Append("                new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<").Append(segType).Append(">(((")
                .Append(declType).Append(')').Append(lambdaParam).Append(").").Append(seg.PropertyName).AppendLine(")")
                .Append("            : (global::System.IObservable<").Append(segType)
                .Append(">)new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<").Append(segType).Append(">(default(").Append(segType)
                .AppendLine(")));");
        }

        var lastSeg = $"__{variableName}_s{propertyPath.Length - 1}";
        _ = sb.Append(GeneratedSyntax.InlineLocalDeclaration).Append(variableName).Append(" = global::ReactiveUI.Primitives.LinqExtensions.DistinctUntilChanged(")
            .Append(lastSeg).AppendLine(");");
    }

    /// <summary>
    /// Computes the worker-method suffix for an observation invocation, keyed only by the source type and
    /// property expression(s) — not the call site. Call sites that share the same type and expression(s)
    /// produce an identical worker, so they resolve to a single generated method.
    /// </summary>
    /// <param name="inv">The invocation info.</param>
    /// <returns>The stable, call-site-independent method-name suffix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string MethodSuffix(InvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.SourceTypeFullName,
            string.Empty,
            0,
            string.Join("|", inv.ExpressionTexts));

    /// <summary>Generates the concrete overload and per-invocation observation methods for a single type group.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The type group to generate code for.</param>
    /// <param name="allClasses">All detected class binding info for type mechanism lookup.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    private static void GenerateGroup(
        StringBuilder sb,
        TypeGroup group,
        ImmutableArray<ClassBindingInfo> allClasses,
        bool supportsCallerArgExpr,
        bool stubHasExpressionParameters,
        string methodPrefix)
    {
        // Generate the concrete typed extension method overload
        GenerateConcreteOverload(sb, group, supportsCallerArgExpr, stubHasExpressionParameters, methodPrefix);
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

            var classInfo = CodeGeneratorHelpers.ResolveObservedTypeInfo(
                allClasses,
                inv.SourceTypeFullName,
                inv.PropertyPaths[0]);

            GenerateObservationMethod(sb, inv, classInfo, suffix, inv.IsBeforeChange, methodPrefix);
        }
    }

    /// <summary>Emits the trailing projection lambda that gathers a selector-less <c>CombineLatest</c> into one emission.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation, whose return type is the emission being constructed.</param>
    private static void EmitCombineLatestValuesProjection(StringBuilder sb, InvocationInfo inv)
    {
        var propertyCount = inv.PropertyPaths.Length;

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

        _ = sb.Append(") => new ").Append(inv.ReturnTypeFullName).Append('(');
        for (var i = 0; i < propertyCount; i++)
        {
            _ = sb.Append('p').Append(i + 1);
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
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    /// <param name="propCount">The number of property expressions.</param>
    /// <param name="hasSelector">Whether a selector function is present.</param>
    private static void EmitOverloadSignature(
        StringBuilder sb,
        InvocationInfo first,
        bool supportsCallerArgExpr,
        bool stubHasExpressionParameters,
        string methodPrefix,
        int propCount,
        bool hasSelector)
    {
        _ = sb.AppendLine("        /// <summary>").Append("        /// Concrete typed overload for ").Append(methodPrefix).Append(" on ")
            .Append(first.SourceTypeFullName).AppendLine(".").AppendLine("        /// </summary>")
            .Append("        public static global::System.IObservable<").Append(first.ReturnTypeFullName).Append("> ").Append(methodPrefix)
            .AppendLine("(").Append("            this ").Append(first.SourceTypeFullName).AppendLine(" objectToMonitor,");

        for (var i = 0; i < propCount; i++)
        {
            var type = first.PropertyPaths[i][first.PropertyPaths[i].Length - 1].PropertyTypeFullName;
            _ = sb.Append("            global::System.Linq.Expressions.Expression<global::System.Func<").Append(first.SourceTypeFullName).Append(", ")
                .Append(type).Append(">> property").Append(i + 1).AppendLine(",");
        }

        if (hasSelector)
        {
            _ = sb.Append("            ").Append(GetSelectorType(first)).AppendLine(" selector,");
        }

        if (stubHasExpressionParameters)
        {
            for (var i = 0; i < propCount; i++)
            {
                CodeGeneratorHelpers.AppendExpressionParameter(
                    sb,
                    $"property{i + 1}",
                    $"property{i + 1}Expression",
                    supportsCallerArgExpr);
            }
        }

        _ = sb.AppendLine("""
                                  [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                                  [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                              {
                      """);
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
                _ = sb.Append("            ").Append(keyword).Append(" (callerLineNumber == ").Append(inv.CallerLineNumber)
                    .Append(" && callerFilePath.EndsWith(\"").Append(CodeGeneratorHelpers.EscapeString(suffix)).Append("\",")
                    .AppendLine(" global::System.StringComparison.OrdinalIgnoreCase))");
            }

            branchIndex++;
            _ = sb.AppendLine("            {");
            var selectorArg = hasSelector ? ", selector" : string.Empty;
            _ = sb.Append("                return __").Append(methodPrefix).Append('_').Append(MethodSuffix(inv)).Append(MonitoredObjectArgument)
                .Append(selectorArg).AppendLine(");").AppendLine("            }");
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
        _ = sb.Append("            ").Append(condition).Append(" (");
        for (var p = 0; p < propCount; p++)
        {
            _ = sb.Append("property").Append(p + 1).Append("Expression == \"").Append(CodeGeneratorHelpers.EscapeString(inv.ExpressionTexts[p]))
                .Append('"');
            if (p < propCount - 1)
            {
                _ = sb.Append(" && ");
            }
        }

        _ = sb.AppendLine(")");
    }

    /// <summary>Where a plugin choice is written, and what it names the two locals it declares.</summary>
    /// <param name="DeclarationPrefix">The <c>var</c> declaration at the indentation the surrounding body sits at.</param>
    /// <param name="ContinuationIndent">The indentation the branches of the choice are written at.</param>
    /// <param name="MechanismVariable">The local holding the observation the generator's own mechanism built.</param>
    /// <param name="VariableName">The local the chosen observation is assigned to.</param>
    internal readonly record struct PluginChoiceLayout(
        string DeclarationPrefix,
        string ContinuationIndent,
        string MechanismVariable,
        string VariableName);

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

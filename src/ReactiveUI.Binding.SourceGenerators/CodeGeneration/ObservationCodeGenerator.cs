// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Shared code generation logic for property observation APIs (WhenChanged, WhenChanging, WhenAnyValue).
/// Generates concrete typed extension method overloads and per-invocation observation methods.
/// </summary>
internal static class ObservationCodeGenerator
{
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

        // Group invocations by their method signature
        var groups = GroupByTypeSignature(invocations);

        for (int g = 0; g < groups.Count; g++)
        {
            var group = groups[g];

            // Generate the concrete typed extension method overload
            GenerateConcreteOverload(sb, group, supportsCallerArgExpr, methodPrefix);
            sb.AppendLine();

            // Generate the observation methods for each invocation in this group
            for (int i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var classInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.SourceTypeFullName);
                string suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, string.Join("|", inv.ExpressionTexts));
                GenerateObservationMethod(sb, inv, classInfo, suffix, isBeforeChange: inv.IsBeforeChange, methodPrefix);
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
        string selectorParam = inv.HasSelector ? ", " + GetSelectorType(inv) + " selector" : string.Empty;

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
                string propertyAccessChain = CodeGeneratorHelpers.BuildPropertyAccessChain("obj", path);
                string leafPropertyName = path.Length > 0 ? path[path.Length - 1].PropertyName : string.Empty;

                if (inv.HasSelector)
                {
                    sb.Append("            return global::ReactiveUI.Binding.Observables.ObservableExtensions.Select(");
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
        for (int i = 0; i < inv.PropertyPaths.Length; i++)
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
        for (int i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            string varName = "__propObs" + i;

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
        for (int i = 0; i < inv.PropertyPaths.Length; i++)
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
            for (int i = 0; i < inv.PropertyPaths.Length; i++)
            {
                sb.Append('p').Append(i + 1);
                if (i < inv.PropertyPaths.Length - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append(") => (");
            for (int i = 0; i < inv.PropertyPaths.Length; i++)
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
    /// Used for inline contexts such as a selector <c>.Select()</c> call.
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
        string leafPropertyName = path[0].PropertyName;
        string propertyAccess = "obj." + leafPropertyName;
        bool isINPC = (classInfo?.ImplementsIReactiveObject ?? false) || (classInfo?.ImplementsINPC ?? false);
        bool isINPChanging = (classInfo?.ImplementsIReactiveObject ?? false) || (classInfo?.ImplementsINPChanging ?? false);

        if (isINPC && !isBeforeChange)
        {
            sb.Append($$"""new global::ReactiveUI.Binding.Observables.PropertyObservable<{{path[0].PropertyTypeFullName}}>(obj, "{{leafPropertyName}}", static (global::System.ComponentModel.INotifyPropertyChanged __o) => (({{classInfo?.FullyQualifiedName ?? "object"}})__o).{{leafPropertyName}}, true)""");
        }
        else if (isINPChanging && isBeforeChange)
        {
            sb.Append($$"""new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{{path[0].PropertyTypeFullName}}>((global::System.ComponentModel.INotifyPropertyChanging)obj, "{{leafPropertyName}}", static (global::System.ComponentModel.INotifyPropertyChanging __o) => (({{classInfo?.FullyQualifiedName ?? "object"}})__o).{{leafPropertyName}})""");
        }
        else
        {
            sb.Append($$"""new global::ReactiveUI.Binding.Observables.ReturnObservable<{{path[0].PropertyTypeFullName}}>({{propertyAccess}})""");
        }
    }

    /// <summary>
    /// Generates a shallow (single-segment) path observable as a properly formatted local variable
    /// declaration. Follows the same multi-line Allman-brace style as <see cref="GenerateSinglePropertyObservation"/>
    /// and <see cref="GenerateDeepChainVariable"/>.
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
        string leafPropertyName = path[0].PropertyName;
        string propertyAccess = "obj." + leafPropertyName;
        bool isINPC = (classInfo?.ImplementsIReactiveObject ?? false) || (classInfo?.ImplementsINPC ?? false);
        bool isINPChanging = (classInfo?.ImplementsIReactiveObject ?? false) || (classInfo?.ImplementsINPChanging ?? false);

        if (isINPC && !isBeforeChange)
        {
            sb.Append($$"""
                            var {{varName}} = new global::ReactiveUI.Binding.Observables.PropertyObservable<{{path[0].PropertyTypeFullName}}>(
                                obj,
                                "{{leafPropertyName}}",
                                static (global::System.ComponentModel.INotifyPropertyChanged __o) => (({{classInfo?.FullyQualifiedName ?? "object"}})__o).{{leafPropertyName}},
                                true);
                """);
        }
        else if (isINPChanging && isBeforeChange)
        {
            sb.Append($$"""
                            var {{varName}} = new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{{path[0].PropertyTypeFullName}}>(
                                (global::System.ComponentModel.INotifyPropertyChanging)obj,
                                "{{leafPropertyName}}",
                                static (global::System.ComponentModel.INotifyPropertyChanging __o) => (({{classInfo?.FullyQualifiedName ?? "object"}})__o).{{leafPropertyName}});
                """);
        }
        else
        {
            sb.Append($$"""
                            var {{varName}} = new global::ReactiveUI.Binding.Observables.ReturnObservable<{{path[0].PropertyTypeFullName}}>({{propertyAccess}});
                """);
        }
    }

    /// <summary>
    /// Generates a deep chain observable as a properly formatted local variable declaration.
    /// Uses the same structured multi-line Allman-brace style as <see cref="GenerateDeepChainObservation"/>.
    /// Used to pre-declare deep chain observables before they are referenced inside CombineLatest
    /// for multi-property observations.
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
        string eventType = isBeforeChange ? "PropertyChangingEventArgs" : "PropertyChangedEventArgs";
        string eventName = isBeforeChange ? "PropertyChanging" : "PropertyChanged";
        string interfaceName = isBeforeChange
            ? "global::System.ComponentModel.INotifyPropertyChanging"
            : "global::System.ComponentModel.INotifyPropertyChanged";

        // First segment: observe root object for first property
        var seg0 = path[0];
        string obs0Var = varName + "_s0";

        if (isBeforeChange)
        {
            sb.AppendLine($$"""
                            var {{obs0Var}} = (global::System.IObservable<{{seg0.PropertyTypeFullName}}?>)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{{seg0.PropertyTypeFullName}}?>(
                                (global::System.ComponentModel.INotifyPropertyChanging)obj,
                                "{{seg0.PropertyName}}",
                                static (global::System.ComponentModel.INotifyPropertyChanging __o) => (({{classInfo?.FullyQualifiedName ?? "object"}})__o).{{seg0.PropertyName}});
                """);
        }
        else
        {
            sb.AppendLine($$"""
                            var {{obs0Var}} = (global::System.IObservable<{{seg0.PropertyTypeFullName}}?>)new global::ReactiveUI.Binding.Observables.PropertyObservable<{{seg0.PropertyTypeFullName}}?>(
                                obj,
                                "{{seg0.PropertyName}}",
                                static (global::System.ComponentModel.INotifyPropertyChanged __o) => (({{classInfo?.FullyQualifiedName ?? "object"}})__o).{{seg0.PropertyName}},
                                false);
                """);
        }

        // Chain remaining segments using Select + Switch for reactive re-subscription
        for (int s = 1; s < path.Length; s++)
        {
            var seg = path[s];
            string prevObsVar = varName + "_s" + (s - 1);
            string curObsVar = varName + "_s" + s;
            string lambdaParam = varName + "_p" + s;
            bool isLeaf = s == path.Length - 1;
            string segType = isLeaf ? seg.PropertyTypeFullName : seg.PropertyTypeFullName + "?";
            string innerObsType = isBeforeChange
                ? $"global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{segType}>"
                : $"global::ReactiveUI.Binding.Observables.PropertyObservable<{segType}>";

            if (isBeforeChange)
            {
                sb.AppendLine()
                    .AppendLine($$"""
                            var {{curObsVar}} = global::ReactiveUI.Binding.Observables.ObservableExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.ObservableExtensions.Select({{prevObsVar}},
                                    {{lambdaParam}} => {{lambdaParam}} != null
                                        ? (global::System.IObservable<{{segType}}>)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{{segType}}>(
                                            (global::System.ComponentModel.INotifyPropertyChanging){{lambdaParam}},
                                            "{{seg.PropertyName}}",
                                            static (global::System.ComponentModel.INotifyPropertyChanging __o) => (({{seg.DeclaringTypeFullName}})__o).{{seg.PropertyName}})
                                        : (global::System.IObservable<{{segType}}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{{segType}}>(default({{segType}}))));
                    """);
            }
            else
            {
                sb.AppendLine()
                    .AppendLine($$"""
                            var {{curObsVar}} = global::ReactiveUI.Binding.Observables.ObservableExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.ObservableExtensions.Select({{prevObsVar}},
                                    {{lambdaParam}} => {{lambdaParam}} != null
                                        ? (global::System.IObservable<{{segType}}>)new global::ReactiveUI.Binding.Observables.PropertyObservable<{{segType}}>(
                                            {{lambdaParam}},
                                            "{{seg.PropertyName}}",
                                            static (global::System.ComponentModel.INotifyPropertyChanged __o) => (({{seg.DeclaringTypeFullName}})__o).{{seg.PropertyName}},
                                            false)
                                        : (global::System.IObservable<{{segType}}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{{segType}}>(default({{segType}}))));
                    """);
            }
        }

        string lastObsVar = varName + "_s" + (path.Length - 1);
        if (isBeforeChange)
        {
            sb.AppendLine($$"""
                            var {{varName}} = {{lastObsVar}};
                """);
        }
        else
        {
            sb.AppendLine($$"""
                            var {{varName}} = global::ReactiveUI.Binding.Observables.ObservableExtensions.DistinctUntilChanged({{lastObsVar}});
                """);
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

        for (int i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            keySb.Clear()
                .Append(inv.SourceTypeFullName).Append('|')
                .Append(inv.ReturnTypeFullName).Append('|')
                .Append(inv.PropertyPaths.Length).Append('|')
                .Append(inv.HasSelector);
            for (int p = 0; p < inv.PropertyPaths.Length; p++)
            {
                var path = inv.PropertyPaths[p];
                keySb.Append('|').Append(path[path.Length - 1].PropertyTypeFullName);
            }

            string key = keySb.ToString();

            if (!groupMap.TryGetValue(key, out var list))
            {
                list = new List<InvocationInfo>();
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
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    internal static void GenerateConcreteOverload(
        StringBuilder sb,
        TypeGroup group,
        bool supportsCallerArgExpr,
        string methodPrefix)
    {
        var first = group.First;
        int propCount = first.PropertyPaths.Length;
        bool hasSelector = first.HasSelector;

        sb.AppendLine($$"""
                /// <summary>
                /// Concrete typed overload for {{methodPrefix}} on {{first.SourceTypeFullName}}.
                /// </summary>
                public static global::System.IObservable<{{first.ReturnTypeFullName}}> {{methodPrefix}}(
                    this {{first.SourceTypeFullName}} objectToMonitor,
        """);

        for (int i = 0; i < propCount; i++)
        {
            string type = first.PropertyPaths[i][first.PropertyPaths[i].Length - 1].PropertyTypeFullName;
            sb.AppendLine($"            global::System.Linq.Expressions.Expression<global::System.Func<{first.SourceTypeFullName}, {type}>> property{i + 1},");
        }

        if (hasSelector)
        {
            sb.AppendLine($"            {GetSelectorType(first)} selector,");
        }

        if (supportsCallerArgExpr)
        {
            for (int i = 0; i < propCount; i++)
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
            for (int i = 0; i < propCount; i++)
            {
                string paramName = $"property{i + 1}Expression";
                sb.AppendLine($$"""            {{paramName}} = {{paramName}}.StartsWith("static ") ? {{paramName}}.Substring(7) : {{paramName}};""");
            }

            sb.AppendLine();
        }

        for (int i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            string condition = i == 0 ? "if" : "else if";

            if (supportsCallerArgExpr)
            {
                sb.Append($"            {condition} (");
                for (int p = 0; p < propCount; p++)
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
                string suffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
                sb.AppendLine($$"""            {{condition}} (callerLineNumber == {{inv.CallerLineNumber}} && callerFilePath.EndsWith("{{CodeGeneratorHelpers.EscapeString(suffix)}}", global::System.StringComparison.OrdinalIgnoreCase))""");
            }

            sb.AppendLine("            {");
            string selectorArg = hasSelector ? ", selector" : string.Empty;
            string methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, string.Join("|", inv.ExpressionTexts));
            sb.AppendLine($"                return __{methodPrefix}_{methodSuffix}(objectToMonitor{selectorArg});")
                .AppendLine("            }");
        }

        GenerateRuntimeFallback(sb, methodPrefix, propCount, hasSelector);

        sb.AppendLine("""
                    }
            """);
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
        bool hasSelector)
    {
        sb.AppendLine($"            throw new global::System.InvalidOperationException(\"No generated {methodPrefix} dispatch matched. Ensure the expression is an inline lambda for compile-time optimization.\");");
    }

    /// <summary>
    /// Generates a single-property observation method body with properly formatted multi-line code.
    /// Emits an <c>Observable.Create</c> with a PropertyChanged/PropertyChanging handler,
    /// <c>.StartWith()</c> for the initial value, and <c>.DistinctUntilChanged()</c> to filter duplicates.
    /// Handles empty <c>PropertyName</c> (<c>""</c>) as a blanket "all properties changed" notification.
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
        bool isINPC = (classInfo?.ImplementsIReactiveObject ?? false) || (classInfo?.ImplementsINPC ?? false);
        bool isINPChanging = (classInfo?.ImplementsIReactiveObject ?? false) || (classInfo?.ImplementsINPChanging ?? false);

        if (isINPC && !isBeforeChange)
        {
            sb.Append($$"""
                            return new global::ReactiveUI.Binding.Observables.PropertyObservable<{{inv.ReturnTypeFullName}}>(
                                obj,
                                "{{propertyName}}",
                                static (global::System.ComponentModel.INotifyPropertyChanged __o) => (({{inv.SourceTypeFullName}})__o).{{propertyName}},
                                true);
                """);
        }
        else if (isINPChanging && isBeforeChange)
        {
            sb.Append($$"""
                            return new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{{inv.ReturnTypeFullName}}>(
                                (global::System.ComponentModel.INotifyPropertyChanging)obj,
                                "{{propertyName}}",
                                static (global::System.ComponentModel.INotifyPropertyChanging __o) => (({{inv.SourceTypeFullName}})__o).{{propertyName}});
                """);
        }
        else
        {
            sb.Append($$"""
                            return new global::ReactiveUI.Binding.Observables.ReturnObservable<{{inv.ReturnTypeFullName}}>({{propertyAccess}});
                """);
        }
    }

    /// <summary>
    /// Generates a deep chain observation method body for a single-property deep path
    /// (e.g. <c>x =&gt; x.Address.City</c>). Uses the Select/Switch pattern with INPC subscriptions
    /// at each depth level for reactive re-subscription when intermediate objects change.
    /// Handles empty <c>PropertyName</c> (<c>""</c>) as a blanket "all properties changed" notification.
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

        // Generate lightweight deep chain observation using PropertyObservable/PropertyChangingObservable.
        // Each segment produces an observable that watches its parent for changes and re-subscribes via Switch.
        //
        // For path x.Address.City:
        //   obs0 = PropertyObservable for obj."Address"
        //   obs1 = Switch(Select(obs0, addr => addr != null ? PropertyObservable(addr, "City") : Return(default)))
        //   return DistinctUntilChanged(obs1)

        // First segment: observe root object for first property
        var seg0 = path[0];

        if (isBeforeChange)
        {
            sb.AppendLine($$"""
                            var __obs0 = (global::System.IObservable<{{seg0.PropertyTypeFullName}}?>)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{{seg0.PropertyTypeFullName}}?>(
                                (global::System.ComponentModel.INotifyPropertyChanging)obj,
                                "{{seg0.PropertyName}}",
                                static (global::System.ComponentModel.INotifyPropertyChanging __o) => (({{classInfo?.FullyQualifiedName ?? "object"}})__o).{{seg0.PropertyName}});
                """);
        }
        else
        {
            sb.AppendLine($$"""
                            var __obs0 = (global::System.IObservable<{{seg0.PropertyTypeFullName}}?>)new global::ReactiveUI.Binding.Observables.PropertyObservable<{{seg0.PropertyTypeFullName}}?>(
                                obj,
                                "{{seg0.PropertyName}}",
                                static (global::System.ComponentModel.INotifyPropertyChanged __o) => (({{classInfo?.FullyQualifiedName ?? "object"}})__o).{{seg0.PropertyName}},
                                false);
                """);
        }

        // Chain remaining segments using Select + Switch for reactive re-subscription
        for (int s = 1; s < path.Length; s++)
        {
            var seg = path[s];
            string prevVar = $"__obs{s - 1}";
            string curVar = $"__obs{s}";
            string lambdaParam = $"__parent{s}";
            bool isLeaf = s == path.Length - 1;
            string segType = isLeaf ? seg.PropertyTypeFullName : seg.PropertyTypeFullName + "?";

            if (isBeforeChange)
            {
                sb.AppendLine()
                    .AppendLine($$"""
                            var {{curVar}} = global::ReactiveUI.Binding.Observables.ObservableExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.ObservableExtensions.Select({{prevVar}},
                                    {{lambdaParam}} => {{lambdaParam}} != null
                                        ? (global::System.IObservable<{{segType}}>)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{{segType}}>(
                                            (global::System.ComponentModel.INotifyPropertyChanging){{lambdaParam}},
                                            "{{seg.PropertyName}}",
                                            static (global::System.ComponentModel.INotifyPropertyChanging __o) => (({{seg.DeclaringTypeFullName}})__o).{{seg.PropertyName}})
                                        : (global::System.IObservable<{{segType}}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{{segType}}>(default({{segType}}))));
                    """);
            }
            else
            {
                sb.AppendLine()
                    .AppendLine($$"""
                            var {{curVar}} = global::ReactiveUI.Binding.Observables.ObservableExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.ObservableExtensions.Select({{prevVar}},
                                    {{lambdaParam}} => {{lambdaParam}} != null
                                        ? (global::System.IObservable<{{segType}}>)new global::ReactiveUI.Binding.Observables.PropertyObservable<{{segType}}>(
                                            {{lambdaParam}},
                                            "{{seg.PropertyName}}",
                                            static (global::System.ComponentModel.INotifyPropertyChanged __o) => (({{seg.DeclaringTypeFullName}})__o).{{seg.PropertyName}},
                                            false)
                                        : (global::System.IObservable<{{segType}}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{{segType}}>(default({{segType}}))));
                    """);
            }
        }

        string lastObs = $"__obs{path.Length - 1}";
        if (isBeforeChange)
        {
            sb.Append($$"""
                            return {{lastObs}};
                """);
        }
        else
        {
            sb.Append($$"""
                            return global::ReactiveUI.Binding.Observables.ObservableExtensions.DistinctUntilChanged({{lastObs}});
                """);
        }
    }

    /// <summary>
    /// Emits an inline INPC after-change observation expression as a variable assignment.
    /// Used by binding generators to emit direct Observable.Create observation code
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
        bool isINPC = (classInfo?.ImplementsIReactiveObject ?? false) || (classInfo?.ImplementsINPC ?? false);

        if (propertyPath.Length == 1)
        {
            string propertyName = propertyPath[0].PropertyName;
            string propertyAccess = $"{rootVar}.{propertyName}";

            if (isINPC)
            {
                sb.AppendLine($$"""
                            var {{variableName}} = new global::ReactiveUI.Binding.Observables.PropertyObservable<{{propertyTypeFullName}}>(
                                {{rootVar}},
                                "{{propertyName}}",
                                static (global::System.ComponentModel.INotifyPropertyChanged __o) => (({{classInfo?.FullyQualifiedName ?? "object"}})__o).{{propertyName}},
                                true);
                    """);
            }
            else
            {
                sb.AppendLine($$"""
                            var {{variableName}} = new global::ReactiveUI.Binding.Observables.ReturnObservable<{{propertyTypeFullName}}>({{propertyAccess}});
                    """);
            }
        }
        else
        {
            // Deep chain: emit Select/Switch pattern using lightweight observables
            var seg0 = propertyPath[0];
            sb.AppendLine($$"""
                            var __{{variableName}}_s0 = (global::System.IObservable<{{seg0.PropertyTypeFullName}}?>)new global::ReactiveUI.Binding.Observables.PropertyObservable<{{seg0.PropertyTypeFullName}}?>(
                                {{rootVar}},
                                "{{seg0.PropertyName}}",
                                static (global::System.ComponentModel.INotifyPropertyChanged __o) => (({{classInfo?.FullyQualifiedName ?? "object"}})__o).{{seg0.PropertyName}},
                                false);
                    """);

            for (int s = 1; s < propertyPath.Length; s++)
            {
                var seg = propertyPath[s];
                string prevVar = $"__{variableName}_s{s - 1}";
                string curVar = $"__{variableName}_s{s}";
                string lambdaParam = $"__p{s}";
                bool isLeaf = s == propertyPath.Length - 1;
                string segType = isLeaf ? seg.PropertyTypeFullName : seg.PropertyTypeFullName + "?";

                sb.AppendLine()
                    .AppendLine($$"""
                            var {{curVar}} = global::ReactiveUI.Binding.Observables.ObservableExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.ObservableExtensions.Select({{prevVar}},
                                    {{lambdaParam}} => {{lambdaParam}} != null
                                        ? (global::System.IObservable<{{segType}}>)new global::ReactiveUI.Binding.Observables.PropertyObservable<{{segType}}>(
                                            {{lambdaParam}},
                                            "{{seg.PropertyName}}",
                                            static (global::System.ComponentModel.INotifyPropertyChanged __o) => (({{seg.DeclaringTypeFullName}})__o).{{seg.PropertyName}},
                                            false)
                                        : (global::System.IObservable<{{segType}}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{{segType}}>(default({{segType}}))));
                        """);
            }

            string lastSeg = $"__{variableName}_s{propertyPath.Length - 1}";
            sb.AppendLine($$"""
                            var {{variableName}} = global::ReactiveUI.Binding.Observables.ObservableExtensions.DistinctUntilChanged({{lastSeg}});
                    """);
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

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads for WhenAnyObservable invocations.
/// Generates Switch (single), Merge (multi same-type), and CombineLatest (multi with selector)
/// patterns for observing properties that are themselves observables.
/// </summary>
internal static class WhenAnyObservableCodeGenerator
{
    /// <summary>
    /// Generates concrete typed overloads and observation methods for WhenAnyObservable invocations.
    /// </summary>
    /// <param name="invocations">All detected WhenAnyObservable invocations.</param>
    /// <param name="allClasses">All detected class binding info for type mechanism lookup.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression (C# 10+).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    internal static string? Generate(
        ImmutableArray<WhenAnyObservableInvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        bool supportsCallerArgExpr)
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
            GenerateConcreteOverload(sb, group, supportsCallerArgExpr);
            sb.AppendLine();

            // Generate the observation methods for each invocation in this group
            for (int i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var classInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.SourceTypeFullName);
                string suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, string.Join("|", inv.ExpressionTexts));
                GenerateObservationMethod(sb, inv, classInfo, suffix);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Generates a concrete typed extension method overload with dispatch logic for WhenAnyObservable.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    internal static void GenerateConcreteOverload(
        StringBuilder sb,
        TypeGroup group,
        bool supportsCallerArgExpr)
    {
        var first = group.First;
        int propCount = first.PropertyPaths.Length;
        bool hasSelector = first.HasSelector;

        sb.AppendLine($$"""
                /// <summary>
                /// Concrete typed overload for WhenAnyObservable on {{first.SourceTypeFullName}}.
                /// </summary>
                public static global::System.IObservable<{{first.ReturnTypeFullName}}> WhenAnyObservable(
                    this {{first.SourceTypeFullName}} objectToMonitor,
        """);

        for (int i = 0; i < propCount; i++)
        {
            string innerType = first.InnerObservableTypeFullNames[i];
            string obsType = $"global::System.IObservable<{innerType}>?";
            sb.AppendLine($"            global::System.Linq.Expressions.Expression<global::System.Func<{first.SourceTypeFullName}, {obsType}>> obs{i + 1},");
        }

        if (hasSelector)
        {
            sb.Append("            ").Append(GetSelectorType(first)).AppendLine(" selector,");
        }

        if (supportsCallerArgExpr)
        {
            for (int i = 0; i < propCount; i++)
            {
                sb.AppendLine($"            [global::System.Runtime.CompilerServices.CallerArgumentExpression(\"obs{i + 1}\")] string obs{i + 1}Expression = \"\",");
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
                string paramName = $"obs{i + 1}Expression";
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
                    sb.Append($"obs{p + 1}Expression == \"{CodeGeneratorHelpers.EscapeString(inv.ExpressionTexts[p])}\"");
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
            sb.AppendLine($"                return __WhenAnyObservable_{methodSuffix}(objectToMonitor{selectorArg});")
                .AppendLine("            }");
        }

        // Runtime fallback: throw for now (WhenAnyObservable doesn't have a simple fallback path)
        sb.AppendLine("            throw new global::System.InvalidOperationException(\"No generated WhenAnyObservable dispatch matched. This indicates a source generator caching issue.\");")
            .AppendLine("""
                    }
            """);
    }

    /// <summary>
    /// Generates an observation method for a single WhenAnyObservable invocation.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="suffix">The stable method suffix.</param>
    internal static void GenerateObservationMethod(
        StringBuilder sb,
        WhenAnyObservableInvocationInfo inv,
        ClassBindingInfo? classInfo,
        string suffix)
    {
        string selectorParam = inv.HasSelector ? ", " + GetSelectorType(inv) + " selector" : string.Empty;

        sb.AppendLine($$"""
                    private static global::System.IObservable<{{inv.ReturnTypeFullName}}> __WhenAnyObservable_{{suffix}}({{inv.SourceTypeFullName}} obj{{selectorParam}})
                    {
            """);

        if (inv.PropertyPaths.Length == 1)
        {
            GenerateSingleObservableSwitch(sb, inv, classInfo);
        }
        else if (!inv.HasSelector)
        {
            GenerateMultiObservableMerge(sb, inv, classInfo);
        }
        else
        {
            GenerateMultiObservableCombineLatest(sb, inv, classInfo);
        }

        sb.AppendLine()
            .AppendLine("        }")
            .AppendLine();
    }

    /// <summary>
    /// Generates a single-property Switch pattern: observe the IObservable property, switch to its latest value.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    internal static void GenerateSingleObservableSwitch(
        StringBuilder sb,
        WhenAnyObservableInvocationInfo inv,
        ClassBindingInfo? classInfo)
    {
        var path = inv.PropertyPaths[0];
        string innerType = inv.InnerObservableTypeFullNames[0];

        // Generate property observation for the observable property itself
        if (path.Length > 1)
        {
            ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, isBeforeChange: false, "__obsProperty");
        }
        else
        {
            ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, isBeforeChange: false, "__obsProperty");
        }

        sb.AppendLine()
            .AppendLine();

        // Switch pattern: take the observable property value, replace null with Empty, and switch
        sb.Append($$"""
                        return global::ReactiveUI.Binding.Observables.ObservableExtensions.Switch(
                            global::ReactiveUI.Binding.Observables.ObservableExtensions.Select(__obsProperty,
                                __obs => __obs ?? (global::System.IObservable<{{innerType}}>)global::ReactiveUI.Binding.Observables.EmptyObservable<{{innerType}}>.Instance));
            """);
    }

    /// <summary>
    /// Generates a multi-property Merge pattern: observe each IObservable property, switch each, then merge.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    internal static void GenerateMultiObservableMerge(
        StringBuilder sb,
        WhenAnyObservableInvocationInfo inv,
        ClassBindingInfo? classInfo)
    {
        // Generate switched observable for each property
        for (int i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            string innerType = inv.InnerObservableTypeFullNames[i];
            string rawVar = "__obsProperty" + i;
            string switchedVar = "__switched" + i;

            if (path.Length > 1)
            {
                ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, isBeforeChange: false, rawVar);
            }
            else
            {
                ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, isBeforeChange: false, rawVar);
            }

            sb.AppendLine()
                .AppendLine()
                .AppendLine($$"""
                            var {{switchedVar}} = global::ReactiveUI.Binding.Observables.ObservableExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.ObservableExtensions.Select({{rawVar}},
                                    __obs => __obs ?? (global::System.IObservable<{{innerType}}>)global::ReactiveUI.Binding.Observables.EmptyObservable<{{innerType}}>.Instance));
                """)
                .AppendLine();
        }

        sb.AppendLine("            return global::ReactiveUI.Binding.Observables.ObservableExtensions.Merge(");
        for (int i = 0; i < inv.PropertyPaths.Length; i++)
        {
            sb.Append("                __switched").Append(i);
            if (i < inv.PropertyPaths.Length - 1)
            {
                sb.AppendLine(",");
            }
        }

        sb.Append(");");
    }

    /// <summary>
    /// Generates a multi-property CombineLatest pattern with selector: observe each IObservable property,
    /// switch each, then CombineLatest with the selector.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    internal static void GenerateMultiObservableCombineLatest(
        StringBuilder sb,
        WhenAnyObservableInvocationInfo inv,
        ClassBindingInfo? classInfo)
    {
        // Generate switched observable for each property
        for (int i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            string innerType = inv.InnerObservableTypeFullNames[i];
            string rawVar = "__obsProperty" + i;
            string switchedVar = "__switched" + i;

            if (path.Length > 1)
            {
                ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, isBeforeChange: false, rawVar);
            }
            else
            {
                ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, isBeforeChange: false, rawVar);
            }

            sb.AppendLine()
                .AppendLine()
                .AppendLine($$"""
                            var {{switchedVar}} = global::ReactiveUI.Binding.Observables.ObservableExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.ObservableExtensions.Select({{rawVar}},
                                    __obs => __obs ?? (global::System.IObservable<{{innerType}}>)global::ReactiveUI.Binding.Observables.EmptyObservable<{{innerType}}>.Instance));
                """)
                .AppendLine();
        }

        sb.AppendLine("            return global::ReactiveUI.Binding.Observables.CombineLatestObservable.Create(");
        for (int i = 0; i < inv.PropertyPaths.Length; i++)
        {
            sb.Append("                __switched").Append(i).AppendLine(",");
        }

        sb.Append("                selector);");
    }

    /// <summary>
    /// Gets the Func type signature for a WhenAnyObservable selector parameter.
    /// </summary>
    /// <param name="inv">The invocation info.</param>
    /// <returns>A fully qualified Func type string.</returns>
    internal static string GetSelectorType(WhenAnyObservableInvocationInfo inv)
    {
        var sb = new StringBuilder("global::System.Func<");
        for (int i = 0; i < inv.InnerObservableTypeFullNames.Length; i++)
        {
            sb.Append(inv.InnerObservableTypeFullNames[i]).Append("?, ");
        }

        sb.Append(inv.ReturnTypeFullName).Append('>');
        return sb.ToString();
    }

    /// <summary>
    /// Groups WhenAnyObservable invocations by their type signature for overload generation.
    /// </summary>
    /// <param name="invocations">All detected invocations.</param>
    /// <returns>A list of type groups.</returns>
    internal static List<TypeGroup> GroupByTypeSignature(ImmutableArray<WhenAnyObservableInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<WhenAnyObservableInvocationInfo>>(invocations.Length);
        var keySb = new StringBuilder(128);

        for (int i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            keySb.Clear()
                .Append(inv.SourceTypeFullName).Append('|')
                .Append(inv.ReturnTypeFullName).Append('|')
                .Append(inv.PropertyPaths.Length).Append('|')
                .Append(inv.HasSelector);

            for (int p = 0; p < inv.InnerObservableTypeFullNames.Length; p++)
            {
                keySb.Append('|').Append(inv.InnerObservableTypeFullNames[p]);
            }

            string key = keySb.ToString();

            if (!groupMap.TryGetValue(key, out var list))
            {
                list = new List<WhenAnyObservableInvocationInfo>();
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
    /// Groups invocations by source type and observable type signature for overload generation.
    /// </summary>
    /// <param name="First">The first invocation in the group, used for type information.</param>
    /// <param name="Invocations">All invocations sharing the same type signature.</param>
    internal sealed record TypeGroup(
        WhenAnyObservableInvocationInfo First,
        WhenAnyObservableInvocationInfo[] Invocations);
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
    /// <summary>The base name used to build emitted local variable identifiers for the raw observable property.</summary>
    private const string ObsPropertyVarName = "__obsProperty";

    /// <summary>Generates concrete typed overloads and observation methods for WhenAnyObservable invocations.</summary>
    /// <param name="invocations">All detected WhenAnyObservable invocations.</param>
    /// <param name="allClasses">All detected class binding info for type mechanism lookup.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    internal static string? Generate(
        ImmutableArray<WhenAnyObservableInvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return null;
        }

        var sb = PooledBuilder.Rent(invocations.Length * CodeGeneratorHelpers.PerInvocationBufferCapacity);
        var supportsCallerArgExpr = features.SupportsCallerArgExpr;
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb, features);
        _ = sb.AppendLine();

        // Group invocations by their method signature
        var groups = GroupByTypeSignature(invocations);

        for (var g = 0; g < groups.Count; g++)
        {
            var group = groups[g];

            // Generate the concrete typed extension method overload
            GenerateConcreteOverload(sb, group, supportsCallerArgExpr, features.SupportsNullable, features.StubHasExpressionParameters);
            _ = sb.AppendLine();

            // Generate the observation methods for each invocation in this group
            for (var i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var classInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.SourceTypeFullName);
                var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                    inv.SourceTypeFullName,
                    inv.CallerFilePath,
                    inv.CallerLineNumber,
                    string.Join("|", inv.ExpressionTexts));
                GenerateObservationMethod(sb, inv, classInfo, suffix);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        _ = sb.AppendLine();

        return PooledBuilder.ToStringAndReturn(sb);
    }

    /// <summary>Generates a concrete typed extension method overload with dispatch logic for WhenAnyObservable.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateConcreteOverload(
        StringBuilder sb,
        TypeGroup group,
        bool supportsCallerArgExpr,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        var first = group.First;
        var propCount = first.PropertyPaths.Length;
        var hasSelector = first.HasSelector;

        _ = sb.AppendLine($"""
                               /// <summary>
                               /// Concrete typed overload for WhenAnyObservable on {first.SourceTypeFullName}.
                               /// </summary>
                               public static global::System.IObservable<{first.ReturnTypeFullName}> WhenAnyObservable(
                                   this {first.SourceTypeFullName} objectToMonitor,
                       """);

        for (var i = 0; i < propCount; i++)
        {
            var innerType = first.InnerObservableTypeFullNames[i];
            var obsType = $"global::System.IObservable<{innerType}>{(supportsNullable ? "?" : string.Empty)}";
            _ = sb.AppendLine(
                $"            global::System.Linq.Expressions.Expression<global::System.Func<{first.SourceTypeFullName}, {obsType}>> obs{i + 1},");
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
                    $"obs{i + 1}",
                    $"obs{i + 1}Expression",
                    supportsCallerArgExpr);
            }
        }

        _ = sb.AppendLine("""
                                  [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                                  [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                              {
                      """);

        EmitStaticPrefixNormalization(sb, supportsCallerArgExpr, propCount);
        EmitDispatchTable(sb, group, supportsCallerArgExpr, propCount, hasSelector);

        // Runtime fallback: throw for now (WhenAnyObservable doesn't have a simple fallback path)
        _ = sb.AppendLine(
                "            throw new global::System.InvalidOperationException(\"No generated WhenAnyObservable dispatch matched. This indicates a source generator caching issue.\");")
            .AppendLine("        }");
    }

    /// <summary>Generates an observation method for a single WhenAnyObservable invocation.</summary>
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
        var selectorParam = inv.HasSelector ? $", {GetSelectorType(inv)} selector" : string.Empty;

        _ = sb.AppendLine($$"""
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

        _ = sb.AppendLine()
            .AppendLine("        }")
            .AppendLine();
    }

    /// <summary>Generates a single-property Switch pattern: observe the IObservable property, switch to its latest value.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    internal static void GenerateSingleObservableSwitch(
        StringBuilder sb,
        WhenAnyObservableInvocationInfo inv,
        ClassBindingInfo? classInfo)
    {
        var path = inv.PropertyPaths[0];
        var innerType = inv.InnerObservableTypeFullNames[0];

        // Generate property observation for the observable property itself
        if (path.Length > 1)
        {
            ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, false, ObsPropertyVarName);
        }
        else
        {
            ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, false, ObsPropertyVarName);
        }

        _ = sb.AppendLine()
            .AppendLine();

        // Switch pattern: take the observable property value, replace null with Empty, and switch
        _ = sb.Append($"""
                               return global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                   global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select(__obsProperty,
                                       __obs => __obs ?? (global::System.IObservable<{innerType}>)global::ReactiveUI.Binding.Observables.EmptyObservable<{innerType}>.Instance));
                   """);
    }

    /// <summary>Generates a multi-property Merge pattern: observe each IObservable property, switch each, then merge.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    internal static void GenerateMultiObservableMerge(
        StringBuilder sb,
        WhenAnyObservableInvocationInfo inv,
        ClassBindingInfo? classInfo)
    {
        // Generate switched observable for each property
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            var innerType = inv.InnerObservableTypeFullNames[i];
            var rawVar = ObsPropertyVarName + i;
            var switchedVar = $"__switched{i}";

            if (path.Length > 1)
            {
                ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, false, rawVar);
            }
            else
            {
                ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, false, rawVar);
            }

            _ = sb.AppendLine()
                .AppendLine()
                .AppendLine($"""
                                         var {switchedVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                             global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({rawVar},
                                                 __obs => __obs ?? (global::System.IObservable<{innerType}>)global::ReactiveUI.Binding.Observables.EmptyObservable<{innerType}>.Instance));
                             """)
                .AppendLine();
        }

        _ = sb.AppendLine("            return global::ReactiveUI.Binding.Observables.RxBindingExtensions.Merge(");
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            _ = sb.Append("                __switched").Append(i);
            if (i < inv.PropertyPaths.Length - 1)
            {
                _ = sb.AppendLine(",");
            }
        }

        _ = sb.Append(");");
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
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            var innerType = inv.InnerObservableTypeFullNames[i];
            var rawVar = ObsPropertyVarName + i;
            var switchedVar = $"__switched{i}";

            if (path.Length > 1)
            {
                ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, false, rawVar);
            }
            else
            {
                ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, false, rawVar);
            }

            _ = sb.AppendLine()
                .AppendLine()
                .AppendLine($"""
                                         var {switchedVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                             global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({rawVar},
                                                 __obs => __obs ?? (global::System.IObservable<{innerType}>)global::ReactiveUI.Binding.Observables.EmptyObservable<{innerType}>.Instance));
                             """)
                .AppendLine();
        }

        _ = sb.AppendLine("            return global::ReactiveUI.Binding.Observables.CombineLatestObservable.Create(");
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            _ = sb.Append("                __switched").Append(i).AppendLine(",");
        }

        _ = sb.Append("                selector);");
    }

    /// <summary>Gets the Func type signature for a WhenAnyObservable selector parameter.</summary>
    /// <param name="inv">The invocation info.</param>
    /// <returns>A fully qualified Func type string.</returns>
    internal static string GetSelectorType(WhenAnyObservableInvocationInfo inv)
    {
        var sb = new PooledStringBuilder().Append("global::System.Func<");
        for (var i = 0; i < inv.InnerObservableTypeFullNames.Length; i++)
        {
            _ = sb.Append(inv.InnerObservableTypeFullNames[i]).Append(", ");
        }

        _ = sb.Append(inv.ReturnTypeFullName).Append('>');
        return sb.ToStringAndReturn();
    }

    /// <summary>Groups WhenAnyObservable invocations by their type signature for overload generation.</summary>
    /// <param name="invocations">All detected invocations.</param>
    /// <returns>A list of type groups.</returns>
    internal static List<TypeGroup> GroupByTypeSignature(ImmutableArray<WhenAnyObservableInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<WhenAnyObservableInvocationInfo>>(invocations.Length);
        var keySb = new PooledStringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            _ = keySb.Clear()
                .Append(inv.SourceTypeFullName).Append('|')
                .Append(inv.ReturnTypeFullName).Append('|')
                .Append(inv.PropertyPaths.Length).Append('|')
                .Append(inv.HasSelector);

            for (var p = 0; p < inv.InnerObservableTypeFullNames.Length; p++)
            {
                _ = keySb.Append('|').Append(inv.InnerObservableTypeFullNames[p]);
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

    /// <summary>Emits normalization that strips the <c>static</c> prefix from CallerArgumentExpression values.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="propCount">The number of observable expressions.</param>
    private static void EmitStaticPrefixNormalization(StringBuilder sb, bool supportsCallerArgExpr, int propCount)
    {
        if (!supportsCallerArgExpr)
        {
            return;
        }

        for (var i = 0; i < propCount; i++)
        {
            var paramName = $"obs{i + 1}Expression";
            _ = sb.AppendLine(
                $"""            {paramName} = {paramName}.StartsWith("static ") ? {paramName}.Substring(7) : {paramName};""");
        }

        _ = sb.AppendLine();
    }

    /// <summary>Emits the if/else-if dispatch table that routes each matched invocation to its generated method.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="propCount">The number of observable expressions.</param>
    /// <param name="hasSelector">Whether a selector function is present.</param>
    private static void EmitDispatchTable(
        StringBuilder sb,
        TypeGroup group,
        bool supportsCallerArgExpr,
        int propCount,
        bool hasSelector)
    {
        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);

            if (supportsCallerArgExpr)
            {
                EmitCallerArgExprCondition(sb, inv, condition, propCount);
            }
            else
            {
                var suffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
                _ = sb
                    .Append($"""            {condition} (callerLineNumber == {inv.CallerLineNumber} && callerFilePath.EndsWith("{CodeGeneratorHelpers.EscapeString(suffix)}",""")
                    .AppendLine(" global::System.StringComparison.OrdinalIgnoreCase))");
            }

            _ = sb.AppendLine("            {");
            var selectorArg = hasSelector ? ", selector" : string.Empty;
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.SourceTypeFullName,
                inv.CallerFilePath,
                inv.CallerLineNumber,
                string.Join("|", inv.ExpressionTexts));
            _ = sb.AppendLine($"                return __WhenAnyObservable_{methodSuffix}(objectToMonitor{selectorArg});")
                .AppendLine("            }");
        }
    }

    /// <summary>Emits the CallerArgumentExpression match condition for a single invocation in the dispatch table.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="condition">The conditional keyword (<c>"if"</c> or <c>"else if"</c>).</param>
    /// <param name="propCount">The number of observable expressions.</param>
    private static void EmitCallerArgExprCondition(
        StringBuilder sb,
        WhenAnyObservableInvocationInfo inv,
        string condition,
        int propCount)
    {
        _ = sb.Append($"            {condition} (");
        for (var p = 0; p < propCount; p++)
        {
            _ = sb.Append(
                $"obs{p + 1}Expression == \"{CodeGeneratorHelpers.EscapeString(inv.ExpressionTexts[p])}\"");
            if (p < propCount - 1)
            {
                _ = sb.Append(" && ");
            }
        }

        _ = sb.AppendLine(")");
    }

    /// <summary>Groups invocations by source type and observable type signature for overload generation.</summary>
    /// <param name="First">The first invocation in the group, used for type information.</param>
    /// <param name="Invocations">All invocations sharing the same type signature.</param>
    internal sealed record TypeGroup(
        WhenAnyObservableInvocationInfo First,
        WhenAnyObservableInvocationInfo[] Invocations);
}

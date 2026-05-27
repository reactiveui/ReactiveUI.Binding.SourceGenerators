// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

using static ReactiveUI.Binding.SourceGenerators.CodeGeneration.GeneratedTypeNames;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads and binding methods for <c>BindTo</c> invocations.
/// The source is an observable stream applied to a target property; differing source/target types are
/// coerced at runtime via <c>RuntimeBindingConverter</c> (matching ReactiveUI's converter registry behavior).
/// </summary>
internal static class BindToCodeGenerator
{
    /// <summary>
    /// Generates concrete typed overloads and binding methods for <c>BindTo</c> invocations.
    /// </summary>
    /// <param name="invocations">All detected <c>BindTo</c> invocations.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    internal static string? Generate(
        ImmutableArray<BindToInvocationInfo> invocations,
        LanguageFeatures features)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return null;
        }

        var sb = new StringBuilder(invocations.Length * 1_024);
        var supportsCallerArgExpr = features.SupportsCallerArgExpr;
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb, features);
        sb.AppendLine();

        var groups = GroupByTypeSignature(invocations);

        for (var g = 0; g < groups.Count; g++)
        {
            var group = groups[g];

            GenerateConcreteOverload(sb, group, supportsCallerArgExpr, features.SupportsNullable);
            sb.AppendLine();

            for (var i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                    inv.TargetTypeFullName,
                    inv.CallerFilePath,
                    inv.CallerLineNumber,
                    inv.TargetExpressionText);
                GenerateBindToMethod(sb, inv, suffix);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Groups <c>BindTo</c> invocations by source value type, target type, target property type, and
    /// overload shape (conversion hint / converter override), so each group produces one concrete overload.
    /// </summary>
    /// <param name="invocations">The collection of <c>BindTo</c> invocation details to be grouped.</param>
    /// <returns>A list of grouped invocations, where each group shares the same overload signature.</returns>
    internal static List<BindToTypeGroup> GroupByTypeSignature(ImmutableArray<BindToInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<BindToInvocationInfo>>(invocations.Length);
        var keySb = new StringBuilder(128);

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            keySb.Clear()
                .Append(inv.SourceValueTypeFullName).Append('|')
                .Append(inv.TargetTypeFullName).Append('|')
                .Append(inv.TargetPropertyTypeFullName).Append('|')
                .Append(inv.HasConversionHint).Append('|')
                .Append(inv.HasConverterOverride);

            var key = keySb.ToString();

            if (!groupMap.TryGetValue(key, out var list))
            {
                list = [];
                groupMap[key] = list;
            }

            list.Add(inv);
        }

        var result = new List<BindToTypeGroup>();
        foreach (var kvp in groupMap)
        {
            var first = kvp.Value[0];
            result.Add(new(
                first.SourceValueTypeFullName,
                first.TargetTypeFullName,
                first.TargetPropertyTypeFullName,
                first.HasConversionHint,
                first.HasConverterOverride,
                [.. kvp.Value]));
        }

        return result;
    }

    /// <summary>
    /// Generates the concrete typed <c>BindTo</c> overload for a group, choosing the dispatch strategy
    /// based on whether CallerArgumentExpression is available.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of invocations sharing one overload signature.</param>
    /// <param name="supportsCallerArgExpr">Whether CallerArgumentExpression is supported.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void GenerateConcreteOverload(StringBuilder sb, BindToTypeGroup group, bool supportsCallerArgExpr, bool supportsNullable)
    {
        if (supportsCallerArgExpr)
        {
            GenerateCallerArgExprOverload(sb, group, supportsNullable);
        }
        else
        {
            GenerateCallerFilePathOverload(sb, group, supportsNullable);
        }
    }

    /// <summary>
    /// Generates a concrete <c>BindTo</c> overload that dispatches using CallerArgumentExpression.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of invocations sharing one overload signature.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void GenerateCallerArgExprOverload(StringBuilder sb, BindToTypeGroup group, bool supportsNullable)
    {
        var targetPropType = CodeGeneratorHelpers.NullableSelectorLeafType(group.Invocations[0].TargetPropertyPath, supportsNullable);
        sb.AppendLine($"""
                               /// <summary>
                               /// Concrete typed overload for BindTo of {IObservable}&lt;{group.SourceValueTypeFullName}&gt; to {group.TargetTypeFullName}.
                               /// Uses CallerArgumentExpression for dispatch.
                               /// </summary>
                               public static {GeneratedTypeNames.IDisposable} BindTo(
                                   this {ObservableOf(group.SourceValueTypeFullName)} source,
                                   {group.TargetTypeFullName} target,
                                   {PropertyExpression(group.TargetTypeFullName, targetPropType)} property,
                       """);

        AppendExtraParameters(sb, group);

        sb.AppendLine($$"""
                                  [{{CallerArgumentExpression}}("property")] string propertyExpression = "",
                                  [{{CallerFilePath}}] string callerFilePath = "",
                                  [{{CallerLineNumber}}] int callerLineNumber = 0)
                              {
                                  propertyExpression = propertyExpression.StartsWith("static ") ? propertyExpression.Substring(7) : propertyExpression;

                      """);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.TargetTypeFullName,
                inv.CallerFilePath,
                inv.CallerLineNumber,
                inv.TargetExpressionText);
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);
            var escapedTargetExpr = CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText);

            sb.AppendLine($$"""
                                        {{condition}} (propertyExpression == "{{escapedTargetExpr}}")
                                        {
                                            return __BindTo_{{methodSuffix}}(source, target{{FormatExtraArgs(group)}});
                                        }
                            """);
        }

        sb.AppendLine($$"""
                                  throw new {{GeneratedTypeNames.InvalidOperationException}}(
                                      "{{NoBindingFoundMessage}}");
                              }
                      """);
    }

    /// <summary>
    /// Generates a concrete <c>BindTo</c> overload that dispatches using CallerFilePath + CallerLineNumber.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of invocations sharing one overload signature.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void GenerateCallerFilePathOverload(StringBuilder sb, BindToTypeGroup group, bool supportsNullable)
    {
        var targetPropType = CodeGeneratorHelpers.NullableSelectorLeafType(group.Invocations[0].TargetPropertyPath, supportsNullable);
        sb.AppendLine($"""
                               /// <summary>
                               /// Concrete typed overload for BindTo of {IObservable}&lt;{group.SourceValueTypeFullName}&gt; to {group.TargetTypeFullName}.
                               /// Uses CallerFilePath + CallerLineNumber for dispatch.
                               /// </summary>
                               public static {GeneratedTypeNames.IDisposable} BindTo(
                                   this {ObservableOf(group.SourceValueTypeFullName)} source,
                                   {group.TargetTypeFullName} target,
                                   {PropertyExpression(group.TargetTypeFullName, targetPropType)} property,
                       """);

        AppendExtraParameters(sb, group);

        sb.AppendLine($$"""
                                  [{{CallerFilePath}}] string callerFilePath = "",
                                  [{{CallerLineNumber}}] int callerLineNumber = 0)
                              {
                      """);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.TargetTypeFullName,
                inv.CallerFilePath,
                inv.CallerLineNumber,
                inv.TargetExpressionText);
            var pathSuffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);

            sb.AppendLine($$"""
                                        {{condition}} (callerLineNumber == {{inv.CallerLineNumber}}
                                            && callerFilePath.EndsWith("{{CodeGeneratorHelpers.EscapeString(pathSuffix)}}", {{OrdinalIgnoreCase}}))
                                        {
                                            return __BindTo_{{methodSuffix}}(source, target{{FormatExtraArgs(group)}});
                                        }
                            """);
        }

        sb.AppendLine($$"""
                                  throw new {{GeneratedTypeNames.InvalidOperationException}}(
                                      "{{NoBindingFoundMessage}}");
                              }
                      """);
    }

    /// <summary>
    /// Generates the private worker method that subscribes to the source observable and assigns each
    /// value to the target property, coercing types when required.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="suffix">The stable method-name suffix.</param>
    internal static void GenerateBindToMethod(StringBuilder sb, BindToInvocationInfo inv, string suffix)
    {
        var targetAccess = CodeGeneratorHelpers.BuildPropertySetterChain("target", inv.TargetPropertyPath);
        var targetPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.TargetPropertyPath);
        var extraParams = FormatExtraMethodParams(inv);

        // Direct assignment is only safe when the value type matches the property type and the caller did
        // not supply an explicit converter. A conversion hint alone is meaningless for identity assignment.
        var directAssign = inv.SourceValueTypeFullName == inv.TargetPropertyTypeFullName && !inv.HasConverterOverride;

        sb.AppendLine($$"""
        private static {{GeneratedTypeNames.IDisposable}} __BindTo_{{suffix}}({{ObservableOf(inv.SourceValueTypeFullName)}} source, {{inv.TargetTypeFullName}} target{{extraParams}})
        {
            // BindTo: observable -> {{targetPathComment}}
""");

        if (directAssign)
        {
            sb.AppendLine($$"""
                                        return {{RxBindingExtensions}}.Subscribe(source, value =>
                                        {
                                            {{targetAccess}} = value;
                                        });
                                    }
                            """)
                .AppendLine();
        }
        else
        {
            var hintArg = inv.HasConversionHint ? "conversionHint" : "null";
            var converterArg = inv.HasConverterOverride ? "converterOverride" : "null";

            sb.AppendLine($$"""
            return {{RxBindingExtensions}}.Subscribe(source, value =>
            {
                if ({{RuntimeBindingConverter}}.TryConvert<{{inv.SourceValueTypeFullName}}, {{inv.TargetPropertyTypeFullName}}>(value, {{hintArg}}, {{converterArg}}, out var __converted))
                {
                    {{targetAccess}} = __converted;
                }
            });
        }
""")
                .AppendLine();
        }
    }

    /// <summary>
    /// Appends the conversion-hint and converter-override parameters to the concrete overload signature.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    internal static void AppendExtraParameters(StringBuilder sb, BindToTypeGroup group)
    {
        if (group.HasConversionHint)
        {
            sb.AppendLine("            object conversionHint,");
        }

        if (!group.HasConverterOverride)
        {
            return;
        }

        sb.AppendLine($"            {IBindingTypeConverter} converterOverride,");
    }

    /// <summary>
    /// Formats the extra arguments forwarded from the concrete overload to the worker method.
    /// </summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>An argument list fragment like ", conversionHint, converterOverride", or empty.</returns>
    internal static string FormatExtraArgs(BindToTypeGroup group)
    {
        var sb = new StringBuilder();
        if (group.HasConversionHint)
        {
            sb.Append(", conversionHint");
        }

        if (group.HasConverterOverride)
        {
            sb.Append(", converterOverride");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats the extra parameters for the private worker method signature.
    /// </summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>A parameter list fragment like ", object conversionHint, ... converterOverride", or empty.</returns>
    internal static string FormatExtraMethodParams(BindToInvocationInfo inv)
    {
        var sb = new StringBuilder();
        if (inv.HasConversionHint)
        {
            sb.Append(", object conversionHint");
        }

        if (inv.HasConverterOverride)
        {
            sb.Append($", {IBindingTypeConverter} converterOverride");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Groups <c>BindTo</c> invocations sharing source value type, target type, target property type,
    /// and overload shape.
    /// </summary>
    /// <param name="SourceValueTypeFullName">The fully qualified observable value type.</param>
    /// <param name="TargetTypeFullName">The fully qualified target object type.</param>
    /// <param name="TargetPropertyTypeFullName">The fully qualified target property type.</param>
    /// <param name="HasConversionHint">Whether this group's overload takes a conversion hint.</param>
    /// <param name="HasConverterOverride">Whether this group's overload takes an explicit converter.</param>
    /// <param name="Invocations">All invocations sharing this overload signature.</param>
    internal sealed record BindToTypeGroup(
        string SourceValueTypeFullName,
        string TargetTypeFullName,
        string TargetPropertyTypeFullName,
        bool HasConversionHint,
        bool HasConverterOverride,
        BindToInvocationInfo[] Invocations);
}

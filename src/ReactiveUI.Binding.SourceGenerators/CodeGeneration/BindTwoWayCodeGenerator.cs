// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;
using static ReactiveUI.Binding.SourceGenerators.CodeGeneration.GeneratedTypeNames;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads and binding methods for BindTwoWay invocations.
/// Supports basic bindings, inline Func converters (source-to-target and target-to-source), and scheduler overloads.
/// </summary>
internal static class BindTwoWayCodeGenerator
{
    /// <summary>What this API calls the source-to-target converter in its generated signatures.</summary>
    private const string ForwardConverterName = "sourceToTargetConv";

    /// <summary>What this API calls the target-to-source converter in its generated signatures.</summary>
    private const string ReverseConverterName = "targetToSourceConv";

    /// <summary>Name of the generated local holding the source side observable.</summary>
    private const string SourceObservableName = "sourceObs";

    /// <summary>Name of the generated local holding the target side observable.</summary>
    private const string TargetObservableName = "targetObs";

    /// <summary>Generates concrete typed overloads and binding methods for BindTwoWay invocations.</summary>
    /// <param name="invocations">All detected BindTwoWay invocations.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    internal static string? Generate(
        ImmutableArray<BindingInvocationInfo> invocations,
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

        // Group invocations by (SourceType, TargetType, SourcePropertyType, TargetPropertyType, HasConversion, HasScheduler)
        var groups = GroupByTypeSignature(invocations);

        for (var g = 0; g < groups.Count; g++)
        {
            var group = groups[g];

            // Generate the concrete typed extension method overload
            GenerateConcreteOverload(sb, group, supportsCallerArgExpr, features.SupportsNullable, features.StubHasExpressionParameters);
            _ = sb.AppendLine();

            // Generate binding methods
            for (var i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var sourceClassInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.SourceTypeFullName);
                var targetClassInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.TargetTypeFullName);
                var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                    inv.SourceTypeFullName,
                    inv.CallerFilePath,
                    inv.CallerLineNumber,
                    $"{inv.SourceExpressionText}|{inv.TargetExpressionText}");
                GenerateBindTwoWayMethod(sb, inv, sourceClassInfo, targetClassInfo, suffix);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        _ = sb.AppendLine();

        return PooledBuilder.ToStringAndReturn(sb);
    }

    /// <summary>Groups BindTwoWay invocations by their type signature for overload generation.</summary>
    /// <param name="invocations">The BindTwoWay invocations to group.</param>
    /// <returns>A list of grouped invocations sharing the same type signature.</returns>
    internal static List<BindingTypeGroup> GroupByTypeSignature(ImmutableArray<BindingInvocationInfo> invocations) =>
        BindingEmitterHelpers.GroupByTypeSignature(invocations);

    /// <summary>Generates the concrete typed overload using the appropriate dispatch strategy.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="supportsCallerArgExpr">Whether CallerArgumentExpression is available.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateConcreteOverload(
        StringBuilder sb,
        BindingTypeGroup group,
        bool supportsCallerArgExpr,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        if (supportsCallerArgExpr)
        {
            GenerateCallerArgExprOverload(sb, group, supportsNullable);
        }
        else
        {
            GenerateCallerFilePathOverload(sb, group, supportsNullable, stubHasExpressionParameters);
        }
    }

    /// <summary>Generates the CallerArgumentExpression-based overload for BindTwoWay dispatch.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void GenerateCallerArgExprOverload(
        StringBuilder sb,
        BindingTypeGroup group,
        bool supportsNullable)
    {
        var sourcePropType = CodeGeneratorHelpers.NullableSelectorLeafType(group.Invocations[0].SourcePropertyPath, supportsNullable);
        var targetPropType = CodeGeneratorHelpers.NullableSelectorLeafType(group.Invocations[0].TargetPropertyPath, supportsNullable);
        _ = sb.AppendLine($"""
                               /// <summary>
                               /// Concrete typed overload for BindTwoWay from {group.SourceTypeFullName} to {group.TargetTypeFullName}.
                               /// Uses CallerArgumentExpression for dispatch.
                               /// </summary>
                               public static global::System.IDisposable BindTwoWay(
                                   this {group.SourceTypeFullName} source,
                                   {group.TargetTypeFullName} target,
                                   global::System.Linq.Expressions.Expression<global::System.Func<{group.SourceTypeFullName}, {sourcePropType}>> sourceProperty,
                                   global::System.Linq.Expressions.Expression<global::System.Func<{group.TargetTypeFullName}, {targetPropType}>> targetProperty,
                       """);

        AppendExtraParameters(sb, group);

        _ = sb.AppendLine("""
                                  [global::System.Runtime.CompilerServices.CallerArgumentExpression("sourceProperty")] string sourcePropertyExpression = "",
                                  [global::System.Runtime.CompilerServices.CallerArgumentExpression("targetProperty")] string targetPropertyExpression = "",
                                  [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                                  [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                              {
                                  sourcePropertyExpression = sourcePropertyExpression.StartsWith("static ") ? sourcePropertyExpression.Substring(7) : sourcePropertyExpression;
                                  targetPropertyExpression = targetPropertyExpression.StartsWith("static ") ? targetPropertyExpression.Substring(7) : targetPropertyExpression;

                      """);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);
            var escapedSourceExpr = CodeGeneratorHelpers.EscapeString(inv.SourceExpressionText);
            var escapedTargetExpr = CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText);
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.SourceTypeFullName,
                inv.CallerFilePath,
                inv.CallerLineNumber,
                $"{inv.SourceExpressionText}|{inv.TargetExpressionText}");

            _ = sb.AppendLine($$"""
                                        {{condition}} (sourcePropertyExpression == "{{escapedSourceExpr}}"
                                            && targetPropertyExpression == "{{escapedTargetExpr}}")
                                        {
                                            return __BindTwoWay_{{methodSuffix}}(source, target{{FormatExtraArgs(group)}});
                                        }
                            """);
        }

        _ = sb.AppendLine("""
                                  throw new global::System.InvalidOperationException(
                                      "No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.");
                              }
                      """);
    }

    /// <summary>Generates the CallerFilePath-based overload for BindTwoWay dispatch.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateCallerFilePathOverload(
        StringBuilder sb,
        BindingTypeGroup group,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        var sourcePropType = CodeGeneratorHelpers.NullableSelectorLeafType(group.Invocations[0].SourcePropertyPath, supportsNullable);
        var targetPropType = CodeGeneratorHelpers.NullableSelectorLeafType(group.Invocations[0].TargetPropertyPath, supportsNullable);
        _ = sb.AppendLine($"""
                               /// <summary>
                               /// Concrete typed overload for BindTwoWay from {group.SourceTypeFullName} to {group.TargetTypeFullName}.
                               /// Uses CallerFilePath + CallerLineNumber for dispatch.
                               /// </summary>
                               public static global::System.IDisposable BindTwoWay(
                                   this {group.SourceTypeFullName} source,
                                   {group.TargetTypeFullName} target,
                                   global::System.Linq.Expressions.Expression<global::System.Func<{group.SourceTypeFullName}, {sourcePropType}>> sourceProperty,
                                   global::System.Linq.Expressions.Expression<global::System.Func<{group.TargetTypeFullName}, {targetPropType}>> targetProperty,
                       """);

        AppendExtraParameters(sb, group);

        if (stubHasExpressionParameters)
        {
            CodeGeneratorHelpers.AppendExpressionParameter(sb, "sourceProperty", "sourcePropertyExpression", false);
            CodeGeneratorHelpers.AppendExpressionParameter(sb, "targetProperty", "targetPropertyExpression", false);
        }

        _ = sb.AppendLine("""
                                  [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                                  [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                              {
                      """);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var pathSuffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.SourceTypeFullName,
                inv.CallerFilePath,
                inv.CallerLineNumber,
                $"{inv.SourceExpressionText}|{inv.TargetExpressionText}");

            _ = sb.AppendLine($$"""
                                        {{condition}} (callerLineNumber == {{inv.CallerLineNumber}}
                                            && callerFilePath.EndsWith("{{CodeGeneratorHelpers.EscapeString(pathSuffix)}}", global::System.StringComparison.OrdinalIgnoreCase))
                                        {
                                            return __BindTwoWay_{{methodSuffix}}(source, target{{FormatExtraArgs(group)}});
                                        }
                            """);
        }

        _ = sb.AppendLine("""
                                  throw new global::System.InvalidOperationException(
                                      "No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.");
                              }
                      """);
    }

    /// <summary>Generates a private BindTwoWay method for a specific invocation.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="sourceClassInfo">The source type class binding info.</param>
    /// <param name="targetClassInfo">The target type class binding info.</param>
    /// <param name="suffix">The stable method name suffix.</param>
    internal static void GenerateBindTwoWayMethod(
        StringBuilder sb,
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        ClassBindingInfo? targetClassInfo,
        string suffix)
    {
        var targetAccess = CodeGeneratorHelpers.BuildPropertySetterChain("target", inv.TargetPropertyPath);
        var sourceSetAccess = CodeGeneratorHelpers.BuildPropertySetterChain("source", inv.SourcePropertyPath);
        var sourcePathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.SourcePropertyPath);
        var targetPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.TargetPropertyPath);

        var extraParams = FormatExtraMethodParams(inv);
        var conversionComment = inv.HasConversion ? " (with conversion)" : string.Empty;
        var schedulerComment = inv.HasScheduler ? " (with scheduler)" : string.Empty;

        _ = sb.AppendLine($$"""
                                private static global::System.IDisposable __BindTwoWay_{{suffix}}({{inv.SourceTypeFullName}} source, {{inv.TargetTypeFullName}} target{{extraParams}})
                                {
                                    // BindTwoWay: {{sourcePathComment}} <-> {{targetPathComment}}{{conversionComment}}{{schedulerComment}}
                        """);

        // Emit inline observation code instead of delegating to WhenChanged dispatch
        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            "source",
            inv.SourcePropertyPath,
            inv.SourcePropertyTypeFullName,
            sourceClassInfo,
            SourceObservableName);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            "target",
            inv.TargetPropertyPath,
            inv.TargetPropertyTypeFullName,
            targetClassInfo,
            TargetObservableName);

        if (inv.HasConversion || inv.HasScheduler)
        {
            var (sourceVar, targetVar) = EmitConversionAndSchedulerStages(sb, inv);

            EmitTwoWaySubscription(sb, sourceVar, targetVar, targetAccess, sourceSetAccess);
        }
        else
        {
            EmitTwoWaySubscription(sb, SourceObservableName, TargetObservableName, targetAccess, sourceSetAccess);
        }
    }

    /// <summary>Appends extra parameters (converters, scheduler) to the concrete overload signature.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    internal static void AppendExtraParameters(StringBuilder sb, BindingTypeGroup group) =>
        BindingEmitterHelpers.AppendTwoWayExtraParameters(sb, group, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats extra arguments (converters, scheduler) for forwarding to the binding method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>Extra arguments string like ", sourceToTargetConv, targetToSourceConv, scheduler" or empty.</returns>
    internal static string FormatExtraArgs(BindingTypeGroup group) =>
        BindingEmitterHelpers.FormatTwoWayExtraArgs(group, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats extra method parameters for the private binding method signature.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>Extra parameters string for two-way converter and scheduler parameters.</returns>
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv) =>
        BindingEmitterHelpers.FormatTwoWayExtraMethodParams(inv, ForwardConverterName, ReverseConverterName);

    /// <summary>
    /// Emits the conversion and scheduler stages that sit between the raw observations and the
    /// subscription, and reports the variable names the subscription should read from.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>The source and target observable variable names after the stages are applied.</returns>
    private static (string SourceVar, string TargetVar) EmitConversionAndSchedulerStages(
        StringBuilder sb,
        BindingInvocationInfo inv)
    {
        var sourceVar = SourceObservableName;
        var targetVar = TargetObservableName;

        if (inv.HasConversion)
        {
            var srcNext = inv.HasScheduler ? "__srcSelected" : "sourceBind";
            var tgtNext = inv.HasScheduler ? "__tgtSelected" : "targetBind";
            _ = sb.AppendLine($"""
                                   var {srcNext} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({sourceVar}, sourceToTargetConv);
                                   var {tgtNext} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({targetVar}, targetToSourceConv);
                           """);
            sourceVar = srcNext;
            targetVar = tgtNext;
        }

        if (inv.HasScheduler)
        {
            _ = sb.AppendLine($"""
                                   var sourceBind = new {ObserveOnObservable}<{inv.TargetPropertyTypeFullName}>({sourceVar}, scheduler);
                                   var targetBind = new {ObserveOnObservable}<{inv.SourcePropertyTypeFullName}>({targetVar}, scheduler);
                           """);
            sourceVar = "sourceBind";
            targetVar = "targetBind";
        }

        return (sourceVar, targetVar);
    }

    /// <summary>Emits the two-way subscription and <c>CompositeDisposable2</c> return block.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="sourceVar">The source observable variable name to subscribe to.</param>
    /// <param name="targetVar">The target observable variable name to subscribe to.</param>
    /// <param name="targetAccess">The target property setter access chain.</param>
    /// <param name="sourceSetAccess">The source property setter access chain.</param>
    private static void EmitTwoWaySubscription(
        StringBuilder sb,
        string sourceVar,
        string targetVar,
        string targetAccess,
        string sourceSetAccess) => _ = sb.AppendLine($$"""

                                    var d1 = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe({{sourceVar}}, value =>
                                    {
                                        {{targetAccess}} = value;
                                    });

                                    var __targetSkipped = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Skip({{targetVar}}, 1);
                                    var d2 = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(__targetSkipped, value =>
                                    {
                                        {{sourceSetAccess}} = value;
                                    });

                                    return new global::ReactiveUI.Binding.Observables.CompositeDisposable2(d1, d2);
                                }
                        """)
            .AppendLine();
}

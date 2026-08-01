// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;
using static ReactiveUI.Binding.SourceGenerators.CodeGeneration.GeneratedTypeNames;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads and binding methods for BindOneWay invocations.
/// Supports basic bindings, inline Func converters, and scheduler overloads.
/// </summary>
internal static class BindOneWayCodeGenerator
{
    /// <summary>What this API calls the conversion argument in its generated signatures.</summary>
    private const string ConversionParameterName = "conversionFunc";

    /// <summary>Name of the emitted local holding the source property observation, before conversion or scheduling.</summary>
    private const string SourceObservableVariable = "sourceObs";

    /// <summary>Generates concrete typed overloads and binding methods for BindOneWay invocations.</summary>
    /// <param name="invocations">All detected BindOneWay invocations.</param>
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
                var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                    inv.SourceTypeFullName,
                    inv.CallerFilePath,
                    inv.CallerLineNumber,
                    $"{inv.SourceExpressionText}|{inv.TargetExpressionText}");
                GenerateBindOneWayMethod(sb, inv, sourceClassInfo, suffix);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        _ = sb.AppendLine();

        return PooledBuilder.ToStringAndReturn(sb);
    }

    /// <summary>Groups binding invocation information by a unique type signature, producing a collection of grouped results.</summary>
    /// <param name="invocations">The collection of binding invocation details to be grouped.</param>
    /// <returns>A list of grouped binding type information, where each group shares the same type signature.</returns>
    internal static List<BindingTypeGroup> GroupByTypeSignature(ImmutableArray<BindingInvocationInfo> invocations) =>
        BindingEmitterHelpers.GroupByTypeSignature(invocations);

    /// <summary>
    /// Generates a concrete typed extension method overload for a specific group of binding types,
    /// adjusting the generated code based on whether the target language version supports CallerArgumentExpression.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance to which the generated code will be appended.</param>
    /// <param name="group">The group of binding types containing information about source and target members, conversion, and scheduling.</param>
    /// <param name="supportsCallerArgExpr">Indicates whether the CallerArgumentExpression feature is supported by the target language version.</param>
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

    /// <summary>
    /// Generates a concrete typed overload for the BindOneWay method, allowing bindings between source and target types
    /// while utilizing CallerArgumentExpression for enhanced debugging and context.
    /// </summary>
    /// <param name="sb">The StringBuilder instance used to append the generated source code.</param>
    /// <param name="group">The grouping of binding-related type and property information required to generate the overload.</param>
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
                               /// Concrete typed overload for BindOneWay from {group.SourceTypeFullName} to {group.TargetTypeFullName}.
                               /// Uses CallerArgumentExpression for dispatch.
                               /// </summary>
                               public static global::System.IDisposable BindOneWay(
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
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.SourceTypeFullName,
                inv.CallerFilePath,
                inv.CallerLineNumber,
                $"{inv.SourceExpressionText}|{inv.TargetExpressionText}");
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);
            var escapedSourceExpr = CodeGeneratorHelpers.EscapeString(inv.SourceExpressionText);
            var escapedTargetExpr = CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText);

            _ = sb.AppendLine($$"""
                                        {{condition}} (sourcePropertyExpression == "{{escapedSourceExpr}}"
                                            && targetPropertyExpression == "{{escapedTargetExpr}}")
                                        {
                                            return __BindOneWay_{{methodSuffix}}(source, target{{FormatExtraArgs(group)}});
                                        }
                            """);
        }

        _ = sb.AppendLine("""
                                  throw new global::System.InvalidOperationException(
                                      "No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.");
                              }
                      """);
    }

    /// <summary>
    /// Generates a BindOneWay overload for a specific binding type group,
    /// enabling bindings that utilize CallerFilePath and CallerLineNumber for diagnostics and dispatch.
    /// </summary>
    /// <param name="sb">The StringBuilder instance used to generate the source code.</param>
    /// <param name="group">Details of the source and target types involved in the binding, including property types and other metadata.</param>
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
                               /// Concrete typed overload for BindOneWay from {group.SourceTypeFullName} to {group.TargetTypeFullName}.
                               /// Uses CallerFilePath + CallerLineNumber for dispatch.
                               /// </summary>
                               public static global::System.IDisposable BindOneWay(
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
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.SourceTypeFullName,
                inv.CallerFilePath,
                inv.CallerLineNumber,
                $"{inv.SourceExpressionText}|{inv.TargetExpressionText}");
            var pathSuffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);

            _ = sb.AppendLine($$"""
                                        {{condition}} (callerLineNumber == {{inv.CallerLineNumber}}
                                            && callerFilePath.EndsWith("{{CodeGeneratorHelpers.EscapeString(pathSuffix)}}", global::System.StringComparison.OrdinalIgnoreCase))
                                        {
                                            return __BindOneWay_{{methodSuffix}}(source, target{{FormatExtraArgs(group)}});
                                        }
                            """);
        }

        _ = sb.AppendLine("""
                                  throw new global::System.InvalidOperationException(
                                      "No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.");
                              }
                      """);
    }

    /// <summary>
    /// Generates the BindOneWay method used for binding a source property to a target property with optional conversion and scheduler.
    /// </summary>
    /// <param name="sb">The StringBuilder for appending the generated code.</param>
    /// <param name="inv">The invocation information containing details about the binding.</param>
    /// <param name="sourceClassInfo">The class binding information of the source, or null if not applicable.</param>
    /// <param name="suffix">The suffix to append to the generated method name for uniqueness.</param>
    internal static void GenerateBindOneWayMethod(
        StringBuilder sb,
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        string suffix)
    {
        var targetAccess = CodeGeneratorHelpers.BuildPropertySetterChain("target", inv.TargetPropertyPath);
        var sourcePathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.SourcePropertyPath);
        var targetPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.TargetPropertyPath);

        var extraParams = FormatExtraMethodParams(inv);
        var conversionComment = inv.HasConversion ? " (with conversion)" : string.Empty;
        var schedulerComment = inv.HasScheduler ? " (with scheduler)" : string.Empty;

        _ = sb.AppendLine($$"""
                                private static global::System.IDisposable __BindOneWay_{{suffix}}({{inv.SourceTypeFullName}} source, {{inv.TargetTypeFullName}} target{{extraParams}})
                                {
                                    // BindOneWay: {{sourcePathComment}} -> {{targetPathComment}}{{conversionComment}}{{schedulerComment}}
                        """);

        // Emit inline observation code instead of delegating to WhenChanged dispatch
        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            "source",
            inv.SourcePropertyPath,
            inv.SourcePropertyTypeFullName,
            sourceClassInfo,
            SourceObservableVariable);

        var subscribeVar = inv.HasConversion || inv.HasScheduler
            ? EmitConversionAndSchedulerStages(sb, inv)
            : SourceObservableVariable;

        _ = sb.AppendLine($$"""

                                    return global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe({{subscribeVar}}, value =>
                                    {
                                        {{targetAccess}} = value;
                                    });
                                }
                        """)
            .AppendLine();
    }

    /// <summary>Appends extra parameters (converter, scheduler) to the concrete overload signature.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    internal static void AppendExtraParameters(StringBuilder sb, BindingTypeGroup group) =>
        BindingEmitterHelpers.AppendExtraParameters(sb, group, ConversionParameterName);

    /// <summary>Formats extra arguments (converter, scheduler) for forwarding to the binding method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>Extra arguments string like ", conversionFunc, scheduler" or empty.</returns>
    internal static string FormatExtraArgs(BindingTypeGroup group) =>
        BindingEmitterHelpers.FormatExtraArgs(group, ConversionParameterName);

    /// <summary>Formats extra method parameters for the private binding method signature.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>Extra parameters string like ", Func&lt;int, string&gt; conversionFunc, ISequencer scheduler" or empty.</returns>
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv) =>
        BindingEmitterHelpers.FormatExtraMethodParams(inv, ConversionParameterName);

    /// <summary>Emits the conversion and scheduler stages between the source observation and the subscription.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>The variable name the subscription should read from.</returns>
    private static string EmitConversionAndSchedulerStages(StringBuilder sb, BindingInvocationInfo inv)
    {
        var currentVar = SourceObservableVariable;

        if (inv.HasConversion)
        {
            var nextVar = inv.HasScheduler ? "__selected" : "bindObs";
            _ = sb.AppendLine(
                $"        var {nextVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({currentVar}, conversionFunc);");
            currentVar = nextVar;
        }

        if (inv.HasScheduler)
        {
            _ = sb.AppendLine(
                $"        var bindObs = new {ObserveOnObservable}<{inv.TargetPropertyTypeFullName}>({currentVar}, scheduler);");
            currentVar = "bindObs";
        }

        return currentVar;
    }
}

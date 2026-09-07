// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
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

    /// <summary>The indentation a statement inside the emitted subscription body sits at.</summary>
    private const string SubscriptionBodyIndent = "                ";

    /// <summary>Groups binding invocation information by a unique type signature, producing a collection of grouped results.</summary>
    /// <param name="invocations">The collection of binding invocation details to be grouped.</param>
    /// <returns>A list of grouped binding type information, where each group shares the same type signature.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        _ = sb.AppendLine("        /// <summary>").Append("        /// Concrete typed overload for BindOneWay from ").Append(group.SourceTypeFullName)
            .Append(" to ").Append(group.TargetTypeFullName).AppendLine(".").AppendLine("        /// Uses CallerArgumentExpression for dispatch.")
            .AppendLine("        /// </summary>").AppendLine("        public static global::System.IDisposable BindOneWay(").Append("            this ")
            .Append(group.SourceTypeFullName).AppendLine(" source,").Append("            ").Append(group.TargetTypeFullName).AppendLine(" target,")
            .Append(GeneratedSyntax.SelectorParameterOpen).Append(group.SourceTypeFullName).Append(", ")
            .Append(sourcePropType).AppendLine(">> sourceProperty,").Append(GeneratedSyntax.SelectorParameterOpen)
            .Append(group.TargetTypeFullName).Append(", ").Append(targetPropType).AppendLine(">> targetProperty,");

        AppendExtraParameters(sb, group);

        _ = sb.AppendLine("""
                                  [global::System.Runtime.CompilerServices.CallerArgumentExpression("sourceProperty")] string sourcePropertyExpression = "",
                                  [global::System.Runtime.CompilerServices.CallerArgumentExpression("targetProperty")] string targetPropertyExpression = "",
                                  [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                                  [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                              {
                                  sourcePropertyExpression = sourcePropertyExpression.StartsWith("static ", global::System.StringComparison.Ordinal)
                                      ? sourcePropertyExpression.Substring(7)
                                      : sourcePropertyExpression;
                                  targetPropertyExpression = targetPropertyExpression.StartsWith("static ", global::System.StringComparison.Ordinal)
                                      ? targetPropertyExpression.Substring(7)
                                      : targetPropertyExpression;

                      """);

        EmitAffinityOverride(sb, group, "targetPropertyExpression");

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

            _ = sb.Append("            ").Append(condition).Append(" (sourcePropertyExpression == \"").Append(escapedSourceExpr).AppendLine("\"")
                .Append("                && targetPropertyExpression == \"").Append(escapedTargetExpr).AppendLine("\")").AppendLine(GeneratedSyntax.StatementBlockOpen)
                .Append("                return __BindOneWay_").Append(methodSuffix).Append("(source, target").Append(FormatExtraArgs(group))
                .AppendLine(");").AppendLine("            }");
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
        _ = sb.AppendLine("        /// <summary>").Append("        /// Concrete typed overload for BindOneWay from ").Append(group.SourceTypeFullName)
            .Append(" to ").Append(group.TargetTypeFullName).AppendLine(".")
            .AppendLine("        /// Uses CallerFilePath + CallerLineNumber for dispatch.").AppendLine("        /// </summary>")
            .AppendLine("        public static global::System.IDisposable BindOneWay(").Append("            this ").Append(group.SourceTypeFullName)
            .AppendLine(" source,").Append("            ").Append(group.TargetTypeFullName).AppendLine(" target,")
            .Append(GeneratedSyntax.SelectorParameterOpen).Append(group.SourceTypeFullName).Append(", ")
            .Append(sourcePropType).AppendLine(">> sourceProperty,").Append(GeneratedSyntax.SelectorParameterOpen)
            .Append(group.TargetTypeFullName).Append(", ").Append(targetPropType).AppendLine(">> targetProperty,");

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

        EmitAffinityOverride(
            sb,
            group,
            $"\"{CodeGeneratorHelpers.EscapeString(group.Invocations[0].TargetExpressionText)}\"");

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

            _ = sb.Append("            ").Append(condition).Append(" (callerLineNumber == ").Append(inv.CallerLineNumber).AppendLine()
                .Append("                && callerFilePath.EndsWith(\"").Append(CodeGeneratorHelpers.EscapeString(pathSuffix))
                .AppendLine("\", global::System.StringComparison.OrdinalIgnoreCase))").AppendLine(GeneratedSyntax.StatementBlockOpen)
                .Append("                return __BindOneWay_").Append(methodSuffix).Append("(source, target").Append(FormatExtraArgs(group))
                .AppendLine(");").AppendLine("            }");
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
        var targetAssignment = CodeGeneratorHelpers.BuildGuardedAssignment(
            "target",
            inv.TargetPropertyPath,
            "value",
            SubscriptionBodyIndent);
        var sourcePathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.SourcePropertyPath);
        var targetPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.TargetPropertyPath);

        var extraParams = FormatExtraMethodParams(inv);
        var conversionComment = inv.HasConversion ? " (with conversion)" : string.Empty;
        var schedulerComment = inv.HasScheduler ? " (with scheduler)" : string.Empty;

        _ = sb.Append("        private static global::System.IDisposable __BindOneWay_").Append(suffix).Append('(').Append(inv.SourceTypeFullName)
            .Append(" source, ").Append(inv.TargetTypeFullName).Append(" target").Append(extraParams).AppendLine(")").AppendLine("        {")
            .Append("            // BindOneWay: ").Append(sourcePathComment).Append(" -> ").Append(targetPathComment).Append(conversionComment)
            .Append(schedulerComment).AppendLine();

        BindingEmitterHelpers.EmitBindingHookGuard(sb, "source", "target", "OneWay", "global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance");

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

        subscribeVar = BindingEmitterHelpers.EmitViewThreadStage(sb, inv, subscribeVar, "targetThreadObs");

        _ = sb.AppendLine().Append("            return ").Append(BindingErrors).Append(".Subscribe(").Append(subscribeVar).AppendLine(", value =>")
            .AppendLine(GeneratedSyntax.StatementBlockOpen).Append("                ").Append(targetAssignment).AppendLine().Append("            }, \"")
            .Append(CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText)).AppendLine("\");").AppendLine("        }").AppendLine();
    }

    /// <summary>Appends extra parameters (converter, scheduler) to the concrete overload signature.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendExtraParameters(StringBuilder sb, BindingTypeGroup group) =>
        BindingEmitterHelpers.AppendExtraParameters(sb, group, ConversionParameterName);

    /// <summary>Formats extra arguments (converter, scheduler) for forwarding to the binding method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>Extra arguments string like ", conversionFunc, scheduler" or empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraArgs(BindingTypeGroup group) =>
        BindingEmitterHelpers.FormatExtraArgs(group, ConversionParameterName);

    /// <summary>Formats extra method parameters for the private binding method signature.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>Extra parameters string like ", Func&lt;int, string&gt; conversionFunc, ISequencer scheduler" or empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv) =>
        BindingEmitterHelpers.FormatExtraMethodParams(inv, ConversionParameterName);

    /// <summary>Emits the check that hands the binding to the runtime engine when a registered plugin outranks the generated one.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group, which fixes the observed type for the whole overload.</param>
    /// <param name="bindingExpression">The C# expression naming the bound target, used when a write faults.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EmitAffinityOverride(StringBuilder sb, BindingTypeGroup group, string bindingExpression) =>
        BindingEmitterHelpers.EmitAffinityOverride(
            sb,
            group,
            "BindOneWay",
            "source, target, sourceProperty, targetProperty, "
            + (group.HasConversion ? $"{ConversionParameterName}, " : string.Empty)
            + (group.HasScheduler ? "scheduler" : "null")
            + $", {bindingExpression}",
            false);

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
            _ = sb.Append("        var ").Append(nextVar).Append(" = new ").Append(MapSignal).Append('<').Append(inv.SourcePropertyTypeFullName)
                .Append(", ").Append(inv.TargetPropertyTypeFullName).Append(">(").Append(currentVar).AppendLine(", conversionFunc);");
            currentVar = nextVar;
        }

        if (inv.HasScheduler)
        {
            _ = sb.Append("        var bindObs = ").Append(LinqExtensions).Append(".ObserveOn<").Append(inv.TargetPropertyTypeFullName).Append(">(")
                .Append(currentVar).AppendLine(", scheduler);");
            currentVar = "bindObs";
        }

        return currentVar;
    }
}

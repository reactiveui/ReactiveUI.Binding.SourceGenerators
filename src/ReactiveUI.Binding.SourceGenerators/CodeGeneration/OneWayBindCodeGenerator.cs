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
/// Generates concrete typed extension method overloads and binding methods for OneWayBind (view-first) invocations.
/// The generated methods return <c>IReactiveBinding&lt;TView, TValue&gt;</c> and use view-first parameter ordering.
/// </summary>
internal static class OneWayBindCodeGenerator
{
    /// <summary>What this API calls the selector for the side it reads from.</summary>
    private const string SourceSelectorName = "viewModelProperty";

    /// <summary>What this API calls the selector for the side it writes to.</summary>
    private const string TargetSelectorName = "viewProperty";

    /// <summary>The parameter carrying the text of the selector for the side read from.</summary>
    private const string SourceExpressionParameter = SourceSelectorName + CodeGeneratorHelpers.ExpressionParameterSuffix;

    /// <summary>The parameter carrying the text of the selector for the side written to.</summary>
    private const string TargetExpressionParameter = TargetSelectorName + CodeGeneratorHelpers.ExpressionParameterSuffix;

    /// <summary>The generated worker each dispatch branch hands the binding to.</summary>
    private const string WorkerMethodPrefix = "__OneWayBind_";

    /// <summary>The two objects a generated worker binds, in its own parameter order.</summary>
    private const string WorkerArguments = "viewModel, view";

    /// <summary>The indentation a statement inside the emitted subscription body sits at.</summary>
    private const string SubscriptionBodyIndent = "                    ";

    /// <summary>What this API calls the projection argument in its generated signatures.</summary>
    private const string ConversionParameterName = "selector";

    /// <summary>Name of the emitted local holding the source property observation, before conversion or scheduling.</summary>
    private const string SourceObservableVariable = "sourceObs";

    /// <summary>Groups OneWayBind invocations by their type signature for overload generation.</summary>
    /// <param name="invocations">The OneWayBind invocations to group.</param>
    /// <returns>A list of grouped invocations sharing the same type signature.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    /// <summary>Generates the CallerArgumentExpression-based overload for OneWayBind dispatch.</summary>
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
        var returnType = FormatReturnType(group);

        _ = sb.AppendLine("        /// <summary>").Append("        /// Concrete typed overload for OneWayBind from ").Append(group.SourceTypeFullName)
            .Append(" to ").Append(group.TargetTypeFullName).AppendLine(".").AppendLine("        /// Uses CallerArgumentExpression for dispatch.")
            .AppendLine("        /// </summary>").Append("        public static ").Append(returnType).AppendLine(" OneWayBind(")
            .Append("            this ").Append(group.TargetTypeFullName).AppendLine(" view,").Append("            ").Append(group.SourceTypeFullName)
            .AppendLine(" viewModel,").Append(GeneratedSyntax.SelectorParameterOpen)
            .Append(group.SourceTypeFullName).Append(", ").Append(sourcePropType).AppendLine(">> viewModelProperty,")
            .Append(GeneratedSyntax.SelectorParameterOpen).Append(group.TargetTypeFullName).Append(", ")
            .Append(targetPropType).AppendLine(">> viewProperty,");

        AppendExtraParameters(sb, group);

        CodeGeneratorHelpers.AppendExpressionDispatchParameters(sb, SourceSelectorName, TargetSelectorName);

        EmitAffinityOverride(sb, group, "viewPropertyExpression");

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];

            CodeGeneratorHelpers.AppendExpressionDispatchCondition(
                sb,
                CodeGeneratorHelpers.ConditionKeyword(i),
                SourceExpressionParameter,
                inv.SourceExpressionText,
                TargetExpressionParameter,
                inv.TargetExpressionText);
            AppendDispatchReturn(sb, group, inv);
        }

        CodeGeneratorHelpers.AppendBindingDispatchFallthrough(sb);
    }

    /// <summary>Generates the CallerFilePath-based overload for OneWayBind dispatch.</summary>
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
        var returnType = FormatReturnType(group);

        _ = sb.AppendLine("        /// <summary>").Append("        /// Concrete typed overload for OneWayBind from ").Append(group.SourceTypeFullName)
            .Append(" to ").Append(group.TargetTypeFullName).AppendLine(".")
            .AppendLine("        /// Uses CallerFilePath + CallerLineNumber for dispatch.").AppendLine("        /// </summary>")
            .Append("        public static ").Append(returnType).AppendLine(" OneWayBind(").Append("            this ").Append(group.TargetTypeFullName)
            .AppendLine(" view,").Append("            ").Append(group.SourceTypeFullName).AppendLine(" viewModel,")
            .Append(GeneratedSyntax.SelectorParameterOpen).Append(group.SourceTypeFullName).Append(", ")
            .Append(sourcePropType).AppendLine(">> viewModelProperty,")
            .Append(GeneratedSyntax.SelectorParameterOpen).Append(group.TargetTypeFullName).Append(", ")
            .Append(targetPropType).AppendLine(">> viewProperty,");

        AppendExtraParameters(sb, group);

        if (stubHasExpressionParameters)
        {
            CodeGeneratorHelpers.AppendExpressionParameter(sb, SourceSelectorName, SourceExpressionParameter, false);
            CodeGeneratorHelpers.AppendExpressionParameter(sb, TargetSelectorName, TargetExpressionParameter, false);
        }

        CodeGeneratorHelpers.AppendCallerInfoDispatchParameters(sb);

        EmitAffinityOverride(
            sb,
            group,
            $"\"{CodeGeneratorHelpers.EscapeString(group.Invocations[0].TargetExpressionText)}\"");

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];

            CodeGeneratorHelpers.AppendCallerInfoDispatchCondition(
                sb,
                CodeGeneratorHelpers.ConditionKeyword(i),
                inv.CallerLineNumber,
                CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath));
            AppendDispatchReturn(sb, group, inv);
        }

        CodeGeneratorHelpers.AppendBindingDispatchFallthrough(sb);
    }

    /// <summary>Generates a private OneWayBind method for a specific invocation.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="sourceClassInfo">The source type class binding info.</param>
    /// <param name="targetClassInfo">The view type class binding info, which says whether it exposes a view model.</param>
    /// <param name="suffix">The stable method name suffix.</param>
    internal static void GenerateOneWayBindMethod(
        StringBuilder sb,
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        ClassBindingInfo? targetClassInfo,
        string suffix)
    {
        var viewAssignment = CodeGeneratorHelpers.BuildGuardedAssignment(
            "view",
            inv.TargetPropertyPath,
            "value",
            SubscriptionBodyIndent);
        var viewModelPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.SourcePropertyPath);
        var viewPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.TargetPropertyPath);

        var extraParams = FormatExtraMethodParams(inv);
        var conversionComment = inv.HasConversion ? " (with conversion)" : string.Empty;
        var schedulerComment = inv.HasScheduler ? " (with scheduler)" : string.Empty;
        var returnType = FormatMethodReturnType(inv);

        _ = sb.Append("        private static ").Append(returnType).Append(" __OneWayBind_").Append(suffix).Append('(').Append(inv.SourceTypeFullName)
            .Append(" viewModel, ").Append(inv.TargetTypeFullName).Append(" view").Append(extraParams).AppendLine(")").AppendLine("        {")
            .Append("            // OneWayBind: ").Append(viewModelPathComment).Append(" -> ").Append(viewPathComment).Append(conversionComment)
            .Append(schedulerComment).AppendLine();

        BindingEmitterHelpers.EmitBindingHookGuard(sb, "viewModel", "view", "OneWay", "null");

        // Emit inline observation code instead of delegating to WhenChanged dispatch
        var observation = BindingEmitterHelpers.ResolveViewModelObservation(inv, sourceClassInfo, targetClassInfo);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            observation.RootVariable,
            observation.Path,
            inv.SourcePropertyTypeFullName,
            observation.RootClassInfo,
            SourceObservableVariable);

        var currentVar = inv.HasConversion || inv.HasScheduler
            ? EmitConversionAndSchedulerStages(sb, inv)
            : SourceObservableVariable;

        if (BindingEmitterHelpers.RequiresRegistryConversion(inv))
        {
            currentVar = EmitRegistryConversionStage(sb, inv, currentVar);
        }

        currentVar = BindingEmitterHelpers.EmitViewThreadStage(sb, inv, currentVar, "viewThreadObs");

        _ = sb.AppendLine().Append("            var sub = ").Append(BindingErrors).Append(".Subscribe(").Append(currentVar).AppendLine(", value =>")
            .AppendLine(GeneratedSyntax.StatementBlockOpen).Append("                ").Append(viewAssignment).AppendLine().Append("            }, \"")
            .Append(CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText)).AppendLine("\");").AppendLine()
            .Append("            return new global::ReactiveUI.Binding.ReactiveBinding<").Append(inv.TargetTypeFullName).Append(", ")
            .Append(inv.TargetPropertyTypeFullName).AppendLine(">(").AppendLine("                view,").Append("                ").Append(currentVar)
            .AppendLine(",").AppendLine("                global::ReactiveUI.Binding.BindingDirection.OneWay,").AppendLine("                sub);")
            .AppendLine("        }").AppendLine();
    }

    /// <summary>Emits the stage that converts the observed values to the target property's type.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="sourceVar">The variable holding the values to convert.</param>
    /// <returns>The name of the variable holding the converted values.</returns>
    internal static string EmitRegistryConversionStage(
        StringBuilder sb,
        BindingInvocationInfo inv,
        string sourceVar)
    {
        const string convertedVar = "convertedObs";

        BindingEmitterHelpers.EmitRegistryConversion(
            sb,
            sourceVar,
            convertedVar,
            inv.SourcePropertyTypeFullName,
            inv.TargetPropertyTypeFullName);

        return convertedVar;
    }

    /// <summary>Appends extra parameters (selector, scheduler) to the concrete overload signature.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendExtraParameters(StringBuilder sb, BindingTypeGroup group) =>
        BindingEmitterHelpers.AppendExtraParameters(sb, group, ConversionParameterName);

    /// <summary>Formats extra arguments (selector, scheduler) for forwarding to the binding method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>Extra arguments string or empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraArgs(BindingTypeGroup group) =>
        BindingEmitterHelpers.FormatExtraArgs(group, ConversionParameterName);

    /// <summary>Formats extra method parameters for the private binding method signature.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>Extra parameters string for selector and scheduler parameters.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv) =>
        BindingEmitterHelpers.FormatExtraMethodParams(inv, ConversionParameterName);

    /// <summary>Formats the return type for a concrete OneWayBind overload.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>The fully qualified return type string.</returns>
    /// <remarks>
    /// Always the view property's type, which is what the runtime stub declares. Using the source type when no
    /// converter was supplied only agrees with the stub while the two sides happen to match.
    /// </remarks>
    internal static string FormatReturnType(BindingTypeGroup group) =>
        $"global::ReactiveUI.Binding.IReactiveBinding<{group.TargetTypeFullName}, {group.TargetPropertyTypeFullName}>";

    /// <summary>Formats the return type for a private OneWayBind method.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>The fully qualified return type string.</returns>
    internal static string FormatMethodReturnType(BindingInvocationInfo inv) =>
        $"global::ReactiveUI.Binding.IReactiveBinding<{inv.TargetTypeFullName}, {inv.TargetPropertyTypeFullName}>";

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
                .Append(", ").Append(inv.TargetPropertyTypeFullName).Append(">(").Append(currentVar).AppendLine(", selector);");
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

    /// <summary>Names the conversion the fallback needs, matching whatever the generated body would apply.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>The conversion argument, trailed by a comma, or empty when the two sides share a type.</returns>
    private static string FormatFallbackConversion(BindingTypeGroup group)
    {
        if (group.HasConversion)
        {
            return $"{ConversionParameterName}, ";
        }

        return BindingEmitterHelpers.RequiresRegistryConversion(group)
            ? $"{CodeGeneratorHelpers.FormatRegistryConversionLambda(group.SourcePropertyTypeFullName, group.TargetPropertyTypeFullName)}, "
            : string.Empty;
    }

    /// <summary>Emits the check that hands the binding to the runtime engine when a registered plugin outranks the generated one.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group, which fixes the observed type for the whole overload.</param>
    /// <param name="bindingExpression">The C# expression naming the bound view property, used when a write faults.</param>
    private static void EmitAffinityOverride(StringBuilder sb, BindingTypeGroup group, string bindingExpression)
    {
        var conversionArg = FormatFallbackConversion(group);
        var schedulerArg = group.HasScheduler ? "scheduler" : "null";

        BindingEmitterHelpers.EmitAffinityOverride(
            sb,
            group,
            "OneWayBind",
            $"view, viewModel, viewModelProperty, viewProperty, {conversionArg}{schedulerArg}, {bindingExpression}",
            false);
    }

    /// <summary>Appends the call a matched dispatch branch hands the binding to.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group, which fixes the arguments the worker takes.</param>
    /// <param name="inv">The call site the branch matched.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendDispatchReturn(StringBuilder sb, BindingTypeGroup group, BindingInvocationInfo inv) =>
        CodeGeneratorHelpers.AppendDispatchReturn(
            sb,
            WorkerMethodPrefix + CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.SourceTypeFullName,
                inv.CallerFilePath,
                inv.CallerLineNumber,
                $"{inv.SourceExpressionText}|{inv.TargetExpressionText}"),
            WorkerArguments + FormatExtraArgs(group));
}

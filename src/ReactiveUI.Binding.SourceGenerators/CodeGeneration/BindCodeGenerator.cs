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
/// Generates concrete typed extension method overloads and binding methods for Bind (view-first two-way) invocations.
/// The generated methods return <c>IReactiveBinding&lt;TView, BindingChange&gt;</c> and use view-first parameter ordering.
/// </summary>
internal static class BindCodeGenerator
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
    private const string WorkerMethodPrefix = "__Bind_";

    /// <summary>The two objects a generated worker binds, in its own parameter order.</summary>
    private const string WorkerArguments = "viewModel, view";

    /// <summary>The indentation a statement inside the emitted subscription body sits at.</summary>
    private const string SubscriptionBodyIndent = "                ";

    /// <summary>What this API calls the view-model-to-view converter in its generated signatures.</summary>
    private const string ForwardConverterName = "viewModelToViewConverter";

    /// <summary>What this API calls the view-to-view-model converter in its generated signatures.</summary>
    private const string ReverseConverterName = "viewToViewModelConverter";

    /// <summary>Name of the generated local holding the view model side observable.</summary>
    private const string ViewModelObservableName = "vmObs";

    /// <summary>Name of the generated local holding the view side observable.</summary>
    private const string ViewObservableName = "viewObs";

    /// <summary>What distinguishes this API's generated dispatch overload from the other three.</summary>
    private static readonly BindingEmitterHelpers.BindingDispatchApi DispatchApi = new()
    {
        Name = "Bind",
        ReceiverParameterName = "view",
        OtherParameterName = "viewModel",
        ReceiverIsTarget = true,
        SourceSelectorName = "viewModelProperty",
        TargetSelectorName = "viewProperty",
        WorkerMethodPrefix = "__Bind_",
        WorkerArguments = "viewModel, view",
        NormalizesStaticPrefix = false,
        FormatReturnType = FormatReturnType,
        AppendExtraParameters = AppendExtraParameters,
        FormatExtraArguments = FormatExtraArgs,
        EmitAffinityOverride = EmitAffinityOverride,
    };

    /// <summary>Groups Bind invocations by their type signature for overload generation.</summary>
    /// <param name="invocations">The Bind invocations to group.</param>
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void GenerateConcreteOverload(StringBuilder sb, BindingTypeGroup group, bool supportsCallerArgExpr, bool supportsNullable, bool stubHasExpressionParameters) =>
        BindingEmitterHelpers.GenerateDispatchOverload(sb, group, DispatchApi, supportsCallerArgExpr, supportsNullable, stubHasExpressionParameters);

    /// <summary>Generates a private Bind method for a specific invocation.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="sourceClassInfo">The source type class binding info.</param>
    /// <param name="targetClassInfo">The target type class binding info.</param>
    /// <param name="suffix">The stable method name suffix.</param>
    internal static void GenerateBindMethod(
        StringBuilder sb,
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        ClassBindingInfo? targetClassInfo,
        string suffix)
    {
        var viewPropertyAccess = CodeGeneratorHelpers.BuildGuardedAssignment(
            "view",
            inv.TargetPropertyPath,
            "value",
            SubscriptionBodyIndent);

        // Both directions follow the view's current view model, so the write walks the same re-rooted path the
        // observation does and its guard drops the write while the view holds none.
        var observation = BindingEmitterHelpers.ResolveViewModelObservation(inv, sourceClassInfo, targetClassInfo);
        var viewModelSetAccess = CodeGeneratorHelpers.BuildGuardedAssignment(
            observation.RootVariable,
            observation.Path,
            "value",
            SubscriptionBodyIndent);
        var viewModelPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.SourcePropertyPath);
        var viewPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.TargetPropertyPath);

        var extraParams = FormatExtraMethodParams(inv);
        var conversionComment = inv.HasConversion ? " (with conversion)" : string.Empty;
        var schedulerComment = inv.HasScheduler ? " (with scheduler)" : string.Empty;
        var returnType = FormatMethodReturnType(inv);

        _ = sb.Append("        private static ").Append(returnType).Append(" __Bind_").Append(suffix).Append('(').Append(inv.SourceTypeFullName)
            .Append(" viewModel, ").Append(inv.TargetTypeFullName).Append(" view").Append(extraParams).AppendLine(")").AppendLine("        {")
            .Append("            // Bind: ").Append(viewModelPathComment).Append(" <-> ").Append(viewPathComment).Append(conversionComment)
            .Append(schedulerComment).AppendLine();

        BindingEmitterHelpers.EmitBindingHookGuard(sb, "viewModel", "view", "TwoWay", "null");

        // Emit inline observation code instead of delegating to WhenChanged dispatch
        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            observation.RootVariable,
            observation.Path,
            inv.SourcePropertyTypeFullName,
            observation.RootClassInfo,
            ViewModelObservableName);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            "view",
            inv.TargetPropertyPath,
            inv.TargetPropertyTypeFullName,
            targetClassInfo,
            ViewObservableName);

        if (inv.HasConversion || inv.HasScheduler)
        {
            var (viewModelVar, viewVar) = EmitConversionAndSchedulerStages(sb, inv);
            (viewModelVar, viewVar) = EmitRegistryConversionStages(sb, inv, viewModelVar, viewVar);
            viewModelVar = BindingEmitterHelpers.EmitViewThreadStage(sb, inv, viewModelVar, "viewThreadObs");

            EmitTwoWaySubscription(sb, inv, viewModelVar, viewVar, viewPropertyAccess, viewModelSetAccess);
        }
        else
        {
            var (viewModelVar, viewVar) =
                EmitRegistryConversionStages(sb, inv, ViewModelObservableName, ViewObservableName);
            viewModelVar = BindingEmitterHelpers.EmitViewThreadStage(sb, inv, viewModelVar, "viewThreadObs");

            EmitTwoWaySubscription(sb, inv, viewModelVar, viewVar, viewPropertyAccess, viewModelSetAccess);
        }
    }

    /// <summary>Appends extra parameters (converters, scheduler) to the concrete overload signature.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendExtraParameters(StringBuilder sb, BindingTypeGroup group) =>
        BindingEmitterHelpers.AppendTwoWayExtraParameters(sb, group, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats extra arguments (converters, scheduler) for forwarding to the binding method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>Extra arguments string or empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraArgs(BindingTypeGroup group) =>
        BindingEmitterHelpers.FormatTwoWayExtraArgs(group, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats extra method parameters for the private binding method signature.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>Extra parameters string for converter and scheduler parameters.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv) =>
        BindingEmitterHelpers.FormatTwoWayExtraMethodParams(inv, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats the return type for a concrete Bind overload.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>The fully qualified return type string.</returns>
    internal static string FormatReturnType(BindingTypeGroup group) =>
        $"global::ReactiveUI.Binding.IReactiveBinding<{group.TargetTypeFullName}, {BindingChange}>";

    /// <summary>Formats the return type for a private Bind method.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>The fully qualified return type string.</returns>
    internal static string FormatMethodReturnType(BindingInvocationInfo inv) =>
        $"global::ReactiveUI.Binding.IReactiveBinding<{inv.TargetTypeFullName}, {BindingChange}>";

    /// <summary>
    /// Emits the conversion and scheduler stages that sit between the raw observations and the
    /// subscription, and reports the variable names the subscription should read from.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>The view model and view observable variable names after the stages are applied.</returns>
    private static BindingObservables EmitConversionAndSchedulerStages(
        StringBuilder sb,
        BindingInvocationInfo inv)
    {
        var viewModelVar = ViewModelObservableName;
        var viewVar = ViewObservableName;

        if (inv.HasConversion)
        {
            var viewModelNext = inv.HasScheduler ? "__vmSelected" : "vmBind";
            var viewNext = inv.HasScheduler ? "__viewSelected" : "viewBind";
            _ = sb.Append("        var ").Append(viewModelNext).Append(" = new ").Append(MapSignal).Append('<').Append(inv.SourcePropertyTypeFullName)
                .Append(", ").Append(inv.TargetPropertyTypeFullName).Append(">(").Append(viewModelVar).AppendLine(", viewModelToViewConverter);")
                .Append("        var ").Append(viewNext).Append(" = new ").Append(MapSignal).Append('<').Append(inv.TargetPropertyTypeFullName)
                .Append(", ").Append(inv.SourcePropertyTypeFullName).Append(">(").Append(viewVar).AppendLine(", viewToViewModelConverter);");
            viewModelVar = viewModelNext;
            viewVar = viewNext;
        }

        if (inv.HasScheduler)
        {
            _ = sb.Append("        var vmBind = ").Append(LinqExtensions).Append(".ObserveOn<").Append(inv.TargetPropertyTypeFullName).Append(">(")
                .Append(viewModelVar).AppendLine(", scheduler);").Append("        var viewBind = ").Append(LinqExtensions).Append(".ObserveOn<")
                .Append(inv.SourcePropertyTypeFullName).Append(">(").Append(viewVar).AppendLine(", scheduler);");
            viewModelVar = "vmBind";
            viewVar = "viewBind";
        }

        return new(viewModelVar, viewVar);
    }

    /// <summary>Emits the two-way subscription, change-stream merge, and <c>ReactiveBinding</c> return block.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="viewModelVar">The view model observable variable name to subscribe to.</param>
    /// <param name="viewVar">The view observable variable name to subscribe to.</param>
    /// <param name="viewPropertyAccess">The view property setter access chain.</param>
    /// <param name="viewModelSetAccess">The view model property setter access chain.</param>
    private static void EmitTwoWaySubscription(
        StringBuilder sb,
        BindingInvocationInfo inv,
        string viewModelVar,
        string viewVar,
        string viewPropertyAccess,
        string viewModelSetAccess) => _ = sb.AppendLine().Append("            var d1 = ").Append(BindingErrors).Append(".Subscribe(")
            .Append(viewModelVar).AppendLine(", value =>").AppendLine(GeneratedSyntax.StatementBlockOpen).Append("                ").Append(viewPropertyAccess)
            .AppendLine().Append("            }, \"").Append(CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText)).AppendLine("\");")
            .AppendLine().Append("            var __viewSkipped = global::ReactiveUI.Primitives.LinqExtensions.Skip(").Append(viewVar)
            .AppendLine(", 1);").Append("            var d2 = ").Append(BindingErrors).AppendLine(".Subscribe(__viewSkipped, value =>")
            .AppendLine(GeneratedSyntax.StatementBlockOpen).Append("                ").Append(viewModelSetAccess).AppendLine().Append("            }, \"")
            .Append(CodeGeneratorHelpers.EscapeString(inv.SourceExpressionText)).AppendLine("\");").AppendLine()
            .Append("            var __vmTagged = new ").Append(MapSignal).Append('<').Append(inv.TargetPropertyTypeFullName).Append(", ")
            .Append(BindingChange).Append(">(").Append(viewModelVar).Append(", v => new ").Append(BindingChange).AppendLine("(v, true));")
            .Append("            var __viewTagged = new ").Append(MapSignal).Append('<').Append(inv.SourcePropertyTypeFullName).Append(", ")
            .Append(BindingChange).Append(">(__viewSkipped, v => new ").Append(BindingChange).AppendLine("(v, false));")
            .Append("            var changed = new ").Append(MergeSignal).Append('<').Append(BindingChange).AppendLine(">(__vmTagged, __viewTagged);")
            .AppendLine().AppendLine("            var disposable = new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(d1, d2);")
            .AppendLine().Append("            return new global::ReactiveUI.Binding.ReactiveBinding<").Append(inv.TargetTypeFullName).Append(", ")
            .Append(BindingChange).AppendLine(">(").AppendLine("                view,").AppendLine("                changed,")
            .AppendLine("                global::ReactiveUI.Binding.BindingDirection.TwoWay,").AppendLine("                disposable);")
            .AppendLine("        }").AppendLine();

    /// <summary>Emits the stages that convert each direction to the type the other side declares.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="viewModelVar">The variable holding the view model side's values.</param>
    /// <param name="viewVar">The variable holding the view side's values.</param>
    /// <returns>The variables to subscribe each direction to.</returns>
    /// <remarks>
    /// Two-way needs both: each direction assigns across the same type gap, in opposite directions.
    /// </remarks>
    private static BindingObservables EmitRegistryConversionStages(
        StringBuilder sb,
        BindingInvocationInfo inv,
        string viewModelVar,
        string viewVar)
    {
        if (!BindingEmitterHelpers.RequiresRegistryConversion(inv))
        {
            return new(viewModelVar, viewVar);
        }

        const string convertedViewModelVar = "convertedVmObs";
        const string convertedViewVar = "convertedViewObs";

        BindingEmitterHelpers.EmitRegistryConversion(
            sb,
            viewModelVar,
            convertedViewModelVar,
            inv.SourcePropertyTypeFullName,
            inv.TargetPropertyTypeFullName);

        BindingEmitterHelpers.EmitRegistryConversion(
            sb,
            viewVar,
            convertedViewVar,
            inv.TargetPropertyTypeFullName,
            inv.SourcePropertyTypeFullName);

        return new(convertedViewModelVar, convertedViewVar);
    }

    /// <summary>Names the conversions the fallback needs, matching whatever the generated body would apply.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>The converter-pair argument, trailed by a comma, or empty when both sides share a type.</returns>
    private static string FormatFallbackConverters(BindingTypeGroup group)
    {
        if (group.HasConversion)
        {
            return $"{TwoWayConverters}.Create({ForwardConverterName}, {ReverseConverterName}), ";
        }

        if (!BindingEmitterHelpers.RequiresRegistryConversion(group))
        {
            return string.Empty;
        }

        var forward = CodeGeneratorHelpers.FormatRegistryConversionLambda(
            group.SourcePropertyTypeFullName,
            group.TargetPropertyTypeFullName);
        var reverse = CodeGeneratorHelpers.FormatRegistryConversionLambda(
            group.TargetPropertyTypeFullName,
            group.SourcePropertyTypeFullName);

        // Both arguments are lambdas, which cannot drive inference, so the pair names its types outright.
        return
            $"{TwoWayConverters}.Create<{group.SourcePropertyTypeFullName}, {group.TargetPropertyTypeFullName}>({forward}, {reverse}), ";
    }

    /// <summary>Emits the check that hands the binding to the runtime engine when a registered plugin outranks the generated one.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group, which fixes both bound types for the whole overload.</param>
    /// <param name="bindingExpression">The C# expression naming the bound view property, used when a write faults.</param>
    private static void EmitAffinityOverride(StringBuilder sb, BindingTypeGroup group, string bindingExpression)
    {
        var convertersArg = FormatFallbackConverters(group);
        var schedulerArg = group.HasScheduler ? "scheduler" : "null";

        BindingEmitterHelpers.EmitAffinityOverride(
            sb,
            group,
            "Bind",
            $"view, viewModel, viewModelProperty, viewProperty, {convertersArg}{schedulerArg}, {bindingExpression}",
            true);
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
    /// <summary>Gets what distinguishes this API's generated dispatch overload from the other three.</summary>
    internal static readonly BindingEmitterHelpers.BindingDispatchApi DispatchApi = new()
    {
        Name = "Bind",
        ReceiverParameterName = "view",
        OtherParameterName = "viewModel",
        ReceiverIsTarget = true,
        SourceSelectorName = "viewModelProperty",
        TargetSelectorName = "viewProperty",
        WorkerMethodPrefix = "__Bind_",
        WorkerSourceParameterName = "viewModel",
        WorkerTargetParameterName = "view",
        IsTwoWay = true,
        HookRefusalValue = "null",
        SourceObservableName = ViewModelObservableName,
        TargetObservableName = ViewObservableName,
        SourceConvertedName = "__vmSelected",
        TargetConvertedName = "__viewSelected",
        SourceScheduledName = "vmBind",
        TargetScheduledName = "viewBind",
        ForwardConverterArgument = ForwardConverterName,
        ReverseConverterArgument = ReverseConverterName,
        NormalizesStaticPrefix = false,
        FormatReturnType = FormatReturnType,
        FormatWorkerReturnType = FormatMethodReturnType,
        AppendExtraParameters = AppendExtraParameters,
        FormatWorkerParameters = FormatExtraMethodParams,
        FormatExtraArguments = FormatExtraArgs,
    };

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
        BindingEmitterHelpers.AppendWorkerMethodHeader(sb, DispatchApi, inv, suffix);

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

        var (viewModelVar, viewVar) = BindingEmitterHelpers.EmitDualStreamStages(sb, DispatchApi, inv);
        (viewModelVar, viewVar) = EmitRegistryConversionStages(sb, inv, viewModelVar, viewVar);

        EmitTwoWaySubscription(sb, inv, viewModelVar, viewVar, viewPropertyAccess, viewModelSetAccess);
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

    /// <summary>Emits the two-way pipeline and the <c>ReactiveBinding</c> return block.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="viewModelVar">The view model observable variable name.</param>
    /// <param name="viewVar">The view observable variable name.</param>
    /// <param name="viewPropertyAccess">The view property setter access chain.</param>
    /// <param name="viewModelSetAccess">The view model property setter access chain.</param>
    /// <remarks>
    /// Both directions become one stream, which is then routed once and applied once. Three things follow from
    /// that, and none of them hold when each direction is wired on its own.
    /// <para>
    /// The view model side is merged first, so its value reaches the view before the view's own first value is
    /// weighed - and the guard, seeing them equal, drops that one. Dropping the view's first value by position
    /// would eat a real change wherever the view reports nothing to begin with, which a chain through a null
    /// intermediate does.
    /// </para>
    /// <para>
    /// Routing the merged stream carries the view's read and the view model's write, not just the write to the
    /// view. Routing one direction leaves the other running wherever its notification was raised.
    /// </para>
    /// <para>
    /// What was applied is published to a subject the caller subscribes to, so a subscriber shares the one
    /// upstream subscription rather than attaching another set of handlers, and never sees a value the guard
    /// refused to write.
    /// </para>
    /// </remarks>
    private static void EmitTwoWaySubscription(
        StringBuilder sb,
        BindingInvocationInfo inv,
        string viewModelVar,
        string viewVar,
        string viewPropertyAccess,
        string viewModelSetAccess)
    {
        _ = sb.AppendLine()
            .Append("            var __vmTagged = new ").Append(MapSignal).Append('<').Append(inv.TargetPropertyTypeFullName).Append(", ")
            .Append(BindingChange).Append(">(").Append(viewModelVar).Append(", v => new ").Append(BindingChange).AppendLine("(v, true));")
            .Append("            var __viewTagged = new ").Append(MapSignal).Append('<').Append(inv.SourcePropertyTypeFullName).Append(", ")
            .Append(BindingChange).Append(">(").Append(viewVar).Append(", v => new ").Append(BindingChange).AppendLine("(v, false));")
            .Append("            var __sides = new ").Append(MergeSignal).Append('<').Append(BindingChange).AppendLine(">(__vmTagged, __viewTagged);");

        var routedVar = BindingEmitterHelpers.EmitViewThreadStage(sb, inv, "__sides", "__routed", "view");

        _ = sb.AppendLine("            var changed = new global::ReactiveUI.Binding.Observables.AppliedChangeObservable();")
            .AppendLine().Append("            var disposable = ").Append(BindingErrors).Append(".Subscribe(").Append(routedVar).AppendLine(", __change =>")
            .AppendLine(GeneratedSyntax.StatementBlockOpen)
            .AppendLine("                if (__change.FromViewModel)")
            .AppendLine("                {")
            .Append("                    var value = (").Append(inv.TargetPropertyTypeFullName).AppendLine(")__change.Value;")
            .Append("                    ").Append(viewPropertyAccess).AppendLine()
            .AppendLine("                }")
            .AppendLine("                else")
            .AppendLine("                {")
            .Append("                    var value = (").Append(inv.SourcePropertyTypeFullName).AppendLine(")__change.Value;")
            .Append("                    ").Append(viewModelSetAccess).AppendLine()
            .AppendLine("                }")
            .AppendLine()
            .AppendLine("                changed.OnNext(__change);")
            .Append("            }, \"").Append(CodeGeneratorHelpers.EscapeString(inv.SourceExpressionText)).Append(" / ")
            .Append(CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText)).AppendLine("\");")
            .AppendLine().Append("            return new global::ReactiveUI.Binding.ReactiveBinding<").Append(inv.TargetTypeFullName).Append(", ")
            .Append(BindingChange).AppendLine(">(").AppendLine("                view,").AppendLine("                changed,")
            .AppendLine("                global::ReactiveUI.Binding.BindingDirection.TwoWay,").AppendLine("                disposable);")
            .AppendLine("        }").AppendLine();
    }

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
}

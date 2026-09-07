// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
    /// <summary>Gets what distinguishes this API's generated dispatch overload from the other three.</summary>
    internal static readonly BindingEmitterHelpers.BindingDispatchApi DispatchApi = new()
    {
        Name = "OneWayBind",
        ReceiverParameterName = "view",
        OtherParameterName = "viewModel",
        ReceiverIsTarget = true,
        SourceSelectorName = "viewModelProperty",
        TargetSelectorName = "viewProperty",
        WorkerMethodPrefix = "__OneWayBind_",
        WorkerSourceParameterName = "viewModel",
        WorkerTargetParameterName = "view",
        IsTwoWay = false,
        HookRefusalValue = "null",
        SourceObservableName = SourceObservableVariable,
        SourceConvertedName = "__selected",
        SourceScheduledName = "bindObs",
        ForwardConverterArgument = ConversionParameterName,
        NormalizesStaticPrefix = false,
        FormatReturnType = FormatReturnType,
        FormatWorkerReturnType = FormatMethodReturnType,
        AppendExtraParameters = AppendExtraParameters,
        FormatWorkerParameters = FormatExtraMethodParams,
        FormatExtraArguments = FormatExtraArgs,
    };

    /// <summary>The indentation a statement inside the emitted subscription body sits at.</summary>
    private const string SubscriptionBodyIndent = "                    ";

    /// <summary>What this API calls the projection argument in its generated signatures.</summary>
    private const string ConversionParameterName = "selector";

    /// <summary>Name of the emitted local holding the source property observation, before conversion or scheduling.</summary>
    private const string SourceObservableVariable = "sourceObs";

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
        BindingEmitterHelpers.AppendWorkerMethodHeader(sb, DispatchApi, inv, suffix);

        // Emit inline observation code instead of delegating to WhenChanged dispatch
        var observation = BindingEmitterHelpers.ResolveViewModelObservation(inv, sourceClassInfo, targetClassInfo);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            observation.RootVariable,
            observation.Path,
            inv.SourcePropertyTypeFullName,
            observation.RootClassInfo,
            SourceObservableVariable);

        var currentVar = BindingEmitterHelpers.EmitSingleStreamStages(sb, DispatchApi, inv);

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
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
    /// <summary>Gets what distinguishes this API's generated dispatch overload from the other three.</summary>
    internal static readonly BindingEmitterHelpers.BindingDispatchApi DispatchApi = new()
    {
        Name = "BindOneWay",
        ReceiverParameterName = "source",
        OtherParameterName = "target",
        ReceiverIsTarget = false,
        SourceSelectorName = "sourceProperty",
        TargetSelectorName = "targetProperty",
        WorkerMethodPrefix = "__BindOneWay_",
        WorkerArguments = "source, target",
        NormalizesStaticPrefix = true,
        AppendExtraParameters = AppendExtraParameters,
        FormatExtraArguments = FormatExtraArgs,
        EmitAffinityOverride = EmitAffinityOverride,
    };

    /// <summary>What this API calls the conversion argument in its generated signatures.</summary>
    private const string ConversionParameterName = "conversionFunc";

    /// <summary>Name of the emitted local holding the source property observation, before conversion or scheduling.</summary>
    private const string SourceObservableVariable = "sourceObs";

    /// <summary>The indentation a statement inside the emitted subscription body sits at.</summary>
    private const string SubscriptionBodyIndent = "                ";

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

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
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
    /// <summary>Gets what distinguishes this API's generated dispatch overload from the other three.</summary>
    internal static readonly BindingEmitterHelpers.BindingDispatchApi DispatchApi = new()
    {
        Name = "BindTwoWay",
        ReceiverParameterName = "source",
        OtherParameterName = "target",
        ReceiverIsTarget = false,
        SourceSelectorName = "sourceProperty",
        TargetSelectorName = "targetProperty",
        WorkerMethodPrefix = "__BindTwoWay_",
        WorkerSourceParameterName = SourceParameterName,
        WorkerTargetParameterName = TargetParameterName,
        IsTwoWay = true,
        HookRefusalValue = "global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance",
        SourceObservableName = BindTwoWayCodeGenerator.SourceObservableName,
        TargetObservableName = BindTwoWayCodeGenerator.TargetObservableName,
        SourceConvertedName = "__srcSelected",
        TargetConvertedName = "__tgtSelected",
        SourceScheduledName = "sourceBind",
        TargetScheduledName = "targetBind",
        ForwardConverterArgument = ForwardConverterName,
        ReverseConverterArgument = ReverseConverterName,
        NormalizesStaticPrefix = true,
        AppendExtraParameters = AppendExtraParameters,
        FormatWorkerParameters = FormatExtraMethodParams,
        FormatExtraArguments = FormatExtraArgs,
    };

    /// <summary>The indentation a statement inside the emitted subscription body sits at.</summary>
    private const string SubscriptionBodyIndent = "                ";

    /// <summary>What this API calls the source-to-target converter in its generated signatures.</summary>
    private const string ForwardConverterName = "sourceToTargetConv";

    /// <summary>What this API calls the target-to-source converter in its generated signatures.</summary>
    private const string ReverseConverterName = "targetToSourceConv";

    /// <summary>Name of the source parameter on the generated binding method.</summary>
    private const string SourceParameterName = "source";

    /// <summary>Name of the target parameter on the generated binding method.</summary>
    private const string TargetParameterName = "target";

    /// <summary>Name of the generated local holding the source side observable.</summary>
    private const string SourceObservableName = "sourceObs";

    /// <summary>Name of the generated local holding the target side observable.</summary>
    private const string TargetObservableName = "targetObs";

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
        var targetAccess = CodeGeneratorHelpers.BuildGuardedAssignment(
            "target",
            inv.TargetPropertyPath,
            "value",
            SubscriptionBodyIndent);
        var sourceSetAccess = CodeGeneratorHelpers.BuildGuardedAssignment(
            "source",
            inv.SourcePropertyPath,
            "value",
            SubscriptionBodyIndent);
        BindingEmitterHelpers.AppendWorkerMethodHeader(sb, DispatchApi, inv, suffix);

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

        var (sourceVar, targetVar) = BindingEmitterHelpers.EmitDualStreamStages(sb, DispatchApi, inv);
        sourceVar = BindingEmitterHelpers.EmitViewThreadStage(sb, inv, sourceVar, "targetThreadObs");

        EmitTwoWaySubscription(sb, inv, sourceVar, targetVar, targetAccess, sourceSetAccess);
    }

    /// <summary>Appends extra parameters (converters, scheduler) to the concrete overload signature.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendExtraParameters(StringBuilder sb, BindingTypeGroup group) =>
        BindingEmitterHelpers.AppendTwoWayExtraParameters(sb, group, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats extra arguments (converters, scheduler) for forwarding to the binding method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>Extra arguments string like ", sourceToTargetConv, targetToSourceConv, scheduler" or empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraArgs(BindingTypeGroup group) =>
        BindingEmitterHelpers.FormatTwoWayExtraArgs(group, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats extra method parameters for the private binding method signature.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>Extra parameters string for two-way converter and scheduler parameters.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv) =>
        BindingEmitterHelpers.FormatTwoWayExtraMethodParams(inv, ForwardConverterName, ReverseConverterName);

    /// <summary>Emits the two-way subscription and <c>MultipleDisposable</c> return block.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The call site, naming the expressions a faulting write is reported against.</param>
    /// <param name="sourceVar">The source observable variable name to subscribe to.</param>
    /// <param name="targetVar">The target observable variable name to subscribe to.</param>
    /// <param name="targetAccess">The target property setter access chain.</param>
    /// <param name="sourceSetAccess">The source property setter access chain.</param>
    private static void EmitTwoWaySubscription(
        StringBuilder sb,
        BindingInvocationInfo inv,
        string sourceVar,
        string targetVar,
        string targetAccess,
        string sourceSetAccess) => _ = sb.AppendLine().Append("            var d1 = ").Append(BindingErrors).Append(".Subscribe(").Append(sourceVar)
            .AppendLine(", value =>").AppendLine(GeneratedSyntax.StatementBlockOpen).Append("                ").Append(targetAccess).AppendLine()
            .Append("            }, \"").Append(CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText)).AppendLine("\");").AppendLine()
            .Append("            var __targetSkipped = global::ReactiveUI.Primitives.LinqExtensions.Skip(").Append(targetVar).AppendLine(", 1);")
            .Append("            var d2 = ").Append(BindingErrors).AppendLine(".Subscribe(__targetSkipped, value =>").AppendLine(GeneratedSyntax.StatementBlockOpen)
            .Append("                ").Append(sourceSetAccess).AppendLine().Append("            }, \"")
            .Append(CodeGeneratorHelpers.EscapeString(inv.SourceExpressionText)).AppendLine("\");").AppendLine()
            .AppendLine("            return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(d1, d2);").AppendLine("        }")
            .AppendLine();
}

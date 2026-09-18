// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using static ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding.EventCommandBindingEmitter;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>
/// Command binding plugin for controls that have a default event but no <c>Command</c>
/// or <c>Enabled</c> properties. Subscribes to the event for command execution only.
/// Replaces the runtime <c>CreatesCommandBindingViaEvent</c> binder.
/// Affinity 3 (lowest priority among plugins).
/// </summary>
/// <remarks>
/// Platforms covered: Any control with a Click/TouchUpInside/Pressed event
/// that does not have Command or Enabled properties.
/// </remarks>
internal sealed class DefaultEventBindingPlugin : ICommandBindingPlugin
{
    /// <summary>The affinity score for the default-event binder (lowest priority among command binding plugins).</summary>
    private static readonly int DefaultEventAffinity = BindingAffinity.DefaultEvent;

    /// <inheritdoc/>
    public int Affinity => DefaultEventAffinity;

    /// <inheritdoc/>
    public bool RequiresCustomBinderFallback => true;

    /// <inheritdoc/>
    public bool CanHandle(BindCommandInvocationInfo inv) =>
        inv.ResolvedEventName is not null;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitBinding(StringBuilder sb, BindCommandInvocationInfo inv, string controlAccess, bool supportsNullable) =>
        CommandEventBindingEmitter.EmitByParameterKind(
            sb,
            inv,
            controlAccess,
            supportsNullable,
            EmitWithObservableParameter,
            EmitWithNoParameter);

    /// <summary>Emits command execution using the latest streamed parameter.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="inv">The extracted binding call.</param>
    /// <param name="controlAccess">The control's typed access expression.</param>
    /// <param name="eventArgsType">The framework event argument type.</param>
    /// <param name="supportsNullable">Whether nullable annotations are available.</param>
    internal static void EmitWithObservableParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        bool supportsNullable)
    {
        AppendLatestParameterCapture(sb, inv);
        AppendCommandMissingExit(sb);
        AppendHandlerDeclaration(sb, eventArgsType, supportsNullable);

        _ = sb.Append("                    var param = ").Append(CommandParameterEmitter.Read(inv)).AppendLine(";");

        AppendHandlerExecution(sb, "param");
        AppendHandlerAttachment(sb, inv, controlAccess);
        AppendParameterisedDisposableReturn(sb);
    }

    /// <summary>Emits command execution using the selected parameter property.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="inv">The extracted binding call.</param>
    /// <param name="controlAccess">The control's typed access expression.</param>
    /// <param name="eventArgsType">The framework event argument type.</param>
    /// <param name="paramAccess">The typed command parameter access.</param>
    /// <param name="supportsNullable">Whether nullable annotations are available.</param>
    internal static void EmitWithExpressionParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        string paramAccess,
        bool supportsNullable)
    {
        _ = sb.AppendLine();
        AppendCommandSubscription(sb);
        AppendCommandMissingExit(sb);
        AppendHandlerDeclaration(sb, eventArgsType, supportsNullable);

        _ = sb.Append("                    var param = ").Append(paramAccess).AppendLine(";");

        AppendHandlerExecution(sb, "param");
        AppendHandlerAttachment(sb, inv, controlAccess);

        _ = sb.AppendLine(CommandBindingSyntax.CommandOnlyDisposableReturn).AppendLine(GeneratedSyntax.MemberBodyClose);
    }

    /// <summary>Emits command execution without a parameter.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="inv">The extracted binding call.</param>
    /// <param name="controlAccess">The control's typed access expression.</param>
    /// <param name="eventArgsType">The framework event argument type.</param>
    /// <param name="supportsNullable">Whether nullable annotations are available.</param>
    internal static void EmitWithNoParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        bool supportsNullable)
    {
        _ = sb.AppendLine();
        AppendCommandSubscription(sb);
        AppendCommandMissingExit(sb);
        AppendHandlerDeclaration(sb, eventArgsType, supportsNullable);
        AppendHandlerExecution(sb, "null");
        AppendHandlerAttachment(sb, inv, controlAccess);

        _ = sb.AppendLine(CommandBindingSyntax.CommandOnlyDisposableReturn).AppendLine(GeneratedSyntax.MemberBodyClose);
    }

    /// <summary>Appends the exit taken while the view model has handed over no command.</summary>
    /// <param name="sb">The string builder to append to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendCommandMissingExit(StringBuilder sb) =>
        _ = sb.AppendLine(CommandBindingSyntax.CommandMissingReturn).AppendLine(CommandBindingSyntax.SubscriptionBlockClose);

    /// <summary>Appends the handler's subscription to the control's event, and the disposable that detaches it.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendHandlerAttachment(StringBuilder sb, BindCommandInvocationInfo inv, string controlAccess) =>
        _ = sb.Append("                ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName).AppendLine(CommandBindingSyntax.HandlerSubscribe)
            .AppendLine(CommandBindingSyntax.SerialDisposableOpen)
            .Append("                    ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName).AppendLine(CommandBindingSyntax.HandlerUnsubscribeAndClose)
            .AppendLine(CommandBindingSyntax.CommandSubscriptionClose);
}

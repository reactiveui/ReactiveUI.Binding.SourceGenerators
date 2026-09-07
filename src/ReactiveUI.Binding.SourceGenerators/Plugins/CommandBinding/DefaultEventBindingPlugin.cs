// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

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
internal sealed class DefaultEventBindingPlugin : EventCommandBindingPlugin
{
    /// <summary>The affinity score for the default-event binder (lowest priority among command binding plugins).</summary>
    private static readonly int DefaultEventAffinity = BindingAffinity.DefaultEvent;

    /// <inheritdoc/>
    public override int Affinity => DefaultEventAffinity;

    /// <inheritdoc/>
    public override bool CanHandle(BindCommandInvocationInfo inv) =>
        inv.ResolvedEventName is not null;

    /// <inheritdoc/>
    protected override void EmitWithObservableParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        bool supportsNullable)
    {
        AppendLatestParameterCapture(sb, inv, supportsNullable);
        AppendCommandMissingExit(sb);
        AppendHandlerDeclaration(sb, eventArgsType, supportsNullable);

        _ = sb.Append("                    var param = ").Append(CommandBindingSyntax.ReadLatestParameter(inv)).AppendLine(";");

        AppendHandlerExecution(sb, "param");
        AppendHandlerAttachment(sb, inv, controlAccess);
        AppendParameterisedDisposableReturn(sb);
    }

    /// <inheritdoc/>
    protected override void EmitWithExpressionParameter(
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

    /// <inheritdoc/>
    protected override void EmitWithNoParameter(
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

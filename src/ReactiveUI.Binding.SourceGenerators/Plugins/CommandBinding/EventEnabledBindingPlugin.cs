// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>
/// Command binding plugin for controls that have a default event and an <c>Enabled</c> property.
/// Subscribes to the event for command execution and synchronizes <c>Enabled</c>
/// with <c>ICommand.CanExecute</c> and <c>CanExecuteChanged</c>.
/// Replaces the runtime <c>CreatesWinFormsCommandBinding</c> and equivalent Android/iOS binders.
/// Affinity 4.
/// </summary>
/// <remarks>
/// Platforms covered: WinForms Control/ToolStripItem (Click+Enabled),
/// Android View (Click+Enabled), Apple UIControl (TouchUpInside+Enabled).
/// </remarks>
internal sealed class EventEnabledBindingPlugin : EventCommandBindingPlugin
{
    /// <summary>Opens the assignment that puts the control's enabled state in step with the command.</summary>
    private const string CanExecuteEnabledOpen = ".Enabled = cmd.CanExecute(";

    /// <summary>The affinity score for the event + Enabled binder.</summary>
    private static readonly int EventEnabledAffinity = BindingAffinity.EventEnabledControl;

    /// <inheritdoc/>
    public override int Affinity => EventEnabledAffinity;

    /// <inheritdoc/>
    public override bool CanHandle(BindCommandInvocationInfo inv) =>
        inv.ResolvedEventName is not null && inv.HasEnabledProperty;

    /// <inheritdoc/>
    protected override void EmitWithObservableParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        bool supportsNullable)
    {
        var latestParameter = CommandBindingSyntax.ReadLatestParameter(inv);

        AppendLatestParameterCapture(sb, inv, supportsNullable);
        AppendCommandMissingExit(sb, controlAccess);

        _ = sb.Append("                var param = ").Append(latestParameter).AppendLine(";");

        AppendEnabledSynchronization(sb, controlAccess, "param", latestParameter);
        AppendHandlerDeclaration(sb, eventArgsType, supportsNullable);

        _ = sb.Append("                    var p = ").Append(latestParameter).AppendLine(";");

        AppendHandlerExecution(sb, "p");
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
        AppendCommandMissingExit(sb, controlAccess);
        AppendEnabledSynchronization(sb, controlAccess, paramAccess, paramAccess);
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
        AppendCommandMissingExit(sb, controlAccess);
        AppendEnabledSynchronization(sb, controlAccess, "null", "null");
        AppendHandlerDeclaration(sb, eventArgsType, supportsNullable);
        AppendHandlerExecution(sb, "null");
        AppendHandlerAttachment(sb, inv, controlAccess);

        _ = sb.AppendLine(CommandBindingSyntax.CommandOnlyDisposableReturn).AppendLine(GeneratedSyntax.MemberBodyClose);
    }

    /// <summary>Appends the exit taken while the view model has handed over no command, disabling the control.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="controlAccess">The control access chain.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendCommandMissingExit(StringBuilder sb, string controlAccess) =>
        _ = sb.Append("                    ").Append(controlAccess).AppendLine(CommandBindingSyntax.DisableControl)
            .AppendLine(CommandBindingSyntax.CommandMissingReturn).AppendLine(CommandBindingSyntax.SubscriptionBlockClose).AppendLine();

    /// <summary>Appends the initial enabled state and the handler that keeps it in step with the command.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="controlAccess">The control access chain.</param>
    /// <param name="initialArgument">The parameter the command is asked about when the binding is made.</param>
    /// <param name="handlerArgument">The parameter the command is asked about on each subsequent change.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendEnabledSynchronization(
        StringBuilder sb,
        string controlAccess,
        string initialArgument,
        string handlerArgument) =>
        _ = sb.Append("                ").Append(controlAccess).Append(CanExecuteEnabledOpen).Append(initialArgument).AppendLine(");")
            .AppendLine(CommandBindingSyntax.CanExecuteHandlerDeclaration).Append("                    ")
            .Append(controlAccess).Append(CanExecuteEnabledOpen).Append(handlerArgument).AppendLine(");")
            .AppendLine(CommandBindingSyntax.CanExecuteSubscribe);

    /// <summary>Appends the handler's subscription to the control's event, and the disposable that detaches it.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendHandlerAttachment(StringBuilder sb, BindCommandInvocationInfo inv, string controlAccess) =>
        _ = sb.Append("                ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName).AppendLine(CommandBindingSyntax.HandlerSubscribe)
            .AppendLine(CommandBindingSyntax.SerialDisposableOpen)
            .AppendLine(CommandBindingSyntax.SubscriptionBlockOpen).Append("                    ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName)
            .AppendLine(CommandBindingSyntax.HandlerUnsubscribe).AppendLine(CommandBindingSyntax.CanExecuteUnsubscribe)
            .AppendLine(CommandBindingSyntax.SerialDisposableClose).AppendLine(CommandBindingSyntax.CommandSubscriptionClose);
}

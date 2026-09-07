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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void EmitWithObservableParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        bool supportsNullable) =>
        sb.AppendLine().Append("            ").Append(inv.ParameterTypeFullName)
            .Append(supportsNullable && inv.ParameterIsReferenceType ? "?" : string.Empty).AppendLine(" __latestParam = default;")
            .AppendLine("            var __paramSub = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(")
            .Append("                withParameter, p => ").Append(CommandBindingSyntax.WriteLatestParameter(inv, "p")).AppendLine(");").AppendLine()
            .AppendLine(CommandBindingSyntax.SerialDisposableDeclaration)
            .AppendLine(CommandBindingSyntax.CommandSubscriptionOpen)
            .AppendLine(GeneratedSyntax.StatementBlockOpen)
            .AppendLine(CommandBindingSyntax.ResetSerialDisposable)
            .AppendLine(CommandBindingSyntax.CommandMissingTest).AppendLine(CommandBindingSyntax.SubscriptionBlockOpen).Append("                    ").Append(controlAccess)
            .AppendLine(CommandBindingSyntax.DisableControl).AppendLine(CommandBindingSyntax.CommandMissingReturn).AppendLine(CommandBindingSyntax.SubscriptionBlockClose).AppendLine()
            .Append("                var param = ").Append(CommandBindingSyntax.ReadLatestParameter(inv)).AppendLine(";").Append("                ")
            .Append(controlAccess).AppendLine(".Enabled = cmd.CanExecute(param);")
            .AppendLine(CommandBindingSyntax.CanExecuteHandlerDeclaration).Append("                    ")
            .Append(controlAccess).Append(CanExecuteEnabledOpen).Append(CommandBindingSyntax.ReadLatestParameter(inv)).AppendLine(");")
            .AppendLine(CommandBindingSyntax.CanExecuteSubscribe).AppendLine().Append(CommandBindingSyntax.HandlerDeclarationOpen)
            .Append(CommandEventBindingEmitter.SenderType(supportsNullable)).Append(CommandBindingSyntax.HandlerSenderSeparator).Append(eventArgsType).AppendLine(" e)")
            .AppendLine(CommandBindingSyntax.SubscriptionBlockOpen)
            .Append("                    var p = ").Append(CommandBindingSyntax.ReadLatestParameter(inv)).AppendLine(";")
            .AppendLine("                    if (cmd.CanExecute(p))").AppendLine(CommandBindingSyntax.NestedBlockOpen)
            .AppendLine("                        cmd.Execute(p);").AppendLine(CommandBindingSyntax.NestedBlockClose).AppendLine(CommandBindingSyntax.SubscriptionBlockClose).AppendLine()
            .Append("                ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName).AppendLine(CommandBindingSyntax.HandlerSubscribe)
            .AppendLine(CommandBindingSyntax.SerialDisposableOpen)
            .AppendLine(CommandBindingSyntax.SubscriptionBlockOpen).Append("                    ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName)
            .AppendLine(CommandBindingSyntax.HandlerUnsubscribe).AppendLine(CommandBindingSyntax.CanExecuteUnsubscribe)
            .AppendLine(CommandBindingSyntax.SerialDisposableClose).AppendLine(CommandBindingSyntax.CommandSubscriptionClose)
            .AppendLine("            return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(")
            .AppendLine("                new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(__cmdSub, __paramSub), serial);")
            .AppendLine(GeneratedSyntax.MemberBodyClose);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void EmitWithExpressionParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        string paramAccess,
        bool supportsNullable) =>
        sb.AppendLine().AppendLine(CommandBindingSyntax.SerialDisposableDeclaration)
            .AppendLine(CommandBindingSyntax.CommandSubscriptionOpen)
            .AppendLine(GeneratedSyntax.StatementBlockOpen)
            .AppendLine(CommandBindingSyntax.ResetSerialDisposable)
            .AppendLine(CommandBindingSyntax.CommandMissingTest).AppendLine(CommandBindingSyntax.SubscriptionBlockOpen).Append("                    ").Append(controlAccess)
            .AppendLine(CommandBindingSyntax.DisableControl).AppendLine(CommandBindingSyntax.CommandMissingReturn).AppendLine(CommandBindingSyntax.SubscriptionBlockClose).AppendLine()
            .Append("                ").Append(controlAccess).Append(CanExecuteEnabledOpen).Append(paramAccess).AppendLine(");")
            .AppendLine(CommandBindingSyntax.CanExecuteHandlerDeclaration).Append("                    ")
            .Append(controlAccess).Append(CanExecuteEnabledOpen).Append(paramAccess).AppendLine(");")
            .AppendLine(CommandBindingSyntax.CanExecuteSubscribe).AppendLine().Append(CommandBindingSyntax.HandlerDeclarationOpen)
            .Append(CommandEventBindingEmitter.SenderType(supportsNullable)).Append(CommandBindingSyntax.HandlerSenderSeparator).Append(eventArgsType).AppendLine(" e)")
            .AppendLine(CommandBindingSyntax.SubscriptionBlockOpen).Append("                    var param = ").Append(paramAccess).AppendLine(";")
            .AppendLine("                    if (cmd.CanExecute(param))").AppendLine(CommandBindingSyntax.NestedBlockOpen)
            .AppendLine("                        cmd.Execute(param);").AppendLine(CommandBindingSyntax.NestedBlockClose).AppendLine(CommandBindingSyntax.SubscriptionBlockClose)
            .AppendLine().Append("                ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName).AppendLine(CommandBindingSyntax.HandlerSubscribe)
            .AppendLine(CommandBindingSyntax.SerialDisposableOpen)
            .AppendLine(CommandBindingSyntax.SubscriptionBlockOpen).Append("                    ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName)
            .AppendLine(CommandBindingSyntax.HandlerUnsubscribe).AppendLine(CommandBindingSyntax.CanExecuteUnsubscribe)
            .AppendLine(CommandBindingSyntax.SerialDisposableClose).AppendLine(CommandBindingSyntax.CommandSubscriptionClose)
            .AppendLine(CommandBindingSyntax.CommandOnlyDisposableReturn)
            .AppendLine(GeneratedSyntax.MemberBodyClose);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void EmitWithNoParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        bool supportsNullable) =>
        sb.AppendLine().AppendLine(CommandBindingSyntax.SerialDisposableDeclaration)
            .AppendLine(CommandBindingSyntax.CommandSubscriptionOpen)
            .AppendLine(GeneratedSyntax.StatementBlockOpen)
            .AppendLine(CommandBindingSyntax.ResetSerialDisposable)
            .AppendLine(CommandBindingSyntax.CommandMissingTest).AppendLine(CommandBindingSyntax.SubscriptionBlockOpen).Append("                    ").Append(controlAccess)
            .AppendLine(CommandBindingSyntax.DisableControl).AppendLine(CommandBindingSyntax.CommandMissingReturn).AppendLine(CommandBindingSyntax.SubscriptionBlockClose).AppendLine()
            .Append("                ").Append(controlAccess).AppendLine(".Enabled = cmd.CanExecute(null);")
            .AppendLine(CommandBindingSyntax.CanExecuteHandlerDeclaration).Append("                    ")
            .Append(controlAccess).AppendLine(".Enabled = cmd.CanExecute(null);")
            .AppendLine(CommandBindingSyntax.CanExecuteSubscribe).AppendLine().Append(CommandBindingSyntax.HandlerDeclarationOpen)
            .Append(CommandEventBindingEmitter.SenderType(supportsNullable)).Append(CommandBindingSyntax.HandlerSenderSeparator).Append(eventArgsType).AppendLine(" e)")
            .AppendLine(CommandBindingSyntax.SubscriptionBlockOpen).AppendLine("                    if (cmd.CanExecute(null))").AppendLine(CommandBindingSyntax.NestedBlockOpen)
            .AppendLine("                        cmd.Execute(null);").AppendLine(CommandBindingSyntax.NestedBlockClose).AppendLine(CommandBindingSyntax.SubscriptionBlockClose).AppendLine()
            .Append("                ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName).AppendLine(CommandBindingSyntax.HandlerSubscribe)
            .AppendLine(CommandBindingSyntax.SerialDisposableOpen)
            .AppendLine(CommandBindingSyntax.SubscriptionBlockOpen).Append("                    ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName)
            .AppendLine(CommandBindingSyntax.HandlerUnsubscribe).AppendLine(CommandBindingSyntax.CanExecuteUnsubscribe)
            .AppendLine(CommandBindingSyntax.SerialDisposableClose).AppendLine(CommandBindingSyntax.CommandSubscriptionClose)
            .AppendLine(CommandBindingSyntax.CommandOnlyDisposableReturn)
            .AppendLine(GeneratedSyntax.MemberBodyClose);
}

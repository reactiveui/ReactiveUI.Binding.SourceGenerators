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
            .AppendLine(CommandBindingSyntax.CommandMissingTest).AppendLine(CommandBindingSyntax.SubscriptionBlockOpen).AppendLine(CommandBindingSyntax.CommandMissingReturn)
            .AppendLine(CommandBindingSyntax.SubscriptionBlockClose).AppendLine().Append(CommandBindingSyntax.HandlerDeclarationOpen)
            .Append(CommandEventBindingEmitter.SenderType(supportsNullable)).Append(CommandBindingSyntax.HandlerSenderSeparator).Append(eventArgsType).AppendLine(" e)")
            .AppendLine(CommandBindingSyntax.SubscriptionBlockOpen)
            .Append("                    var param = ").Append(CommandBindingSyntax.ReadLatestParameter(inv)).AppendLine(";")
            .AppendLine("                    if (cmd.CanExecute(param))").AppendLine(CommandBindingSyntax.NestedBlockOpen)
            .AppendLine("                        cmd.Execute(param);").AppendLine(CommandBindingSyntax.NestedBlockClose).AppendLine(CommandBindingSyntax.SubscriptionBlockClose)
            .AppendLine().Append("                ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName).AppendLine(CommandBindingSyntax.HandlerSubscribe)
            .AppendLine(CommandBindingSyntax.SerialDisposableOpen)
            .Append("                    ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName).AppendLine(CommandBindingSyntax.HandlerUnsubscribeAndClose)
            .AppendLine(CommandBindingSyntax.CommandSubscriptionClose).AppendLine("            return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(")
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
            .AppendLine(CommandBindingSyntax.CommandMissingTest).AppendLine(CommandBindingSyntax.SubscriptionBlockOpen).AppendLine(CommandBindingSyntax.CommandMissingReturn)
            .AppendLine(CommandBindingSyntax.SubscriptionBlockClose).AppendLine().Append(CommandBindingSyntax.HandlerDeclarationOpen)
            .Append(CommandEventBindingEmitter.SenderType(supportsNullable)).Append(CommandBindingSyntax.HandlerSenderSeparator).Append(eventArgsType).AppendLine(" e)")
            .AppendLine(CommandBindingSyntax.SubscriptionBlockOpen).Append("                    var param = ").Append(paramAccess).AppendLine(";")
            .AppendLine("                    if (cmd.CanExecute(param))").AppendLine(CommandBindingSyntax.NestedBlockOpen)
            .AppendLine("                        cmd.Execute(param);").AppendLine(CommandBindingSyntax.NestedBlockClose).AppendLine(CommandBindingSyntax.SubscriptionBlockClose)
            .AppendLine().Append("                ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName).AppendLine(CommandBindingSyntax.HandlerSubscribe)
            .AppendLine(CommandBindingSyntax.SerialDisposableOpen)
            .Append("                    ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName).AppendLine(CommandBindingSyntax.HandlerUnsubscribeAndClose)
            .AppendLine(CommandBindingSyntax.CommandSubscriptionClose)
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
            .AppendLine(CommandBindingSyntax.CommandMissingTest).AppendLine(CommandBindingSyntax.SubscriptionBlockOpen).AppendLine(CommandBindingSyntax.CommandMissingReturn)
            .AppendLine(CommandBindingSyntax.SubscriptionBlockClose).AppendLine().Append(CommandBindingSyntax.HandlerDeclarationOpen)
            .Append(CommandEventBindingEmitter.SenderType(supportsNullable)).Append(CommandBindingSyntax.HandlerSenderSeparator).Append(eventArgsType).AppendLine(" e)")
            .AppendLine(CommandBindingSyntax.SubscriptionBlockOpen).AppendLine("                    if (cmd.CanExecute(null))").AppendLine(CommandBindingSyntax.NestedBlockOpen)
            .AppendLine("                        cmd.Execute(null);").AppendLine(CommandBindingSyntax.NestedBlockClose).AppendLine(CommandBindingSyntax.SubscriptionBlockClose).AppendLine()
            .Append("                ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName).AppendLine(CommandBindingSyntax.HandlerSubscribe)
            .AppendLine(CommandBindingSyntax.SerialDisposableOpen)
            .Append("                    ").Append(controlAccess).Append('.').Append(inv.ResolvedEventName).AppendLine(CommandBindingSyntax.HandlerUnsubscribeAndClose)
            .AppendLine(CommandBindingSyntax.CommandSubscriptionClose)
            .AppendLine(CommandBindingSyntax.CommandOnlyDisposableReturn)
            .AppendLine(GeneratedSyntax.MemberBodyClose);
}

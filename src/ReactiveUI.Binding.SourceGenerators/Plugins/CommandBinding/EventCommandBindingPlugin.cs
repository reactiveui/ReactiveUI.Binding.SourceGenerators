// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Base for the command binding plugins that drive a command from a control's event.</summary>
/// <remarks>
/// A derived plugin says which controls it claims and writes the three parameter-kind bodies. Resolving the
/// event-args type and choosing between those three is the same wherever the command comes from an event, so
/// the interface is implemented once here rather than repeated per plugin.
/// </remarks>
internal abstract class EventCommandBindingPlugin : ICommandBindingPlugin
{
    /// <inheritdoc/>
    public abstract int Affinity { get; }

    /// <inheritdoc/>
    public bool RequiresCustomBinderFallback => true;

    /// <inheritdoc/>
    public abstract bool CanHandle(BindCommandInvocationInfo inv);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitBinding(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        bool supportsNullable) => CommandEventBindingEmitter.EmitByParameterKind(
            sb,
            inv,
            controlAccess,
            supportsNullable,
            EmitWithObservableParameter,
            EmitWithExpressionParameter,
            EmitWithNoParameter);

    /// <summary>Appends the subscription that rebinds the control whenever the command property changes.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <remarks>
    /// The command is observed rather than read, so a view model that hands one over later - or swaps one out -
    /// rebinds the control. Everything the previous command attached is dropped first, and a binding with no
    /// command yet stays inert.
    /// </remarks>
    protected static void AppendCommandSubscription(StringBuilder sb) =>
        _ = sb.AppendLine(CommandBindingSyntax.SerialDisposableDeclaration)
            .AppendLine(CommandBindingSyntax.CommandSubscriptionOpen)
            .AppendLine(GeneratedSyntax.StatementBlockOpen)
            .AppendLine(CommandBindingSyntax.ResetSerialDisposable)
            .AppendLine(CommandBindingSyntax.CommandMissingTest)
            .AppendLine(CommandBindingSyntax.SubscriptionBlockOpen);

    /// <summary>Appends the capture of the latest parameter, and opens the command subscription over it.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types.</param>
    /// <remarks>
    /// A parameter stream and a command stream emit independently, so the parameter is held in a local the
    /// event handler reads at the moment it runs rather than captured when the command arrived.
    /// </remarks>
    protected static void AppendLatestParameterCapture(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        bool supportsNullable)
    {
        _ = sb.AppendLine().Append("            ").Append(inv.ParameterTypeFullName)
            .Append(supportsNullable && inv.ParameterIsReferenceType ? "?" : string.Empty).AppendLine(" __latestParam = default;")
            .AppendLine("            var __paramSub = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(")
            .Append("                withParameter, p => ").Append(CommandBindingSyntax.WriteLatestParameter(inv, "p")).AppendLine(");").AppendLine();

        AppendCommandSubscription(sb);
    }

    /// <summary>Appends the declaration of the handler the control's event runs the command from.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="eventArgsType">The event args type the control's event carries.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types.</param>
    protected static void AppendHandlerDeclaration(StringBuilder sb, string eventArgsType, bool supportsNullable) =>
        _ = sb.AppendLine().Append(CommandBindingSyntax.HandlerDeclarationOpen)
            .Append(CommandEventBindingEmitter.SenderType(supportsNullable)).Append(CommandBindingSyntax.HandlerSenderSeparator)
            .Append(eventArgsType).AppendLine(" e)").AppendLine(CommandBindingSyntax.SubscriptionBlockOpen);

    /// <summary>Appends the guarded run of the command inside the control's event handler.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="argument">The expression the command is asked about and run with.</param>
    protected static void AppendHandlerExecution(StringBuilder sb, string argument) =>
        _ = sb.Append("                    if (cmd.CanExecute(").Append(argument).AppendLine("))").AppendLine(CommandBindingSyntax.NestedBlockOpen)
            .Append("                        cmd.Execute(").Append(argument).AppendLine(");").AppendLine(CommandBindingSyntax.NestedBlockClose)
            .AppendLine(CommandBindingSyntax.SubscriptionBlockClose).AppendLine();

    /// <summary>Appends the return of the binding's own subscriptions when a parameter stream was subscribed to.</summary>
    /// <param name="sb">The string builder to append to.</param>
    protected static void AppendParameterisedDisposableReturn(StringBuilder sb) =>
        _ = sb.AppendLine("            return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(")
            .AppendLine("                new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(__cmdSub, __paramSub), serial);")
            .AppendLine(GeneratedSyntax.MemberBodyClose);

    /// <summary>Emits the binding when the command parameter arrives on an observable.</summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain.</param>
    /// <param name="eventArgsType">The event args type.</param>
    /// <param name="supportsNullable">There can be a null type.</param>
    protected abstract void EmitWithObservableParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        bool supportsNullable);

    /// <summary>Emits the binding when the command parameter comes from a property expression.</summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain.</param>
    /// <param name="eventArgsType">The event args type.</param>
    /// <param name="paramAccess">The parameter access chain.</param>
    /// <param name="supportsNullable">There can be a null type.</param>
    protected abstract void EmitWithExpressionParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        string paramAccess,
        bool supportsNullable);

    /// <summary>Emits the binding when the command takes no parameter.</summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain.</param>
    /// <param name="eventArgsType">The event args type.</param>
    /// <param name="supportsNullable">There can be a null type.</param>
    protected abstract void EmitWithNoParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        bool supportsNullable);
}

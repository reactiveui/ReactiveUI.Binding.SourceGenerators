// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Shares command-event subscription fragments across binding mechanisms.</summary>
internal static class EventCommandBindingEmitter
{
    /// <summary>Appends the subscription that rebinds the control whenever the command property changes.</summary>
    /// <param name="sb">The string builder to append to.</param>
    internal static void AppendCommandSubscription(StringBuilder sb) =>
        _ = sb.AppendLine(CommandBindingSyntax.SerialDisposableDeclaration)
            .AppendLine(CommandBindingSyntax.CommandSubscriptionOpen)
            .AppendLine(GeneratedSyntax.StatementBlockOpen)
            .AppendLine(CommandBindingSyntax.ResetSerialDisposable)
            .AppendLine(CommandBindingSyntax.CommandMissingTest)
            .AppendLine(CommandBindingSyntax.SubscriptionBlockOpen);

    /// <summary>Appends the capture of the latest parameter, and opens the command subscription over it.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    internal static void AppendLatestParameterCapture(
        StringBuilder sb,
        BindCommandInvocationInfo inv)
    {
        CommandParameterEmitter.EmitCapture(sb, inv);
        AppendCommandSubscription(sb);
    }

    /// <summary>Appends the declaration of the handler the control's event runs the command from.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="eventArgsType">The event args type the control's event carries.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types.</param>
    internal static void AppendHandlerDeclaration(StringBuilder sb, string eventArgsType, bool supportsNullable) =>
        _ = sb.AppendLine().Append(CommandBindingSyntax.HandlerDeclarationOpen)
            .Append(CommandEventBindingEmitter.SenderType(supportsNullable)).Append(CommandBindingSyntax.HandlerSenderSeparator)
            .Append(eventArgsType).AppendLine(" e)").AppendLine(CommandBindingSyntax.SubscriptionBlockOpen);

    /// <summary>Appends the guarded run of the command inside the control's event handler.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="argument">The expression the command is asked about and run with.</param>
    internal static void AppendHandlerExecution(StringBuilder sb, string argument) =>
        _ = sb.Append("                    if (cmd.CanExecute(").Append(argument).AppendLine("))").AppendLine(CommandBindingSyntax.NestedBlockOpen)
            .Append("                        cmd.Execute(").Append(argument).AppendLine(");").AppendLine(CommandBindingSyntax.NestedBlockClose)
            .AppendLine(CommandBindingSyntax.SubscriptionBlockClose).AppendLine();

    /// <summary>Appends the return of the binding's own subscriptions when a parameter stream was subscribed to.</summary>
    /// <param name="sb">The string builder to append to.</param>
    internal static void AppendParameterisedDisposableReturn(StringBuilder sb) =>
        _ = sb.AppendLine("            return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(")
            .AppendLine("                new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(__cmdSub, __paramSub), serial);")
            .AppendLine(GeneratedSyntax.MemberBodyClose);
}

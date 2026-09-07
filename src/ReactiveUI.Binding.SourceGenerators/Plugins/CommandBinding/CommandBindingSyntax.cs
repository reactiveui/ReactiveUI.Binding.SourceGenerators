// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>
/// The fixed fragments of a generated command binding, named once so every plugin that drives a command
/// from a control's event writes the same lines.
/// </summary>
/// <remarks>
/// The bindings differ in what supplies the command parameter and whether the control has an
/// <c>Enabled</c> property to keep in step; the subscription around them is the same in each. Every member
/// is a <c>const</c>, so referencing it costs nothing at run time.
/// </remarks>
internal static class CommandBindingSyntax
{
    /// <summary>Declares the disposable holding whatever the current command subscribed to.</summary>
    internal const string SerialDisposableDeclaration =
        "            var serial = new global::ReactiveUI.Primitives.Disposables.SwapDisposable();";

    /// <summary>Opens the subscription that rebinds whenever the command property changes.</summary>
    internal const string CommandSubscriptionOpen =
        "            var __cmdSub = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(commandObs, cmd =>";

    /// <summary>Closes the subscription that rebinds whenever the command property changes.</summary>
    internal const string CommandSubscriptionClose = "            });";

    /// <summary>Drops what the previous command subscribed to, before the new one binds.</summary>
    internal const string ResetSerialDisposable =
        "                serial.Disposable = global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;";

    /// <summary>Tests whether the view model has handed over a command at all.</summary>
    internal const string CommandMissingTest = "                if (cmd == null)";

    /// <summary>Leaves the binding inert until a command arrives.</summary>
    internal const string CommandMissingReturn = "                    return;";

    /// <summary>Opens a block inside the command subscription.</summary>
    internal const string SubscriptionBlockOpen = "                {";

    /// <summary>Closes a block inside the command subscription.</summary>
    internal const string SubscriptionBlockClose = "                }";

    /// <summary>Opens a block nested inside the command subscription.</summary>
    internal const string NestedBlockOpen = "                    {";

    /// <summary>Closes a block nested inside the command subscription.</summary>
    internal const string NestedBlockClose = "                    }";

    /// <summary>Opens the declaration of the handler the control's event runs the command from.</summary>
    internal const string HandlerDeclarationOpen = "                void __Handler(";

    /// <summary>Separates the handler's sender parameter from its event-args parameter.</summary>
    internal const string HandlerSenderSeparator = " sender, ";

    /// <summary>Subscribes the generated handler to the control's event.</summary>
    internal const string HandlerSubscribe = " += __Handler;";

    /// <summary>Unsubscribes the generated handler from the control's event.</summary>
    internal const string HandlerUnsubscribe = " -= __Handler;";

    /// <summary>Unsubscribes the generated handler and closes the single-statement dispose lambda.</summary>
    internal const string HandlerUnsubscribeAndClose = " -= __Handler);";

    /// <summary>Opens the disposable that detaches the generated handler.</summary>
    internal const string SerialDisposableOpen =
        "                serial.Disposable = new global::ReactiveUI.Primitives.Disposables.ActionDisposable(() =>";

    /// <summary>Closes the disposable that detaches the generated handler.</summary>
    internal const string SerialDisposableClose = "                });";

    /// <summary>Declares the handler that keeps the control's enabled state in step with the command.</summary>
    internal const string CanExecuteHandlerDeclaration =
        "                global::System.EventHandler __canExecHandler = (s, e) =>";

    /// <summary>Subscribes the enabled-state handler to the command.</summary>
    internal const string CanExecuteSubscribe = "                cmd.CanExecuteChanged += __canExecHandler;";

    /// <summary>Unsubscribes the enabled-state handler from the command.</summary>
    internal const string CanExecuteUnsubscribe = "                    cmd.CanExecuteChanged -= __canExecHandler;";

    /// <summary>Disables the control while it has no command to run.</summary>
    internal const string DisableControl = ".Enabled = false;";

    /// <summary>Returns the binding's own subscriptions when only the command was subscribed to.</summary>
    internal const string CommandOnlyDisposableReturn =
        "            return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(__cmdSub, serial);";

    /// <summary>Renders the write that records the parameter a later command emission will be given.</summary>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="valueExpression">The expression producing the value to record.</param>
    /// <returns>The rendered write, without a trailing semicolon.</returns>
    /// <remarks>
    /// The volatile write is what makes a parameter arriving on one thread visible to a command arriving on
    /// another, and it is only available for a reference type - <c>Volatile</c> offers no overload for an
    /// arbitrary value type, so a parameter such as a <c>Guid</c> would not compile. Those are recorded by a
    /// plain write, which is what the runtime engine does for every parameter it boxes.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string WriteLatestParameter(BindCommandInvocationInfo inv, string valueExpression) =>
        inv.ParameterIsReferenceType
            ? $"global::System.Threading.Volatile.Write(ref __latestParam, {valueExpression})"
            : $"__latestParam = {valueExpression}";

    /// <summary>Renders the read that recovers the parameter a command emission should be given.</summary>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <returns>The rendered read.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string ReadLatestParameter(BindCommandInvocationInfo inv) =>
        inv.ParameterIsReferenceType
            ? "global::System.Threading.Volatile.Read(ref __latestParam)"
            : "__latestParam";
}

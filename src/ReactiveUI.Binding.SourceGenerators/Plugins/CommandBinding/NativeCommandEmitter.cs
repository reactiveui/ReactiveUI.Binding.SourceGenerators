// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Emits native event and UIKit touch command bindings.</summary>
internal static class NativeCommandEmitter
{
    /// <summary>Opens the command's nested subscription body.</summary>
    private const string SubscriptionBlockOpen = "                {";

    /// <summary>The shared event implementation, called directly after native selection.</summary>
    private static readonly EventEnabledBindingPlugin EventBinding = new();

    /// <summary>Uses the verified native event instead of a generic default-event guess.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="inv">The binding with its native event.</param>
    /// <param name="controlAccess">The concrete control expression.</param>
    /// <param name="supportsNullable">Whether nullable annotations are supported.</param>
    internal static void EmitEvent(StringBuilder sb, BindCommandInvocationInfo inv, string controlAccess, bool supportsNullable)
    {
        var native = inv.NativeCommand!;
        EventBinding.EmitBinding(sb, inv with { ResolvedEventName = native.EventName, ResolvedEventArgsTypeFullName = native.EventArgsType }, controlAccess, supportsNullable);
    }

    /// <summary>Attaches UIKit's native touch target and keeps enabled state in step with the command.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="inv">The selected binding.</param>
    /// <param name="controlAccess">The concrete control expression.</param>
    internal static void EmitTouch(StringBuilder sb, BindCommandInvocationInfo inv, string controlAccess)
    {
        if (CommandParameterEmitter.HasParameter(inv))
        {
            CommandParameterEmitter.EmitCapture(sb, inv);
        }

        var parameter = CommandParameterEmitter.Read(inv);
        _ = sb.Append("            var __nativeControl = (global::UIKit.UIControl)").Append(controlAccess).AppendLine(";")
            .AppendLine("            var serial = new global::ReactiveUI.Primitives.Disposables.SwapDisposable();")
            .AppendLine("            var __cmdSub = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(commandObs, (global::System.Windows.Input.ICommand cmd) =>")
            .AppendLine("            {")
            .AppendLine("                serial.Disposable = global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;")
            .AppendLine("                if (cmd == null)")
            .AppendLine(SubscriptionBlockOpen)
            .AppendLine("                    __nativeControl.Enabled = false;")
            .AppendLine("                    return;")
            .AppendLine("                }")
            .AppendLine("                global::System.EventHandler __action = (__sender, __args) =>")
            .AppendLine(SubscriptionBlockOpen)
            .Append("                    var __parameter = (object)").Append(parameter).AppendLine(";")
            .AppendLine("                    if (cmd.CanExecute(__parameter))")
            .AppendLine("                    {")
            .AppendLine("                        cmd.Execute(__parameter);")
            .AppendLine("                    }")
            .AppendLine("                };")
            .Append("                global::System.EventHandler __enabled = (__sender, __args) => __nativeControl.Enabled = cmd.CanExecute(")
            .Append(parameter).AppendLine(");")
            .Append("                __nativeControl.Enabled = cmd.CanExecute(").Append(parameter).AppendLine(");")
            .AppendLine("                __nativeControl.AddTarget(__action, global::UIKit.UIControlEvent.TouchUpInside);")
            .AppendLine("                cmd.CanExecuteChanged += __enabled;")
            .AppendLine("                serial.Disposable = new global::ReactiveUI.Primitives.Disposables.ActionDisposable(() =>")
            .AppendLine(SubscriptionBlockOpen)
            .AppendLine("                    __nativeControl.RemoveTarget(__action, global::UIKit.UIControlEvent.TouchUpInside);")
            .AppendLine("                    cmd.CanExecuteChanged -= __enabled;")
            .AppendLine("                });")
            .AppendLine("            });");
        AppendReturn(sb, inv);
    }

    /// <summary>Returns the command, native-handler and optional parameter subscriptions.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="inv">The binding whose parameter stream may be present.</param>
    internal static void AppendReturn(StringBuilder sb, BindCommandInvocationInfo inv)
    {
        _ = sb.Append("            return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(__cmdSub, serial");
        if (CommandParameterEmitter.HasParameter(inv))
        {
            _ = sb.Append(", __paramSub");
        }

        _ = sb.AppendLine(");").AppendLine("        }");
    }
}

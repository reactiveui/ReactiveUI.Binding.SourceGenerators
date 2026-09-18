// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Emits Cocoa target/action routing with concrete setters and a native selector bridge.</summary>
internal static class AppKitCommandEmitter
{
    /// <summary>Opens the command's nested subscription body.</summary>
    private const string SubscriptionBlockOpen = "                {";

    /// <summary>Installs and removes the native target while commands and parameters change.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="inv">The verified native binding.</param>
    /// <param name="controlAccess">The concrete target expression.</param>
    internal static void EmitBinding(StringBuilder sb, BindCommandInvocationInfo inv, string controlAccess)
    {
        if (CommandParameterEmitter.HasParameter(inv))
        {
            CommandParameterEmitter.EmitCapture(sb, inv);
        }

        var native = inv.NativeCommand!;
        var parameter = CommandParameterEmitter.Read(inv);
        _ = sb.AppendLine("            var serial = new global::ReactiveUI.Primitives.Disposables.SwapDisposable();")
            .AppendLine("            var __cmdSub = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(commandObs, cmd =>")
            .AppendLine("            {")
            .AppendLine("                serial.Disposable = global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;")
            .AppendLine("                if (cmd == null)")
            .AppendLine(SubscriptionBlockOpen);
        if (native.HasEnabled)
        {
            _ = sb.Append("                    ").Append(controlAccess).AppendLine(".Enabled = false;");
        }

        _ = sb.AppendLine("                    return;").AppendLine("                }")
            .Append("                var __target = new __AppKitCommandTarget(cmd, () => ").Append(parameter).AppendLine(");");
        if (native.HasAction)
        {
            _ = sb.AppendLine("                var __selector = new global::ObjCRuntime.Selector(\"theAction:\");")
                .Append("                ").Append(controlAccess).AppendLine(".Action = __selector;");
        }

        _ = sb.Append("                ").Append(controlAccess).AppendLine(".Target = __target;");
        AppendEnabled(sb, native, controlAccess, parameter);
        AppendDetach(sb, native, controlAccess);
        NativeCommandEmitter.AppendReturn(sb, inv);
    }

    /// <summary>Declares the NSObject bridge required by the native selector contract.</summary>
    /// <param name="sb">The output builder.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void EmitHelper(StringBuilder sb) =>
        sb.AppendLine("""
                private sealed class __AppKitCommandTarget : global::Foundation.NSObject
                {
                    private readonly global::System.Windows.Input.ICommand _command;
                    private readonly global::System.Func<object> _parameter;
                    internal __AppKitCommandTarget(global::System.Windows.Input.ICommand command, global::System.Func<object> parameter)
                    {
                        _command = command;
                        _parameter = parameter;
                        IsEnabled = command.CanExecute(null);
                    }
                    internal bool IsEnabled { get; set; }
                    [global::Foundation.Export("theAction:")]
                    public void Execute(global::Foundation.NSObject sender)
                    {
                        var parameter = _parameter();
                        if (_command.CanExecute(parameter))
                        {
                            _command.Execute(parameter);
                        }
                    }
                    [global::Foundation.Export("validateMenuItem:")]
                    public bool ValidateMenuItem(global::AppKit.NSMenuItem item)
                    {
                        return IsEnabled;
                    }
                }
        """);

    /// <summary>Synchronizes the native enabled property and menu-validation result.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="native">The verified native capabilities.</param>
    /// <param name="control">The concrete control expression.</param>
    /// <param name="parameter">The current command argument expression.</param>
    private static void AppendEnabled(StringBuilder sb, NativeCommandInfo native, string control, string parameter)
    {
        if (!native.HasEnabled)
        {
            return;
        }

        _ = sb.AppendLine("                global::System.EventHandler __enabled = (__sender, __args) =>")
            .AppendLine(SubscriptionBlockOpen)
            .Append("                    __target.IsEnabled = cmd.CanExecute(").Append(parameter).AppendLine(");")
            .Append("                    ").Append(control).AppendLine(".Enabled = __target.IsEnabled;")
            .AppendLine("                };")
            .AppendLine("                __enabled(null, global::System.EventArgs.Empty);")
            .AppendLine("                cmd.CanExecuteChanged += __enabled;");
    }

    /// <summary>Clears native target/action references and releases the bridge and selector.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="native">The verified native capabilities.</param>
    /// <param name="control">The concrete control expression.</param>
    private static void AppendDetach(StringBuilder sb, NativeCommandInfo native, string control)
    {
        _ = sb.AppendLine("                serial.Disposable = new global::ReactiveUI.Primitives.Disposables.ActionDisposable(() =>")
            .AppendLine(SubscriptionBlockOpen)
            .Append("                    ").Append(control).AppendLine(".Target = null;");
        if (native.HasAction)
        {
            _ = sb.Append("                    ").Append(control).AppendLine(".Action = null;")
                .AppendLine("                    __selector.Dispose();");
        }

        if (native.HasEnabled)
        {
            _ = sb.AppendLine("                    cmd.CanExecuteChanged -= __enabled;");
        }

        _ = sb.AppendLine("                    __target.Dispose();")
            .AppendLine("                });")
            .AppendLine("            });");
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
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
        sb.AppendLine($$"""

                                    {{inv.ParameterTypeFullName}}{{(supportsNullable && inv.ParameterIsReferenceType ? "?" : string.Empty)}} __latestParam = default;
                                    var __paramSub = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(
                                        withParameter, p => System.Threading.Volatile.Write(ref __latestParam, p));

                                    var serial = new global::ReactiveUI.Primitives.Disposables.SwapDisposable();
                                    var __cmdSub = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(commandObs, cmd =>
                                    {
                                        serial.Disposable = global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;
                                        if (cmd == null)
                                        {
                                            {{controlAccess}}.Enabled = false;
                                            return;
                                        }

                                        var param = System.Threading.Volatile.Read(ref __latestParam);
                                        {{controlAccess}}.Enabled = cmd.CanExecute(param);
                                        global::System.EventHandler __canExecHandler = (s, e) =>
                                            {{controlAccess}}.Enabled = cmd.CanExecute(System.Threading.Volatile.Read(ref __latestParam));
                                        cmd.CanExecuteChanged += __canExecHandler;

                                        void __Handler({{CommandEventBindingEmitter.SenderType(supportsNullable)}} sender, {{eventArgsType}} e)
                                        {
                                            var p = System.Threading.Volatile.Read(ref __latestParam);
                                            if (cmd.CanExecute(p))
                                            {
                                                cmd.Execute(p);
                                            }
                                        }

                                        {{controlAccess}}.{{inv.ResolvedEventName}} += __Handler;
                                        serial.Disposable = new global::ReactiveUI.Primitives.Disposables.ActionDisposable(() =>
                                        {
                                            {{controlAccess}}.{{inv.ResolvedEventName}} -= __Handler;
                                            cmd.CanExecuteChanged -= __canExecHandler;
                                        });
                                    });
                                    return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(
                                        new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(__cmdSub, __paramSub), serial);
                                }
                        """);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void EmitWithExpressionParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        string paramAccess,
        bool supportsNullable) =>
        sb.AppendLine($$"""

                                    var serial = new global::ReactiveUI.Primitives.Disposables.SwapDisposable();
                                    var __cmdSub = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(commandObs, cmd =>
                                    {
                                        serial.Disposable = global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;
                                        if (cmd == null)
                                        {
                                            {{controlAccess}}.Enabled = false;
                                            return;
                                        }

                                        {{controlAccess}}.Enabled = cmd.CanExecute({{paramAccess}});
                                        global::System.EventHandler __canExecHandler = (s, e) =>
                                            {{controlAccess}}.Enabled = cmd.CanExecute({{paramAccess}});
                                        cmd.CanExecuteChanged += __canExecHandler;

                                        void __Handler({{CommandEventBindingEmitter.SenderType(supportsNullable)}} sender, {{eventArgsType}} e)
                                        {
                                            var param = {{paramAccess}};
                                            if (cmd.CanExecute(param))
                                            {
                                                cmd.Execute(param);
                                            }
                                        }

                                        {{controlAccess}}.{{inv.ResolvedEventName}} += __Handler;
                                        serial.Disposable = new global::ReactiveUI.Primitives.Disposables.ActionDisposable(() =>
                                        {
                                            {{controlAccess}}.{{inv.ResolvedEventName}} -= __Handler;
                                            cmd.CanExecuteChanged -= __canExecHandler;
                                        });
                                    });
                                    return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(__cmdSub, serial);
                                }
                        """);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void EmitWithNoParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        bool supportsNullable) =>
        sb.AppendLine($$"""

                                    var serial = new global::ReactiveUI.Primitives.Disposables.SwapDisposable();
                                    var __cmdSub = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(commandObs, cmd =>
                                    {
                                        serial.Disposable = global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;
                                        if (cmd == null)
                                        {
                                            {{controlAccess}}.Enabled = false;
                                            return;
                                        }

                                        {{controlAccess}}.Enabled = cmd.CanExecute(null);
                                        global::System.EventHandler __canExecHandler = (s, e) =>
                                            {{controlAccess}}.Enabled = cmd.CanExecute(null);
                                        cmd.CanExecuteChanged += __canExecHandler;

                                        void __Handler({{CommandEventBindingEmitter.SenderType(supportsNullable)}} sender, {{eventArgsType}} e)
                                        {
                                            if (cmd.CanExecute(null))
                                            {
                                                cmd.Execute(null);
                                            }
                                        }

                                        {{controlAccess}}.{{inv.ResolvedEventName}} += __Handler;
                                        serial.Disposable = new global::ReactiveUI.Primitives.Disposables.ActionDisposable(() =>
                                        {
                                            {{controlAccess}}.{{inv.ResolvedEventName}} -= __Handler;
                                            cmd.CanExecuteChanged -= __canExecHandler;
                                        });
                                    });
                                    return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(__cmdSub, serial);
                                }
                        """);
}

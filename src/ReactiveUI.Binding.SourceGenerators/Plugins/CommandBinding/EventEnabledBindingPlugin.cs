// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
internal sealed class EventEnabledBindingPlugin : ICommandBindingPlugin
{
    /// <summary>
    /// The affinity score for the event + Enabled binder.
    /// </summary>
    private static readonly int EventEnabledAffinity = BindingAffinity.EventEnabledControl;

    /// <inheritdoc/>
    public int Affinity => EventEnabledAffinity;

    /// <inheritdoc/>
    public bool RequiresCustomBinderFallback => true;

    /// <inheritdoc/>
    public bool CanHandle(BindCommandInvocationInfo inv)
        => inv.ResolvedEventName != null && inv.HasEnabledProperty;

    /// <inheritdoc/>
    public void EmitBinding(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        bool supportsNullable)
    {
        CommandEventBindingEmitter.EmitByParameterKind(
            sb,
            inv,
            controlAccess,
            supportsNullable,
            EmitWithObservableParameter,
            EmitWithExpressionParameter,
            EmitWithNoParameter);
    }

    /// <summary>
    /// Emits event + Enabled binding with an observable parameter.
    /// </summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain.</param>
    /// <param name="eventArgsType">The event args type.</param>
    /// <param name="supportsNullable">There can be a null type.</param>
    private static void EmitWithObservableParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        bool supportsNullable) =>
        sb.AppendLine($$"""

                                    {{inv.ParameterTypeFullName}}{{(supportsNullable && inv.ParameterIsReferenceType ? "?" : string.Empty)}} __latestParam = default;
                                    var __paramSub = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(
                                        withParameter, p => System.Threading.Volatile.Write(ref __latestParam, p));

                                    var serial = new global::ReactiveUI.Binding.Observables.SerialDisposable();
                                    var __cmdSub = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(commandObs, cmd =>
                                    {
                                        serial.Disposable = global::ReactiveUI.Binding.Observables.EmptyDisposable.Instance;
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
                                        serial.Disposable = new global::ReactiveUI.Binding.Observables.ActionDisposable(() =>
                                        {
                                            {{controlAccess}}.{{inv.ResolvedEventName}} -= __Handler;
                                            cmd.CanExecuteChanged -= __canExecHandler;
                                        });
                                    });
                                    return new global::ReactiveUI.Binding.Observables.CompositeDisposable2(
                                        new global::ReactiveUI.Binding.Observables.CompositeDisposable2(__cmdSub, __paramSub), serial);
                                }
                        """);

    /// <summary>
    /// Emits event + Enabled binding with an expression parameter.
    /// </summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain.</param>
    /// <param name="eventArgsType">The event args type.</param>
    /// <param name="paramAccess">The parameter access chain.</param>
    /// <param name="supportsNullable">There can be a null type.</param>
    private static void EmitWithExpressionParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        string paramAccess,
        bool supportsNullable) =>
        sb.AppendLine($$"""

                                    var serial = new global::ReactiveUI.Binding.Observables.SerialDisposable();
                                    var __cmdSub = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(commandObs, cmd =>
                                    {
                                        serial.Disposable = global::ReactiveUI.Binding.Observables.EmptyDisposable.Instance;
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
                                        serial.Disposable = new global::ReactiveUI.Binding.Observables.ActionDisposable(() =>
                                        {
                                            {{controlAccess}}.{{inv.ResolvedEventName}} -= __Handler;
                                            cmd.CanExecuteChanged -= __canExecHandler;
                                        });
                                    });
                                    return new global::ReactiveUI.Binding.Observables.CompositeDisposable2(__cmdSub, serial);
                                }
                        """);

    /// <summary>
    /// Emits event + Enabled binding with no parameter.
    /// </summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain.</param>
    /// <param name="eventArgsType">The event args type.</param>
    /// <param name="supportsNullable">There can be a null type.</param>
    private static void EmitWithNoParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        bool supportsNullable) =>
        sb.AppendLine($$"""

                                    var serial = new global::ReactiveUI.Binding.Observables.SerialDisposable();
                                    var __cmdSub = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(commandObs, cmd =>
                                    {
                                        serial.Disposable = global::ReactiveUI.Binding.Observables.EmptyDisposable.Instance;
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
                                        serial.Disposable = new global::ReactiveUI.Binding.Observables.ActionDisposable(() =>
                                        {
                                            {{controlAccess}}.{{inv.ResolvedEventName}} -= __Handler;
                                            cmd.CanExecuteChanged -= __canExecHandler;
                                        });
                                    });
                                    return new global::ReactiveUI.Binding.Observables.CompositeDisposable2(__cmdSub, serial);
                                }
                        """);
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
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
internal sealed class DefaultEventBindingPlugin : ICommandBindingPlugin
{
    /// <summary>
    /// The affinity score for the default-event binder (lowest priority among command binding plugins).
    /// </summary>
    private static readonly int DefaultEventAffinity = BindingAffinity.DefaultEvent;

    /// <inheritdoc/>
    public int Affinity => DefaultEventAffinity;

    /// <inheritdoc/>
    public bool RequiresCustomBinderFallback => true;

    /// <inheritdoc/>
    public bool CanHandle(BindCommandInvocationInfo inv)
        => inv.ResolvedEventName != null;

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
    /// Emits event-only command binding with an observable parameter.
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
                                                return;
                                            }

                                            void __Handler({{CommandEventBindingEmitter.SenderType(supportsNullable)}} sender, {{eventArgsType}} e)
                                            {
                                                var param = System.Threading.Volatile.Read(ref __latestParam);
                                                if (cmd.CanExecute(param))
                                                {
                                                    cmd.Execute(param);
                                                }
                                            }

                                            {{controlAccess}}.{{inv.ResolvedEventName}} += __Handler;
                                            serial.Disposable = new global::ReactiveUI.Binding.Observables.ActionDisposable(() =>
                                                {{controlAccess}}.{{inv.ResolvedEventName}} -= __Handler);
                                        });
                                        return new global::ReactiveUI.Binding.Observables.CompositeDisposable2(
                                            new global::ReactiveUI.Binding.Observables.CompositeDisposable2(__cmdSub, __paramSub), serial);
                                    }
                            """);

    /// <summary>
    /// Emits event-only command binding with an expression parameter.
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
                                                return;
                                            }

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
                                                {{controlAccess}}.{{inv.ResolvedEventName}} -= __Handler);
                                        });
                                        return new global::ReactiveUI.Binding.Observables.CompositeDisposable2(__cmdSub, serial);
                                    }
                            """);

    /// <summary>
    /// Emits event-only command binding with no parameter.
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
                                                return;
                                            }

                                            void __Handler({{CommandEventBindingEmitter.SenderType(supportsNullable)}} sender, {{eventArgsType}} e)
                                            {
                                                if (cmd.CanExecute(null))
                                                {
                                                    cmd.Execute(null);
                                                }
                                            }

                                            {{controlAccess}}.{{inv.ResolvedEventName}} += __Handler;
                                            serial.Disposable = new global::ReactiveUI.Binding.Observables.ActionDisposable(() =>
                                                {{controlAccess}}.{{inv.ResolvedEventName}} -= __Handler);
                                        });
                                        return new global::ReactiveUI.Binding.Observables.CompositeDisposable2(__cmdSub, serial);
                                    }
                            """);
}

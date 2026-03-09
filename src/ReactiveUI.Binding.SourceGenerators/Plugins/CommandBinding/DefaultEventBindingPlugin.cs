// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
internal sealed class DefaultEventBindingPlugin : ICommandBindingPlugin
{
    /// <inheritdoc/>
    public int Affinity => 3;

    /// <inheritdoc/>
    public bool RequiresCustomBinderFallback => true;

    /// <inheritdoc/>
    public bool CanHandle(BindCommandInvocationInfo inv)
        => inv.ResolvedEventName != null;

    /// <inheritdoc/>
    public void EmitBinding(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess)
    {
        var eventArgsType = inv.ResolvedEventArgsTypeFullName ?? "global::System.EventArgs";

        if (inv.HasObservableParameter)
        {
            sb.AppendLine($$"""

                        {{inv.ParameterTypeFullName}} __latestParam = default;
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

                            void __Handler(object sender, {{eventArgsType}} e)
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
        }
        else if (inv is { HasExpressionParameter: true, ParameterPropertyPath: not null })
        {
            var paramAccess = CodeGeneratorHelpers.BuildPropertyAccessChain("viewModel", inv.ParameterPropertyPath.Value);
            sb.AppendLine($$"""

                        var serial = new global::ReactiveUI.Binding.Observables.SerialDisposable();
                        var __cmdSub = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(commandObs, cmd =>
                        {
                            serial.Disposable = global::ReactiveUI.Binding.Observables.EmptyDisposable.Instance;
                            if (cmd == null)
                            {
                                return;
                            }

                            void __Handler(object sender, {{eventArgsType}} e)
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
        }
        else
        {
            sb.AppendLine($$"""

                        var serial = new global::ReactiveUI.Binding.Observables.SerialDisposable();
                        var __cmdSub = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(commandObs, cmd =>
                        {
                            serial.Disposable = global::ReactiveUI.Binding.Observables.EmptyDisposable.Instance;
                            if (cmd == null)
                            {
                                return;
                            }

                            void __Handler(object sender, {{eventArgsType}} e)
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
    }
}

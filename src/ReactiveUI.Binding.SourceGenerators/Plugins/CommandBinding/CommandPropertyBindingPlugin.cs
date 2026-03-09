// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>
/// Command binding plugin for controls that have a <c>Command</c> property (ICommand)
/// and optionally a <c>CommandParameter</c> property.
/// Replaces the runtime <c>CreatesCommandBindingViaCommandParameter</c> binder.
/// Affinity 5 (highest priority).
/// </summary>
/// <remarks>
/// Platforms covered: WPF Button, WinUI Button, MAUI Button, and any control
/// with Command + CommandParameter properties.
/// No Enabled synchronization is needed because these frameworks handle it
/// internally through the Command property binding.
/// </remarks>
internal sealed class CommandPropertyBindingPlugin : ICommandBindingPlugin
{
    /// <inheritdoc/>
    public int Affinity => 5;

    /// <inheritdoc/>
    public bool RequiresCustomBinderFallback => false;

    /// <inheritdoc/>
    public bool CanHandle(BindCommandInvocationInfo inv)
        => inv.HasCommandProperty;

    /// <inheritdoc/>
    public void EmitBinding(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess)
    {
        if (inv.HasCommandParameterProperty && inv.HasObservableParameter)
        {
            // Command + CommandParameter + observable parameter variant
            sb.AppendLine($$"""

                        {{inv.ParameterTypeFullName}} __latestParam = default;
                        var __paramSub = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(
                            withParameter, p => System.Threading.Volatile.Write(ref __latestParam, p));

                        var serial = new global::ReactiveUI.Binding.Observables.SerialDisposable();
                        var __cmdSub = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(commandObs, cmd =>
                        {
                            serial.Disposable = global::ReactiveUI.Binding.Observables.EmptyDisposable.Instance;
                            {{controlAccess}}.Command = cmd;
                            var param = System.Threading.Volatile.Read(ref __latestParam);
                            {{controlAccess}}.CommandParameter = param;
                            if (cmd != null)
                            {
                                serial.Disposable = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(
                                    withParameter, p =>
                                    {
                                        System.Threading.Volatile.Write(ref __latestParam, p);
                                        {{controlAccess}}.CommandParameter = p;
                                    });
                            }
                        });
                        return new global::ReactiveUI.Binding.Observables.CompositeDisposable2(
                            new global::ReactiveUI.Binding.Observables.CompositeDisposable2(__cmdSub, __paramSub), serial);
                    }
            """);
        }
        else if (inv.HasCommandParameterProperty
            && inv is { HasExpressionParameter: true, ParameterPropertyPath: not null })
        {
            // Command + CommandParameter + expression parameter variant
            var paramAccess = CodeGeneratorHelpers.BuildPropertyAccessChain("viewModel", inv.ParameterPropertyPath.Value);
            sb.AppendLine($$"""

                        var serial = new global::ReactiveUI.Binding.Observables.SerialDisposable();
                        var __cmdSub = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(commandObs, cmd =>
                        {
                            serial.Disposable = global::ReactiveUI.Binding.Observables.EmptyDisposable.Instance;
                            {{controlAccess}}.Command = cmd;
                            {{controlAccess}}.CommandParameter = {{paramAccess}};
                        });
                        return new global::ReactiveUI.Binding.Observables.CompositeDisposable2(__cmdSub, serial);
                    }
            """);
        }
        else
        {
            // Command only (no parameter, or no CommandParameter property)
            sb.AppendLine($$"""

                        var serial = new global::ReactiveUI.Binding.Observables.SerialDisposable();
                        var __cmdSub = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(commandObs, cmd =>
                        {
                            serial.Disposable = global::ReactiveUI.Binding.Observables.EmptyDisposable.Instance;
                            {{controlAccess}}.Command = cmd;
                        });
                        return new global::ReactiveUI.Binding.Observables.CompositeDisposable2(__cmdSub, serial);
                    }
            """);
        }
    }
}

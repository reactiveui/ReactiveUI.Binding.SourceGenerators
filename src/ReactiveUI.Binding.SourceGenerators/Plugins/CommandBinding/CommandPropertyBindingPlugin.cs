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
    /// <summary>The affinity score for the Command property binder (highest priority among command binding plugins).</summary>
    private static readonly int CommandPropertyAffinity = BindingAffinity.Explicit;

    /// <inheritdoc/>
    public int Affinity => CommandPropertyAffinity;

    /// <inheritdoc/>
    public bool RequiresCustomBinderFallback => false;

    /// <inheritdoc/>
    public bool CanHandle(BindCommandInvocationInfo inv) =>
        inv.HasCommandProperty;

    /// <inheritdoc/>
    public void EmitBinding(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        bool supportsNullable)
    {
        if (inv.HasCommandParameterProperty && inv.HasObservableParameter)
        {
            EmitObservableParameterBinding(sb, inv, controlAccess, supportsNullable);
        }
        else if (inv.HasCommandParameterProperty
                 && inv is { HasExpressionParameter: true, ParameterPropertyPath: not null })
        {
            var paramAccess =
                CodeGeneratorHelpers.BuildPropertyAccessChain("viewModel", inv.ParameterPropertyPath.Value);
            _ = sb.AppendLine($$"""

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
            _ = sb.AppendLine($$"""

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

    /// <summary>
    /// Emits the binding for the variant that has both a CommandParameter property and an observable
    /// parameter, which must track the latest parameter value and re-subscribe per command.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The access chain to the bound control.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    private static void EmitObservableParameterBinding(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        bool supportsNullable) =>
        _ = sb.AppendLine($$"""

                                        {{inv.ParameterTypeFullName}}{{(supportsNullable && inv.ParameterIsReferenceType ? "?" : string.Empty)}} __latestParam = default;
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

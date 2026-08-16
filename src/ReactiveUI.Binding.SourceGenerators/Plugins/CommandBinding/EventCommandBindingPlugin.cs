// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Base for the command binding plugins that drive a command from a control's event.</summary>
/// <remarks>
/// A derived plugin says which controls it claims and writes the three parameter-kind bodies. Resolving the
/// event-args type and choosing between those three is the same wherever the command comes from an event, so
/// the interface is implemented once here rather than repeated per plugin.
/// </remarks>
internal abstract class EventCommandBindingPlugin : ICommandBindingPlugin
{
    /// <inheritdoc/>
    public abstract int Affinity { get; }

    /// <inheritdoc/>
    public bool RequiresCustomBinderFallback => true;

    /// <inheritdoc/>
    public abstract bool CanHandle(BindCommandInvocationInfo inv);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitBinding(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        bool supportsNullable) => CommandEventBindingEmitter.EmitByParameterKind(
            sb,
            inv,
            controlAccess,
            supportsNullable,
            EmitWithObservableParameter,
            EmitWithExpressionParameter,
            EmitWithNoParameter);

    /// <summary>Emits the binding when the command parameter arrives on an observable.</summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain.</param>
    /// <param name="eventArgsType">The event args type.</param>
    /// <param name="supportsNullable">There can be a null type.</param>
    protected abstract void EmitWithObservableParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        bool supportsNullable);

    /// <summary>Emits the binding when the command parameter comes from a property expression.</summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain.</param>
    /// <param name="eventArgsType">The event args type.</param>
    /// <param name="paramAccess">The parameter access chain.</param>
    /// <param name="supportsNullable">There can be a null type.</param>
    protected abstract void EmitWithExpressionParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        string paramAccess,
        bool supportsNullable);

    /// <summary>Emits the binding when the command takes no parameter.</summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain.</param>
    /// <param name="eventArgsType">The event args type.</param>
    /// <param name="supportsNullable">There can be a null type.</param>
    protected abstract void EmitWithNoParameter(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        string eventArgsType,
        bool supportsNullable);
}

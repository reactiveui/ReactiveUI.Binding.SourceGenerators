// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// Base for the observation plugins that watch a type through
/// <see cref="System.ComponentModel.INotifyPropertyChanged"/> and
/// <see cref="System.ComponentModel.INotifyPropertyChanging"/>.
/// </summary>
/// <remarks>
/// A derived plugin says only which types it claims and how strongly. Everything it emits is the same,
/// so the interface is implemented once here rather than repeated per plugin.
/// </remarks>
internal abstract class NotifyPropertyObservationPlugin : IObservationPlugin
{
    /// <inheritdoc/>
    public abstract int Affinity { get; }

    /// <inheritdoc/>
    public abstract string ObservationKind { get; }

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => true;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => false;

    /// <inheritdoc/>
    public abstract bool IsAMatch(ClassBindingInfo classInfo);

    /// <inheritdoc/>
    /// <remarks>
    /// The notification interfaces carry the property name in the event they raise rather than declaring
    /// anything per property, so every property of a type that implements one is reachable.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) => true;

    /// <inheritdoc/>
    public void EmitHelperClasses(StringBuilder sb)
    {
        // Nothing to declare - the observables come from the runtime library.
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitShallowObservation(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        bool includeStartWith) =>
        NotifyPropertyEmitter.EmitShallowObservation(sb, rootVar, segment, castTypeName, isBeforeChange, includeStartWith);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitShallowObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string varName) =>
        NotifyPropertyEmitter.EmitShallowObservationVariable(sb, rootVar, segment, castTypeName, isBeforeChange, varName);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitDeepChainRootSegment(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string obsVarName) =>
        NotifyPropertyEmitter.EmitDeepChainRootSegment(sb, rootVar, segment, castTypeName, isBeforeChange, obsVarName);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitDeepChainInnerSegment(
        StringBuilder sb,
        string prevVar,
        string curVar,
        string lambdaParam,
        PropertyPathSegment segment,
        bool isBeforeChange,
        NullParentObservationBehavior nullParentBehavior) =>
        NotifyPropertyEmitter.EmitDeepChainInnerSegment(sb, prevVar, curVar, lambdaParam, segment, isBeforeChange, nullParentBehavior);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitInlineObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string varName) =>
        NotifyPropertyEmitter.EmitInlineObservationVariable(sb, rootVar, segment, castTypeName, varName);
}

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>The base for observation plugins that watch a type through its property change notifications.</summary>
internal closed class NotifyPropertyObservationPlugin : IObservationPlugin
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
        NotifyPropertyEmitter.EmitDeepChainInnerSegment(sb, new(prevVar, curVar, lambdaParam), segment, isBeforeChange, nullParentBehavior, Affinity);

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

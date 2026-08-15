// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// Observation plugin for types implementing <see cref="System.ComponentModel.INotifyPropertyChanged"/>.
/// Affinity: 5 (matches ReactiveUI's INPCObservableForProperty).
/// Supports both after-change and before-change (if type also implements INotifyPropertyChanging).
/// Generates <c>PropertyObservable</c> / <c>PropertyChangingObservable</c> from the runtime library.
/// </summary>
internal sealed class INPCObservationPlugin : IObservationPlugin
{
    /// <summary>The affinity score for the INotifyPropertyChanged observation plugin (matches ReactiveUI's INPCObservableForProperty).</summary>
    private static readonly int INPCAffinity = BindingAffinity.Explicit;

    /// <inheritdoc/>
    public int Affinity => INPCAffinity;

    /// <inheritdoc/>
    public string ObservationKind => "INPC";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => true;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => false;

    /// <inheritdoc/>
    public bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.ImplementsINPC && !classInfo.ImplementsIReactiveObject;

    /// <inheritdoc/>
    public void EmitHelperClasses(StringBuilder sb)
    {
        // No helper classes needed - uses PropertyObservable/PropertyChangingObservable from runtime library.
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

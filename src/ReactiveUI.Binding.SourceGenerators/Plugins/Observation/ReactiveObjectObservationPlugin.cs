// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// Observation plugin for types implementing <c>IReactiveObject</c>.
/// Affinity: 10 (matches ReactiveUI's IROObservableForProperty).
/// Supports both after-change and before-change notifications.
/// IReactiveObject implements INPC/INPChanging, so this plugin correctly
/// emits <c>PropertyObservable</c> / <c>PropertyChangingObservable</c>.
/// </summary>
internal sealed class ReactiveObjectObservationPlugin : IObservationPlugin
{
    /// <summary>The affinity score for the IReactiveObject observation plugin (matches ReactiveUI's IROObservableForProperty).</summary>
    private static readonly int ReactiveObjectAffinity = BindingAffinity.ExactType;

    /// <inheritdoc/>
    public int Affinity => ReactiveObjectAffinity;

    /// <inheritdoc/>
    public string ObservationKind => "ReactiveObject";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => true;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => false;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.ImplementsIReactiveObject;

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

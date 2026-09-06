// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// Observation plugin for Android <c>View</c> types.
/// Affinity: 5 (matches ReactiveUI's AndroidObservableForWidgets).
/// Does NOT support before-change notifications.
/// </summary>
/// <remarks>
/// <para>
/// Android <c>View</c> does NOT implement <c>INotifyPropertyChanged</c>.
/// ReactiveUI's runtime uses a static dispatch table mapping (WidgetType, PropertyName)
/// to widget-specific events (e.g., <c>TextView.TextChanged</c>,
/// <c>CompoundButton.CheckedChange</c>, <c>RatingBar.RatingBarChange</c>).
/// </para>
/// <para>
/// Currently emits <c>ImmediateReturnSignal</c> (returns current value, no ongoing observation)
/// as a safe fallback. This matches ReactiveUI's POCO fallback behavior for unknown
/// widget/property combinations.
/// </para>
/// <para>
/// Future enhancement: Build a compile-time dispatch table matching ReactiveUI's
/// <c>AndroidObservableForWidgets</c> to generate direct widget event subscriptions.
/// Supported mappings would include: TextView.Text → TextChanged,
/// CompoundButton.Checked → CheckedChange, NumberPicker.Value → ValueChanged, etc.
/// </para>
/// </remarks>
internal sealed class AndroidObservationPlugin : IObservationPlugin
{
    /// <summary>The affinity score for the Android View observation plugin (matches ReactiveUI's AndroidObservableForWidgets).</summary>
    private static readonly int AndroidAffinity = BindingAffinity.Explicit;

    /// <inheritdoc/>
    public int Affinity => AndroidAffinity;

    /// <inheritdoc/>
    public string ObservationKind => "Android";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => false;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => false;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.InheritsAndroidView;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) => true;

    /// <inheritdoc/>
    public void EmitHelperClasses(StringBuilder sb)
    {
        // No helper classes needed. Future: may emit event-based observable.
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
        // Android View does not implement INPC. Emit ImmediateReturnSignal as POCO fallback.
        // Returns the current property value once, no ongoing observation.
        sb.Append(
            $"new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{segment.PropertyTypeFullName}>((({castTypeName}){rootVar}).{segment.PropertyName})");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitShallowObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string varName) =>
        sb.Append(
            $"            var {varName} = new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{segment.PropertyTypeFullName}>((({castTypeName}){rootVar}).{segment.PropertyName});");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitDeepChainRootSegment(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string obsVarName) =>
        sb
            .Append($"            var {obsVarName} = (global::System.IObservable<{segment.PropertyTypeFullName}>")
            .AppendLine($")new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{segment.PropertyTypeFullName}>((({castTypeName}){rootVar}).{segment.PropertyName});");

    /// <inheritdoc/>
    public void EmitDeepChainInnerSegment(
        StringBuilder sb,
        string prevVar,
        string curVar,
        string lambdaParam,
        PropertyPathSegment segment,
        bool isBeforeChange,
        NullParentObservationBehavior nullParentBehavior)
    {
        var segType = segment.PropertyTypeFullName;
        var declType = segment.DeclaringTypeFullName;
        var nullParentObservable = nullParentBehavior == NullParentObservationBehavior.EmitDefault
            ? $"new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{segType}>(default({segType}))"
            : $"global::ReactiveUI.Primitives.Advanced.ImmutableEmptySignal<{segType}>.Instance";

        _ = sb.AppendLine()
            .AppendLine($"""
                                 var {curVar} = {GeneratedTypeNames.OpenChainSwitchMap(segment, segType, prevVar)}
                                     {lambdaParam} => {lambdaParam} != null
                                         ? (global::System.IObservable<{segType}>)
                                             new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{segType}>((({declType}){lambdaParam}).{segment.PropertyName})
                                         : (global::System.IObservable<{segType}>){nullParentObservable});
                         """);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitInlineObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string varName) =>
        sb.AppendLine(
            $"            var {varName} = new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{segment.PropertyTypeFullName}>((({castTypeName}){rootVar}).{segment.PropertyName});");
}

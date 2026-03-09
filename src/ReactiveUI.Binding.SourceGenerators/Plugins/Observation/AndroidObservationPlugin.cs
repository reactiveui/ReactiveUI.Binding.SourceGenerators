// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

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
/// Currently emits <c>ReturnObservable</c> (returns current value, no ongoing observation)
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
    /// <inheritdoc/>
    public int Affinity => 5;

    /// <inheritdoc/>
    public string ObservationKind => "Android";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => false;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => false;

    /// <inheritdoc/>
    public bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.InheritsAndroidView;

    /// <inheritdoc/>
    public void EmitHelperClasses(StringBuilder sb)
    {
        // No helper classes needed. Future: may emit event-based observable.
    }

    /// <inheritdoc/>
    public void EmitShallowObservation(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        bool includeStartWith)
    {
        // Android View does not implement INPC. Emit ReturnObservable as POCO fallback.
        // Returns the current property value once, no ongoing observation.
        sb.Append($"new global::ReactiveUI.Binding.Observables.ReturnObservable<{segment.PropertyTypeFullName}>((({castTypeName}){rootVar}).{segment.PropertyName})");
    }

    /// <inheritdoc/>
    public void EmitShallowObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string varName)
    {
        sb.Append($"            var {varName} = new global::ReactiveUI.Binding.Observables.ReturnObservable<{segment.PropertyTypeFullName}>((({castTypeName}){rootVar}).{segment.PropertyName});");
    }

    /// <inheritdoc/>
    public void EmitDeepChainRootSegment(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string obsVarName)
    {
        sb.AppendLine($"            var {obsVarName} = (global::System.IObservable<{segment.PropertyTypeFullName}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segment.PropertyTypeFullName}>((({castTypeName}){rootVar}).{segment.PropertyName});");
    }

    /// <inheritdoc/>
    public void EmitDeepChainInnerSegment(
        StringBuilder sb,
        string prevVar,
        string curVar,
        string lambdaParam,
        PropertyPathSegment segment,
        bool isBeforeChange)
    {
        var segType = segment.PropertyTypeFullName;
        var declType = segment.DeclaringTypeFullName;

        sb.AppendLine()
            .AppendLine($"""
                            var {curVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevVar},
                                    {lambdaParam} => (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(
                                        {lambdaParam} != null ? (({declType}){lambdaParam}).{segment.PropertyName} : default({segType}))));
                    """);
    }

    /// <inheritdoc/>
    public void EmitInlineObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string varName)
    {
        sb.AppendLine($"            var {varName} = new global::ReactiveUI.Binding.Observables.ReturnObservable<{segment.PropertyTypeFullName}>((({castTypeName}){rootVar}).{segment.PropertyName});");
    }
}

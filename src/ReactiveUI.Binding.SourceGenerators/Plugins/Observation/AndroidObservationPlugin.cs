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
    /// <remarks>
    /// Only the widget properties that raise an event of their own. Everything else on an Android view changes
    /// silently, so claiming it would replace a mechanism the type may genuinely carry with one that reports
    /// nothing. A property the consumer declared on its own subclass is its own, not the widget's.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) =>
        AndroidWidgetEvents.FindChangeEvent(propertyName) is not null
        && !ObservedProperties.IsDeclaredByConsumer(classInfo, propertyName);

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
        var changeEvent = ChangeEventOrUnchanging(sb, rootVar, segment, castTypeName, isBeforeChange);
        if (changeEvent is null)
        {
            return;
        }

        _ = EventObservationEmitter.AppendExpression(
            sb,
            rootVar,
            segment,
            castTypeName,
            changeEvent,
            includeStartWith);
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
        var changeEvent = AndroidWidgetEvents.FindChangeEvent(segment.PropertyName);
        if (isBeforeChange || changeEvent is null)
        {
            _ = UnchangingObservationEmitter.AppendVariable(sb, rootVar, segment, castTypeName, varName);
            return;
        }

        _ = EventObservationEmitter.AppendVariable(sb, rootVar, segment, castTypeName, changeEvent, varName);
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
        var changeEvent = AndroidWidgetEvents.FindChangeEvent(segment.PropertyName);
        if (isBeforeChange || changeEvent is null)
        {
            _ = UnchangingObservationEmitter.AppendTypedVariable(sb, rootVar, segment, castTypeName, obsVarName)
                .AppendLine();
            return;
        }

        _ = sb.Append("            var ").Append(obsVarName)
            .Append(" = (global::System.IObservable<").Append(segment.PropertyTypeFullName).Append(">)");
        _ = EventObservationEmitter
            .AppendExpression(sb, rootVar, segment, castTypeName, changeEvent, true)
            .AppendLine(";");
    }

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
    public void EmitInlineObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string varName)
    {
        var changeEvent = AndroidWidgetEvents.FindChangeEvent(segment.PropertyName);

        _ = changeEvent is null
            ? UnchangingObservationEmitter.AppendVariable(sb, rootVar, segment, castTypeName, varName)
            : EventObservationEmitter.AppendVariable(sb, rootVar, segment, castTypeName, changeEvent, varName);

        _ = sb.AppendLine();
    }

    /// <summary>Resolves the event the property reports on, falling back to the unchanging observation.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable the property is read from.</param>
    /// <param name="segment">The property being observed.</param>
    /// <param name="castTypeName">The type the root is cast to.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <returns>The event name, or <see langword="null"/> once the fallback has been appended instead.</returns>
    /// <remarks>
    /// A widget reports a change once it has happened and has nothing to say before it, so a before-change
    /// observation takes the unchanging answer whatever the property is.
    /// </remarks>
    private static string? ChangeEventOrUnchanging(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange)
    {
        var changeEvent = AndroidWidgetEvents.FindChangeEvent(segment.PropertyName);
        if (!isBeforeChange && changeEvent is not null)
        {
            return changeEvent;
        }

        _ = UnchangingObservationEmitter.AppendExpression(sb, rootVar, segment, castTypeName);

        return null;
    }
}

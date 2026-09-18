// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Observes Android widget properties through their native typed events.</summary>
internal sealed class AndroidObservationPlugin : IPlatformObservationPlugin
{
    /// <inheritdoc/>
    public int Affinity => BindingAffinity.Explicit;

    /// <inheritdoc/>
    public string ObservationKind => "Android";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => false;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForProperty(ClassBindingInfo classInfo, string propertyName, bool isBeforeChange) =>
        !isBeforeChange && CanObserveProperty(classInfo, propertyName) ? Affinity : 0;

    /// <inheritdoc/>
    public PlatformObservationInfo? InspectProperty(INamedTypeSymbol owner, IPropertySymbol property)
    {
        var widget = WidgetFor(property.Name);
        if (widget is null || !PlatformSymbols.DerivesFrom(property.ContainingType, widget))
        {
            return null;
        }

        var first = PlatformSymbols.FindEvent(owner, AndroidWidgetEvents.FindChangeEvent(property.Name)!);
        if (first is null)
        {
            return null;
        }

        var events = new EquatableArray<NotificationEventInfo>([first]);
        if (property.Name == "SelectedItem")
        {
            var second = PlatformSymbols.FindEvent(owner, "NothingSelected");
            if (second is null)
            {
                return null;
            }

            events = new([first, second]);
        }

        return new(ObservationKind, Affinity, events, null, null, null);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAMatch(ClassBindingInfo classInfo) => PlatformSymbols.HasCandidate(classInfo, ObservationKind);

    /// <inheritdoc/>
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) =>
        PlatformSymbols.Candidate(classInfo, propertyName, ObservationKind) is not null;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitHelperClasses(StringBuilder sb) => NativeObservableEmitter.EmitHelper(sb, "__AndroidObservable");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitObservation(StringBuilder sb, in ObservationExpression observation) =>
        NativeObservationEmitter.Emit(sb, observation, ObservationKind, AppendSubscription);

    /// <summary>Emits the concrete native event subscriptions and their removal.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="segment">The observed property.</param>
    /// <param name="info">The selected native mechanism.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendSubscription(StringBuilder sb, PropertyPathSegment segment, PlatformObservationInfo info) =>
        NativeEventSubscriptionEmitter.Append(sb, info.Events);

    /// <summary>Names the widget that defines each observable property contract.</summary>
    /// <param name="propertyName">The observed property.</param>
    /// <returns>The native widget type, or null.</returns>
    private static string? WidgetFor(string propertyName) => propertyName switch
    {
        "Text" => "Android.Widget.TextView",
        "Value" => "Android.Widget.NumberPicker",
        "Rating" => "Android.Widget.RatingBar",
        "Checked" => "Android.Widget.CompoundButton",
        "Date" => "Android.Widget.CalendarView",
        "CurrentTab" => "Android.Widget.TabHost",
        "SelectedItem" => "Android.Widget.AdapterView",
        "Hour" or "Minute" or "CurrentHour" or "CurrentMinute" => "Android.Widget.TimePicker",
        _ => null,
    };
}

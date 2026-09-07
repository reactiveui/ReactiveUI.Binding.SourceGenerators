// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>The Android widget properties that report their own changes, and the event each reports them on.</summary>
/// <remarks>
/// <para>
/// An Android view is not observable in general: it implements no change-notification interface, and most of
/// its properties are plain CLR properties that change silently. A handful of widgets do raise an event for one
/// property, and that handful is the whole of what this mechanism reaches - the runtime engine keeps the same
/// list and scores everything outside it zero, so a property that is not here falls through to whatever else
/// the type carries rather than being observed badly.
/// </para>
/// <para>
/// Keyed by property name alone, which is enough: no two widgets in the list report different events for the
/// same property name, and a property only resolves at all on a type that declares it, so matching the name
/// implies the declaring widget and therefore the event.
/// </para>
/// </remarks>
internal static class AndroidWidgetEvents
{
    /// <summary>The property each reporting widget raises an event for, and the event it raises.</summary>
    private static readonly Dictionary<string, string> _changeEvents = new(StringComparer.Ordinal)
    {
        // TextView and everything built on it.
        ["Text"] = "TextChanged",

        // NumberPicker.
        ["Value"] = "ValueChanged",

        // RatingBar.
        ["Rating"] = "RatingBarChange",

        // CompoundButton, and so CheckBox, RadioButton and Switch.
        ["Checked"] = "CheckedChange",

        // CalendarView.
        ["Date"] = "DateChange",

        // TabHost.
        ["CurrentTab"] = "TabChanged",

        // TimePicker, whose hour and minute are named one way from API 23 and the other before it.
        ["Hour"] = "TimeChanged",
        ["Minute"] = "TimeChanged",
        ["CurrentHour"] = "TimeChanged",
        ["CurrentMinute"] = "TimeChanged",

        // AdapterView, and so Spinner and ListView.
        ["SelectedItem"] = "ItemSelected",
    };

    /// <summary>Finds the event a widget raises when the named property changes.</summary>
    /// <param name="propertyName">The property being observed.</param>
    /// <returns>The event name, or <see langword="null"/> when no widget reports that property.</returns>
    internal static string? FindChangeEvent(string propertyName) =>
        _changeEvents.TryGetValue(propertyName, out var changeEvent) ? changeEvent : null;
}

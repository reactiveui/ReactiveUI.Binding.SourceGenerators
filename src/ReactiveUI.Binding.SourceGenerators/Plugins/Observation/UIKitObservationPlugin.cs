// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Observes UIKit text, selection, date and switch properties through their native notifications.</summary>
internal sealed class UIKitObservationPlugin : IPlatformObservationPlugin
{
    /// <summary>The native UIKit property-specific score.</summary>
    private const int PropertyAffinity = 30;

    /// <inheritdoc/>
    public int Affinity => PropertyAffinity;

    /// <inheritdoc/>
    public string ObservationKind => "UIKit";

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
        var notification = NotificationFor(owner, property.Name);
        if (notification is not null)
        {
            return new(ObservationKind, Affinity, default, notification, null, null);
        }

        var widget = property.Name switch
        {
            "Date" => "UIKit.UIDatePicker",
            "SelectedSegment" => "UIKit.UISegmentedControl",
            "On" => "UIKit.UISwitch",
            "SelectedItem" => "UIKit.UITabBar",
            "Text" => "UIKit.UISearchBar",
            _ => null,
        };
        if (widget is null || !PlatformSymbols.DerivesFrom(owner, widget))
        {
            return null;
        }

        var eventName = property.Name switch
        {
            "SelectedItem" => "ItemSelected",
            "Text" => "TextChanged",
            _ => "ValueChanged",
        };
        var changeEvent = PlatformSymbols.FindEvent(owner, eventName);
        return changeEvent is null ? null : new(ObservationKind, Affinity, new([changeEvent]), null, null, null);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAMatch(ClassBindingInfo classInfo) => PlatformSymbols.HasCandidate(classInfo, ObservationKind);

    /// <inheritdoc/>
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) =>
        PlatformSymbols.Candidate(classInfo, propertyName, ObservationKind) is not null;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitHelperClasses(StringBuilder sb) => NativeObservableEmitter.EmitHelper(sb, "__UIKitObservable");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitObservation(StringBuilder sb, in ObservationExpression observation) =>
        NativeObservationEmitter.Emit(sb, observation, ObservationKind, AppendSubscription);

    /// <summary>Emits the native notification selected for this property.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="segment">The observed property.</param>
    /// <param name="info">The verified native notification.</param>
    internal static void AppendSubscription(StringBuilder sb, PropertyPathSegment segment, PlatformObservationInfo info)
    {
        if (info.NotificationName is not null)
        {
            AppleNotificationEmitter.AppendSubscription(sb, segment, info);
        }
        else
        {
            NativeEventSubscriptionEmitter.Append(sb, info.Events);
        }
    }

    /// <summary>Resolves sender-scoped text notifications for UIKit text controls.</summary>
    /// <param name="owner">The property owner.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The notification constant, or null.</returns>
    internal static string? NotificationFor(INamedTypeSymbol owner, string name)
    {
        if (name != "Text")
        {
            return null;
        }

        if (PlatformSymbols.DerivesFrom(owner, "UIKit.UITextField"))
        {
            return AppleNotificationEmitter.ResolveNotification(owner, "TextFieldTextDidChangeNotification");
        }

        return PlatformSymbols.DerivesFrom(owner, "UIKit.UITextView")
            ? AppleNotificationEmitter.ResolveNotification(owner, "TextDidChangeNotification")
            : null;
    }
}

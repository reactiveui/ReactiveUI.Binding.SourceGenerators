// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Observes AppKit control values through sender-scoped text-change notifications.</summary>
internal sealed class AppKitObservationPlugin : IPlatformObservationPlugin
{
    /// <summary>The native NSControl property observation score.</summary>
    private const int ControlAffinity = 20;

    /// <inheritdoc/>
    public int Affinity => ControlAffinity;

    /// <inheritdoc/>
    public string ObservationKind => "AppKit";

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
        if (!Reports(property.Name) || !PlatformSymbols.DerivesFrom(owner, "AppKit.NSControl"))
        {
            return null;
        }

        var notification = AppleNotificationEmitter.ResolveNotification(owner, "TextDidChangeNotification");
        return notification is null ? null : new(ObservationKind, Affinity, default, notification, null, null);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAMatch(ClassBindingInfo classInfo) => PlatformSymbols.HasCandidate(classInfo, ObservationKind);

    /// <inheritdoc/>
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) =>
        PlatformSymbols.Candidate(classInfo, propertyName, ObservationKind) is not null;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitHelperClasses(StringBuilder sb) => NativeObservableEmitter.EmitHelper(sb, "__AppKitObservable");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitObservation(StringBuilder sb, in ObservationExpression observation) =>
        NativeObservationEmitter.Emit(sb, observation, ObservationKind, AppleNotificationEmitter.AppendSubscription);

    /// <summary>Identifies the NSControl values covered by its native text notification.</summary>
    /// <param name="name">The property name.</param>
    /// <returns>True for a supported native value.</returns>
    internal static bool Reports(string name) => name is
        "AlphaValue" or "DoubleValue" or "FloatValue" or "IntValue" or "NintValue" or "ObjectValue" or "StringValue" or "AttributedStringValue";
}

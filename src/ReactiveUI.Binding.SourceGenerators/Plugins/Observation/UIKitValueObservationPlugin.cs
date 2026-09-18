// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Observes the value exposed by UIKit controls through their native value-change event.</summary>
internal sealed class UIKitValueObservationPlugin : IPlatformObservationPlugin
{
    /// <summary>The native UIControl value observation score.</summary>
    private const int ValueAffinity = 20;

    /// <inheritdoc/>
    public int Affinity => ValueAffinity;

    /// <inheritdoc/>
    public string ObservationKind => "UIKitValue";

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
        if (property.Name != "Value" || !PlatformSymbols.DerivesFrom(owner, "UIKit.UIControl"))
        {
            return null;
        }

        var changeEvent = PlatformSymbols.FindEvent(owner, "ValueChanged");
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
    public void EmitHelperClasses(StringBuilder sb) => NativeObservableEmitter.EmitHelper(sb, "__UIKitValueObservable");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitObservation(StringBuilder sb, in ObservationExpression observation) =>
        NativeObservationEmitter.Emit(sb, observation, ObservationKind, AppendSubscription);

    /// <summary>Emits the concrete value-change handler.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="segment">The observed property.</param>
    /// <param name="info">The verified event delegate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendSubscription(StringBuilder sb, PropertyPathSegment segment, PlatformObservationInfo info) =>
        NativeEventSubscriptionEmitter.Append(sb, info.Events);
}

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Observes dependency properties exposed through Uno's Windows UI namespace.</summary>
internal sealed class UnoObservationPlugin : IPlatformObservationPlugin
{
    /// <inheritdoc/>
    public string ObservationKind => "UnoDP";

    /// <inheritdoc/>
    public int Affinity => BindingAffinity.WinUiDependencyObject;

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => false;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForProperty(ClassBindingInfo classInfo, string propertyName, bool isBeforeChange) =>
        !isBeforeChange && CanObserveProperty(classInfo, propertyName) ? Affinity : 0;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PlatformObservationInfo? InspectProperty(INamedTypeSymbol owner, IPropertySymbol property) =>
        DependencyPropertyObservationEmitter.Inspect(owner, property, ObservationKind, "Windows.UI.Xaml");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAMatch(ClassBindingInfo classInfo) => PlatformSymbols.HasCandidate(classInfo, ObservationKind);

    /// <inheritdoc/>
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) =>
        PlatformSymbols.Candidate(classInfo, propertyName, ObservationKind) is not null;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitHelperClasses(StringBuilder sb) => NativeObservableEmitter.EmitHelper(sb, "__UnoDPObservable");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitObservation(StringBuilder sb, in ObservationExpression observation) =>
        NativeObservationEmitter.Emit(sb, observation, ObservationKind, DependencyPropertyObservationEmitter.AppendSubscription);
}

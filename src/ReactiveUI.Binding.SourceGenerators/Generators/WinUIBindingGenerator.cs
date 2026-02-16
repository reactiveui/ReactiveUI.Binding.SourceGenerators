// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Generators;

/// <summary>
/// Filters types inheriting WinUI DependencyObject for fallback binders.
/// Affinity: 22. Before-change NOT supported (WinUI DPs have no before-change notification).
/// </summary>
internal static class WinUIBindingGenerator
{
    internal const int Affinity = 22;
    internal const string ObservationKind = "WinUIDP";

    /// <summary>
    /// Filters the shared allClasses pipeline for WinUI DependencyObject types.
    /// </summary>
    /// <param name="allClasses">The shared type detection pipeline.</param>
    /// <returns>Filtered observable type information for WinUI DP types.</returns>
    internal static IncrementalValuesProvider<ObservableTypeInfo> Filter(IncrementalValuesProvider<ClassBindingInfo> allClasses)
        => allClasses
            .Where(static x => x.InheritsWinUIDependencyObject)
            .Select(static (x, _) => new ObservableTypeInfo(
                x.FullyQualifiedName,
                x.MetadataName,
                ObservationKind,
                Affinity,
                SupportsBeforeChanged: false,
                x.Properties));
}

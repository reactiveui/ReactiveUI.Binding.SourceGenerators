// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Generators;

/// <summary>
/// Filters types inheriting WPF DependencyObject for fallback binders.
/// Affinity: 20. Before-change NOT supported (WPF DPs have no before-change notification).
/// </summary>
internal static class WpfBindingGenerator
{
    internal const int Affinity = 20;
    internal const string ObservationKind = "WpfDP";

    /// <summary>
    /// Filters the shared allClasses pipeline for WPF DependencyObject types.
    /// </summary>
    /// <param name="allClasses">The shared type detection pipeline.</param>
    /// <returns>Filtered observable type information for WPF DP types.</returns>
    internal static IncrementalValuesProvider<ObservableTypeInfo> Filter(IncrementalValuesProvider<ClassBindingInfo> allClasses)
        => allClasses
            .Where(static x => x.InheritsWpfDependencyObject)
            .Select(static (x, _) => new ObservableTypeInfo(
                x.FullyQualifiedName,
                x.MetadataName,
                ObservationKind,
                Affinity,
                SupportsBeforeChanged: false,
                x.Properties));
}

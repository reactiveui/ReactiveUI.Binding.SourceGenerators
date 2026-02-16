// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Generators;

/// <summary>
/// Filters types inheriting Android.Views.View for Android fallback binders.
/// Affinity: 19. Before-change NOT supported (Android widget events fire after-only).
/// </summary>
internal static class AndroidBindingGenerator
{
    internal const int Affinity = 19;
    internal const string ObservationKind = "Android";

    /// <summary>
    /// Filters the shared allClasses pipeline for Android View types.
    /// </summary>
    /// <param name="allClasses">The shared type detection pipeline.</param>
    /// <returns>Filtered observable type information for Android types.</returns>
    internal static IncrementalValuesProvider<ObservableTypeInfo> Filter(IncrementalValuesProvider<ClassBindingInfo> allClasses)
        => allClasses
            .Where(static x => x.InheritsAndroidView)
            .Select(static (x, _) => new ObservableTypeInfo(
                x.FullyQualifiedName,
                x.MetadataName,
                ObservationKind,
                Affinity,
                SupportsBeforeChanged: false,
                x.Properties));
}

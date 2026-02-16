// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Generators;

/// <summary>
/// Filters types inheriting NSObject for KVO-based fallback binders.
/// Affinity: 25. Supports both after-change and before-change (KVO NSKeyValueObservingOptions).
/// </summary>
internal static class KVOBindingGenerator
{
    internal const int Affinity = 25;
    internal const string ObservationKind = "KVO";

    /// <summary>
    /// Filters the shared allClasses pipeline for NSObject types.
    /// </summary>
    /// <param name="allClasses">The shared type detection pipeline.</param>
    /// <returns>Filtered observable type information for KVO types.</returns>
    internal static IncrementalValuesProvider<ObservableTypeInfo> Filter(IncrementalValuesProvider<ClassBindingInfo> allClasses)
        => allClasses
            .Where(static x => x.InheritsNSObject)
            .Select(static (x, _) => new ObservableTypeInfo(
                x.FullyQualifiedName,
                x.MetadataName,
                ObservationKind,
                Affinity,
                SupportsBeforeChanged: true,
                x.Properties));
}

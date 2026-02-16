// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Generators;

/// <summary>
/// Filters types implementing IReactiveObject for high-affinity fallback binders.
/// Affinity: 24. Supports both after-change (GetChangedObservable) and before-change (GetChangingObservable).
/// </summary>
internal static class ReactiveObjectBindingGenerator
{
    internal const int Affinity = 24;
    internal const string ObservationKind = "ReactiveObject";

    /// <summary>
    /// Filters the shared allClasses pipeline for types implementing IReactiveObject.
    /// </summary>
    /// <param name="allClasses">The shared type detection pipeline.</param>
    /// <returns>Filtered observable type information for IReactiveObject types.</returns>
    internal static IncrementalValuesProvider<ObservableTypeInfo> Filter(IncrementalValuesProvider<ClassBindingInfo> allClasses)
        => allClasses
            .Where(static x => x.ImplementsIReactiveObject)
            .Select(static (x, _) => new ObservableTypeInfo(
                x.FullyQualifiedName,
                x.MetadataName,
                ObservationKind,
                Affinity,
                SupportsBeforeChanged: true,
                x.Properties));
}

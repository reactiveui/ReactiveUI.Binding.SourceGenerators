// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Generators;

/// <summary>
/// Filters types inheriting System.ComponentModel.Component for WinForms fallback binders.
/// Affinity: 23. Before-change NOT supported (WinForms events fire after-only).
/// </summary>
internal static class WinFormsBindingGenerator
{
    internal const int Affinity = 23;
    internal const string ObservationKind = "WinForms";

    /// <summary>
    /// Filters the shared allClasses pipeline for WinForms Component types.
    /// </summary>
    /// <param name="allClasses">The shared type detection pipeline.</param>
    /// <returns>Filtered observable type information for WinForms types.</returns>
    internal static IncrementalValuesProvider<ObservableTypeInfo> Filter(IncrementalValuesProvider<ClassBindingInfo> allClasses)
        => allClasses
            .Where(static x => x.InheritsWinFormsComponent)
            .Select(static (x, _) => new ObservableTypeInfo(
                x.FullyQualifiedName,
                x.MetadataName,
                ObservationKind,
                Affinity,
                SupportsBeforeChanged: false,
                x.Properties));
}

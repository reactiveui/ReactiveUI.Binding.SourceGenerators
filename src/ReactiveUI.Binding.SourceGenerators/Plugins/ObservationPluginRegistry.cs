// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

namespace ReactiveUI.Binding.SourceGenerators.Plugins;

/// <summary>
/// Static registry of observation plugins, sorted by affinity descending.
/// Returns the highest-affinity plugin that matches a given type's <see cref="ClassBindingInfo"/>.
/// Affinity values match ReactiveUI's runtime <c>GetAffinityForObject</c> values exactly.
/// </summary>
internal static class ObservationPluginRegistry
{
    /// <summary>
    /// All plugins sorted by affinity descending (highest priority first).
    /// When multiple plugins match the same type, the first match wins.
    /// </summary>
    private static readonly IObservationPlugin[] Plugins =
    [
        new KVOObservationPlugin(),             // Affinity 15 - Apple NSObject KVO
        new ReactiveObjectObservationPlugin(),  // Affinity 10 - IReactiveObject
        new WinFormsObservationPlugin(),        // Affinity  8 - WinForms Component
        new WinUIObservationPlugin(),           // Affinity  6 - WinUI DependencyObject
        new INPCObservationPlugin(),            // Affinity  5 - INotifyPropertyChanged
        new AndroidObservationPlugin(),         // Affinity  5 - Android View
        new WpfObservationPlugin(),             // Affinity  4 - WPF DependencyObject
    ];

    /// <summary>
    /// Gets the total number of registered plugins.
    /// </summary>
    internal static int Count => Plugins.Length;

    /// <summary>
    /// Gets the highest-affinity plugin that can handle the given type.
    /// </summary>
    /// <param name="classInfo">The type-level binding info.</param>
    /// <returns>The best matching plugin, or <see langword="null"/> if no plugin matches.</returns>
    internal static IObservationPlugin? GetBestPlugin(ClassBindingInfo classInfo)
    {
        for (var i = 0; i < Plugins.Length; i++)
        {
            if (Plugins[i].IsAMatch(classInfo))
            {
                return Plugins[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a plugin by its observation kind identifier.
    /// </summary>
    /// <param name="observationKind">The observation kind (e.g., "INPC", "WpfDP").</param>
    /// <returns>The matching plugin, or <see langword="null"/> if not found.</returns>
    internal static IObservationPlugin? GetPluginByKind(string observationKind)
    {
        for (var i = 0; i < Plugins.Length; i++)
        {
            if (Plugins[i].ObservationKind == observationKind)
            {
                return Plugins[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the plugin at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The plugin at the specified index.</returns>
    internal static IObservationPlugin GetPlugin(int index) => Plugins[index];
}

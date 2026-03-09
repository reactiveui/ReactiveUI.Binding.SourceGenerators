// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

namespace ReactiveUI.Binding.SourceGenerators.Plugins;

/// <summary>
/// Static registry of <see cref="ICommandBindingPlugin"/> instances sorted by affinity descending.
/// Returns the highest-affinity plugin that can handle a given <see cref="BindCommandInvocationInfo"/>.
/// </summary>
internal static class CommandBindingPluginRegistry
{
    /// <summary>
    /// All command binding plugins sorted by affinity descending (highest priority first).
    /// </summary>
    private static readonly ICommandBindingPlugin[] Plugins =
    [
        new CommandPropertyBindingPlugin(),  // Affinity 5
        new EventEnabledBindingPlugin(),     // Affinity 4
        new DefaultEventBindingPlugin(),     // Affinity 3
    ];

    /// <summary>
    /// Returns the highest-affinity plugin that can handle the given invocation,
    /// or <see langword="null"/> if no plugin matches.
    /// </summary>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <returns>The best matching plugin, or null.</returns>
    internal static ICommandBindingPlugin? GetBestPlugin(BindCommandInvocationInfo inv)
    {
        for (var i = 0; i < Plugins.Length; i++)
        {
            if (Plugins[i].CanHandle(inv))
            {
                return Plugins[i];
            }
        }

        return null;
    }
}

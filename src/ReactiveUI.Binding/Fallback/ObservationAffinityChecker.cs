// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Fallback;

/// <summary>
/// Checks whether any user-registered <see cref="ICreatesObservableForProperty"/>
/// implementation has higher affinity than the source generator's compile-time plugin
/// for a given type. Used by generated code to allow user-registered plugins to
/// override source-generated observation at runtime.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ObservationAffinityChecker
{
    /// <summary>
    /// Returns <see langword="true"/> if any registered <see cref="ICreatesObservableForProperty"/>
    /// implementation reports a higher affinity than <paramref name="generatedAffinity"/>
    /// for the given type.
    /// </summary>
    /// <param name="type">The type being observed.</param>
    /// <param name="generatedAffinity">The affinity of the source generator's selected plugin.</param>
    /// <param name="beforeChanged">Whether before-change (PropertyChanging) observation is requested.</param>
    /// <returns><see langword="true"/> if a user plugin should override the generated observation.</returns>
    public static bool HasHigherAffinityPlugin(Type type, int generatedAffinity, bool beforeChanged)
    {
        ArgumentExceptionHelper.ThrowIfNull(type);

        var services = AppLocator.Current.GetServices<ICreatesObservableForProperty>();
        foreach (var plugin in services)
        {
            if (plugin.GetAffinityForObject(type, string.Empty, beforeChanged) > generatedAffinity)
            {
                return true;
            }
        }

        return false;
    }
}

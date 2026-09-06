// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>
/// Checks whether any user-registered <see cref="ICreatesObservableForProperty"/>
/// implementation has higher affinity than the source generator's compile-time plugin
/// for a given type. Used by generated code to allow user-registered plugins to
/// override source-generated observation at runtime.
/// </summary>
/// <remarks>
/// Every generated observation and binding asks this before it does anything else, so the answer is on the
/// path of every binding an application creates. The registered set is resolved once and kept rather than
/// re-read from the locator each time: asking the locator allocates an enumeration per call, which a view
/// with many bindings pays for repeatedly and never gets anything back for. <see cref="Refresh"/> drops the
/// resolved set for a host that registers a plugin after the first binding.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ObservationAffinityChecker
{
    /// <summary>Guards <see cref="_plugins"/> while it is (re)resolved.</summary>
    private static readonly Lock Gate = new();

    /// <summary>The resolved plugins, or null while none have been resolved yet.</summary>
    private static ICreatesObservableForProperty[]? _plugins;

    /// <summary>Re-reads the registered plugins, for a host that registers them after the first binding.</summary>
    public static void Refresh()
    {
        lock (Gate)
        {
            _plugins = null;
        }
    }

    /// <summary>Returns <see langword="true"/> if a registered <see cref="ICreatesObservableForProperty"/> outranks <paramref name="generatedAffinity"/>.</summary>
    /// <param name="type">The type being observed.</param>
    /// <param name="generatedAffinity">The affinity of the source generator's selected plugin.</param>
    /// <param name="beforeChanged">Whether before-change (PropertyChanging) observation is requested.</param>
    /// <returns><see langword="true"/> if a user plugin should override the generated observation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
    public static bool HasHigherAffinityPlugin(Type type, int generatedAffinity, bool beforeChanged)
    {
        ArgumentExceptionHelper.ThrowIfNull(type);

        var plugins = Resolve();
        for (var i = 0; i < plugins.Length; i++)
        {
            if (plugins[i].GetAffinityForObject(type, string.Empty, beforeChanged) > generatedAffinity)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Resolves the registered plugins once and keeps them.</summary>
    /// <returns>The registered plugins, empty when none is registered.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ICreatesObservableForProperty[] Resolve()
    {
        var resolved = _plugins;
        if (resolved is not null)
        {
            return resolved;
        }

        lock (Gate)
        {
            _plugins ??= [.. AppLocator.Current.GetServices<ICreatesObservableForProperty>()];
            return _plugins;
        }
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Fallback;

/// <summary>
/// Checks whether any user-registered <see cref="ICreatesCommandBinding"/>
/// implementation has higher affinity than the source generator's compile-time plugin
/// for a given control type. Used by generated code to allow user-registered plugins to
/// override source-generated command binding at runtime.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class CommandBindingAffinityChecker
{
    /// <summary>
    /// Returns <see langword="true"/> if any registered <see cref="ICreatesCommandBinding"/>
    /// implementation reports a higher affinity than <paramref name="generatedAffinity"/>
    /// for the given control type.
    /// </summary>
    /// <typeparam name="T">The control type being bound to.</typeparam>
    /// <param name="generatedAffinity">The affinity of the source generator's selected plugin.</param>
    /// <param name="hasEventTarget">Whether the caller specifies a custom event target.</param>
    /// <returns><see langword="true"/> if a user plugin should override the generated binding.</returns>
    public static bool HasHigherAffinityPlugin<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicEvents | DynamicallyAccessedMemberTypes.PublicProperties)] T>(int generatedAffinity, bool hasEventTarget)
    {
        var services = Locator.Current.GetServices<ICreatesCommandBinding>();
        foreach (var plugin in services)
        {
            if (plugin.GetAffinityForObject<T>(hasEventTarget) > generatedAffinity)
            {
                return true;
            }
        }

        return false;
    }
}

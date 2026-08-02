// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>
/// Checks whether any user-registered <see cref="ICreatesCommandBinding"/>
/// implementation has higher affinity than the source generator's compile-time plugin
/// for a given control type. Used by generated code to allow user-registered plugins to
/// override source-generated command binding at runtime.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class CommandBindingAffinityChecker
{
    /// <summary>Returns <see langword="true"/> if a registered <see cref="ICreatesCommandBinding"/> outranks <paramref name="generatedAffinity"/>.</summary>
    /// <typeparam name="T">The control type being bound to.</typeparam>
    /// <param name="generatedAffinity">The affinity of the source generator's selected plugin.</param>
    /// <param name="hasEventTarget">Whether the caller specifies a custom event target.</param>
    /// <returns><see langword="true"/> if a user plugin should override the generated binding.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; the interface shape dictates it.")]
    public static bool HasHigherAffinityPlugin<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicEvents
                                    | DynamicallyAccessedMemberTypes.PublicProperties)]
    T>(int generatedAffinity, bool hasEventTarget)
    {
        foreach (var plugin in Locator.Current.GetServices<ICreatesCommandBinding>())
        {
            if (plugin.GetAffinityForObject<T>(hasEventTarget) > generatedAffinity)
            {
                return true;
            }
        }

        return false;
    }
}

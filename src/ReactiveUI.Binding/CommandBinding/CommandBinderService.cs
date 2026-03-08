// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.CommandBinding;

/// <summary>
/// Service that resolves the best <see cref="ICreatesCommandBinding"/> for a given
/// control type using affinity scoring. Follows the same resolution pattern as
/// property observation via <see cref="ICreatesObservableForProperty"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class CommandBinderService
{
    /// <summary>
    /// Gets the highest-affinity <see cref="ICreatesCommandBinding"/> registered for the specified control type.
    /// </summary>
    /// <typeparam name="T">The type of the control.</typeparam>
    /// <param name="hasEventTarget">Whether the caller specifies a custom event target.</param>
    /// <returns>The best binder, or <see langword="null"/> if no registered binder supports the control type.</returns>
    public static ICreatesCommandBinding? GetBinder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicEvents | DynamicallyAccessedMemberTypes.PublicProperties)] T>(bool hasEventTarget)
    {
        var resolver = Locator.Current;
        var binders = resolver.GetServices<ICreatesCommandBinding>();

        ICreatesCommandBinding? bestBinder = null;
        var bestAffinity = 0;

        foreach (var binder in binders)
        {
            var affinity = binder.GetAffinityForObject<T>(hasEventTarget);
            if (affinity > bestAffinity)
            {
                bestAffinity = affinity;
                bestBinder = binder;
            }
        }

        return bestBinder;
    }
}

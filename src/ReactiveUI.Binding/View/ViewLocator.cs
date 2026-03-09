// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Static accessor for the current <see cref="IViewLocator"/> instance.
/// </summary>
public static class ViewLocator
{
    /// <summary>
    /// Gets the current <see cref="IViewLocator"/> from the service locator.
    /// </summary>
    /// <returns>The current <see cref="IViewLocator"/> instance.</returns>
    /// <exception cref="ViewLocatorNotFoundException">
    /// Thrown when no <see cref="IViewLocator"/> is registered.
    /// </exception>
    public static IViewLocator GetCurrent()
    {
        var locator = AppLocator.Current.GetService<IViewLocator>();
        if (locator is null)
        {
            throw new ViewLocatorNotFoundException();
        }

        return locator;
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Convenience overloads for <see cref="IViewLocator"/> that supply the default (null) contract.
/// Provided as overloads rather than optional parameters so the interface stays free of optional parameters.
/// </summary>
public static class ViewLocatorMixin
{
    /// <summary>
    /// Resolves a view for the specified view model type using the default contract.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <param name="locator">The view locator.</param>
    /// <param name="viewModel">The view model instance to resolve a view for.</param>
    /// <returns>The resolved view, or <see langword="null"/> if no view is found.</returns>
    public static IViewFor? ResolveView<TViewModel>(this IViewLocator locator, TViewModel viewModel)
        where TViewModel : class
    {
        ArgumentExceptionHelper.ThrowIfNull(locator);
        return locator.ResolveView(viewModel, null);
    }

    /// <summary>
    /// Resolves a view for the specified view model instance using the default contract.
    /// </summary>
    /// <param name="locator">The view locator.</param>
    /// <param name="viewModel">The view model instance to resolve a view for.</param>
    /// <returns>The resolved view, or <see langword="null"/> if no view is found.</returns>
    public static IViewFor? ResolveView(this IViewLocator locator, object? viewModel)
    {
        ArgumentExceptionHelper.ThrowIfNull(locator);
        return locator.ResolveView(viewModel, null);
    }
}

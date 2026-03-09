// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Resolves views for view models. Supports both AOT-safe compile-time dispatch
/// (via source-generated mappings) and runtime service locator lookup.
/// </summary>
public interface IViewLocator : IEnableLogger
{
    /// <summary>
    /// Resolves a view for the specified view model type.
    /// This overload is AOT-safe when registered mappings are available.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <param name="viewModel">The view model instance to resolve a view for.</param>
    /// <param name="contract">An optional contract string for named registrations.</param>
    /// <returns>The resolved view, or <see langword="null"/> if no view is found.</returns>
    IViewFor? ResolveView<TViewModel>(TViewModel viewModel, string? contract = null)
        where TViewModel : class;

    /// <summary>
    /// Resolves a view for the specified view model instance using runtime type dispatch.
    /// Uses source-generated dispatch when available, followed by explicit mappings,
    /// then falls back to service locator lookup.
    /// </summary>
    /// <param name="viewModel">The view model instance to resolve a view for.</param>
    /// <param name="contract">An optional contract string for named registrations.</param>
    /// <returns>The resolved view, or <see langword="null"/> if no view is found.</returns>
    IViewFor? ResolveView(object? viewModel, string? contract = null);
}

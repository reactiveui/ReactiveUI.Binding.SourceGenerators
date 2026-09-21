// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Resolves views for view models.</summary>
public interface IViewLocator : IEnableLogger
{
    /// <summary>Resolves a view for the view model using its compile-time type, without reflection over the view model's runtime type.</summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <param name="viewModel">The view model instance to resolve a view for.</param>
    /// <param name="contract">The contract to resolve under, or null for the default view.</param>
    /// <returns>The resolved view, or <see langword="null"/> if no view is found.</returns>
    IViewFor? ResolveView<TViewModel>(TViewModel viewModel, string? contract)
        where TViewModel : class;

    /// <summary>Resolves a view for the view model using its runtime type.</summary>
    /// <param name="viewModel">The view model instance to resolve a view for.</param>
    /// <param name="contract">The contract to resolve under, or null for the default view.</param>
    /// <returns>The resolved view, or <see langword="null"/> if no view is found.</returns>
    [RequiresDynamicCode("Resolving a view from an object closes IViewFor<> over its runtime type. Use the generic overload, or register the view, to stay ahead-of-time safe.")]
    IViewFor? ResolveView(object? viewModel, string? contract);
}

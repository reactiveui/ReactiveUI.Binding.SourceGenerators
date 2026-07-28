// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>Fluent builder for registering view-to-view-model mappings on a <see cref="DefaultViewLocator"/>.</summary>
public sealed class ViewMappingBuilder
{
    /// <summary>The view locator to register mappings on.</summary>
    private readonly DefaultViewLocator _locator;

    /// <summary>Initializes a new instance of the <see cref="ViewMappingBuilder"/> class.</summary>
    /// <param name="locator">The view locator to register mappings on.</param>
    internal ViewMappingBuilder(DefaultViewLocator locator) => _locator = locator;

    /// <summary>Maps a view model type to a view type with direct construction.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type. Must have a parameterless constructor.</typeparam>
    /// <returns>This builder for chaining.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    public ViewMappingBuilder Map<TViewModel, TView>()
        where TViewModel : class
        where TView : IViewFor, new() => Map<TViewModel, TView>(null);

    /// <summary>Maps a view model type to a view type with direct construction.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type. Must have a parameterless constructor.</typeparam>
    /// <param name="contract">A contract string for named registrations.</param>
    /// <returns>This builder for chaining.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    public ViewMappingBuilder Map<TViewModel, TView>(string? contract)
        where TViewModel : class
        where TView : IViewFor, new()
    {
        _locator.Map<TViewModel, TView>(contract);
        return this;
    }

    /// <summary>Maps a view model type to a view using a custom factory function.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="factory">A factory function that creates the view.</param>
    /// <returns>This builder for chaining.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    public ViewMappingBuilder Map<TViewModel>(Func<IViewFor> factory)
        where TViewModel : class => Map<TViewModel>(factory, null);

    /// <summary>Maps a view model type to a view using a custom factory function.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="factory">A factory function that creates the view.</param>
    /// <param name="contract">A contract string for named registrations.</param>
    /// <returns>This builder for chaining.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    public ViewMappingBuilder Map<TViewModel>(Func<IViewFor> factory, string? contract)
        where TViewModel : class
    {
        _locator.Map<TViewModel>(factory, contract);
        return this;
    }
}

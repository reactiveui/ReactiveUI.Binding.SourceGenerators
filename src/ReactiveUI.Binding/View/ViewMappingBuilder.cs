// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Fluent builder for registering view-to-view-model mappings on a <see cref="DefaultViewLocator"/>.
/// </summary>
public sealed class ViewMappingBuilder
{
    /// <summary>
    /// The view locator to register mappings on.
    /// </summary>
    private readonly DefaultViewLocator _locator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewMappingBuilder"/> class.
    /// </summary>
    /// <param name="locator">The view locator to register mappings on.</param>
    internal ViewMappingBuilder(DefaultViewLocator locator)
    {
        _locator = locator;
    }

    /// <summary>
    /// Maps a view model type to a view type with direct construction.
    /// </summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type. Must have a parameterless constructor.</typeparam>
    /// <param name="contract">An optional contract string for named registrations.</param>
    /// <returns>This builder for chaining.</returns>
    public ViewMappingBuilder Map<TViewModel, TView>(string? contract = null)
        where TViewModel : class
        where TView : IViewFor, new()
    {
        _locator.Map<TViewModel, TView>(contract);
        return this;
    }

    /// <summary>
    /// Maps a view model type to a view using a custom factory function.
    /// </summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="factory">A factory function that creates the view.</param>
    /// <param name="contract">An optional contract string for named registrations.</param>
    /// <returns>This builder for chaining.</returns>
    public ViewMappingBuilder Map<TViewModel>(Func<IViewFor> factory, string? contract = null)
        where TViewModel : class
    {
        _locator.Map<TViewModel>(factory, contract);
        return this;
    }
}

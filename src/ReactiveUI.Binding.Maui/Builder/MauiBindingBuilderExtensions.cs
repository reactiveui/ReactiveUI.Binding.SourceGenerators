// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;

using Splat.Builder;

namespace ReactiveUI.Binding.Maui.Builder;

/// <summary>
/// MAUI-specific extensions for the ReactiveUI.Binding builder.
/// </summary>
public static class MauiBindingBuilderExtensions
{
    /// <summary>
    /// Configures ReactiveUI.Binding for MAUI platform, registering WinUI DependencyProperty
    /// observation (on Windows) and Visibility converters.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IReactiveUIBindingBuilder WithMaui(this IReactiveUIBindingBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.WithPlatformModule(new MauiBindingModule());
    }

    /// <summary>
    /// Configures ReactiveUI.Binding for MAUI platform, registering WinUI DependencyProperty
    /// observation (on Windows) and Visibility converters.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IReactiveUIBindingBuilder WithMaui(this IAppBuilder builder) =>
        ((IReactiveUIBindingBuilder)builder).WithMaui();
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;

using Splat.Builder;

namespace ReactiveUI.Binding.Wpf.Builder;

/// <summary>
/// WPF-specific extensions for the ReactiveUI.Binding builder.
/// </summary>
public static class WpfBindingBuilderExtensions
{
    /// <summary>
    /// Configures ReactiveUI.Binding for WPF platform, registering DependencyObject observation
    /// and WPF-specific Visibility converters.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IReactiveUIBindingBuilder WithWpf(this IReactiveUIBindingBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.WithPlatformModule(new WpfBindingModule());
    }

    /// <summary>
    /// Configures ReactiveUI.Binding for WPF platform, registering DependencyObject observation
    /// and WPF-specific Visibility converters.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IReactiveUIBindingBuilder WithWpf(this IAppBuilder builder) =>
        ((IReactiveUIBindingBuilder)builder).WithWpf();
}

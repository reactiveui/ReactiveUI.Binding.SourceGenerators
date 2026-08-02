// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Wpf.Builder;
#else
namespace ReactiveUI.Binding.Wpf.Builder;
#endif

/// <summary>WPF-specific extensions for the ReactiveUI.Binding builder.</summary>
public static class WpfBindingBuilderExtensions
{
    /// <summary>Provides WithWpf extension members for <paramref name="builder"/>.</summary>
    /// <param name="builder">The builder instance.</param>
    extension(IAppBuilder builder)
    {
        /// <summary>
        /// Configures ReactiveUI.Binding for WPF platform, registering DependencyObject observation
        /// and WPF-specific Visibility converters.
        /// </summary>
        /// <returns>The builder instance for chaining.</returns>
        public IReactiveUIBindingBuilder WithWpf() =>
            ((IReactiveUIBindingBuilder)builder).WithWpf();
    }

    /// <summary>Provides WithWpf extension members for <paramref name="builder"/>.</summary>
    /// <param name="builder">The builder instance.</param>
    extension(IReactiveUIBindingBuilder builder)
    {
        /// <summary>
        /// Configures ReactiveUI.Binding for WPF platform, registering DependencyObject observation
        /// and WPF-specific Visibility converters.
        /// </summary>
        /// <returns>The builder instance for chaining.</returns>
        public IReactiveUIBindingBuilder WithWpf()
        {
            ArgumentExceptionHelper.ThrowIfNull(builder);

            return builder.WithPlatformModule(new WpfBindingModule());
        }
    }
}

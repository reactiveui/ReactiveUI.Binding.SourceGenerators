// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Maui.Builder;
#else
namespace ReactiveUI.Binding.Maui.Builder;
#endif

/// <summary>MAUI-specific extensions for the ReactiveUI.Binding builder.</summary>
public static class MauiBindingBuilderExtensions
{
    /// <summary>Provides WithMaui extension members for <paramref name="builder"/>.</summary>
    /// <param name="builder">The builder instance.</param>
    extension(IAppBuilder builder)
    {
        /// <summary>
        /// Configures ReactiveUI.Binding for MAUI platform, registering WinUI DependencyProperty
        /// observation (on Windows) and Visibility converters.
        /// </summary>
        /// <returns>The builder instance for chaining.</returns>
        public IReactiveUIBindingBuilder WithMaui() =>
            ((IReactiveUIBindingBuilder)builder).WithMaui();
    }

    /// <summary>Provides WithMaui extension members for <paramref name="builder"/>.</summary>
    /// <param name="builder">The builder instance.</param>
    extension(IReactiveUIBindingBuilder builder)
    {
        /// <summary>
        /// Configures ReactiveUI.Binding for MAUI platform, registering WinUI DependencyProperty
        /// observation (on Windows) and Visibility converters.
        /// </summary>
        /// <returns>The builder instance for chaining.</returns>
        public IReactiveUIBindingBuilder WithMaui()
        {
            ArgumentExceptionHelper.ThrowIfNull(builder);

            return builder.WithPlatformModule(new MauiBindingModule());
        }
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.WinForms.Builder;
#else
namespace ReactiveUI.Binding.WinForms.Builder;
#endif

/// <summary>WinForms-specific extensions for the ReactiveUI.Binding builder.</summary>
public static class WinFormsBindingBuilderExtensions
{
    /// <summary>Provides WithWinForms extension members for <paramref name="builder"/>.</summary>
    /// <param name="builder">The builder instance.</param>
    extension(IAppBuilder builder)
    {
        /// <summary>
        /// Configures ReactiveUI.Binding for WinForms platform, registering event-based
        /// property observation for WinForms components.
        /// </summary>
        /// <returns>The builder instance for chaining.</returns>
        public IReactiveUIBindingBuilder WithWinForms() =>
            ((IReactiveUIBindingBuilder)builder).WithWinForms();
    }

    /// <summary>Provides WithWinForms extension members for <paramref name="builder"/>.</summary>
    /// <param name="builder">The builder instance.</param>
    extension(IReactiveUIBindingBuilder builder)
    {
        /// <summary>
        /// Configures ReactiveUI.Binding for WinForms platform, registering event-based
        /// property observation for WinForms components.
        /// </summary>
        /// <returns>The builder instance for chaining.</returns>
        public IReactiveUIBindingBuilder WithWinForms()
        {
            ArgumentExceptionHelper.ThrowIfNull(builder);

            return builder.WithPlatformModule(new WinFormsBindingModule());
        }
    }
}

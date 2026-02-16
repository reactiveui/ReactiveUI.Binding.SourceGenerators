// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;

using Splat.Builder;

namespace ReactiveUI.Binding.WinForms.Builder;

/// <summary>
/// WinForms-specific extensions for the ReactiveUI.Binding builder.
/// </summary>
public static class WinFormsBindingBuilderExtensions
{
    /// <summary>
    /// Configures ReactiveUI.Binding for WinForms platform, registering event-based
    /// property observation for WinForms components.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IReactiveUIBindingBuilder WithWinForms(this IReactiveUIBindingBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.WithPlatformModule(new WinFormsBindingModule());
    }

    /// <summary>
    /// Configures ReactiveUI.Binding for WinForms platform, registering event-based
    /// property observation for WinForms components.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IReactiveUIBindingBuilder WithWinForms(this IAppBuilder builder) =>
        ((IReactiveUIBindingBuilder)builder).WithWinForms();
}

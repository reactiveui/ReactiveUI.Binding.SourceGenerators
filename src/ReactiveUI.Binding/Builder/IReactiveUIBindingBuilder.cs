// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

namespace ReactiveUI.Binding.Builder;

/// <summary>
/// Fluent builder that configures ReactiveUI.Binding services, converters, and platform modules
/// before building an application instance.
/// </summary>
/// <remarks>
/// <para>
/// The builder wraps <see cref="AppBuilder"/> to provide ReactiveUI.Binding-specific configuration
/// for property observation and binding services. Platform modules (WPF, WinForms, MAUI) are
/// registered via extension methods such as <c>WithWpf()</c>, <c>WithWinForms()</c>, and <c>WithMaui()</c>.
/// </para>
/// </remarks>
/// <seealso cref="IAppBuilder" />
public interface IReactiveUIBindingBuilder : IAppBuilder
{
    /// <summary>
    /// Registers the core ReactiveUI.Binding services (INPC/POCO observation, default converters).
    /// Hides <see cref="IAppBuilder.WithCoreServices"/> to return <see cref="IReactiveUIBindingBuilder"/>
    /// for fluent chaining.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    new IReactiveUIBindingBuilder WithCoreServices();

    /// <summary>
    /// Registers a platform-specific module with the builder.
    /// </summary>
    /// <typeparam name="T">The type of the platform module. Must implement <see cref="IModule"/>.</typeparam>
    /// <param name="module">The platform module instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    IReactiveUIBindingBuilder WithPlatformModule<T>(T module)
        where T : IModule;

    /// <summary>
    /// Registers a custom action to be executed during the build phase.
    /// </summary>
    /// <param name="configureAction">An action that receives the mutable dependency resolver.</param>
    /// <returns>The builder instance for chaining.</returns>
    IReactiveUIBindingBuilder WithRegistration(Action<IMutableDependencyResolver> configureAction);

    /// <summary>
    /// Builds the application and returns the configured instance.
    /// </summary>
    /// <returns>The configured application instance.</returns>
    IReactiveUIBindingInstance BuildApp();
}

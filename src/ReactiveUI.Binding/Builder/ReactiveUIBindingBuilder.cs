// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

namespace ReactiveUI.Binding.Builder;

/// <summary>
/// A builder class for configuring ReactiveUI.Binding services.
/// Extends the Splat <see cref="AppBuilder"/> to provide binding-specific configuration.
/// </summary>
/// <remarks>
/// <para>
/// Use this builder to register core services, default converters, and platform-specific modules
/// for property observation and binding.
/// </para>
/// <example>
/// <code>
/// RxBindingBuilder.CreateReactiveUIBindingBuilder()
///     .WithCoreServices()
///     .WithPlatformModule(new WpfBindingModule())
///     .BuildApp();
/// </code>
/// </example>
/// </remarks>
public sealed class ReactiveUIBindingBuilder : AppBuilder, IReactiveUIBindingBuilder, IReactiveUIBindingInstance
{
    private bool _coreRegistered;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactiveUIBindingBuilder"/> class.
    /// </summary>
    /// <param name="resolver">The dependency resolver to configure.</param>
    /// <param name="current">The configured services.</param>
    public ReactiveUIBindingBuilder(IMutableDependencyResolver resolver, IReadonlyDependencyResolver? current)
        : base(resolver, current)
    {
        CurrentMutable.InitializeSplat();

        // Register the ConverterService instance so it's accessible to registrations
        CurrentMutable.RegisterConstant(() => ConverterService);
    }

    /// <summary>
    /// Gets the converter service used for binding type conversions.
    /// </summary>
    /// <remarks>
    /// This service provides access to three specialized registries:
    /// <list type="bullet">
    /// <item><description><see cref="ConverterService.TypedConverters"/> - For exact type-pair converters.</description></item>
    /// <item><description><see cref="ConverterService.FallbackConverters"/> - For fallback converters with runtime type checking.</description></item>
    /// <item><description><see cref="ConverterService.SetMethodConverters"/> - For set-method converters.</description></item>
    /// </list>
    /// Use the <c>WithConverter*</c> methods to register converters during application initialization.
    /// </remarks>
    public ConverterService ConverterService { get; } = new();

    /// <summary>
    /// Registers a platform-specific module with the builder.
    /// </summary>
    /// <typeparam name="T">The type of the platform module. Must implement <see cref="IModule"/>.</typeparam>
    /// <param name="module">The platform module instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    public IReactiveUIBindingBuilder WithPlatformModule<T>(T module)
        where T : IModule
    {
        ArgumentExceptionHelper.ThrowIfNull(module);
        UsingModule(module);
        return this;
    }

    /// <summary>
    /// Adds a custom registration action to be executed during the build phase.
    /// </summary>
    /// <param name="configureAction">An action that receives the mutable dependency resolver.</param>
    /// <returns>The builder instance for chaining.</returns>
    public IReactiveUIBindingBuilder WithRegistration(Action<IMutableDependencyResolver> configureAction)
    {
        ArgumentExceptionHelper.ThrowIfNull(configureAction);
        configureAction(CurrentMutable);
        return this;
    }

    /// <summary>
    /// Registers a typed binding converter.
    /// </summary>
    /// <param name="converter">The converter instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    public IReactiveUIBindingBuilder WithConverter(IBindingTypeConverter converter)
    {
        ArgumentExceptionHelper.ThrowIfNull(converter);
        ConverterService.TypedConverters.Register(converter);
        return this;
    }

    /// <summary>
    /// Registers a fallback binding converter.
    /// </summary>
    /// <param name="converter">The fallback converter instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    public IReactiveUIBindingBuilder WithFallbackConverter(IBindingFallbackConverter converter)
    {
        ArgumentExceptionHelper.ThrowIfNull(converter);
        ConverterService.FallbackConverters.Register(converter);
        return this;
    }

    /// <summary>
    /// Registers a set-method binding converter.
    /// </summary>
    /// <param name="converter">The set-method converter instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    public IReactiveUIBindingBuilder WithSetMethodConverter(ISetMethodBindingConverter converter)
    {
        ArgumentExceptionHelper.ThrowIfNull(converter);
        ConverterService.SetMethodConverters.Register(converter);
        return this;
    }

    /// <summary>
    /// Registers the core ReactiveUI.Binding services in an AOT-compatible manner.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public override IAppBuilder WithCoreServices()
    {
        if (!_coreRegistered)
        {
            // Register all standard converters to the ConverterService
            DefaultConverterRegistration.RegisterDefaults(ConverterService);

            // Register core observation services
            WithPlatformModule(new ReactiveUIBindingModule());
            _coreRegistered = true;
        }

        return this;
    }

    /// <inheritdoc/>
    IReactiveUIBindingBuilder IReactiveUIBindingBuilder.WithCoreServices()
    {
        if (WithCoreServices() is not IReactiveUIBindingBuilder result)
        {
            throw new InvalidOperationException("WithCoreServices did not return an IReactiveUIBindingBuilder");
        }

        return result;
    }

    /// <summary>
    /// Builds the application and returns the configured instance.
    /// </summary>
    /// <returns>The configured application instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if building the app instance fails.</exception>
    public IReactiveUIBindingInstance BuildApp()
    {
        if (Build() is not IReactiveUIBindingInstance appInstance || appInstance.Current is null)
        {
            throw new InvalidOperationException("Failed to create ReactiveUIBindingInstance instance");
        }

        // Set the global converter service
        BindingConverters.SetService(ConverterService);

        // Mark as initialized
        RxBindingBuilder.MarkAsInitialized();

        return appInstance;
    }
}

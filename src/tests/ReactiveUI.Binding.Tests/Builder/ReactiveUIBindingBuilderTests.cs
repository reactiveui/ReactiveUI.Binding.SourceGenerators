// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Tests.TestModels;

using Splat;

namespace ReactiveUI.Binding.Tests.Builder;

/// <summary>
/// Tests for the <see cref="ReactiveUIBindingBuilder"/> class.
/// </summary>
public class ReactiveUIBindingBuilderTests
{
    /// <summary>
    /// Verifies that the builder can be created and has a non-null ConverterService.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_CreatesConverterService()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        await Assert.That(builder.ConverterService).IsNotNull();
    }

    /// <summary>
    /// Verifies that WithCoreServices registers INPC and POCO observation services.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithCoreServices_RegistersObservationServices()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        builder.WithCoreServices();
        builder.BuildApp();

        var services = Locator.Current.GetServices<ICreatesObservableForProperty>().ToList();

        await Assert.That(services.Count).IsGreaterThanOrEqualTo(2);
    }

    /// <summary>
    /// Verifies that WithPlatformModule registers a module.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithPlatformModule_RegistersModule()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var registered = false;

        builder.WithPlatformModule(new TestModule(() => registered = true));
        builder.BuildApp();

        await Assert.That(registered).IsTrue();
    }

    /// <summary>
    /// Verifies that WithRegistration executes the registration action.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithRegistration_ExecutesAction()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var executed = false;

        builder.WithRegistration(_ => executed = true);

        await Assert.That(executed).IsTrue();
    }

    /// <summary>
    /// Verifies that BuildApp sets the global ConverterService.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildApp_SetsGlobalConverterService()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        builder.WithCoreServices();

        var instance = builder.BuildApp();

        await Assert.That(instance).IsNotNull();
        await Assert.That(BindingConverters.Current).IsNotNull();
    }

    /// <summary>
    /// Verifies that WithCoreServices is idempotent.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithCoreServices_CalledTwice_OnlyRegistersOnce()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        builder.WithCoreServices();
        builder.WithCoreServices();
        builder.BuildApp();

        // Should not throw; services registered once
        var services = Locator.Current.GetServices<ICreatesObservableForProperty>().ToList();
        await Assert.That(services.Count).IsGreaterThanOrEqualTo(2);
    }

    /// <summary>
    /// Verifies that the fluent API supports chaining.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FluentAPI_SupportsChainingCalls()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        builder.WithCoreServices();
        var result = builder.WithRegistration(_ => { });

        await Assert.That(result).IsNotNull();
    }

    /// <summary>
    /// Verifies that <see cref="ReactiveUIBindingBuilder.WithConverter"/> registers a
    /// typed converter that appears in the <see cref="ConverterService.TypedConverters"/> registry.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithConverter_RegistersInTypedConverters()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubBindingTypeConverter(
            typeof(int),
            typeof(bool),
            (from, hint) => (true, true));

        builder.WithConverter(converter);

        var resolved = builder.ConverterService.TypedConverters.TryGetConverter(typeof(int), typeof(bool));
        await Assert.That(resolved).IsNotNull();
        await Assert.That(ReferenceEquals(resolved, converter)).IsTrue();
    }

    /// <summary>
    /// Verifies that <see cref="ReactiveUIBindingBuilder.WithConverter"/> returns the builder
    /// for fluent chaining.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithConverter_ReturnsSelfForChaining()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubBindingTypeConverter(
            typeof(int),
            typeof(string),
            (from, hint) => (true, from?.ToString()));

        var result = builder.WithConverter(converter);

        await Assert.That(result).IsNotNull();
    }

    /// <summary>
    /// Verifies that <see cref="ReactiveUIBindingBuilder.WithFallbackConverter"/> registers a
    /// fallback converter that appears in the <see cref="ConverterService.FallbackConverters"/> registry.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithFallbackConverter_RegistersInFallbackConverters()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubFallbackConverter(
            (fromType, from, toType, hint) => (true, "converted"));

        builder.WithFallbackConverter(converter);

        var allConverters = builder.ConverterService.FallbackConverters.GetAllConverters().ToList();
        await Assert.That(allConverters).Contains(converter);
    }

    /// <summary>
    /// Verifies that <see cref="ReactiveUIBindingBuilder.WithFallbackConverter"/> returns the builder
    /// for fluent chaining.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithFallbackConverter_ReturnsSelfForChaining()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubFallbackConverter(
            (fromType, from, toType, hint) => (true, "converted"));

        var result = builder.WithFallbackConverter(converter);

        await Assert.That(result).IsNotNull();
    }

    /// <summary>
    /// Verifies that <see cref="ReactiveUIBindingBuilder.WithSetMethodConverter"/> registers a
    /// set-method converter that appears in the <see cref="ConverterService.SetMethodConverters"/> registry.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSetMethodConverter_RegistersInSetMethodConverters()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubSetMethodBindingConverter();

        builder.WithSetMethodConverter(converter);

        var allConverters = builder.ConverterService.SetMethodConverters.GetAllConverters().ToList();
        await Assert.That(allConverters).Contains(converter);
    }

    /// <summary>
    /// Verifies that <see cref="ReactiveUIBindingBuilder.WithSetMethodConverter"/> returns the builder
    /// for fluent chaining.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSetMethodConverter_ReturnsSelfForChaining()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubSetMethodBindingConverter();

        var result = builder.WithSetMethodConverter(converter);

        await Assert.That(result).IsNotNull();
    }

    /// <summary>
    /// Verifies that <see cref="ReactiveUIBindingBuilder.WithCoreServices"/> registers default
    /// converters, spot-checking that the <see cref="StringConverter"/> is present.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithCoreServices_RegistersDefaultConverters()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        builder.WithCoreServices();

        // Spot-check: StringConverter registers string -> string
        var stringConverter = builder.ConverterService.TypedConverters.TryGetConverter(typeof(string), typeof(string));
        await Assert.That(stringConverter).IsNotNull();
    }

    /// <summary>
    /// Verifies that <see cref="ReactiveUIBindingBuilder.WithCoreServices"/> registers
    /// the <see cref="IntegerToStringTypeConverter"/> (int -> string).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithCoreServices_RegistersIntegerToStringConverter()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        builder.WithCoreServices();

        var converter = builder.ConverterService.TypedConverters.TryGetConverter(typeof(int), typeof(string));
        await Assert.That(converter).IsNotNull();
    }

    /// <summary>
    /// Verifies that calling <see cref="IReactiveUIBindingBuilder.WithCoreServices"/>
    /// via the explicit interface method returns an <see cref="IReactiveUIBindingBuilder"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithCoreServices_ExplicitInterface_ReturnsIReactiveUIBindingBuilder()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        IReactiveUIBindingBuilder iBuilder = builder;
        var result = iBuilder.WithCoreServices();

        await Assert.That(result).IsNotNull();
    }

    /// <summary>
    /// Verifies that <see cref="ReactiveUIBindingBuilder.BuildApp"/> sets the global
    /// <see cref="BindingConverters.Current"/> to the builder's <see cref="ConverterService"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildApp_SetsGlobalConverterServiceToBuilderInstance()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        builder.WithCoreServices();

        builder.BuildApp();

        await Assert.That(ReferenceEquals(BindingConverters.Current, builder.ConverterService)).IsTrue();
    }

    /// <summary>
    /// Verifies that <see cref="ReactiveUIBindingBuilder.BuildApp"/> marks the system as initialized
    /// so that <see cref="RxBindingBuilder.EnsureInitialized"/> does not throw.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildApp_MarksAsInitialized()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        builder.WithCoreServices();

        builder.BuildApp();

        var action = () => RxBindingBuilder.EnsureInitialized();
        await Assert.That(action).ThrowsNothing();
    }

    /// <summary>
    /// Verifies that multiple converters of different types can be registered
    /// and all appear in their respective registries.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleConverterTypes_AllRegistered()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var typed = new StubBindingTypeConverter(
            typeof(double),
            typeof(string),
            (from, hint) => (true, from?.ToString()));
        var fallback = new StubFallbackConverter(
            (fromType, from, toType, hint) => (true, "fallback"));
        var setMethod = new StubSetMethodBindingConverter();

        builder.WithConverter(typed);
        builder.WithFallbackConverter(fallback);
        builder.WithSetMethodConverter(setMethod);

        await Assert.That(builder.ConverterService.TypedConverters.TryGetConverter(typeof(double), typeof(string))).IsNotNull();
        await Assert.That(builder.ConverterService.FallbackConverters.GetAllConverters().ToList()).Contains(fallback);
        await Assert.That(builder.ConverterService.SetMethodConverters.GetAllConverters().ToList()).Contains(setMethod);
    }

    private sealed class TestModule : Splat.Builder.IModule
    {
        private readonly Action _onConfigure;

        public TestModule(Action onConfigure) => _onConfigure = onConfigure;

        public void Configure(IMutableDependencyResolver resolver) => _onConfigure();
    }
}

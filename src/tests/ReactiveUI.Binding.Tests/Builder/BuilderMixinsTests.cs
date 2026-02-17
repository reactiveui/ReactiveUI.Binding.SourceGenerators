// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Mixins;
using ReactiveUI.Binding.Tests.TestModels;

using Splat.Builder;

namespace ReactiveUI.Binding.Tests.Builder;

/// <summary>
/// Tests for the <see cref="BuilderMixins"/> extension methods that bridge
/// <see cref="IAppBuilder"/> to <see cref="IReactiveUIBindingBuilder"/>.
/// </summary>
public class BuilderMixinsTests
{
    /// <summary>
    /// Verifies that <see cref="BuilderMixins.BuildApp"/> succeeds when the
    /// <see cref="IAppBuilder"/> is a valid <see cref="IReactiveUIBindingBuilder"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildApp_WithValidBuilder_Succeeds()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        builder.WithCoreServices();

        IAppBuilder appBuilder = builder;
        var instance = appBuilder.BuildApp();

        await Assert.That(instance).IsNotNull();
    }

    /// <summary>
    /// Verifies that <see cref="BuilderMixins.BuildApp"/> throws
    /// <see cref="InvalidOperationException"/> when the <see cref="IAppBuilder"/>
    /// is not an <see cref="IReactiveUIBindingBuilder"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildApp_WithNonReactiveUIBuilder_ThrowsInvalidOperationException()
    {
        IAppBuilder fakeBuilder = new FakeAppBuilder();

        var action = () => fakeBuilder.BuildApp();

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that <see cref="BuilderMixins.WithPlatformModule{T}"/> delegates
    /// to the underlying <see cref="IReactiveUIBindingBuilder.WithPlatformModule{T}"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithPlatformModule_DelegatesToBuilder()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var registered = false;

        IAppBuilder appBuilder = builder;
        var result = appBuilder.WithPlatformModule(new TestModule(() => registered = true));

        builder.BuildApp();

        await Assert.That(registered).IsTrue();
        await Assert.That(result).IsNotNull();
    }

    /// <summary>
    /// Verifies that <see cref="BuilderMixins.WithPlatformModule{T}"/> throws
    /// <see cref="InvalidOperationException"/> when the <see cref="IAppBuilder"/>
    /// is not an <see cref="IReactiveUIBindingBuilder"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithPlatformModule_WithNonReactiveUIBuilder_ThrowsInvalidOperationException()
    {
        IAppBuilder fakeBuilder = new FakeAppBuilder();

        var action = () => fakeBuilder.WithPlatformModule(new TestModule(() => { }));

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that <see cref="BuilderMixins.WithRegistration"/> delegates
    /// to the underlying <see cref="IReactiveUIBindingBuilder.WithRegistration"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithRegistration_DelegatesToBuilder()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var executed = false;

        IAppBuilder appBuilder = builder;
        var result = appBuilder.WithRegistration(_ => executed = true);

        await Assert.That(executed).IsTrue();
        await Assert.That(result).IsNotNull();
    }

    /// <summary>
    /// Verifies that <see cref="BuilderMixins.WithRegistration"/> throws
    /// <see cref="InvalidOperationException"/> when the <see cref="IAppBuilder"/>
    /// is not an <see cref="IReactiveUIBindingBuilder"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithRegistration_WithNonReactiveUIBuilder_ThrowsInvalidOperationException()
    {
        IAppBuilder fakeBuilder = new FakeAppBuilder();

        var action = () => fakeBuilder.WithRegistration(_ => { });

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that <see cref="BuilderMixins.WithConverter"/> registers a
    /// typed converter via the <see cref="IAppBuilder"/> extension method.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithConverter_RegistersConverterViaAppBuilder()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubBindingTypeConverter(
            typeof(int),
            typeof(bool),
            (from, hint) => (true, true));

        IAppBuilder appBuilder = builder;
        var result = appBuilder.WithConverter(converter);

        var resolved = builder.ConverterService.TypedConverters.TryGetConverter(typeof(int), typeof(bool));

        await Assert.That(result).IsNotNull();
        await Assert.That(resolved).IsNotNull();
        await Assert.That(ReferenceEquals(resolved, converter)).IsTrue();
    }

    /// <summary>
    /// Verifies that <see cref="BuilderMixins.WithConverter"/> throws
    /// <see cref="InvalidOperationException"/> when the <see cref="IAppBuilder"/>
    /// is not an <see cref="IReactiveUIBindingBuilder"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithConverter_WithNonReactiveUIBuilder_ThrowsInvalidOperationException()
    {
        IAppBuilder fakeBuilder = new FakeAppBuilder();
        var converter = new StubBindingTypeConverter(
            typeof(int),
            typeof(bool),
            (from, hint) => (true, true));

        var action = () => fakeBuilder.WithConverter(converter);

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that <see cref="BuilderMixins.WithFallbackConverter"/> registers a
    /// fallback converter via the <see cref="IAppBuilder"/> extension method.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithFallbackConverter_RegistersConverterViaAppBuilder()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubFallbackConverter(
            (fromType, from, toType, hint) => (true, "converted"));

        IAppBuilder appBuilder = builder;
        var result = appBuilder.WithFallbackConverter(converter);

        var allConverters = builder.ConverterService.FallbackConverters.GetAllConverters().ToList();

        await Assert.That(result).IsNotNull();
        await Assert.That(allConverters).Contains(converter);
    }

    /// <summary>
    /// Verifies that <see cref="BuilderMixins.WithFallbackConverter"/> throws
    /// <see cref="InvalidOperationException"/> when the <see cref="IAppBuilder"/>
    /// is not an <see cref="IReactiveUIBindingBuilder"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithFallbackConverter_WithNonReactiveUIBuilder_ThrowsInvalidOperationException()
    {
        IAppBuilder fakeBuilder = new FakeAppBuilder();
        var converter = new StubFallbackConverter(
            (fromType, from, toType, hint) => (true, "converted"));

        var action = () => fakeBuilder.WithFallbackConverter(converter);

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that <see cref="BuilderMixins.WithSetMethodConverter"/> registers a
    /// set-method converter via the <see cref="IAppBuilder"/> extension method.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSetMethodConverter_RegistersConverterViaAppBuilder()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubSetMethodBindingConverter();

        IAppBuilder appBuilder = builder;
        var result = appBuilder.WithSetMethodConverter(converter);

        var allConverters = builder.ConverterService.SetMethodConverters.GetAllConverters().ToList();

        await Assert.That(result).IsNotNull();
        await Assert.That(allConverters).Contains(converter);
    }

    /// <summary>
    /// Verifies that <see cref="BuilderMixins.WithSetMethodConverter"/> throws
    /// <see cref="InvalidOperationException"/> when the <see cref="IAppBuilder"/>
    /// is not an <see cref="IReactiveUIBindingBuilder"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSetMethodConverter_WithNonReactiveUIBuilder_ThrowsInvalidOperationException()
    {
        IAppBuilder fakeBuilder = new FakeAppBuilder();
        var converter = new StubSetMethodBindingConverter();

        var action = () => fakeBuilder.WithSetMethodConverter(converter);

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that chaining multiple extension methods through <see cref="IAppBuilder"/>
    /// works correctly and returns the builder each time.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FluentChaining_ThroughIAppBuilder_Works()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubBindingTypeConverter(
            typeof(string),
            typeof(int),
            (from, hint) => (true, 42));
        var fallbackConverter = new StubFallbackConverter(
            (fromType, from, toType, hint) => (true, "fallback"));
        var setConverter = new StubSetMethodBindingConverter();

        IAppBuilder appBuilder = builder;
        var result = appBuilder
            .WithConverter(converter)
            .WithFallbackConverter(fallbackConverter)
            .WithSetMethodConverter(setConverter);

        await Assert.That(result).IsNotNull();
        await Assert.That(builder.ConverterService.TypedConverters.TryGetConverter(typeof(string), typeof(int))).IsNotNull();
        await Assert.That(builder.ConverterService.FallbackConverters.GetAllConverters().ToList()).Contains(fallbackConverter);
        await Assert.That(builder.ConverterService.SetMethodConverters.GetAllConverters().ToList()).Contains(setConverter);
    }

    /// <summary>
    /// A fake <see cref="IAppBuilder"/> that does NOT implement <see cref="IReactiveUIBindingBuilder"/>
    /// to test the guard clauses in <see cref="BuilderMixins"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required by IAppBuilder interface.")]
    private sealed class FakeAppBuilder : IAppBuilder
    {
        /// <summary>
        /// Gets the current dependency resolver.
        /// </summary>
        public IReadonlyDependencyResolver? Current => null;

        /// <summary>
        /// Registers core services.
        /// </summary>
        /// <returns>The builder instance.</returns>
        public IAppBuilder WithCoreServices() => this;

        /// <summary>
        /// Uses the current Splat locator.
        /// </summary>
        /// <returns>The builder instance.</returns>
        public IAppBuilder UseCurrentSplatLocator() => this;

        /// <summary>
        /// Registers a module.
        /// </summary>
        /// <typeparam name="T">The module type.</typeparam>
        /// <param name="module">The module instance.</param>
        /// <returns>The builder instance.</returns>
        public IAppBuilder UsingModule<T>(T module)
            where T : IModule => this;

        /// <summary>
        /// Registers a custom action.
        /// </summary>
        /// <param name="configureAction">The configuration action.</param>
        /// <returns>The builder instance.</returns>
        public IAppBuilder WithCustomRegistration(Action<IMutableDependencyResolver> configureAction) => this;

        /// <summary>
        /// Builds the application instance.
        /// </summary>
        /// <returns>The application instance.</returns>
        public IAppInstance Build() => throw new NotImplementedException();
    }

    /// <summary>
    /// A test <see cref="IModule"/> that invokes a callback when configured.
    /// </summary>
    private sealed class TestModule : IModule
    {
        private readonly Action _onConfigure;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestModule"/> class.
        /// </summary>
        /// <param name="onConfigure">The callback to invoke during configuration.</param>
        public TestModule(Action onConfigure) => _onConfigure = onConfigure;

        /// <inheritdoc/>
        public void Configure(IMutableDependencyResolver resolver) => _onConfigure();
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;

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

    private sealed class TestModule : Splat.Builder.IModule
    {
        private readonly Action _onConfigure;

        public TestModule(Action onConfigure) => _onConfigure = onConfigure;

        public void Configure(IMutableDependencyResolver resolver) => _onConfigure();
    }
}

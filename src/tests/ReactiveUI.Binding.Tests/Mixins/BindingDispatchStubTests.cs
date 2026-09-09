// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>Covers the binding overloads that exist only to be replaced by a generated one.</summary>
/// <remarks>
/// Unlike the observation surface, a binding has no runtime fallback: resolving one needs the two lambdas at
/// compile time. Each overload here therefore refuses the call rather than binding something. They are reached
/// on the declaring class rather than as extension methods, because written as an extension call the generated
/// dispatch wins overload resolution and the refusal never happens.
/// </remarks>
public class BindingDispatchStubTests
{
    /// <summary>The source of a binding.</summary>
    private readonly DispatchStubViewModel _viewModel = new();

    /// <summary>The target of a binding.</summary>
    private readonly DispatchStubView _view = new();

    /// <summary>The stream a BindTo call writes from.</summary>
    private readonly ManualObservable<string> _source = new();

    /// <summary>A one-way binding with no generated overload refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_WithNoGeneratedOverload_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.BindOneWay(
                _viewModel,
                _view,
                x => x.Caption,
                x => x.Caption))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>A converting one-way binding with no generated overload refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_ConvertingWithNoGeneratedOverload_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.BindOneWay(
                _viewModel,
                _view,
                x => x.Caption,
                x => x.Caption,
                static value => value))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>A two-way binding with no generated overload refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_WithNoGeneratedOverload_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.BindTwoWay(
                _viewModel,
                _view,
                x => x.Caption,
                x => x.Caption))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>A converting two-way binding with no generated overload refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_ConvertingWithNoGeneratedOverload_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.BindTwoWay(
                _viewModel,
                _view,
                x => x.Caption,
                x => x.Caption,
                static value => value,
                static value => value))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>The view-first spelling of a one-way binding refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_WithNoGeneratedOverload_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.OneWayBind(
                _view,
                _viewModel,
                x => x.Caption,
                x => x.Caption))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>The converting view-first spelling of a one-way binding refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_ConvertingWithNoGeneratedOverload_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.OneWayBind(
                _view,
                _viewModel,
                x => x.Caption,
                x => x.Caption,
                static value => value))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>The view-first spelling of a two-way binding refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_WithNoGeneratedOverload_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.Bind(
                _view,
                _viewModel,
                x => x.Caption,
                x => x.Caption))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>The converting view-first spelling of a two-way binding refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_ConvertingWithNoGeneratedOverload_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.Bind(
                _view,
                _viewModel,
                x => x.Caption,
                x => x.Caption,
                static value => value,
                static value => value))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>Writing a stream into a property with no generated overload refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_WithNoGeneratedOverload_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.BindTo(
                _source,
                _view,
                x => x.Caption))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>Writing a stream into a property with a conversion hint refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_WithAConversionHint_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.BindTo(
                _source,
                _view,
                x => x.Caption,
                conversionHint: null))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>Writing a stream into a property with a converter refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_WithAConverter_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.BindTo(
                _source,
                _view,
                x => x.Caption,
                converterOverride: null))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>Writing a stream into a property with both a hint and a converter refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_WithAConversionHintAndAConverter_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.BindTo(
                _source,
                _view,
                x => x.Caption,
                conversionHint: null,
                converterOverride: null))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>A command binding with no generated overload refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_WithNoGeneratedOverload_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.BindCommand(
                _view,
                _viewModel,
                x => x.Run,
                x => x.Control))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>A command binding taking its parameter from a stream refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_WithAParameterStream_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.BindCommand(
                _view,
                _viewModel,
                x => x.Run,
                x => x.Control,
                _source))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>A command binding taking its parameter from a property refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_WithAParameterProperty_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.BindCommand(
                _view,
                _viewModel,
                x => x.Run,
                x => x.Control,
                x => x.Parameter))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>An interaction binding handled asynchronously refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_WithAnAsynchronousHandler_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.BindInteraction(
                _view,
                _viewModel,
                x => x.Confirm,
                static context => Task.CompletedTask))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>An interaction binding handled by a stream refuses the call.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_WithAStreamHandler_ThrowsInvalidOperationException() =>
        await Assert.That(() => ReactiveUIBindingExtensions.BindInteraction(
                _view,
                _viewModel,
                x => x.Confirm,
                static IObservable<string> (context) => new ManualObservable<string>()))
            .ThrowsExactly<InvalidOperationException>();
}

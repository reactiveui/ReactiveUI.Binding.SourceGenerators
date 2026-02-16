// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Disposables;
using System.Reactive.Linq;

using ReactiveUI.Binding;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>
/// Tests for <see cref="ReactiveBinding{TView, TValue}"/>.
/// </summary>
public class ReactiveBindingTests
{
    /// <summary>
    /// Verifies that Dispose disposes the underlying subscription.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_DisposesSubscription()
    {
        bool disposed = false;
        var subscription = Disposable.Create(() => disposed = true);
        var view = new FakeView();
        var changed = Observable.Empty<string>();

        var binding = new ReactiveBinding<FakeView, string>(
            view, changed, BindingDirection.OneWay, subscription);

        binding.Dispose();

        await Assert.That(disposed).IsTrue();
    }

    /// <summary>
    /// Verifies that double-disposal does not throw.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_Twice_DoesNotThrow()
    {
        int disposeCount = 0;
        var subscription = Disposable.Create(() => disposeCount++);
        var view = new FakeView();
        var changed = Observable.Empty<string>();

        var binding = new ReactiveBinding<FakeView, string>(
            view, changed, BindingDirection.OneWay, subscription);

        binding.Dispose();
        binding.Dispose();

        await Assert.That(disposeCount).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that constructor values are returned by properties.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Properties_ReturnConstructorValues()
    {
        var view = new FakeView();
        var changed = Observable.Empty<int>();
        var subscription = Disposable.Empty;

        var binding = new ReactiveBinding<FakeView, int>(
            view, changed, BindingDirection.TwoWay, subscription);

        await Assert.That(binding.View).IsEqualTo(view);
        await Assert.That(binding.Direction).IsEqualTo(BindingDirection.TwoWay);
        await Assert.That(binding.Changed).IsEqualTo(changed);
        await Assert.That(binding.ViewModelExpression).IsNull();
        await Assert.That(binding.ViewExpression).IsNull();
    }

    /// <summary>
    /// A minimal fake view for testing.
    /// </summary>
    private sealed class FakeView : IViewFor
    {
        /// <inheritdoc/>
        public object? ViewModel { get; set; }
    }
}

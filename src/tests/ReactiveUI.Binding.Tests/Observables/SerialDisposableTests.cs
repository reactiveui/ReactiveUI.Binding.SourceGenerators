// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Observables;

using SerialDisposable = ReactiveUI.Binding.Observables.SerialDisposable;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Tests for <see cref="SerialDisposable"/>.
/// </summary>
public class SerialDisposableTests
{
    /// <summary>
    /// Verifies that setting a new disposable disposes the previous one.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SetDisposable_DisposesPrevious()
    {
        var serial = new SerialDisposable();
        var disposed1 = 0;
        var disposed2 = 0;

        serial.Disposable = new ActionDisposable(() => disposed1++);
        serial.Disposable = new ActionDisposable(() => disposed2++);

        await Assert.That(disposed1).IsEqualTo(1);
        await Assert.That(disposed2).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that disposing the serial disposable disposes the current inner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_DisposesInner()
    {
        var serial = new SerialDisposable();
        var disposed = 0;
        serial.Disposable = new ActionDisposable(() => disposed++);

        serial.Dispose();
        await Assert.That(disposed).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that setting a disposable after the serial is disposed, disposes it immediately.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SetAfterDispose_DisposesImmediately()
    {
        var serial = new SerialDisposable();
        serial.Dispose();

        var disposed = 0;
        serial.Disposable = new ActionDisposable(() => disposed++);
        await Assert.That(disposed).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that disposing twice does not double-dispose the inner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_Twice_NoDoubleFree()
    {
        var serial = new SerialDisposable();
        var disposed = 0;
        serial.Disposable = new ActionDisposable(() => disposed++);

        serial.Dispose();
        serial.Dispose();
        await Assert.That(disposed).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that the Disposable getter returns the currently set disposable.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Disposable_Getter_ReturnsCurrentValue()
    {
        var serial = new SerialDisposable();
        await Assert.That(serial.Disposable).IsNull();

        var inner = new ActionDisposable(() => { });
        serial.Disposable = inner;
        await Assert.That(serial.Disposable).IsSameReferenceAs(inner);
    }

    /// <summary>
    /// Verifies that setting Disposable to null disposes the old value.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SetToNull_DisposesOldValue()
    {
        var serial = new SerialDisposable();
        var disposed = 0;
        serial.Disposable = new ActionDisposable(() => disposed++);

        serial.Disposable = null;
        await Assert.That(disposed).IsEqualTo(1);
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="CompositeDisposable2"/>.
/// </summary>
public class CompositeDisposable2Tests
{
    /// <summary>
    /// Verifies that Dispose() disposes both inner disposables.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_DisposesBothInners()
    {
        var d1Disposed = 0;
        var d2Disposed = 0;
        var d1 = new ActionDisposable(() => d1Disposed++);
        var d2 = new ActionDisposable(() => d2Disposed++);
        var composite = new CompositeDisposable2(d1, d2);

        composite.Dispose();

        await Assert.That(d1Disposed).IsEqualTo(1);
        await Assert.That(d2Disposed).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that multiple calls to Dispose() only dispose the inner disposables once.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_MultipleTimes_DisposesInnersOnlyOnce()
    {
        var d1Disposed = 0;
        var d2Disposed = 0;
        var d1 = new ActionDisposable(() => d1Disposed++);
        var d2 = new ActionDisposable(() => d2Disposed++);
        var composite = new CompositeDisposable2(d1, d2);

        composite.Dispose();
        composite.Dispose();

        await Assert.That(d1Disposed).IsEqualTo(1);
        await Assert.That(d2Disposed).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that the constructor throws <see cref="ArgumentNullException"/> when d1 is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullD1_ThrowsArgumentNullException()
    {
        var action = () => new CompositeDisposable2(null!, EmptyDisposable.Instance);

        await Assert.That(action).Throws<ArgumentNullException>().WithParameterName("d1");
    }

    /// <summary>
    /// Verifies that the constructor throws <see cref="ArgumentNullException"/> when d2 is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullD2_ThrowsArgumentNullException()
    {
        var action = () => new CompositeDisposable2(EmptyDisposable.Instance, null!);

        await Assert.That(action).Throws<ArgumentNullException>().WithParameterName("d2");
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
///     Tests for the <see cref="ReactiveUI.Binding.Observables.ActionDisposable"/> class which invokes an action on disposal.
/// </summary>
public class ActionDisposableTests
{
    /// <summary>
    ///     Verifies that the action is invoked when Dispose is called.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Dispose_InvokesAction()
    {
        var invoked = false;
        var disposable = new ActionDisposable(() => invoked = true);

        disposable.Dispose();

        await Assert.That(invoked).IsTrue();
    }

    /// <summary>
    ///     Verifies that the action is invoked only once even if Dispose is called multiple times.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Dispose_CalledTwice_InvokesActionOnlyOnce()
    {
        var invokeCount = 0;
        var disposable = new ActionDisposable(() => Interlocked.Increment(ref invokeCount));

        disposable.Dispose();
        disposable.Dispose();

        await Assert.That(invokeCount).IsEqualTo(1);
    }

    /// <summary>
    ///     Verifies that constructor throws ArgumentNullException when action is null.
    /// </summary>
    [Test]
    public void Constructor_NullAction_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ActionDisposable(null!));
    }
}

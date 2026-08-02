// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Wpf.Tests;

/// <summary>
/// Tests for the state-carrying teardown disposable the platform observers unhook with. It is compiled
/// privately into each platform assembly rather than shared, so it is exercised through one of them.
/// </summary>
public class ActionDisposableTests
{
    /// <summary>The state value handed to the teardown action.</summary>
    private const int StateValue = 42;

    /// <summary>The number of times a teardown action may run however often Dispose is called.</summary>
    private const int ExpectedRuns = 1;

    /// <summary>Verifies that disposing runs the teardown action against the state it was given.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Dispose_RunsTheActionWithItsState()
    {
        var seen = 0;
        var disposable = new ActionDisposable<int>(StateValue, state => seen = state);

        disposable.Dispose();

        await Assert.That(seen).IsEqualTo(StateValue);
    }

    /// <summary>Verifies that a second Dispose is a no-op, so an observer cannot unhook twice.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Dispose_CalledTwice_RunsTheActionOnce()
    {
        var runs = 0;
        var disposable = new ActionDisposable<int>(StateValue, _ => runs++);

        disposable.Dispose();
        disposable.Dispose();

        await Assert.That(runs).IsEqualTo(ExpectedRuns);
    }

    /// <summary>Verifies that a null teardown action is rejected at construction, not at disposal.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Constructor_NullAction_ThrowsArgumentNullException() =>
        await Assert.That(static () => new ActionDisposable<int>(StateValue, null!))
            .ThrowsExactly<ArgumentNullException>();
}

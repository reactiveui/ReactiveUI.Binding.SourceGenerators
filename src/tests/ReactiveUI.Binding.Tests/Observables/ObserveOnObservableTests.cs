// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>Tests for <see cref="ObserveOnObservable{T}"/>, which forwards notifications on a sequencer.</summary>
/// <remarks>Scheduling runs on <see cref="ImmediateSequencer"/> so delivery is observable without waiting.</remarks>
public class ObserveOnObservableTests
{
    /// <summary>The second value pushed through the source observable.</summary>
    private const int SecondValue = 2;

    /// <summary>The third value pushed through the source observable.</summary>
    private const int ThirdValue = 3;

    /// <summary>The index of the third received value.</summary>
    private const int ThirdIndex = 2;

    /// <summary>The expected number of forwarded notifications when three values are pushed.</summary>
    private const int ExpectedThreeCount = 3;

    /// <summary>Verifies that OnNext notifications reach the observer in order.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Subscribe_ForwardsOnNextNotifications()
    {
        var subject = new Subject<int>();
        var observable = new ObserveOnObservable<int>(subject, ImmediateSequencer.Instance);
        var received = new List<int>();

        using var subscription = observable.Subscribe(received.Add);

        subject.OnNext(1);
        subject.OnNext(SecondValue);
        subject.OnNext(ThirdValue);

        await Assert.That(received.Count).IsEqualTo(ExpectedThreeCount);
        await Assert.That(received[0]).IsEqualTo(1);
        await Assert.That(received[1]).IsEqualTo(SecondValue);
        await Assert.That(received[ThirdIndex]).IsEqualTo(ThirdValue);
    }

    /// <summary>Verifies that completion reaches the observer.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Subscribe_ForwardsOnCompletedNotification()
    {
        var subject = new Subject<int>();
        var observable = new ObserveOnObservable<int>(subject, ImmediateSequencer.Instance);
        var completed = false;

        using var subscription = observable.Subscribe(static _ => { }, () => completed = true);

        subject.OnCompleted();

        await Assert.That(completed).IsTrue();
    }

    /// <summary>Verifies that an error reaches the observer.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Subscribe_ForwardsOnErrorNotification()
    {
        var subject = new Subject<int>();
        var observable = new ObserveOnObservable<int>(subject, ImmediateSequencer.Instance);
        Exception? actual = null;

        using var subscription = observable.Subscribe(static _ => { }, error => actual = error);

        subject.OnError(new InvalidOperationException("test error"));

        await Assert.That(actual?.Message).IsEqualTo("test error");
    }

    /// <summary>Verifies that disposing the subscription detaches it from the source.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Subscribe_DisposalStopsNotifications()
    {
        var subject = new Subject<int>();
        var observable = new ObserveOnObservable<int>(subject, ImmediateSequencer.Instance);
        var received = new List<int>();

        var subscription = observable.Subscribe(received.Add);

        subject.OnNext(1);
        subscription.Dispose();
        subject.OnNext(SecondValue);

        await Assert.That(received.Count).IsEqualTo(1);
    }

    /// <summary>Verifies that Subscribe rejects a null observer.</summary>
    [Test]
    public void Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        var observable = new ObserveOnObservable<int>(new Subject<int>(), ImmediateSequencer.Instance);

        _ = Assert.Throws<ArgumentNullException>(() => observable.Subscribe(null!));
    }

    /// <summary>Verifies that the constructor rejects a null source.</summary>
    [Test]
    public void Constructor_NullSource_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(static () => _ = new ObserveOnObservable<int>(null!, ImmediateSequencer.Instance));

    /// <summary>Verifies that the constructor rejects a null sequencer.</summary>
    [Test]
    public void Constructor_NullSequencer_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(static () => _ = new ObserveOnObservable<int>(new Subject<int>(), null!));
}

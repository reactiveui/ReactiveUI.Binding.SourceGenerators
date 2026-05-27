// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using ReactiveUI.Binding.Reactive;

namespace ReactiveUI.Binding.Tests.Reactive;

/// <summary>
///     Tests for the <see cref="ObserveOnObservable{T}"/> class which forwards notifications on a scheduler.
/// </summary>
public class ObserveOnObservableTests
{
    /// <summary>
    ///     The second value pushed through the source observable.
    /// </summary>
    private const int SecondValue = 2;

    /// <summary>
    ///     The third value pushed through the source observable.
    /// </summary>
    private const int ThirdValue = 3;

    /// <summary>
    ///     The index of the third received value.
    /// </summary>
    private const int ThirdIndex = 2;

    /// <summary>
    ///     The expected number of forwarded notifications when three values are pushed.
    /// </summary>
    private const int ExpectedThreeCount = 3;

    /// <summary>
    ///     The delay, in milliseconds, allowed for the scheduler to process notifications.
    /// </summary>
    private const int SchedulerProcessingDelayMs = 50;

    /// <summary>
    ///     Verifies that notifications are forwarded to the observer.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Subscribe_ForwardsOnNextNotifications()
    {
        var subject = new Subject<int>();
        var scheduler = new EventLoopScheduler();
        var observable = new ObserveOnObservable<int>(subject, scheduler);
        var received = new List<int>();
        var completed = new TaskCompletionSource<bool>();

        observable.Subscribe(
            received.Add,
            () => completed.SetResult(true));

        subject.OnNext(1);
        subject.OnNext(SecondValue);
        subject.OnNext(ThirdValue);
        subject.OnCompleted();

        await completed.Task;
        scheduler.Dispose();

        await Assert.That(received.Count).IsEqualTo(ExpectedThreeCount);
        await Assert.That(received[0]).IsEqualTo(1);
        await Assert.That(received[1]).IsEqualTo(SecondValue);
        await Assert.That(received[ThirdIndex]).IsEqualTo(ThirdValue);
    }

    /// <summary>
    ///     Verifies that OnError is forwarded to the observer.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Subscribe_ForwardsOnErrorNotification()
    {
        var subject = new Subject<int>();
        var scheduler = new EventLoopScheduler();
        var observable = new ObserveOnObservable<int>(subject, scheduler);
        var errorReceived = new TaskCompletionSource<Exception>();

        observable.Subscribe(
            _ => { },
            errorReceived.SetResult);

        var expected = new InvalidOperationException("test error");
        subject.OnError(expected);

        var actual = await errorReceived.Task;
        scheduler.Dispose();

        await Assert.That(actual.Message).IsEqualTo("test error");
    }

    /// <summary>
    ///     Verifies that disposal stops notifications.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Subscribe_DisposalStopsNotifications()
    {
        var subject = new Subject<int>();
        var scheduler = new EventLoopScheduler();
        var observable = new ObserveOnObservable<int>(subject, scheduler);
        var received = new List<int>();

        var subscription = observable.Subscribe(received.Add);

        subject.OnNext(1);

        // Allow time for scheduler to process
        await Task.Delay(SchedulerProcessingDelayMs);

        subscription.Dispose();

        subject.OnNext(SecondValue); // Should not be received

        // Allow time for any potential delivery
        await Task.Delay(SchedulerProcessingDelayMs);
        scheduler.Dispose();

        await Assert.That(received.Count).IsEqualTo(1);
    }

    /// <summary>
    ///     Verifies that Subscribe throws ArgumentNullException when observer is null.
    /// </summary>
    [Test]
    public void Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        var subject = new Subject<int>();
        var scheduler = new EventLoopScheduler();
        var observable = new ObserveOnObservable<int>(subject, scheduler);

        Assert.Throws<ArgumentNullException>(() => observable.Subscribe(null!));
        scheduler.Dispose();
    }

    /// <summary>
    ///     Verifies that constructor throws when source is null.
    /// </summary>
    [Test]
    public void Constructor_NullSource_ThrowsArgumentNullException()
    {
        var scheduler = new EventLoopScheduler();
        Assert.Throws<ArgumentNullException>(() => _ = new ObserveOnObservable<int>(null!, scheduler));
        scheduler.Dispose();
    }

    /// <summary>
    ///     Verifies that constructor throws when scheduler is null.
    /// </summary>
    [Test]
    public void Constructor_NullScheduler_ThrowsArgumentNullException()
    {
        var subject = new Subject<int>();
        Assert.Throws<ArgumentNullException>(() => _ = new ObserveOnObservable<int>(subject, null!));
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="CombineLatest14Observable{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult}"/> edge cases.
/// </summary>
public class CombineLatest14ObservableTests
{
    /// <summary>
    /// Verifies that a null first source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource1_Throws()
    {
        var act = () => CombineLatestObservable.Create(
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null second source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource2_Throws()
    {
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null third source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource3_Throws()
    {
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null fourth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource4_Throws()
    {
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null fifth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource5_Throws()
    {
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null sixth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource6_Throws()
    {
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null seventh source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource7_Throws()
    {
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null eighth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource8_Throws()
    {
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null ninth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource9_Throws()
    {
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null tenth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource10_Throws()
    {
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null eleventh source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource11_Throws()
    {
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null twelfth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource12_Throws()
    {
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null thirteenth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource13_Throws()
    {
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null fourteenth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource14_Throws()
    {
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null result selector throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullResultSelector_Throws()
    {
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>)null!);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that subscribing with a null observer throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_Throws()
    {
        var combined = CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        var act = () => combined.Subscribe(null!);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that disposing twice does not throw an exception.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_CalledTwice_NoException()
    {
        var combined = CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        var subscription = combined.Subscribe(new AnonymousObserver<int>(_ => { }, _ => { }, () => { }));
        subscription.Dispose();
        subscription.Dispose();

        await Assert.That(subscription.GetType()).IsNotNull();
    }

    /// <summary>
    /// Verifies that a source emitting after dispose does not throw.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SourceEmitsAfterDispose_NoException()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        var results = new List<int>();
        var subscription = combined.Subscribe(new AnonymousObserver<int>(results.Add, _ => { }, () => { }));

        source1.OnNext(1);
        source2.OnNext(1);
        source3.OnNext(1);
        source4.OnNext(1);
        source5.OnNext(1);
        source6.OnNext(1);
        source7.OnNext(1);
        source8.OnNext(1);
        source9.OnNext(1);
        source10.OnNext(1);
        source11.OnNext(1);
        source12.OnNext(1);
        source13.OnNext(1);
        source14.OnNext(1);
        subscription.Dispose();

        source1.OnNext(10);

        await Assert.That(results).Count().IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that an error after dispose is ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorAfterDispose_IsIgnored()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        Exception? receivedError = null;
        var subscription = combined.Subscribe(new AnonymousObserver<int>(
            _ => { },
            ex => receivedError = ex,
            () => { }));

        subscription.Dispose();
        source1.OnError(new InvalidOperationException("should be ignored"));
        source2.OnError(new InvalidOperationException("should be ignored"));
        source3.OnError(new InvalidOperationException("should be ignored"));
        source4.OnError(new InvalidOperationException("should be ignored"));
        source5.OnError(new InvalidOperationException("should be ignored"));
        source6.OnError(new InvalidOperationException("should be ignored"));
        source7.OnError(new InvalidOperationException("should be ignored"));
        source8.OnError(new InvalidOperationException("should be ignored"));
        source9.OnError(new InvalidOperationException("should be ignored"));
        source10.OnError(new InvalidOperationException("should be ignored"));
        source11.OnError(new InvalidOperationException("should be ignored"));
        source12.OnError(new InvalidOperationException("should be ignored"));
        source13.OnError(new InvalidOperationException("should be ignored"));
        source14.OnError(new InvalidOperationException("should be ignored"));

        await Assert.That(receivedError).IsNull();
    }

    /// <summary>
    /// Verifies that observer OnError and TryEmit null-conditional branches are covered
    /// when the subscription has been disposed but the observers are still reachable.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorAfterDispose_AllObservers_CoverNullPath()
    {
        var manual1 = new ManualObservable<int>();
        var manual2 = new ManualObservable<int>();
        var manual3 = new ManualObservable<int>();
        var manual4 = new ManualObservable<int>();
        var manual5 = new ManualObservable<int>();
        var manual6 = new ManualObservable<int>();
        var manual7 = new ManualObservable<int>();
        var manual8 = new ManualObservable<int>();
        var manual9 = new ManualObservable<int>();
        var manual10 = new ManualObservable<int>();
        var manual11 = new ManualObservable<int>();
        var manual12 = new ManualObservable<int>();
        var manual13 = new ManualObservable<int>();
        var manual14 = new ManualObservable<int>();
        var combined = CombineLatestObservable.Create(
            manual1,
            manual2,
            manual3,
            manual4,
            manual5,
            manual6,
            manual7,
            manual8,
            manual9,
            manual10,
            manual11,
            manual12,
            manual13,
            manual14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => a + b + c + d + e + f + g + h + i + j + k + l + m + n);

        Exception? receivedError = null;
        var subscription = combined.Subscribe(new AnonymousObserver<int>(
            _ => { },
            ex => receivedError = ex,
            () => { }));

        // Set all has-value flags so TryEmit reaches _observer?.OnNext
        manual1.Observer!.OnNext(1);
        manual2.Observer!.OnNext(2);
        manual3.Observer!.OnNext(3);
        manual4.Observer!.OnNext(4);
        manual5.Observer!.OnNext(5);
        manual6.Observer!.OnNext(6);
        manual7.Observer!.OnNext(7);
        manual8.Observer!.OnNext(8);
        manual9.Observer!.OnNext(9);
        manual10.Observer!.OnNext(10);
        manual11.Observer!.OnNext(11);
        manual12.Observer!.OnNext(12);
        manual13.Observer!.OnNext(13);
        manual14.Observer!.OnNext(14);

        subscription.Dispose();

        // These reach the observers because ManualObservable retains references
        manual1.Observer.OnNext(10);
        manual2.Observer.OnNext(20);
        manual3.Observer.OnNext(30);
        manual4.Observer.OnNext(40);
        manual5.Observer.OnNext(50);
        manual6.Observer.OnNext(60);
        manual7.Observer.OnNext(70);
        manual8.Observer.OnNext(80);
        manual9.Observer.OnNext(90);
        manual10.Observer.OnNext(100);
        manual11.Observer.OnNext(110);
        manual12.Observer.OnNext(120);
        manual13.Observer.OnNext(130);
        manual14.Observer.OnNext(140);
        manual1.Observer.OnError(new InvalidOperationException("should be ignored"));
        manual2.Observer.OnError(new InvalidOperationException("should be ignored"));
        manual3.Observer.OnError(new InvalidOperationException("should be ignored"));
        manual4.Observer.OnError(new InvalidOperationException("should be ignored"));
        manual5.Observer.OnError(new InvalidOperationException("should be ignored"));
        manual6.Observer.OnError(new InvalidOperationException("should be ignored"));
        manual7.Observer.OnError(new InvalidOperationException("should be ignored"));
        manual8.Observer.OnError(new InvalidOperationException("should be ignored"));
        manual9.Observer.OnError(new InvalidOperationException("should be ignored"));
        manual10.Observer.OnError(new InvalidOperationException("should be ignored"));
        manual11.Observer.OnError(new InvalidOperationException("should be ignored"));
        manual12.Observer.OnError(new InvalidOperationException("should be ignored"));
        manual13.Observer.OnError(new InvalidOperationException("should be ignored"));
        manual14.Observer.OnError(new InvalidOperationException("should be ignored"));

        await Assert.That(receivedError).IsNull();
    }

    /// <summary>
    /// Verifies that an error in the first source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSource1_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source1 error");
        source1.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that an error in the second source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSource2_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source2 error");
        source2.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that an error in the third source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSource3_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source3 error");
        source3.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that an error in the fourth source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSource4_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source4 error");
        source4.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that an error in the fifth source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSource5_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source5 error");
        source5.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that an error in the sixth source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSource6_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source6 error");
        source6.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that an error in the seventh source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSource7_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source7 error");
        source7.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that an error in the eighth source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSource8_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source8 error");
        source8.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that an error in the ninth source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSource9_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source9 error");
        source9.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that an error in the tenth source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSource10_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source10 error");
        source10.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that an error in the eleventh source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSource11_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source11 error");
        source11.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that an error in the twelfth source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSource12_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source12 error");
        source12.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that an error in the thirteenth source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSource13_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source13 error");
        source13.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that an error in the fourteenth source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSource14_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source14 error");
        source14.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that OnCompleted from a single source does not propagate completion.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CompletedInSource_DoesNotPropagateCompletion()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();
        var source4 = new Subject<int>();
        var source5 = new Subject<int>();
        var source6 = new Subject<int>();
        var source7 = new Subject<int>();
        var source8 = new Subject<int>();
        var source9 = new Subject<int>();
        var source10 = new Subject<int>();
        var source11 = new Subject<int>();
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            source9,
            source10,
            source11,
            source12,
            source13,
            source14,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}");

        var completed = false;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            _ => { },
            () => completed = true));

        source1.OnNext(1);
        source2.OnNext(2);
        source3.OnNext(3);
        source4.OnNext(4);
        source5.OnNext(5);
        source6.OnNext(6);
        source7.OnNext(7);
        source8.OnNext(8);
        source9.OnNext(9);
        source10.OnNext(10);
        source11.OnNext(11);
        source12.OnNext(12);
        source13.OnNext(13);
        source14.OnNext(14);

        source1.OnCompleted();
        source2.OnCompleted();
        source3.OnCompleted();
        source4.OnCompleted();
        source5.OnCompleted();
        source6.OnCompleted();
        source7.OnCompleted();
        source8.OnCompleted();
        source9.OnCompleted();
        source10.OnCompleted();
        source11.OnCompleted();
        source12.OnCompleted();
        source13.OnCompleted();
        source14.OnCompleted();

        await Assert.That(completed).IsFalse();
        await Assert.That(results).Count().IsEqualTo(1);
    }

    private sealed class AnonymousObserver<T> : IObserver<T>
    {
        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;

        public AnonymousObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        public void OnCompleted() => _onCompleted();

        public void OnError(Exception error) => _onError(error);

        public void OnNext(T value) => _onNext(value);
    }
}

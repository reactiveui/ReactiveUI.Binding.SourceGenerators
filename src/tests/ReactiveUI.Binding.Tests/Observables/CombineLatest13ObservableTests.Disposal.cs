// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Disposal and post-dispose behavior tests for <see cref="CombineLatest13Observable{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult}"/>.
/// </summary>
public partial class CombineLatest13ObservableTests
{
    /// <summary>
    /// Verifies that disposing twice does not throw an exception.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
            (a, b, c, d, e, f, g, h, i, j, k, l, m) => a + b + c + d + e + f + g + h + i + j + k + l + m);

        var subscription = combined.Subscribe(new AnonymousObserver<int>(_ => { }, _ => { }, () => { }));
        subscription.Dispose();
        subscription.Dispose();

        await Assert.That(subscription.GetType()).IsNotNull();
    }

    /// <summary>
    /// Verifies that a source emitting after dispose does not throw.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
            (a, b, c, d, e, f, g, h, i, j, k, l, m) => a + b + c + d + e + f + g + h + i + j + k + l + m);

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
        subscription.Dispose();

        source1.OnNext(PostDisposeValue1);

        await Assert.That(results).Count().IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that an error after dispose is ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
            (a, b, c, d, e, f, g, h, i, j, k, l, m) => a + b + c + d + e + f + g + h + i + j + k + l + m);

        Exception? receivedError = null;
        var subscription = combined.Subscribe(new AnonymousObserver<int>(
            _ => { },
            ex => receivedError = ex,
            () => { }));

        subscription.Dispose();
        source1.OnError(new InvalidOperationException(IgnoredErrorMessage));
        source2.OnError(new InvalidOperationException(IgnoredErrorMessage));
        source3.OnError(new InvalidOperationException(IgnoredErrorMessage));
        source4.OnError(new InvalidOperationException(IgnoredErrorMessage));
        source5.OnError(new InvalidOperationException(IgnoredErrorMessage));
        source6.OnError(new InvalidOperationException(IgnoredErrorMessage));
        source7.OnError(new InvalidOperationException(IgnoredErrorMessage));
        source8.OnError(new InvalidOperationException(IgnoredErrorMessage));
        source9.OnError(new InvalidOperationException(IgnoredErrorMessage));
        source10.OnError(new InvalidOperationException(IgnoredErrorMessage));
        source11.OnError(new InvalidOperationException(IgnoredErrorMessage));
        source12.OnError(new InvalidOperationException(IgnoredErrorMessage));
        source13.OnError(new InvalidOperationException(IgnoredErrorMessage));

        await Assert.That(receivedError).IsNull();
    }

    /// <summary>
    /// Verifies that observer OnError and TryEmit null-conditional branches are covered
    /// when the subscription has been disposed but the observers are still reachable.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
            (a, b, c, d, e, f, g, h, i, j, k, l, m) => a + b + c + d + e + f + g + h + i + j + k + l + m);

        Exception? receivedError = null;
        var subscription = combined.Subscribe(new AnonymousObserver<int>(
            _ => { },
            ex => receivedError = ex,
            () => { }));

        // Set all has-value flags so TryEmit reaches _observer?.OnNext
        manual1.Observer!.OnNext(1);
        manual2.Observer!.OnNext(SampleValue2);
        manual3.Observer!.OnNext(SampleValue3);
        manual4.Observer!.OnNext(SampleValue4);
        manual5.Observer!.OnNext(SampleValue5);
        manual6.Observer!.OnNext(SampleValue6);
        manual7.Observer!.OnNext(SampleValue7);
        manual8.Observer!.OnNext(SampleValue8);
        manual9.Observer!.OnNext(SampleValue9);
        manual10.Observer!.OnNext(SampleValue10);
        manual11.Observer!.OnNext(SampleValue11);
        manual12.Observer!.OnNext(SampleValue12);
        manual13.Observer!.OnNext(SampleValue13);

        subscription.Dispose();

        // These reach the observers because ManualObservable retains references
        manual1.Observer.OnNext(PostDisposeValue1);
        manual2.Observer.OnNext(PostDisposeValue2);
        manual3.Observer.OnNext(PostDisposeValue3);
        manual4.Observer.OnNext(PostDisposeValue4);
        manual5.Observer.OnNext(PostDisposeValue5);
        manual6.Observer.OnNext(PostDisposeValue6);
        manual7.Observer.OnNext(PostDisposeValue7);
        manual8.Observer.OnNext(PostDisposeValue8);
        manual9.Observer.OnNext(PostDisposeValue9);
        manual10.Observer.OnNext(PostDisposeValue10);
        manual11.Observer.OnNext(PostDisposeValue11);
        manual12.Observer.OnNext(PostDisposeValue12);
        manual13.Observer.OnNext(PostDisposeValue13);
        manual1.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual2.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual3.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual4.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual5.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual6.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual7.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual8.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual9.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual10.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual11.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual12.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual13.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));

        await Assert.That(receivedError).IsNull();
    }
}

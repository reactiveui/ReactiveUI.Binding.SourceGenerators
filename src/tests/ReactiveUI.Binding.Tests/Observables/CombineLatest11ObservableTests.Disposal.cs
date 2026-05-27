// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Tests for CombineLatest with 11 sources — disposal, post-dispose, and completion behavior.
/// </summary>
public partial class CombineLatest11ObservableTests
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
            (a, b, c, d, e, f, g, h, i, j, k) => a + b + c + d + e + f + g + h + i + j + k);

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
            (a, b, c, d, e, f, g, h, i, j, k) => a + b + c + d + e + f + g + h + i + j + k);

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
        subscription.Dispose();

        source1.OnNext(Source1ValueAfterDispose);

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
            (a, b, c, d, e, f, g, h, i, j, k) => a + b + c + d + e + f + g + h + i + j + k);

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
            (a, b, c, d, e, f, g, h, i, j, k) => a + b + c + d + e + f + g + h + i + j + k);

        Exception? receivedError = null;
        var subscription = combined.Subscribe(new AnonymousObserver<int>(
            _ => { },
            ex => receivedError = ex,
            () => { }));

        // Set all has-value flags so TryEmit reaches _observer?.OnNext
        manual1.Observer!.OnNext(1);
        manual2.Observer!.OnNext(Source2Value);
        manual3.Observer!.OnNext(Source3Value);
        manual4.Observer!.OnNext(Source4Value);
        manual5.Observer!.OnNext(Source5Value);
        manual6.Observer!.OnNext(Source6Value);
        manual7.Observer!.OnNext(Source7Value);
        manual8.Observer!.OnNext(Source8Value);
        manual9.Observer!.OnNext(Source9Value);
        manual10.Observer!.OnNext(Source10Value);
        manual11.Observer!.OnNext(Source11Value);

        subscription.Dispose();

        // These reach the observers because ManualObservable retains references
        manual1.Observer.OnNext(Source1ValueAfterDispose);
        manual2.Observer.OnNext(Source2ValueAfterDispose);
        manual3.Observer.OnNext(Source3ValueAfterDispose);
        manual4.Observer.OnNext(Source4ValueAfterDispose);
        manual5.Observer.OnNext(Source5ValueAfterDispose);
        manual6.Observer.OnNext(Source6ValueAfterDispose);
        manual7.Observer.OnNext(Source7ValueAfterDispose);
        manual8.Observer.OnNext(Source8ValueAfterDispose);
        manual9.Observer.OnNext(Source9ValueAfterDispose);
        manual10.Observer.OnNext(Source10ValueAfterDispose);
        manual11.Observer.OnNext(Source11ValueAfterDispose);
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

        await Assert.That(receivedError).IsNull();
    }

    /// <summary>
    /// Verifies that OnCompleted from a single source does not propagate completion.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
            (a, b, c, d, e, f, g, h, i, j, k) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}");

        var completed = false;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            _ => { },
            () => completed = true));

        source1.OnNext(1);
        source2.OnNext(Source2Value);
        source3.OnNext(Source3Value);
        source4.OnNext(Source4Value);
        source5.OnNext(Source5Value);
        source6.OnNext(Source6Value);
        source7.OnNext(Source7Value);
        source8.OnNext(Source8Value);
        source9.OnNext(Source9Value);
        source10.OnNext(Source10Value);
        source11.OnNext(Source11Value);

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

        await Assert.That(completed).IsFalse();
        await Assert.That(results).Count().IsEqualTo(1);
    }
}

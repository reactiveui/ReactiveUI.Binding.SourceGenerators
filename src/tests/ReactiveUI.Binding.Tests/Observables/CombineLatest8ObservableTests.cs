// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="CombineLatest8Observable{T1, T2, T3, T4, T5, T6, T7, T8, TResult}"/> edge cases.
/// </summary>
public class CombineLatest8ObservableTests
{
    /// <summary>The value emitted by the first source on the initial round.</summary>
    private const int FirstValue = 1;

    /// <summary>The value emitted by the second source on the initial round.</summary>
    private const int SecondValue = 2;

    /// <summary>The value emitted by the third source on the initial round.</summary>
    private const int ThirdValue = 3;

    /// <summary>The value emitted by the fourth source on the initial round.</summary>
    private const int FourthValue = 4;

    /// <summary>The value emitted by the fifth source on the initial round.</summary>
    private const int FifthValue = 5;

    /// <summary>The value emitted by the sixth source on the initial round.</summary>
    private const int SixthValue = 6;

    /// <summary>The value emitted by the seventh source on the initial round.</summary>
    private const int SeventhValue = 7;

    /// <summary>The value emitted by the eighth source on the initial round.</summary>
    private const int EighthValue = 8;

    /// <summary>The value emitted by the first source after the subscription is disposed.</summary>
    private const int FirstUpdatedValue = 10;

    /// <summary>The value emitted by the second source after the subscription is disposed.</summary>
    private const int SecondUpdatedValue = 20;

    /// <summary>The value emitted by the third source after the subscription is disposed.</summary>
    private const int ThirdUpdatedValue = 30;

    /// <summary>The value emitted by the fourth source after the subscription is disposed.</summary>
    private const int FourthUpdatedValue = 40;

    /// <summary>The value emitted by the fifth source after the subscription is disposed.</summary>
    private const int FifthUpdatedValue = 50;

    /// <summary>The value emitted by the sixth source after the subscription is disposed.</summary>
    private const int SixthUpdatedValue = 60;

    /// <summary>The value emitted by the seventh source after the subscription is disposed.</summary>
    private const int SeventhUpdatedValue = 70;

    /// <summary>The value emitted by the eighth source after the subscription is disposed.</summary>
    private const int EighthUpdatedValue = 80;

    /// <summary>The expected combined result of the initial source values.</summary>
    private const int ExpectedSum = 36;

    /// <summary>The expected number of emissions observed by the subscriber.</summary>
    private const int ExpectedEmissionCount = 1;

    /// <summary>The message used for errors that are expected to be ignored.</summary>
    private const string IgnoredErrorMessage = "should be ignored";

    /// <summary>
    /// Verifies that a null first source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
    [Test]
    public async Task Constructor_NullSource1_Throws()
    {
        Func<int, int, int, int, int, int, int, int, int> selector =
            (a, b, c, d, e, f, g, h) => a + b + c + d + e + f + g + h;
        var act = () => CombineLatestObservable.Create(
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            selector);
        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null second source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
    [Test]
    public async Task Constructor_NullSource2_Throws()
    {
        Func<int, int, int, int, int, int, int, int, int> selector =
            (a, b, c, d, e, f, g, h) => a + b + c + d + e + f + g + h;
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            selector);
        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null third source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
    [Test]
    public async Task Constructor_NullSource3_Throws()
    {
        Func<int, int, int, int, int, int, int, int, int> selector =
            (a, b, c, d, e, f, g, h) => a + b + c + d + e + f + g + h;
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            selector);
        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null fourth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
    [Test]
    public async Task Constructor_NullSource4_Throws()
    {
        Func<int, int, int, int, int, int, int, int, int> selector =
            (a, b, c, d, e, f, g, h) => a + b + c + d + e + f + g + h;
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            selector);
        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null fifth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
    [Test]
    public async Task Constructor_NullSource5_Throws()
    {
        Func<int, int, int, int, int, int, int, int, int> selector =
            (a, b, c, d, e, f, g, h) => a + b + c + d + e + f + g + h;
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            selector);
        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null sixth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
    [Test]
    public async Task Constructor_NullSource6_Throws()
    {
        Func<int, int, int, int, int, int, int, int, int> selector =
            (a, b, c, d, e, f, g, h) => a + b + c + d + e + f + g + h;
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            new Subject<int>(),
            selector);
        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null seventh source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
    [Test]
    public async Task Constructor_NullSource7_Throws()
    {
        Func<int, int, int, int, int, int, int, int, int> selector =
            (a, b, c, d, e, f, g, h) => a + b + c + d + e + f + g + h;
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            new Subject<int>(),
            selector);
        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null eighth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
    [Test]
    public async Task Constructor_NullSource8_Throws()
    {
        Func<int, int, int, int, int, int, int, int, int> selector =
            (a, b, c, d, e, f, g, h) => a + b + c + d + e + f + g + h;
        var act = () => CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (IObservable<int>)null!,
            selector);
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
            (Func<int, int, int, int, int, int, int, int, int>)null!);
        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that subscribing with a null observer throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
    [Test]
    public async Task Subscribe_NullObserver_Throws()
    {
        Func<int, int, int, int, int, int, int, int, int> selector =
            (a, b, c, d, e, f, g, h) => a + b + c + d + e + f + g + h;
        var combined = CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            selector);

        var act = () => combined.Subscribe(null!);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

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
        Func<int, int, int, int, int, int, int, int, int> selector =
            (a, b, c, d, e, f, g, h) => a + b + c + d + e + f + g + h;
        var combined = CombineLatestObservable.Create(
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            selector);

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
        Func<int, int, int, int, int, int, int, int, int> selector =
            (a, b, c, d, e, f, g, h) => a + b + c + d + e + f + g + h;
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            selector);

        var results = new List<int>();
        var subscription = combined.Subscribe(new AnonymousObserver<int>(results.Add, _ => { }, () => { }));

        source1.OnNext(FirstValue);
        source2.OnNext(SecondValue);
        source3.OnNext(ThirdValue);
        source4.OnNext(FourthValue);
        source5.OnNext(FifthValue);
        source6.OnNext(SixthValue);
        source7.OnNext(SeventhValue);
        source8.OnNext(EighthValue);
        subscription.Dispose();

        source1.OnNext(FirstUpdatedValue);
        source2.OnNext(SecondUpdatedValue);

        await Assert.That(results).Count().IsEqualTo(ExpectedEmissionCount);
        await Assert.That(results[0]).IsEqualTo(ExpectedSum);
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
        Func<int, int, int, int, int, int, int, int, int> selector =
            (a, b, c, d, e, f, g, h) => a + b + c + d + e + f + g + h;
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            selector);

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
        Func<int, int, int, int, int, int, int, int, int> selector =
            (a, b, c, d, e, f, g, h) => a + b + c + d + e + f + g + h;
        var combined = CombineLatestObservable.Create(
            manual1,
            manual2,
            manual3,
            manual4,
            manual5,
            manual6,
            manual7,
            manual8,
            selector);

        Exception? receivedError = null;
        var subscription = combined.Subscribe(new AnonymousObserver<int>(
            _ => { },
            ex => receivedError = ex,
            () => { }));

        // Set all has-value flags so TryEmit reaches _observer?.OnNext
        manual1.Observer!.OnNext(FirstValue);
        manual2.Observer!.OnNext(SecondValue);
        manual3.Observer!.OnNext(ThirdValue);
        manual4.Observer!.OnNext(FourthValue);
        manual5.Observer!.OnNext(FifthValue);
        manual6.Observer!.OnNext(SixthValue);
        manual7.Observer!.OnNext(SeventhValue);
        manual8.Observer!.OnNext(EighthValue);

        subscription.Dispose();

        // These reach the observers because ManualObservable retains references
        manual1.Observer.OnNext(FirstUpdatedValue);
        manual2.Observer.OnNext(SecondUpdatedValue);
        manual3.Observer.OnNext(ThirdUpdatedValue);
        manual4.Observer.OnNext(FourthUpdatedValue);
        manual5.Observer.OnNext(FifthUpdatedValue);
        manual6.Observer.OnNext(SixthUpdatedValue);
        manual7.Observer.OnNext(SeventhUpdatedValue);
        manual8.Observer.OnNext(EighthUpdatedValue);
        manual1.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual2.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual3.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual4.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual5.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual6.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual7.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));
        manual8.Observer.OnError(new InvalidOperationException(IgnoredErrorMessage));

        await Assert.That(receivedError).IsNull();
    }

    /// <summary>
    /// Verifies that an error in the first source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            (a, b, c, d, e, f, g, h) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}");

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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            (a, b, c, d, e, f, g, h) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}");

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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            (a, b, c, d, e, f, g, h) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}");

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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            (a, b, c, d, e, f, g, h) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}");

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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            (a, b, c, d, e, f, g, h) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}");

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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            (a, b, c, d, e, f, g, h) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}");

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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            (a, b, c, d, e, f, g, h) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}");

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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            (a, b, c, d, e, f, g, h) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}");

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
        var combined = CombineLatestObservable.Create(
            source1,
            source2,
            source3,
            source4,
            source5,
            source6,
            source7,
            source8,
            (a, b, c, d, e, f, g, h) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}");

        var completed = false;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            _ => { },
            () => completed = true));

        source1.OnNext(FirstValue);
        source2.OnNext(SecondValue);
        source3.OnNext(ThirdValue);
        source4.OnNext(FourthValue);
        source5.OnNext(FifthValue);
        source6.OnNext(SixthValue);
        source7.OnNext(SeventhValue);
        source8.OnNext(EighthValue);

        source1.OnCompleted();
        source2.OnCompleted();
        source3.OnCompleted();
        source4.OnCompleted();
        source5.OnCompleted();
        source6.OnCompleted();
        source7.OnCompleted();
        source8.OnCompleted();

        await Assert.That(completed).IsFalse();
        await Assert.That(results).Count().IsEqualTo(ExpectedEmissionCount);
    }

    /// <summary>
    /// A simple observer implementation that delegates to provided action callbacks.
    /// </summary>
    /// <typeparam name="T">The type of elements observed.</typeparam>
    /// <param name="onNext">The action to invoke for each observed element.</param>
    /// <param name="onError">The action to invoke when an error occurs.</param>
    /// <param name="onCompleted">The action to invoke when the sequence completes.</param>
    private sealed class AnonymousObserver<T>(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        : IObserver<T>
    {
        /// <inheritdoc/>
        public void OnCompleted() => onCompleted();

        /// <inheritdoc/>
        public void OnError(Exception error) => onError(error);

        /// <inheritdoc/>
        public void OnNext(T value) => onNext(value);
    }
}

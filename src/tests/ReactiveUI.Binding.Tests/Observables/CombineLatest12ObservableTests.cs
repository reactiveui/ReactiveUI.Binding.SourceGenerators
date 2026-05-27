// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="CombineLatest12Observable{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult}"/> edge cases.
/// </summary>
public partial class CombineLatest12ObservableTests
{
    /// <summary>
    /// The exception message used for errors that the subscription is expected to ignore.
    /// </summary>
    private const string IgnoredErrorMessage = "should be ignored";

    /// <summary>
    /// The distinct sample value emitted from the second source during a combination sequence.
    /// </summary>
    private const int Source2Value = 2;

    /// <summary>
    /// The distinct sample value emitted from the third source during a combination sequence.
    /// </summary>
    private const int Source3Value = 3;

    /// <summary>
    /// The distinct sample value emitted from the fourth source during a combination sequence.
    /// </summary>
    private const int Source4Value = 4;

    /// <summary>
    /// The distinct sample value emitted from the fifth source during a combination sequence.
    /// </summary>
    private const int Source5Value = 5;

    /// <summary>
    /// The distinct sample value emitted from the sixth source during a combination sequence.
    /// </summary>
    private const int Source6Value = 6;

    /// <summary>
    /// The distinct sample value emitted from the seventh source during a combination sequence.
    /// </summary>
    private const int Source7Value = 7;

    /// <summary>
    /// The distinct sample value emitted from the eighth source during a combination sequence.
    /// </summary>
    private const int Source8Value = 8;

    /// <summary>
    /// The distinct sample value emitted from the ninth source during a combination sequence.
    /// </summary>
    private const int Source9Value = 9;

    /// <summary>
    /// The distinct sample value emitted from the tenth source during a combination sequence.
    /// </summary>
    private const int Source10Value = 10;

    /// <summary>
    /// The distinct sample value emitted from the eleventh source during a combination sequence.
    /// </summary>
    private const int Source11Value = 11;

    /// <summary>
    /// The distinct sample value emitted from the twelfth source during a combination sequence.
    /// </summary>
    private const int Source12Value = 12;

    /// <summary>
    /// The value emitted from the first source after the subscription has been disposed.
    /// </summary>
    private const int Source1ValueAfterDispose = 10;

    /// <summary>
    /// The value emitted from the second source after the subscription has been disposed.
    /// </summary>
    private const int Source2ValueAfterDispose = 20;

    /// <summary>
    /// The value emitted from the third source after the subscription has been disposed.
    /// </summary>
    private const int Source3ValueAfterDispose = 30;

    /// <summary>
    /// The value emitted from the fourth source after the subscription has been disposed.
    /// </summary>
    private const int Source4ValueAfterDispose = 40;

    /// <summary>
    /// The value emitted from the fifth source after the subscription has been disposed.
    /// </summary>
    private const int Source5ValueAfterDispose = 50;

    /// <summary>
    /// The value emitted from the sixth source after the subscription has been disposed.
    /// </summary>
    private const int Source6ValueAfterDispose = 60;

    /// <summary>
    /// The value emitted from the seventh source after the subscription has been disposed.
    /// </summary>
    private const int Source7ValueAfterDispose = 70;

    /// <summary>
    /// The value emitted from the eighth source after the subscription has been disposed.
    /// </summary>
    private const int Source8ValueAfterDispose = 80;

    /// <summary>
    /// The value emitted from the ninth source after the subscription has been disposed.
    /// </summary>
    private const int Source9ValueAfterDispose = 90;

    /// <summary>
    /// The value emitted from the tenth source after the subscription has been disposed.
    /// </summary>
    private const int Source10ValueAfterDispose = 100;

    /// <summary>
    /// The value emitted from the eleventh source after the subscription has been disposed.
    /// </summary>
    private const int Source11ValueAfterDispose = 110;

    /// <summary>
    /// The value emitted from the twelfth source after the subscription has been disposed.
    /// </summary>
    private const int Source12ValueAfterDispose = 120;

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
            (a, b, c, d, e, f, g, h, i, j, k, l) => a + b + c + d + e + f + g + h + i + j + k + l);

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
            (a, b, c, d, e, f, g, h, i, j, k, l) => a + b + c + d + e + f + g + h + i + j + k + l);

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
            (a, b, c, d, e, f, g, h, i, j, k, l) => a + b + c + d + e + f + g + h + i + j + k + l);

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
            (a, b, c, d, e, f, g, h, i, j, k, l) => a + b + c + d + e + f + g + h + i + j + k + l);

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
            (a, b, c, d, e, f, g, h, i, j, k, l) => a + b + c + d + e + f + g + h + i + j + k + l);

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
            (a, b, c, d, e, f, g, h, i, j, k, l) => a + b + c + d + e + f + g + h + i + j + k + l);

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
            (a, b, c, d, e, f, g, h, i, j, k, l) => a + b + c + d + e + f + g + h + i + j + k + l);

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
            (a, b, c, d, e, f, g, h, i, j, k, l) => a + b + c + d + e + f + g + h + i + j + k + l);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null ninth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
            (a, b, c, d, e, f, g, h, i, j, k, l) => a + b + c + d + e + f + g + h + i + j + k + l);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null tenth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
            (a, b, c, d, e, f, g, h, i, j, k, l) => a + b + c + d + e + f + g + h + i + j + k + l);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null eleventh source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
            (a, b, c, d, e, f, g, h, i, j, k, l) => a + b + c + d + e + f + g + h + i + j + k + l);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null twelfth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
            (a, b, c, d, e, f, g, h, i, j, k, l) => a + b + c + d + e + f + g + h + i + j + k + l);

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
            (Func<int, int, int, int, int, int, int, int, int, int, int, int, int>)null!);

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
            (a, b, c, d, e, f, g, h, i, j, k, l) => a + b + c + d + e + f + g + h + i + j + k + l);

        var act = () => combined.Subscribe(null!);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
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

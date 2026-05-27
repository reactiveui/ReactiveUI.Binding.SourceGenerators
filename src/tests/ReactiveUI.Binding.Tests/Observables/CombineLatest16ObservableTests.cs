// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="CombineLatest16Observable{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}"/> edge cases.
/// </summary>
public partial class CombineLatest16ObservableTests
{
    /// <summary>
    /// Sample value emitted by the second source under test.
    /// </summary>
    private const int SampleValue2 = 2;

    /// <summary>
    /// Sample value emitted by the third source under test.
    /// </summary>
    private const int SampleValue3 = 3;

    /// <summary>
    /// Sample value emitted by the fourth source under test.
    /// </summary>
    private const int SampleValue4 = 4;

    /// <summary>
    /// Sample value emitted by the fifth source under test.
    /// </summary>
    private const int SampleValue5 = 5;

    /// <summary>
    /// Sample value emitted by the sixth source under test.
    /// </summary>
    private const int SampleValue6 = 6;

    /// <summary>
    /// Sample value emitted by the seventh source under test.
    /// </summary>
    private const int SampleValue7 = 7;

    /// <summary>
    /// Sample value emitted by the eighth source under test.
    /// </summary>
    private const int SampleValue8 = 8;

    /// <summary>
    /// Sample value emitted by the ninth source under test.
    /// </summary>
    private const int SampleValue9 = 9;

    /// <summary>
    /// Sample value emitted by the tenth source under test.
    /// </summary>
    private const int SampleValue10 = 10;

    /// <summary>
    /// Sample value emitted by the eleventh source under test.
    /// </summary>
    private const int SampleValue11 = 11;

    /// <summary>
    /// Sample value emitted by the twelfth source under test.
    /// </summary>
    private const int SampleValue12 = 12;

    /// <summary>
    /// Sample value emitted by the thirteenth source under test.
    /// </summary>
    private const int SampleValue13 = 13;

    /// <summary>
    /// Sample value emitted by the fourteenth source under test.
    /// </summary>
    private const int SampleValue14 = 14;

    /// <summary>
    /// Sample value emitted by the fifteenth source under test.
    /// </summary>
    private const int SampleValue15 = 15;

    /// <summary>
    /// Sample value emitted by the sixteenth source under test.
    /// </summary>
    private const int SampleValue16 = 16;

    /// <summary>
    /// Sample value emitted by the first source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue1 = 10;

    /// <summary>
    /// Sample value emitted by the second source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue2 = 20;

    /// <summary>
    /// Sample value emitted by the third source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue3 = 30;

    /// <summary>
    /// Sample value emitted by the fourth source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue4 = 40;

    /// <summary>
    /// Sample value emitted by the fifth source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue5 = 50;

    /// <summary>
    /// Sample value emitted by the sixth source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue6 = 60;

    /// <summary>
    /// Sample value emitted by the seventh source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue7 = 70;

    /// <summary>
    /// Sample value emitted by the eighth source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue8 = 80;

    /// <summary>
    /// Sample value emitted by the ninth source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue9 = 90;

    /// <summary>
    /// Sample value emitted by the tenth source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue10 = 100;

    /// <summary>
    /// Sample value emitted by the eleventh source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue11 = 110;

    /// <summary>
    /// Sample value emitted by the twelfth source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue12 = 120;

    /// <summary>
    /// Sample value emitted by the thirteenth source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue13 = 130;

    /// <summary>
    /// Sample value emitted by the fourteenth source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue14 = 140;

    /// <summary>
    /// Sample value emitted by the fifteenth source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue15 = 150;

    /// <summary>
    /// Sample value emitted by the sixteenth source after the subscription is disposed.
    /// </summary>
    private const int PostDisposeValue16 = 160;

    /// <summary>
    /// The message used for errors that are expected to be ignored after disposal.
    /// </summary>
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
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

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
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

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
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

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
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

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
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

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
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

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
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

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
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

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
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

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
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

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
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

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
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null thirteenth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null fourteenth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null fifteenth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
    [Test]
    public async Task Constructor_NullSource15_Throws()
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
            (IObservable<int>)null!,
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null sixteenth source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
    [Test]
    public async Task Constructor_NullSource16_Throws()
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
            new Subject<int>(),
            (IObservable<int>)null!,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null result selector throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
            new Subject<int>(),
            new Subject<int>(),
            (Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>)null!);

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
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            new Subject<int>(),
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p);

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

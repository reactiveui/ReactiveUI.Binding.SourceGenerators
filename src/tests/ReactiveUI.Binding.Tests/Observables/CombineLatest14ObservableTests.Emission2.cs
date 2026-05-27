// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Per-source error-propagation tests (part 2) for <see cref="CombineLatest14Observable{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult}"/>.
/// </summary>
public partial class CombineLatest14ObservableTests
{
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "The combine selector lambda's parameter count equals the source arity under test.")]
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
}

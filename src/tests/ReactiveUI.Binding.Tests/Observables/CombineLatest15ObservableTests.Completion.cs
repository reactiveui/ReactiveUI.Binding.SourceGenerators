// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Completion-propagation tests for <see cref="CombineLatest15Observable{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult}"/>.
/// </summary>
public partial class CombineLatest15ObservableTests
{
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
        var source12 = new Subject<int>();
        var source13 = new Subject<int>();
        var source14 = new Subject<int>();
        var source15 = new Subject<int>();
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
            source15,
            (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) => $"{a}-{b}-{c}-{d}-{e}-{f}-{g}-{h}-{i}-{j}-{k}-{l}-{m}-{n}-{o}");

        var completed = false;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            _ => { },
            () => completed = true));

        source1.OnNext(1);
        source2.OnNext(SampleValue2);
        source3.OnNext(SampleValue3);
        source4.OnNext(SampleValue4);
        source5.OnNext(SampleValue5);
        source6.OnNext(SampleValue6);
        source7.OnNext(SampleValue7);
        source8.OnNext(SampleValue8);
        source9.OnNext(SampleValue9);
        source10.OnNext(SampleValue10);
        source11.OnNext(SampleValue11);
        source12.OnNext(SampleValue12);
        source13.OnNext(SampleValue13);
        source14.OnNext(SampleValue14);
        source15.OnNext(SampleValue15);

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
        source15.OnCompleted();

        await Assert.That(completed).IsFalse();
        await Assert.That(results).Count().IsEqualTo(1);
    }
}

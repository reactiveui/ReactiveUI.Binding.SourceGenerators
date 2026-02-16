// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using System.Reactive.Subjects;

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.WhenAny;

/// <summary>
/// Tests for WhenAnyObservable extension methods.
/// </summary>
public class WhenAnyObservableTests
{
    /// <summary>
    /// Verifies that null observables do not cause exceptions.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullObservablesDoNotCauseExceptions()
    {
        EnsureInitialized();

        var vm = new TestWhenAnyObsViewModel();
        var values = new List<int>();

        // Command1 is null, should not throw
        using var sub = vm.WhenAnyObservable(x => x.Command1)
            .Subscribe(values.Add);

        // Set it to a real observable
        var subject = new Subject<int>();
        vm.Command1 = subject;

        subject.OnNext(42);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(42);
    }

    /// <summary>
    /// Verifies that merging two observable properties works.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SmokeTestMerging()
    {
        EnsureInitialized();

        var subject1 = new Subject<int>();
        var subject2 = new Subject<int>();
        var vm = new TestWhenAnyObsViewModel
        {
            Command1 = subject1,
            Command2 = subject2
        };
        var values = new List<int>();

        using var sub = vm.WhenAnyObservable(x => x.Command1, x => x.Command2)
            .Subscribe(values.Add);

        subject1.OnNext(1);
        subject2.OnNext(2);

        await Assert.That(values.Count).IsEqualTo(2);
        await Assert.That(values).Contains(1);
        await Assert.That(values).Contains(2);
    }

    /// <summary>
    /// Verifies that combining three observable properties works.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SmokeTestCombining()
    {
        EnsureInitialized();

        var subject1 = new Subject<int>();
        var subject2 = new Subject<int>();
        var subject3 = new Subject<int>();
        var vm = new TestWhenAnyObsViewModel
        {
            Command1 = subject1,
            Command2 = subject2,
            Command3 = subject3
        };
        var values = new List<int>();

        using var sub = vm.WhenAnyObservable(x => x.Command1, x => x.Command2, x => x.Command3)
            .Subscribe(values.Add);

        subject1.OnNext(10);
        subject2.OnNext(20);
        subject3.OnNext(30);

        await Assert.That(values.Count).IsEqualTo(3);
    }

    /// <summary>
    /// Verifies that a null object updates when a non-null observable is assigned.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullObjectUpdatesWhenNotNullAnymore()
    {
        EnsureInitialized();

        var vm = new TestWhenAnyObsViewModel();
        var values = new List<int>();

        using var sub = vm.WhenAnyObservable(x => x.Changes)
            .Subscribe(values.Add);

        // Initially null, should not emit
        await Assert.That(values.Count).IsEqualTo(0);

        // Now set a real observable
        var subject = new Subject<int>();
        vm.Changes = subject;

        subject.OnNext(99);

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(99);
    }

    internal static void EnsureInitialized()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        builder.WithCoreServices();
        builder.BuildApp();
    }
}

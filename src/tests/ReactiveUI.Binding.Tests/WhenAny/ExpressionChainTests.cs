// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Reactive.Linq;

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.WhenAny;

/// <summary>
/// Tests for SubscribeToExpressionChain with various options.
/// </summary>
public class ExpressionChainTests
{
    /// <summary>
    /// Verifies basic usage notifies on change.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BasicUsage_NotifiesOnChange()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "Start" };
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var values = new List<string>();

        using var sub = fixture.SubscribeToExpressionChain<TestFixture, string>(
            expr.Body,
            beforeChange: false,
            skipInitial: false,
            isDistinct: true)
            .Select(x => x.Value)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Start");

        fixture.IsNotNullString = "End";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values[1]).IsEqualTo("End");
    }

    /// <summary>
    /// Verifies that before-change notification works via expression chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithBeforeChange_NotifiesBeforeChange()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "Before" };
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var values = new List<string>();

        using var sub = fixture.SubscribeToExpressionChain<TestFixture, string>(
            expr.Body,
            beforeChange: true,
            skipInitial: false,
            isDistinct: false)
            .Select(x => x.Value)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);

        fixture.IsNotNullString = "After";

        // Should have received a notification (before-change)
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(2);
    }

    /// <summary>
    /// Verifies that skipInitial skips the first emission.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSkipInitial_SkipsFirstEmission()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "Initial" };
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var values = new List<string>();

        using var sub = fixture.SubscribeToExpressionChain<TestFixture, string>(
            expr.Body,
            beforeChange: false,
            skipInitial: true,
            isDistinct: true)
            .Select(x => x.Value)
            .Subscribe(values.Add);

        // Should NOT have emitted the initial value
        await Assert.That(values.Count).IsEqualTo(0);

        fixture.IsNotNullString = "Changed";

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Changed");
    }

    /// <summary>
    /// Verifies that isDistinct deduplicates same values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithIsDistinct_DeduplicatesSameValues()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "Same" };
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var values = new List<string>();

        using var sub = fixture.SubscribeToExpressionChain<TestFixture, string>(
            expr.Body,
            beforeChange: false,
            skipInitial: true,
            isDistinct: true)
            .Select(x => x.Value)
            .Subscribe(values.Add);

        fixture.IsNotNullString = "A";
        fixture.IsNotNullString = "B";
        fixture.IsNotNullString = "B"; // Same value — setter will short-circuit

        await Assert.That(values.Count).IsEqualTo(2);
        await Assert.That(values[0]).IsEqualTo("A");
        await Assert.That(values[1]).IsEqualTo("B");
    }

    /// <summary>
    /// Verifies that null in a chain propagates correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullInChain_PropagatesCorrectly()
    {
        EnsureInitialized();

        var fixture = new HostTestFixture();
        Expression<Func<HostTestFixture, string>> expr = x => x.Child!.IsNotNullString;
        var values = new List<IObservedChange<HostTestFixture, string>>();

        // Child is null initially
        using var sub = fixture.SubscribeToExpressionChain<HostTestFixture, string>(
            expr.Body,
            beforeChange: false,
            skipInitial: false,
            isDistinct: false)
            .Subscribe(values.Add);

        // Set Child to something
        fixture.Child = new TestFixture { IsNotNullString = "Hello" };

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
    }

    internal static void EnsureInitialized()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        builder.WithCoreServices();
        builder.BuildApp();
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.WhenAny;

/// <summary>Tests for SubscribeToExpressionChain with various options.</summary>
public class ExpressionChainTests
{
    /// <summary>The expected number of emitted values when two notifications are produced.</summary>
    private const int ExpectedTwoEmissions = 2;

    /// <summary>The value a fixture starts out holding.</summary>
    private const string StartValue = "Start";

    /// <summary>Verifies basic usage notifies on change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BasicUsage_NotifiesOnChange()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = StartValue };
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var values = new List<string>();

        using var sub = fixture.SubscribeToExpressionChain<TestFixture, string>(
                expr.Body,
                false,
                false,
                true)
            .Select(static x => x.Value)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(StartValue);

        fixture.IsNotNullString = "End";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedTwoEmissions);
        await Assert.That(values[1]).IsEqualTo("End");
    }

    /// <summary>Verifies that before-change notification works via expression chain.</summary>
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
                true,
                false,
                false)
            .Select(static x => x.Value)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);

        fixture.IsNotNullString = "After";

        // Should have received a notification (before-change)
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedTwoEmissions);
    }

    /// <summary>Verifies that skipInitial skips the first emission.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSkipInitial_SkipsFirstEmission()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "Initial" };
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var values = new List<string>();

        using var sub = fixture.SubscribeToExpressionChain<TestFixture, string>(
                expr.Body)
            .Select(static x => x.Value)
            .Subscribe(values.Add);

        // Should NOT have emitted the initial value
        await Assert.That(values.Count).IsEqualTo(0);

        fixture.IsNotNullString = "Changed";

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Changed");
    }

    /// <summary>Verifies that isDistinct deduplicates same values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithIsDistinct_DeduplicatesSameValues()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "Same" };
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var values = new List<string>();

        using var sub = fixture.SubscribeToExpressionChain<TestFixture, string>(
                expr.Body)
            .Select(static x => x.Value)
            .Subscribe(values.Add);

        fixture.IsNotNullString = "A";
        fixture.IsNotNullString = "B";
        fixture.IsNotNullString = "B"; // Same value — setter will short-circuit

        await Assert.That(values.Count).IsEqualTo(ExpectedTwoEmissions);
        await Assert.That(values[0]).IsEqualTo("A");
        await Assert.That(values[1]).IsEqualTo("B");
    }

    /// <summary>Verifies that null in a chain propagates correctly.</summary>
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
                false,
                false,
                false)
            .Subscribe(values.Add);

        // Set Child to something
        fixture.Child = new() { IsNotNullString = "Hello" };

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
    }

    /// <summary>Verifies that an expression with no member access yields a chain that never emits.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IdentityExpression_HasNoLinksAndNeverEmits()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = StartValue };
        Expression<Func<TestFixture, TestFixture>> expr = x => x;
        var recorder = new Recorder<TestFixture, TestFixture>();

        using var sub = fixture.SubscribeToExpressionChain<TestFixture, TestFixture>(
                expr.Body,
                false,
                false,
                false)
            .Subscribe(recorder);

        await Assert.That(recorder.Values.Count).IsEqualTo(0);
        await Assert.That(recorder.Error).IsNull();
    }

    /// <summary>Verifies that a leaf value of an unrelated type is reported rather than silently dropped.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LeafValueOfAnotherType_SignalsInvalidCast()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = StartValue };
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var recorder = new Recorder<TestFixture, int>();

        using var sub = fixture.SubscribeToExpressionChain<TestFixture, int>(
                expr.Body,
                false,
                false,
                false)
            .Subscribe(recorder);

        await Assert.That(recorder.Error).IsTypeOf<InvalidCastException>();
    }

    /// <summary>Verifies that a null root leaves the whole chain unparented rather than throwing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullRoot_ProducesNoValues()
    {
        EnsureInitialized();

        HostTestFixture? fixture = null;
        Expression<Func<HostTestFixture, string>> expr = x => x.Child!.IsNotNullString;
        var recorder = new Recorder<HostTestFixture, string>();

        using var sub = fixture.SubscribeToExpressionChain<HostTestFixture, string>(
                expr.Body,
                false,
                false,
                false)
            .Subscribe(recorder);

        await Assert.That(recorder.Values.Count).IsEqualTo(0);
        await Assert.That(recorder.Error).IsNull();
    }

    /// <summary>Resets and initializes the ReactiveUI binding infrastructure for testing.</summary>
    internal static void EnsureInitialized()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();
    }

    /// <summary>Records what a chain subscription produced, including the terminal signal.</summary>
    /// <typeparam name="TSender">The root sender type.</typeparam>
    /// <typeparam name="TValue">The leaf value type.</typeparam>
    private sealed class Recorder<TSender, TValue> : IObserver<IObservedChange<TSender, TValue>>
    {
        /// <summary>Gets the values the chain emitted, in order.</summary>
        public List<IObservedChange<TSender, TValue>> Values { get; } = [];

        /// <summary>Gets the error the chain signalled, if any.</summary>
        public Exception? Error { get; private set; }

        /// <inheritdoc/>
        public void OnNext(IObservedChange<TSender, TValue> value) => Values.Add(value);

        /// <inheritdoc/>
        public void OnError(Exception error) => Error = error;

        /// <inheritdoc/>
        public void OnCompleted()
        {
        }
    }
}

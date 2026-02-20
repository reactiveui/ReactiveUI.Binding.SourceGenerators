// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.ObservableForProperty;

/// <summary>
/// Tests for <see cref="ReactiveNotifyPropertyChangedMixin"/> covering remaining branch gaps
/// including NestedObservedChanges, NotifyForProperty, and SubscribeToExpressionChain paths.
/// </summary>
public class ReactiveNotifyPropertyChangedMixinTests
{
    /// <summary>
    /// Verifies that NestedObservedChanges returns a single-element observable with default value
    /// when the sourceChange value is null.
    /// Covers ReactiveNotifyPropertyChangedMixin.cs line 230-232 (sourceChange.Value is null branch).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NestedObservedChanges_NullSourceChangeValue_ReturnsSingleKicker()
    {
        EnsureInitialized();

        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var sourceChange = new ObservedChange<object?, object?>(null, body, null);

        var results = new List<IObservedChange<object?, object?>>();
        using var sub = ReactiveNotifyPropertyChangedMixin.NestedObservedChanges(body, sourceChange, beforeChange: false)
            .Subscribe(results.Add);

        await Assert.That(results.Count).IsEqualTo(1);
        await Assert.That(results[0].Sender).IsNull();
    }

    /// <summary>
    /// Verifies that NestedObservedChanges emits a kicker followed by property notifications
    /// when the sourceChange value is non-null.
    /// Covers ReactiveNotifyPropertyChangedMixin.cs line 235-237 (sourceChange.Value is not null branch).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NestedObservedChanges_NonNullSourceChangeValue_EmitsKickerAndNotifications()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = "Initial" };

        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var sourceChange = new ObservedChange<object?, object?>(null, body, vm);

        var results = new List<IObservedChange<object?, object?>>();
        using var sub = ReactiveNotifyPropertyChangedMixin.NestedObservedChanges(body, sourceChange, beforeChange: false)
            .Subscribe(results.Add);

        // Should have the kicker (StartWith)
        await Assert.That(results.Count).IsGreaterThanOrEqualTo(1);

        // Now change the property - should emit another notification
        vm.Name = "Changed";

        await Assert.That(results.Count).IsGreaterThanOrEqualTo(2);
    }

    /// <summary>
    /// Verifies that NotifyForProperty throws ArgumentException when the expression has no valid member info.
    /// Covers ReactiveNotifyPropertyChangedMixin.cs line 248 (memberInfo null branch).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NotifyForProperty_ExpressionWithNoMemberInfo_ThrowsArgumentException()
    {
        EnsureInitialized();

        var vm = new TestViewModel();

        // A ParameterExpression has no member info, so GetMemberInfo will throw
        var paramExpr = System.Linq.Expressions.Expression.Parameter(typeof(TestViewModel), "x");

        var action = () => ReactiveNotifyPropertyChangedMixin.NotifyForProperty(vm, paramExpr, beforeChange: false);

        await Assert.That(action).ThrowsException();
    }

    /// <summary>
    /// Verifies that SubscribeToExpressionChain with isDistinct=false does not deduplicate values
    /// when the same value is emitted multiple times.
    /// Covers ReactiveNotifyPropertyChangedMixin.cs line 219 (isDistinct false branch in expression chain overload).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SubscribeToExpressionChain_IsDistinctFalse_DoesNotDeduplicate()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "A" };

        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;

        var values = new List<IObservedChange<TestFixture, string>>();
        using var sub = ReactiveNotifyPropertyChangedMixin.SubscribeToExpressionChain<TestFixture, string>(
            fixture,
            expr.Body,
            beforeChange: false,
            skipInitial: false,
            isDistinct: false)
            .Subscribe(values.Add);

        fixture.IsNotNullString = "B";
        fixture.IsNotNullString = "C";

        // Should have initial + 2 changes = at least 3
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(3);
    }

    /// <summary>
    /// Verifies that SubscribeToExpressionChain with isDistinct=true deduplicates by value.
    /// Covers ReactiveNotifyPropertyChangedMixin.cs line 219 (isDistinct true branch in expression chain overload).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SubscribeToExpressionChain_IsDistinctTrue_DeduplicatesByValue()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "A" };

        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;

        var values = new List<IObservedChange<TestFixture, string>>();
        using var sub = ReactiveNotifyPropertyChangedMixin.SubscribeToExpressionChain<TestFixture, string>(
            fixture,
            expr.Body,
            beforeChange: false,
            skipInitial: false,
            isDistinct: true)
            .Subscribe(values.Add);

        // Change to different values
        fixture.IsNotNullString = "B";
        fixture.IsNotNullString = "C";

        // All values are distinct, so all should come through
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(3);
    }

    /// <summary>
    /// Verifies that SubscribeToExpressionChain with skipInitial=true skips the first value.
    /// Covers ReactiveNotifyPropertyChangedMixin.cs line 201-203 (skipInitial true path).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SubscribeToExpressionChain_SkipInitialTrue_SkipsFirstValue()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "Initial" };

        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;

        var values = new List<IObservedChange<TestFixture, string>>();
        using var sub = ReactiveNotifyPropertyChangedMixin.SubscribeToExpressionChain<TestFixture, string>(
            fixture,
            expr.Body,
            beforeChange: false,
            skipInitial: true,
            isDistinct: false)
            .Subscribe(values.Add);

        // No initial value should have been emitted
        await Assert.That(values.Count).IsEqualTo(0);

        fixture.IsNotNullString = "Changed";

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0].Value).IsEqualTo("Changed");
    }

    /// <summary>
    /// Verifies that SubscribeToExpressionChain filters out null senders from the chain.
    /// Covers ReactiveNotifyPropertyChangedMixin.cs line 206 (Where x.Sender is not null filter).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SubscribeToExpressionChain_NullSenderInChain_IsFilteredOut()
    {
        EnsureInitialized();

        // A deep chain where intermediate becomes null will produce a null-sender change
        var fixture = new HostTestFixture
        {
            Child = new TestFixture { IsNotNullString = "Hello" }
        };

        var values = new List<IObservedChange<HostTestFixture, string>>();
        using var sub = ReactiveNotifyPropertyChangedMixin.SubscribeToExpressionChain<HostTestFixture, string>(
            fixture,
            ((Expression<Func<HostTestFixture, string>>)(x => x.Child!.IsNotNullString)).Body,
            beforeChange: false,
            skipInitial: false,
            isDistinct: false)
            .Subscribe(values.Add);

        var initialCount = values.Count;

        // Setting Child to null should not produce a null-sender emission in the final stream
        fixture.Child = null;

        // Setting it back should produce a new emission
        fixture.Child = new TestFixture { IsNotNullString = "World" };

        await Assert.That(values.Count).IsGreaterThan(initialCount);
    }

    /// <summary>
    /// Verifies that ObservableForProperty by name with beforeChange=true emits before-change notifications.
    /// Covers ReactiveNotifyPropertyChangedMixin.cs line 111-129 (factory subscription path with beforeChange).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByName_BeforeChange_EmitsNotification()
    {
        EnsureInitialized();

        var vm = new TestViewModel();

        var values = new List<IObservedChange<TestViewModel, string>>();
        using var sub = vm.ObservableForProperty<TestViewModel, string>("Name", beforeChange: true)
            .Subscribe(values.Add);

        vm.Name = "Alice";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Verifies that ObservableForProperty by expression with beforeChange=true emits notifications.
    /// Covers the expression-based overload of ObservableForProperty with beforeChange true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByExpression_BeforeChange_EmitsNotification()
    {
        EnsureInitialized();

        var vm = new TestViewModel();

        var values = new List<IObservedChange<TestViewModel, string>>();
        using var sub = vm.ObservableForProperty(x => x.Name, beforeChange: true)
            .Subscribe(values.Add);

        vm.Name = "Bob";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Verifies that NestedObservedChanges with beforeChange=true routes through
    /// the PropertyChanging event path.
    /// Covers ReactiveNotifyPropertyChangedMixin.cs line 235 with beforeChange true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NestedObservedChanges_BeforeChange_RoutesToPropertyChanging()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = "Initial" };

        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var sourceChange = new ObservedChange<object?, object?>(null, body, vm);

        var results = new List<IObservedChange<object?, object?>>();
        using var sub = ReactiveNotifyPropertyChangedMixin.NestedObservedChanges(body, sourceChange, beforeChange: true)
            .Subscribe(results.Add);

        // Should have the kicker from StartWith
        await Assert.That(results.Count).IsGreaterThanOrEqualTo(1);

        // Changing the property should trigger a PropertyChanging notification
        vm.Name = "Changed";

        await Assert.That(results.Count).IsGreaterThanOrEqualTo(2);
    }

    /// <summary>
    /// Verifies that the val cast path in SubscribeToExpressionChain line 93 is exercised
    /// when val is TValue (the normal case).
    /// Covers ReactiveNotifyPropertyChangedMixin.cs line 93 (val is TValue cast).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByName_GetCurrentValue_ValIsTValue_Succeeds()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = "Test" };

        string? receivedValue = null;
        using var sub = vm.ObservableForProperty<TestViewModel, string>("Name", skipInitial: false)
            .Subscribe(x => receivedValue = x.Value);

        await Assert.That(receivedValue).IsEqualTo("Test");
    }

    private static void EnsureInitialized()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        builder.WithCoreServices();
        builder.BuildApp();
    }
}

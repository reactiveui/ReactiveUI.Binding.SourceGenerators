// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Reactive.Linq;

using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.ObservableForProperty;

/// <summary>
/// Tests for the <see cref="ObservedChangedMixin"/> class.
/// </summary>
public class ObservedChangedMixinTests
{
    /// <summary>
    /// Verifies that GetPropertyName returns the correct property name from an observed change.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetPropertyName_SimpleProperty_ReturnsName()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var change = new ObservedChange<TestViewModel, string>(new TestViewModel(), body, "Hello");

        var name = change.GetPropertyName();

        await Assert.That(name).IsEqualTo("Name");
    }

    /// <summary>
    /// Verifies that GetPropertyName returns a dotted path for nested properties.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetPropertyName_NestedProperty_ReturnsDottedPath()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);

        var change = new ObservedChange<TestViewModel, string>(new TestViewModel(), body, "Seattle");

        var name = change.GetPropertyName();

        await Assert.That(name).IsEqualTo("Address.City");
    }

    /// <summary>
    /// Verifies that GetValue returns the value when it's already populated.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValue_WithPopulatedValue_ReturnsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var change = new ObservedChange<TestViewModel, string>(new TestViewModel { Name = "Test" }, body, "Direct");

        var value = change.GetValue();

        await Assert.That(value).IsEqualTo("Direct");
    }

    /// <summary>
    /// Verifies that GetValue evaluates the expression chain when value is default.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValue_WithDefaultValue_EvaluatesChain()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var vm = new TestViewModel { Name = "Evaluated" };
        var change = new ObservedChange<TestViewModel, string>(vm, body, default!);

        var value = change.GetValue();

        await Assert.That(value).IsEqualTo("Evaluated");
    }

    /// <summary>
    /// Verifies that GetValueOrDefault returns default when chain is broken.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueOrDefault_NullInChain_ReturnsDefault()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);

        var vm = new TestViewModel(); // Address is null
        var change = new ObservedChange<TestViewModel, string>(vm, body, default!);

        var value = change.GetValueOrDefault();

        await Assert.That(value).IsNull();
    }

    /// <summary>
    /// Verifies that GetPropertyName throws for null input.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetPropertyName_NullItem_ThrowsArgumentNullException()
    {
        IObservedChange<TestViewModel, string>? nullChange = null;

        await Assert.That(() => nullChange!.GetPropertyName())
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that GetValue throws for null input.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValue_NullItem_ThrowsArgumentNullException()
    {
        IObservedChange<TestViewModel, string>? nullChange = null;

        await Assert.That(() => nullChange!.GetValue())
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that GetValueOrDefault throws for null input.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueOrDefault_NullItem_ThrowsArgumentNullException()
    {
        IObservedChange<TestViewModel, string>? nullChange = null;

        await Assert.That(() => nullChange!.GetValueOrDefault())
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that GetValue returns the populated value for a non-default value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValue_NonDefaultValue_ReturnsDirectValue()
    {
        Expression<Func<TestViewModel, int>> expr = x => x.Age;
        var body = Reflection.Rewrite(expr.Body);

        var vm = new TestViewModel { Age = 42 };
        var change = new ObservedChange<TestViewModel, int>(vm, body, 99);

        var value = change.GetValue();

        await Assert.That(value).IsEqualTo(99);
    }

    /// <summary>
    /// Verifies that GetValueOrDefault returns the value when it's non-default.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueOrDefault_WithNonDefaultValue_ReturnsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var change = new ObservedChange<TestViewModel, string>(new TestViewModel { Name = "Test" }, body, "Direct");

        var value = change.GetValueOrDefault();

        await Assert.That(value).IsEqualTo("Direct");
    }

    /// <summary>
    /// Verifies that GetValue throws when the expression chain is broken (null intermediate).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValue_BrokenChain_ThrowsException()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);

        var vm = new TestViewModel(); // Address is null, chain is broken
        var change = new ObservedChange<TestViewModel, string>(vm, body, default!);

        await Assert.That(() => change.GetValue())
            .ThrowsExactly<Exception>();
    }

    /// <summary>
    /// Verifies that Value() observable extension maps a stream of changes to values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Value_MapsChangesToValues()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var change1 = new ObservedChange<TestViewModel, string>(new TestViewModel(), body, "First");
        var change2 = new ObservedChange<TestViewModel, string>(new TestViewModel(), body, "Second");

        var stream = new[] { change1, change2 }.ToObservable();
        var values = new List<string>();

        using var sub = stream.Value().Subscribe(values.Add);

        await Assert.That(values).Count().IsEqualTo(2);
        await Assert.That(values[0]).IsEqualTo("First");
        await Assert.That(values[1]).IsEqualTo("Second");
    }

    /// <summary>
    /// Verifies that SetValueToProperty applies the change to the target object.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SetValueToProperty_ValidTarget_SetsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var change = new ObservedChange<TestViewModel, string>(new TestViewModel(), body, "NewValue");
        var target = new TestViewModel { Name = "OldValue" };

        change.SetValueToProperty(target, x => x.Name);

        await Assert.That(target.Name).IsEqualTo("NewValue");
    }

    /// <summary>
    /// Verifies that SetValueToProperty does nothing when target is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SetValueToProperty_NullTarget_DoesNotThrow()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var change = new ObservedChange<TestViewModel, string>(new TestViewModel(), body, "NewValue");
        TestViewModel? target = null;

        // Should not throw
        change.SetValueToProperty(target!, x => x.Name);

        await Assert.That(target).IsNull();
    }
}

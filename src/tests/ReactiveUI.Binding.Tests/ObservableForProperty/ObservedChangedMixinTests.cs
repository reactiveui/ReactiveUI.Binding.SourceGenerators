// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;

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
}

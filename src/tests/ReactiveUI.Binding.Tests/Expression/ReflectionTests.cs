// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;

using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Expression;

/// <summary>
/// Tests for the <see cref="Reflection"/> class.
/// </summary>
public class ReflectionTests
{
    /// <summary>
    /// Verifies that ExpressionToPropertyNames returns the correct name for a simple property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionToPropertyNames_SimpleProperty_ReturnsName()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var name = Reflection.ExpressionToPropertyNames(body);

        await Assert.That(name).IsEqualTo("Name");
    }

    /// <summary>
    /// Verifies that ExpressionToPropertyNames returns a dotted path for nested properties.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionToPropertyNames_NestedProperty_ReturnsDottedPath()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);

        var name = Reflection.ExpressionToPropertyNames(body);

        await Assert.That(name).IsEqualTo("Address.City");
    }

    /// <summary>
    /// Verifies that GetValueFetcherOrThrow returns a working getter for a property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueFetcherOrThrow_PropertyMember_ReturnsGetter()
    {
        var memberInfo = typeof(TestViewModel).GetProperty("Name")!;
        var getter = Reflection.GetValueFetcherOrThrow(memberInfo);

        var vm = new TestViewModel { Name = "Test" };
        var value = getter(vm, null);

        await Assert.That(value).IsEqualTo("Test");
    }

    /// <summary>
    /// Verifies that GetValueSetterOrThrow returns a working setter for a property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueSetterOrThrow_PropertyMember_ReturnsSetter()
    {
        var memberInfo = typeof(TestViewModel).GetProperty("Name")!;
        var setter = Reflection.GetValueSetterOrThrow(memberInfo);

        var vm = new TestViewModel();
        setter(vm, "Hello", null);

        await Assert.That(vm.Name).IsEqualTo("Hello");
    }

    /// <summary>
    /// Verifies that TryGetValueForPropertyChain returns the correct value for a simple chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_SimpleProperty_ReturnsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel { Name = "Alice" };
        var result = Reflection.TryGetValueForPropertyChain<string>(out var value, vm, chain);

        await Assert.That(result).IsTrue();
        await Assert.That(value).IsEqualTo("Alice");
    }

    /// <summary>
    /// Verifies that TryGetValueForPropertyChain traverses nested properties.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_NestedProperty_TraversesChain()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel { Address = new TestAddress { City = "Seattle" } };
        var result = Reflection.TryGetValueForPropertyChain<string>(out var value, vm, chain);

        await Assert.That(result).IsTrue();
        await Assert.That(value).IsEqualTo("Seattle");
    }

    /// <summary>
    /// Verifies that TryGetValueForPropertyChain returns false when a null is in the chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_NullInChain_ReturnsFalse()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel(); // Address is null
        var result = Reflection.TryGetValueForPropertyChain<string>(out _, vm, chain);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that TrySetValueToPropertyChain sets a value on a simple property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_SimpleProperty_SetsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel();
        var result = Reflection.TrySetValueToPropertyChain(vm, chain, "Bob");

        await Assert.That(result).IsTrue();
        await Assert.That(vm.Name).IsEqualTo("Bob");
    }
}

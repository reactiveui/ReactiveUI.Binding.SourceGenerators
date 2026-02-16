// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Reflection;

using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Expression;

/// <summary>
/// Tests for the <see cref="ExpressionMixins"/> class.
/// </summary>
public class ExpressionMixinsTests
{
    /// <summary>
    /// Verifies that GetExpressionChain returns a single member for a simple property access.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetExpressionChain_SimpleProperty_ReturnsSingleExpression()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var chain = body.GetExpressionChain().ToList();

        await Assert.That(chain.Count).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that GetExpressionChain returns multiple members for a nested property chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetExpressionChain_NestedProperty_ReturnsMultipleExpressions()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);

        var chain = body.GetExpressionChain().ToList();

        await Assert.That(chain.Count).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies that GetMemberInfo returns the correct member for a property expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetMemberInfo_PropertyExpression_ReturnsMemberInfo()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var chain = body.GetExpressionChain().ToList();
        var memberInfo = chain[0].GetMemberInfo();

        await Assert.That(memberInfo).IsNotNull();
        await Assert.That(memberInfo!.Name).IsEqualTo("Name");
    }

    /// <summary>
    /// Verifies that GetParent returns the correct parent expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetParent_MemberExpression_ReturnsParent()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body) as MemberExpression;

        var parent = body!.GetParent();

        await Assert.That(parent).IsNotNull();
        await Assert.That(parent!.NodeType).IsEqualTo(ExpressionType.Parameter);
    }

    /// <summary>
    /// Verifies that GetArgumentsArray returns null for non-index expressions.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetArgumentsArray_NonIndexExpression_ReturnsNull()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var args = body.GetArgumentsArray();

        await Assert.That(args).IsNull();
    }
}

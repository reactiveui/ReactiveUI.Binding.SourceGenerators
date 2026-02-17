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
    /// Verifies that GetExpressionChain returns three members for a 3-level deep property chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetExpressionChain_ThreeLevelDeepChain_ReturnsThreeExpressions()
    {
        Expression<Func<ObjChain1, HostTestFixture?>> expr = x => x.Chain2!.Chain3!.Host;
        var body = Reflection.Rewrite(expr.Body);

        var chain = body.GetExpressionChain().ToList();

        await Assert.That(chain.Count).IsEqualTo(3);

        var firstMember = chain[0].GetMemberInfo();
        var secondMember = chain[1].GetMemberInfo();
        var thirdMember = chain[2].GetMemberInfo();

        await Assert.That(firstMember).IsNotNull();
        await Assert.That(firstMember!.Name).IsEqualTo("Chain2");
        await Assert.That(secondMember).IsNotNull();
        await Assert.That(secondMember!.Name).IsEqualTo("Chain3");
        await Assert.That(thirdMember).IsNotNull();
        await Assert.That(thirdMember!.Name).IsEqualTo("Host");
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
    /// Verifies that GetMemberInfo unwraps Convert expressions (boxing scenarios) and returns the underlying member.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetMemberInfo_ConvertExpression_UnwrapsToUnderlyingMember()
    {
        // Boxing int to object creates a Convert expression wrapping MemberAccess
        Expression<Func<TestViewModel, object>> expr = x => x.Age;
        var body = expr.Body;

        // The body should be a Convert node (boxing int -> object)
        await Assert.That(body.NodeType).IsEqualTo(ExpressionType.Convert);

        var memberInfo = body.GetMemberInfo();

        await Assert.That(memberInfo).IsNotNull();
        await Assert.That(memberInfo!.Name).IsEqualTo("Age");
        await Assert.That(memberInfo.MemberType).IsEqualTo(MemberTypes.Property);
    }

    /// <summary>
    /// Verifies that GetArgumentsArray returns null for a non-index member access expression.
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

    /// <summary>
    /// Verifies that GetParent returns the correct parent expression for a simple member access.
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
    /// Verifies that GetParent returns the intermediate expression for a nested member access.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetParent_NestedMemberExpression_ReturnsIntermediateExpression()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body) as MemberExpression;

        // body is City, parent should be Address (a MemberAccess on the parameter)
        var parent = body!.GetParent();

        await Assert.That(parent).IsNotNull();
        await Assert.That(parent!.NodeType).IsEqualTo(ExpressionType.MemberAccess);
        var parentMember = (MemberExpression)parent;
        await Assert.That(parentMember.Member.Name).IsEqualTo("Address");
    }
}

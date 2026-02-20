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

    /// <summary>
    /// Verifies that GetExpressionChain throws NotSupportedException for a ConstantExpression with a helpful message.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetExpressionChain_ConstantExpression_ThrowsWithHelpfulMessage()
    {
        var constant = System.Linq.Expressions.Expression.Constant("hello");

        var ex = Assert.Throws<NotSupportedException>(() => constant.GetExpressionChain().ToList());
        await Assert.That(ex!.Message).Contains("Did you miss the member access prefix");
    }

    /// <summary>
    /// Verifies that GetMemberInfo throws NotSupportedException for an unsupported expression type.
    /// </summary>
    [Test]
    public void GetMemberInfo_UnsupportedExpression_ThrowsNotSupportedException()
    {
        var constant = System.Linq.Expressions.Expression.Constant(42);

        Assert.Throws<NotSupportedException>(() => constant.GetMemberInfo());
    }

    /// <summary>
    /// Verifies that GetParent throws NotSupportedException for an unsupported expression type.
    /// </summary>
    [Test]
    public void GetParent_UnsupportedExpression_ThrowsNotSupportedException()
    {
        var constant = System.Linq.Expressions.Expression.Constant(42);

        Assert.Throws<NotSupportedException>(() => constant.GetParent());
    }

    /// <summary>
    /// Verifies that GetArgumentsArray returns constant arguments for an index expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetArgumentsArray_IndexExpression_ReturnsConstantArguments()
    {
        Expression<Func<IndexTestClass, int>> expr = x => x.Items[2];
        var body = Reflection.Rewrite(expr.Body);

        var args = body.GetArgumentsArray();

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Length).IsEqualTo(1);
        await Assert.That(args[0]).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies that GetExpressionChain handles index expressions in chains.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetExpressionChain_WithIndexExpression_ReturnsChainIncludingIndex()
    {
        Expression<Func<IndexTestClass, int>> expr = x => x.Items[0];
        var body = Reflection.Rewrite(expr.Body);

        var chain = body.GetExpressionChain().ToList();

        await Assert.That(chain.Count).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies that GetMemberInfo returns indexer info for an IndexExpression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetMemberInfo_IndexExpression_ReturnsIndexerInfo()
    {
        Expression<Func<IndexTestClass, int>> expr = x => x.Items[0];
        var body = Reflection.Rewrite(expr.Body);

        // body is an IndexExpression for Items[0]
        var memberInfo = body.GetMemberInfo();

        await Assert.That(memberInfo).IsNotNull();
        await Assert.That(memberInfo!.Name).IsEqualTo("Item");
    }

    /// <summary>
    /// Verifies that GetParent returns the object expression for an IndexExpression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetParent_IndexExpression_ReturnsObjectExpression()
    {
        Expression<Func<IndexTestClass, int>> expr = x => x.Items[0];
        var body = Reflection.Rewrite(expr.Body);

        var parent = body.GetParent();

        await Assert.That(parent).IsNotNull();
        await Assert.That(parent!.NodeType).IsEqualTo(ExpressionType.MemberAccess);
    }

    /// <summary>
    /// Verifies that GetExpressionChain correctly handles an IndexExpression whose Object is not a Parameter
    /// (nested indexer access like x.Items[0]) by producing an updated IndexExpression with a Parameter node.
    /// Covers ExpressionMixins.cs line 33 (TRUE path for nested Index with non-Parameter Object).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetExpressionChain_NestedIndexExpression_HandlesNonParameterObject()
    {
        // x => x.Items[0] produces a chain: [Items (MemberAccess), Item[0] (Index)]
        // The IndexExpression's Object is MemberAccess (not Parameter), triggering the nested branch
        Expression<Func<IndexTestClass, int>> expr = x => x.Items[0];
        var body = Reflection.Rewrite(expr.Body);

        var chain = body.GetExpressionChain().ToList();

        // Should return 2 expressions: MemberAccess for Items, then Index for [0]
        await Assert.That(chain.Count).IsEqualTo(2);

        // First expression should be MemberAccess (Items property)
        await Assert.That(chain[0].NodeType).IsEqualTo(ExpressionType.MemberAccess);

        // Second expression should be Index
        await Assert.That(chain[1].NodeType).IsEqualTo(ExpressionType.Index);
    }

    /// <summary>
    /// Verifies that GetExpressionChain correctly handles an IndexExpression where Object IS a Parameter
    /// (direct indexer on parameter like x[0]) by adding the IndexExpression directly.
    /// Covers ExpressionMixins.cs line 33 (FALSE/else path for Index with Parameter Object).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetExpressionChain_DirectIndexOnParameter_AddsIndexDirectly()
    {
        // Build an IndexExpression directly on the parameter: param["key"]
        var param = System.Linq.Expressions.Expression.Parameter(typeof(DirectIndexableClass), "x");
        var indexer = typeof(DirectIndexableClass).GetProperty("Item")!;
        var indexExpr = System.Linq.Expressions.Expression.MakeIndex(
            param,
            indexer,
            new[] { System.Linq.Expressions.Expression.Constant("key") });

        var chain = indexExpr.GetExpressionChain().ToList();

        // Should return 1 expression: the Index expression itself
        await Assert.That(chain.Count).IsEqualTo(1);
        await Assert.That(chain[0].NodeType).IsEqualTo(ExpressionType.Index);
    }

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used as type parameter in expression lambdas.")]
    private sealed class IndexTestClass
    {
        public List<int> Items { get; } = [10, 20, 30];
    }

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used as type parameter in expression lambdas.")]
    private sealed class DirectIndexableClass
    {
        private readonly Dictionary<string, string> _data = new() { ["key"] = "value" };

        public string this[string key]
        {
            get => _data.TryGetValue(key, out var val) ? val : string.Empty;
            set => _data[key] = value;
        }
    }
}

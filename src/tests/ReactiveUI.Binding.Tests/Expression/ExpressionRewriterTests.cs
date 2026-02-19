// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Expressions;

namespace ReactiveUI.Binding.Tests.Expression;

/// <summary>
///     Tests for the expression rewriter which normalizes expression trees for property path extraction.
/// </summary>
public class ExpressionRewriterTests
{
    /// <summary>
    ///     Verifies that array index expressions are rewritten to index expressions.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithArrayIndex_ReturnsIndexExpression()
    {
        Expression<Func<TestClass, int>> expr = x => x.Array[0];

        var result = Reflection.Rewrite(expr.Body);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.Index);
    }

    /// <summary>
    ///     Verifies that array index with non-constant index throws.
    /// </summary>
    [Test]
    public void Rewrite_WithArrayIndexNonConstant_Throws()
    {
        var index = 0;
        Expression<Func<TestClass, int>> expr = x => x.Array[index];

        Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(expr.Body));
    }

    /// <summary>
    ///     Verifies that array length is rewritten to member access.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithArrayLength_ReturnsMemberAccess()
    {
        Expression<Func<TestClass, int>> expr = x => x.Array.Length;

        var result = Reflection.Rewrite(expr.Body);

        // ArrayLength should be rewritten to MemberAccess of Length property
        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.MemberAccess);
        var memberExpr = (MemberExpression)result;
        await Assert.That(memberExpr.Member.Name).IsEqualTo("Length");
    }

    /// <summary>
    ///     Verifies that constant expressions pass through unchanged.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithConstant_ReturnsConstantExpression()
    {
        Expression<Func<string>> expr = () => "test";

        var result = Reflection.Rewrite(expr.Body);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.Constant);
    }

    /// <summary>
    ///     Verifies that convert expressions are unwrapped.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithConvert_ReturnsUnderlyingExpression()
    {
        Expression<Func<TestClass, object>> expr = x => x.Property!;

        var result = Reflection.Rewrite(expr.Body);

        // Convert should be unwrapped to the underlying MemberAccess
        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.MemberAccess);
    }

    /// <summary>
    ///     Verifies that index expression with constant arguments are validated.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithIndexExpression_ValidatesConstantArguments()
    {
        Expression<Func<TestClass, int>> expr = x => x.List[1];

        var result = Reflection.Rewrite(expr.Body);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.Index);
    }

    /// <summary>
    ///     Verifies that index expression with non-constant arguments throws.
    /// </summary>
    [Test]
    public void Rewrite_WithIndexExpressionNonConstantArguments_Throws()
    {
        // Create an IndexExpression with non-constant arguments
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TestClass), "x");
        var listProperty = System.Linq.Expressions.Expression.Property(parameter, "List");
        var indexer = typeof(List<int>).GetProperty("Item")!;
        var nonConstantArg = System.Linq.Expressions.Expression.Parameter(typeof(int), "index");
        var indexExpr = System.Linq.Expressions.Expression.MakeIndex(listProperty, indexer, [nonConstantArg]);

        Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(indexExpr));
    }

    /// <summary>
    ///     Verifies that list indexer expressions are rewritten to index expressions.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithListIndexer_ReturnsIndexExpression()
    {
        Expression<Func<TestClass, int>> expr = x => x.List[0];

        var result = Reflection.Rewrite(expr.Body);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.Index);
    }

    /// <summary>
    ///     Verifies that list indexer with non-constant index throws.
    /// </summary>
    [Test]
    public void Rewrite_WithListIndexerNonConstant_Throws()
    {
        var index = 0;
        Expression<Func<TestClass, int>> expr = x => x.List[index];

        Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(expr.Body));
    }

    /// <summary>
    ///     Verifies that member access expressions pass through unchanged.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithMemberAccess_ReturnsMemberExpression()
    {
        Expression<Func<TestClass, string?>> expr = x => x.Property;

        var result = Reflection.Rewrite(expr.Body);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.MemberAccess);
    }

    /// <summary>
    ///     Verifies that non-special method calls throw.
    /// </summary>
    [Test]
    public void Rewrite_WithMethodCallNonSpecialName_Throws()
    {
        Expression<Func<TestClass, string?>> expr = x => x.GetValue();

        Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(expr.Body));
    }

    /// <summary>
    ///     Verifies that nested member access expressions pass through unchanged.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithNestedMemberAccess_ReturnsMemberExpression()
    {
        Expression<Func<TestClass, string?>> expr = x => x.Nested!.Property;

        var result = Reflection.Rewrite(expr.Body);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.MemberAccess);
    }

    /// <summary>
    ///     Verifies that null expression throws.
    /// </summary>
    [Test]
    public void Rewrite_WithNullExpression_Throws() =>
        Assert.Throws<ArgumentNullException>(() => Reflection.Rewrite(null));

    /// <summary>
    ///     Verifies that parameter expressions pass through unchanged.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithParameterExpression_ReturnsParameterExpression()
    {
        Expression<Func<TestClass, TestClass>> expr = x => x;

        var result = Reflection.Rewrite(expr.Body);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.Parameter);
    }

    /// <summary>
    ///     Verifies that unsupported unary expressions throw.
    /// </summary>
    [Test]
    public void Rewrite_WithUnaryExpressionNotArrayLengthOrConvert_Throws()
    {
        // Create a unary expression that is not ArrayLength or Convert (e.g., Not)
        // This should trigger the unsupported expression path
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(bool), "x");
        var notExpr = System.Linq.Expressions.Expression.Not(parameter);

        Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(notExpr));
    }

    /// <summary>
    ///     Verifies that unsupported binary expressions throw with helpful message.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithUnsupportedBinaryExpression_ThrowsWithHelpfulMessage()
    {
        Expression<Func<int, bool>> expr = x => x > 5;

        var ex = Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(expr.Body));
        await Assert.That(ex!.Message).Contains("Did you meant to use expressions");
    }

    /// <summary>
    ///     Verifies that unsupported expressions throw with node type in message.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithUnsupportedExpression_Throws()
    {
        Expression<Func<int, int>> expr = x => x + 1;

        var ex = Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(expr.Body));
        await Assert.That(ex!.Message).Contains("Unsupported expression");
        await Assert.That(ex.Message).Contains("Add");
    }

    /// <summary>
    ///     Verifies that Convert expression wrapping a member access is unwrapped to the inner member access.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithConvertWrappingMemberAccess_UnwrapsToMemberAccess()
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TestClass), "x");
        var property = System.Linq.Expressions.Expression.Property(parameter, "Property");
        var convert = System.Linq.Expressions.Expression.Convert(property, typeof(object));

        var result = Reflection.Rewrite(convert);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.MemberAccess);
        var memberExpr = (MemberExpression)result;
        await Assert.That(memberExpr.Member.Name).IsEqualTo("Property");
    }

    /// <summary>
    ///     Verifies that a static method call (non-special-name) throws NotSupportedException.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithStaticMethodCall_ThrowsNotSupportedException()
    {
        var methodCall = System.Linq.Expressions.Expression.Call(
            typeof(int).GetMethod("Parse", new[] { typeof(string) })!,
            System.Linq.Expressions.Expression.Constant("42"));

        var ex = Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(methodCall));
        await Assert.That(ex).IsNotNull();
    }

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used as type parameter in expression lambdas.")]
    private sealed class TestClass
    {
        public int[] Array { get; } = [1, 2, 3];

        public List<int> List { get; } = [4, 5, 6];

        public TestClass? Nested { get; set; }

        public string? Property { get; set; }

        public string? GetValue() => Property;
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;

using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Tests for <see cref="RoslynHelpers"/> predicate methods.
/// </summary>
public class RoslynHelpersTests
{
    /// <summary>
    /// Verifies that GetMemberAccessName returns the method name for a member access invocation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetMemberAccessName_MemberAccessInvocation_ReturnsName()
    {
        var code = "obj.WhenChanged(x => x.Name)";
        var expr = SyntaxFactory.ParseExpression(code);

        var result = RoslynHelpers.GetMemberAccessName(expr);

        await Assert.That(result).IsEqualTo("WhenChanged");
    }

    /// <summary>
    /// Verifies that GetMemberAccessName returns null for a non-invocation node.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetMemberAccessName_NonInvocation_ReturnsNull()
    {
        var expr = SyntaxFactory.ParseExpression("x + y");

        var result = RoslynHelpers.GetMemberAccessName(expr);

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that GetMemberAccessName returns null for a simple invocation without member access.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetMemberAccessName_SimpleInvocation_ReturnsNull()
    {
        var expr = SyntaxFactory.ParseExpression("DoSomething()");

        var result = RoslynHelpers.GetMemberAccessName(expr);

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies IsBindSpecificInvocation returns true only for Bind method name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsBindSpecificInvocation_BindMethod_ReturnsTrue()
    {
        var code = "obj.Bind(view, x => x.Name, x => x.Text)";
        var node = SyntaxFactory.ParseExpression(code);

        var result = RoslynHelpers.IsBindSpecificInvocation(node, CancellationToken.None);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies IsBindSpecificInvocation returns false for BindOneWay method.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsBindSpecificInvocation_BindOneWayMethod_ReturnsFalse()
    {
        var code = "obj.BindOneWay(view, x => x.Name, x => x.Text)";
        var node = SyntaxFactory.ParseExpression(code);

        var result = RoslynHelpers.IsBindSpecificInvocation(node, CancellationToken.None);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies IsBindOneWaySpecificInvocation returns true for BindOneWay.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsBindOneWaySpecificInvocation_BindOneWayMethod_ReturnsTrue()
    {
        var code = "obj.BindOneWay(view, x => x.Name, x => x.Text)";
        var node = SyntaxFactory.ParseExpression(code);

        var result = RoslynHelpers.IsBindOneWaySpecificInvocation(node, CancellationToken.None);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies IsBindOneWaySpecificInvocation returns false for BindTwoWay.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsBindOneWaySpecificInvocation_BindTwoWayMethod_ReturnsFalse()
    {
        var code = "obj.BindTwoWay(view, x => x.Name, x => x.Text)";
        var node = SyntaxFactory.ParseExpression(code);

        var result = RoslynHelpers.IsBindOneWaySpecificInvocation(node, CancellationToken.None);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies IsBindTwoWaySpecificInvocation returns true for BindTwoWay.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsBindTwoWaySpecificInvocation_BindTwoWayMethod_ReturnsTrue()
    {
        var code = "obj.BindTwoWay(view, x => x.Name, x => x.Text)";
        var node = SyntaxFactory.ParseExpression(code);

        var result = RoslynHelpers.IsBindTwoWaySpecificInvocation(node, CancellationToken.None);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies IsOneWayBindSpecificInvocation returns true for OneWayBind.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsOneWayBindSpecificInvocation_OneWayBindMethod_ReturnsTrue()
    {
        var code = "obj.OneWayBind(view, x => x.Name, x => x.Text)";
        var node = SyntaxFactory.ParseExpression(code);

        var result = RoslynHelpers.IsOneWayBindSpecificInvocation(node, CancellationToken.None);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies IsOneWayBindSpecificInvocation returns false for Bind.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsOneWayBindSpecificInvocation_BindMethod_ReturnsFalse()
    {
        var code = "obj.Bind(view, x => x.Name, x => x.Text)";
        var node = SyntaxFactory.ParseExpression(code);

        var result = RoslynHelpers.IsOneWayBindSpecificInvocation(node, CancellationToken.None);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies specific predicates return false for non-invocation nodes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SpecificPredicates_NonInvocation_ReturnFalse()
    {
        var node = SyntaxFactory.ParseExpression("x + y");
        var ct = CancellationToken.None;

        await Assert.That(RoslynHelpers.IsBindSpecificInvocation(node, ct)).IsFalse();
        await Assert.That(RoslynHelpers.IsBindOneWaySpecificInvocation(node, ct)).IsFalse();
        await Assert.That(RoslynHelpers.IsBindTwoWaySpecificInvocation(node, ct)).IsFalse();
        await Assert.That(RoslynHelpers.IsOneWayBindSpecificInvocation(node, ct)).IsFalse();
    }
}

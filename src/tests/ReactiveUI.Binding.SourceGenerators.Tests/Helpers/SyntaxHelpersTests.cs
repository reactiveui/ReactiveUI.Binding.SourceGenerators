// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Tests for <see cref="SyntaxHelpers"/> methods.
/// </summary>
public class SyntaxHelpersTests
{
    /// <summary>
    /// Verifies ExtractPropertyPathFromLambda extracts a single property path from a lambda expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractPropertyPathFromLambda_SingleProperty_ExtractsPath()
    {
        const string source = """
            using System;
            using System.ComponentModel;
            using System.Linq.Expressions;

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        Expression<Func<MyViewModel, string>> expr = x => x.Name;
                    }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var lambda = tree.GetRoot().DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().First();

        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(lambda, semanticModel, default);

        await Assert.That(path).IsNotNull();
        await Assert.That(path!.Length).IsEqualTo(1);
        await Assert.That(path[0].PropertyName).IsEqualTo("Name");
    }

    /// <summary>
    /// Verifies ExtractPropertyPathFromLambda extracts a chained property path.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractPropertyPathFromLambda_ChainedProperty_ExtractsFullPath()
    {
        const string source = """
            using System;
            using System.ComponentModel;
            using System.Linq.Expressions;

            namespace TestApp
            {
                public class Address
                {
                    public string City { get; set; } = "";
                }

                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public Address Address { get; set; } = new();
                }

                public class Usage
                {
                    public void Test()
                    {
                        Expression<Func<MyViewModel, string>> expr = x => x.Address.City;
                    }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var lambda = tree.GetRoot().DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().First();

        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(lambda, semanticModel, default);

        await Assert.That(path).IsNotNull();
        await Assert.That(path!.Length).IsEqualTo(2);
        await Assert.That(path[0].PropertyName).IsEqualTo("Address");
        await Assert.That(path[1].PropertyName).IsEqualTo("City");
    }

    /// <summary>
    /// Verifies ExtractPropertyPathFromLambda returns null for non-lambda expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractPropertyPathFromLambda_NonLambda_ReturnsNull()
    {
        const string source = """
            using System;

            namespace TestApp
            {
                public class Usage
                {
                    public void Test()
                    {
                        var x = 42;
                    }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        // Pass a non-lambda expression (the literal 42)
        var literal = tree.GetRoot().DescendantNodes().OfType<LiteralExpressionSyntax>().First();

        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(literal, semanticModel, default);

        await Assert.That(path).IsNull();
    }

    /// <summary>
    /// Verifies GetLambdaBody returns the body for a simple lambda expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetLambdaBody_SimpleLambda_ReturnsBody()
    {
        const string source = """
            using System;
            namespace TestApp
            {
                public class Usage
                {
                    public void Test()
                    {
                        System.Func<int, int> f = x => x;
                    }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var lambda = tree.GetRoot().DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().First();

        var body = SyntaxHelpers.GetLambdaBody(lambda);

        await Assert.That(body).IsNotNull();
        await Assert.That(body).IsTypeOf<IdentifierNameSyntax>();
    }

    /// <summary>
    /// Verifies GetLambdaBody returns the body for a parenthesized lambda expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetLambdaBody_ParenthesizedLambda_ReturnsBody()
    {
        const string source = """
            using System;
            namespace TestApp
            {
                public class Usage
                {
                    public void Test()
                    {
                        System.Func<int, int, int> f = (x, y) => x + y;
                    }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var lambda = tree.GetRoot().DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().First();

        var body = SyntaxHelpers.GetLambdaBody(lambda);

        await Assert.That(body).IsNotNull();
        await Assert.That(body).IsTypeOf<BinaryExpressionSyntax>();
    }

    /// <summary>
    /// Verifies GetLambdaBody returns null for a parenthesized lambda with a block body.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetLambdaBody_BlockBody_ReturnsNull()
    {
        const string source = """
            using System;
            namespace TestApp
            {
                public class Usage
                {
                    public void Test()
                    {
                        System.Func<int, int> f = (x) => { return x; };
                    }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var lambda = tree.GetRoot().DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().First();

        var body = SyntaxHelpers.GetLambdaBody(lambda);

        await Assert.That(body).IsNull();
    }

    /// <summary>
    /// Verifies ExtractPropertyPathFromLambda works with parenthesized lambda expressions.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractPropertyPathFromLambda_ParenthesizedLambda_ExtractsPath()
    {
        const string source = """
            using System;
            using System.ComponentModel;
            using System.Linq.Expressions;

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        Expression<Func<MyViewModel, string>> expr = (x) => x.Name;
                    }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var lambda = tree.GetRoot().DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().First();

        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(lambda, semanticModel, default);

        await Assert.That(path).IsNotNull();
        await Assert.That(path!.Length).IsEqualTo(1);
        await Assert.That(path[0].PropertyName).IsEqualTo("Name");
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Tests for <see cref="MetadataExtractor"/> helper methods that require Roslyn compilation context.
/// </summary>
public class MetadataExtractorHelperTests
{
    /// <summary>
    /// Verifies ExtractProperties extracts public properties from a type symbol.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractProperties_PublicProperties_ExtractsAll()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string Name { get; set; } = "";
                    public int Age { get; set; }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyViewModel");

        var properties = MetadataExtractor.ExtractProperties(typeSymbol, default);

        await Assert.That(properties.Length).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies ExtractProperties skips static properties.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractProperties_StaticProperty_Skipped()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public static string StaticProp { get; set; } = "";
                    public string Name { get; set; } = "";
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyViewModel");

        var properties = MetadataExtractor.ExtractProperties(typeSymbol, default);

        // Only Name should be extracted; StaticProp should be skipped
        await Assert.That(properties.Length).IsEqualTo(1);
        await Assert.That(properties[0].PropertyName).IsEqualTo("Name");
    }

    /// <summary>
    /// Verifies ExtractProperties skips write-only properties.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractProperties_WriteOnlyProperty_Skipped()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    private string _name = "";
                    public string Name { set { _name = value; } }
                    public string Title { get; set; } = "";
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyViewModel");

        var properties = MetadataExtractor.ExtractProperties(typeSymbol, default);

        // Only Title should be extracted; Name (write-only) should be skipped
        await Assert.That(properties.Length).IsEqualTo(1);
        await Assert.That(properties[0].PropertyName).IsEqualTo("Title");
    }

    /// <summary>
    /// Verifies ExtractProperties skips indexer properties.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractProperties_Indexer_Included()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string this[int index] => "";
                    public string Name { get; set; } = "";
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyViewModel");

        var properties = MetadataExtractor.ExtractProperties(typeSymbol, default);

        // Both the indexer and Name are included; the indexer has IsIndexer = true
        var indexerProp = properties.FirstOrDefault(p => p.IsIndexer);
        await Assert.That(properties.Length).IsEqualTo(2);
        await Assert.That(indexerProp).IsNotNull();
    }

    /// <summary>
    /// Verifies ExtractProperties detects dependency properties via companion static field heuristic.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractProperties_DependencyPropertyHeuristic_DetectsDP()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public static readonly object TitleProperty = new object();
                    public string Title { get; set; } = "";
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyViewModel");

        var properties = MetadataExtractor.ExtractProperties(typeSymbol, default);

        var titleProp = properties.First(p => p.PropertyName == "Title");
        await Assert.That(titleProp.IsDependencyProperty).IsTrue();
    }

    /// <summary>
    /// Verifies GetWellKnownSymbols resolves INPC symbol from a compilation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetWellKnownSymbols_ResolvesINPC()
    {
        const string source = """
            using System.ComponentModel;
            namespace TestApp { public class Dummy {} }
            """;

        var compilation = TestHelper.CreateCompilation(source);

        var symbols = MetadataExtractor.GetWellKnownSymbols(compilation);

        await Assert.That(symbols.INPC).IsNotNull();
        await Assert.That(symbols.INPC!.Name).IsEqualTo("INotifyPropertyChanged");
    }

    /// <summary>
    /// Verifies GetWellKnownSymbols returns cached instance for same compilation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetWellKnownSymbols_SameCompilation_ReturnsCachedInstance()
    {
        const string source = """
            namespace TestApp { public class Dummy {} }
            """;

        var compilation = TestHelper.CreateCompilation(source);

        var symbols1 = MetadataExtractor.GetWellKnownSymbols(compilation);
        var symbols2 = MetadataExtractor.GetWellKnownSymbols(compilation);

        await Assert.That(ReferenceEquals(symbols1, symbols2)).IsTrue();
    }

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

        var path = MetadataExtractor.ExtractPropertyPathFromLambda(lambda, semanticModel, default);

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

        var path = MetadataExtractor.ExtractPropertyPathFromLambda(lambda, semanticModel, default);

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

        var path = MetadataExtractor.ExtractPropertyPathFromLambda(literal, semanticModel, default);

        await Assert.That(path).IsNull();
    }

    internal static INamedTypeSymbol GetNamedTypeSymbol(Compilation compilation, string typeName)
    {
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);
        var classDecl = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
            .First(c => c.Identifier.Text == typeName);
        return (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(classDecl)!;
    }
}

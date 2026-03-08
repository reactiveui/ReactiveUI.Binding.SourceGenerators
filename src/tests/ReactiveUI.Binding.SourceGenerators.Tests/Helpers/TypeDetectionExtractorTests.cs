// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Tests for <see cref="TypeDetectionExtractor"/> methods.
/// </summary>
public class TypeDetectionExtractorTests
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

        var properties = TypeDetectionExtractor.ExtractProperties(typeSymbol, default);

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

        var properties = TypeDetectionExtractor.ExtractProperties(typeSymbol, default);

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

        var properties = TypeDetectionExtractor.ExtractProperties(typeSymbol, default);

        // Only Title should be extracted; Name (write-only) should be skipped
        await Assert.That(properties.Length).IsEqualTo(1);
        await Assert.That(properties[0].PropertyName).IsEqualTo("Title");
    }

    /// <summary>
    /// Verifies ExtractProperties includes indexer properties with IsIndexer flag.
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

        var properties = TypeDetectionExtractor.ExtractProperties(typeSymbol, default);

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

        var properties = TypeDetectionExtractor.ExtractProperties(typeSymbol, default);

        var titleProp = properties.First(p => p.PropertyName == "Title");
        await Assert.That(titleProp.IsDependencyProperty).IsTrue();
    }

    /// <summary>
    /// Gets a named type symbol from a compilation.
    /// </summary>
    /// <param name="compilation">The compilation.</param>
    /// <param name="typeName">The type name.</param>
    /// <returns>The named type symbol.</returns>
    private static INamedTypeSymbol GetNamedTypeSymbol(Compilation compilation, string typeName)
    {
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);
        var classDecl = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
            .First(c => c.Identifier.Text == typeName);
        return (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(classDecl)!;
    }
}

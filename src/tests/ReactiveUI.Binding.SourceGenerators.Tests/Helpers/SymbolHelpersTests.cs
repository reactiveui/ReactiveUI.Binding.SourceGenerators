// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Tests for <see cref="SymbolHelpers"/> methods.
/// </summary>
public class SymbolHelpersTests
{
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

        var symbols = SymbolHelpers.GetWellKnownSymbols(compilation);

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
        const string source = "namespace TestApp { public class Dummy {} }";

        var compilation = TestHelper.CreateCompilation(source);

        var symbols1 = SymbolHelpers.GetWellKnownSymbols(compilation);
        var symbols2 = SymbolHelpers.GetWellKnownSymbols(compilation);

        await Assert.That(ReferenceEquals(symbols1, symbols2)).IsTrue();
    }

    /// <summary>
    /// Verifies IsIObservable returns true for a direct IObservable&lt;T&gt; type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsIObservable_DirectIObservableType_ReturnsTrue()
    {
        const string source = """
            using System;
            namespace TestApp
            {
                public class MyVm { public IObservable<string> Obs { get; set; } }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyVm");
        var prop = typeSymbol.GetMembers("Obs").OfType<IPropertySymbol>().First();
        var obsPropType = (INamedTypeSymbol)prop.Type;

        var result = SymbolHelpers.IsIObservable(obsPropType);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies IsIObservable returns false for a non-observable type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsIObservable_NonObservableType_ReturnsFalse()
    {
        const string source = """
            namespace TestApp
            {
                public class MyVm { public string Name { get; set; } = ""; }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyVm");
        var prop = typeSymbol.GetMembers("Name").OfType<IPropertySymbol>().First();
        var stringType = (INamedTypeSymbol)prop.Type;

        var result = SymbolHelpers.IsIObservable(stringType);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies IsInteractionType returns true for a direct IInteraction&lt;TInput,TOutput&gt; type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsInteractionType_DirectIInteractionType_ReturnsTrue()
    {
        const string source = """
            using ReactiveUI.Binding;
            namespace TestApp
            {
                public class MyVm { public IInteraction<string, bool> Confirm { get; set; } }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyVm");
        var prop = typeSymbol.GetMembers("Confirm").OfType<IPropertySymbol>().First();
        var interactionType = (INamedTypeSymbol)prop.Type;

        var result = SymbolHelpers.IsInteractionType(interactionType);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies IsInteractionType returns false for a non-interaction type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsInteractionType_NonInteractionType_ReturnsFalse()
    {
        const string source = """
            namespace TestApp
            {
                public class MyVm { public string Name { get; set; } = ""; }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyVm");
        var prop = typeSymbol.GetMembers("Name").OfType<IPropertySymbol>().First();
        var stringType = (INamedTypeSymbol)prop.Type;

        var result = SymbolHelpers.IsInteractionType(stringType);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies ExtractInteractionTypeArguments returns true and extracts types when the property
    /// type is <c>IInteraction&lt;TInput, TOutput&gt;</c> directly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractInteractionTypeArguments_DirectIInteractionProperty_ReturnsTrueWithTypes()
    {
        const string source = """
            using ReactiveUI.Binding;
            namespace TestApp
            {
                public class MyVm { public IInteraction<string, bool> Confirm { get; set; } }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyVm");
        var prop = typeSymbol.GetMembers("Confirm").OfType<IPropertySymbol>().First();

        var result = SymbolHelpers.ExtractInteractionTypeArguments(
            prop.Type, out var inputType, out var outputType);

        await Assert.That(result).IsTrue();
        await Assert.That(inputType).IsEqualTo("string");
        await Assert.That(outputType).IsEqualTo("bool");
    }

    /// <summary>
    /// Verifies ExtractInteractionTypeArguments returns false when the type is not an interaction type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractInteractionTypeArguments_NonInteractionType_ReturnsFalse()
    {
        const string source = """
            namespace TestApp
            {
                public class MyVm { public string Name { get; set; } = ""; }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyVm");
        var prop = typeSymbol.GetMembers("Name").OfType<IPropertySymbol>().First();

        var result = SymbolHelpers.ExtractInteractionTypeArguments(
            prop.Type, out var inputType, out var outputType);

        await Assert.That(result).IsFalse();
        await Assert.That(inputType).IsEqualTo(string.Empty);
        await Assert.That(outputType).IsEqualTo(string.Empty);
    }

    /// <summary>
    /// Verifies ExtractInnerObservableType correctly extracts the inner type when the property's
    /// type implements IObservable&lt;T&gt; via an interface (not directly IObservable&lt;T&gt;).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractInnerObservableType_ViaInterfaceImplementation_ReturnsInnerType()
    {
        const string source = """
            using System;
            using System.Reactive.Subjects;
            using System.Linq.Expressions;
            namespace TestApp
            {
                public class MyVm { public Subject<int> Count { get; set; } }
                public class Usage
                {
                    public void Test()
                    {
                        Expression<Func<MyVm, Subject<int>>> expr = x => x.Count;
                    }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var lambda = tree.GetRoot().DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().First();
        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(lambda, semanticModel, default);

        await Assert.That(path).IsNotNull();
        var leafSegment = path![0];

        var innerType = SymbolHelpers.ExtractInnerObservableType(leafSegment, semanticModel, lambda, default);

        await Assert.That(innerType).IsEqualTo("int");
    }

    /// <summary>
    /// Verifies ExtractInnerObservableType falls back to the leaf segment type when the
    /// expression is not a simple member-access lambda.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractInnerObservableType_NonLambdaArg_ReturnsFallbackType()
    {
        const string source = """
            using System;
            using System.Linq.Expressions;
            namespace TestApp
            {
                public class MyVm { public IObservable<string> Obs { get; set; } }
                public class Usage
                {
                    public void Test()
                    {
                        Expression<Func<MyVm, IObservable<string>>> expr = x => x.Obs;
                    }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var lambda = tree.GetRoot().DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().First();
        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(lambda, semanticModel, default);

        await Assert.That(path).IsNotNull();
        var leafSegment = path![0];

        // Pass a non-lambda expression as argExpression to hit the fallback return
        var memberAccess = (ExpressionSyntax)tree.GetRoot()
            .DescendantNodes()
            .OfType<MemberAccessExpressionSyntax>()
            .First();

        var innerType = SymbolHelpers.ExtractInnerObservableType(leafSegment, semanticModel, memberAccess, default);

        // Fallback: should return the leaf segment's PropertyTypeFullName unchanged
        await Assert.That(innerType).IsEqualTo(leafSegment.PropertyTypeFullName);
    }

    /// <summary>
    /// Verifies DetectHasConverterOverride returns true when a parameter is typed as IBindingTypeConverter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DetectHasConverterOverride_WithIBindingTypeConverterParam_ReturnsTrue()
    {
        const string source = """
            using ReactiveUI.Binding;
            namespace TestApp
            {
                public static class Ext
                {
                    public static void Bind(IBindingTypeConverter converter) { }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);
        var classDecl = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
            .First(c => c.Identifier.Text == "Ext");
        var classSymbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(classDecl)!;
        var methodSymbol = classSymbol.GetMembers("Bind").OfType<IMethodSymbol>().First();
        var converterParam = methodSymbol.Parameters[0];

        var result = SymbolHelpers.DetectHasConverterOverride(converterParam);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies DetectHasConverterOverride returns false when a parameter has a Func type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DetectHasConverterOverride_WithFuncParam_ReturnsFalse()
    {
        const string source = """
            using System;
            namespace TestApp
            {
                public static class Ext
                {
                    public static void Bind(Func<string, int> converter) { }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);
        var classDecl = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
            .First(c => c.Identifier.Text == "Ext");
        var classSymbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(classDecl)!;
        var methodSymbol = classSymbol.GetMembers("Bind").OfType<IMethodSymbol>().First();
        var converterParam = methodSymbol.Parameters[0];

        var result = SymbolHelpers.DetectHasConverterOverride(converterParam);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies ResolveNamedType returns null when the lambda has a block body.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveNamedType_BlockBodyLambda_ReturnsNull()
    {
        const string source = """
            using System;
            using System.Linq.Expressions;
            namespace TestApp
            {
                public class MyVm { public string Name { get; set; } = ""; }
                public class Usage
                {
                    public string Test(MyVm vm)
                    {
                        Func<MyVm, string> fn = (x) => { return x.Name; };
                        return fn(vm);
                    }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        // Get a valid segment from a separate compilation with expression-body lambda
        const string simpleLambdaSource = """
            using System;
            using System.Linq.Expressions;
            namespace TestApp
            {
                public class MyVm2 { public string Name { get; set; } = ""; }
                public class Usage2
                {
                    public void Test()
                    {
                        Expression<Func<MyVm2, string>> expr = x => x.Name;
                    }
                }
            }
            """;

        var compilation2 = TestHelper.CreateCompilation(simpleLambdaSource, LanguageVersion.CSharp10);
        var tree2 = compilation2.SyntaxTrees.First();
        var semanticModel2 = compilation2.GetSemanticModel(tree2);
        var refLambda = tree2.GetRoot().DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().First();
        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(refLambda, semanticModel2, default);
        await Assert.That(path).IsNotNull();
        var segment = path![0];

        // Now get the block-body parenthesized lambda from original source
        var blockLambda = tree.GetRoot().DescendantNodes()
            .OfType<ParenthesizedLambdaExpressionSyntax>()
            .First();

        var result = SymbolHelpers.ResolveNamedType(segment, semanticModel, blockLambda, default);

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies ResolveNamedType returns null when the expression is not a lambda expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveNamedType_NonLambdaExpression_ReturnsNull()
    {
        const string source = """
            using System;
            using System.Linq.Expressions;
            namespace TestApp
            {
                public class MyVm { public string Name { get; set; } = ""; }
                public class Usage
                {
                    public void Test()
                    {
                        Expression<Func<MyVm, string>> expr = x => x.Name;
                    }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var lambda = tree.GetRoot().DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().First();
        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(lambda, semanticModel, default);

        await Assert.That(path).IsNotNull();

        // Pass the lambda BODY (a member access expression, not a lambda) as argExpression
        var memberAccess = (ExpressionSyntax)lambda.Body;
        var result = SymbolHelpers.ResolveNamedType(path![0], semanticModel, memberAccess, default);

        await Assert.That(result).IsNull();
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

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;

using ReactiveUI.Binding.SourceGenerators.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>
/// Tests for <see cref="CommandExtractor"/> internal helper methods.
/// </summary>
public class CommandExtractorHelperTests
{
    /// <summary>
    /// Verifies that IsToEventArgument returns true for a named argument with "toEvent".
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsToEventArgument_NamedToEvent_ReturnsTrue()
    {
        var argument = ParseFirstArgument("Method(toEvent: \"Click\")");

        var result = CommandExtractor.IsToEventArgument(argument, argumentIndex: 0, parameterIndex: 5);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that IsToEventArgument returns false for a named argument with a different name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsToEventArgument_NamedOtherParam_ReturnsFalse()
    {
        var argument = ParseFirstArgument("Method(scheduler: null)");

        var result = CommandExtractor.IsToEventArgument(argument, argumentIndex: 0, parameterIndex: 5);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that IsToEventArgument returns true for a positional argument at the correct index.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsToEventArgument_PositionalAtCorrectIndex_ReturnsTrue()
    {
        var argument = ParseFirstArgument("Method(\"Click\")");

        var result = CommandExtractor.IsToEventArgument(argument, argumentIndex: 3, parameterIndex: 3);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that IsToEventArgument returns false for a positional argument at the wrong index.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsToEventArgument_PositionalAtWrongIndex_ReturnsFalse()
    {
        var argument = ParseFirstArgument("Method(\"Click\")");

        var result = CommandExtractor.IsToEventArgument(argument, argumentIndex: 2, parameterIndex: 5);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that HasCommandProperties returns false for a type with no Command property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasCommandProperties_NoCommand_ReturnsFalse()
    {
        var source = """
            namespace TestApp
            {
                public class PlainControl
                {
                    public string Text { get; set; } = "";
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var classSymbol = GetFirstClassSymbol(tree, model);

        var result = CommandExtractor.HasCommandProperties(classSymbol, out var hasParam);

        await Assert.That(result).IsFalse();
        await Assert.That(hasParam).IsFalse();
    }

    /// <summary>
    /// Verifies that HasCommandProperties returns true for a type with a settable Command (ICommand) property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasCommandProperties_WithCommand_ReturnsTrue()
    {
        var source = """
            using System.Windows.Input;
            namespace TestApp
            {
                public class ButtonControl
                {
                    public ICommand Command { get; set; }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var classSymbol = GetFirstClassSymbol(tree, model);

        var result = CommandExtractor.HasCommandProperties(classSymbol, out var hasParam);

        await Assert.That(result).IsTrue();
        await Assert.That(hasParam).IsFalse();
    }

    /// <summary>
    /// Verifies that HasCommandProperties returns true with CommandParameter when both exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasCommandProperties_WithCommandAndParameter_ReturnsTrueWithParam()
    {
        var source = """
            using System.Windows.Input;
            namespace TestApp
            {
                public class ButtonControl
                {
                    public ICommand Command { get; set; }
                    public object CommandParameter { get; set; }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var classSymbol = GetFirstClassSymbol(tree, model);

        var result = CommandExtractor.HasCommandProperties(classSymbol, out var hasParam);

        await Assert.That(result).IsTrue();
        await Assert.That(hasParam).IsTrue();
    }

    /// <summary>
    /// Verifies that HasCommandProperties does not match a readonly Command property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasCommandProperties_ReadOnlyCommand_ReturnsFalse()
    {
        var source = """
            using System.Windows.Input;
            namespace TestApp
            {
                public class ReadOnlyCommandControl
                {
                    public ICommand Command { get; }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var classSymbol = GetFirstClassSymbol(tree, model);

        var result = CommandExtractor.HasCommandProperties(classSymbol, out var hasParam);

        await Assert.That(result).IsFalse();
        await Assert.That(hasParam).IsFalse();
    }

    /// <summary>
    /// Verifies that HasEnabledProperty returns false when no Enabled property exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasEnabledProperty_NoEnabledProperty_ReturnsFalse()
    {
        var source = """
            namespace TestApp
            {
                public class PlainControl
                {
                    public string Text { get; set; } = "";
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var classSymbol = GetFirstClassSymbol(tree, model);

        var result = CommandExtractor.HasEnabledProperty(classSymbol);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that HasEnabledProperty returns true when a settable bool Enabled property exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasEnabledProperty_WithEnabled_ReturnsTrue()
    {
        var source = """
            namespace TestApp
            {
                public class WinFormsControl
                {
                    public bool Enabled { get; set; }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var classSymbol = GetFirstClassSymbol(tree, model);

        var result = CommandExtractor.HasEnabledProperty(classSymbol);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that HasEnabledProperty returns false for non-bool Enabled property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasEnabledProperty_StringEnabled_ReturnsFalse()
    {
        var source = """
            namespace TestApp
            {
                public class ControlWithStringEnabled
                {
                    public string Enabled { get; set; } = "";
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var classSymbol = GetFirstClassSymbol(tree, model);

        var result = CommandExtractor.HasEnabledProperty(classSymbol);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that FindParameterLambda returns null when there are only 3 arguments
    /// (loop starts at index 3, so never enters).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindParameterLambda_ThreeArgsOnly_ReturnsNull()
    {
        var source = """
            namespace TestApp
            {
                public class Vm { public string Name { get; set; } = ""; }
                public class View { public string Text { get; set; } = ""; }

                public class Caller
                {
                    public void Go()
                    {
                        Method(new Vm(), new View(), "test");
                    }

                    public void Method(Vm vm, View view, string s) { }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var invocation = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .First();

        var result = CommandExtractor.FindParameterLambda(invocation.ArgumentList.Arguments, model, CancellationToken.None);

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that FindParameterLambda returns null when args after index 3
    /// are not valid lambda expressions.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindParameterLambda_NonLambdaArgs_ReturnsNull()
    {
        var source = """
            namespace TestApp
            {
                public class Vm { public string Name { get; set; } = ""; }
                public class View { public string Text { get; set; } = ""; }

                public class Caller
                {
                    public void Go()
                    {
                        Method(new Vm(), new View(), "test", "not a lambda");
                    }

                    public void Method(Vm vm, View view, string s1, string s2) { }
                }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var invocation = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .First();

        var result = CommandExtractor.FindParameterLambda(invocation.ArgumentList.Arguments, model, CancellationToken.None);

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Parses an invocation expression and returns the first argument.
    /// </summary>
    /// <param name="expression">The invocation expression text to parse.</param>
    /// <returns>The first argument syntax node.</returns>
    private static Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax ParseFirstArgument(string expression)
    {
        var parsed = SyntaxFactory.ParseExpression(expression);
        var invocation = (Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax)parsed;
        return invocation.ArgumentList.Arguments[0];
    }

    /// <summary>
    /// Gets the first class symbol from a syntax tree.
    /// </summary>
    /// <param name="tree">The syntax tree.</param>
    /// <param name="model">The semantic model.</param>
    /// <returns>The first named type symbol.</returns>
    private static Microsoft.CodeAnalysis.INamedTypeSymbol GetFirstClassSymbol(
        Microsoft.CodeAnalysis.SyntaxTree tree,
        Microsoft.CodeAnalysis.SemanticModel model)
    {
        var classDecl = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .First();

        return (Microsoft.CodeAnalysis.INamedTypeSymbol)model.GetDeclaredSymbol(classDecl)!;
    }
}

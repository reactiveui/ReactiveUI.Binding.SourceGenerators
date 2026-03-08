// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using NSubstitute;

using ReactiveUI.Binding.Analyzer.Analyzers;

namespace ReactiveUI.Binding.Analyzer.Tests.Helpers;

/// <summary>
/// Tests for <see cref="AnalyzerHelpers"/>.
/// </summary>
public class AnalyzerHelpersTests
{
    /// <summary>
    /// Verifies that ExtractFirstTypeArgument returns null when the method has no type arguments.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractFirstTypeArgument_NoTypeArguments_ReturnsNull()
    {
        var source = """
            namespace TestApp
            {
                public class MyClass
                {
                    public void NonGenericMethod() { }
                }
            }
            """;

        var compilation = CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var methodDecl = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var methodSymbol = model.GetDeclaredSymbol(methodDecl)!;

        var result = AnalyzerHelpers.ExtractFirstTypeArgument(methodSymbol);

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that ExtractFirstTypeArgument returns the type when the method has a named type argument.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractFirstTypeArgument_WithNamedTypeArgument_ReturnsType()
    {
        var source = """
            namespace TestApp
            {
                public class MyClass
                {
                    public void GenericMethod<T>() { }

                    public void Usage()
                    {
                        GenericMethod<string>();
                    }
                }
            }
            """;

        var compilation = CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var invocation = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .First();

        var methodSymbol = (IMethodSymbol)model.GetSymbolInfo(invocation).Symbol!;

        var result = AnalyzerHelpers.ExtractFirstTypeArgument(methodSymbol);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Name).IsEqualTo("String");
    }

    /// <summary>
    /// Verifies that IsBindingExtensionMethod returns false when ContainingType is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsBindingExtensionMethod_NullContainingType_ReturnsFalse()
    {
        var methodSymbol = Substitute.For<IMethodSymbol>();
        methodSymbol.ContainingType.Returns((INamedTypeSymbol?)null);

        var result = AnalyzerHelpers.IsBindingExtensionMethod(methodSymbol);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that ExtractFirstTypeArgument returns null when TypeArguments is empty
    /// (using a substitute method symbol).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractFirstTypeArgument_EmptyTypeArguments_ReturnsNull_Substitute()
    {
        var methodSymbol = Substitute.For<IMethodSymbol>();
        methodSymbol.TypeArguments.Returns(ImmutableArray<ITypeSymbol>.Empty);

        var result = AnalyzerHelpers.ExtractFirstTypeArgument(methodSymbol);

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that IsBindingExtensionMethod returns false for a method whose containing type
    /// is not the generated extension class.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsBindingExtensionMethod_UnrelatedMethod_ReturnsFalse()
    {
        var source = """
            namespace TestApp
            {
                public class SomeClass
                {
                    public void DoSomething() { }
                }
            }
            """;

        var compilation = CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var methodDecl = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var methodSymbol = model.GetDeclaredSymbol(methodDecl)!;

        var result = AnalyzerHelpers.IsBindingExtensionMethod(methodSymbol);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that IsBindingExtensionMethod returns true for a method on the generated extension class.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsBindingExtensionMethod_GeneratedClass_ReturnsTrue()
    {
        var source = """
            namespace ReactiveUI.Binding
            {
                public static class __ReactiveUIGeneratedBindings
                {
                    public static void WhenChanged() { }
                }
            }
            """;

        var compilation = CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var methodDecl = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var methodSymbol = model.GetDeclaredSymbol(methodDecl)!;

        var result = AnalyzerHelpers.IsBindingExtensionMethod(methodSymbol);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that IsBindingExtensionMethod returns true for a method on the stub extension class.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsBindingExtensionMethod_StubClass_ReturnsTrue()
    {
        var source = """
            namespace ReactiveUI.Binding
            {
                public static class ReactiveUIBindingExtensions
                {
                    public static void WhenChanged() { }
                }
            }
            """;

        var compilation = CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var methodDecl = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var methodSymbol = model.GetDeclaredSymbol(methodDecl)!;

        var result = AnalyzerHelpers.IsBindingExtensionMethod(methodSymbol);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that IsInlineLambda returns true for a SimpleLambdaExpressionSyntax.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsInlineLambda_SimpleLambda_ReturnsTrue()
    {
        var expr = SyntaxFactory.ParseExpression("x => x.Name");
        var result = AnalyzerHelpers.IsInlineLambda(expr);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that IsInlineLambda returns true for a ParenthesizedLambdaExpressionSyntax.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsInlineLambda_ParenthesizedLambda_ReturnsTrue()
    {
        var expr = SyntaxFactory.ParseExpression("(x) => x.Name");
        var result = AnalyzerHelpers.IsInlineLambda(expr);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that IsInlineLambda returns false for a non-lambda expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsInlineLambda_Identifier_ReturnsFalse()
    {
        var expr = SyntaxFactory.ParseExpression("myVariable");
        var result = AnalyzerHelpers.IsInlineLambda(expr);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that ImplementsInterface returns false when the type does not implement the interface.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ImplementsInterface_DoesNotImplement_ReturnsFalse()
    {
        var source = """
            using System.ComponentModel;
            namespace TestApp
            {
                public class PlainClass { }
            }
            """;

        var compilation = CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var classDecl = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .First();

        var classSymbol = (INamedTypeSymbol)model.GetDeclaredSymbol(classDecl)!;
        var inpc = compilation.GetTypeByMetadataName("System.ComponentModel.INotifyPropertyChanged")!;

        var result = AnalyzerHelpers.ImplementsInterface(classSymbol, inpc);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that ImplementsInterface returns true when the type implements the interface.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ImplementsInterface_Implements_ReturnsTrue()
    {
        var source = """
            using System.ComponentModel;
            namespace TestApp
            {
                public class ObservableClass : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                }
            }
            """;

        var compilation = CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var classDecl = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .First();

        var classSymbol = (INamedTypeSymbol)model.GetDeclaredSymbol(classDecl)!;
        var inpc = compilation.GetTypeByMetadataName("System.ComponentModel.INotifyPropertyChanged")!;

        var result = AnalyzerHelpers.ImplementsInterface(classSymbol, inpc);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that InheritsFrom returns false when the type does not inherit from the base type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InheritsFrom_DoesNotInherit_ReturnsFalse()
    {
        var source = """
            namespace TestApp
            {
                public class BaseClass { }
                public class UnrelatedClass { }
            }
            """;

        var compilation = CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var classDecls = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .ToArray();

        var baseSymbol = (INamedTypeSymbol)model.GetDeclaredSymbol(classDecls[0])!;
        var unrelatedSymbol = (INamedTypeSymbol)model.GetDeclaredSymbol(classDecls[1])!;

        var result = AnalyzerHelpers.InheritsFrom(unrelatedSymbol, baseSymbol);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that InheritsFrom returns true when the type inherits from the base type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InheritsFrom_Inherits_ReturnsTrue()
    {
        var source = """
            namespace TestApp
            {
                public class BaseClass { }
                public class DerivedClass : BaseClass { }
            }
            """;

        var compilation = CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var classDecls = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .ToArray();

        var baseSymbol = (INamedTypeSymbol)model.GetDeclaredSymbol(classDecls[0])!;
        var derivedSymbol = (INamedTypeSymbol)model.GetDeclaredSymbol(classDecls[1])!;

        var result = AnalyzerHelpers.InheritsFrom(derivedSymbol, baseSymbol);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that LacksObservableMechanism returns false for a non-generic method
    /// (no type arguments to extract).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LacksObservableMechanism_NonGenericMethod_ReturnsFalse()
    {
        var (methodSymbol, compilation) = GetNonGenericMethodSymbol();

        var result = AnalyzerHelpers.LacksObservableMechanism(methodSymbol, compilation, out var sourceType);

        await Assert.That(result).IsFalse();
        await Assert.That(sourceType).IsNull();
    }

    /// <summary>
    /// Verifies that LacksObservableMechanism returns false for a generic method
    /// whose first type argument implements INPC.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LacksObservableMechanism_GenericWithINPC_ReturnsFalse()
    {
        var source = """
            using System.ComponentModel;
            namespace TestApp
            {
                public class MyVm : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                }

                public class Caller
                {
                    public void M<T>(T obj) where T : class { }
                    public void Go() { M<MyVm>(new MyVm()); }
                }
            }
            """;

        var (methodSymbol, compilation) = GetInvocationMethodSymbol(source);

        var result = AnalyzerHelpers.LacksObservableMechanism(methodSymbol, compilation, out var sourceType);

        await Assert.That(result).IsFalse();
        await Assert.That(sourceType).IsNotNull();
    }

    /// <summary>
    /// Verifies that LacksObservableMechanism returns true for a generic method
    /// whose first type argument has no observable mechanism.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LacksObservableMechanism_GenericWithPlainClass_ReturnsTrue()
    {
        var source = """
            namespace TestApp
            {
                public class PlainVm { public string Name { get; set; } = ""; }

                public class Caller
                {
                    public void M<T>(T obj) where T : class { }
                    public void Go() { M<PlainVm>(new PlainVm()); }
                }
            }
            """;

        var (methodSymbol, compilation) = GetInvocationMethodSymbol(source);

        var result = AnalyzerHelpers.LacksObservableMechanism(methodSymbol, compilation, out var sourceType);

        await Assert.That(result).IsTrue();
        await Assert.That(sourceType!.Name).IsEqualTo("PlainVm");
    }

    /// <summary>
    /// Verifies that LacksBeforeChangeSupport returns false for a non-generic method
    /// (no type arguments to extract).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LacksBeforeChangeSupport_NonGenericMethod_ReturnsFalse()
    {
        var (methodSymbol, compilation) = GetNonGenericMethodSymbol();

        var result = AnalyzerHelpers.LacksBeforeChangeSupport(methodSymbol, compilation, out var receiverType, out var mechanism);

        await Assert.That(result).IsFalse();
        await Assert.That(receiverType).IsNull();
        await Assert.That(mechanism).IsEqualTo(string.Empty);
    }

    /// <summary>
    /// Verifies that LacksBeforeChangeSupport returns true for a generic method
    /// whose first type argument implements only INPC (no INotifyPropertyChanging).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LacksBeforeChangeSupport_GenericINPCOnly_ReturnsTrue()
    {
        var source = """
            using System.ComponentModel;
            namespace TestApp
            {
                public class MyVm : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                }

                public class Caller
                {
                    public void M<T>(T obj) where T : class { }
                    public void Go() { M<MyVm>(new MyVm()); }
                }
            }
            """;

        var (methodSymbol, compilation) = GetInvocationMethodSymbol(source);

        var result = AnalyzerHelpers.LacksBeforeChangeSupport(methodSymbol, compilation, out var receiverType, out var mechanism);

        await Assert.That(result).IsTrue();
        await Assert.That(receiverType!.Name).IsEqualTo("MyVm");
        await Assert.That(mechanism).Contains("INotifyPropertyChanged");
    }

    /// <summary>
    /// Verifies that ImplementsDataErrorInfo returns false for a non-generic method
    /// (no type arguments to extract).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ImplementsDataErrorInfo_NonGenericMethod_ReturnsFalse()
    {
        var (methodSymbol, compilation) = GetNonGenericMethodSymbol();

        var result = AnalyzerHelpers.ImplementsDataErrorInfo(methodSymbol, compilation, out var sourceType);

        await Assert.That(result).IsFalse();
        await Assert.That(sourceType).IsNull();
    }

    /// <summary>
    /// Verifies that ImplementsDataErrorInfo returns true for a generic method
    /// whose first type argument implements INotifyDataErrorInfo.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ImplementsDataErrorInfo_GenericWithDataErrorInfo_ReturnsTrue()
    {
        var source = """
            using System;
            using System.Collections;
            using System.ComponentModel;
            namespace TestApp
            {
                public class MyVm : INotifyDataErrorInfo
                {
                    public bool HasErrors { get { return false; } }
                    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
                    public IEnumerable GetErrors(string propertyName) { return new object[0]; }
                }

                public class Caller
                {
                    public void M<T>(T obj) where T : class { }
                    public void Go() { M<MyVm>(new MyVm()); }
                }
            }
            """;

        var (methodSymbol, compilation) = GetInvocationMethodSymbol(source);

        var result = AnalyzerHelpers.ImplementsDataErrorInfo(methodSymbol, compilation, out var sourceType);

        await Assert.That(result).IsTrue();
        await Assert.That(sourceType!.Name).IsEqualTo("MyVm");
    }

    /// <summary>
    /// Verifies that ImplementsDataErrorInfo returns false for a generic method
    /// whose first type argument does not implement INotifyDataErrorInfo.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ImplementsDataErrorInfo_GenericWithoutDataErrorInfo_ReturnsFalse()
    {
        var source = """
            using System.ComponentModel;
            namespace TestApp
            {
                public class MyVm : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                }

                public class Caller
                {
                    public void M<T>(T obj) where T : class { }
                    public void Go() { M<MyVm>(new MyVm()); }
                }
            }
            """;

        var (methodSymbol, compilation) = GetInvocationMethodSymbol(source);

        var result = AnalyzerHelpers.ImplementsDataErrorInfo(methodSymbol, compilation, out var sourceType);

        await Assert.That(result).IsFalse();
        await Assert.That(sourceType!.Name).IsEqualTo("MyVm");
    }

    /// <summary>
    /// Verifies that ImplementsDataErrorInfo returns false when the compilation
    /// does not contain the INotifyDataErrorInfo type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ImplementsDataErrorInfo_TypeNotInCompilation_ReturnsFalse()
    {
        var source = """
            namespace TestApp
            {
                public class MyVm { public string Name { get; set; } }

                public class Caller
                {
                    public void M<T>(T obj) where T : class { }
                    public void Go() { M<MyVm>(new MyVm()); }
                }
            }
            """;

        var (methodSymbol, compilation) = GetInvocationMethodSymbolMinimal(source);

        var result = AnalyzerHelpers.ImplementsDataErrorInfo(methodSymbol, compilation, out var sourceType);

        await Assert.That(result).IsFalse();
        await Assert.That(sourceType!.Name).IsEqualTo("MyVm");
    }

    /// <summary>
    /// Verifies that HasObservableMechanism returns true when the type implements IReactiveObject.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasObservableMechanism_IReactiveObject_ReturnsTrue()
    {
        var source = """
            using ReactiveUI;
            namespace TestApp
            {
                public class MyVm : IReactiveObject
                {
                    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
                    public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
                    public void RaisePropertyChanging(System.ComponentModel.PropertyChangingEventArgs args) { }
                    public void RaisePropertyChanged(System.ComponentModel.PropertyChangedEventArgs args) { }
                }
            }
            """;

        var compilation = CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyVm");

        var result = TypeAnalyzer.HasObservableMechanism(typeSymbol, compilation);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that HasObservableMechanism handles the case where IReactiveObject
    /// is not in the compilation (iro == null short-circuit).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasObservableMechanism_IReactiveObjectNotInCompilation_ReturnsFalseForInterfacedType()
    {
        // Use a minimal compilation that does NOT reference ReactiveUI (no IReactiveObject)
        var (_, compilation) = GetInvocationMethodSymbolMinimal(
            """
            namespace TestApp
            {
                public interface IMyInterface { }
                public class MyVm : IMyInterface
                {
                    public string Name { get; set; }
                }

                public class Caller
                {
                    public void M<T>(T obj) where T : class { }
                    public void Go() { M<MyVm>(new MyVm()); }
                }
            }
            """);

        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var classDecl = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .First(c => c.Identifier.Text == "MyVm");

        var typeSymbol = (INamedTypeSymbol)model.GetDeclaredSymbol(classDecl)!;

        var result = TypeAnalyzer.HasObservableMechanism(typeSymbol, compilation);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Gets a named type symbol from a compilation by class name.
    /// </summary>
    /// <param name="compilation">The compilation.</param>
    /// <param name="className">The class name to find.</param>
    /// <returns>The named type symbol.</returns>
    private static INamedTypeSymbol GetNamedTypeSymbol(CSharpCompilation compilation, string className)
    {
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var classDecl = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .First(c => c.Identifier.Text == className);

        return (INamedTypeSymbol)model.GetDeclaredSymbol(classDecl)!;
    }

    /// <summary>
    /// Creates a compilation from the specified source code.
    /// </summary>
    /// <param name="source">The source code.</param>
    /// <returns>A CSharpCompilation.</returns>
    private static CSharpCompilation CreateCompilation(string source) =>
        AnalyzerTestHelper.CreateCompilation(source);

    /// <summary>
    /// Gets a non-generic method symbol and its compilation.
    /// </summary>
    /// <returns>The method symbol and compilation.</returns>
    private static (IMethodSymbol MethodSymbol, Compilation Compilation) GetNonGenericMethodSymbol()
    {
        var source = """
            namespace TestApp
            {
                public class MyClass
                {
                    public void NonGenericMethod() { }
                }
            }
            """;

        var compilation = CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var methodDecl = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        return (model.GetDeclaredSymbol(methodDecl)!, compilation);
    }

    /// <summary>
    /// Gets the resolved method symbol from the first invocation in the source.
    /// </summary>
    /// <param name="source">The source code containing an invocation.</param>
    /// <returns>The method symbol and compilation.</returns>
    private static (IMethodSymbol MethodSymbol, Compilation Compilation) GetInvocationMethodSymbol(string source)
    {
        var compilation = CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var invocation = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .First();

        return ((IMethodSymbol)model.GetSymbolInfo(invocation).Symbol!, compilation);
    }

    /// <summary>
    /// Gets the resolved method symbol from the first invocation using a minimal compilation
    /// that only includes core references (no INotifyDataErrorInfo).
    /// </summary>
    /// <param name="source">The source code containing an invocation.</param>
    /// <returns>The method symbol and compilation.</returns>
    private static (IMethodSymbol MethodSymbol, Compilation Compilation) GetInvocationMethodSymbolMinimal(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Only add the core runtime reference — no System.ComponentModel assemblies
        var references = new List<MetadataReference>();
        var addedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddReference(System.Reflection.Assembly assembly)
        {
            if (!assembly.IsDynamic && addedPaths.Add(assembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        AddReference(typeof(object).Assembly);

        // Add System.Runtime by name
        var systemRuntime = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "System.Runtime");
        if (systemRuntime != null)
        {
            AddReference(systemRuntime);
        }

        var compilation = CSharpCompilation.Create(
            "MinimalTestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var invocation = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .First();

        return ((IMethodSymbol)model.GetSymbolInfo(invocation).Symbol!, compilation);
    }
}

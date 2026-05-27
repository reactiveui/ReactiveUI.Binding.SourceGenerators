// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ReactiveUI.Binding.Helpers;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>
/// Analyzes types used in binding invocations to detect types with no observable properties.
/// Reports RXUIBIND002.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TypeAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticWarnings.NoObservableProperties];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        ArgumentExceptionHelper.ThrowIfNull(context);
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    /// <summary>
    /// Analyzes a method invocation operation to determine whether the source type
    /// has any observable property notification mechanism.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    internal static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocationOp = (IInvocationOperation)context.Operation;

        var methodSymbol = invocationOp.TargetMethod;
        if (!AnalyzerHelpers.IsBindingExtensionMethod(methodSymbol))
        {
            return;
        }

        // Check if the source type lacks any observable mechanism
        if (!AnalyzerHelpers.LacksObservableMechanism(methodSymbol, context.Compilation, out var sourceType))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticWarnings.NoObservableProperties,
                invocationOp.Syntax.GetLocation(),
                sourceType!.Name));
    }

    /// <summary>
    /// Determines whether a type has any observable property notification mechanism,
    /// including INPC, IReactiveObject, WPF/WinUI DependencyObject, NSObject (KVO),
    /// WinForms Component, or Android View.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="compilation">The current compilation for type resolution.</param>
    /// <returns><c>true</c> if the type supports property observation; otherwise, <c>false</c>.</returns>
    internal static bool HasObservableMechanism(INamedTypeSymbol typeSymbol, Compilation compilation) =>
        ImplementsObservableInterface(typeSymbol, compilation)
        || InheritsObservableBaseType(typeSymbol, compilation);

    /// <summary>
    /// Determines whether the type implements one of the observable notification interfaces
    /// (INotifyPropertyChanged or IReactiveObject).
    /// </summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="compilation">The current compilation for type resolution.</param>
    /// <returns><c>true</c> if an observable interface is implemented; otherwise, <c>false</c>.</returns>
    private static bool ImplementsObservableInterface(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        INamedTypeSymbol?[] interfaces =
        [
            compilation.GetTypeByMetadataName(Constants.INotifyPropertyChangedMetadataName),
            compilation.GetTypeByMetadataName(Constants.IReactiveObjectMetadataName),
        ];

        var allInterfaces = typeSymbol.AllInterfaces;
        for (var i = 0; i < allInterfaces.Length; i++)
        {
            if (MatchesAny(allInterfaces[i], interfaces))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the type inherits from a platform-specific observable base type
    /// (WPF/WinUI DependencyObject, NSObject, WinForms Component, or Android View).
    /// </summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="compilation">The current compilation for type resolution.</param>
    /// <returns><c>true</c> if an observable base type is found; otherwise, <c>false</c>.</returns>
    private static bool InheritsObservableBaseType(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        INamedTypeSymbol?[] baseTypes =
        [
            compilation.GetTypeByMetadataName(Constants.WpfDependencyObjectMetadataName),
            compilation.GetTypeByMetadataName(Constants.WinUIDependencyObjectMetadataName),
            compilation.GetTypeByMetadataName(Constants.NSObjectMetadataName),
            compilation.GetTypeByMetadataName(Constants.WinFormsComponentMetadataName),
            compilation.GetTypeByMetadataName(Constants.AndroidViewMetadataName),
        ];

        var current = typeSymbol.BaseType;
        while (current != null)
        {
            if (MatchesAny(current, baseTypes))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Determines whether <paramref name="symbol"/> equals any of the (possibly null) candidate symbols.
    /// </summary>
    /// <param name="symbol">The symbol to compare.</param>
    /// <param name="candidates">The candidate symbols; null entries are skipped.</param>
    /// <returns><c>true</c> if <paramref name="symbol"/> matches a non-null candidate; otherwise, <c>false</c>.</returns>
    private static bool MatchesAny(ISymbol symbol, INamedTypeSymbol?[] candidates)
    {
        for (var i = 0; i < candidates.Length; i++)
        {
            if (candidates[i] != null && SymbolEqualityComparer.Default.Equals(symbol, candidates[i]))
            {
                return true;
            }
        }

        return false;
    }
}

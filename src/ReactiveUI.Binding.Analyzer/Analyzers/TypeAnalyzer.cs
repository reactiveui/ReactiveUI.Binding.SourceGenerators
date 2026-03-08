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
        if (AnalyzerHelpers.LacksObservableMechanism(methodSymbol, context.Compilation, out var sourceType))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticWarnings.NoObservableProperties,
                    invocationOp.Syntax.GetLocation(),
                    sourceType!.Name));
        }
    }

    /// <summary>
    /// Determines whether a type has any observable property notification mechanism,
    /// including INPC, IReactiveObject, WPF/WinUI DependencyObject, NSObject (KVO),
    /// WinForms Component, or Android View.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="compilation">The current compilation for type resolution.</param>
    /// <returns><c>true</c> if the type supports property observation; otherwise, <c>false</c>.</returns>
    internal static bool HasObservableMechanism(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        // Check interfaces
        var inpc = compilation.GetTypeByMetadataName(Constants.INotifyPropertyChangedMetadataName);
        var iro = compilation.GetTypeByMetadataName(Constants.IReactiveObjectMetadataName);

        var allInterfaces = typeSymbol.AllInterfaces;
        for (var i = 0; i < allInterfaces.Length; i++)
        {
            if (inpc != null && SymbolEqualityComparer.Default.Equals(allInterfaces[i], inpc))
            {
                return true;
            }

            if (iro != null && SymbolEqualityComparer.Default.Equals(allInterfaces[i], iro))
            {
                return true;
            }
        }

        // Check base types for platform-specific observable mechanisms
        var wpfDO = compilation.GetTypeByMetadataName(Constants.WpfDependencyObjectMetadataName);
        var winuiDO = compilation.GetTypeByMetadataName(Constants.WinUIDependencyObjectMetadataName);
        var nsObject = compilation.GetTypeByMetadataName(Constants.NSObjectMetadataName);
        var winformsComp = compilation.GetTypeByMetadataName(Constants.WinFormsComponentMetadataName);
        var androidView = compilation.GetTypeByMetadataName(Constants.AndroidViewMetadataName);

        var current = typeSymbol.BaseType;
        while (current != null)
        {
            if (wpfDO != null && SymbolEqualityComparer.Default.Equals(current, wpfDO))
            {
                return true;
            }

            if (winuiDO != null && SymbolEqualityComparer.Default.Equals(current, winuiDO))
            {
                return true;
            }

            if (nsObject != null && SymbolEqualityComparer.Default.Equals(current, nsObject))
            {
                return true;
            }

            if (winformsComp != null && SymbolEqualityComparer.Default.Equals(current, winformsComp))
            {
                return true;
            }

            if (androidView != null && SymbolEqualityComparer.Default.Equals(current, androidView))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }
}

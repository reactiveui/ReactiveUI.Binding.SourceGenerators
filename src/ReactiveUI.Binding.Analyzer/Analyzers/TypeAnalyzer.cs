// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

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
        ImmutableArray.Create(DiagnosticWarnings.NoObservableProperties);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new System.ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    internal static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation invocationOp)
        {
            return;
        }

        var methodSymbol = invocationOp.TargetMethod;
        if (!AnalyzerHelpers.IsBindingExtensionMethod(methodSymbol))
        {
            return;
        }

        // Get the source type (first type argument)
        var sourceType = methodSymbol.TypeArguments.Length > 0
            ? methodSymbol.TypeArguments[0] as INamedTypeSymbol
            : null;

        if (sourceType == null)
        {
            return;
        }

        // Check if the type has any observable mechanism
        if (HasObservableMechanism(sourceType, context.Compilation))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticWarnings.NoObservableProperties,
                invocationOp.Syntax.GetLocation(),
                sourceType.Name));
    }

    internal static bool HasObservableMechanism(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        // Check interfaces
        var inpc = compilation.GetTypeByMetadataName(Constants.INotifyPropertyChangedMetadataName);
        var iro = compilation.GetTypeByMetadataName(Constants.IReactiveObjectMetadataName);

        var allInterfaces = typeSymbol.AllInterfaces;
        for (int i = 0; i < allInterfaces.Length; i++)
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

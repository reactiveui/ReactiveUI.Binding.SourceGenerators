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

/// <summary>Analyzes types used in binding invocations to detect types with no observable properties. Reports RXUIBIND002.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TypeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostics this analyzer reports.</summary>
    private static readonly ImmutableArray<DiagnosticDescriptor> ReportedDiagnostics =
        new[] { DiagnosticWarnings.NoObservableProperties }.ToImmutableArray();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ReportedDiagnostics;

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        ArgumentExceptionHelper.ThrowIfNull(context);
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(static operationContext => AnalyzeInvocation(in operationContext), OperationKind.Invocation);
    }

    /// <summary>
    /// Analyzes a method invocation operation to determine whether the source type
    /// has any observable property notification mechanism.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    internal static void AnalyzeInvocation(in OperationAnalysisContext context)
    {
        var invocationOp = (IInvocationOperation)context.Operation;

        var methodSymbol = invocationOp.TargetMethod;
        if (!AnalyzerHelpers.IsBindingExtensionMethod(methodSymbol))
        {
            return;
        }

        // The check reads the first type argument, which most APIs name the observed object with.
        // BindTo names the value type of a stream the caller already built, and nothing about that type is
        // ever observed, so asking whether it notifies has no answer worth reporting.
        if (methodSymbol.Name == Constants.BindToMethodName)
        {
            return;
        }

        // InvokeCommand names that stream's value type first as well; the object it observes is the one holding
        // the command, which it names second.
        var observedTypeArgument = methodSymbol.Name == Constants.InvokeCommandMethodName ? 1 : 0;

        // Check if the source type lacks any observable mechanism
        if (!AnalyzerHelpers.LacksObservableMechanism(
            methodSymbol,
            context.Compilation,
            observedTypeArgument,
            out var sourceType))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticWarnings.NoObservableProperties,
                invocationOp.Syntax.GetLocation(),
                sourceType!.Name));
    }
}

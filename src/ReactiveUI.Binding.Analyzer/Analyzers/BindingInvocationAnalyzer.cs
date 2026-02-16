// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>
/// Analyzes WhenChanged/WhenChanging/BindOneWay/BindTwoWay invocations for common issues.
/// Reports RXUIBIND001, RXUIBIND003, RXUIBIND004, RXUIBIND005.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BindingInvocationAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticWarnings.NonInlineLambda,
            DiagnosticWarnings.PrivateMember,
            DiagnosticWarnings.NoBeforeChangeSupport,
            DiagnosticWarnings.ValidationNotGenerated,
            DiagnosticWarnings.UnsupportedPathSegment);

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

        string methodName = methodSymbol.Name;
        var arguments = invocationOp.Arguments;

        // Check RXUIBIND001: Non-inline lambda
        CheckNonInlineLambda(context, arguments, methodName);

        // Check RXUIBIND003: Private/protected member access
        CheckPrivateMember(context, arguments);

        // Check RXUIBIND006: Unsupported path segments (indexer, field, method call)
        CheckUnsupportedPathSegment(context, arguments);

        // Check RXUIBIND004: Before-change support
        if (methodName == Constants.WhenChangingMethodName)
        {
            CheckBeforeChangeSupport(context, invocationOp, methodSymbol);
        }

        // Check RXUIBIND005: Validation support
        if (methodName is Constants.BindOneWayMethodName or Constants.BindTwoWayMethodName)
        {
            CheckValidationSupport(context, invocationOp);
        }
    }

    internal static void CheckNonInlineLambda(
        OperationAnalysisContext context,
        ImmutableArray<IArgumentOperation> arguments,
        string methodName)
    {
        // Find the Expression<Func<...>> arguments
        for (int i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];
            if (arg.Value.Syntax is not ExpressionSyntax expr)
            {
                continue;
            }

            // Check if this is an Expression<Func<...>> parameter
            var parameterType = arg.Parameter?.Type;
            if (parameterType == null)
            {
                continue;
            }

            string typeDisplay = parameterType.ToDisplayString();
            if (!typeDisplay.StartsWith("System.Linq.Expressions.Expression<", System.StringComparison.Ordinal))
            {
                continue;
            }

            // Must be an inline lambda
            if (!AnalyzerHelpers.IsInlineLambda(expr))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(DiagnosticWarnings.NonInlineLambda, expr.GetLocation()));
            }
        }
    }

    internal static void CheckPrivateMember(
        OperationAnalysisContext context,
        ImmutableArray<IArgumentOperation> arguments)
    {
        for (int i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];
            if (arg.Value.Syntax is not LambdaExpressionSyntax lambda)
            {
                continue;
            }

            // Walk the lambda body looking for member accesses
            ExpressionSyntax? body = lambda switch
            {
                SimpleLambdaExpressionSyntax simple => simple.Body as ExpressionSyntax,
                ParenthesizedLambdaExpressionSyntax parens => parens.Body as ExpressionSyntax,
                _ => null
            };

            if (body == null)
            {
                continue;
            }

            var current = body;
            while (current is MemberAccessExpressionSyntax memberAccess)
            {
                var memberSymbol = context.Operation.SemanticModel?.GetSymbolInfo(memberAccess).Symbol;
                if (memberSymbol != null
                    && memberSymbol.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticWarnings.PrivateMember,
                            memberAccess.Name.GetLocation(),
                            memberSymbol.Name));
                    break;
                }

                current = memberAccess.Expression;
            }
        }
    }

    internal static void CheckBeforeChangeSupport(
        OperationAnalysisContext context,
        IInvocationOperation invocationOp,
        IMethodSymbol methodSymbol)
    {
        // Get the receiver type
        var receiverType = methodSymbol.TypeArguments.Length > 0
            ? methodSymbol.TypeArguments[0] as INamedTypeSymbol
            : null;

        if (receiverType == null)
        {
            return;
        }

        var compilation = context.Compilation;
        if (!AnalyzerHelpers.HasBeforeChangeSupport(receiverType, compilation, out string mechanism))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticWarnings.NoBeforeChangeSupport,
                    invocationOp.Syntax.GetLocation(),
                    receiverType.Name,
                    mechanism));
        }
    }

    internal static void CheckValidationSupport(
        OperationAnalysisContext context,
        IInvocationOperation invocationOp)
    {
        var methodSymbol = invocationOp.TargetMethod;

        // Get the source type (first type argument)
        var sourceType = methodSymbol.TypeArguments.Length > 0
            ? methodSymbol.TypeArguments[0] as INamedTypeSymbol
            : null;

        if (sourceType == null)
        {
            return;
        }

        // Check if source implements INotifyDataErrorInfo
        var dataErrorInfo = context.Compilation.GetTypeByMetadataName(Constants.INotifyDataErrorInfoMetadataName);
        if (dataErrorInfo == null)
        {
            return;
        }

        var allInterfaces = sourceType.AllInterfaces;
        for (int i = 0; i < allInterfaces.Length; i++)
        {
            if (SymbolEqualityComparer.Default.Equals(allInterfaces[i], dataErrorInfo))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticWarnings.ValidationNotGenerated,
                        invocationOp.Syntax.GetLocation(),
                        sourceType.Name));
                return;
            }
        }
    }

    internal static void CheckUnsupportedPathSegment(
        OperationAnalysisContext context,
        ImmutableArray<IArgumentOperation> arguments)
    {
        for (int i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];

            // Only check Expression<Func<...>> parameters
            var parameterType = arg.Parameter?.Type;
            if (parameterType == null)
            {
                continue;
            }

            string typeDisplay = parameterType.ToDisplayString();
            if (!typeDisplay.StartsWith("System.Linq.Expressions.Expression<", System.StringComparison.Ordinal))
            {
                continue;
            }

            if (arg.Value.Syntax is not LambdaExpressionSyntax lambda)
            {
                continue;
            }

            ExpressionSyntax? body = lambda switch
            {
                SimpleLambdaExpressionSyntax simple => simple.Body as ExpressionSyntax,
                ParenthesizedLambdaExpressionSyntax parens => parens.Body as ExpressionSyntax,
                _ => null
            };

            if (body == null)
            {
                continue;
            }

            WalkForUnsupportedSegments(context, body);
        }
    }

    internal static void WalkForUnsupportedSegments(
        OperationAnalysisContext context,
        ExpressionSyntax expression)
    {
        var current = expression;
        while (current != null)
        {
            if (current is InvocationExpressionSyntax invocation)
            {
                // Method call in path: x => x.GetValue().Name
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticWarnings.UnsupportedPathSegment,
                        invocation.GetLocation(),
                        invocation.ToString()));
                return;
            }

            if (current is ElementAccessExpressionSyntax elementAccess)
            {
                // Indexer in path: x => x.Items[0].Name
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticWarnings.UnsupportedPathSegment,
                        elementAccess.GetLocation(),
                        elementAccess.ToString()));
                return;
            }

            if (current is MemberAccessExpressionSyntax memberAccess)
            {
                // Check if the member is a field
                var memberSymbol = context.Operation.SemanticModel?.GetSymbolInfo(memberAccess).Symbol;
                if (memberSymbol is IFieldSymbol fieldSymbol && !fieldSymbol.IsConst)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticWarnings.UnsupportedPathSegment,
                            memberAccess.Name.GetLocation(),
                            memberSymbol.Name));
                    return;
                }

                current = memberAccess.Expression;
                continue;
            }

            // Lambda parameter or other terminal — stop walking
            break;
        }
    }
}

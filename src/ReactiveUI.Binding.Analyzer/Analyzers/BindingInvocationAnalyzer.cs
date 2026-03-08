// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

using ReactiveUI.Binding.Helpers;
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
    [
        DiagnosticWarnings.NonInlineLambda,
            DiagnosticWarnings.PrivateMember,
            DiagnosticWarnings.NoBeforeChangeSupport,
            DiagnosticWarnings.ValidationNotGenerated,
            DiagnosticWarnings.UnsupportedPathSegment,
            DiagnosticWarnings.NoBindableEvent,
            DiagnosticWarnings.InvalidInteractionType
    ];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        ArgumentExceptionHelper.ThrowIfNull(context);
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    /// <summary>
    /// Analyzes a method invocation operation for binding-related diagnostics.
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

        var methodName = methodSymbol.Name;
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

        // Check RXUIBIND008: Invalid interaction type
        if (methodName == Constants.BindInteractionMethodName)
        {
            CheckInteractionType(context, invocationOp);
        }

        // Check RXUIBIND007: No bindable event on control
        if (methodName == Constants.BindCommandMethodName)
        {
            CheckBindableEvent(context, invocationOp);
        }
    }

    /// <summary>
    /// Checks for RXUIBIND001: Expression arguments that are not inline lambdas.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="arguments">The invocation arguments to inspect.</param>
    /// <param name="methodName">The name of the method being invoked.</param>
    internal static void CheckNonInlineLambda(
        OperationAnalysisContext context,
        ImmutableArray<IArgumentOperation> arguments,
        string methodName)
    {
        // Find the Expression<Func<...>> arguments
        for (var i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];
            if (arg.Value.Syntax is not ExpressionSyntax expr)
            {
                continue;
            }

            // Check if this is an Expression<Func<...>> parameter
            var parameterType = arg.Parameter!.Type;

            var typeDisplay = parameterType.ToDisplayString();
            if (!typeDisplay.StartsWith("System.Linq.Expressions.Expression<", StringComparison.Ordinal))
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

    /// <summary>
    /// Checks for RXUIBIND003: Lambda expressions that access private or protected members.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="arguments">The invocation arguments to inspect.</param>
    internal static void CheckPrivateMember(
        OperationAnalysisContext context,
        ImmutableArray<IArgumentOperation> arguments)
    {
        for (var i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];
            if (arg.Value.Syntax is not LambdaExpressionSyntax lambda)
            {
                continue;
            }

            // Walk the lambda body looking for member accesses
            var body = GetLambdaBody(lambda);
            if (body == null)
            {
                continue;
            }

            var current = body;
            while (current is MemberAccessExpressionSyntax memberAccess)
            {
                var memberSymbol = context.Operation.SemanticModel!.GetSymbolInfo(memberAccess).Symbol;
                if (memberSymbol is { DeclaredAccessibility: Accessibility.Private or Accessibility.Protected })
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

    /// <summary>
    /// Checks for RXUIBIND004: WhenChanging invocations on types that do not support before-change notifications.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="invocationOp">The invocation operation being analyzed.</param>
    /// <param name="methodSymbol">The method symbol of the invocation target.</param>
    internal static void CheckBeforeChangeSupport(
        OperationAnalysisContext context,
        IInvocationOperation invocationOp,
        IMethodSymbol methodSymbol)
    {
        if (AnalyzerHelpers.LacksBeforeChangeSupport(methodSymbol, context.Compilation, out var receiverType, out var mechanism))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticWarnings.NoBeforeChangeSupport,
                    invocationOp.Syntax.GetLocation(),
                    receiverType!.Name,
                    mechanism));
        }
    }

    /// <summary>
    /// Checks for RXUIBIND005: Binding invocations where the source type implements
    /// <see cref="System.ComponentModel.INotifyDataErrorInfo"/>, which requires the runtime engine for validation binding.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="invocationOp">The invocation operation being analyzed.</param>
    internal static void CheckValidationSupport(
        OperationAnalysisContext context,
        IInvocationOperation invocationOp)
    {
        if (AnalyzerHelpers.ImplementsDataErrorInfo(
                invocationOp.TargetMethod, context.Compilation, out var sourceType))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticWarnings.ValidationNotGenerated,
                    invocationOp.Syntax.GetLocation(),
                    sourceType!.Name));
        }
    }

    /// <summary>
    /// Checks for RXUIBIND008: BindInteraction invocations where the property type does not implement
    /// the <c>IInteraction&lt;TInput, TOutput&gt;</c> interface.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="invocationOp">The invocation operation being analyzed.</param>
    internal static void CheckInteractionType(
        OperationAnalysisContext context,
        IInvocationOperation invocationOp)
    {
        // Find the propertyName argument (the Expression<Func<TViewModel, IInteraction<...>>> parameter)
        var arguments = invocationOp.Arguments;
        for (var i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];
            if (arg.Parameter!.Name != "propertyName")
            {
                continue;
            }

            if (arg.Value.Syntax is not LambdaExpressionSyntax lambda)
            {
                break;
            }

            // Get the lambda body return type
            var body = GetLambdaBody(lambda);

            if (body == null)
            {
                break;
            }

            var typeInfo = context.Operation.SemanticModel!.GetTypeInfo(body);
            if (typeInfo.Type == null)
            {
                break;
            }

            var interactionType = context.Compilation.GetTypeByMetadataName(Constants.IInteractionMetadataName);
            if (interactionType == null)
            {
                break;
            }

            // Check if the type implements IInteraction<,>
            var propertyType = typeInfo.Type;
            if (!ImplementsOpenGenericInterface(propertyType, interactionType))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticWarnings.InvalidInteractionType,
                        body.GetLocation(),
                        propertyType.ToDisplayString()));
            }

            break;
        }
    }

    /// <summary>
    /// Checks for RXUIBIND007: BindCommand invocations where the target control type
    /// does not expose a default bindable event (Click, TouchUpInside, MouseUp, or Pressed).
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="invocationOp">The invocation operation being analyzed.</param>
    internal static void CheckBindableEvent(
        OperationAnalysisContext context,
        IInvocationOperation invocationOp)
    {
        var methodSymbol = invocationOp.TargetMethod;
        var arguments = invocationOp.Arguments;

        // If toEvent is explicitly specified, skip this check
        for (var i = 0; i < arguments.Length; i++)
        {
            var toEventArg = arguments[i];
            if (toEventArg.Parameter!.Name != "toEvent")
            {
                continue;
            }

            if (!toEventArg.IsImplicit
                && toEventArg.Value.ConstantValue.HasValue
                && toEventArg.Value.ConstantValue.Value is string { Length: > 0 })
            {
                return;
            }
        }

        // Find the controlName argument to get the control type
        for (var i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];
            if (arg.Parameter!.Name != "controlName")
            {
                continue;
            }

            if (arg.Value.Syntax is not LambdaExpressionSyntax lambda)
            {
                break;
            }

            var body = GetLambdaBody(lambda);

            if (body == null)
            {
                break;
            }

            var typeInfo = context.Operation.SemanticModel!.GetTypeInfo(body);
            if (typeInfo.Type is not INamedTypeSymbol controlType)
            {
                break;
            }

            // Check for default events: Click, TouchUpInside, MouseUp, Pressed
            string[] defaultEvents = ["Click", "TouchUpInside", "MouseUp", "Pressed"];
            var foundEvent = false;
            for (var j = 0; j < defaultEvents.Length; j++)
            {
                var members = controlType.GetMembers(defaultEvents[j]);
                for (var k = 0; k < members.Length; k++)
                {
                    if (members[k] is IEventSymbol)
                    {
                        foundEvent = true;
                        break;
                    }
                }

                if (foundEvent)
                {
                    break;
                }
            }

            if (!foundEvent)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticWarnings.NoBindableEvent,
                        body.GetLocation(),
                        controlType.Name));
            }

            break;
        }
    }

    /// <summary>
    /// Determines whether a type implements a specific open generic interface.
    /// </summary>
    /// <param name="type">The type symbol to check.</param>
    /// <param name="openGenericInterface">The open generic interface to look for (e.g., <c>IInteraction&lt;,&gt;</c>).</param>
    /// <returns><c>true</c> if the type implements the specified open generic interface; otherwise, <c>false</c>.</returns>
    internal static bool ImplementsOpenGenericInterface(ITypeSymbol type, INamedTypeSymbol openGenericInterface)
    {
        if (type is INamedTypeSymbol { IsGenericType: true } named
            && SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, openGenericInterface))
        {
            return true;
        }

        var allInterfaces = type.AllInterfaces;
        for (var i = 0; i < allInterfaces.Length; i++)
        {
            if (allInterfaces[i].IsGenericType
                && SymbolEqualityComparer.Default.Equals(allInterfaces[i].ConstructedFrom, openGenericInterface))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks for RXUIBIND006: Lambda expressions containing unsupported path segments
    /// such as indexers, field accesses, or method calls.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="arguments">The invocation arguments to inspect.</param>
    internal static void CheckUnsupportedPathSegment(
        OperationAnalysisContext context,
        ImmutableArray<IArgumentOperation> arguments)
    {
        for (var i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];

            // Only check Expression<Func<...>> parameters
            var parameterType = arg.Parameter!.Type;

            var typeDisplay = parameterType.ToDisplayString();
            if (!typeDisplay.StartsWith("System.Linq.Expressions.Expression<", StringComparison.Ordinal))
            {
                continue;
            }

            if (arg.Value.Syntax is not LambdaExpressionSyntax lambda)
            {
                continue;
            }

            var body = GetLambdaBody(lambda);

            if (body == null)
            {
                continue;
            }

            WalkForUnsupportedSegments(context, body);
        }
    }

    /// <summary>
    /// Walks a member access chain looking for unsupported path segments (method calls, indexers, or fields).
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="expression">The expression to walk.</param>
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
                var memberSymbol = context.Operation.SemanticModel!.GetSymbolInfo(memberAccess).Symbol;
                if (memberSymbol is IFieldSymbol { IsConst: false })
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

    /// <summary>
    /// Extracts the body expression from a lambda expression syntax node.
    /// Only <see cref="SimpleLambdaExpressionSyntax"/> and <see cref="ParenthesizedLambdaExpressionSyntax"/>
    /// exist in Roslyn's C# syntax model.
    /// </summary>
    /// <param name="lambda">The lambda expression syntax node to extract the body from.</param>
    /// <returns>
    /// The body as an <see cref="ExpressionSyntax"/>, or <c>null</c> if the lambda body is a block statement.
    /// </returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static ExpressionSyntax? GetLambdaBody(LambdaExpressionSyntax lambda)
    {
        if (lambda is SimpleLambdaExpressionSyntax simple)
        {
            return simple.Body as ExpressionSyntax;
        }

        var parenthesized = (ParenthesizedLambdaExpressionSyntax)lambda;
        return parenthesized.Body as ExpressionSyntax;
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>
/// Extracts BindInteractionInvocationInfo from BindInteraction invocations.
/// </summary>
internal static class InteractionExtractor
{
    /// <summary>
    /// The minimum number of arguments a BindInteraction invocation must have
    /// (view model, property name, handler).
    /// </summary>
    private const int MinimumBindInteractionArgumentCount = 3;

    /// <summary>
    /// Pipeline B transform: extracts BindInteractionInvocationInfo from a BindInteraction invocation.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A BindInteractionInvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static BindInteractionInvocationInfo? ExtractBindInteractionInvocation(
        GeneratorSyntaxContext context,
        CancellationToken ct)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

        var semanticModel = context.SemanticModel;
        var methodSymbol = ExtractorValidation.ExtractMethodSymbol(semanticModel.GetSymbolInfo(invocation, ct));
        if (methodSymbol == null)
        {
            return null;
        }

        // Verify this is our stub or generated method
        if (!ExtractorValidation.IsRecognizedExtensionClass(methodSymbol.ContainingType.Name))
        {
            return null;
        }

        var args = invocation.ArgumentList.Arguments;
        InvalidOperationExceptionHelper.EnsureMinimumArguments(args.Count, MinimumBindInteractionArgumentCount);

        // Extract the interaction property path from the second argument (propertyName)
        var propertyNameArg = args[1].Expression;
        var interactionPropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(propertyNameArg, semanticModel, ct);
        if (interactionPropertyPath == null)
        {
            return null;
        }

        // Resolve TInput, TOutput from the IInteraction<TInput, TOutput> type
        // Re-resolve via the semantic model to get the actual type arguments
        ResolveInteractionTypeArguments(
            propertyNameArg,
            semanticModel,
            ct,
            out var inputTypeFullName,
            out var outputTypeFullName);

        inputTypeFullName =
            InvalidOperationExceptionHelper.EnsureNotNullOrEmpty(inputTypeFullName, "interaction TInput type argument");
        outputTypeFullName =
            InvalidOperationExceptionHelper.EnsureNotNullOrEmpty(
                outputTypeFullName,
                "interaction TOutput type argument");

        // Determine handler type (Task vs Observable)
        var isTaskHandler = DetermineHandlerVariant(methodSymbol, out var dontCareTypeFullName);

        // Get types
        var viewTypeFullName = InvalidOperationExceptionHelper.EnsureNotNull(
            ExtractorValidation.GetTypeDisplayName(semanticModel.GetTypeInfo(memberAccess.Expression, ct).Type),
            "view type display name");

        var viewModelTypeFullName = InvalidOperationExceptionHelper.EnsureNotNull(
            ExtractorValidation.GetTypeDisplayName(semanticModel.GetTypeInfo(args[0].Expression, ct).Type),
            "view model type display name");

        var filePath = invocation.SyntaxTree.FilePath;
        var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        var expressionText = CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(propertyNameArg.ToString());

        return new(
            filePath,
            lineNumber,
            viewTypeFullName,
            viewModelTypeFullName,
            new(interactionPropertyPath),
            inputTypeFullName,
            outputTypeFullName,
            isTaskHandler,
            dontCareTypeFullName,
            Constants.BindInteractionMethodName,
            expressionText);
    }

    /// <summary>
    /// Resolves the <c>TInput</c> and <c>TOutput</c> type arguments of the targeted
    /// <c>IInteraction&lt;TInput, TOutput&gt;</c> property by re-resolving the lambda body.
    /// </summary>
    /// <param name="propertyNameArg">The property-name lambda expression.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="inputTypeFullName">The resolved fully qualified TInput type, or empty string.</param>
    /// <param name="outputTypeFullName">The resolved fully qualified TOutput type, or empty string.</param>
    private static void ResolveInteractionTypeArguments(
        ExpressionSyntax propertyNameArg,
        SemanticModel semanticModel,
        CancellationToken ct,
        out string inputTypeFullName,
        out string outputTypeFullName)
    {
        inputTypeFullName = string.Empty;
        outputTypeFullName = string.Empty;

        if (propertyNameArg is not LambdaExpressionSyntax lambda)
        {
            return;
        }

        var body = SyntaxHelpers.GetLambdaBody(lambda);
        if (body == null)
        {
            return;
        }

        body = SyntaxHelpers.UnwrapNullForgiving(body);
        if (body is not MemberAccessExpressionSyntax leafMemberAccess
            || semanticModel.GetSymbolInfo(leafMemberAccess, ct).Symbol is not IPropertySymbol propertySymbol
            || !SymbolHelpers.ExtractInteractionTypeArguments(propertySymbol.Type, out var input, out var output))
        {
            return;
        }

        inputTypeFullName = input;
        outputTypeFullName = output;
    }

    /// <summary>
    /// Determines whether the BindInteraction handler is a Task-based or Observable-based handler
    /// by inspecting the method's <c>handler</c> parameter type.
    /// </summary>
    /// <param name="methodSymbol">The resolved method symbol.</param>
    /// <param name="dontCareTypeFullName">
    /// For observable handlers, the fully qualified <c>TDontCare</c> type argument; otherwise null.
    /// </param>
    /// <returns><see langword="true"/> for a Task-based handler; otherwise <see langword="false"/>.</returns>
    private static bool DetermineHandlerVariant(IMethodSymbol methodSymbol, out string? dontCareTypeFullName)
    {
        dontCareTypeFullName = null;

        // The handler parameter is the 3rd argument (index 2).
        // Check the method's parameter type to determine handler variant.
        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            var param = methodSymbol.Parameters[i];

            // Observable handler: Func<IInteractionContext<TInput, TOutput>, IObservable<TDontCare>>
            if (param is { Name: "handler", Type: INamedTypeSymbol handlerType }
                && handlerType.TypeArguments.Length == 2
                && handlerType.TypeArguments[1] is INamedTypeSymbol returnType
                && SymbolHelpers.IsIObservable(returnType))
            {
                dontCareTypeFullName = returnType.TypeArguments[0]
                    .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                return false;
            }
        }

        return true;
    }
}

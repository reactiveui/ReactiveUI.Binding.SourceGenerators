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
    /// Pipeline B transform: extracts BindInteractionInvocationInfo from a BindInteraction invocation.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A BindInteractionInvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static BindInteractionInvocationInfo? ExtractBindInteractionInvocation(GeneratorSyntaxContext context, CancellationToken ct)
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
        InvalidOperationExceptionHelper.EnsureMinimumArguments(args.Count, 3);

        // Extract the interaction property path from the second argument (propertyName)
        var propertyNameArg = args[1].Expression;
        var interactionPropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(propertyNameArg, semanticModel, ct);
        if (interactionPropertyPath == null)
        {
            return null;
        }

        // Get the interaction property type to extract TInput, TOutput
        var leafType = interactionPropertyPath[interactionPropertyPath.Length - 1].PropertyTypeFullName;

        // Resolve TInput, TOutput from the IInteraction<TInput, TOutput> type
        // Re-resolve via the semantic model to get the actual type arguments
        var inputTypeFullName = string.Empty;
        var outputTypeFullName = string.Empty;

        if (propertyNameArg is LambdaExpressionSyntax lambda)
        {
            var body = SyntaxHelpers.GetLambdaBody(lambda);

            if (body != null)
            {
                body = SyntaxHelpers.UnwrapNullForgiving(body);
                if (body is MemberAccessExpressionSyntax leafMemberAccess)
                {
                    var memberSymbol = semanticModel.GetSymbolInfo(leafMemberAccess, ct).Symbol;
                    if (memberSymbol is IPropertySymbol propertySymbol)
                    {
                        var propType = propertySymbol.Type;
                        if (SymbolHelpers.ExtractInteractionTypeArguments(propType, out var input, out var output))
                        {
                            inputTypeFullName = input;
                            outputTypeFullName = output;
                        }
                    }
                }
            }
        }

        inputTypeFullName = InvalidOperationExceptionHelper.EnsureNotNullOrEmpty(inputTypeFullName, "interaction TInput type argument");
        outputTypeFullName = InvalidOperationExceptionHelper.EnsureNotNullOrEmpty(outputTypeFullName, "interaction TOutput type argument");

        // Determine handler type (Task vs Observable)
        var isTaskHandler = true;
        string? dontCareTypeFullName = null;

        // The handler parameter is the 3rd argument (index 2)
        // Check the method's parameter type to determine handler variant
        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            var param = methodSymbol.Parameters[i];
            if (param is { Name: "handler", Type: INamedTypeSymbol handlerType })
            {
                // Observable handler: Func<IInteractionContext<TInput, TOutput>, IObservable<TDontCare>>
                if (handlerType.TypeArguments.Length == 2
                    && handlerType.TypeArguments[1] is INamedTypeSymbol returnType
                    && SymbolHelpers.IsIObservable(returnType))
                {
                    isTaskHandler = false;
                    dontCareTypeFullName = returnType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }
            }
        }

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

        return new BindInteractionInvocationInfo(
            CallerFilePath: filePath,
            CallerLineNumber: lineNumber,
            ViewTypeFullName: viewTypeFullName,
            ViewModelTypeFullName: viewModelTypeFullName,
            InteractionPropertyPath: new EquatableArray<PropertyPathSegment>(interactionPropertyPath),
            InputTypeFullName: inputTypeFullName,
            OutputTypeFullName: outputTypeFullName,
            IsTaskHandler: isTaskHandler,
            DontCareTypeFullName: dontCareTypeFullName,
            MethodName: Constants.BindInteractionMethodName,
            ExpressionText: expressionText);
    }
}

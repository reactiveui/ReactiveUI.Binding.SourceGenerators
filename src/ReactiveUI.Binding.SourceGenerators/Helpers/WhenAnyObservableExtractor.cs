// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>
/// Extracts WhenAnyObservableInvocationInfo from WhenAnyObservable invocations.
/// </summary>
internal static class WhenAnyObservableExtractor
{
    /// <summary>
    /// Pipeline B transform: extracts InvocationInfo from a WhenAnyObservable invocation.
    /// For each Expression&lt;Func&lt;TSender, IObservable&lt;T&gt;?&gt;&gt; parameter, extracts the property path
    /// and the inner type T by unwrapping IObservable&lt;T&gt; from the leaf property type.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A WhenAnyObservableInvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static WhenAnyObservableInvocationInfo? ExtractWhenAnyObservableInvocation(
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
        var propertyPaths = new List<EquatableArray<PropertyPathSegment>>(args.Count);
        var expressionTexts = new List<string>(args.Count);
        var innerObservableTypes = new List<string>(args.Count);
        var hasSelector = false;

        // Loop over parameters to identify expression parameters and selector
        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            var parameter = methodSymbol.Parameters[i];

            // Check if parameter type is Expression<Func<TSender, IObservable<T>?>>
            if (parameter.Type is INamedTypeSymbol { Name: "Expression" })
            {
                var path = SyntaxHelpers.ExtractPropertyPathFromLambda(args[i].Expression, semanticModel, ct);
                if (path != null)
                {
                    propertyPaths.Add(new(path));
                    expressionTexts.Add(
                        CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(args[i].Expression.ToString()));

                    // Extract the inner type T from the leaf property type IObservable<T>?
                    var leafSegment = path[path.Length - 1];
                    var innerType =
                        SymbolHelpers.ExtractInnerObservableType(leafSegment, semanticModel, args[i].Expression, ct);
                    innerObservableTypes.Add(innerType);
                }
            }
            else if (parameter.Name == "selector")
            {
                hasSelector = true;
            }
        }

        if (propertyPaths.Count == 0)
        {
            return null;
        }

        // Get the source type from the receiver
        var sourceTypeFullName = InvalidOperationExceptionHelper.EnsureNotNull(
            ExtractorValidation.GetTypeDisplayName(semanticModel.GetTypeInfo(memberAccess.Expression, ct).Type),
            "source type display name");

        // Compute return type
        string returnTypeFullName = hasSelector
            ? ExtractorValidation.FindSelectorReturnType(
                methodSymbol.Parameters,
                "selector")!
            : InvalidOperationExceptionHelper.EnsureNotNull(
                innerObservableTypes[0],
                "inner observable types");

        var filePath = invocation.SyntaxTree.FilePath;
        var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

        return new(
            filePath,
            lineNumber,
            sourceTypeFullName,
            new([.. propertyPaths]),
            new([.. innerObservableTypes]),
            returnTypeFullName,
            hasSelector,
            new([.. expressionTexts]));
    }
}

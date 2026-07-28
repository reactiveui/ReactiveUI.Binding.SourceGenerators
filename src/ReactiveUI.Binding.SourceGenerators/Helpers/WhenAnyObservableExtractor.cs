// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Extracts WhenAnyObservableInvocationInfo from WhenAnyObservable invocations.</summary>
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
        if (methodSymbol is null)
        {
            return null;
        }

        // Verify this is our stub or generated method
        if (!ExtractorValidation.IsRecognizedExtensionClass(methodSymbol.ContainingType.Name))
        {
            return null;
        }

        var args = invocation.ArgumentList.Arguments;
        var (propertyPaths, expressionTexts, innerObservableTypes, hasSelector) =
            CollectObservableArguments(methodSymbol, args, semanticModel, ct);

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

    /// <summary>
    /// Walks the invocation's parameters, collecting one property path, expression text and inner
    /// observable type per <c>Expression&lt;Func&lt;TSender, IObservable&lt;T&gt;&gt;&gt;</c> argument.
    /// </summary>
    /// <param name="methodSymbol">The resolved method.</param>
    /// <param name="args">The invocation arguments.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The observed paths, their expression texts, their inner types, and whether a selector was supplied.</returns>
    private static (List<EquatableArray<PropertyPathSegment>> PropertyPaths, List<string> ExpressionTexts, List<string> InnerObservableTypes, bool HasSelector)
        CollectObservableArguments(
            IMethodSymbol methodSymbol,
            SeparatedSyntaxList<ArgumentSyntax> args,
            SemanticModel semanticModel,
            CancellationToken ct)
    {
        var propertyPaths = new List<EquatableArray<PropertyPathSegment>>(args.Count);
        var expressionTexts = new List<string>(args.Count);
        var innerObservableTypes = new List<string>(args.Count);
        var hasSelector = false;

        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            var parameter = methodSymbol.Parameters[i];

            if (parameter.Name == "selector")
            {
                hasSelector = true;
                continue;
            }

            if (parameter.Type is not INamedTypeSymbol { Name: "Expression" })
            {
                continue;
            }

            var path = SyntaxHelpers.ExtractPropertyPathFromLambda(args[i].Expression, semanticModel, ct);
            if (path is null)
            {
                continue;
            }

            propertyPaths.Add(new(path));
            expressionTexts.Add(
                CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(args[i].Expression.ToString()));

            // The leaf property type is IObservable<T>; the generated code needs T.
            innerObservableTypes.Add(
                SymbolHelpers.ExtractInnerObservableType(path[^1], semanticModel, args[i].Expression, ct));
        }

        return (propertyPaths, expressionTexts, innerObservableTypes, hasSelector);
    }
}

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
    internal static WhenAnyObservableInvocationInfo? ExtractWhenAnyObservableInvocation(GeneratorSyntaxContext context, CancellationToken ct)
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
            if (parameter.Type is INamedTypeSymbol { Name: "Expression" } expressionType)
            {
                var path = SyntaxHelpers.ExtractPropertyPathFromLambda(args[i].Expression, semanticModel, ct);
                if (path != null)
                {
                    propertyPaths.Add(new EquatableArray<PropertyPathSegment>(path));
                    expressionTexts.Add(CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(args[i].Expression.ToString()));

                    // Extract the inner type T from the leaf property type IObservable<T>?
                    var leafSegment = path[path.Length - 1];
                    var innerType = SymbolHelpers.ExtractInnerObservableType(leafSegment, semanticModel, args[i].Expression, ct);
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
        string returnTypeFullName;
        if (hasSelector)
        {
            // For selector overloads, extract TReturn from the selector Func
            // hasSelector is only true when the parameter exists, so this always returns non-null.
            returnTypeFullName = ExtractorValidation.FindSelectorReturnType(
                methodSymbol.Parameters, "selector")!;
        }
        else
        {
            // For merge overloads, return type is the inner observable type (all same).
            // innerObservableTypes.Count is always > 0 here because propertyPaths.Count > 0
            // was verified above and both lists are populated together.
            returnTypeFullName = InvalidOperationExceptionHelper.EnsureNotNull(
                innerObservableTypes[0],
                "inner observable types");
        }

        var filePath = invocation.SyntaxTree.FilePath;
        var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

        return new WhenAnyObservableInvocationInfo(
            CallerFilePath: filePath,
            CallerLineNumber: lineNumber,
            SourceTypeFullName: sourceTypeFullName,
            PropertyPaths: new EquatableArray<EquatableArray<PropertyPathSegment>>(propertyPaths.ToArray()),
            InnerObservableTypeFullNames: new EquatableArray<string>(innerObservableTypes.ToArray()),
            ReturnTypeFullName: returnTypeFullName,
            HasSelector: hasSelector,
            ExpressionTexts: new EquatableArray<string>(expressionTexts.ToArray()));
    }
}

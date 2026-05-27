// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>
/// Extracts InvocationInfo from WhenChanged, WhenChanging, WhenAnyValue, and WhenAny invocations.
/// </summary>
internal static class ObservationExtractor
{
    /// <summary>
    /// Pipeline B transform: extracts InvocationInfo from a WhenChanged invocation.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An InvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    internal static InvocationInfo? ExtractWhenChangedInvocation(GeneratorSyntaxContext context, CancellationToken ct)
        => ExtractInvocationInfo(context, false, Constants.WhenChangedMethodName, ct);

    /// <summary>
    /// Pipeline B transform: extracts InvocationInfo from a WhenChanging invocation.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An InvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    internal static InvocationInfo? ExtractWhenChangingInvocation(GeneratorSyntaxContext context, CancellationToken ct)
        => ExtractInvocationInfo(context, true, Constants.WhenChangingMethodName, ct);

    /// <summary>
    /// Pipeline B transform: extracts InvocationInfo from a WhenAnyValue invocation.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An InvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    internal static InvocationInfo? ExtractWhenAnyValueInvocation(GeneratorSyntaxContext context, CancellationToken ct)
        => ExtractInvocationInfo(context, false, Constants.WhenAnyValueMethodName, ct);

    /// <summary>
    /// Pipeline B transform: extracts InvocationInfo from a WhenAny invocation (with IObservedChange selector).
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An InvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    internal static InvocationInfo? ExtractWhenAnyInvocation(GeneratorSyntaxContext context, CancellationToken ct)
        => ExtractInvocationInfo(context, false, Constants.WhenAnyMethodName, ct);

    /// <summary>
    /// Extracts the invocation info from the generator syntax context.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="isBeforeChange">A value indicating whether the invocation is before a change.</param>
    /// <param name="expectedMethodName">The expected method name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An InvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static InvocationInfo? ExtractInvocationInfo(
        GeneratorSyntaxContext context,
        bool isBeforeChange,
        string expectedMethodName,
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

        var hasSelector = CollectPropertyPaths(
            methodSymbol,
            args,
            semanticModel,
            propertyPaths,
            expressionTexts,
            ct);

        if (propertyPaths.Count == 0)
        {
            return null;
        }

        // Get the source type from the receiver
        var sourceTypeFullName = InvalidOperationExceptionHelper.EnsureNotNull(
            ExtractorValidation.GetTypeDisplayName(semanticModel.GetTypeInfo(memberAccess.Expression, ct).Type),
            "source type display name");

        var returnTypeFullName = ComputeReturnTypeFullName(methodSymbol, propertyPaths, hasSelector);

        var filePath = invocation.SyntaxTree.FilePath;
        var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

        return new(
            filePath,
            lineNumber,
            sourceTypeFullName,
            new([.. propertyPaths]),
            returnTypeFullName,
            isBeforeChange,
            hasSelector,
            expectedMethodName,
            new([.. expressionTexts]));
    }

    /// <summary>
    /// Loops over the method parameters, collecting property paths and expression texts for
    /// each <c>Expression&lt;Func&lt;...&gt;&gt;</c> argument and detecting a selector parameter.
    /// </summary>
    /// <param name="methodSymbol">The resolved method symbol.</param>
    /// <param name="args">The invocation argument list.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="propertyPaths">The list to append extracted property paths to.</param>
    /// <param name="expressionTexts">The list to append normalized expression texts to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> if a selector/conversion parameter was found.</returns>
    private static bool CollectPropertyPaths(
        IMethodSymbol methodSymbol,
        SeparatedSyntaxList<ArgumentSyntax> args,
        SemanticModel semanticModel,
        List<EquatableArray<PropertyPathSegment>> propertyPaths,
        List<string> expressionTexts,
        CancellationToken ct)
    {
        var hasSelector = false;

        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            var parameter = methodSymbol.Parameters[i];

            // Check if parameter type is Expression<Func<...>>
            if (parameter.Type is INamedTypeSymbol { Name: "Expression" })
            {
                var path = SyntaxHelpers.ExtractPropertyPathFromLambda(args[i].Expression, semanticModel, ct);
                if (path != null)
                {
                    propertyPaths.Add(new(path));
                    expressionTexts.Add(
                        CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(args[i].Expression.ToString()));
                }
            }
            else if (parameter.Name is "conversionFunc" or "selector")
            {
                hasSelector = true;
            }
        }

        return hasSelector;
    }

    /// <summary>
    /// Computes the observation return type from the collected property paths rather than from
    /// <c>methodSymbol.ReturnType</c>. This avoids an off-by-one issue where overload resolution
    /// might resolve to a previously-generated concrete overload with a different property count.
    /// </summary>
    /// <param name="methodSymbol">The resolved method symbol.</param>
    /// <param name="propertyPaths">The collected property paths.</param>
    /// <param name="hasSelector">Whether the overload has a selector/conversion parameter.</param>
    /// <returns>The fully qualified return type name.</returns>
    private static string ComputeReturnTypeFullName(
        IMethodSymbol methodSymbol,
        List<EquatableArray<PropertyPathSegment>> propertyPaths,
        bool hasSelector)
    {
        if (hasSelector)
        {
            // For selector overloads, find the selector parameter's Func<..., TReturn>
            // and extract TReturn (the last type argument).
            // hasSelector is only true when the parameter exists, so this always returns non-null.
            return ExtractorValidation.FindSelectorReturnType(
                methodSymbol.Parameters,
                "conversionFunc",
                "selector")!;
        }

        if (propertyPaths.Count == 1)
        {
            // Single property: return type is the leaf property type.
            var singlePath = propertyPaths[0];
            return singlePath[singlePath.Length - 1].PropertyTypeFullName;
        }

        // Multiple properties: return type is a named value tuple.
        var tupleBuilder = new System.Text.StringBuilder("(");
        for (var i = 0; i < propertyPaths.Count; i++)
        {
            var path = propertyPaths[i];
            var leafType = path[path.Length - 1].PropertyTypeFullName;
            tupleBuilder.Append(leafType).Append(" property").Append(i + 1);
            if (i < propertyPaths.Count - 1)
            {
                tupleBuilder.Append(", ");
            }
        }

        tupleBuilder.Append(')');
        return tupleBuilder.ToString();
    }
}

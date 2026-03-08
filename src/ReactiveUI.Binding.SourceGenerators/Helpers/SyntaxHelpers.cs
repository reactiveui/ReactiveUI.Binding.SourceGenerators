// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>
/// Provides syntax-level helpers for extracting property paths from lambda expressions.
/// </summary>
internal static class SyntaxHelpers
{
    /// <summary>
    /// Extracts the property path from a lambda expression.
    /// </summary>
    /// <param name="expression">The expression syntax.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An array of property path segments, or null if the expression is not a valid lambda.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static PropertyPathSegment[]? ExtractPropertyPathFromLambda(
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        // Must be an inline lambda
        if (expression is not LambdaExpressionSyntax lambda)
        {
            return null;
        }

        // Get the lambda body (only SimpleLambda and ParenthesizedLambda exist in Roslyn)
        var body = GetLambdaBody(lambda);

        if (body == null)
        {
            return null;
        }

        // Decompose member access chain: x.A.B.C → [A, B, C]
        // Also handles null-forgiving operator: x.A!.B!.C → [A, B, C]
        var segments = new List<PropertyPathSegment>(4);
        var current = UnwrapNullForgiving(body);

        while (current is MemberAccessExpressionSyntax memberAccess)
        {
            ct.ThrowIfCancellationRequested();

            var memberSymbolInfo = semanticModel.GetSymbolInfo(memberAccess, ct);
            if (memberSymbolInfo.Symbol is not IPropertySymbol propertySymbol)
            {
                return null;
            }

            // Check accessibility — skip private/protected members
            if (propertySymbol.DeclaredAccessibility != Accessibility.Public
                && propertySymbol.DeclaredAccessibility != Accessibility.Internal)
            {
                return null;
            }

            segments.Add(new PropertyPathSegment(
                PropertyName: propertySymbol.Name,
                PropertyTypeFullName: propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                DeclaringTypeFullName: propertySymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));

            current = UnwrapNullForgiving(memberAccess.Expression);
        }

        if (segments.Count == 0)
        {
            return null;
        }

        // Reverse so the path goes from root to leaf
        segments.Reverse();
        return segments.ToArray();
    }

    /// <summary>
    /// Extracts the body expression from a <see cref="LambdaExpressionSyntax"/>.
    /// Handles both <see cref="SimpleLambdaExpressionSyntax"/> and
    /// <see cref="ParenthesizedLambdaExpressionSyntax"/>.
    /// </summary>
    /// <param name="lambda">The lambda expression.</param>
    /// <returns>The body as an <see cref="ExpressionSyntax"/>, or null if the body is a block or unsupported form.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ExpressionSyntax? GetLambdaBody(LambdaExpressionSyntax lambda)
    {
        if (lambda is SimpleLambdaExpressionSyntax simple)
        {
            return simple.Body as ExpressionSyntax;
        }

        var parenthesized = (ParenthesizedLambdaExpressionSyntax)lambda;
        return parenthesized.Body as ExpressionSyntax;
    }

    /// <summary>
    /// Unwraps null-forgiving operators (!) from an expression.
    /// For example, <c>x.Child!</c> is a <see cref="PostfixUnaryExpressionSyntax"/>
    /// wrapping the <see cref="MemberAccessExpressionSyntax"/> for <c>x.Child</c>.
    /// This method strips those wrappers so the path extraction loop can proceed.
    /// </summary>
    /// <param name="expression">The expression to unwrap.</param>
    /// <returns>The unwrapped expression.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ExpressionSyntax UnwrapNullForgiving(ExpressionSyntax expression)
    {
        while (expression is PostfixUnaryExpressionSyntax postfix
               && postfix.IsKind(SyntaxKind.SuppressNullableWarningExpression))
        {
            expression = postfix.Operand;
        }

        return expression;
    }
}

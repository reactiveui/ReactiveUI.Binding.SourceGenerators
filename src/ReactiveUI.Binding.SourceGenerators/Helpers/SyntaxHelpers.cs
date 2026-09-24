// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Provides syntax-level helpers for extracting property paths from lambda expressions.</summary>
internal static class SyntaxHelpers
{
    /// <summary>Gets the line the compiler passes to a <c>CallerLineNumber</c> parameter of an invocation.</summary>
    /// <param name="invocation">The invocation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The one-based line number.</returns>
    /// <remarks>
    /// The compiler reports the line of the invoked member's name, not the line the invocation starts on. The two
    /// differ for a chained call written across lines, where the invocation of <c>.ToProperty(...)</c> on its own
    /// line starts back at the receiver on an earlier line.
    /// </remarks>
    internal static int CallerLineNumber(InvocationExpressionSyntax invocation, CancellationToken ct)
    {
        var anchor = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Span,
            MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Span,
            _ => invocation.Span,
        };

        return invocation.SyntaxTree.GetLineSpan(anchor, ct).StartLinePosition.Line + 1;
    }

    /// <summary>Extracts the property path from a lambda expression.</summary>
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

        if (body is null)
        {
            return null;
        }

        // Decompose member access chain: x.A.B.C → [A, B, C]
        // Also handles null-forgiving operator: x.A!.B!.C → [A, B, C]
        const int TypicalPathDepth = 4;

        var segments = new List<PropertyPathSegment>(TypicalPathDepth);
        var current = UnwrapNullForgiving(body);

        while (current is MemberAccessExpressionSyntax memberAccess)
        {
            ct.ThrowIfCancellationRequested();

            if (ReadPathSegment(memberAccess, semanticModel, segments.Count == 0, ct) is not { } segment)
            {
                return null;
            }

            segments.Add(segment);
            current = UnwrapNullForgiving(memberAccess.Expression);
        }

        if (segments.Count == 0)
        {
            return null;
        }

        // Reverse so the path goes from root to leaf
        segments.Reverse();
        return [.. segments];
    }

    /// <summary>Extracts the body expression from a <see cref="LambdaExpressionSyntax"/>.</summary>
    /// <param name="lambda">The lambda expression.</param>
    /// <returns>The body as an <see cref="ExpressionSyntax"/>, or null if the body is a block or unsupported form.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ExpressionSyntax? GetLambdaBody(LambdaExpressionSyntax lambda) =>
        lambda.Body as ExpressionSyntax;

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

    /// <summary>Reads one link of an observed property path.</summary>
    /// <param name="memberAccess">The member access naming the link.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="isLeaf">Whether the link is the last of the path.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The segment, or null when the link is not a property or field generated code can read.</returns>
    private static PropertyPathSegment? ReadPathSegment(
        MemberAccessExpressionSyntax memberAccess,
        SemanticModel semanticModel,
        bool isLeaf,
        CancellationToken ct)
    {
        var member = semanticModel.GetSymbolInfo(memberAccess, ct).Symbol;
        var memberType = member switch
        {
            IPropertySymbol property => property.Type,
            IFieldSymbol field when IsReadableFieldLink(field, isLeaf) => field.Type,
            _ => null,
        };

        // Private and protected members are out of reach of generated code.
        if (memberType is null || member!.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
        {
            return null;
        }

        var owner = semanticModel.GetTypeInfo(memberAccess.Expression, ct).Type as INamedTypeSymbol ?? member.ContainingType;

        // Generated code names every link's owner and value type, so a link through a type it cannot reach
        // leaves the whole path to the runtime stub.
        if (!ExtractorValidation.IsReachableFromGeneratedCode(owner, semanticModel.Compilation)
            || !ExtractorValidation.IsReachableFromGeneratedCode(memberType, semanticModel.Compilation))
        {
            return null;
        }

        // A field raises no notification, so it carries no mechanism and is read once, like any other link
        // whose owner cannot notify.
        return member is IPropertySymbol propertySymbol
            ? new(
                propertySymbol.Name,
                memberType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                owner.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                memberType.IsReferenceType,
                TypeDetectionExtractor.ExtractPropertyOwner(owner, propertySymbol, semanticModel.Compilation, ct))
            : new(
                member.Name,
                memberType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                owner.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                memberType.IsReferenceType,
                null,
                IsField: true);
    }

    /// <summary>Determines whether a field can stand as a link of a generated path.</summary>
    /// <param name="field">The field.</param>
    /// <param name="isLeaf">Whether the field is the last link, which a binding may write.</param>
    /// <returns><see langword="true"/> for an instance field that generated code may read, and write at the leaf.</returns>
    /// <remarks>
    /// Controls named in XAML or by a designer are exposed as fields, so a view binding usually reads through one,
    /// as in <c>v =&gt; v.NameBox.Text</c>. A read-only field is fine along the way but not at the leaf, which a
    /// two-way binding or <c>BindTo</c> assigns.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsReadableFieldLink(IFieldSymbol field, bool isLeaf) =>
        !field.IsStatic && !field.IsConst && (!isLeaf || !field.IsReadOnly);
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveUI.Binding.SourceGenerators;

/// <summary>
/// Fast syntax-only predicates for the incremental generator pipeline.
/// These run on every keystroke and must be extremely cheap (no semantic model).
/// </summary>
internal static class RoslynHelpers
{
    /// <summary>
    /// Pipeline A predicate: detects class declarations with a base list (potential INPC, IRO, DP, etc.).
    /// </summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a class with a base list; otherwise, false.</returns>
    internal static bool IsClassWithBaseList(SyntaxNode node, CancellationToken ct)
        => node is ClassDeclarationSyntax { BaseList.Types.Count: > 0 };

    /// <summary>
    /// Pipeline B predicate: detects WhenChanged invocations.
    /// </summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a WhenChanged invocation; otherwise, false.</returns>
    internal static bool IsWhenChangedInvocation(SyntaxNode node, CancellationToken ct)
        => node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name.Identifier.Text: Constants.WhenChangedMethodName
            }
        };

    /// <summary>
    /// Pipeline B predicate: detects WhenChanging invocations.
    /// </summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a WhenChanging invocation; otherwise, false.</returns>
    internal static bool IsWhenChangingInvocation(SyntaxNode node, CancellationToken ct)
        => node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name.Identifier.Text: Constants.WhenChangingMethodName
            }
        };

    /// <summary>
    /// Pipeline B predicate: detects BindOneWay or BindTwoWay invocations.
    /// </summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a bind invocation; otherwise, false.</returns>
    internal static bool IsBindInvocation(SyntaxNode node, CancellationToken ct)
        => node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name.Identifier.Text: Constants.BindOneWayMethodName or Constants.BindTwoWayMethodName or Constants.OneWayBindMethodName or Constants.BindMethodName
            }
        };

    /// <summary>
    /// Pipeline B predicate: detects WhenAnyValue invocations only.
    /// </summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a WhenAnyValue invocation; otherwise, false.</returns>
    internal static bool IsWhenAnyValueInvocation(SyntaxNode node, CancellationToken ct)
        => node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name.Identifier.Text: Constants.WhenAnyValueMethodName
            }
        };

    /// <summary>
    /// Pipeline B predicate: detects WhenAny invocations (with IObservedChange selector).
    /// </summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a WhenAny invocation; otherwise, false.</returns>
    internal static bool IsWhenAnyInvocation(SyntaxNode node, CancellationToken ct)
        => node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name.Identifier.Text: Constants.WhenAnyMethodName
            }
        };

    /// <summary>
    /// Pipeline B predicate: detects WhenAnyObservable invocations.
    /// </summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a WhenAnyObservable invocation; otherwise, false.</returns>
    internal static bool IsWhenAnyObservableInvocation(SyntaxNode node, CancellationToken ct)
        => node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name.Identifier.Text: Constants.WhenAnyObservableMethodName
            }
        };
}

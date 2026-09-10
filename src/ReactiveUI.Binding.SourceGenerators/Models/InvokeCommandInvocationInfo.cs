// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Per-call-site value-equatable POCO for <c>InvokeCommand</c> invocations. The values come from an observable
/// stream, so the only path captured is the one reaching the command. Contains no ISymbol, SyntaxNode, or
/// Location references.
/// </summary>
/// <param name="CallerFilePath">The source file path of the call site, captured via <c>[CallerFilePath]</c>.</param>
/// <param name="CallerLineNumber">The line number of the call site, captured via <c>[CallerLineNumber]</c>.</param>
/// <param name="SourceValueTypeFullName">The fully qualified type produced by the source observable (the <c>T</c> in <c>IObservable&lt;T&gt;</c>).</param>
/// <param name="TargetTypeFullName">The fully qualified name of the type declaring the command property.</param>
/// <param name="CommandPropertyPath">The property path chain reaching the command.</param>
/// <param name="CommandExpressionText">The original expression text of the command lambda argument.</param>
/// <param name="Interceptor">
/// Where this call site is, for a build that claims call sites outright rather than competing for them.
/// </param>
internal sealed record InvokeCommandInvocationInfo(
    string CallerFilePath,
    int CallerLineNumber,
    string SourceValueTypeFullName,
    string TargetTypeFullName,
    EquatableArray<PropertyPathSegment> CommandPropertyPath,
    string CommandExpressionText,
    InterceptorLocation Interceptor = default);

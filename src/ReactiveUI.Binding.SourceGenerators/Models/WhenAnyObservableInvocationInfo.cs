// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Per-call-site value-equatable POCO for WhenAnyObservable invocations.
/// Represents a single WhenAnyObservable invocation detected in user code.
/// Contains no ISymbol, SyntaxNode, or Location references.
/// </summary>
/// <param name="CallerFilePath">The source file path of the call site, captured via <c>[CallerFilePath]</c>.</param>
/// <param name="CallerLineNumber">The line number of the call site, captured via <c>[CallerLineNumber]</c>.</param>
/// <param name="SourceTypeFullName">The fully qualified name of the source (observed) type.</param>
/// <param name="PropertyPaths">The property path chains extracted from each lambda expression argument.</param>
/// <param name="InnerObservableTypeFullNames">The inner type T from IObservable&lt;T&gt; for each property path.</param>
/// <param name="ReturnTypeFullName">The fully qualified return type of the observation.</param>
/// <param name="HasSelector">Whether the invocation includes a selector/projection function (CombineLatest variant).</param>
/// <param name="ExpressionTexts">The original expression text of each lambda argument, used for CallerArgumentExpression dispatch.</param>
internal sealed record WhenAnyObservableInvocationInfo(
    string CallerFilePath,
    int CallerLineNumber,
    string SourceTypeFullName,
    EquatableArray<EquatableArray<PropertyPathSegment>> PropertyPaths,
    EquatableArray<string> InnerObservableTypeFullNames,
    string ReturnTypeFullName,
    bool HasSelector,
    EquatableArray<string> ExpressionTexts) : IEquatable<WhenAnyObservableInvocationInfo>;

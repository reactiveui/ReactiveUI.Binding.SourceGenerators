// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Per-call-site value-equatable POCO for BindInteraction invocations.
/// Contains no ISymbol, SyntaxNode, or Location references.
/// </summary>
/// <param name="CallerFilePath">The source file path of the call site.</param>
/// <param name="CallerLineNumber">The line number of the call site.</param>
/// <param name="ViewTypeFullName">The fully qualified name of the view type.</param>
/// <param name="ViewModelTypeFullName">The fully qualified name of the view model type.</param>
/// <param name="InteractionPropertyPath">The property path chain to the interaction property.</param>
/// <param name="InputTypeFullName">The fully qualified name of the interaction input type.</param>
/// <param name="OutputTypeFullName">The fully qualified name of the interaction output type.</param>
/// <param name="IsTaskHandler">True if the handler is task-based; false if observable-based.</param>
/// <param name="DontCareTypeFullName">The TDontCare type for observable handlers, or null for task handlers.</param>
/// <param name="MethodName">The name of the invoked method (BindInteraction).</param>
/// <param name="ExpressionText">The normalized expression text of the property path lambda.</param>
internal sealed record BindInteractionInvocationInfo(
    string CallerFilePath,
    int CallerLineNumber,
    string ViewTypeFullName,
    string ViewModelTypeFullName,
    EquatableArray<PropertyPathSegment> InteractionPropertyPath,
    string InputTypeFullName,
    string OutputTypeFullName,
    bool IsTaskHandler,
    string? DontCareTypeFullName,
    string MethodName,
    string ExpressionText);

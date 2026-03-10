// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Per-call-site value-equatable POCO for BindCommand invocations.
/// Contains no ISymbol, SyntaxNode, or Location references.
/// All event and command property information is resolved at extraction time.
/// </summary>
/// <param name="CallerFilePath">The source file path of the call site.</param>
/// <param name="CallerLineNumber">The line number of the call site.</param>
/// <param name="ViewTypeFullName">The fully qualified name of the view type.</param>
/// <param name="ViewModelTypeFullName">The fully qualified name of the view model type.</param>
/// <param name="CommandPropertyPath">The property path chain to the command property.</param>
/// <param name="ControlPropertyPath">The property path chain to the control.</param>
/// <param name="CommandTypeFullName">The fully qualified name of the command type.</param>
/// <param name="ControlTypeFullName">The fully qualified name of the control type.</param>
/// <param name="HasObservableParameter">Whether the invocation includes an IObservable parameter.</param>
/// <param name="HasExpressionParameter">Whether the invocation includes an Expression parameter for the command parameter.</param>
/// <param name="ParameterTypeFullName">The fully qualified type of the command parameter, or null.</param>
/// <param name="ParameterPropertyPath">The property path for the expression parameter variant, or null.</param>
/// <param name="ResolvedEventName">The event name resolved at compile time (from toEvent literal or default event detection).</param>
/// <param name="ResolvedEventArgsTypeFullName">The EventArgs type resolved at compile time, or null.</param>
/// <param name="MethodName">The name of the invoked method (BindCommand).</param>
/// <param name="CommandExpressionText">The normalized expression text of the command property lambda.</param>
/// <param name="ControlExpressionText">The normalized expression text of the control property lambda.</param>
/// <param name="ParameterExpressionText">The normalized expression text of the parameter property lambda, or null.</param>
/// <param name="HasCommandProperty">Whether the control type has a settable Command property (ICommand).</param>
/// <param name="HasCommandParameterProperty">Whether the control type has a settable CommandParameter property.</param>
/// <param name="HasEnabledProperty">Whether the control type has a settable Enabled property (bool).</param>
internal sealed record BindCommandInvocationInfo(
    string CallerFilePath,
    int CallerLineNumber,
    string ViewTypeFullName,
    string ViewModelTypeFullName,
    EquatableArray<PropertyPathSegment> CommandPropertyPath,
    EquatableArray<PropertyPathSegment> ControlPropertyPath,
    string CommandTypeFullName,
    string ControlTypeFullName,
    bool HasObservableParameter,
    bool HasExpressionParameter,
    string? ParameterTypeFullName,
    EquatableArray<PropertyPathSegment>? ParameterPropertyPath,
    string? ResolvedEventName,
    string? ResolvedEventArgsTypeFullName,
    string MethodName,
    string CommandExpressionText,
    string ControlExpressionText,
    string? ParameterExpressionText,
    bool HasCommandProperty,
    bool HasCommandParameterProperty,
    bool HasEnabledProperty);

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>
/// Shared emission helpers for event-based command binding plugins (<see cref="DefaultEventBindingPlugin"/>
/// and <see cref="EventEnabledBindingPlugin"/>), which dispatch on the command-parameter kind and emit a
/// generated event handler with the same nullable-aware signature.
/// </summary>
internal static class CommandEventBindingEmitter
{
    /// <summary>
    /// Returns the <c>sender</c> parameter type for a generated event handler, annotated nullable
    /// (<c>object?</c>) when the target supports nullable reference types so it matches the
    /// <see cref="System.EventHandler"/> delegate, or plain <c>object</c> otherwise (C# 7.3).
    /// </summary>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <returns><c>"object?"</c> when nullable is supported; otherwise <c>"object"</c>.</returns>
    internal static string SenderType(bool supportsNullable) =>
        supportsNullable ? "object?" : "object";

    /// <summary>
    /// Resolves the event-args type and command-parameter kind, then dispatches to the matching emitter.
    /// Centralizes the dispatch shared by the event-based command binding plugins.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain expression.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="emitObservableParameter">Emits the binding when an observable parameter is supplied.</param>
    /// <param name="emitExpressionParameter">Emits the binding when an expression parameter is supplied.</param>
    /// <param name="emitNoParameter">Emits the binding when no parameter is supplied.</param>
    internal static void EmitByParameterKind(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        bool supportsNullable,
        Action<StringBuilder, BindCommandInvocationInfo, string, string, bool> emitObservableParameter,
        Action<StringBuilder, BindCommandInvocationInfo, string, string, string, bool> emitExpressionParameter,
        Action<StringBuilder, BindCommandInvocationInfo, string, string, bool> emitNoParameter)
    {
        var eventArgsType = inv.ResolvedEventArgsTypeFullName ?? "global::System.EventArgs";

        if (inv.HasObservableParameter)
        {
            emitObservableParameter(sb, inv, controlAccess, eventArgsType, supportsNullable);
        }
        else if (inv is { HasExpressionParameter: true, ParameterPropertyPath: not null })
        {
            var paramAccess =
                CodeGeneratorHelpers.BuildPropertyAccessChain("viewModel", inv.ParameterPropertyPath.Value);
            emitExpressionParameter(sb, inv, controlAccess, eventArgsType, paramAccess, supportsNullable);
        }
        else
        {
            emitNoParameter(sb, inv, controlAccess, eventArgsType, supportsNullable);
        }
    }
}

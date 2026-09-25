// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Owns a command binding for each concrete control reached by the view's property path.</summary>
internal static class CommandControlEmitter
{
    /// <summary>Closes the observing worker and opens the worker that captures one control instance.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="inv">The command binding.</param>
    /// <param name="viewInfo">The view's observation capabilities.</param>
    /// <param name="suffix">The call site's stable identifier.</param>
    internal static void EmitRebinding(StringBuilder sb, BindCommandInvocationInfo inv, ClassBindingInfo? viewInfo, string suffix)
    {
        ObservationCodeGenerator.EmitInlineObservation(sb, "view", inv.ControlPropertyPath, inv.ControlTypeFullName, viewInfo, "__controlChanges");
        _ = BindingEmitterHelpers.AppendViewThreadCall(sb.Append("            var __controls = "), "__controlChanges", "view", inv.ViewThreadInvoker)
            .AppendLine(";")
            .AppendLine("            var __controlBinding = new global::ReactiveUI.Primitives.Disposables.SwapDisposable();")
            .AppendLine("            var __controlSub = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(__controls, __control =>")
            .AppendLine("            {")
            .AppendLine("                __controlBinding.Disposable = global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;")
            .AppendLine("                if (__control != null)")
            .AppendLine("                {")
            .Append("                    __controlBinding.Disposable = __BindCommandCore_").Append(suffix).Append("(__control, commandObs")
            .Append(CommandParameterEmitter.HasParameter(inv) ? ", withParameter" : string.Empty).AppendLine(");")
            .AppendLine("                }")
            .AppendLine("            });")
            .AppendLine("            return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(__controlSub, __controlBinding);")
            .AppendLine("        }")
            .AppendLine()
            .Append("        private static global::System.IDisposable __BindCommandCore_").Append(suffix).Append('(')
            .Append(inv.ControlTypeFullName).Append(" __control, global::System.IObservable<").Append(inv.CommandTypeFullName).Append("> commandObs");
        if (CommandParameterEmitter.HasParameter(inv))
        {
            _ = sb.Append(", global::System.IObservable<").Append(inv.ParameterTypeFullName).Append("> withParameter");
        }

        _ = sb.AppendLine(")").AppendLine("        {");

        // The view is not thread-affine but the control is: each command lands on the control's thread.
        if (inv.ControlOnlyViewThreadInvoker is { } controlInvoker)
        {
            _ = BindingEmitterHelpers.AppendViewThreadCall(sb.Append("            commandObs = "), "commandObs", "__control", controlInvoker)
                .AppendLine(";");
        }
    }
}

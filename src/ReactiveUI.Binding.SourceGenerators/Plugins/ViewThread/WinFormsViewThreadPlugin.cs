// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

/// <summary>Writes to a WinForms control on the thread that created its handle.</summary>
internal sealed class WinFormsViewThreadPlugin : IViewThreadPlugin
{
    /// <summary>The fully qualified WinForms type the invoker claims.</summary>
    private const string OwnerTypeFullName = "global::System.Windows.Forms.Control";

    /// <inheritdoc/>
    public string OwnerMetadataName => "System.Windows.Forms.Control";

    /// <inheritdoc/>
    public string InvokerTypeName => "__WinFormsViewThreadInvoker";

    /// <inheritdoc/>
    public void EmitInvoker(StringBuilder sb, string nullableSuffix)
    {
        _ = ViewThreadInvokerSyntax.AppendOpen(sb, InvokerTypeName, OwnerTypeFullName)
            .AppendLine("            public bool CheckAccess(object target)")
            .AppendLine("            {")
            .Append("                return !((").Append(OwnerTypeFullName).AppendLine(")target).InvokeRequired;")
            .AppendLine("            }")
            .AppendLine();

        _ = ViewThreadInvokerSyntax.AppendPostOpen(sb, nullableSuffix)
            .Append("                var control = (").Append(OwnerTypeFullName).AppendLine(")target;")
            .AppendLine("                if (!control.InvokeRequired)")
            .AppendLine("                {")
            .AppendLine("                    callback(state);")
            .AppendLine("                    return;")
            .AppendLine("                }")
            .AppendLine()
            .AppendLine("                control.BeginInvoke(callback, new[] { state });")
            .AppendLine("            }")
            .AppendLine("        }");
    }
}

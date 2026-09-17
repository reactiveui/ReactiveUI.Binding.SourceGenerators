// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

using static ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread.ViewThreadInvokerSyntax;

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
        _ = AppendOpen(sb, InvokerTypeName, OwnerTypeFullName);
        _ = AppendCheckAccess(sb, null, $"!(({OwnerTypeFullName})target).InvokeRequired");
        _ = AppendPost(
            sb,
            nullableSuffix,
            $"var control = ({OwnerTypeFullName})target;",
            "!control.InvokeRequired",
            "control.BeginInvoke(callback, new[] { state });");
        _ = AppendClose(sb);
    }
}

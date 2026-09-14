// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

using static ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread.ViewThreadInvokerSyntax;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

/// <summary>Writes to a WPF object on the thread its dispatcher owns.</summary>
internal sealed class WpfViewThreadPlugin : IViewThreadPlugin
{
    /// <summary>The fully qualified WPF type the invoker claims.</summary>
    private const string OwnerTypeFullName = "global::System.Windows.Threading.DispatcherObject";

    /// <inheritdoc/>
    public string OwnerMetadataName => "System.Windows.Threading.DispatcherObject";

    /// <inheritdoc/>
    public string InvokerTypeName => "__WpfViewThreadInvoker";

    /// <inheritdoc/>
    public void EmitInvoker(StringBuilder sb, string nullableSuffix)
    {
        _ = AppendOpen(sb, InvokerTypeName, OwnerTypeFullName);
        _ = AppendCheckAccess(sb, null, $"(({OwnerTypeFullName})target).CheckAccess()");
        _ = AppendPost(
            sb,
            nullableSuffix,
            $"var dispatcher = (({OwnerTypeFullName})target).Dispatcher;",
            "dispatcher == null",
            "dispatcher.BeginInvoke(global::System.Windows.Threading.DispatcherPriority.Normal, callback, state);");
        _ = AppendClose(sb);
    }
}

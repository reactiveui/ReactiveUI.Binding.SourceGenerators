// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

using static ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread.ViewThreadInvokerSyntax;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

/// <summary>Writes to a MAUI object through the dispatcher it carries.</summary>
internal sealed class MauiViewThreadPlugin : IViewThreadPlugin
{
    /// <summary>The fully qualified MAUI type the invoker claims.</summary>
    private const string OwnerTypeFullName = "global::Microsoft.Maui.Controls.BindableObject";

    /// <summary>The statement that reads the dispatcher an object carries.</summary>
    private const string FindDispatcherStatement = "var dispatcher = FindDispatcher((global::Microsoft.Maui.Controls.BindableObject)target);";

    /// <inheritdoc/>
    public string OwnerMetadataName => "Microsoft.Maui.Controls.BindableObject";

    /// <inheritdoc/>
    public string InvokerTypeName => "__MauiViewThreadInvoker";

    /// <inheritdoc/>
    public void EmitInvoker(StringBuilder sb, string nullableSuffix)
    {
        _ = AppendOpen(sb, InvokerTypeName, OwnerTypeFullName);
        _ = AppendCheckAccess(sb, FindDispatcherStatement, "dispatcher == null || !dispatcher.IsDispatchRequired");
        _ = AppendPost(sb, nullableSuffix, FindDispatcherStatement, "dispatcher == null", "dispatcher.Dispatch(() => callback(state));")
            .AppendLine()
            .Append("            private static global::Microsoft.Maui.Dispatching.IDispatcher").Append(nullableSuffix)
            .Append(" FindDispatcher(").Append(OwnerTypeFullName).AppendLine(" owner)")
            .AppendLine(MemberOpen)
            .AppendLine("                try")
            .AppendLine(BodyBlockOpen)
            .AppendLine("                    return owner.Dispatcher;")
            .AppendLine(BodyBlockClose)
            .AppendLine("                catch (global::System.InvalidOperationException)")
            .AppendLine(BodyBlockOpen)
            .AppendLine("                    return null;")
            .AppendLine(BodyBlockClose)
            .AppendLine(MemberClose);
        _ = AppendClose(sb);
    }
}

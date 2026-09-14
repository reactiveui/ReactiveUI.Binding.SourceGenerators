// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

using static ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread.ViewThreadInvokerSyntax;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

/// <summary>Writes to a MAUI object through the dispatcher it carries.</summary>
internal sealed class MauiViewThreadPlugin : IViewThreadPlugin
{
    /// <summary>The fully qualified MAUI type the invoker claims.</summary>
    private const string OwnerTypeFullName = "global::Microsoft.Maui.Controls.BindableObject";

    /// <summary>The call that reads the dispatcher an object carries.</summary>
    private const string FindDispatcherCall = "                var dispatcher = FindDispatcher((global::Microsoft.Maui.Controls.BindableObject)target);";

    /// <inheritdoc/>
    public string OwnerMetadataName => "Microsoft.Maui.Controls.BindableObject";

    /// <inheritdoc/>
    public string InvokerTypeName => "__MauiViewThreadInvoker";

    /// <inheritdoc/>
    public void EmitInvoker(StringBuilder sb, string nullableSuffix)
    {
        _ = AppendOpen(sb, InvokerTypeName, OwnerTypeFullName)
            .AppendLine("            public bool CheckAccess(object target)")
            .AppendLine(MemberOpen)
            .AppendLine(FindDispatcherCall)
            .AppendLine("                return dispatcher == null || !dispatcher.IsDispatchRequired;")
            .AppendLine(MemberClose)
            .AppendLine();

        _ = AppendPostOpen(sb, nullableSuffix)
            .AppendLine(FindDispatcherCall)
            .AppendLine("                if (dispatcher == null)")
            .AppendLine(BodyBlockOpen)
            .AppendLine("                    callback(state);")
            .AppendLine("                    return;")
            .AppendLine(BodyBlockClose)
            .AppendLine()
            .AppendLine("                dispatcher.Dispatch(() => callback(state));")
            .AppendLine(MemberClose)
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
            .AppendLine(MemberClose)
            .AppendLine("        }");
    }
}

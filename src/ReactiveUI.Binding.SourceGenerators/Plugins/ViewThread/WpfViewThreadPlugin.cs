// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

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
        _ = ViewThreadInvokerSyntax.AppendOpen(sb, InvokerTypeName, OwnerTypeFullName)
            .AppendLine("            public bool CheckAccess(object target)")
            .AppendLine("            {")
            .Append("                return ((").Append(OwnerTypeFullName).AppendLine(")target).CheckAccess();")
            .AppendLine("            }")
            .AppendLine();

        _ = ViewThreadInvokerSyntax.AppendPostOpen(sb, nullableSuffix)
            .Append("                var dispatcher = ((").Append(OwnerTypeFullName).AppendLine(")target).Dispatcher;")
            .AppendLine("                if (dispatcher == null)")
            .AppendLine("                {")
            .AppendLine("                    callback(state);")
            .AppendLine("                    return;")
            .AppendLine("                }")
            .AppendLine()
            .AppendLine("                dispatcher.BeginInvoke(global::System.Windows.Threading.DispatcherPriority.Normal, callback, state);")
            .AppendLine("            }")
            .AppendLine("        }");
    }
}

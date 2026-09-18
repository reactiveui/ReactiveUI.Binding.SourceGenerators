// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.SetMethod;

/// <summary>Emits typed collection mutation and isolates winning custom set-method providers.</summary>
internal static class CollectionSetMethodEmitter
{
    /// <summary>Opens a block within the write callback.</summary>
    private const string WriteBlockOpen = "                {";

    /// <summary>Closes a block within the write callback.</summary>
    private const string WriteBlockClose = "                }";

    /// <summary>Subscribes after scheduling so collection operations run on the owning thread.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="write">The concrete collection write.</param>
    /// <param name="source">The scheduled input stream.</param>
    internal static void EmitSubscription(StringBuilder sb, in SetMethodEmission write, string source)
    {
        var targetType = write.Path[write.Path.Length - 1].PropertyTypeFullName;
        AppendSelection(sb, write, targetType);
        if (write.ReportChanges)
        {
            _ = sb.Append("            var __setChanges = new global::ReactiveUI.Primitives.Signals.Signal<").Append(targetType).AppendLine(">();");
        }

        _ = sb.Append("            var __setSubscription = ").Append(GeneratedTypeNames.BindingErrors).Append(".Subscribe(").Append(source).AppendLine(", value =>")
            .AppendLine("            {");
        AppendTarget(sb, write);
        _ = sb.AppendLine("                if (__setConverter != null)")
            .AppendLine(WriteBlockOpen)
            .Append("                    var __result = (").Append(targetType).AppendLine(")__setConverter.PerformSet(__collection, value, null);");
        if (write.ReportChanges)
        {
            _ = sb.AppendLine("                    __setChanges.OnNext(__result);");
        }

        _ = sb.AppendLine("                    return;").AppendLine(WriteBlockClose);
        AppendMutation(sb, write.Mechanism);
        if (write.ReportChanges)
        {
            _ = sb.AppendLine("                __setChanges.OnNext(__collection);");
        }

        _ = sb.Append("            }, \"").Append(CodeGeneratorHelpers.EscapeString(write.Expression)).AppendLine("\");");
    }

    /// <summary>Resolves the custom winner once, retaining native ties.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="write">The concrete write.</param>
    /// <param name="targetType">The collection type.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendSelection(StringBuilder sb, in SetMethodEmission write, string targetType) =>
        sb.Append("            var __setConverter = global::ReactiveUI.Binding.BindingConverters.Current.SetMethodConverters.TryGetConverter(typeof(")
            .Append(write.SourceType).Append("), typeof(").Append(targetType).AppendLine("));")
            .Append("            if (__setConverter != null && __setConverter.GetAffinityForObjects(typeof(")
            .Append(write.SourceType).Append("), typeof(").Append(targetType).Append(")) <= ").Append(write.Mechanism.Affinity).AppendLine(")")
            .AppendLine("            {").AppendLine("                __setConverter = null;").AppendLine("            }");

    /// <summary>Reads each path link once and skips a temporarily missing collection owner.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="write">The target path.</param>
    private static void AppendTarget(StringBuilder sb, in SetMethodEmission write)
    {
        var parent = write.Root;
        for (var i = 0; i < write.Path.Length; i++)
        {
            var local = i == write.Path.Length - 1 ? "__collection" : $"__setParent{i.ToString(CultureInfo.InvariantCulture)}";
            _ = sb.Append("                var ").Append(local).Append(" = ").Append(CodeGeneratorHelpers.AppendSegmentRead(parent, write.Path[i])).AppendLine(";")
                .Append("                if (").Append(local).AppendLine(" == null)")
                .AppendLine(WriteBlockOpen).AppendLine("                    return;").AppendLine(WriteBlockClose);
            parent = local;
        }
    }

    /// <summary>Balances layout suspension even when enumeration or a native collection call fails.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="mechanism">The selected collection mechanism.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendMutation(StringBuilder sb, SetMethodInfo mechanism) =>
        sb.Append("                var __layoutOwner = __collection.").Append(mechanism.LayoutOwner).AppendLine(";")
            .AppendLine("                __layoutOwner.SuspendLayout();")
            .AppendLine("                try")
            .AppendLine(WriteBlockOpen)
            .AppendLine("                    __collection.Clear();")
            .Append("                    __collection.AddRange(")
            .Append(mechanism.SourceIsArray ? "value" : "global::System.Linq.Enumerable.ToArray<global::System.Windows.Forms.Control>(value)")
            .AppendLine(");")
            .AppendLine(WriteBlockClose)
            .AppendLine("                finally")
            .AppendLine(WriteBlockOpen)
            .AppendLine("                    __layoutOwner.ResumeLayout();")
            .AppendLine(WriteBlockClose);
}

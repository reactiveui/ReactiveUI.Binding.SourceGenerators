// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

/// <summary>Emits the parts every generated invoker class shares.</summary>
internal static class ViewThreadInvokerSyntax
{
    /// <summary>Opens a block inside a member body.</summary>
    internal const string BodyBlockOpen = "                {";

    /// <summary>Closes a block inside a member body.</summary>
    internal const string BodyBlockClose = "                }";

    /// <summary>Closes a member of the invoker class.</summary>
    internal const string MemberClose = "            }";

    /// <summary>Opens a member of the invoker class.</summary>
    internal const string MemberOpen = "            {";

    /// <summary>Opens an invoker class, declaring its instance and its <c>Claims</c> member.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="invokerTypeName">The name of the invoker class.</param>
    /// <param name="ownerTypeFullName">The fully qualified platform type the invoker claims.</param>
    /// <returns>The string builder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static StringBuilder AppendOpen(StringBuilder sb, string invokerTypeName, string ownerTypeFullName) =>
        sb.AppendLine()
            .Append("        private sealed class ").Append(invokerTypeName).Append(" : ").AppendLine(GeneratedTypeNames.IViewThreadInvoker)
            .AppendLine("        {")
            .Append("            internal static readonly ").Append(invokerTypeName).Append(" Instance = new ").Append(invokerTypeName).AppendLine("();")
            .AppendLine()
            .AppendLine("            public bool Claims(object target)")
            .AppendLine(MemberOpen)
            .Append("                return target is ").Append(ownerTypeFullName).AppendLine(";")
            .AppendLine(MemberClose)
            .AppendLine();

    /// <summary>Opens the <c>Post</c> member.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="nullableSuffix">The annotation for a nullable reference type, or empty below C# 8.</param>
    /// <returns>The string builder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static StringBuilder AppendPostOpen(StringBuilder sb, string nullableSuffix) =>
        sb.Append("            public void Post(object target, global::System.Action<object").Append(nullableSuffix)
            .Append("> callback, object").Append(nullableSuffix).AppendLine(" state)")
            .AppendLine(MemberOpen);
}

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

    /// <summary>The indentation of a statement inside a member body.</summary>
    internal const string StatementIndent = "                ";

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
            .Append(StatementIndent).Append("return target is ").Append(ownerTypeFullName).AppendLine(";")
            .AppendLine(MemberClose)
            .AppendLine();

    /// <summary>Emits the <c>CheckAccess</c> member.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="leadingStatement">A statement to run before the check, or null for none.</param>
    /// <param name="accessExpression">The expression that is true when the caller may write to the target.</param>
    /// <returns>The string builder.</returns>
    internal static StringBuilder AppendCheckAccess(StringBuilder sb, string? leadingStatement, string accessExpression)
    {
        _ = sb.AppendLine("            public bool CheckAccess(object target)").AppendLine(MemberOpen);

        if (leadingStatement is not null)
        {
            _ = sb.Append(StatementIndent).AppendLine(leadingStatement);
        }

        return sb.Append(StatementIndent).Append("return ").Append(accessExpression).AppendLine(";")
            .AppendLine(MemberClose)
            .AppendLine();
    }

    /// <summary>Emits the <c>Post</c> member: find the owner, run inline when it has no thread, otherwise queue.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="nullableSuffix">The annotation for a nullable reference type, or empty below C# 8.</param>
    /// <param name="ownerStatement">The statement that reads what queues the work.</param>
    /// <param name="inlineCondition">The condition under which the callback runs on the calling thread.</param>
    /// <param name="queueStatement">The statement that queues the callback on the owning thread.</param>
    /// <returns>The string builder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static StringBuilder AppendPost(
        StringBuilder sb,
        string nullableSuffix,
        string ownerStatement,
        string inlineCondition,
        string queueStatement) =>
        sb.Append("            public void Post(object target, global::System.Action<object").Append(nullableSuffix)
            .Append("> callback, object").Append(nullableSuffix).AppendLine(" state)")
            .AppendLine(MemberOpen)
            .Append(StatementIndent).AppendLine(ownerStatement)
            .Append(StatementIndent).Append("if (").Append(inlineCondition).AppendLine(")")
            .AppendLine(BodyBlockOpen)
            .AppendLine("                    callback(state);")
            .AppendLine("                    return;")
            .AppendLine(BodyBlockClose)
            .AppendLine()
            .Append(StatementIndent).AppendLine(queueStatement)
            .AppendLine(MemberClose);

    /// <summary>Closes an invoker class.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <returns>The string builder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static StringBuilder AppendClose(StringBuilder sb) => sb.AppendLine("        }");
}

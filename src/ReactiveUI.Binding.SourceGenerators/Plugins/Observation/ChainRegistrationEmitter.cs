// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Opens the choice that lets a registration observe one link of a chain in a mechanism's place.</summary>
/// <remarks>
/// Every mechanism offers its links on the same terms, so the arguments are written once here and each plugin
/// supplies only the observation that follows them. All of it is fixed at compile time - the declaring type,
/// the property name, the accessor, and a lambda the compiler turns into member tokens - so a link honours a
/// registration without resolving anything by name, and a consumer publishing ahead-of-time carries no
/// expression engine for it.
/// </remarks>
internal static class ChainRegistrationEmitter
{
    /// <summary>The opening a link inside a switch expression is written with.</summary>
    internal const string TernaryOpening = "                ? ";

    /// <summary>The argument indent a link inside a switch expression is written with.</summary>
    internal const string TernaryArgumentIndent = "                    ";

    /// <summary>Appends the chooser and its arguments, leaving the mechanism's observation to follow.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="sourceExpression">The expression naming the object the property is read from.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="generatedAffinity">The affinity of the mechanism this link was built from.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="opening">What the call is written after, which differs by the position it sits in.</param>
    /// <param name="argumentIndent">The indent each argument is written at.</param>
    /// <remarks>
    /// A link inside a switch expression is written after a <c>?</c>; the first link of a chain and a
    /// single-property observation are written after a <c>return</c>. The arguments are the same either way, so
    /// the position is handed in rather than the call being written twice.
    /// </remarks>
    internal static void AppendChoiceOpen(
        StringBuilder sb,
        string sourceExpression,
        PropertyPathSegment segment,
        int generatedAffinity,
        bool isBeforeChange,
        string opening = TernaryOpening,
        string argumentIndent = TernaryArgumentIndent)
    {
        var declaringType = segment.DeclaringTypeFullName;
        var valueType = segment.PropertyTypeFullName;

        _ = sb.Append(opening).Append("global::ReactiveUI.Binding.Observables.PluginObservationSource.Choose<")
            .Append(valueType).AppendLine(">(")
            .Append(argumentIndent).Append(sourceExpression).AppendLine(",")
            .Append(argumentIndent).Append("((global::System.Linq.Expressions.Expression<global::System.Func<").Append(declaringType).Append(", ")
            .Append(valueType).Append(">>)(__e => __e.").Append(segment.PropertyName).AppendLine(")).Body,")
            .Append(argumentIndent).Append('"').Append(segment.PropertyName).AppendLine("\",")
            .Append(argumentIndent).Append(isBeforeChange ? "true" : "false").AppendLine(",")
            .Append(argumentIndent).Append(generatedAffinity).AppendLine(",")
            .Append(argumentIndent).Append("(object __o) => ((").Append(declaringType).Append(")__o).").Append(segment.PropertyName).AppendLine(",");
    }
}

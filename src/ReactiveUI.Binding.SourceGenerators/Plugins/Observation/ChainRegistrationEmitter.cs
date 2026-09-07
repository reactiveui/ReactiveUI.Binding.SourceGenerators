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
    /// <summary>Appends the chooser and its arguments, leaving the mechanism's observation to follow.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="lambdaParam">The name the switch lambda gives the parent value.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="generatedAffinity">The affinity of the mechanism this link was built from.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    internal static void AppendChoiceOpen(
        StringBuilder sb,
        string lambdaParam,
        PropertyPathSegment segment,
        int generatedAffinity,
        bool isBeforeChange)
    {
        var declaringType = segment.DeclaringTypeFullName;
        var valueType = segment.PropertyTypeFullName;

        _ = sb.Append("                ? global::ReactiveUI.Binding.Observables.PluginObservationSource.Choose<")
            .Append(valueType).AppendLine(">(")
            .Append("                    ").Append(lambdaParam).AppendLine(",")
            .Append("                    ((global::System.Linq.Expressions.Expression<global::System.Func<").Append(declaringType).Append(", ")
            .Append(valueType).Append(">>)(__e => __e.").Append(segment.PropertyName).AppendLine(")).Body,")
            .Append("                    \"").Append(segment.PropertyName).AppendLine("\",")
            .Append("                    ").Append(isBeforeChange ? "true" : "false").AppendLine(",")
            .Append("                    ").Append(generatedAffinity).AppendLine(",")
            .Append("                    (object __o) => ((").Append(declaringType).Append(")__o).").Append(segment.PropertyName).AppendLine(",");
    }
}

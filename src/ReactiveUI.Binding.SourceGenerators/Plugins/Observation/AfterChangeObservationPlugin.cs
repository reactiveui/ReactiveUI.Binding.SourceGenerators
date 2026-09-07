// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>The base for a mechanism that reports a property change only once it has happened.</summary>
/// <remarks>
/// A dependency property, a component's change event and an Android widget's event all raise after the value
/// has already moved, so none of them can say what a property is about to become. What they do about being
/// asked anyway is the one thing that differs, so the shape is decided here and the answer is left to
/// <see cref="AnswersBeforeChangeWithLiveStream"/>.
/// </remarks>
internal abstract class AfterChangeObservationPlugin
{
    /// <summary>Gets the affinity this mechanism bids with.</summary>
    /// <remarks>
    /// A chain link offers its observation to any registration scoring higher than this, so the number the
    /// plugin bids to the registry is the same one the emitted comparison carries.
    /// </remarks>
    public abstract int Affinity { get; }

    /// <summary>Gets a value indicating whether this mechanism can report a change before it happens.</summary>
    public bool SupportsBeforeChanged => false;

    /// <summary>Gets a value indicating whether a before-change request is answered with the live change stream.</summary>
    /// <remarks>
    /// None of these mechanisms can say what a property is about to become, but they do not all decline the
    /// question the same way. A dependency property hands back the stream it always has, so the caller keeps
    /// tracking and merely receives the value after each change rather than before it. A component's change
    /// event scores nothing for a before-change request instead, which withdraws the mechanism and leaves the
    /// property read once. Following whichever the platform does is what keeps a before-change observation
    /// behaving the same here as it does through the runtime engine.
    /// </remarks>
    protected virtual bool AnswersBeforeChangeWithLiveStream => false;

    /// <summary>Emits the observation of a property read directly off the object a call site named.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable holding the observed object.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="castTypeName">The type the observed object is cast to.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="includeStartWith">Whether the observation opens with the property's current value.</param>
    public void EmitShallowObservation(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        bool includeStartWith)
    {
        if (isBeforeChange && !AnswersBeforeChangeWithLiveStream)
        {
            _ = UnchangingObservationEmitter.AppendExpression(sb, rootVar, segment, castTypeName);
            return;
        }

        AppendShallowObservation(sb, rootVar, segment, castTypeName, includeStartWith);
    }

    /// <summary>Emits that same observation assigned to a local.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable holding the observed object.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="castTypeName">The type the observed object is cast to.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="varName">The name of the local to assign.</param>
    public void EmitShallowObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string varName)
    {
        if (isBeforeChange && !AnswersBeforeChangeWithLiveStream)
        {
            _ = UnchangingObservationEmitter.AppendVariable(sb, rootVar, segment, castTypeName, varName);
            return;
        }

        AppendShallowObservationVariable(sb, rootVar, segment, castTypeName, varName);
    }

    /// <summary>Emits the observation the first stage of a deep chain is rooted on.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable holding the observed object.</param>
    /// <param name="segment">The first property path segment.</param>
    /// <param name="castTypeName">The type the observed object is cast to.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="obsVarName">The name of the observable local to assign.</param>
    public void EmitDeepChainRootSegment(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string obsVarName)
    {
        if (isBeforeChange && !AnswersBeforeChangeWithLiveStream)
        {
            _ = UnchangingObservationEmitter.AppendTypedVariable(sb, rootVar, segment, castTypeName, obsVarName)
                .AppendLine();
            return;
        }

        AppendDeepChainRootSegment(sb, rootVar, segment, castTypeName, obsVarName);
    }

    /// <summary>Emits the observation of one link past the first in a deep chain.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="prevVar">The variable holding the previous link's observation.</param>
    /// <param name="curVar">The name of the local this link's observation is assigned to.</param>
    /// <param name="lambdaParam">The name the switch lambda gives the parent value.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="nullParentBehavior">What the link observes while its parent is null.</param>
    /// <remarks>
    /// The link switches onto whichever parent the previous one last produced, so the whole shape - the switch,
    /// the null-parent test and the substitute observation - is the same whatever the mechanism. Only the
    /// observation of a present parent differs, which is what each plugin supplies.
    /// </remarks>
    public void EmitDeepChainInnerSegment(
        StringBuilder sb,
        string prevVar,
        string curVar,
        string lambdaParam,
        PropertyPathSegment segment,
        bool isBeforeChange,
        NullParentObservationBehavior nullParentBehavior)
    {
        var segType = segment.PropertyTypeFullName;
        var nullParentObservable = nullParentBehavior == NullParentObservationBehavior.EmitDefault
            ? $"new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{segType}>(default({segType}))"
            : $"global::ReactiveUI.Primitives.Advanced.ImmutableEmptySignal<{segType}>.Instance";

        _ = sb.AppendLine().Append(GeneratedSyntax.InlineLocalDeclaration).Append(curVar).Append(" = ")
            .Append(GeneratedTypeNames.OpenChainSwitchMap(segment, segType, prevVar)).AppendLine().Append("            ").Append(lambdaParam)
            .Append(" => ").Append(lambdaParam).AppendLine(" != null");

        if (isBeforeChange && !AnswersBeforeChangeWithLiveStream)
        {
            _ = sb.Append("                ? (global::System.IObservable<").Append(segType);
            AppendUnchangingChainSegment(sb, lambdaParam, segment);
        }
        else
        {
            ChainRegistrationEmitter.AppendChoiceOpen(sb, lambdaParam, segment, Affinity, false);
            AppendChainSegmentObservation(sb, lambdaParam, segment);
            _ = sb.AppendLine(")");
        }

        _ = sb.Append("                : (global::System.IObservable<").Append(segType).Append(">)")
            .Append(nullParentObservable).AppendLine(");");
    }

    /// <summary>Appends the after-change observation as a bare expression.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable holding the observed object.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="castTypeName">The type the observed object is cast to.</param>
    /// <param name="includeStartWith">Whether the observation opens with the property's current value.</param>
    protected abstract void AppendShallowObservation(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool includeStartWith);

    /// <summary>Appends the after-change observation assigned to a local.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable holding the observed object.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="castTypeName">The type the observed object is cast to.</param>
    /// <param name="varName">The name of the local to assign.</param>
    protected abstract void AppendShallowObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string varName);

    /// <summary>Appends the after-change observation the first stage of a deep chain is rooted on.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable holding the observed object.</param>
    /// <param name="segment">The first property path segment.</param>
    /// <param name="castTypeName">The type the observed object is cast to.</param>
    /// <param name="obsVarName">The name of the observable local to assign.</param>
    protected abstract void AppendDeepChainRootSegment(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string obsVarName);

    /// <summary>Appends the after-change observation of a chain link whose parent is present.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="lambdaParam">The name the switch lambda gives the parent value.</param>
    /// <param name="segment">The property path segment being observed.</param>
    protected abstract void AppendChainSegmentObservation(
        StringBuilder sb,
        string lambdaParam,
        PropertyPathSegment segment);

    /// <summary>Appends the read that stands in for a before-change observation of a chain link.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="lambdaParam">The name the switch lambda gives the parent value.</param>
    /// <param name="segment">The property path segment being observed.</param>
    private static void AppendUnchangingChainSegment(
        StringBuilder sb,
        string lambdaParam,
        PropertyPathSegment segment) =>
        _ = sb.AppendLine(">)").Append("                    new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<")
            .Append(segment.PropertyTypeFullName).Append(">(((").Append(segment.DeclaringTypeFullName).Append(')').Append(lambdaParam).Append(").")
            .Append(segment.PropertyName).AppendLine(")");
}

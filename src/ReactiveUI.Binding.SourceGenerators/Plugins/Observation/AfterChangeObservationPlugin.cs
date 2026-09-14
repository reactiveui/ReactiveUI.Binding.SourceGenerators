// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>The base for an observation mechanism that raises only after a property has changed.</summary>
internal closed class AfterChangeObservationPlugin : IObservationPlugin
{
    /// <summary>Gets the affinity this mechanism bids with.</summary>
    public abstract int Affinity { get; }

    /// <inheritdoc/>
    public abstract string ObservationKind { get; }

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => false;

    /// <inheritdoc/>
    public abstract bool RequiresHelperClasses { get; }

    /// <summary>Gets a value indicating whether a before-change request observes the after-change stream instead of reading the value once.</summary>
    protected virtual bool AnswersBeforeChangeWithLiveStream => false;

    /// <inheritdoc/>
    public abstract bool IsAMatch(ClassBindingInfo classInfo);

    /// <inheritdoc/>
    public abstract bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName);

    /// <inheritdoc/>
    public abstract void EmitHelperClasses(StringBuilder sb);

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

    /// <inheritdoc/>
    public abstract void EmitInlineObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string varName);

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

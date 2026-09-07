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
/// has already moved, so none of them can say what a property is about to become. Asking any of them for a
/// before-change observation reads the value once and stays open - the same answer in every case, which is why
/// it is decided here rather than in each plugin.
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
        if (isBeforeChange)
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
        if (isBeforeChange)
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
        if (isBeforeChange)
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

        if (isBeforeChange)
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

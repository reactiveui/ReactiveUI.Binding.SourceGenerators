// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
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
}

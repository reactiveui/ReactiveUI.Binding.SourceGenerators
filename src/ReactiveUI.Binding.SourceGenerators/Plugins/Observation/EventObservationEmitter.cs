// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Emits the observation of a property that reports its own changes on a named event.</summary>
/// <remarks>
/// Shared by the mechanisms that work this way and differ only in what the event is called: a component names
/// it after the property, and an Android widget names it whatever its framework chose. Both fuse the add, the
/// remove, the read and the initial value into one object rather than composing four.
/// </remarks>
internal static class EventObservationEmitter
{
    /// <summary>Appends the observation as a bare expression, for a caller assembling a larger one.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable the property is read from.</param>
    /// <param name="segment">The property being observed.</param>
    /// <param name="castTypeName">The type the root is cast to.</param>
    /// <param name="eventName">The event the property raises when it changes.</param>
    /// <param name="includeStartWith">Whether the observation opens with the property's current value.</param>
    /// <returns>The same string builder.</returns>
    internal static StringBuilder AppendExpression(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string eventName,
        bool includeStartWith)
    {
        _ = sb.Append("new global::ReactiveUI.Binding.Observables.EventObservable<")
            .Append(segment.PropertyTypeFullName).Append(">(");

        AppendHandler(sb, rootVar, castTypeName, eventName, " += __h, ");
        AppendHandler(sb, rootVar, castTypeName, eventName, " -= __h, ");

        _ = sb.Append("() => ");
        AppendRead(sb, rootVar, segment, castTypeName);

        return sb.Append(", ").Append(includeStartWith ? "true" : "false").Append(')');
    }

    /// <summary>Appends the observation as a local variable declaration, opening with the current value.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable the property is read from.</param>
    /// <param name="segment">The property being observed.</param>
    /// <param name="castTypeName">The type the root is cast to.</param>
    /// <param name="eventName">The event the property raises when it changes.</param>
    /// <param name="varName">The local the observation is assigned to.</param>
    /// <returns>The same string builder, so a caller can terminate the line as it needs.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static StringBuilder AppendVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string eventName,
        string varName)
    {
        _ = sb.Append("            var ").Append(varName).Append(" = ");

        return AppendExpression(sb, rootVar, segment, castTypeName, eventName, true).Append(';');
    }

    /// <summary>Appends one add or remove handler registration.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable carrying the event.</param>
    /// <param name="castTypeName">The type the root is cast to.</param>
    /// <param name="eventName">The event name.</param>
    /// <param name="operation">The rest of the registration, from the operator to the separator.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendHandler(
        StringBuilder sb,
        string rootVar,
        string castTypeName,
        string eventName,
        string operation) =>
        sb.Append("__h => ((").Append(castTypeName).Append(')').Append(rootVar).Append(").")
            .Append(eventName).Append(operation);

    /// <summary>Appends the read of the property's current value.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable the property is read from.</param>
    /// <param name="segment">The property being read.</param>
    /// <param name="castTypeName">The type the root is cast to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendRead(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName) =>
        sb.Append("((").Append(castTypeName).Append(')').Append(rootVar).Append(").")
            .Append(segment.PropertyName);
}

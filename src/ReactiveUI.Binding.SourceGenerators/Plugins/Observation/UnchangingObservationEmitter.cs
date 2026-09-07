// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Emits the observation for a property whose mechanism cannot report the change asked about.</summary>
/// <remarks>
/// <para>
/// Every platform mechanism answers one timing. A dependency property, a WinUI dependency property and a
/// WinForms companion event all report a change after it happened and have nothing to say before it, so a
/// before-change observation of one of them has no mechanism at all - which is the same position a plain
/// CLR property is in, and it takes the same answer: the value as it stands, once, and then silence.
/// </para>
/// <para>
/// The value matters. Reporting <c>default(T)</c> would say the property is empty when it is not, and a
/// binding reading it would clear its target on the strength of that. The runtime engine reports what the
/// property actually holds.
/// </para>
/// </remarks>
internal static class UnchangingObservationEmitter
{
    /// <summary>Appends the observation as a bare expression, for a caller assembling a larger one.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable the property is read from.</param>
    /// <param name="segment">The property being read.</param>
    /// <param name="castTypeName">The type the root is cast to before the read.</param>
    /// <returns>The same string builder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static StringBuilder AppendExpression(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName) =>
        sb.Append(GeneratedTypeNames.OpenUnchangingProperty).Append(segment.PropertyTypeFullName)
            .Append(">(((").Append(castTypeName).Append(')').Append(rootVar).Append(").")
            .Append(segment.PropertyName).Append(')');

    /// <summary>Appends the observation as a local variable declaration.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable the property is read from.</param>
    /// <param name="segment">The property being read.</param>
    /// <param name="castTypeName">The type the root is cast to before the read.</param>
    /// <param name="varName">The local the observation is assigned to.</param>
    /// <returns>The same string builder, so a caller can terminate the line as it needs.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static StringBuilder AppendVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string varName)
    {
        _ = sb.Append("            var ").Append(varName).Append(" = ");

        return AppendExpression(sb, rootVar, segment, castTypeName).Append(';');
    }

    /// <summary>Appends the observation as a local typed to the property's observable interface.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable the property is read from.</param>
    /// <param name="segment">The property being read.</param>
    /// <param name="castTypeName">The type the root is cast to before the read.</param>
    /// <param name="varName">The local the observation is assigned to.</param>
    /// <returns>The same string builder.</returns>
    /// <remarks>
    /// A chain's stages are re-assigned to one another, so each has to be typed as the interface rather
    /// than as the concrete observation, or the stage below cannot take what the stage above produced.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static StringBuilder AppendTypedVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string varName)
    {
        _ = sb.Append("            var ").Append(varName)
            .Append(" = (global::System.IObservable<").Append(segment.PropertyTypeFullName).Append(">)");

        return AppendExpression(sb, rootVar, segment, castTypeName).Append(';');
    }
}

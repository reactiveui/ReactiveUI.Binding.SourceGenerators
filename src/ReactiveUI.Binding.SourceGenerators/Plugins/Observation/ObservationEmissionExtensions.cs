// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Composes property expressions into locals and typed observation chains.</summary>
internal static class ObservationEmissionExtensions
{
    /// <summary>Composes the selected mechanism into the surrounding observation.</summary>
    /// <param name="plugin">The selected property observation mechanism.</param>
    extension(IObservationPlugin plugin)
    {
        /// <summary>Emits one property expression with the requested notification timing.</summary>
        /// <param name="sb">The output builder.</param>
        /// <param name="rootVar">The source variable.</param>
        /// <param name="segment">The observed property.</param>
        /// <param name="castTypeName">The concrete source type.</param>
        /// <param name="isBeforeChange">Whether to observe before the change.</param>
        /// <param name="includeStartWith">Whether equal consecutive values are suppressed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EmitShallowObservation(
            StringBuilder sb,
            string rootVar,
            PropertyPathSegment segment,
            string castTypeName,
            bool isBeforeChange,
            bool includeStartWith) =>
            plugin.EmitObservation(sb, new(rootVar, segment, castTypeName, isBeforeChange, includeStartWith));

        /// <summary>Assigns a typed observation to a local.</summary>
        /// <param name="sb">The output builder.</param>
        /// <param name="rootVar">The source variable.</param>
        /// <param name="segment">The observed property.</param>
        /// <param name="castTypeName">The concrete source type.</param>
        /// <param name="isBeforeChange">Whether to observe before the change.</param>
        /// <param name="varName">The resulting observable local.</param>
        internal void EmitShallowObservationVariable(
            StringBuilder sb,
            string rootVar,
            PropertyPathSegment segment,
            string castTypeName,
            bool isBeforeChange,
            string varName)
        {
            _ = sb.Append(GeneratedSyntax.BodyLocalDeclaration).Append(varName).Append(" = ");
            plugin.EmitShallowObservation(sb, rootVar, segment, castTypeName, isBeforeChange, true);
            _ = sb.AppendLine(";");
        }

        /// <summary>Emits the root observation that owns a property chain.</summary>
        /// <param name="sb">The output builder.</param>
        /// <param name="rootVar">The source variable.</param>
        /// <param name="segment">The observed property.</param>
        /// <param name="castTypeName">The concrete source type.</param>
        /// <param name="isBeforeChange">Whether to observe before the change.</param>
        /// <param name="obsVarName">The resulting observable local.</param>
        internal void EmitDeepChainRootSegment(
            StringBuilder sb,
            string rootVar,
            PropertyPathSegment segment,
            string castTypeName,
            bool isBeforeChange,
            string obsVarName)
        {
            _ = sb.Append(GeneratedSyntax.BodyLocalDeclaration).Append(obsVarName).Append(" = ");
            plugin.EmitShallowObservation(sb, rootVar, segment, castTypeName, isBeforeChange, false);
            _ = sb.AppendLine(";");
        }

        /// <summary>Emits an after-change observation used by a binding.</summary>
        /// <param name="sb">The output builder.</param>
        /// <param name="rootVar">The source variable.</param>
        /// <param name="segment">The observed property.</param>
        /// <param name="castTypeName">The concrete source type.</param>
        /// <param name="varName">The resulting observable local.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EmitInlineObservationVariable(
            StringBuilder sb,
            string rootVar,
            PropertyPathSegment segment,
            string castTypeName,
            string varName) =>
            plugin.EmitShallowObservationVariable(sb, rootVar, segment, castTypeName, false, varName);

        /// <summary>Re-subscribes a property whenever its parent changes, preserving custom-provider voting.</summary>
        /// <param name="sb">The output builder.</param>
        /// <param name="stage">The chain's observable locals and parent parameter.</param>
        /// <param name="segment">The observed property.</param>
        /// <param name="isBeforeChange">Whether to observe before the change.</param>
        /// <param name="nullParentBehavior">The delivery behavior while the parent is null.</param>
        internal void EmitDeepChainInnerSegment(
            StringBuilder sb,
            ChainStageVariables stage,
            PropertyPathSegment segment,
            bool isBeforeChange,
            NullParentObservationBehavior nullParentBehavior)
        {
            var valueType = segment.PropertyTypeFullName;
            _ = sb.AppendLine().Append(GeneratedSyntax.BodyLocalDeclaration).Append(stage.CurrentObservable).Append(" = ")
                .Append(GeneratedTypeNames.OpenChainSwitchMap(segment, valueType, stage.PreviousObservable)).AppendLine()
                .Append("                ").Append(stage.ParentParameter).Append(" => ").Append(stage.ParentParameter).AppendLine(" != null");
            ChainRegistrationEmitter.AppendChoiceOpen(sb, stage.ParentParameter, segment, plugin.Affinity, isBeforeChange);
            plugin.EmitShallowObservation(sb, stage.ParentParameter, segment, segment.DeclaringTypeFullName, isBeforeChange, false);
            _ = sb.AppendLine(")").Append("                : (global::System.IObservable<").Append(valueType).Append(">)");
            if (nullParentBehavior == NullParentObservationBehavior.EmitDefault)
            {
                _ = sb.Append("new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<").Append(valueType)
                    .Append(">(default(").Append(valueType).Append("))");
            }
            else
            {
                _ = sb.Append("global::ReactiveUI.Primitives.Advanced.ImmutableEmptySignal<").Append(valueType).Append(">.Instance");
            }

            _ = sb.AppendLine(");");
        }
    }
}

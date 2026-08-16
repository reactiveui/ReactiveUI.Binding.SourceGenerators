// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// The observation emission shared by every plugin that watches a type through
/// <see cref="System.ComponentModel.INotifyPropertyChanged"/> and
/// <see cref="System.ComponentModel.INotifyPropertyChanging"/>.
/// </summary>
/// <remarks>
/// A plain INPC type and an <c>IReactiveObject</c> differ only in which plugin claims them and at what
/// affinity - <c>IReactiveObject</c> implements both interfaces, so the code emitted for the two is the
/// same <c>PropertyObservable</c> / <c>PropertyChangingObservable</c> either way. The emission lives here
/// once so the two plugins stay in step.
/// </remarks>
internal static class NotifyPropertyEmitter
{
    /// <summary>Emits a shallow observation as an inline expression.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable holding the observed object.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="castTypeName">The type the observed object is cast to.</param>
    /// <param name="isBeforeChange">True for before-change observation.</param>
    /// <param name="includeStartWith">Whether the observation emits the current value on subscribe.</param>
    internal static void EmitShallowObservation(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        bool includeStartWith)
    {
        if (isBeforeChange)
        {
            _ = sb
                .Append($"new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{segment.PropertyTypeFullName}>((")
                .Append($"""global::System.ComponentModel.INotifyPropertyChanging){rootVar}, "{segment.PropertyName}", (""")
                .Append($"global::System.ComponentModel.INotifyPropertyChanging __o) => (({castTypeName})__o).{segment.PropertyName})");
            return;
        }

        _ = sb
            .Append($"""new global::ReactiveUI.Binding.Observables.PropertyObservable<{segment.PropertyTypeFullName}>({rootVar}, "{segment.PropertyName}", (""")
            .Append($"global::System.ComponentModel.INotifyPropertyChanged __o) => (({castTypeName})__o).{segment.PropertyName}, ")
            .Append(includeStartWith ? "true" : "false")
            .Append(')');
    }

    /// <summary>Emits a shallow observation assigned to a local.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable holding the observed object.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="castTypeName">The type the observed object is cast to.</param>
    /// <param name="isBeforeChange">True for before-change observation.</param>
    /// <param name="varName">The name of the local to assign.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void EmitShallowObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string varName) =>
        sb.Append(isBeforeChange
            ? $"""
                           var {varName} = new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{segment.PropertyTypeFullName}>(
                               (global::System.ComponentModel.INotifyPropertyChanging){rootVar},
                               "{segment.PropertyName}",
                               (global::System.ComponentModel.INotifyPropertyChanging __o) => (({castTypeName})__o).{segment.PropertyName});
               """
            : $"""
                           var {varName} = new global::ReactiveUI.Binding.Observables.PropertyObservable<{segment.PropertyTypeFullName}>(
                               {rootVar},
                               "{segment.PropertyName}",
                               (global::System.ComponentModel.INotifyPropertyChanged __o) => (({castTypeName})__o).{segment.PropertyName},
                               true);
               """);

    /// <summary>Emits the first stage of a deep observation chain.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable holding the observed object.</param>
    /// <param name="segment">The first property path segment.</param>
    /// <param name="castTypeName">The type the observed object is cast to.</param>
    /// <param name="isBeforeChange">True for before-change observation.</param>
    /// <param name="obsVarName">The name of the observable local to assign.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void EmitDeepChainRootSegment(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string obsVarName) =>
        sb.AppendLine(isBeforeChange
            ? $"""
            var {obsVarName} = (global::System.IObservable<{segment.PropertyTypeFullName}>)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{segment.PropertyTypeFullName}>(
                (global::System.ComponentModel.INotifyPropertyChanging){rootVar},
                "{segment.PropertyName}",
                (global::System.ComponentModel.INotifyPropertyChanging __o) => (({castTypeName})__o).{segment.PropertyName});
"""
            : $"""
                           var {obsVarName} = (global::System.IObservable<{segment.PropertyTypeFullName}>)new global::ReactiveUI.Binding.Observables.PropertyObservable<{segment.PropertyTypeFullName}>(
                               {rootVar},
                               "{segment.PropertyName}",
                               (global::System.ComponentModel.INotifyPropertyChanged __o) => (({castTypeName})__o).{segment.PropertyName},
                               false);
               """);

    /// <summary>Emits an inner stage of a deep observation chain.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="prevVar">The previous stage's observable.</param>
    /// <param name="curVar">The name of the observable local to assign.</param>
    /// <param name="lambdaParam">The lambda parameter holding the parent value.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="isBeforeChange">True for before-change observation.</param>
    /// <param name="nullParentBehavior">What this stage emits while its parent is null.</param>
    internal static void EmitDeepChainInnerSegment(
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
            ? $"new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(default({segType}))"
            : $"global::ReactiveUI.Binding.Observables.EmptyObservable<{segType}>.Instance";

        var stageOpen = GeneratedTypeNames.OpenChainSwitchMap(segment, segType, prevVar);

        _ = sb.AppendLine()
            .AppendLine(isBeforeChange
                ? $"""
                           var {curVar} = {stageOpen}
                               {lambdaParam} => {lambdaParam} != null
                                   ? (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{segType}>(
                                       (global::System.ComponentModel.INotifyPropertyChanging){lambdaParam},
                                       "{segment.PropertyName}",
                                       (global::System.ComponentModel.INotifyPropertyChanging __o) => (({segment.DeclaringTypeFullName})__o).{segment.PropertyName})
                                   : (global::System.IObservable<{segType}>){nullParentObservable});
                   """
                : $"""
                           var {curVar} = {stageOpen}
                               {lambdaParam} => {lambdaParam} != null
                                   ? (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.PropertyObservable<{segType}>(
                                       (global::System.ComponentModel.INotifyPropertyChanged){lambdaParam},
                                       "{segment.PropertyName}",
                                       (global::System.ComponentModel.INotifyPropertyChanged __o) => (({segment.DeclaringTypeFullName})__o).{segment.PropertyName},
                                       false)
                                   : (global::System.IObservable<{segType}>){nullParentObservable});
                   """);
    }

    /// <summary>Emits the inline observation a binding generator assigns to a local.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable holding the observed object.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="castTypeName">The type the observed object is cast to.</param>
    /// <param name="varName">The name of the local to assign.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void EmitInlineObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string varName) =>
        sb.AppendLine($"""
                               var {varName} = new global::ReactiveUI.Binding.Observables.PropertyObservable<{segment.PropertyTypeFullName}>(
                                   {rootVar},
                                   "{segment.PropertyName}",
                                   (global::System.ComponentModel.INotifyPropertyChanged __o) => (({castTypeName})__o).{segment.PropertyName},
                                   true);
                       """);
}

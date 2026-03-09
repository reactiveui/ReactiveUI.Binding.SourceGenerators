// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// Observation plugin for types implementing <see cref="System.ComponentModel.INotifyPropertyChanged"/>.
/// Affinity: 5 (matches ReactiveUI's INPCObservableForProperty).
/// Supports both after-change and before-change (if type also implements INotifyPropertyChanging).
/// Generates <c>PropertyObservable</c> / <c>PropertyChangingObservable</c> from the runtime library.
/// </summary>
internal sealed class INPCObservationPlugin : IObservationPlugin
{
    /// <inheritdoc/>
    public int Affinity => 5;

    /// <inheritdoc/>
    public string ObservationKind => "INPC";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => true;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => false;

    /// <inheritdoc/>
    public bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.ImplementsINPC && !classInfo.ImplementsIReactiveObject;

    /// <inheritdoc/>
    public void EmitHelperClasses(StringBuilder sb)
    {
        // No helper classes needed — uses PropertyObservable/PropertyChangingObservable from runtime library.
    }

    /// <inheritdoc/>
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
            sb.Append($"""new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{segment.PropertyTypeFullName}>((global::System.ComponentModel.INotifyPropertyChanging){rootVar}, "{segment.PropertyName}", (global::System.ComponentModel.INotifyPropertyChanging __o) => (({castTypeName})__o).{segment.PropertyName})""");
        }
        else
        {
            sb.Append($"""new global::ReactiveUI.Binding.Observables.PropertyObservable<{segment.PropertyTypeFullName}>({rootVar}, "{segment.PropertyName}", (global::System.ComponentModel.INotifyPropertyChanged __o) => (({castTypeName})__o).{segment.PropertyName}, {(includeStartWith ? "true" : "false")})""");
        }
    }

    /// <inheritdoc/>
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
            sb.Append($"""
                            var {varName} = new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{segment.PropertyTypeFullName}>(
                                (global::System.ComponentModel.INotifyPropertyChanging){rootVar},
                                "{segment.PropertyName}",
                                (global::System.ComponentModel.INotifyPropertyChanging __o) => (({castTypeName})__o).{segment.PropertyName});
                """);
        }
        else
        {
            sb.Append($"""
                            var {varName} = new global::ReactiveUI.Binding.Observables.PropertyObservable<{segment.PropertyTypeFullName}>(
                                {rootVar},
                                "{segment.PropertyName}",
                                (global::System.ComponentModel.INotifyPropertyChanged __o) => (({castTypeName})__o).{segment.PropertyName},
                                true);
                """);
        }
    }

    /// <inheritdoc/>
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
            sb.AppendLine($"""
                            var {obsVarName} = (global::System.IObservable<{segment.PropertyTypeFullName}>)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{segment.PropertyTypeFullName}>(
                                (global::System.ComponentModel.INotifyPropertyChanging){rootVar},
                                "{segment.PropertyName}",
                                (global::System.ComponentModel.INotifyPropertyChanging __o) => (({castTypeName})__o).{segment.PropertyName});
                """);
        }
        else
        {
            sb.AppendLine($"""
                            var {obsVarName} = (global::System.IObservable<{segment.PropertyTypeFullName}>)new global::ReactiveUI.Binding.Observables.PropertyObservable<{segment.PropertyTypeFullName}>(
                                {rootVar},
                                "{segment.PropertyName}",
                                (global::System.ComponentModel.INotifyPropertyChanged __o) => (({castTypeName})__o).{segment.PropertyName},
                                false);
                """);
        }
    }

    /// <inheritdoc/>
    public void EmitDeepChainInnerSegment(
        StringBuilder sb,
        string prevVar,
        string curVar,
        string lambdaParam,
        PropertyPathSegment segment,
        bool isBeforeChange)
    {
        var segType = segment.PropertyTypeFullName;

        if (isBeforeChange)
        {
            sb.AppendLine()
                .AppendLine($"""
                            var {curVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevVar},
                                    {lambdaParam} => {lambdaParam} != null
                                        ? (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<{segType}>(
                                            (global::System.ComponentModel.INotifyPropertyChanging){lambdaParam},
                                            "{segment.PropertyName}",
                                            (global::System.ComponentModel.INotifyPropertyChanging __o) => (({segment.DeclaringTypeFullName})__o).{segment.PropertyName})
                                        : (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(default({segType}))));
                    """);
        }
        else
        {
            sb.AppendLine()
                .AppendLine($"""
                            var {curVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevVar},
                                    {lambdaParam} => {lambdaParam} != null
                                        ? (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.PropertyObservable<{segType}>(
                                            {lambdaParam},
                                            "{segment.PropertyName}",
                                            (global::System.ComponentModel.INotifyPropertyChanged __o) => (({segment.DeclaringTypeFullName})__o).{segment.PropertyName},
                                            false)
                                        : (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(default({segType}))));
                    """);
        }
    }

    /// <inheritdoc/>
    public void EmitInlineObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string varName)
    {
        sb.AppendLine($"""
                    var {varName} = new global::ReactiveUI.Binding.Observables.PropertyObservable<{segment.PropertyTypeFullName}>(
                        {rootVar},
                        "{segment.PropertyName}",
                        (global::System.ComponentModel.INotifyPropertyChanged __o) => (({castTypeName})__o).{segment.PropertyName},
                        true);
            """);
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// Observation plugin for WinForms <c>Component</c> types.
/// Affinity: 8 (matches ReactiveUI's WinformsCreatesObservableForProperty).
/// Does NOT support before-change notifications.
/// Generates <c>EventObservable</c> with direct <c>{PropertyName}Changed</c> event subscription —
/// no reflection needed.
/// </summary>
/// <remarks>
/// <para>
/// WinForms uses the convention that observable properties have a corresponding
/// <c>{PropertyName}Changed</c> event with <see cref="System.EventHandler"/> signature.
/// Generated code subscribes directly to these events (e.g., <c>obj.TextChanged += handler</c>).
/// </para>
/// <para>
/// If a WinForms component does not have the expected <c>{PropertyName}Changed</c> event,
/// the generated code will produce a compile error in the user's project, clearly indicating
/// that the property cannot be observed via the WinForms event convention.
/// </para>
/// </remarks>
internal sealed class WinFormsObservationPlugin : IObservationPlugin
{
    /// <inheritdoc/>
    public int Affinity => 8;

    /// <inheritdoc/>
    public string ObservationKind => "WinForms";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => false;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => false;

    /// <inheritdoc/>
    public bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.InheritsWinFormsComponent;

    /// <inheritdoc/>
    public void EmitHelperClasses(StringBuilder sb)
    {
        // No helper classes needed — uses EventObservable<T> from runtime library.
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
            sb.Append($"new global::ReactiveUI.Binding.Observables.ReturnObservable<{segment.PropertyTypeFullName}>(default({segment.PropertyTypeFullName}))");
            return;
        }

        sb.Append($"new global::ReactiveUI.Binding.Observables.EventObservable<{segment.PropertyTypeFullName}>(")
            .Append($"__h => (({castTypeName}){rootVar}).{segment.PropertyName}Changed += __h, ")
            .Append($"__h => (({castTypeName}){rootVar}).{segment.PropertyName}Changed -= __h, ")
            .Append($"() => (({castTypeName}){rootVar}).{segment.PropertyName}, ")
            .Append(includeStartWith ? "true" : "false")
            .Append(')');
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
            sb.Append($"            var {varName} = new global::ReactiveUI.Binding.Observables.ReturnObservable<{segment.PropertyTypeFullName}>(default({segment.PropertyTypeFullName}));");
            return;
        }

        sb.Append($"""
                            var {varName} = new global::ReactiveUI.Binding.Observables.EventObservable<{segment.PropertyTypeFullName}>(
                                __h => (({castTypeName}){rootVar}).{segment.PropertyName}Changed += __h,
                                __h => (({castTypeName}){rootVar}).{segment.PropertyName}Changed -= __h,
                                () => (({castTypeName}){rootVar}).{segment.PropertyName},
                                true);
                """);
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
            sb.AppendLine($"            var {obsVarName} = (global::System.IObservable<{segment.PropertyTypeFullName}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segment.PropertyTypeFullName}>(default({segment.PropertyTypeFullName}));");
            return;
        }

        sb.AppendLine($"""
                            var {obsVarName} = (global::System.IObservable<{segment.PropertyTypeFullName}>)new global::ReactiveUI.Binding.Observables.EventObservable<{segment.PropertyTypeFullName}>(
                                __h => (({castTypeName}){rootVar}).{segment.PropertyName}Changed += __h,
                                __h => (({castTypeName}){rootVar}).{segment.PropertyName}Changed -= __h,
                                () => (({castTypeName}){rootVar}).{segment.PropertyName},
                                false);
                """);
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
        var declType = segment.DeclaringTypeFullName;

        if (isBeforeChange)
        {
            sb.AppendLine()
                .AppendLine($"""
                            var {curVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevVar},
                                    {lambdaParam} => (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(
                                        {lambdaParam} != null ? (({declType}){lambdaParam}).{segment.PropertyName} : default({segType}))));
                    """);
            return;
        }

        sb.AppendLine()
            .AppendLine($"""
                            var {curVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevVar},
                                    {lambdaParam} => {lambdaParam} != null
                                        ? (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.EventObservable<{segType}>(
                                            __h => (({declType}){lambdaParam}).{segment.PropertyName}Changed += __h,
                                            __h => (({declType}){lambdaParam}).{segment.PropertyName}Changed -= __h,
                                            () => (({declType}){lambdaParam}).{segment.PropertyName},
                                            false)
                                        : (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(default({segType}))));
                    """);
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
                    var {varName} = new global::ReactiveUI.Binding.Observables.EventObservable<{segment.PropertyTypeFullName}>(
                        __h => (({castTypeName}){rootVar}).{segment.PropertyName}Changed += __h,
                        __h => (({castTypeName}){rootVar}).{segment.PropertyName}Changed -= __h,
                        () => (({castTypeName}){rootVar}).{segment.PropertyName},
                        true);
            """);
    }
}

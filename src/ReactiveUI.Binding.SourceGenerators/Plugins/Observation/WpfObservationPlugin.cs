// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// Observation plugin for WPF <c>DependencyObject</c> types.
/// Affinity: 4 (matches ReactiveUI's DependencyObjectObservableForProperty).
/// Does NOT support before-change notifications (DependencyProperties have no before-change event).
/// Generates <c>EventObservable</c> with <c>DependencyPropertyDescriptor.AddValueChanged</c> —
/// direct static field access, no reflection.
/// </summary>
/// <remarks>
/// <para>
/// WPF DependencyProperties use the naming convention <c>{PropertyName}Property</c> for the
/// static <c>DependencyProperty</c> field. Generated code accesses this field directly
/// (e.g., <c>global::MyApp.MyControl.TextProperty</c>) instead of using reflection.
/// </para>
/// <para>
/// <c>DependencyPropertyDescriptor.FromProperty(dp, type).AddValueChanged(obj, handler)</c>
/// uses <see cref="System.EventHandler"/>, which is compatible with <c>EventObservable</c>
/// from the runtime library.
/// </para>
/// </remarks>
internal sealed class WpfObservationPlugin : IObservationPlugin
{
    /// <inheritdoc/>
    public int Affinity => 4;

    /// <inheritdoc/>
    public string ObservationKind => "WpfDP";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => false;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => false;

    /// <inheritdoc/>
    public bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.InheritsWpfDependencyObject;

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
        // WPF DP does not support before-change; caller should not reach here with isBeforeChange=true
        // but we emit ReturnObservable as a safe fallback.
        if (isBeforeChange)
        {
            sb.Append($"new global::ReactiveUI.Binding.Observables.ReturnObservable<{segment.PropertyTypeFullName}>(default({segment.PropertyTypeFullName}))");
            return;
        }

        sb.Append($"new global::ReactiveUI.Binding.Observables.EventObservable<{segment.PropertyTypeFullName}>(")
            .Append($"__h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty({castTypeName}.{segment.PropertyName}Property, typeof({castTypeName})).AddValueChanged({rootVar}, __h), ")
            .Append($"__h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty({castTypeName}.{segment.PropertyName}Property, typeof({castTypeName})).RemoveValueChanged({rootVar}, __h), ")
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
                                __h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(
                                    {castTypeName}.{segment.PropertyName}Property, typeof({castTypeName})).AddValueChanged({rootVar}, __h),
                                __h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(
                                    {castTypeName}.{segment.PropertyName}Property, typeof({castTypeName})).RemoveValueChanged({rootVar}, __h),
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
                                __h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(
                                    {castTypeName}.{segment.PropertyName}Property, typeof({castTypeName})).AddValueChanged({rootVar}, __h),
                                __h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(
                                    {castTypeName}.{segment.PropertyName}Property, typeof({castTypeName})).RemoveValueChanged({rootVar}, __h),
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
            // WPF DP does not support before-change; emit ReturnObservable for inner segments too
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
                                            __h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(
                                                {declType}.{segment.PropertyName}Property, typeof({declType})).AddValueChanged({lambdaParam}, __h),
                                            __h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(
                                                {declType}.{segment.PropertyName}Property, typeof({declType})).RemoveValueChanged({lambdaParam}, __h),
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
                        __h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(
                            {castTypeName}.{segment.PropertyName}Property, typeof({castTypeName})).AddValueChanged({rootVar}, __h),
                        __h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(
                            {castTypeName}.{segment.PropertyName}Property, typeof({castTypeName})).RemoveValueChanged({rootVar}, __h),
                        () => (({castTypeName}){rootVar}).{segment.PropertyName},
                        true);
            """);
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
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
/// uses <see cref="EventHandler"/>, which is compatible with <c>EventObservable</c>
/// from the runtime library.
/// </para>
/// </remarks>
internal sealed class WpfObservationPlugin : IObservationPlugin
{
    /// <summary>Opens the descriptor lookup the handler is added to or removed from.</summary>
    private const string DescriptorLookupOpen = "                __h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(";

    /// <summary>Closes the descriptor lookup and subscribes the generated handler.</summary>
    private const string AddValueChangedCall = ")).AddValueChanged(";

    /// <summary>Closes the descriptor lookup and unsubscribes the generated handler.</summary>
    private const string RemoveValueChangedCall = ")).RemoveValueChanged(";

    /// <summary>Passes the generated handler and closes the add or remove lambda.</summary>
    private const string HandlerArgumentClose = ", __h),";

    /// <summary>Completes the dependency property field name and opens its owner type.</summary>
    private const string DependencyPropertyOwnerOpen = "Property, typeof(";

    /// <summary>
    /// The affinity score for the WPF DependencyObject observation plugin
    /// (matches ReactiveUI's DependencyObjectObservableForProperty).
    /// </summary>
    private static readonly int WpfAffinity = BindingAffinity.WpfDependencyObject;

    /// <inheritdoc/>
    public int Affinity => WpfAffinity;

    /// <inheritdoc/>
    public string ObservationKind => "WpfDP";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => false;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => false;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.InheritsWpfDependencyObject;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) =>
        ObservedProperties.IsDependencyProperty(classInfo, propertyName);

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
        // but we emit ImmediateReturnSignal as a safe fallback.
        if (isBeforeChange)
        {
            _ = UnchangingObservationEmitter.AppendExpression(sb, rootVar, segment, castTypeName);
            return;
        }

        _ = sb.Append("new global::ReactiveUI.Binding.Observables.EventObservable<").Append(segment.PropertyTypeFullName).Append(">(")
            .Append("__h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(").Append(castTypeName).Append('.')
            .Append(segment.PropertyName).Append("Property,").Append(" typeof(").Append(castTypeName).Append(AddValueChangedCall).Append(rootVar)
            .Append(", __h), ").Append("__h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(").Append(castTypeName).Append('.')
            .Append(segment.PropertyName).Append("Property,").Append(" typeof(").Append(castTypeName).Append(RemoveValueChangedCall).Append(rootVar)
            .Append(", __h), ").Append("() => ((").Append(castTypeName).Append(')').Append(rootVar).Append(").").Append(segment.PropertyName).Append(", ")
            .Append(includeStartWith ? "true" : "false").Append(')');
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
            _ = UnchangingObservationEmitter.AppendVariable(sb, rootVar, segment, castTypeName, varName);
            return;
        }

        _ = sb.Append("            var ").Append(varName).Append(" = new global::ReactiveUI.Binding.Observables.EventObservable<")
            .Append(segment.PropertyTypeFullName).AppendLine(">(")
            .AppendLine(DescriptorLookupOpen).Append("                    ")
            .Append(castTypeName).Append('.').Append(segment.PropertyName).Append(DependencyPropertyOwnerOpen).Append(castTypeName).Append(AddValueChangedCall)
            .Append(rootVar).AppendLine(HandlerArgumentClose)
            .AppendLine(DescriptorLookupOpen).Append("                    ")
            .Append(castTypeName).Append('.').Append(segment.PropertyName).Append(DependencyPropertyOwnerOpen).Append(castTypeName)
            .Append(RemoveValueChangedCall).Append(rootVar).AppendLine(HandlerArgumentClose).Append("                () => ((").Append(castTypeName).Append(')')
            .Append(rootVar).Append(").").Append(segment.PropertyName).AppendLine(",").Append("                true);");
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
            _ = UnchangingObservationEmitter.AppendTypedVariable(sb, rootVar, segment, castTypeName, obsVarName)
                .AppendLine();
            return;
        }

        _ = sb.Append("            var ").Append(obsVarName).Append(" = (global::System.IObservable<").Append(segment.PropertyTypeFullName)
            .Append(">)new global::ReactiveUI.Binding.Observables.EventObservable<").Append(segment.PropertyTypeFullName).AppendLine(">(")
            .AppendLine(DescriptorLookupOpen).Append("                    ")
            .Append(castTypeName).Append('.').Append(segment.PropertyName).Append(DependencyPropertyOwnerOpen).Append(castTypeName).Append(AddValueChangedCall)
            .Append(rootVar).AppendLine(HandlerArgumentClose)
            .AppendLine(DescriptorLookupOpen).Append("                    ")
            .Append(castTypeName).Append('.').Append(segment.PropertyName).Append(DependencyPropertyOwnerOpen).Append(castTypeName)
            .Append(RemoveValueChangedCall).Append(rootVar).AppendLine(HandlerArgumentClose).Append("                () => ((").Append(castTypeName).Append(')')
            .Append(rootVar).Append(").").Append(segment.PropertyName).AppendLine(",").AppendLine("                false);");
    }

    /// <inheritdoc/>
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
        var declType = segment.DeclaringTypeFullName;
        var nullParentObservable = nullParentBehavior == NullParentObservationBehavior.EmitDefault
            ? $"new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{segType}>(default({segType}))"
            : $"global::ReactiveUI.Primitives.Advanced.ImmutableEmptySignal<{segType}>.Instance";

        if (isBeforeChange)
        {
            // WPF DP does not support before-change; emit ImmediateReturnSignal for inner segments too
            _ = sb.AppendLine();
            _ = sb.Append(GeneratedSyntax.InlineLocalDeclaration).Append(curVar).Append(" = ")
                .Append(GeneratedTypeNames.OpenChainSwitchMap(segment, segType, prevVar))
                .AppendLine().Append("            ").Append(lambdaParam).Append(" => ").Append(lambdaParam).AppendLine(" != null")
                .Append("                ? (global::System.IObservable<").Append(segType).AppendLine(">)")
                .Append("                    new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<").Append(segType).Append(">(((").Append(declType)
                .Append(')').Append(lambdaParam).Append(").").Append(segment.PropertyName).AppendLine(")").Append("                : (global::System.IObservable<")
                .Append(segType).Append(">)").Append(nullParentObservable).AppendLine(");");
            return;
        }

        _ = sb.AppendLine().Append("        var ").Append(curVar).Append(" = ")
            .Append(GeneratedTypeNames.OpenChainSwitchMap(segment, segType, prevVar)).AppendLine().Append("            ").Append(lambdaParam)
            .Append(" => ").Append(lambdaParam).AppendLine(" != null").Append("                ? (global::System.IObservable<").Append(segType)
            .Append(">)new global::ReactiveUI.Binding.Observables.EventObservable<").Append(segType).AppendLine(">(")
            .AppendLine("                    __h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(")
            .Append("                        ").Append(declType).Append('.').Append(segment.PropertyName).Append(DependencyPropertyOwnerOpen).Append(declType)
            .Append(AddValueChangedCall).Append(lambdaParam).AppendLine(HandlerArgumentClose)
            .AppendLine("                    __h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(")
            .Append("                        ").Append(declType).Append('.').Append(segment.PropertyName).Append(DependencyPropertyOwnerOpen).Append(declType)
            .Append(RemoveValueChangedCall).Append(lambdaParam).AppendLine(HandlerArgumentClose).Append("                    () => ((").Append(declType)
            .Append(')').Append(lambdaParam).Append(").").Append(segment.PropertyName).AppendLine(",").AppendLine("                    false)")
            .Append("                : (global::System.IObservable<").Append(segType).Append(">)").Append(nullParentObservable).AppendLine(");");
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitInlineObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string varName) =>
        sb.Append("        var ").Append(varName).Append(" = new global::ReactiveUI.Binding.Observables.EventObservable<")
            .Append(segment.PropertyTypeFullName).AppendLine(">(")
            .AppendLine("            __h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(").Append("                ")
            .Append(castTypeName).Append('.').Append(segment.PropertyName).Append("Property, typeof(").Append(castTypeName)
            .Append(")).AddValueChanged(").Append(rootVar).AppendLine(", __h),")
            .AppendLine("            __h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(").Append("                ")
            .Append(castTypeName).Append('.').Append(segment.PropertyName).Append("Property, typeof(").Append(castTypeName)
            .Append(")).RemoveValueChanged(").Append(rootVar).AppendLine(", __h),").Append("            () => ((").Append(castTypeName).Append(')')
            .Append(rootVar).Append(").").Append(segment.PropertyName).AppendLine(",").AppendLine("            true);");
}

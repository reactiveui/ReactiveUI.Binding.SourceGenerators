// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
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
/// <c>{PropertyName}Changed</c> event with <see cref="EventHandler"/> signature.
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
    /// <summary>Opens the lambda that adds or removes the generated event handler.</summary>
    private const string HandlerLambdaOpen = "                __h => ((";

    /// <summary>Completes the event name and subscribes the generated handler.</summary>
    private const string ChangedEventAdd = "Changed += __h,";

    /// <summary>Completes the event name and unsubscribes the generated handler.</summary>
    private const string ChangedEventRemove = "Changed -= __h,";

    /// <summary>
    /// The affinity score for the WinForms Component observation plugin
    /// (matches ReactiveUI's WinformsCreatesObservableForProperty).
    /// </summary>
    private static readonly int WinFormsAffinity = BindingAffinity.WinFormsEvent;

    /// <inheritdoc/>
    public int Affinity => WinFormsAffinity;

    /// <inheritdoc/>
    public string ObservationKind => "WinForms";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => false;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => false;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.InheritsWinFormsComponent;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) =>
        ObservedProperties.HasChangeEvent(classInfo, propertyName);

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
            _ = UnchangingObservationEmitter.AppendExpression(sb, rootVar, segment, castTypeName);
            return;
        }

        _ = sb.Append("new global::ReactiveUI.Binding.Observables.EventObservable<").Append(segment.PropertyTypeFullName).Append(">(")
            .Append("__h => ((").Append(castTypeName).Append(')').Append(rootVar).Append(").").Append(segment.PropertyName).Append("Changed += __h, ")
            .Append("__h => ((").Append(castTypeName).Append(')').Append(rootVar).Append(").").Append(segment.PropertyName).Append("Changed -= __h, ")
            .Append("() => ((").Append(castTypeName).Append(')').Append(rootVar).Append(").").Append(segment.PropertyName).Append(", ")
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
            .Append(segment.PropertyTypeFullName).AppendLine(">(").Append(HandlerLambdaOpen).Append(castTypeName).Append(')').Append(rootVar)
            .Append(").").Append(segment.PropertyName).AppendLine(ChangedEventAdd).Append(HandlerLambdaOpen).Append(castTypeName).Append(')')
            .Append(rootVar).Append(").").Append(segment.PropertyName).AppendLine(ChangedEventRemove).Append("                () => ((")
            .Append(castTypeName).Append(')').Append(rootVar).Append(").").Append(segment.PropertyName).AppendLine(",").Append("                true);");
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
            .Append(HandlerLambdaOpen).Append(castTypeName).Append(')').Append(rootVar).Append(").").Append(segment.PropertyName)
            .AppendLine(ChangedEventAdd).Append(HandlerLambdaOpen).Append(castTypeName).Append(')').Append(rootVar).Append(").")
            .Append(segment.PropertyName).AppendLine(ChangedEventRemove).Append("                () => ((").Append(castTypeName).Append(')')
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
            _ = sb.AppendLine().Append(GeneratedSyntax.InlineLocalDeclaration).Append(curVar).Append(" = ")
                .Append(GeneratedTypeNames.OpenChainSwitchMap(segment, segType, prevVar)).AppendLine().Append("            ").Append(lambdaParam)
                .Append(" => ").Append(lambdaParam).AppendLine(" != null").Append("                ? (global::System.IObservable<").Append(segType)
                .AppendLine(">)").Append("                    new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<").Append(segType)
                .Append(">(((").Append(declType).Append(')').Append(lambdaParam).Append(").").Append(segment.PropertyName).AppendLine(")")
                .Append("                : (global::System.IObservable<").Append(segType).Append(">)").Append(nullParentObservable).AppendLine(");");
            return;
        }

        _ = sb.AppendLine().Append(GeneratedSyntax.InlineLocalDeclaration).Append(curVar).Append(" = ")
            .Append(GeneratedTypeNames.OpenChainSwitchMap(segment, segType, prevVar)).AppendLine().Append("            ").Append(lambdaParam)
            .Append(" => ").Append(lambdaParam).AppendLine(" != null").Append("                ? (global::System.IObservable<").Append(segType)
            .Append(">)new global::ReactiveUI.Binding.Observables.EventObservable<").Append(segType).AppendLine(">(")
            .Append("                    __h => ((").Append(declType).Append(')').Append(lambdaParam).Append(").").Append(segment.PropertyName)
            .AppendLine(ChangedEventAdd).Append("                    __h => ((").Append(declType).Append(')').Append(lambdaParam).Append(").")
            .Append(segment.PropertyName).AppendLine(ChangedEventRemove).Append("                    () => ((").Append(declType).Append(')')
            .Append(lambdaParam).Append(").").Append(segment.PropertyName).AppendLine(",").AppendLine("                    false)")
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
        sb.Append(GeneratedSyntax.InlineLocalDeclaration).Append(varName).Append(" = new global::ReactiveUI.Binding.Observables.EventObservable<")
            .Append(segment.PropertyTypeFullName).AppendLine(">(").Append("            __h => ((").Append(castTypeName).Append(')').Append(rootVar)
            .Append(").").Append(segment.PropertyName).AppendLine("Changed += __h,").Append("            __h => ((").Append(castTypeName).Append(')')
            .Append(rootVar).Append(").").Append(segment.PropertyName).AppendLine("Changed -= __h,").Append("            () => ((")
            .Append(castTypeName).Append(')').Append(rootVar).Append(").").Append(segment.PropertyName).AppendLine(",")
            .AppendLine("            true);");
}

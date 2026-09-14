// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Observes WinForms components through their <c>{PropertyName}Changed</c> events.</summary>
internal sealed class WinFormsObservationPlugin : AfterChangeObservationPlugin
{
    /// <summary>Opens the lambda that adds or removes the generated event handler.</summary>
    private const string HandlerLambdaOpen = "                __h => ((";

    /// <summary>Completes the event name and subscribes the generated handler.</summary>
    private const string ChangedEventAdd = "Changed += __h,";

    /// <summary>Completes the event name and unsubscribes the generated handler.</summary>
    private const string ChangedEventRemove = "Changed -= __h,";

    /// <summary>The affinity this plugin bids with.</summary>
    private static readonly int WinFormsAffinity = BindingAffinity.WinFormsEvent;

    /// <inheritdoc/>
    public override int Affinity => WinFormsAffinity;

    /// <inheritdoc/>
    public override string ObservationKind => "WinForms";

    /// <inheritdoc/>
    public override bool RequiresHelperClasses => false;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.InheritsWinFormsComponent;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) =>
        ObservedProperties.HasChangeEvent(classInfo, propertyName);

    /// <inheritdoc/>
    public override void EmitHelperClasses(StringBuilder sb)
    {
        // No helper classes needed — uses EventObservable<T> from runtime library.
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void EmitInlineObservationVariable(
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

    /// <inheritdoc/>
    protected override void AppendShallowObservation(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool includeStartWith) =>
        _ = sb.Append("new global::ReactiveUI.Binding.Observables.EventObservable<").Append(segment.PropertyTypeFullName).Append(">(")
            .Append("__h => ((").Append(castTypeName).Append(')').Append(rootVar).Append(").").Append(segment.PropertyName).Append("Changed += __h, ")
            .Append("__h => ((").Append(castTypeName).Append(')').Append(rootVar).Append(").").Append(segment.PropertyName).Append("Changed -= __h, ")
            .Append("() => ((").Append(castTypeName).Append(')').Append(rootVar).Append(").").Append(segment.PropertyName).Append(", ")
            .Append(includeStartWith ? "true" : "false").Append(')');

    /// <inheritdoc/>
    protected override void AppendShallowObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string varName) =>
        _ = sb.Append("            var ").Append(varName).Append(" = new global::ReactiveUI.Binding.Observables.EventObservable<")
            .Append(segment.PropertyTypeFullName).AppendLine(">(").Append(HandlerLambdaOpen).Append(castTypeName).Append(')').Append(rootVar)
            .Append(").").Append(segment.PropertyName).AppendLine(ChangedEventAdd).Append(HandlerLambdaOpen).Append(castTypeName).Append(')')
            .Append(rootVar).Append(").").Append(segment.PropertyName).AppendLine(ChangedEventRemove).Append("                () => ((")
            .Append(castTypeName).Append(')').Append(rootVar).Append(").").Append(segment.PropertyName).AppendLine(",").Append("                true);");

    /// <inheritdoc/>
    protected override void AppendChainSegmentObservation(
        StringBuilder sb,
        string lambdaParam,
        PropertyPathSegment segment) =>
        _ = sb.Append("                    new global::ReactiveUI.Binding.Observables.EventObservable<").Append(segment.PropertyTypeFullName).AppendLine(">(")
            .Append("                    __h => ((").Append(segment.DeclaringTypeFullName).Append(')').Append(lambdaParam).Append(").")
            .Append(segment.PropertyName).AppendLine(ChangedEventAdd).Append("                    __h => ((").Append(segment.DeclaringTypeFullName)
            .Append(')').Append(lambdaParam).Append(").").Append(segment.PropertyName).AppendLine(ChangedEventRemove)
            .Append("                    () => ((").Append(segment.DeclaringTypeFullName).Append(')').Append(lambdaParam).Append(").")
            .Append(segment.PropertyName).AppendLine(",").Append("                    false)");

    /// <inheritdoc/>
    protected override void AppendDeepChainRootSegment(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string obsVarName) =>
        _ = sb.Append("            var ").Append(obsVarName).Append(" = (global::System.IObservable<").Append(segment.PropertyTypeFullName)
            .Append("                    new global::ReactiveUI.Binding.Observables.EventObservable<").Append(segment.PropertyTypeFullName).AppendLine(">(")
            .Append(HandlerLambdaOpen).Append(castTypeName).Append(')').Append(rootVar).Append(").").Append(segment.PropertyName)
            .AppendLine(ChangedEventAdd).Append(HandlerLambdaOpen).Append(castTypeName).Append(')').Append(rootVar).Append(").")
            .Append(segment.PropertyName).AppendLine(ChangedEventRemove).Append("                () => ((").Append(castTypeName).Append(')')
            .Append(rootVar).Append(").").Append(segment.PropertyName).AppendLine(",").AppendLine("                false);");
}

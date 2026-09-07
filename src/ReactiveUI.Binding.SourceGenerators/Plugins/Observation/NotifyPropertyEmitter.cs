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
    /// <summary>Opens a before-change observation of the object a call site named.</summary>
    private const string ChangingObservableOpen =
        " = new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<";

    /// <summary>Opens an after-change observation of the object a call site named.</summary>
    private const string ChangedObservableOpen =
        " = new global::ReactiveUI.Binding.Observables.PropertyObservable<";

    /// <summary>Opens the lambda that reads a property back off a before-change notification.</summary>
    private const string ChangingReaderLambdaOpen =
        "                (global::System.ComponentModel.INotifyPropertyChanging __o) => ";

    /// <summary>Opens the lambda that reads a property back off an after-change notification.</summary>
    private const string ChangedReaderLambdaOpen =
        "                (global::System.ComponentModel.INotifyPropertyChanged __o) => ";

    /// <summary>Passes the observed object to a before-change observation.</summary>
    private const string ChangingSourceArgumentOpen =
        "                (global::System.ComponentModel.INotifyPropertyChanging)";

    /// <summary>Opens the branch a chain stage takes while its parent is present.</summary>
    private const string ParentPresentBranchOpen = "                ? (global::System.IObservable<";

    /// <summary>Opens the branch a chain stage takes while its parent is null.</summary>
    private const string ParentMissingBranchOpen = "                : (global::System.IObservable<";

    /// <summary>Opens the quoted property-name argument of a chain stage's observation.</summary>
    private const string StageQuotedArgumentOpen = "                    \"";

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
            _ = sb.Append("new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<").Append(segment.PropertyTypeFullName).Append(">((")
                .Append("global::System.ComponentModel.INotifyPropertyChanging)").Append(rootVar).Append(", \"").Append(segment.PropertyName)
                .Append("\", (").Append("global::System.ComponentModel.INotifyPropertyChanging __o) => ((").Append(castTypeName)
                .Append(GeneratedSyntax.ObserverCastClose).Append(segment.PropertyName).Append(')');
            return;
        }

        _ = sb.Append("new global::ReactiveUI.Binding.Observables.PropertyObservable<").Append(segment.PropertyTypeFullName).Append(">(")
            .Append(rootVar).Append(", \"").Append(segment.PropertyName).Append("\", (")
            .Append("global::System.ComponentModel.INotifyPropertyChanged __o) => ((").Append(castTypeName)
            .Append(GeneratedSyntax.ObserverCastClose).Append(segment.PropertyName)
            .Append(", ").Append(includeStartWith ? "true" : "false").Append(')');
    }

    /// <summary>Emits a shallow observation assigned to a local.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable holding the observed object.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="castTypeName">The type the observed object is cast to.</param>
    /// <param name="isBeforeChange">True for before-change observation.</param>
    /// <param name="varName">The name of the local to assign.</param>
    internal static void EmitShallowObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string varName)
    {
        if (isBeforeChange)
        {
            _ = sb.Append(GeneratedSyntax.BodyLocalDeclaration).Append(varName).Append(ChangingObservableOpen)
                .Append(segment.PropertyTypeFullName).AppendLine(">(").Append(ChangingSourceArgumentOpen)
                .Append(rootVar).AppendLine(",").Append(GeneratedSyntax.QuotedArgumentOpen).Append(segment.PropertyName).AppendLine("\",")
                .Append(ChangingReaderLambdaOpen).Append("((").Append(castTypeName)
                .Append(GeneratedSyntax.ObserverCastClose).Append(segment.PropertyName).Append(");");
            return;
        }

        _ = sb.Append(GeneratedSyntax.BodyLocalDeclaration).Append(varName).Append(ChangedObservableOpen)
            .Append(segment.PropertyTypeFullName).AppendLine(">(").Append("                ").Append(rootVar).AppendLine(",")
            .Append(GeneratedSyntax.QuotedArgumentOpen).Append(segment.PropertyName).AppendLine("\",")
            .Append(ChangedReaderLambdaOpen).Append("((").Append(castTypeName)
            .Append(GeneratedSyntax.ObserverCastClose).Append(segment.PropertyName).AppendLine(",").Append("                true);");
    }

    /// <summary>Emits the first stage of a deep observation chain.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The variable holding the observed object.</param>
    /// <param name="segment">The first property path segment.</param>
    /// <param name="castTypeName">The type the observed object is cast to.</param>
    /// <param name="isBeforeChange">True for before-change observation.</param>
    /// <param name="obsVarName">The name of the observable local to assign.</param>
    internal static void EmitDeepChainRootSegment(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string obsVarName)
    {
        var read = GeneratedTypeNames.ReadProperty(segment, castTypeName, "__o");

        if (isBeforeChange)
        {
            _ = sb.Append(GeneratedSyntax.BodyLocalDeclaration).Append(obsVarName).Append(" = (global::System.IObservable<")
                .Append(segment.PropertyTypeFullName).Append(">)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<")
                .Append(segment.PropertyTypeFullName).AppendLine(">(").Append(ChangingSourceArgumentOpen).Append(rootVar).AppendLine(",")
                .Append(GeneratedSyntax.QuotedArgumentOpen).Append(segment.PropertyName).AppendLine("\",")
                .Append(ChangingReaderLambdaOpen).Append(read).AppendLine(");");
            return;
        }

        _ = sb.Append(GeneratedSyntax.BodyLocalDeclaration).Append(obsVarName).Append(" = (global::System.IObservable<")
            .Append(segment.PropertyTypeFullName).Append(">)new global::ReactiveUI.Binding.Observables.PropertyObservable<")
            .Append(segment.PropertyTypeFullName).AppendLine(">(").Append("                ").Append(rootVar).AppendLine(",")
            .Append(GeneratedSyntax.QuotedArgumentOpen).Append(segment.PropertyName).AppendLine("\",")
            .Append(ChangedReaderLambdaOpen).Append(read).AppendLine(",").AppendLine("                false);");
    }

    /// <summary>Emits an inner stage of a deep observation chain.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="stage">The locals this stage reads from and writes to.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="isBeforeChange">True for before-change observation.</param>
    /// <param name="nullParentBehavior">What this stage emits while its parent is null.</param>
    /// <param name="generatedAffinity">The affinity of the mechanism this stage was built from.</param>
    /// <remarks>
    /// The stage offers its observation to a registration that outranks the mechanism, which is how a chain
    /// honours one link at a time. The registration decides only when the property changed - the value is read
    /// with the accessor emitted here either way - so the link resolves nothing by name.
    /// </remarks>
    internal static void EmitDeepChainInnerSegment(
        StringBuilder sb,
        in ChainStageVariables stage,
        PropertyPathSegment segment,
        bool isBeforeChange,
        NullParentObservationBehavior nullParentBehavior,
        int generatedAffinity)
    {
        var (prevVar, curVar, lambdaParam) = stage;
        var segType = segment.PropertyTypeFullName;
        var nullParentObservable = nullParentBehavior == NullParentObservationBehavior.EmitDefault
            ? $"new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{segType}>(default({segType}))"
            : $"global::ReactiveUI.Primitives.Advanced.ImmutableEmptySignal<{segType}>.Instance";

        _ = sb.AppendLine();
        AppendStageOpening(sb, curVar, lambdaParam, GeneratedTypeNames.OpenChainSwitchMap(segment, segType, prevVar));

        if (isBeforeChange)
        {
            _ = sb.Append(ParentPresentBranchOpen).Append(segType)
                .Append(">)new global::ReactiveUI.Binding.Observables.PropertyChangingObservable<").Append(segType).AppendLine(">(")
                .Append("                    (global::System.ComponentModel.INotifyPropertyChanging)").Append(lambdaParam).AppendLine(",")
                .Append(StageQuotedArgumentOpen).Append(segment.PropertyName).AppendLine("\",")
                .Append("                    (global::System.ComponentModel.INotifyPropertyChanging __o) => ((").Append(segment.DeclaringTypeFullName)
                .Append(GeneratedSyntax.ObserverCastClose).Append(segment.PropertyName).AppendLine(")");
        }
        else
        {
            var declaringType = segment.DeclaringTypeFullName;

            _ = sb.Append("                ? global::ReactiveUI.Binding.Observables.PluginObservationSource.Choose<").Append(segType).AppendLine(">(")
                .Append("                    ").Append(lambdaParam).AppendLine(",")
                .Append("                    ((global::System.Linq.Expressions.Expression<global::System.Func<").Append(declaringType).Append(", ")
                .Append(segType).Append(">>)(__e => __e.").Append(segment.PropertyName).AppendLine(")).Body,")
                .Append(StageQuotedArgumentOpen).Append(segment.PropertyName).AppendLine("\",")
                .AppendLine("                    false,")
                .Append("                    ").Append(generatedAffinity).AppendLine(",")
                .Append("                    (object __o) => ((").Append(declaringType).Append(")__o).").Append(segment.PropertyName).AppendLine(",")
                .Append("                    new global::ReactiveUI.Binding.Observables.PropertyObservable<").Append(segType).AppendLine(">(")
                .Append("                        (global::System.ComponentModel.INotifyPropertyChanged)").Append(lambdaParam).AppendLine(",")
                .Append("                        \"").Append(segment.PropertyName).AppendLine("\",")
                .Append("                        (global::System.ComponentModel.INotifyPropertyChanged __o) => ((").Append(declaringType)
                .Append(GeneratedSyntax.ObserverCastClose).Append(segment.PropertyName).AppendLine(",").AppendLine("                        false))");
        }

        _ = sb.Append(ParentMissingBranchOpen).Append(segType).Append(">)").Append(nullParentObservable).AppendLine(");");
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
        sb.Append(GeneratedSyntax.InlineLocalDeclaration).Append(varName).Append(ChangedObservableOpen)
            .Append(segment.PropertyTypeFullName).AppendLine(">(").Append("            ").Append(rootVar).AppendLine(",").Append("            \"")
            .Append(segment.PropertyName).AppendLine("\",").Append("            (global::System.ComponentModel.INotifyPropertyChanged __o) => ((")
            .Append(castTypeName).Append(GeneratedSyntax.ObserverCastClose).Append(segment.PropertyName).AppendLine(",").AppendLine("            true);");

    /// <summary>Appends the switch-map stage a chain link is observed through, up to its parent test.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="curVar">The name of the observable local to assign.</param>
    /// <param name="lambdaParam">The lambda parameter holding the parent value.</param>
    /// <param name="stageOpen">The opening of the switch-map the stage is built from.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendStageOpening(StringBuilder sb, string curVar, string lambdaParam, string stageOpen) =>
        sb.Append(GeneratedSyntax.InlineLocalDeclaration).Append(curVar).Append(" = ").Append(stageOpen).AppendLine()
            .Append("            ").Append(lambdaParam).Append(" => ").Append(lambdaParam).AppendLine(" != null");
}

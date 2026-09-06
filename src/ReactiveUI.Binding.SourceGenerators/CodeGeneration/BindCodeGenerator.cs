// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;
using static ReactiveUI.Binding.SourceGenerators.CodeGeneration.GeneratedTypeNames;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads and binding methods for Bind (view-first two-way) invocations.
/// The generated methods return <c>IReactiveBinding&lt;TView, BindingChange&gt;</c> and use view-first parameter ordering.
/// </summary>
internal static class BindCodeGenerator
{
    /// <summary>The indentation a statement inside the emitted subscription body sits at.</summary>
    private const string SubscriptionBodyIndent = "                ";

    /// <summary>What this API calls the view-model-to-view converter in its generated signatures.</summary>
    private const string ForwardConverterName = "viewModelToViewConverter";

    /// <summary>What this API calls the view-to-view-model converter in its generated signatures.</summary>
    private const string ReverseConverterName = "viewToViewModelConverter";

    /// <summary>Name of the generated local holding the view model side observable.</summary>
    private const string ViewModelObservableName = "vmObs";

    /// <summary>Name of the generated local holding the view side observable.</summary>
    private const string ViewObservableName = "viewObs";

    /// <summary>Groups Bind invocations by their type signature for overload generation.</summary>
    /// <param name="invocations">The Bind invocations to group.</param>
    /// <returns>A list of grouped invocations sharing the same type signature.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static List<BindingTypeGroup> GroupByTypeSignature(ImmutableArray<BindingInvocationInfo> invocations) =>
        BindingEmitterHelpers.GroupByTypeSignature(invocations);

    /// <summary>Generates the concrete typed overload using the appropriate dispatch strategy.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="supportsCallerArgExpr">Whether CallerArgumentExpression is available.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateConcreteOverload(
        StringBuilder sb,
        BindingTypeGroup group,
        bool supportsCallerArgExpr,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        if (supportsCallerArgExpr)
        {
            GenerateCallerArgExprOverload(sb, group, supportsNullable);
        }
        else
        {
            GenerateCallerFilePathOverload(sb, group, supportsNullable, stubHasExpressionParameters);
        }
    }

    /// <summary>Generates the CallerArgumentExpression-based overload for Bind dispatch.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void GenerateCallerArgExprOverload(
        StringBuilder sb,
        BindingTypeGroup group,
        bool supportsNullable)
    {
        var sourcePropType = CodeGeneratorHelpers.NullableSelectorLeafType(group.Invocations[0].SourcePropertyPath, supportsNullable);
        var targetPropType = CodeGeneratorHelpers.NullableSelectorLeafType(group.Invocations[0].TargetPropertyPath, supportsNullable);
        var returnType = FormatReturnType(group);

        _ = sb.AppendLine($"""
                               /// <summary>
                               /// Concrete typed overload for Bind from {group.SourceTypeFullName} to {group.TargetTypeFullName}.
                               /// Uses CallerArgumentExpression for dispatch.
                               /// </summary>
                               public static {returnType} Bind(
                                   this {group.TargetTypeFullName} view,
                                   {group.SourceTypeFullName} viewModel,
                                   global::System.Linq.Expressions.Expression<global::System.Func<{group.SourceTypeFullName}, {sourcePropType}>> viewModelProperty,
                                   global::System.Linq.Expressions.Expression<global::System.Func<{group.TargetTypeFullName}, {targetPropType}>> viewProperty,
                       """);

        AppendExtraParameters(sb, group);

        _ = sb.AppendLine("""
                                  [global::System.Runtime.CompilerServices.CallerArgumentExpression("viewModelProperty")] string viewModelPropertyExpression = "",
                                  [global::System.Runtime.CompilerServices.CallerArgumentExpression("viewProperty")] string viewPropertyExpression = "",
                                  [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                                  [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                              {
                      """);

        EmitAffinityOverride(sb, group, "viewPropertyExpression");

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);
            var escapedSourceExpr = CodeGeneratorHelpers.EscapeString(inv.SourceExpressionText);
            var escapedTargetExpr = CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText);
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.SourceTypeFullName,
                inv.CallerFilePath,
                inv.CallerLineNumber,
                $"{inv.SourceExpressionText}|{inv.TargetExpressionText}");

            _ = sb.AppendLine($$"""
                                        {{condition}} (viewModelPropertyExpression == "{{escapedSourceExpr}}"
                                            && viewPropertyExpression == "{{escapedTargetExpr}}")
                                        {
                                            return __Bind_{{methodSuffix}}(viewModel, view{{FormatExtraArgs(group)}});
                                        }
                            """);
        }

        _ = sb.AppendLine("""
                                  throw new global::System.InvalidOperationException(
                                      "No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.");
                              }
                      """);
    }

    /// <summary>Generates the CallerFilePath-based overload for Bind dispatch.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateCallerFilePathOverload(
        StringBuilder sb,
        BindingTypeGroup group,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        var sourcePropType = CodeGeneratorHelpers.NullableSelectorLeafType(group.Invocations[0].SourcePropertyPath, supportsNullable);
        var targetPropType = CodeGeneratorHelpers.NullableSelectorLeafType(group.Invocations[0].TargetPropertyPath, supportsNullable);
        var returnType = FormatReturnType(group);

        _ = sb.AppendLine($"""
                               /// <summary>
                               /// Concrete typed overload for Bind from {group.SourceTypeFullName} to {group.TargetTypeFullName}.
                               /// Uses CallerFilePath + CallerLineNumber for dispatch.
                               /// </summary>
                               public static {returnType} Bind(
                                   this {group.TargetTypeFullName} view,
                                   {group.SourceTypeFullName} viewModel,
                                   global::System.Linq.Expressions.Expression<global::System.Func<{group.SourceTypeFullName}, {sourcePropType}>> viewModelProperty,
                                   global::System.Linq.Expressions.Expression<global::System.Func<{group.TargetTypeFullName}, {targetPropType}>> viewProperty,
                       """);

        AppendExtraParameters(sb, group);

        if (stubHasExpressionParameters)
        {
            CodeGeneratorHelpers.AppendExpressionParameter(sb, "viewModelProperty", "viewModelPropertyExpression", false);
            CodeGeneratorHelpers.AppendExpressionParameter(sb, "viewProperty", "viewPropertyExpression", false);
        }

        _ = sb.AppendLine("""
                                  [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                                  [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                              {
                      """);

        EmitAffinityOverride(
            sb,
            group,
            $"\"{CodeGeneratorHelpers.EscapeString(group.Invocations[0].TargetExpressionText)}\"");

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var suffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.SourceTypeFullName,
                inv.CallerFilePath,
                inv.CallerLineNumber,
                $"{inv.SourceExpressionText}|{inv.TargetExpressionText}");

            _ = sb.AppendLine($$"""
                                        {{condition}} (callerLineNumber == {{inv.CallerLineNumber}}
                                            && callerFilePath.EndsWith("{{CodeGeneratorHelpers.EscapeString(suffix)}}", global::System.StringComparison.OrdinalIgnoreCase))
                                        {
                                            return __Bind_{{methodSuffix}}(viewModel, view{{FormatExtraArgs(group)}});
                                        }
                            """);
        }

        _ = sb.AppendLine("""
                                  throw new global::System.InvalidOperationException(
                                      "No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.");
                              }
                      """);
    }

    /// <summary>Generates a private Bind method for a specific invocation.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="sourceClassInfo">The source type class binding info.</param>
    /// <param name="targetClassInfo">The target type class binding info.</param>
    /// <param name="suffix">The stable method name suffix.</param>
    internal static void GenerateBindMethod(
        StringBuilder sb,
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        ClassBindingInfo? targetClassInfo,
        string suffix)
    {
        var viewPropertyAccess = CodeGeneratorHelpers.BuildGuardedAssignment(
            "view",
            inv.TargetPropertyPath,
            "value",
            SubscriptionBodyIndent);

        // Both directions follow the view's current view model, so the write walks the same re-rooted path the
        // observation does and its guard drops the write while the view holds none.
        var observation = BindingEmitterHelpers.ResolveViewModelObservation(inv, sourceClassInfo, targetClassInfo);
        var viewModelSetAccess = CodeGeneratorHelpers.BuildGuardedAssignment(
            observation.RootVariable,
            observation.Path,
            "value",
            SubscriptionBodyIndent);
        var viewModelPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.SourcePropertyPath);
        var viewPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.TargetPropertyPath);

        var extraParams = FormatExtraMethodParams(inv);
        var conversionComment = inv.HasConversion ? " (with conversion)" : string.Empty;
        var schedulerComment = inv.HasScheduler ? " (with scheduler)" : string.Empty;
        var returnType = FormatMethodReturnType(inv);

        _ = sb.AppendLine($$"""
                                private static {{returnType}} __Bind_{{suffix}}({{inv.SourceTypeFullName}} viewModel, {{inv.TargetTypeFullName}} view{{extraParams}})
                                {
                                    // Bind: {{viewModelPathComment}} <-> {{viewPathComment}}{{conversionComment}}{{schedulerComment}}
                        """);

        BindingEmitterHelpers.EmitBindingHookGuard(sb, "viewModel", "view", "TwoWay", "null");

        // Emit inline observation code instead of delegating to WhenChanged dispatch
        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            observation.RootVariable,
            observation.Path,
            inv.SourcePropertyTypeFullName,
            observation.RootClassInfo,
            ViewModelObservableName);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            "view",
            inv.TargetPropertyPath,
            inv.TargetPropertyTypeFullName,
            targetClassInfo,
            ViewObservableName);

        if (inv.HasConversion || inv.HasScheduler)
        {
            var (viewModelVar, viewVar) = EmitConversionAndSchedulerStages(sb, inv);
            (viewModelVar, viewVar) = EmitRegistryConversionStages(sb, inv, viewModelVar, viewVar);
            viewModelVar = BindingEmitterHelpers.EmitViewThreadStage(sb, inv, viewModelVar, "viewThreadObs");

            EmitTwoWaySubscription(sb, inv, viewModelVar, viewVar, viewPropertyAccess, viewModelSetAccess);
        }
        else
        {
            var (viewModelVar, viewVar) =
                EmitRegistryConversionStages(sb, inv, ViewModelObservableName, ViewObservableName);
            viewModelVar = BindingEmitterHelpers.EmitViewThreadStage(sb, inv, viewModelVar, "viewThreadObs");

            EmitTwoWaySubscription(sb, inv, viewModelVar, viewVar, viewPropertyAccess, viewModelSetAccess);
        }
    }

    /// <summary>Appends extra parameters (converters, scheduler) to the concrete overload signature.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendExtraParameters(StringBuilder sb, BindingTypeGroup group) =>
        BindingEmitterHelpers.AppendTwoWayExtraParameters(sb, group, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats extra arguments (converters, scheduler) for forwarding to the binding method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>Extra arguments string or empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraArgs(BindingTypeGroup group) =>
        BindingEmitterHelpers.FormatTwoWayExtraArgs(group, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats extra method parameters for the private binding method signature.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>Extra parameters string for converter and scheduler parameters.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv) =>
        BindingEmitterHelpers.FormatTwoWayExtraMethodParams(inv, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats the return type for a concrete Bind overload.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>The fully qualified return type string.</returns>
    internal static string FormatReturnType(BindingTypeGroup group) =>
        $"global::ReactiveUI.Binding.IReactiveBinding<{group.TargetTypeFullName}, {BindingChange}>";

    /// <summary>Formats the return type for a private Bind method.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>The fully qualified return type string.</returns>
    internal static string FormatMethodReturnType(BindingInvocationInfo inv) =>
        $"global::ReactiveUI.Binding.IReactiveBinding<{inv.TargetTypeFullName}, {BindingChange}>";

    /// <summary>
    /// Emits the conversion and scheduler stages that sit between the raw observations and the
    /// subscription, and reports the variable names the subscription should read from.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>The view model and view observable variable names after the stages are applied.</returns>
    private static (string ViewModelVar, string ViewVar) EmitConversionAndSchedulerStages(
        StringBuilder sb,
        BindingInvocationInfo inv)
    {
        var viewModelVar = ViewModelObservableName;
        var viewVar = ViewObservableName;

        if (inv.HasConversion)
        {
            var viewModelNext = inv.HasScheduler ? "__vmSelected" : "vmBind";
            var viewNext = inv.HasScheduler ? "__viewSelected" : "viewBind";
            _ = sb.AppendLine($"""
                                   var {viewModelNext} = new {MapSignal}<{inv.SourcePropertyTypeFullName}, {inv.TargetPropertyTypeFullName}>({viewModelVar}, viewModelToViewConverter);
                                   var {viewNext} = new {MapSignal}<{inv.TargetPropertyTypeFullName}, {inv.SourcePropertyTypeFullName}>({viewVar}, viewToViewModelConverter);
                           """);
            viewModelVar = viewModelNext;
            viewVar = viewNext;
        }

        if (inv.HasScheduler)
        {
            _ = sb.AppendLine($"""
                                   var vmBind = {LinqExtensions}.ObserveOn<{inv.TargetPropertyTypeFullName}>({viewModelVar}, scheduler);
                                   var viewBind = {LinqExtensions}.ObserveOn<{inv.SourcePropertyTypeFullName}>({viewVar}, scheduler);
                           """);
            viewModelVar = "vmBind";
            viewVar = "viewBind";
        }

        return (viewModelVar, viewVar);
    }

    /// <summary>Emits the two-way subscription, change-stream merge, and <c>ReactiveBinding</c> return block.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="viewModelVar">The view model observable variable name to subscribe to.</param>
    /// <param name="viewVar">The view observable variable name to subscribe to.</param>
    /// <param name="viewPropertyAccess">The view property setter access chain.</param>
    /// <param name="viewModelSetAccess">The view model property setter access chain.</param>
    private static void EmitTwoWaySubscription(
        StringBuilder sb,
        BindingInvocationInfo inv,
        string viewModelVar,
        string viewVar,
        string viewPropertyAccess,
        string viewModelSetAccess) => _ = sb.AppendLine($$"""

                                    var d1 = {{BindingErrors}}.Subscribe({{viewModelVar}}, value =>
                                    {
                                        {{viewPropertyAccess}}
                                    }, "{{CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText)}}");

                                    var __viewSkipped = global::ReactiveUI.Primitives.LinqExtensions.Skip({{viewVar}}, 1);
                                    var d2 = {{BindingErrors}}.Subscribe(__viewSkipped, value =>
                                    {
                                        {{viewModelSetAccess}}
                                    }, "{{CodeGeneratorHelpers.EscapeString(inv.SourceExpressionText)}}");

                                    var __vmTagged = new {{MapSignal}}<{{inv.TargetPropertyTypeFullName}}, {{BindingChange}}>({{viewModelVar}}, v => new {{BindingChange}}(v, true));
                                    var __viewTagged = new {{MapSignal}}<{{inv.SourcePropertyTypeFullName}}, {{BindingChange}}>(__viewSkipped, v => new {{BindingChange}}(v, false));
                                    var changed = new {{MergeSignal}}<{{BindingChange}}>(__vmTagged, __viewTagged);

                                    var disposable = new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(d1, d2);

                                    return new global::ReactiveUI.Binding.ReactiveBinding<{{inv.TargetTypeFullName}}, {{BindingChange}}>(
                                        view,
                                        changed,
                                        global::ReactiveUI.Binding.BindingDirection.TwoWay,
                                        disposable);
                                }
                        """)
            .AppendLine();

    /// <summary>Emits the stages that convert each direction to the type the other side declares.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="viewModelVar">The variable holding the view model side's values.</param>
    /// <param name="viewVar">The variable holding the view side's values.</param>
    /// <returns>The variables to subscribe each direction to.</returns>
    /// <remarks>
    /// Two-way needs both: each direction assigns across the same type gap, in opposite directions.
    /// </remarks>
    private static (string ViewModelVar, string ViewVar) EmitRegistryConversionStages(
        StringBuilder sb,
        BindingInvocationInfo inv,
        string viewModelVar,
        string viewVar)
    {
        if (!BindingEmitterHelpers.RequiresRegistryConversion(inv))
        {
            return (viewModelVar, viewVar);
        }

        const string convertedViewModelVar = "convertedVmObs";
        const string convertedViewVar = "convertedViewObs";

        BindingEmitterHelpers.EmitRegistryConversion(
            sb,
            viewModelVar,
            convertedViewModelVar,
            inv.SourcePropertyTypeFullName,
            inv.TargetPropertyTypeFullName);

        BindingEmitterHelpers.EmitRegistryConversion(
            sb,
            viewVar,
            convertedViewVar,
            inv.TargetPropertyTypeFullName,
            inv.SourcePropertyTypeFullName);

        return (convertedViewModelVar, convertedViewVar);
    }

    /// <summary>Reports whether the two sides differ with no supplied converter, so the registry supplies one.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns><see langword="true"/> when the registry has to convert between the two sides.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool RequiresRegistryConversion(BindingTypeGroup group) =>
        !group.HasConversion
        && !string.Equals(
            group.SourcePropertyTypeFullName,
            group.TargetPropertyTypeFullName,
            StringComparison.Ordinal);

    /// <summary>Names the conversions the fallback needs, matching whatever the generated body would apply.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>The converter-pair argument, trailed by a comma, or empty when both sides share a type.</returns>
    private static string FormatFallbackConverters(BindingTypeGroup group)
    {
        if (group.HasConversion)
        {
            return $"{TwoWayConverters}.Create({ForwardConverterName}, {ReverseConverterName}), ";
        }

        if (!RequiresRegistryConversion(group))
        {
            return string.Empty;
        }

        var forward = CodeGeneratorHelpers.FormatRegistryConversionLambda(
            group.SourcePropertyTypeFullName,
            group.TargetPropertyTypeFullName);
        var reverse = CodeGeneratorHelpers.FormatRegistryConversionLambda(
            group.TargetPropertyTypeFullName,
            group.SourcePropertyTypeFullName);

        // Both arguments are lambdas, which cannot drive inference, so the pair names its types outright.
        return
            $"{TwoWayConverters}.Create<{group.SourcePropertyTypeFullName}, {group.TargetPropertyTypeFullName}>({forward}, {reverse}), ";
    }

    /// <summary>Emits the check that hands the binding to the runtime engine when a registered plugin outranks the generated one.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group, which fixes both bound types for the whole overload.</param>
    /// <param name="bindingExpression">The C# expression naming the bound view property, used when a write faults.</param>
    private static void EmitAffinityOverride(StringBuilder sb, BindingTypeGroup group, string bindingExpression)
    {
        var convertersArg = FormatFallbackConverters(group);
        var schedulerArg = group.HasScheduler ? "scheduler" : "null";

        BindingEmitterHelpers.EmitAffinityOverride(
            sb,
            group,
            "Bind",
            $"view, viewModel, viewModelProperty, viewProperty, {convertersArg}{schedulerArg}, {bindingExpression}",
            true);
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;
using static ReactiveUI.Binding.SourceGenerators.CodeGeneration.GeneratedTypeNames;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads and binding methods for Bind (view-first two-way) invocations.
/// The generated methods return <c>IReactiveBinding&lt;TView, (object?, bool)&gt;</c> and use view-first parameter ordering.
/// </summary>
internal static class BindCodeGenerator
{
    /// <summary>What this API calls the view-model-to-view converter in its generated signatures.</summary>
    private const string ForwardConverterName = "viewModelToViewConverter";

    /// <summary>What this API calls the view-to-view-model converter in its generated signatures.</summary>
    private const string ReverseConverterName = "viewToViewModelConverter";

    /// <summary>Name of the generated local holding the view model side observable.</summary>
    private const string ViewModelObservableName = "vmObs";

    /// <summary>Name of the generated local holding the view side observable.</summary>
    private const string ViewObservableName = "viewObs";

    /// <summary>Generates concrete typed overloads and binding methods for Bind invocations.</summary>
    /// <param name="invocations">All detected Bind invocations.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    internal static string? Generate(
        ImmutableArray<BindingInvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return null;
        }

        var sb = PooledBuilder.Rent(invocations.Length * CodeGeneratorHelpers.PerInvocationBufferCapacity);
        var supportsCallerArgExpr = features.SupportsCallerArgExpr;
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb, features);
        _ = sb.AppendLine();

        var groups = GroupByTypeSignature(invocations);

        for (var g = 0; g < groups.Count; g++)
        {
            var group = groups[g];

            GenerateConcreteOverload(sb, group, supportsCallerArgExpr, features.SupportsNullable, features.StubHasExpressionParameters);
            _ = sb.AppendLine();

            for (var i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var sourceClassInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.SourceTypeFullName);
                var targetClassInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.TargetTypeFullName);
                var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                    inv.SourceTypeFullName,
                    inv.CallerFilePath,
                    inv.CallerLineNumber,
                    $"{inv.SourceExpressionText}|{inv.TargetExpressionText}");
                GenerateBindMethod(sb, inv, sourceClassInfo, targetClassInfo, suffix, features.SupportsNullable);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        _ = sb.AppendLine();

        return PooledBuilder.ToStringAndReturn(sb);
    }

    /// <summary>Groups Bind invocations by their type signature for overload generation.</summary>
    /// <param name="invocations">The Bind invocations to group.</param>
    /// <returns>A list of grouped invocations sharing the same type signature.</returns>
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
        var returnType = FormatReturnType(group, supportsNullable);

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
        var returnType = FormatReturnType(group, supportsNullable);

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
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void GenerateBindMethod(
        StringBuilder sb,
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        ClassBindingInfo? targetClassInfo,
        string suffix,
        bool supportsNullable)
    {
        var viewPropertyAccess = CodeGeneratorHelpers.BuildPropertySetterChain("view", inv.TargetPropertyPath);
        var viewModelSetAccess = CodeGeneratorHelpers.BuildPropertySetterChain("viewModel", inv.SourcePropertyPath);
        var viewModelPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.SourcePropertyPath);
        var viewPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.TargetPropertyPath);

        var extraParams = FormatExtraMethodParams(inv);
        var conversionComment = inv.HasConversion ? " (with conversion)" : string.Empty;
        var schedulerComment = inv.HasScheduler ? " (with scheduler)" : string.Empty;
        var returnType = FormatMethodReturnType(inv, supportsNullable);

        _ = sb.AppendLine($$"""
                                private static {{returnType}} __Bind_{{suffix}}({{inv.SourceTypeFullName}} viewModel, {{inv.TargetTypeFullName}} view{{extraParams}})
                                {
                                    // Bind: {{viewModelPathComment}} <-> {{viewPathComment}}{{conversionComment}}{{schedulerComment}}
                        """);

        // Emit inline observation code instead of delegating to WhenChanged dispatch
        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            "viewModel",
            inv.SourcePropertyPath,
            inv.SourcePropertyTypeFullName,
            sourceClassInfo,
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

            EmitTwoWaySubscription(sb, inv, viewModelVar, viewVar, viewPropertyAccess, viewModelSetAccess, supportsNullable);
        }
        else
        {
            EmitTwoWaySubscription(sb, inv, ViewModelObservableName, ViewObservableName, viewPropertyAccess, viewModelSetAccess, supportsNullable);
        }
    }

    /// <summary>Appends extra parameters (converters, scheduler) to the concrete overload signature.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    internal static void AppendExtraParameters(StringBuilder sb, BindingTypeGroup group) =>
        BindingEmitterHelpers.AppendTwoWayExtraParameters(sb, group, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats extra arguments (converters, scheduler) for forwarding to the binding method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>Extra arguments string or empty.</returns>
    internal static string FormatExtraArgs(BindingTypeGroup group) =>
        BindingEmitterHelpers.FormatTwoWayExtraArgs(group, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats extra method parameters for the private binding method signature.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>Extra parameters string for converter and scheduler parameters.</returns>
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv) =>
        BindingEmitterHelpers.FormatTwoWayExtraMethodParams(inv, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats the return type for a concrete Bind overload.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>The fully qualified return type string.</returns>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static string FormatReturnType(BindingTypeGroup group, bool supportsNullable) =>
        $"global::ReactiveUI.Binding.IReactiveBinding<{group.TargetTypeFullName}, {BindReturnValueType(supportsNullable)}>";

    /// <summary>Formats the return type for a private Bind method.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>The fully qualified return type string.</returns>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static string FormatMethodReturnType(BindingInvocationInfo inv, bool supportsNullable) =>
        $"global::ReactiveUI.Binding.IReactiveBinding<{inv.TargetTypeFullName}, {BindReturnValueType(supportsNullable)}>";

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
                                   var {viewModelNext} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({viewModelVar}, viewModelToViewConverter);
                                   var {viewNext} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({viewVar}, viewToViewModelConverter);
                           """);
            viewModelVar = viewModelNext;
            viewVar = viewNext;
        }

        if (inv.HasScheduler)
        {
            _ = sb.AppendLine($"""
                                   var vmBind = new {ObserveOnObservable}<{inv.TargetPropertyTypeFullName}>({viewModelVar}, scheduler);
                                   var viewBind = new {ObserveOnObservable}<{inv.SourcePropertyTypeFullName}>({viewVar}, scheduler);
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
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    private static void EmitTwoWaySubscription(
        StringBuilder sb,
        BindingInvocationInfo inv,
        string viewModelVar,
        string viewVar,
        string viewPropertyAccess,
        string viewModelSetAccess,
        bool supportsNullable)
    {
        var nullable = supportsNullable ? "?" : string.Empty;
        _ = sb.AppendLine($$"""

                                    var d1 = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe({{viewModelVar}}, value =>
                                    {
                                        {{viewPropertyAccess}} = value;
                                    });

                                    var __viewSkipped = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Skip({{viewVar}}, 1);
                                    var d2 = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(__viewSkipped, value =>
                                    {
                                        {{viewModelSetAccess}} = value;
                                    });

                                    var __vmTagged = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({{viewModelVar}}, v => ((object{{nullable}})v, true));
                                    var __viewTagged = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select(__viewSkipped, v => ((object{{nullable}})v, false));
                                    var changed = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Merge(__vmTagged, __viewTagged);

                                    var disposable = new global::ReactiveUI.Binding.Observables.CompositeDisposable2(d1, d2);

                                    return new global::ReactiveUI.Binding.ReactiveBinding<{{inv.TargetTypeFullName}}, {{BindReturnValueType(supportsNullable)}}>(
                                        view,
                                        changed,
                                        global::ReactiveUI.Binding.BindingDirection.TwoWay,
                                        disposable);
                                }
                        """)
            .AppendLine();
    }

    /// <summary>
    /// The <c>IReactiveBinding</c> value-tuple type emitted for two-way bindings. Annotated nullable
    /// (<c>object?</c>) when the target supports nullable reference types so the bound value (which may be
    /// null for reference types) is correctly typed; plain <c>object</c> on C# 7.3.
    /// </summary>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <returns>The value-tuple type string.</returns>
    private static string BindReturnValueType(bool supportsNullable) =>
        supportsNullable ? "(object? view, bool isViewModel)" : "(object view, bool isViewModel)";
}

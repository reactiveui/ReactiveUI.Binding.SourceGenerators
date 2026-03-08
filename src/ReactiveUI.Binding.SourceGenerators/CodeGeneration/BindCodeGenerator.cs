// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads and binding methods for Bind (view-first two-way) invocations.
/// The generated methods return <c>IReactiveBinding&lt;TView, (object?, bool)&gt;</c> and use view-first parameter ordering.
/// </summary>
internal static class BindCodeGenerator
{
    /// <summary>
    /// The tuple type used as the value type in Bind return values.
    /// </summary>
    private const string BindReturnValueType = "(object view, bool isViewModel)";

    /// <summary>
    /// Generates concrete typed overloads and binding methods for Bind invocations.
    /// </summary>
    /// <param name="invocations">All detected Bind invocations.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression (C# 10+).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    internal static string? Generate(
        ImmutableArray<BindingInvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        bool supportsCallerArgExpr)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return null;
        }

        var sb = new StringBuilder(invocations.Length * 1024);
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb);
        sb.AppendLine();

        var groups = GroupByTypeSignature(invocations);

        for (var g = 0; g < groups.Count; g++)
        {
            var group = groups[g];

            GenerateConcreteOverload(sb, group, supportsCallerArgExpr);
            sb.AppendLine();

            for (var i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var sourceClassInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.SourceTypeFullName);
                var targetClassInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.TargetTypeFullName);
                var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.SourceExpressionText + "|" + inv.TargetExpressionText);
                GenerateBindMethod(sb, inv, sourceClassInfo, targetClassInfo, suffix);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Groups Bind invocations by their type signature for overload generation.
    /// </summary>
    /// <param name="invocations">The Bind invocations to group.</param>
    /// <returns>A list of grouped invocations sharing the same type signature.</returns>
    internal static List<BindingTypeGroup> GroupByTypeSignature(ImmutableArray<BindingInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<BindingInvocationInfo>>(invocations.Length);
        var keySb = new StringBuilder(128);

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            keySb.Clear()
                .Append(inv.SourceTypeFullName).Append('|')
                .Append(inv.TargetTypeFullName).Append('|')
                .Append(inv.SourcePropertyTypeFullName).Append('|')
                .Append(inv.TargetPropertyTypeFullName).Append('|')
                .Append(inv.HasConversion).Append('|')
                .Append(inv.HasScheduler);

            var key = keySb.ToString();

            if (!groupMap.TryGetValue(key, out var list))
            {
                list = [];
                groupMap[key] = list;
            }

            list.Add(inv);
        }

        var result = new List<BindingTypeGroup>();
        foreach (var kvp in groupMap)
        {
            var first = kvp.Value[0];
            result.Add(new BindingTypeGroup(
                first.SourceTypeFullName,
                first.TargetTypeFullName,
                first.SourcePropertyTypeFullName,
                first.TargetPropertyTypeFullName,
                first.HasConversion,
                first.HasScheduler,
                kvp.Value.ToArray()));
        }

        return result;
    }

    /// <summary>
    /// Generates the concrete typed overload using the appropriate dispatch strategy.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="supportsCallerArgExpr">Whether CallerArgumentExpression is available.</param>
    internal static void GenerateConcreteOverload(
        StringBuilder sb,
        BindingTypeGroup group,
        bool supportsCallerArgExpr)
    {
        if (supportsCallerArgExpr)
        {
            GenerateCallerArgExprOverload(sb, group);
        }
        else
        {
            GenerateCallerFilePathOverload(sb, group);
        }
    }

    /// <summary>
    /// Generates the CallerArgumentExpression-based overload for Bind dispatch.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    internal static void GenerateCallerArgExprOverload(
        StringBuilder sb,
        BindingTypeGroup group)
    {
        var returnType = FormatReturnType(group);

        sb.AppendLine($"""
                    /// <summary>
                    /// Concrete typed overload for Bind from {group.SourceTypeFullName} to {group.TargetTypeFullName}.
                    /// Uses CallerArgumentExpression for dispatch.
                    /// </summary>
                    public static {returnType} Bind(
                        this {group.TargetTypeFullName} view,
                        {group.SourceTypeFullName} viewModel,
                        global::System.Linq.Expressions.Expression<global::System.Func<{group.SourceTypeFullName}, {group.SourcePropertyTypeFullName}>> vmProperty,
                        global::System.Linq.Expressions.Expression<global::System.Func<{group.TargetTypeFullName}, {group.TargetPropertyTypeFullName}>> viewProperty,
            """);

        AppendExtraParameters(sb, group);

        sb.AppendLine("""
                        [global::System.Runtime.CompilerServices.CallerArgumentExpression("vmProperty")] string vmPropertyExpression = "",
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
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.SourceExpressionText + "|" + inv.TargetExpressionText);

            sb.AppendLine($$"""
                            {{condition}} (vmPropertyExpression == "{{escapedSourceExpr}}"
                                && viewPropertyExpression == "{{escapedTargetExpr}}")
                            {
                                return __Bind_{{methodSuffix}}(viewModel, view{{FormatExtraArgs(group)}});
                            }
                """);
        }

        sb.AppendLine("""
                        throw new global::System.InvalidOperationException(
                            "No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.");
                    }
            """);
    }

    /// <summary>
    /// Generates the CallerFilePath-based overload for Bind dispatch.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    internal static void GenerateCallerFilePathOverload(
        StringBuilder sb,
        BindingTypeGroup group)
    {
        var returnType = FormatReturnType(group);

        sb.AppendLine($"""
                    /// <summary>
                    /// Concrete typed overload for Bind from {group.SourceTypeFullName} to {group.TargetTypeFullName}.
                    /// Uses CallerFilePath + CallerLineNumber for dispatch.
                    /// </summary>
                    public static {returnType} Bind(
                        this {group.TargetTypeFullName} view,
                        {group.SourceTypeFullName} viewModel,
                        global::System.Linq.Expressions.Expression<global::System.Func<{group.SourceTypeFullName}, {group.SourcePropertyTypeFullName}>> vmProperty,
                        global::System.Linq.Expressions.Expression<global::System.Func<{group.TargetTypeFullName}, {group.TargetPropertyTypeFullName}>> viewProperty,
            """);

        AppendExtraParameters(sb, group);

        sb.AppendLine("""
                        [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                        [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                    {
            """);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var suffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.SourceExpressionText + "|" + inv.TargetExpressionText);

            sb.AppendLine($$"""
                            {{condition}} (callerLineNumber == {{inv.CallerLineNumber}}
                                && callerFilePath.EndsWith("{{CodeGeneratorHelpers.EscapeString(suffix)}}", global::System.StringComparison.OrdinalIgnoreCase))
                            {
                                return __Bind_{{methodSuffix}}(viewModel, view{{FormatExtraArgs(group)}});
                            }
                """);
        }

        sb.AppendLine("""
                        throw new global::System.InvalidOperationException(
                            "No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.");
                    }
            """);
    }

    /// <summary>
    /// Generates a private Bind method for a specific invocation.
    /// </summary>
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
        var viewPropertyAccess = CodeGeneratorHelpers.BuildPropertySetterChain("view", inv.TargetPropertyPath);
        var vmSetAccess = CodeGeneratorHelpers.BuildPropertySetterChain("viewModel", inv.SourcePropertyPath);
        var vmPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.SourcePropertyPath);
        var viewPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.TargetPropertyPath);

        var extraParams = FormatExtraMethodParams(inv);
        var conversionComment = inv.HasConversion ? " (with conversion)" : string.Empty;
        var schedulerComment = inv.HasScheduler ? " (with scheduler)" : string.Empty;
        var returnType = FormatMethodReturnType(inv);

        sb.AppendLine($$"""
                    private static {{returnType}} __Bind_{{suffix}}({{inv.SourceTypeFullName}} viewModel, {{inv.TargetTypeFullName}} view{{extraParams}})
                    {
                        // Bind: {{vmPathComment}} <-> {{viewPathComment}}{{conversionComment}}{{schedulerComment}}
            """);

        // Emit inline observation code instead of delegating to WhenChanged dispatch
        ObservationCodeGenerator.EmitInlineObservation(
            sb, "viewModel", inv.SourcePropertyPath, inv.SourcePropertyTypeFullName, sourceClassInfo, "vmObs");

        ObservationCodeGenerator.EmitInlineObservation(
            sb, "view", inv.TargetPropertyPath, inv.TargetPropertyTypeFullName, targetClassInfo, "viewObs");

        if (inv.HasConversion || inv.HasScheduler)
        {
            var vmVar = "vmObs";
            var viewVar = "viewObs";

            if (inv.HasConversion)
            {
                var vmNext = inv.HasScheduler ? "__vmSelected" : "vmBind";
                var viewNext = inv.HasScheduler ? "__viewSelected" : "viewBind";
                sb.AppendLine($"""
                        var {vmNext} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({vmVar}, vmToViewConverter);
                        var {viewNext} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({viewVar}, viewToVmConverter);
                """);
                vmVar = vmNext;
                viewVar = viewNext;
            }

            if (inv.HasScheduler)
            {
                sb.AppendLine($"""
                        var vmBind = new global::ReactiveUI.Binding.Reactive.ObserveOnObservable<{inv.TargetPropertyTypeFullName}>({vmVar}, scheduler);
                        var viewBind = new global::ReactiveUI.Binding.Reactive.ObserveOnObservable<{inv.SourcePropertyTypeFullName}>({viewVar}, scheduler);
                """);
                vmVar = "vmBind";
                viewVar = "viewBind";
            }

            sb.AppendLine($$"""

                        var d1 = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe({{vmVar}}, value =>
                        {
                            {{viewPropertyAccess}} = value;
                        });

                        var __viewSkipped = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Skip({{viewVar}}, 1);
                        var d2 = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(__viewSkipped, value =>
                        {
                            {{vmSetAccess}} = value;
                        });

                        var __vmTagged = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({{vmVar}}, v => ((object)v, true));
                        var __viewTagged = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select(__viewSkipped, v => ((object)v, false));
                        var changed = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Merge(__vmTagged, __viewTagged);

                        var disposable = new global::ReactiveUI.Binding.Observables.CompositeDisposable2(d1, d2);

                        return new global::ReactiveUI.Binding.ReactiveBinding<{{inv.TargetTypeFullName}}, {{BindReturnValueType}}>(
                            view,
                            changed,
                            global::ReactiveUI.Binding.BindingDirection.TwoWay,
                            disposable);
                    }
            """)
                .AppendLine();
        }
        else
        {
            sb.AppendLine($$"""

                        var d1 = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(vmObs, value =>
                        {
                            {{viewPropertyAccess}} = value;
                        });

                        var __viewSkipped = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Skip(viewObs, 1);
                        var d2 = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(__viewSkipped, value =>
                        {
                            {{vmSetAccess}} = value;
                        });

                        var __vmTagged = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select(vmObs, v => ((object)v, true));
                        var __viewTagged = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select(__viewSkipped, v => ((object)v, false));
                        var changed = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Merge(__vmTagged, __viewTagged);

                        var disposable = new global::ReactiveUI.Binding.Observables.CompositeDisposable2(d1, d2);

                        return new global::ReactiveUI.Binding.ReactiveBinding<{{inv.TargetTypeFullName}}, {{BindReturnValueType}}>(
                            view,
                            changed,
                            global::ReactiveUI.Binding.BindingDirection.TwoWay,
                            disposable);
                    }
            """)
                .AppendLine();
        }
    }

    /// <summary>
    /// Appends extra parameters (converters, scheduler) to the concrete overload signature.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    internal static void AppendExtraParameters(StringBuilder sb, BindingTypeGroup group)
    {
        if (group.HasConversion)
        {
            sb.AppendLine($"""
                            global::System.Func<{group.SourcePropertyTypeFullName}, {group.TargetPropertyTypeFullName}> vmToViewConverter,
                            global::System.Func<{group.TargetPropertyTypeFullName}, {group.SourcePropertyTypeFullName}> viewToVmConverter,
                """);
        }

        if (group.HasScheduler)
        {
            sb.AppendLine("            global::System.Reactive.Concurrency.IScheduler scheduler,");
        }
    }

    /// <summary>
    /// Formats extra arguments (converters, scheduler) for forwarding to the binding method.
    /// </summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>Extra arguments string or empty.</returns>
    internal static string FormatExtraArgs(BindingTypeGroup group)
    {
        var sb = new StringBuilder();
        if (group.HasConversion)
        {
            sb.Append(", vmToViewConverter, viewToVmConverter");
        }

        if (group.HasScheduler)
        {
            sb.Append(", scheduler");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats extra method parameters for the private binding method signature.
    /// </summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>Extra parameters string for converter and scheduler parameters.</returns>
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv)
    {
        var sb = new StringBuilder();
        if (inv.HasConversion)
        {
            sb.Append($", global::System.Func<{inv.SourcePropertyTypeFullName}, {inv.TargetPropertyTypeFullName}> vmToViewConverter")
                .Append($", global::System.Func<{inv.TargetPropertyTypeFullName}, {inv.SourcePropertyTypeFullName}> viewToVmConverter");
        }

        if (inv.HasScheduler)
        {
            sb.Append(", global::System.Reactive.Concurrency.IScheduler scheduler");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats the return type for a concrete Bind overload.
    /// </summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>The fully qualified return type string.</returns>
    internal static string FormatReturnType(BindingTypeGroup group) => $"global::ReactiveUI.Binding.IReactiveBinding<{group.TargetTypeFullName}, {BindReturnValueType}>";

    /// <summary>
    /// Formats the return type for a private Bind method.
    /// </summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>The fully qualified return type string.</returns>
    internal static string FormatMethodReturnType(BindingInvocationInfo inv) => $"global::ReactiveUI.Binding.IReactiveBinding<{inv.TargetTypeFullName}, {BindReturnValueType}>";

    /// <summary>
    /// Groups binding invocations by source, target, property types, and overload variant for overload generation.
    /// </summary>
    internal sealed record BindingTypeGroup(
        string SourceTypeFullName,
        string TargetTypeFullName,
        string SourcePropertyTypeFullName,
        string TargetPropertyTypeFullName,
        bool HasConversion,
        bool HasScheduler,
        BindingInvocationInfo[] Invocations);
}

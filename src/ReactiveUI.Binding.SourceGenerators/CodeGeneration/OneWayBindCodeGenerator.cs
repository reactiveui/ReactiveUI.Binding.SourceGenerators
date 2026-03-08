// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads and binding methods for OneWayBind (view-first) invocations.
/// The generated methods return <c>IReactiveBinding&lt;TView, TValue&gt;</c> and use view-first parameter ordering.
/// </summary>
internal static class OneWayBindCodeGenerator
{
    /// <summary>
    /// Generates concrete typed overloads and binding methods for OneWayBind invocations.
    /// </summary>
    /// <param name="invocations">All detected OneWayBind invocations.</param>
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

        var sb = new StringBuilder();
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
                var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.SourceExpressionText + "|" + inv.TargetExpressionText);
                GenerateOneWayBindMethod(sb, inv, sourceClassInfo, suffix);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Groups OneWayBind invocations by their type signature for overload generation.
    /// </summary>
    /// <param name="invocations">The OneWayBind invocations to group.</param>
    /// <returns>A list of grouped invocations sharing the same type signature.</returns>
    internal static List<BindingTypeGroup> GroupByTypeSignature(ImmutableArray<BindingInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<BindingInvocationInfo>>();

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            var key = inv.SourceTypeFullName + "|" + inv.TargetTypeFullName + "|" +
                      inv.SourcePropertyTypeFullName + "|" + inv.TargetPropertyTypeFullName + "|" +
                      inv.HasConversion + "|" + inv.HasScheduler;

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
    /// Generates the CallerArgumentExpression-based overload for OneWayBind dispatch.
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
                    /// Concrete typed overload for OneWayBind from {group.SourceTypeFullName} to {group.TargetTypeFullName}.
                    /// Uses CallerArgumentExpression for dispatch.
                    /// </summary>
                    public static {returnType} OneWayBind(
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
                                return __OneWayBind_{{methodSuffix}}(viewModel, view{{FormatExtraArgs(group)}});
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
    /// Generates the CallerFilePath-based overload for OneWayBind dispatch.
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
                    /// Concrete typed overload for OneWayBind from {group.SourceTypeFullName} to {group.TargetTypeFullName}.
                    /// Uses CallerFilePath + CallerLineNumber for dispatch.
                    /// </summary>
                    public static {returnType} OneWayBind(
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
            var pathSuffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.SourceExpressionText + "|" + inv.TargetExpressionText);

            sb.AppendLine($$"""
                            {{condition}} (callerLineNumber == {{inv.CallerLineNumber}}
                                && callerFilePath.EndsWith("{{CodeGeneratorHelpers.EscapeString(pathSuffix)}}", global::System.StringComparison.OrdinalIgnoreCase))
                            {
                                return __OneWayBind_{{methodSuffix}}(viewModel, view{{FormatExtraArgs(group)}});
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
    /// Generates a private OneWayBind method for a specific invocation.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="sourceClassInfo">The source type class binding info.</param>
    /// <param name="suffix">The stable method name suffix.</param>
    internal static void GenerateOneWayBindMethod(
        StringBuilder sb,
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        string suffix)
    {
        var viewPropertyAccess = CodeGeneratorHelpers.BuildPropertySetterChain("view", inv.TargetPropertyPath);
        var vmPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.SourcePropertyPath);
        var viewPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.TargetPropertyPath);

        var extraParams = FormatExtraMethodParams(inv);
        var conversionComment = inv.HasConversion ? " (with conversion)" : string.Empty;
        var schedulerComment = inv.HasScheduler ? " (with scheduler)" : string.Empty;
        var returnType = FormatMethodReturnType(inv);

        sb.AppendLine($$"""
                    private static {{returnType}} __OneWayBind_{{suffix}}({{inv.SourceTypeFullName}} viewModel, {{inv.TargetTypeFullName}} view{{extraParams}})
                    {
                        // OneWayBind: {{vmPathComment}} -> {{viewPathComment}}{{conversionComment}}{{schedulerComment}}
            """);

        // Emit inline observation code instead of delegating to WhenChanged dispatch
        ObservationCodeGenerator.EmitInlineObservation(
            sb, "viewModel", inv.SourcePropertyPath, inv.SourcePropertyTypeFullName, sourceClassInfo, "sourceObs");

        if (inv.HasConversion || inv.HasScheduler)
        {
            var currentVar = "sourceObs";

            if (inv.HasConversion)
            {
                var nextVar = inv.HasScheduler ? "__selected" : "bindObs";
                sb.AppendLine($"        var {nextVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({currentVar}, selector);");
                currentVar = nextVar;
            }

            if (inv.HasScheduler)
            {
                sb.AppendLine($"        var bindObs = new global::ReactiveUI.Binding.Reactive.ObserveOnObservable<{inv.TargetPropertyTypeFullName}>({currentVar}, scheduler);");
                currentVar = "bindObs";
            }

            sb.AppendLine($$"""

                        var sub = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe({{currentVar}}, value =>
                        {
                            {{viewPropertyAccess}} = value;
                        });

                        return new global::ReactiveUI.Binding.ReactiveBinding<{{inv.TargetTypeFullName}}, {{inv.TargetPropertyTypeFullName}}>(
                            view,
                            {{currentVar}},
                            global::ReactiveUI.Binding.BindingDirection.OneWay,
                            sub);
                    }
            """)
                .AppendLine();
        }
        else
        {
            sb.AppendLine($$"""

                        var sub = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(sourceObs, value =>
                        {
                            {{viewPropertyAccess}} = value;
                        });

                        return new global::ReactiveUI.Binding.ReactiveBinding<{{inv.TargetTypeFullName}}, {{inv.TargetPropertyTypeFullName}}>(
                            view,
                            sourceObs,
                            global::ReactiveUI.Binding.BindingDirection.OneWay,
                            sub);
                    }
            """)
                .AppendLine();
        }
    }

    /// <summary>
    /// Appends extra parameters (selector, scheduler) to the concrete overload signature.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    internal static void AppendExtraParameters(StringBuilder sb, BindingTypeGroup group)
    {
        if (group.HasConversion)
        {
            sb.AppendLine($"            global::System.Func<{group.SourcePropertyTypeFullName}, {group.TargetPropertyTypeFullName}> selector,");
        }

        if (group.HasScheduler)
        {
            sb.AppendLine("            global::System.Reactive.Concurrency.IScheduler scheduler,");
        }
    }

    /// <summary>
    /// Formats extra arguments (selector, scheduler) for forwarding to the binding method.
    /// </summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>Extra arguments string or empty.</returns>
    internal static string FormatExtraArgs(BindingTypeGroup group)
    {
        var sb = new StringBuilder();
        if (group.HasConversion)
        {
            sb.Append(", selector");
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
    /// <returns>Extra parameters string for selector and scheduler parameters.</returns>
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv)
    {
        var sb = new StringBuilder();
        if (inv.HasConversion)
        {
            sb.Append($", global::System.Func<{inv.SourcePropertyTypeFullName}, {inv.TargetPropertyTypeFullName}> selector");
        }

        if (inv.HasScheduler)
        {
            sb.Append(", global::System.Reactive.Concurrency.IScheduler scheduler");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats the return type for a concrete OneWayBind overload.
    /// </summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>The fully qualified return type string.</returns>
    internal static string FormatReturnType(BindingTypeGroup group)
    {
        var valueType = group.HasConversion ? group.TargetPropertyTypeFullName : group.SourcePropertyTypeFullName;
        return $"global::ReactiveUI.Binding.IReactiveBinding<{group.TargetTypeFullName}, {valueType}>";
    }

    /// <summary>
    /// Formats the return type for a private OneWayBind method.
    /// </summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>The fully qualified return type string.</returns>
    internal static string FormatMethodReturnType(BindingInvocationInfo inv) => $"global::ReactiveUI.Binding.IReactiveBinding<{inv.TargetTypeFullName}, {inv.TargetPropertyTypeFullName}>";

    /// <summary>
    /// Represents a grouping of binding invocations categorized by source type, target type,
    /// source property type, target property type, presence of conversion, and presence of a scheduler.
    /// This record is used to facilitate generating binding method overloads.
    /// </summary>
    /// <param name="SourceTypeFullName">The full name of the source type.</param>
    /// <param name="TargetTypeFullName">The full name of the target type.</param>
    /// <param name="SourcePropertyTypeFullName">The full name of the source property type.</param>
    /// <param name="TargetPropertyTypeFullName">The full name of the target property type.</param>
    /// <param name="HasConversion">A value indicating whether the binding has a conversion.</param>
    /// <param name="HasScheduler">A value indicating whether the binding has a scheduler.</param>
    /// <param name="Invocations">The binding invocations for this group.</param>
    internal sealed record BindingTypeGroup(
        string SourceTypeFullName,
        string TargetTypeFullName,
        string SourcePropertyTypeFullName,
        string TargetPropertyTypeFullName,
        bool HasConversion,
        bool HasScheduler,
        BindingInvocationInfo[] Invocations);
}

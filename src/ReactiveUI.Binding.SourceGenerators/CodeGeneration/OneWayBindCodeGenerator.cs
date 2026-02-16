// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
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

        for (int g = 0; g < groups.Count; g++)
        {
            var group = groups[g];

            GenerateConcreteOverload(sb, group, supportsCallerArgExpr);
            sb.AppendLine();

            for (int i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var sourceClassInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.SourceTypeFullName);
                string suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.SourceExpressionText + "|" + inv.TargetExpressionText);
                GenerateOneWayBindMethod(sb, inv, sourceClassInfo, suffix);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        sb.AppendLine();

        return sb.ToString();
    }

    internal static List<BindingTypeGroup> GroupByTypeSignature(ImmutableArray<BindingInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<BindingInvocationInfo>>();

        for (int i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            string key = inv.SourceTypeFullName + "|" + inv.TargetTypeFullName + "|" +
                inv.SourcePropertyTypeFullName + "|" + inv.TargetPropertyTypeFullName + "|" +
                inv.HasConversion + "|" + inv.HasScheduler;

            if (!groupMap.TryGetValue(key, out var list))
            {
                list = new List<BindingInvocationInfo>();
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

    internal static void GenerateCallerArgExprOverload(
        StringBuilder sb,
        BindingTypeGroup group)
    {
        string returnType = FormatReturnType(group);

        sb.AppendLine($$"""
                    /// <summary>
                    /// Concrete typed overload for OneWayBind from {{group.SourceTypeFullName}} to {{group.TargetTypeFullName}}.
                    /// Uses CallerArgumentExpression for dispatch.
                    /// </summary>
                    public static {{returnType}} OneWayBind(
                        this {{group.TargetTypeFullName}} view,
                        {{group.SourceTypeFullName}} viewModel,
                        global::System.Linq.Expressions.Expression<global::System.Func<{{group.SourceTypeFullName}}, {{group.SourcePropertyTypeFullName}}>> vmProperty,
                        global::System.Linq.Expressions.Expression<global::System.Func<{{group.TargetTypeFullName}}, {{group.TargetPropertyTypeFullName}}>> viewProperty,
            """);

        AppendExtraParameters(sb, group);

        sb.AppendLine("""
                        [global::System.Runtime.CompilerServices.CallerArgumentExpression("vmProperty")] string vmPropertyExpression = "",
                        [global::System.Runtime.CompilerServices.CallerArgumentExpression("viewProperty")] string viewPropertyExpression = "",
                        [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                        [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                    {
            """);

        for (int i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            string condition = i == 0 ? "if" : "else if";
            string escapedSourceExpr = CodeGeneratorHelpers.EscapeString(inv.SourceExpressionText);
            string escapedTargetExpr = CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText);
            string methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.SourceExpressionText + "|" + inv.TargetExpressionText);

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

    internal static void GenerateCallerFilePathOverload(
        StringBuilder sb,
        BindingTypeGroup group)
    {
        string returnType = FormatReturnType(group);

        sb.AppendLine($$"""
                    /// <summary>
                    /// Concrete typed overload for OneWayBind from {{group.SourceTypeFullName}} to {{group.TargetTypeFullName}}.
                    /// Uses CallerFilePath + CallerLineNumber for dispatch.
                    /// </summary>
                    public static {{returnType}} OneWayBind(
                        this {{group.TargetTypeFullName}} view,
                        {{group.SourceTypeFullName}} viewModel,
                        global::System.Linq.Expressions.Expression<global::System.Func<{{group.SourceTypeFullName}}, {{group.SourcePropertyTypeFullName}}>> vmProperty,
                        global::System.Linq.Expressions.Expression<global::System.Func<{{group.TargetTypeFullName}}, {{group.TargetPropertyTypeFullName}}>> viewProperty,
            """);

        AppendExtraParameters(sb, group);

        sb.AppendLine("""
                        [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                        [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                    {
            """);

        for (int i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            string pathSuffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
            string condition = i == 0 ? "if" : "else if";
            string methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.SourceExpressionText + "|" + inv.TargetExpressionText);

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

    internal static void GenerateOneWayBindMethod(
        StringBuilder sb,
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        string suffix)
    {
        string viewPropertyAccess = CodeGeneratorHelpers.BuildPropertySetterChain("view", inv.TargetPropertyPath);
        string vmPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.SourcePropertyPath);
        string viewPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.TargetPropertyPath);

        string extraParams = FormatExtraMethodParams(inv);
        string conversionComment = inv.HasConversion ? " (with conversion)" : string.Empty;
        string schedulerComment = inv.HasScheduler ? " (with scheduler)" : string.Empty;
        string returnType = FormatMethodReturnType(inv);

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
            string currentVar = "sourceObs";

            if (inv.HasConversion)
            {
                string nextVar = inv.HasScheduler ? "__selected" : "bindObs";
                sb.AppendLine($$"""
                        var {{nextVar}} = global::ReactiveUI.Binding.Observables.ObservableExtensions.Select({{currentVar}}, selector);
                """);
                currentVar = nextVar;
            }

            if (inv.HasScheduler)
            {
                sb.AppendLine($$"""
                        var bindObs = new global::ReactiveUI.Binding.Reactive.ObserveOnObservable<{{inv.TargetPropertyTypeFullName}}>({{currentVar}}, scheduler);
                """);
                currentVar = "bindObs";
            }

            sb.AppendLine($$"""

                        var sub = global::ReactiveUI.Binding.Observables.ObservableExtensions.Subscribe({{currentVar}}, value =>
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

                        var sub = global::ReactiveUI.Binding.Observables.ObservableExtensions.Subscribe(sourceObs, value =>
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

    internal static void AppendExtraParameters(StringBuilder sb, BindingTypeGroup group)
    {
        if (group.HasConversion)
        {
            sb.AppendLine($$"""
                            global::System.Func<{{group.SourcePropertyTypeFullName}}, {{group.TargetPropertyTypeFullName}}> selector,
                """);
        }

        if (group.HasScheduler)
        {
            sb.AppendLine("""
                            global::System.Reactive.Concurrency.IScheduler? scheduler,
                """);
        }
    }

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

    internal static string FormatExtraMethodParams(BindingInvocationInfo inv)
    {
        var sb = new StringBuilder();
        if (inv.HasConversion)
        {
            sb.Append($", global::System.Func<{inv.SourcePropertyTypeFullName}, {inv.TargetPropertyTypeFullName}> selector");
        }

        if (inv.HasScheduler)
        {
            sb.Append(", global::System.Reactive.Concurrency.IScheduler? scheduler");
        }

        return sb.ToString();
    }

    internal static string FormatReturnType(BindingTypeGroup group)
    {
        string valueType = group.HasConversion ? group.TargetPropertyTypeFullName : group.SourcePropertyTypeFullName;
        return $"global::ReactiveUI.Binding.IReactiveBinding<{group.TargetTypeFullName}, {valueType}>";
    }

    internal static string FormatMethodReturnType(BindingInvocationInfo inv)
    {
        return $"global::ReactiveUI.Binding.IReactiveBinding<{inv.TargetTypeFullName}, {inv.TargetPropertyTypeFullName}>";
    }

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

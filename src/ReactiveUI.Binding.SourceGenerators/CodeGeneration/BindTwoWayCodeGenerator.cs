// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads and binding methods for BindTwoWay invocations.
/// Supports basic bindings, inline Func converters (source-to-target and target-to-source), and scheduler overloads.
/// </summary>
internal static class BindTwoWayCodeGenerator
{
    /// <summary>
    /// Generates concrete typed overloads and binding methods for BindTwoWay invocations.
    /// </summary>
    /// <param name="invocations">All detected BindTwoWay invocations.</param>
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

        // Group invocations by (SourceType, TargetType, SourcePropertyType, TargetPropertyType, HasConversion, HasScheduler)
        var groups = GroupByTypeSignature(invocations);

        for (int g = 0; g < groups.Count; g++)
        {
            var group = groups[g];

            // Generate the concrete typed extension method overload
            GenerateConcreteOverload(sb, group, supportsCallerArgExpr);
            sb.AppendLine();

            // Generate binding methods
            for (int i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var sourceClassInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.SourceTypeFullName);
                var targetClassInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.TargetTypeFullName);
                string suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.SourceExpressionText + "|" + inv.TargetExpressionText);
                GenerateBindTwoWayMethod(sb, inv, sourceClassInfo, targetClassInfo, suffix);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        sb.AppendLine();

        return sb.ToString();
    }

    internal static List<BindingTypeGroup> GroupByTypeSignature(ImmutableArray<BindingInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<BindingInvocationInfo>>(invocations.Length);
        var keySb = new StringBuilder(128);

        for (int i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            keySb.Clear()
                .Append(inv.SourceTypeFullName).Append('|')
                .Append(inv.TargetTypeFullName).Append('|')
                .Append(inv.SourcePropertyTypeFullName).Append('|')
                .Append(inv.TargetPropertyTypeFullName).Append('|')
                .Append(inv.HasConversion).Append('|')
                .Append(inv.HasScheduler);

            string key = keySb.ToString();

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
        sb.AppendLine($$"""
                    /// <summary>
                    /// Concrete typed overload for BindTwoWay from {{group.SourceTypeFullName}} to {{group.TargetTypeFullName}}.
                    /// Uses CallerArgumentExpression for dispatch.
                    /// </summary>
                    public static global::System.IDisposable BindTwoWay(
                        this {{group.SourceTypeFullName}} source,
                        {{group.TargetTypeFullName}} target,
                        global::System.Linq.Expressions.Expression<global::System.Func<{{group.SourceTypeFullName}}, {{group.SourcePropertyTypeFullName}}>> sourceProperty,
                        global::System.Linq.Expressions.Expression<global::System.Func<{{group.TargetTypeFullName}}, {{group.TargetPropertyTypeFullName}}>> targetProperty,
            """);

        AppendExtraParameters(sb, group);

        sb.AppendLine("""
                        [global::System.Runtime.CompilerServices.CallerArgumentExpression("sourceProperty")] string sourcePropertyExpression = "",
                        [global::System.Runtime.CompilerServices.CallerArgumentExpression("targetProperty")] string targetPropertyExpression = "",
                        [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                        [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                    {
                        sourcePropertyExpression = sourcePropertyExpression.StartsWith("static ") ? sourcePropertyExpression.Substring(7) : sourcePropertyExpression;
                        targetPropertyExpression = targetPropertyExpression.StartsWith("static ") ? targetPropertyExpression.Substring(7) : targetPropertyExpression;

            """);

        for (int i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            string condition = i == 0 ? "if" : "else if";
            string escapedSourceExpr = CodeGeneratorHelpers.EscapeString(inv.SourceExpressionText);
            string escapedTargetExpr = CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText);
            string methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.SourceExpressionText + "|" + inv.TargetExpressionText);

            sb.AppendLine($$"""
                            {{condition}} (sourcePropertyExpression == "{{escapedSourceExpr}}"
                                && targetPropertyExpression == "{{escapedTargetExpr}}")
                            {
                                return __BindTwoWay_{{methodSuffix}}(source, target{{FormatExtraArgs(group)}});
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
        sb.AppendLine($$"""
                    /// <summary>
                    /// Concrete typed overload for BindTwoWay from {{group.SourceTypeFullName}} to {{group.TargetTypeFullName}}.
                    /// Uses CallerFilePath + CallerLineNumber for dispatch.
                    /// </summary>
                    public static global::System.IDisposable BindTwoWay(
                        this {{group.SourceTypeFullName}} source,
                        {{group.TargetTypeFullName}} target,
                        global::System.Linq.Expressions.Expression<global::System.Func<{{group.SourceTypeFullName}}, {{group.SourcePropertyTypeFullName}}>> sourceProperty,
                        global::System.Linq.Expressions.Expression<global::System.Func<{{group.TargetTypeFullName}}, {{group.TargetPropertyTypeFullName}}>> targetProperty,
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
                                return __BindTwoWay_{{methodSuffix}}(source, target{{FormatExtraArgs(group)}});
                            }
                """);
        }

        sb.AppendLine("""
                        throw new global::System.InvalidOperationException(
                            "No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.");
                    }
            """);
    }

    internal static void GenerateBindTwoWayMethod(
        StringBuilder sb,
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        ClassBindingInfo? targetClassInfo,
        string suffix)
    {
        string targetAccess = CodeGeneratorHelpers.BuildPropertySetterChain("target", inv.TargetPropertyPath);
        string sourceSetAccess = CodeGeneratorHelpers.BuildPropertySetterChain("source", inv.SourcePropertyPath);
        string sourcePathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.SourcePropertyPath);
        string targetPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.TargetPropertyPath);

        string extraParams = FormatExtraMethodParams(inv);
        string conversionComment = inv.HasConversion ? " (with conversion)" : string.Empty;
        string schedulerComment = inv.HasScheduler ? " (with scheduler)" : string.Empty;

        sb.AppendLine($$"""
                    private static global::System.IDisposable __BindTwoWay_{{suffix}}({{inv.SourceTypeFullName}} source, {{inv.TargetTypeFullName}} target{{extraParams}})
                    {
                        // BindTwoWay: {{sourcePathComment}} <-> {{targetPathComment}}{{conversionComment}}{{schedulerComment}}
            """);

        // Emit inline observation code instead of delegating to WhenChanged dispatch
        ObservationCodeGenerator.EmitInlineObservation(
            sb, "source", inv.SourcePropertyPath, inv.SourcePropertyTypeFullName, sourceClassInfo, "sourceObs");

        ObservationCodeGenerator.EmitInlineObservation(
            sb, "target", inv.TargetPropertyPath, inv.TargetPropertyTypeFullName, targetClassInfo, "targetObs");

        if (inv.HasConversion || inv.HasScheduler)
        {
            string sourceVar = "sourceObs";
            string targetVar = "targetObs";

            if (inv.HasConversion)
            {
                string srcNext = inv.HasScheduler ? "__srcSelected" : "sourceBind";
                string tgtNext = inv.HasScheduler ? "__tgtSelected" : "targetBind";
                sb.AppendLine($$"""
                        var {{srcNext}} = global::ReactiveUI.Binding.Observables.ObservableExtensions.Select({{sourceVar}}, sourceToTargetConv);
                        var {{tgtNext}} = global::ReactiveUI.Binding.Observables.ObservableExtensions.Select({{targetVar}}, targetToSourceConv);
                """);
                sourceVar = srcNext;
                targetVar = tgtNext;
            }

            if (inv.HasScheduler)
            {
                sb.AppendLine($$"""
                        var sourceBind = new global::ReactiveUI.Binding.Reactive.ObserveOnObservable<{{inv.TargetPropertyTypeFullName}}>({{sourceVar}}, scheduler);
                        var targetBind = new global::ReactiveUI.Binding.Reactive.ObserveOnObservable<{{inv.SourcePropertyTypeFullName}}>({{targetVar}}, scheduler);
                """);
                sourceVar = "sourceBind";
                targetVar = "targetBind";
            }

            sb.AppendLine($$"""

                        var d1 = global::ReactiveUI.Binding.Observables.ObservableExtensions.Subscribe({{sourceVar}}, value =>
                        {
                            {{targetAccess}} = value;
                        });

                        var __targetSkipped = global::ReactiveUI.Binding.Observables.ObservableExtensions.Skip({{targetVar}}, 1);
                        var d2 = global::ReactiveUI.Binding.Observables.ObservableExtensions.Subscribe(__targetSkipped, value =>
                        {
                            {{sourceSetAccess}} = value;
                        });

                        return new global::ReactiveUI.Binding.Observables.CompositeDisposable2(d1, d2);
                    }
            """)
                .AppendLine();
        }
        else
        {
            sb.AppendLine($$"""

                        var d1 = global::ReactiveUI.Binding.Observables.ObservableExtensions.Subscribe(sourceObs, value =>
                        {
                            {{targetAccess}} = value;
                        });

                        var __targetSkipped = global::ReactiveUI.Binding.Observables.ObservableExtensions.Skip(targetObs, 1);
                        var d2 = global::ReactiveUI.Binding.Observables.ObservableExtensions.Subscribe(__targetSkipped, value =>
                        {
                            {{sourceSetAccess}} = value;
                        });

                        return new global::ReactiveUI.Binding.Observables.CompositeDisposable2(d1, d2);
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
            sb.AppendLine($$"""
                            global::System.Func<{{group.SourcePropertyTypeFullName}}, {{group.TargetPropertyTypeFullName}}> sourceToTargetConv,
                            global::System.Func<{{group.TargetPropertyTypeFullName}}, {{group.SourcePropertyTypeFullName}}> targetToSourceConv,
                """);
        }

        if (group.HasScheduler)
        {
            sb.AppendLine("""
                            global::System.Reactive.Concurrency.IScheduler? scheduler,
                """);
        }
    }

    /// <summary>
    /// Formats extra arguments (converters, scheduler) for forwarding to the binding method.
    /// </summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>Extra arguments string like ", sourceToTargetConv, targetToSourceConv, scheduler" or empty.</returns>
    internal static string FormatExtraArgs(BindingTypeGroup group)
    {
        var sb = new StringBuilder();
        if (group.HasConversion)
        {
            sb.Append(", sourceToTargetConv, targetToSourceConv");
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
    /// <returns>Extra parameters string for two-way converter and scheduler parameters.</returns>
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv)
    {
        var sb = new StringBuilder();
        if (inv.HasConversion)
        {
            sb.Append($", global::System.Func<{inv.SourcePropertyTypeFullName}, {inv.TargetPropertyTypeFullName}> sourceToTargetConv")
                .Append($", global::System.Func<{inv.TargetPropertyTypeFullName}, {inv.SourcePropertyTypeFullName}> targetToSourceConv");
        }

        if (inv.HasScheduler)
        {
            sb.Append(", global::System.Reactive.Concurrency.IScheduler? scheduler");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Groups binding invocations by source, target, property types, and overload variant for overload generation.
    /// </summary>
    /// <param name="SourceTypeFullName">The fully qualified name of the source (data) type.</param>
    /// <param name="TargetTypeFullName">The fully qualified name of the target (UI) type.</param>
    /// <param name="SourcePropertyTypeFullName">The fully qualified type of the source property.</param>
    /// <param name="TargetPropertyTypeFullName">The fully qualified type of the target property.</param>
    /// <param name="HasConversion">Whether this group uses inline Func conversion.</param>
    /// <param name="HasScheduler">Whether this group uses an explicit scheduler.</param>
    /// <param name="Invocations">All binding invocations sharing the same type signature.</param>
    internal sealed record BindingTypeGroup(
        string SourceTypeFullName,
        string TargetTypeFullName,
        string SourcePropertyTypeFullName,
        string TargetPropertyTypeFullName,
        bool HasConversion,
        bool HasScheduler,
        BindingInvocationInfo[] Invocations);
}

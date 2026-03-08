// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads and binding methods for BindOneWay invocations.
/// Supports basic bindings, inline Func converters, and scheduler overloads.
/// </summary>
internal static class BindOneWayCodeGenerator
{
    /// <summary>
    /// Generates concrete typed overloads and binding methods for BindOneWay invocations.
    /// </summary>
    /// <param name="invocations">All detected BindOneWay invocations.</param>
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

        // Group invocations by (SourceType, TargetType, SourcePropertyType, TargetPropertyType, HasConversion, HasScheduler)
        var groups = GroupByTypeSignature(invocations);

        for (var g = 0; g < groups.Count; g++)
        {
            var group = groups[g];

            // Generate the concrete typed extension method overload
            GenerateConcreteOverload(sb, group, supportsCallerArgExpr);
            sb.AppendLine();

            // Generate binding methods
            for (var i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var sourceClassInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.SourceTypeFullName);
                var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.SourceExpressionText + "|" + inv.TargetExpressionText);
                GenerateBindOneWayMethod(sb, inv, sourceClassInfo, suffix);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Groups binding invocation information by a unique type signature, producing a collection of grouped results.
    /// </summary>
    /// <param name="invocations">The collection of binding invocation details to be grouped.</param>
    /// <returns>A list of grouped binding type information, where each group shares the same type signature.</returns>
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
    /// Generates a concrete typed extension method overload for a specific group of binding types,
    /// adjusting the generated code based on whether the target language version supports CallerArgumentExpression.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance to which the generated code will be appended.</param>
    /// <param name="group">The group of binding types containing information about source and target members, conversion, and scheduling.</param>
    /// <param name="supportsCallerArgExpr">Indicates whether the CallerArgumentExpression feature is supported by the target language version.</param>
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
    /// Generates a concrete typed overload for the BindOneWay method, allowing bindings between source and target types
    /// while utilizing CallerArgumentExpression for enhanced debugging and context.
    /// </summary>
    /// <param name="sb">The StringBuilder instance used to append the generated source code.</param>
    /// <param name="group">The grouping of binding-related type and property information required to generate the overload.</param>
    internal static void GenerateCallerArgExprOverload(
        StringBuilder sb,
        BindingTypeGroup group)
    {
        sb.AppendLine($"""
                    /// <summary>
                    /// Concrete typed overload for BindOneWay from {group.SourceTypeFullName} to {group.TargetTypeFullName}.
                    /// Uses CallerArgumentExpression for dispatch.
                    /// </summary>
                    public static global::System.IDisposable BindOneWay(
                        this {group.SourceTypeFullName} source,
                        {group.TargetTypeFullName} target,
                        global::System.Linq.Expressions.Expression<global::System.Func<{group.SourceTypeFullName}, {group.SourcePropertyTypeFullName}>> sourceProperty,
                        global::System.Linq.Expressions.Expression<global::System.Func<{group.TargetTypeFullName}, {group.TargetPropertyTypeFullName}>> targetProperty,
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

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.SourceExpressionText + "|" + inv.TargetExpressionText);
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);
            var escapedSourceExpr = CodeGeneratorHelpers.EscapeString(inv.SourceExpressionText);
            var escapedTargetExpr = CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText);

            sb.AppendLine($$"""
                            {{condition}} (sourcePropertyExpression == "{{escapedSourceExpr}}"
                                && targetPropertyExpression == "{{escapedTargetExpr}}")
                            {
                                return __BindOneWay_{{methodSuffix}}(source, target{{FormatExtraArgs(group)}});
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
    /// Generates a BindOneWay overload for a specific binding type group,
    /// enabling bindings that utilize CallerFilePath and CallerLineNumber for diagnostics and dispatch.
    /// </summary>
    /// <param name="sb">The StringBuilder instance used to generate the source code.</param>
    /// <param name="group">Details of the source and target types involved in the binding, including property types and other metadata.</param>
    internal static void GenerateCallerFilePathOverload(
        StringBuilder sb,
        BindingTypeGroup group)
    {
        sb.AppendLine($"""
                    /// <summary>
                    /// Concrete typed overload for BindOneWay from {group.SourceTypeFullName} to {group.TargetTypeFullName}.
                    /// Uses CallerFilePath + CallerLineNumber for dispatch.
                    /// </summary>
                    public static global::System.IDisposable BindOneWay(
                        this {group.SourceTypeFullName} source,
                        {group.TargetTypeFullName} target,
                        global::System.Linq.Expressions.Expression<global::System.Func<{group.SourceTypeFullName}, {group.SourcePropertyTypeFullName}>> sourceProperty,
                        global::System.Linq.Expressions.Expression<global::System.Func<{group.TargetTypeFullName}, {group.TargetPropertyTypeFullName}>> targetProperty,
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
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.SourceExpressionText + "|" + inv.TargetExpressionText);
            var pathSuffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);

            sb.AppendLine($$"""
                            {{condition}} (callerLineNumber == {{inv.CallerLineNumber}}
                                && callerFilePath.EndsWith("{{CodeGeneratorHelpers.EscapeString(pathSuffix)}}", global::System.StringComparison.OrdinalIgnoreCase))
                            {
                                return __BindOneWay_{{methodSuffix}}(source, target{{FormatExtraArgs(group)}});
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
    /// Generates the BindOneWay method used for binding a source property to a target property with optional conversion and scheduler.
    /// </summary>
    /// <param name="sb">The StringBuilder for appending the generated code.</param>
    /// <param name="inv">The invocation information containing details about the binding.</param>
    /// <param name="sourceClassInfo">The class binding information of the source, or null if not applicable.</param>
    /// <param name="suffix">The suffix to append to the generated method name for uniqueness.</param>
    internal static void GenerateBindOneWayMethod(
        StringBuilder sb,
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        string suffix)
    {
        var targetAccess = CodeGeneratorHelpers.BuildPropertySetterChain("target", inv.TargetPropertyPath);
        var sourcePathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.SourcePropertyPath);
        var targetPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.TargetPropertyPath);

        var extraParams = FormatExtraMethodParams(inv);
        var conversionComment = inv.HasConversion ? " (with conversion)" : string.Empty;
        var schedulerComment = inv.HasScheduler ? " (with scheduler)" : string.Empty;

        sb.AppendLine($$"""
                    private static global::System.IDisposable __BindOneWay_{{suffix}}({{inv.SourceTypeFullName}} source, {{inv.TargetTypeFullName}} target{{extraParams}})
                    {
                        // BindOneWay: {{sourcePathComment}} -> {{targetPathComment}}{{conversionComment}}{{schedulerComment}}
            """);

        // Emit inline observation code instead of delegating to WhenChanged dispatch
        ObservationCodeGenerator.EmitInlineObservation(
            sb, "source", inv.SourcePropertyPath, inv.SourcePropertyTypeFullName, sourceClassInfo, "sourceObs");

        if (inv.HasConversion || inv.HasScheduler)
        {
            var currentVar = "sourceObs";

            if (inv.HasConversion)
            {
                var nextVar = inv.HasScheduler ? "__selected" : "bindObs";
                sb.AppendLine($"        var {nextVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({currentVar}, conversionFunc);");
                currentVar = nextVar;
            }

            if (inv.HasScheduler)
            {
                sb.AppendLine($"        var bindObs = new global::ReactiveUI.Binding.Reactive.ObserveOnObservable<{inv.TargetPropertyTypeFullName}>({currentVar}, scheduler);");
                currentVar = "bindObs";
            }

            sb.AppendLine($$"""

                        return global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe({{currentVar}}, value =>
                        {
                            {{targetAccess}} = value;
                        });
                    }
            """)
                .AppendLine();
        }
        else
        {
            sb.AppendLine($$"""

                        return global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(sourceObs, value =>
                        {
                            {{targetAccess}} = value;
                        });
                    }
            """)
                .AppendLine();
        }
    }

    /// <summary>
    /// Appends extra parameters (converter, scheduler) to the concrete overload signature.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    internal static void AppendExtraParameters(StringBuilder sb, BindingTypeGroup group)
    {
        if (group.HasConversion)
        {
            sb.AppendLine($"            global::System.Func<{group.SourcePropertyTypeFullName}, {group.TargetPropertyTypeFullName}> conversionFunc,");
        }

        if (group.HasScheduler)
        {
            sb.AppendLine("            global::System.Reactive.Concurrency.IScheduler scheduler,");
        }
    }

    /// <summary>
    /// Formats extra arguments (converter, scheduler) for forwarding to the binding method.
    /// </summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>Extra arguments string like ", conversionFunc, scheduler" or empty.</returns>
    internal static string FormatExtraArgs(BindingTypeGroup group)
    {
        var sb = new StringBuilder();
        if (group.HasConversion)
        {
            sb.Append(", conversionFunc");
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
    /// <returns>Extra parameters string like ", Func&lt;int, string&gt; conversionFunc, IScheduler scheduler" or empty.</returns>
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv)
    {
        var sb = new StringBuilder();
        if (inv.HasConversion)
        {
            sb.Append($", global::System.Func<{inv.SourcePropertyTypeFullName}, {inv.TargetPropertyTypeFullName}> conversionFunc");
        }

        if (inv.HasScheduler)
        {
            sb.Append(", global::System.Reactive.Concurrency.IScheduler scheduler");
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

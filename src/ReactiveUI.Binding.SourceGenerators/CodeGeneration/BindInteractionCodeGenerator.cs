// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads and binding methods for BindInteraction invocations.
/// </summary>
internal static class BindInteractionCodeGenerator
{
    /// <summary>
    /// Generates concrete typed overloads and binding methods for BindInteraction invocations.
    /// </summary>
    /// <param name="invocations">All detected BindInteraction invocations.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="supportsCallerArgExpr">Whether CallerArgumentExpression (C# 10+) is available.</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    internal static string? Generate(
        ImmutableArray<BindInteractionInvocationInfo> invocations,
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
                var vmClassInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.ViewModelTypeFullName);
                var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                    inv.ViewTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.ExpressionText);
                GenerateBindInteractionMethod(sb, inv, vmClassInfo, suffix);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Groups BindInteraction invocations by their type signature for overload generation.
    /// </summary>
    /// <param name="invocations">The BindInteraction invocations to group.</param>
    /// <returns>A list of grouped invocations sharing the same type signature.</returns>
    internal static List<BindInteractionTypeGroup> GroupByTypeSignature(ImmutableArray<BindInteractionInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<BindInteractionInvocationInfo>>(invocations.Length);
        var keySb = new StringBuilder(128);

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            keySb.Clear()
                .Append(inv.ViewTypeFullName).Append('|')
                .Append(inv.ViewModelTypeFullName).Append('|')
                .Append(inv.InputTypeFullName).Append('|')
                .Append(inv.OutputTypeFullName).Append('|')
                .Append(inv.IsTaskHandler);

            var key = keySb.ToString();

            if (!groupMap.TryGetValue(key, out var list))
            {
                list = [];
                groupMap[key] = list;
            }

            list.Add(inv);
        }

        var result = new List<BindInteractionTypeGroup>();
        foreach (var kvp in groupMap)
        {
            var first = kvp.Value[0];
            result.Add(new BindInteractionTypeGroup(
                first.ViewTypeFullName,
                first.ViewModelTypeFullName,
                first.InputTypeFullName,
                first.OutputTypeFullName,
                first.IsTaskHandler,
                first.DontCareTypeFullName,
                kvp.Value.ToArray()));
        }

        return result;
    }

    /// <summary>
    /// Generates the concrete typed overload using the appropriate dispatch strategy.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindInteraction type group.</param>
    /// <param name="supportsCallerArgExpr">Whether CallerArgumentExpression is available.</param>
    internal static void GenerateConcreteOverload(
        StringBuilder sb,
        BindInteractionTypeGroup group,
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
    /// Generates the CallerArgumentExpression-based overload for BindInteraction dispatch.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindInteraction type group.</param>
    internal static void GenerateCallerArgExprOverload(
        StringBuilder sb,
        BindInteractionTypeGroup group)
    {
        var handlerType = group.IsTaskHandler
            ? $"global::System.Func<global::ReactiveUI.Binding.IInteractionContext<{group.InputTypeFullName}, {group.OutputTypeFullName}>, global::System.Threading.Tasks.Task>"
            : $"global::System.Func<global::ReactiveUI.Binding.IInteractionContext<{group.InputTypeFullName}, {group.OutputTypeFullName}>, global::System.IObservable<{group.DontCareTypeFullName}>>";

        sb.AppendLine($$"""
                    /// <summary>
                    /// Concrete typed overload for BindInteraction on {{group.ViewTypeFullName}}.
                    /// Uses CallerArgumentExpression for dispatch.
                    /// </summary>
                    public static global::System.IDisposable BindInteraction(
                        this {{group.ViewTypeFullName}} view,
                        {{group.ViewModelTypeFullName}} viewModel,
                        global::System.Linq.Expressions.Expression<global::System.Func<{{group.ViewModelTypeFullName}}, global::ReactiveUI.Binding.IInteraction<{{group.InputTypeFullName}}, {{group.OutputTypeFullName}}>>> propertyName,
                        {{handlerType}} handler,
                        [global::System.Runtime.CompilerServices.CallerArgumentExpression("propertyName")] string propertyNameExpression = "",
                        [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                        [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                    {
                        propertyNameExpression = propertyNameExpression.StartsWith("static ") ? propertyNameExpression.Substring(7) : propertyNameExpression;

            """);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.ViewTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.ExpressionText);
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);
            var escapedExpr = CodeGeneratorHelpers.EscapeString(inv.ExpressionText);

            sb.AppendLine($$"""
                            {{condition}} (propertyNameExpression == "{{escapedExpr}}")
                            {
                                return __BindInteraction_{{methodSuffix}}(view, viewModel, handler);
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
    /// Generates the CallerFilePath-based overload for BindInteraction dispatch.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindInteraction type group.</param>
    internal static void GenerateCallerFilePathOverload(
        StringBuilder sb,
        BindInteractionTypeGroup group)
    {
        var handlerType = group.IsTaskHandler
            ? $"global::System.Func<global::ReactiveUI.Binding.IInteractionContext<{group.InputTypeFullName}, {group.OutputTypeFullName}>, global::System.Threading.Tasks.Task>"
            : $"global::System.Func<global::ReactiveUI.Binding.IInteractionContext<{group.InputTypeFullName}, {group.OutputTypeFullName}>, global::System.IObservable<{group.DontCareTypeFullName}>>";

        sb.AppendLine($$"""
                    /// <summary>
                    /// Concrete typed overload for BindInteraction on {{group.ViewTypeFullName}}.
                    /// Uses CallerFilePath + CallerLineNumber for dispatch.
                    /// </summary>
                    public static global::System.IDisposable BindInteraction(
                        this {{group.ViewTypeFullName}} view,
                        {{group.ViewModelTypeFullName}} viewModel,
                        global::System.Linq.Expressions.Expression<global::System.Func<{{group.ViewModelTypeFullName}}, global::ReactiveUI.Binding.IInteraction<{{group.InputTypeFullName}}, {{group.OutputTypeFullName}}>>> propertyName,
                        {{handlerType}} handler,
                        [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                        [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                    {
            """);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.ViewTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, inv.ExpressionText);
            var pathSuffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);

            sb.AppendLine($$"""
                            {{condition}} (callerLineNumber == {{inv.CallerLineNumber}}
                                && callerFilePath.EndsWith("{{CodeGeneratorHelpers.EscapeString(pathSuffix)}}", global::System.StringComparison.OrdinalIgnoreCase))
                            {
                                return __BindInteraction_{{methodSuffix}}(view, viewModel, handler);
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
    /// Generates a private BindInteraction method for a specific invocation.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The BindInteraction invocation info.</param>
    /// <param name="vmClassInfo">The view model type class binding info.</param>
    /// <param name="suffix">The stable method name suffix.</param>
    internal static void GenerateBindInteractionMethod(
        StringBuilder sb,
        BindInteractionInvocationInfo inv,
        ClassBindingInfo? vmClassInfo,
        string suffix)
    {
        var handlerType = inv.IsTaskHandler
            ? $"global::System.Func<global::ReactiveUI.Binding.IInteractionContext<{inv.InputTypeFullName}, {inv.OutputTypeFullName}>, global::System.Threading.Tasks.Task>"
            : $"global::System.Func<global::ReactiveUI.Binding.IInteractionContext<{inv.InputTypeFullName}, {inv.OutputTypeFullName}>, global::System.IObservable<{inv.DontCareTypeFullName}>>";

        var interactionType = $"global::ReactiveUI.Binding.IInteraction<{inv.InputTypeFullName}, {inv.OutputTypeFullName}>";
        var pathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.InteractionPropertyPath);

        sb.AppendLine($$"""
                    private static global::System.IDisposable __BindInteraction_{{suffix}}(
                        {{inv.ViewTypeFullName}} view,
                        {{inv.ViewModelTypeFullName}} viewModel,
                        {{handlerType}} handler)
                    {
                        // BindInteraction: {{pathComment}}
                        var serial = new global::ReactiveUI.Binding.Observables.SerialDisposable();

            """);

        // Emit inline observation of the interaction property on the view model
        // We need to handle the case where viewModel is null
        if (inv.InteractionPropertyPath.Length == 1)
        {
            var propertyName = inv.InteractionPropertyPath[0].PropertyName;
            var isINPC = ObservationCodeGenerator.IsINPC(vmClassInfo);

            if (isINPC)
            {
                sb.AppendLine($$"""
                        if (viewModel == null)
                        {
                            return serial;
                        }

                        var interactionObs = new global::ReactiveUI.Binding.Observables.PropertyObservable<{{interactionType}}>(
                            viewModel,
                            "{{propertyName}}",
                            (global::System.ComponentModel.INotifyPropertyChanged __o) => (({{inv.ViewModelTypeFullName}})__o).{{propertyName}},
                            true);
                """);
            }
            else
            {
                sb.AppendLine($$"""
                        if (viewModel == null)
                        {
                            return serial;
                        }

                        var interactionObs = new global::ReactiveUI.Binding.Observables.ReturnObservable<{{interactionType}}>(viewModel.{{propertyName}});
                """);
            }
        }
        else
        {
            // Deep path — emit full chain observation using ObservationCodeGenerator pattern
            sb.AppendLine("""
                        if (viewModel == null)
                        {
                            return serial;
                        }
                """);

            ObservationCodeGenerator.EmitInlineObservation(
                sb, "viewModel", inv.InteractionPropertyPath, interactionType, vmClassInfo, "interactionObs");
        }

        // Subscribe to the interaction observable and register the handler
        var registerCall = inv.IsTaskHandler
            ? "interaction.RegisterHandler(handler)"
            : "interaction.RegisterHandler(handler)";

        sb.AppendLine($$"""

                        var sub = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(interactionObs, interaction =>
                        {
                            serial.Disposable = interaction != null
                                ? {{registerCall}}
                                : global::ReactiveUI.Binding.Observables.EmptyDisposable.Instance;
                        });
                        return new global::ReactiveUI.Binding.Observables.CompositeDisposable2(sub, serial);
                    }
            """)
            .AppendLine();
    }

    /// <summary>
    /// Groups BindInteraction invocations by type signature for overload generation.
    /// </summary>
    internal sealed record BindInteractionTypeGroup(
        string ViewTypeFullName,
        string ViewModelTypeFullName,
        string InputTypeFullName,
        string OutputTypeFullName,
        bool IsTaskHandler,
        string? DontCareTypeFullName,
        BindInteractionInvocationInfo[] Invocations);
}

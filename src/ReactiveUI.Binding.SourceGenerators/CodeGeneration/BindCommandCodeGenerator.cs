// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;

using ReactiveUI.Binding.SourceGenerators.Generators.CommandBinding;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads and binding methods for BindCommand invocations.
/// </summary>
internal static class BindCommandCodeGenerator
{
    /// <summary>
    /// Generates concrete typed overloads and binding methods for BindCommand invocations.
    /// </summary>
    /// <param name="invocations">All detected BindCommand invocations.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="supportsCallerArgExpr">Whether CallerArgumentExpression (C# 10+) is available.</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    internal static string? Generate(
        ImmutableArray<BindCommandInvocationInfo> invocations,
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
                    inv.ViewTypeFullName,
                    inv.CallerFilePath,
                    inv.CallerLineNumber,
                    inv.CommandExpressionText + "|" + inv.ControlExpressionText);
                GenerateBindCommandMethod(sb, inv, vmClassInfo, suffix);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Groups BindCommand invocations by their type signature for overload generation.
    /// </summary>
    /// <param name="invocations">The BindCommand invocations to group.</param>
    /// <returns>A list of grouped invocations sharing the same type signature.</returns>
    internal static List<BindCommandTypeGroup> GroupByTypeSignature(ImmutableArray<BindCommandInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<BindCommandInvocationInfo>>(invocations.Length);
        var keySb = new StringBuilder(128);

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            keySb.Clear()
                .Append(inv.ViewTypeFullName).Append('|')
                .Append(inv.ViewModelTypeFullName).Append('|')
                .Append(inv.CommandTypeFullName).Append('|')
                .Append(inv.ControlTypeFullName).Append('|')
                .Append(inv.HasObservableParameter).Append('|')
                .Append(inv.HasExpressionParameter).Append('|')
                .Append(inv.ParameterTypeFullName ?? string.Empty);

            var key = keySb.ToString();

            if (!groupMap.TryGetValue(key, out var list))
            {
                list = [];
                groupMap[key] = list;
            }

            list.Add(inv);
        }

        var result = new List<BindCommandTypeGroup>();
        foreach (var kvp in groupMap)
        {
            var first = kvp.Value[0];
            result.Add(new BindCommandTypeGroup(
                first.ViewTypeFullName,
                first.ViewModelTypeFullName,
                first.CommandTypeFullName,
                first.ControlTypeFullName,
                first.HasObservableParameter,
                first.HasExpressionParameter,
                first.ParameterTypeFullName,
                kvp.Value.ToArray()));
        }

        return result;
    }

    /// <summary>
    /// Generates the concrete typed overload using the appropriate dispatch strategy.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindCommand type group.</param>
    /// <param name="supportsCallerArgExpr">Whether CallerArgumentExpression is available.</param>
    internal static void GenerateConcreteOverload(
        StringBuilder sb,
        BindCommandTypeGroup group,
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
    /// Generates the CallerArgumentExpression-based overload for BindCommand dispatch.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindCommand type group.</param>
    internal static void GenerateCallerArgExprOverload(
        StringBuilder sb,
        BindCommandTypeGroup group)
    {
        sb.AppendLine($"""
                    /// <summary>
                    /// Concrete typed overload for BindCommand on {group.ViewTypeFullName}.
                    /// Uses CallerArgumentExpression for dispatch.
                    /// </summary>
                    public static global::System.IDisposable BindCommand(
                        this {group.ViewTypeFullName} view,
                        {group.ViewModelTypeFullName} viewModel,
                        global::System.Linq.Expressions.Expression<global::System.Func<{group.ViewModelTypeFullName}, {group.CommandTypeFullName}>> propertyName,
                        global::System.Linq.Expressions.Expression<global::System.Func<{group.ViewTypeFullName}, {group.ControlTypeFullName}>> controlName,
            """);

        if (group.HasObservableParameter)
        {
            sb.AppendLine($"            global::System.IObservable<{group.ParameterTypeFullName}> withParameter,");
        }
        else if (group.HasExpressionParameter)
        {
            sb.AppendLine($"            global::System.Linq.Expressions.Expression<global::System.Func<{group.ViewModelTypeFullName}, {group.ParameterTypeFullName}>> withParameter,");
        }

        sb.AppendLine("""
                        string toEvent = null,
                        [global::System.Runtime.CompilerServices.CallerArgumentExpression("propertyName")] string propertyNameExpression = "",
                        [global::System.Runtime.CompilerServices.CallerArgumentExpression("controlName")] string controlNameExpression = "",
            """);

        if (group.HasExpressionParameter)
        {
            sb.AppendLine("""
                        [global::System.Runtime.CompilerServices.CallerArgumentExpression("withParameter")] string withParameterExpression = "",
            """);
        }

        sb.AppendLine("""
                        [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                        [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                    {
                        propertyNameExpression = propertyNameExpression.StartsWith("static ") ? propertyNameExpression.Substring(7) : propertyNameExpression;
                        controlNameExpression = controlNameExpression.StartsWith("static ") ? controlNameExpression.Substring(7) : controlNameExpression;

            """);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.ViewTypeFullName,
                inv.CallerFilePath,
                inv.CallerLineNumber,
                inv.CommandExpressionText + "|" + inv.ControlExpressionText);
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);
            var escapedCmdExpr = CodeGeneratorHelpers.EscapeString(inv.CommandExpressionText);
            var escapedCtrlExpr = CodeGeneratorHelpers.EscapeString(inv.ControlExpressionText);

            var extraArgs = group.HasObservableParameter || group.HasExpressionParameter
                ? ", withParameter" : string.Empty;

            sb.AppendLine($$"""
                            {{condition}} (propertyNameExpression == "{{escapedCmdExpr}}"
                                && controlNameExpression == "{{escapedCtrlExpr}}")
                            {
                                return __BindCommand_{{methodSuffix}}(view, viewModel{{extraArgs}});
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
    /// Generates the CallerFilePath-based overload for BindCommand dispatch.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindCommand type group.</param>
    internal static void GenerateCallerFilePathOverload(
        StringBuilder sb,
        BindCommandTypeGroup group)
    {
        sb.AppendLine($"""
                    /// <summary>
                    /// Concrete typed overload for BindCommand on {group.ViewTypeFullName}.
                    /// Uses CallerFilePath + CallerLineNumber for dispatch.
                    /// </summary>
                    public static global::System.IDisposable BindCommand(
                        this {group.ViewTypeFullName} view,
                        {group.ViewModelTypeFullName} viewModel,
                        global::System.Linq.Expressions.Expression<global::System.Func<{group.ViewModelTypeFullName}, {group.CommandTypeFullName}>> propertyName,
                        global::System.Linq.Expressions.Expression<global::System.Func<{group.ViewTypeFullName}, {group.ControlTypeFullName}>> controlName,
            """);

        if (group.HasObservableParameter)
        {
            sb.AppendLine($"            global::System.IObservable<{group.ParameterTypeFullName}> withParameter,");
        }
        else if (group.HasExpressionParameter)
        {
            sb.AppendLine($"            global::System.Linq.Expressions.Expression<global::System.Func<{group.ViewModelTypeFullName}, {group.ParameterTypeFullName}>> withParameter,");
        }

        sb.AppendLine("""
                        string toEvent = null,
                        [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                        [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                    {
            """);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.ViewTypeFullName,
                inv.CallerFilePath,
                inv.CallerLineNumber,
                inv.CommandExpressionText + "|" + inv.ControlExpressionText);
            var pathSuffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);

            var extraArgs = group.HasObservableParameter || group.HasExpressionParameter
                ? ", withParameter" : string.Empty;

            sb.AppendLine($$"""
                            {{condition}} (callerLineNumber == {{inv.CallerLineNumber}}
                                && callerFilePath.EndsWith("{{CodeGeneratorHelpers.EscapeString(pathSuffix)}}", global::System.StringComparison.OrdinalIgnoreCase))
                            {
                                return __BindCommand_{{methodSuffix}}(view, viewModel{{extraArgs}});
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
    /// Generates a private BindCommand method for a specific invocation.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="vmClassInfo">The view model type class binding info.</param>
    /// <param name="suffix">The stable method name suffix.</param>
    internal static void GenerateBindCommandMethod(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        ClassBindingInfo? vmClassInfo,
        string suffix)
    {
        var cmdPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.CommandPropertyPath);
        var ctrlPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.ControlPropertyPath);

        var extraParams = string.Empty;
        if (inv.HasObservableParameter)
        {
            extraParams = $", global::System.IObservable<{inv.ParameterTypeFullName}> withParameter";
        }
        else if (inv.HasExpressionParameter)
        {
            extraParams = $", global::System.Linq.Expressions.Expression<global::System.Func<{inv.ViewModelTypeFullName}, {inv.ParameterTypeFullName}>> withParameter";
        }

        sb.AppendLine($$"""
                    private static global::System.IDisposable __BindCommand_{{suffix}}(
                        {{inv.ViewTypeFullName}} view,
                        {{inv.ViewModelTypeFullName}} viewModel{{extraParams}})
                    {
                        // BindCommand: {{cmdPathComment}} -> {{ctrlPathComment}} (event: {{inv.ResolvedEventName ?? "none"}})
                        if (viewModel == null)
                        {
                            return global::ReactiveUI.Binding.Observables.EmptyDisposable.Instance;
                        }

            """);

        // Get the control access chain
        var controlAccess = CodeGeneratorHelpers.BuildPropertyAccessChain("view", inv.ControlPropertyPath);

        // Emit command observation (for rebinding when command property changes)
        ObservationCodeGenerator.EmitInlineObservation(
            sb, "viewModel", inv.CommandPropertyPath, inv.CommandTypeFullName, vmClassInfo, "commandObs");

        // Try plugins in affinity order (highest first)
        if (CommandPropertyBindingPlugin.CanHandle(inv))
        {
            CommandPropertyBindingPlugin.EmitBinding(sb, inv, controlAccess);
        }
        else if (EventEnabledBindingPlugin.CanHandle(inv))
        {
            // Emit custom binder check before event+Enabled binding
            EmitCustomBinderFallback(sb, inv, controlAccess, hasEvent: true);
            EventEnabledBindingPlugin.EmitBinding(sb, inv, controlAccess);
        }
        else if (DefaultEventBindingPlugin.CanHandle(inv))
        {
            // Emit custom binder check before basic event binding
            EmitCustomBinderFallback(sb, inv, controlAccess, hasEvent: true);
            DefaultEventBindingPlugin.EmitBinding(sb, inv, controlAccess);
        }
        else
        {
            // No plugin matched — emit runtime fallback for custom binders, then throw
            EmitCustomBinderFallback(sb, inv, controlAccess, hasEvent: false);
            sb.AppendLine("""
                        throw new global::System.InvalidOperationException(
                            "No bindable event found on the control. Specify the 'toEvent' parameter.");
                    }
            """);
        }

        sb.AppendLine();
    }

    /// <summary>
    /// Emits the custom binder check that tries registered <c>ICreatesCommandBinding</c> binders
    /// before falling through to the generated event subscription code.
    /// </summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain (e.g., "view.MyButton").</param>
    /// <param name="hasEvent">Whether a resolved event was found at compile time.</param>
    internal static void EmitCustomBinderFallback(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        bool hasEvent)
    {
        // Build the parameter observable expression for the custom binder
        string paramObsExpr;
        if (inv.HasObservableParameter)
        {
            // Cast the typed observable to IObservable<object> via Select
            paramObsExpr = "new global::ReactiveUI.Binding.Observables.SelectObservable<"
                + inv.ParameterTypeFullName + ", object>(withParameter, __p => __p)";
        }
        else if (inv is { HasExpressionParameter: true, ParameterPropertyPath: not null })
        {
            // Read the parameter property at call time
            var paramAccess = CodeGeneratorHelpers.BuildPropertyAccessChain("viewModel", inv.ParameterPropertyPath.Value);
            paramObsExpr = "new global::ReactiveUI.Binding.Observables.ReturnObservable<object>(" + paramAccess + ")";
        }
        else
        {
            paramObsExpr = "global::ReactiveUI.Binding.Observables.EmptyObservable<object>.Instance";
        }

        sb.AppendLine($$"""

                        var __customBinder = global::ReactiveUI.Binding.CommandBinding.CommandBinderService
                            .GetBinder<{{inv.ControlTypeFullName}}>({{(hasEvent ? "true" : "false")}});
                        if (__customBinder != null)
                        {
                            var __serial = new global::ReactiveUI.Binding.Observables.SerialDisposable();
                            var __binderCmdSub = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Subscribe(commandObs, __cmd =>
                            {
                                __serial.Disposable = global::ReactiveUI.Binding.Observables.EmptyDisposable.Instance;
                                global::System.IObservable<object> __paramObs = {{paramObsExpr}};
                                __serial.Disposable = __customBinder.BindCommandToObject<{{inv.ControlTypeFullName}}>(
                                    __cmd, {{controlAccess}}, __paramObs)
                                    ?? global::ReactiveUI.Binding.Observables.EmptyDisposable.Instance;
                            });
                            return new global::ReactiveUI.Binding.Observables.CompositeDisposable2(__binderCmdSub, __serial);
                        }

            """);
    }

    /// <summary>
    /// Groups BindCommand invocations by type signature for overload generation.
    /// </summary>
    internal sealed record BindCommandTypeGroup(
        string ViewTypeFullName,
        string ViewModelTypeFullName,
        string CommandTypeFullName,
        string ControlTypeFullName,
        bool HasObservableParameter,
        bool HasExpressionParameter,
        string? ParameterTypeFullName,
        BindCommandInvocationInfo[] Invocations);
}

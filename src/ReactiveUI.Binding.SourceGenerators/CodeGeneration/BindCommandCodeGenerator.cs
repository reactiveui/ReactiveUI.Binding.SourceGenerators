// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Generates concrete typed extension method overloads and binding methods for BindCommand invocations.</summary>
internal static class BindCommandCodeGenerator
{
    /// <summary>Generates concrete typed overloads and binding methods for BindCommand invocations.</summary>
    /// <param name="invocations">All detected BindCommand invocations.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    internal static string? Generate(
        ImmutableArray<BindCommandInvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        LanguageFeatures features)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return null;
        }

        var sb = new StringBuilder(invocations.Length * CodeGeneratorHelpers.PerInvocationBufferCapacity);
        var supportsCallerArgExpr = features.SupportsCallerArgExpr;
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb, features);
        _ = sb.AppendLine();

        var groups = GroupByTypeSignature(invocations);

        for (var g = 0; g < groups.Count; g++)
        {
            var group = groups[g];

            GenerateConcreteOverload(sb, group, supportsCallerArgExpr, features.SupportsNullable);
            _ = sb.AppendLine();

            for (var i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var viewModelClassInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.ViewModelTypeFullName);
                var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                    inv.ViewTypeFullName,
                    inv.CallerFilePath,
                    inv.CallerLineNumber,
                    $"{inv.CommandExpressionText}|{inv.ControlExpressionText}");
                GenerateBindCommandMethod(sb, inv, viewModelClassInfo, suffix, features.SupportsNullable);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        _ = sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>Groups BindCommand invocations by their type signature for overload generation.</summary>
    /// <param name="invocations">The BindCommand invocations to group.</param>
    /// <returns>A list of grouped invocations sharing the same type signature.</returns>
    internal static List<BindCommandTypeGroup> GroupByTypeSignature(
        ImmutableArray<BindCommandInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<BindCommandInvocationInfo>>(invocations.Length);
        var keySb = new StringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            _ = keySb.Clear()
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
            result.Add(new(
                first.ViewTypeFullName,
                first.ViewModelTypeFullName,
                first.CommandTypeFullName,
                first.ControlTypeFullName,
                first.HasObservableParameter,
                first.HasExpressionParameter,
                first.ParameterTypeFullName,
                [.. kvp.Value]));
        }

        return result;
    }

    /// <summary>Generates the concrete typed overload using the appropriate dispatch strategy.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindCommand type group.</param>
    /// <param name="supportsCallerArgExpr">Whether CallerArgumentExpression is available.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void GenerateConcreteOverload(
        StringBuilder sb,
        BindCommandTypeGroup group,
        bool supportsCallerArgExpr,
        bool supportsNullable)
    {
        if (supportsCallerArgExpr)
        {
            GenerateCallerArgExprOverload(sb, group, supportsNullable);
        }
        else
        {
            GenerateCallerFilePathOverload(sb, group, supportsNullable);
        }
    }

    /// <summary>Generates the CallerArgumentExpression-based overload for BindCommand dispatch.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindCommand type group.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void GenerateCallerArgExprOverload(
        StringBuilder sb,
        BindCommandTypeGroup group,
        bool supportsNullable)
    {
        // The command selector is nullable (a command property may be null), matching the runtime stub's
        // Expression<Func<TViewModel, TProp?>>. The control selector stays non-nullable to match the stub's
        // Expression<Func<TView, TControl>> (TControl is a non-null class) so overload resolution selects this
        // generated overload instead of falling through to the runtime fallback.
        var commandType = CodeGeneratorHelpers.NullableSelectorLeafType(group.Invocations[0].CommandPropertyPath, supportsNullable);
        var controlType = group.ControlTypeFullName;

        // The expression-form command-parameter selector is nullable for reference-type parameters, matching the
        // runtime stub's Expression<Func<TViewModel, TParam?>> so the parameter lambda may return null without CS8603.
        var withParameterExprType = supportsNullable && group.Invocations[0].ParameterIsReferenceType
            ? $"{group.ParameterTypeFullName}?"
            : group.ParameterTypeFullName;
        _ = sb.AppendLine($"""
                               /// <summary>
                               /// Concrete typed overload for BindCommand on {group.ViewTypeFullName}.
                               /// Uses CallerArgumentExpression for dispatch.
                               /// </summary>
                               public static global::System.IDisposable BindCommand(
                                   this {group.ViewTypeFullName} view,
                                   {group.ViewModelTypeFullName} viewModel,
                                   global::System.Linq.Expressions.Expression<global::System.Func<{group.ViewModelTypeFullName}, {commandType}>> propertyName,
                                   global::System.Linq.Expressions.Expression<global::System.Func<{group.ViewTypeFullName}, {controlType}>> controlName,
                       """);

        if (group.HasObservableParameter)
        {
            _ = sb.AppendLine($"            global::System.IObservable<{group.ParameterTypeFullName}> withParameter,");
        }
        else if (group.HasExpressionParameter)
        {
            _ = sb.AppendLine(
                $"            global::System.Linq.Expressions.Expression<global::System.Func<{group.ViewModelTypeFullName}, {withParameterExprType}>> withParameter,");
        }

        _ = sb.AppendLine($"""
                                  string{(supportsNullable ? "?" : string.Empty)} toEvent = null,
                                  [global::System.Runtime.CompilerServices.CallerArgumentExpression("propertyName")] string propertyNameExpression = "",
                                  [global::System.Runtime.CompilerServices.CallerArgumentExpression("controlName")] string controlNameExpression = "",
                      """);

        if (group.HasExpressionParameter)
        {
            _ = sb.AppendLine("""
                                      [global::System.Runtime.CompilerServices.CallerArgumentExpression("withParameter")] string withParameterExpression = "",
                          """);
        }

        _ = sb.AppendLine("""
                                  [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                                  [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                              {
                                  propertyNameExpression = propertyNameExpression.StartsWith("static ") ? propertyNameExpression.Substring(7) : propertyNameExpression;
                                  controlNameExpression = controlNameExpression.StartsWith("static ") ? controlNameExpression.Substring(7) : controlNameExpression;

                      """);

        EmitExpressionDispatchBranches(sb, group);
        EmitDispatchFallthrough(sb);
    }

    /// <summary>Generates the CallerFilePath-based overload for BindCommand dispatch.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindCommand type group.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void GenerateCallerFilePathOverload(
        StringBuilder sb,
        BindCommandTypeGroup group,
        bool supportsNullable)
    {
        // The command selector is nullable (a command property may be null), matching the runtime stub's
        // Expression<Func<TViewModel, TProp?>>. The control selector stays non-nullable to match the stub's
        // Expression<Func<TView, TControl>> (TControl is a non-null class) so overload resolution selects this
        // generated overload instead of falling through to the runtime fallback.
        var commandType = CodeGeneratorHelpers.NullableSelectorLeafType(group.Invocations[0].CommandPropertyPath, supportsNullable);
        var controlType = group.ControlTypeFullName;

        // The expression-form command-parameter selector is nullable for reference-type parameters, matching the
        // runtime stub's Expression<Func<TViewModel, TParam?>> so the parameter lambda may return null without CS8603.
        var withParameterExprType = supportsNullable && group.Invocations[0].ParameterIsReferenceType
            ? $"{group.ParameterTypeFullName}?"
            : group.ParameterTypeFullName;
        _ = sb.AppendLine($"""
                               /// <summary>
                               /// Concrete typed overload for BindCommand on {group.ViewTypeFullName}.
                               /// Uses CallerFilePath + CallerLineNumber for dispatch.
                               /// </summary>
                               public static global::System.IDisposable BindCommand(
                                   this {group.ViewTypeFullName} view,
                                   {group.ViewModelTypeFullName} viewModel,
                                   global::System.Linq.Expressions.Expression<global::System.Func<{group.ViewModelTypeFullName}, {commandType}>> propertyName,
                                   global::System.Linq.Expressions.Expression<global::System.Func<{group.ViewTypeFullName}, {controlType}>> controlName,
                       """);

        if (group.HasObservableParameter)
        {
            _ = sb.AppendLine($"            global::System.IObservable<{group.ParameterTypeFullName}> withParameter,");
        }
        else if (group.HasExpressionParameter)
        {
            _ = sb.AppendLine(
                $"            global::System.Linq.Expressions.Expression<global::System.Func<{group.ViewModelTypeFullName}, {withParameterExprType}>> withParameter,");
        }

        _ = sb.AppendLine($$"""
                                  string{{(supportsNullable ? "?" : string.Empty)}} toEvent = null,
                                  [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                                  [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                              {
                      """);

        EmitFilePathDispatchBranches(sb, group);
        EmitDispatchFallthrough(sb);
    }

    /// <summary>Generates a private BindCommand method for a specific invocation.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="viewModelClassInfo">The view model type class binding info.</param>
    /// <param name="suffix">The stable method name suffix.</param>
    /// <param name="supportsNullable">There can be a null type.</param>
    internal static void GenerateBindCommandMethod(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        ClassBindingInfo? viewModelClassInfo,
        string suffix,
        bool supportsNullable)
    {
        var cmdPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.CommandPropertyPath);
        var ctrlPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.ControlPropertyPath);

        // Only the observable-parameter worker actually consumes 'withParameter' (see
        // BuildParameterObservableExpression). The expression-parameter case reads the value via the
        // compile-time-extracted ParameterPropertyPath, so the worker takes no extra parameter there.
        var extraParams = inv.HasObservableParameter
            ? $", global::System.IObservable<{inv.ParameterTypeFullName}> withParameter"
            : string.Empty;

        _ = sb.AppendLine($$"""
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
            sb,
            "viewModel",
            inv.CommandPropertyPath,
            inv.CommandTypeFullName,
            viewModelClassInfo,
            "commandObs");

        // Try plugins in affinity order (highest first) via registry
        var plugin = CommandBindingPluginRegistry.GetBestPlugin(inv);
        var generatedAffinity = plugin is not null ? plugin.Affinity : -1;
        var hasEvent = inv.ResolvedEventName is not null;

        // Emit affinity check: let user-registered ICreatesCommandBinding plugins override
        // if they have higher affinity than the source-generated binding
        EmitCommandAffinityCheck(sb, inv, controlAccess, generatedAffinity, hasEvent);

        if (plugin is not null)
        {
            plugin.EmitBinding(sb, inv, controlAccess, supportsNullable);
        }
        else
        {
            // No plugin matched — throw after the affinity check fallback
            _ = sb.AppendLine("""
                                      throw new global::System.InvalidOperationException(
                                          "No bindable event found on the control. Specify the 'toEvent' parameter.");
                                  }
                          """);
        }

        _ = sb.AppendLine();
    }

    /// <summary>
    /// Emits the command binding affinity check that allows user-registered
    /// <c>ICreatesCommandBinding</c> implementations to override the generated binding
    /// when they have higher affinity. If no user plugin has higher affinity, falls through
    /// to the generated event subscription code.
    /// </summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain (e.g., "view.MyButton").</param>
    /// <param name="generatedAffinity">The affinity of the source-generated plugin, or -1 if none.</param>
    /// <param name="hasEvent">Whether a resolved event was found at compile time.</param>
    internal static void EmitCommandAffinityCheck(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        int generatedAffinity,
        bool hasEvent)
    {
        // Build the parameter observable expression for the custom binder
        var paramObsExpr = BuildParameterObservableExpression(inv);

        _ = sb.AppendLine($$"""

                                    if (global::ReactiveUI.Binding.Fallback.CommandBindingAffinityChecker
                                        .HasHigherAffinityPlugin<{{inv.ControlTypeFullName}}>({{generatedAffinity}}, {{(hasEvent ? "true" : "false")}}))
                                    {
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
                                    }

                        """);
    }

    /// <summary>Builds the parameter observable expression string for custom binder fallback code.</summary>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <returns>The parameter observable expression to embed in generated code.</returns>
    internal static string BuildParameterObservableExpression(BindCommandInvocationInfo inv)
    {
        if (inv.HasObservableParameter)
        {
            // Cast the typed observable to IObservable<object> via Select
            return $"new global::ReactiveUI.Binding.Observables.SelectObservable<{inv.ParameterTypeFullName}, object>(withParameter, __p => __p)";
        }

        if (inv is { HasExpressionParameter: true, ParameterPropertyPath: not null })
        {
            // Read the parameter property at call time
            var paramAccess =
                CodeGeneratorHelpers.BuildPropertyAccessChain("viewModel", inv.ParameterPropertyPath.Value);
            return $"new global::ReactiveUI.Binding.Observables.ReturnObservable<object>({paramAccess})";
        }

        return "global::ReactiveUI.Binding.Observables.EmptyObservable<object>.Instance";
    }

    /// <summary>Emits one expression-text comparison branch per call site in the group.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindCommand type group.</param>
    private static void EmitExpressionDispatchBranches(StringBuilder sb, BindCommandTypeGroup group)
    {
        var extraArgs = group.HasObservableParameter ? ", withParameter" : string.Empty;

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.ViewTypeFullName,
                inv.CallerFilePath,
                inv.CallerLineNumber,
                $"{inv.CommandExpressionText}|{inv.ControlExpressionText}");
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);
            var escapedCmdExpr = CodeGeneratorHelpers.EscapeString(inv.CommandExpressionText);
            var escapedCtrlExpr = CodeGeneratorHelpers.EscapeString(inv.ControlExpressionText);

            _ = sb.AppendLine($$"""
                                        {{condition}} (propertyNameExpression == "{{escapedCmdExpr}}"
                                            && controlNameExpression == "{{escapedCtrlExpr}}")
                                        {
                                            return __BindCommand_{{methodSuffix}}(view, viewModel{{extraArgs}});
                                        }
                            """);
        }
    }

    /// <summary>Emits the throw that closes a dispatch method when no call site matched.</summary>
    /// <param name="sb">The string builder to append to.</param>
    private static void EmitDispatchFallthrough(StringBuilder sb) =>
        _ = sb.AppendLine("""
                                  throw new global::System.InvalidOperationException(
                                      "No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.");
                              }
                      """);

    /// <summary>Emits one file-and-line comparison branch per call site in the group.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindCommand type group.</param>
    private static void EmitFilePathDispatchBranches(StringBuilder sb, BindCommandTypeGroup group)
    {
        var extraArgs = group.HasObservableParameter ? ", withParameter" : string.Empty;

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.ViewTypeFullName,
                inv.CallerFilePath,
                inv.CallerLineNumber,
                $"{inv.CommandExpressionText}|{inv.ControlExpressionText}");
            var pathSuffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);

            _ = sb.AppendLine($$"""
                                        {{condition}} (callerLineNumber == {{inv.CallerLineNumber}}
                                            && callerFilePath.EndsWith("{{CodeGeneratorHelpers.EscapeString(pathSuffix)}}", global::System.StringComparison.OrdinalIgnoreCase))
                                        {
                                            return __BindCommand_{{methodSuffix}}(view, viewModel{{extraArgs}});
                                        }
                            """);
        }
    }

    /// <summary>Groups BindCommand invocations by type signature for overload generation.</summary>
    /// <param name="ViewTypeFullName">The fully qualified view type.</param>
    /// <param name="ViewModelTypeFullName">The fully qualified view model type.</param>
    /// <param name="CommandTypeFullName">The fully qualified type of the bound command property.</param>
    /// <param name="ControlTypeFullName">The fully qualified type of the control the command binds to.</param>
    /// <param name="HasObservableParameter">Whether the overload takes an observable command parameter.</param>
    /// <param name="HasExpressionParameter">Whether the overload takes an expression command parameter.</param>
    /// <param name="ParameterTypeFullName">The fully qualified command parameter type, when the overload has one.</param>
    /// <param name="Invocations">The call sites sharing this group's shape.</param>
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

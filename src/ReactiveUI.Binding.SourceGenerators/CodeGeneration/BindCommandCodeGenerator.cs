// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Generates concrete typed extension method overloads and binding methods for BindCommand invocations.</summary>
internal static class BindCommandCodeGenerator
{
    /// <summary>The parameter carrying the text of the selector naming the command.</summary>
    private const string CommandExpressionParameter = "propertyNameExpression";

    /// <summary>The parameter carrying the text of the selector naming the control.</summary>
    private const string ControlExpressionParameter = "controlNameExpression";

    /// <summary>The generated worker each dispatch branch hands the binding to.</summary>
    private const string WorkerMethodPrefix = "__BindCommand_";

    /// <summary>The two objects a generated worker binds, in its own parameter order.</summary>
    private const string WorkerArguments = "view, viewModel";

    /// <summary>Closes the view parameter of a generated binding worker.</summary>
    private const string ViewParameterSuffix = " view,";

    /// <summary>Generates concrete typed overloads and binding methods for BindCommand invocations.</summary>
    /// <param name="invocations">All detected BindCommand invocations.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? Generate(
        ImmutableArray<BindCommandInvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features) =>
        CodeGeneratorHelpers.GenerateDispatchFile(
            invocations,
            features,
            GroupByTypeSignature,
            (sb, group, snapshot) => EmitGroup(sb, group, allClasses, snapshot));

    /// <summary>Groups BindCommand invocations by their type signature for overload generation.</summary>
    /// <param name="invocations">The BindCommand invocations to group.</param>
    /// <returns>A list of grouped invocations sharing the same type signature.</returns>
    internal static List<BindCommandTypeGroup> GroupByTypeSignature(
        ImmutableArray<BindCommandInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<BindCommandInvocationInfo>>(invocations.Length);
        var keySb = new PooledStringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);

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

        keySb.Return();

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
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateConcreteOverload(
        StringBuilder sb,
        BindCommandTypeGroup group,
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
        _ = sb.AppendLine("        /// <summary>").Append("        /// Concrete typed overload for BindCommand on ").Append(group.ViewTypeFullName)
            .AppendLine(".").AppendLine("        /// Uses CallerArgumentExpression for dispatch.").AppendLine("        /// </summary>")
            .AppendLine("        public static global::System.IDisposable BindCommand(").Append("            this ").Append(group.ViewTypeFullName)
            .AppendLine(ViewParameterSuffix).Append("            ").Append(group.ViewModelTypeFullName).AppendLine(" viewModel,")
            .Append(GeneratedSyntax.SelectorParameterOpen).Append(group.ViewModelTypeFullName).Append(", ")
            .Append(commandType).AppendLine(">> propertyName,").Append(GeneratedSyntax.SelectorParameterOpen)
            .Append(group.ViewTypeFullName).Append(", ").Append(controlType).AppendLine(">> controlName,");

        if (group.HasObservableParameter)
        {
            _ = sb.Append("            global::System.IObservable<").Append(group.ParameterTypeFullName).AppendLine("> withParameter,");
        }
        else if (group.HasExpressionParameter)
        {
            _ = sb.Append(GeneratedSyntax.SelectorParameterOpen).Append(group.ViewModelTypeFullName)
                .Append(", ").Append(withParameterExprType).AppendLine(">> withParameter,");
        }

        _ = sb.Append("            string").Append(supportsNullable ? "?" : string.Empty).AppendLine(" toEvent = null,")
            .AppendLine("            [global::System.Runtime.CompilerServices.CallerArgumentExpression(\"propertyName\")] string propertyNameExpression = \"\",")
            .AppendLine("            [global::System.Runtime.CompilerServices.CallerArgumentExpression(\"controlName\")] string controlNameExpression = \"\",");

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
                                  propertyNameExpression = propertyNameExpression.StartsWith("static ", global::System.StringComparison.Ordinal)
                                      ? propertyNameExpression.Substring(7)
                                      : propertyNameExpression;
                                  controlNameExpression = controlNameExpression.StartsWith("static ", global::System.StringComparison.Ordinal)
                                      ? controlNameExpression.Substring(7)
                                      : controlNameExpression;

                      """);

        EmitExpressionDispatchBranches(sb, group);
        EmitDispatchFallthrough(sb);
    }

    /// <summary>Generates the CallerFilePath-based overload for BindCommand dispatch.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindCommand type group.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateCallerFilePathOverload(
        StringBuilder sb,
        BindCommandTypeGroup group,
        bool supportsNullable,
        bool stubHasExpressionParameters)
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
        _ = sb.AppendLine("        /// <summary>").Append("        /// Concrete typed overload for BindCommand on ").Append(group.ViewTypeFullName)
            .AppendLine(".").AppendLine("        /// Uses CallerFilePath + CallerLineNumber for dispatch.").AppendLine("        /// </summary>")
            .AppendLine("        public static global::System.IDisposable BindCommand(").Append("            this ").Append(group.ViewTypeFullName)
            .AppendLine(ViewParameterSuffix).Append("            ").Append(group.ViewModelTypeFullName).AppendLine(" viewModel,")
            .Append(GeneratedSyntax.SelectorParameterOpen).Append(group.ViewModelTypeFullName).Append(", ")
            .Append(commandType).AppendLine(">> propertyName,").Append(GeneratedSyntax.SelectorParameterOpen)
            .Append(group.ViewTypeFullName).Append(", ").Append(controlType).AppendLine(">> controlName,");

        if (group.HasObservableParameter)
        {
            _ = sb.Append("            global::System.IObservable<").Append(group.ParameterTypeFullName).AppendLine("> withParameter,");
        }
        else if (group.HasExpressionParameter)
        {
            _ = sb.Append(GeneratedSyntax.SelectorParameterOpen).Append(group.ViewModelTypeFullName)
                .Append(", ").Append(withParameterExprType).AppendLine(">> withParameter,");
        }

        _ = sb.Append("            string").Append(supportsNullable ? "?" : string.Empty).AppendLine(" toEvent = null,");

        if (stubHasExpressionParameters)
        {
            CodeGeneratorHelpers.AppendExpressionParameter(sb, "propertyName", "propertyNameExpression", false);
            CodeGeneratorHelpers.AppendExpressionParameter(sb, "controlName", "controlNameExpression", false);

            if (group.HasExpressionParameter)
            {
                CodeGeneratorHelpers.AppendExpressionParameter(sb, "withParameter", "withParameterExpression", false);
            }
        }

        _ = sb.AppendLine("""
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

        _ = sb.Append("        private static global::System.IDisposable __BindCommand_").Append(suffix).AppendLine("(").Append("            ")
            .Append(inv.ViewTypeFullName).AppendLine(ViewParameterSuffix).Append("            ").Append(inv.ViewModelTypeFullName).Append(" viewModel")
            .Append(extraParams).AppendLine(")").AppendLine("        {").Append("            // BindCommand: ").Append(cmdPathComment).Append(" -> ")
            .Append(ctrlPathComment).Append(" (event: ").Append(inv.ResolvedEventName ?? "none").AppendLine(")")
            .AppendLine("            if (viewModel == null)").AppendLine(GeneratedSyntax.StatementBlockOpen)
            .AppendLine("                return global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;").AppendLine(GeneratedSyntax.StatementBlockClose)
            .AppendLine();

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

        // A parameter named as a property is observed, not read once. The control has to see each value the
        // property takes, the same as it would from a caller-supplied stream; reading it when the command
        // arrives leaves the control holding whatever it happened to be at that moment.
        if (inv is { HasObservableParameter: false, HasExpressionParameter: true, ParameterPropertyPath: not null })
        {
            ObservationCodeGenerator.EmitInlineObservation(
                sb,
                "viewModel",
                inv.ParameterPropertyPath.Value,
                inv.ParameterTypeFullName ?? "object",
                viewModelClassInfo,
                "withParameter");
        }

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

        _ = sb.AppendLine().AppendLine("            if (global::ReactiveUI.Binding.Fallback.CommandBindingAffinityChecker")
            .Append("                .HasHigherAffinityPlugin<").Append(inv.ControlTypeFullName).Append(">(").Append(generatedAffinity).Append(", ")
            .Append(hasEvent ? "true" : "false").AppendLine("))").AppendLine(GeneratedSyntax.StatementBlockOpen)
            .AppendLine("                var __customBinder = global::ReactiveUI.Binding.CommandBinding.CommandBinderService")
            .Append("                    .GetBinder<").Append(inv.ControlTypeFullName).Append(">(").Append(hasEvent ? "true" : "false").AppendLine(");")
            .AppendLine("                if (__customBinder != null)").AppendLine("                {")
            .AppendLine("                    var __serial = new global::ReactiveUI.Primitives.Disposables.SwapDisposable();")
            .AppendLine("                    var __binderCmdSub = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(commandObs, __cmd =>")
            .AppendLine("                    {")
            .AppendLine("                        __serial.Disposable = global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;")
            .Append("                        global::System.IObservable<object> __paramObs = ").Append(paramObsExpr).AppendLine(";")
            .Append("                        __serial.Disposable = __customBinder.BindCommandToObject<").Append(inv.ControlTypeFullName).AppendLine(">(")
            .Append("                            __cmd, ").Append(controlAccess).AppendLine(", __paramObs)")
            .AppendLine("                            ?? global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;")
            .AppendLine("                    });")
            .AppendLine("                    return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(__binderCmdSub, __serial);")
            .AppendLine("                }").AppendLine(GeneratedSyntax.StatementBlockClose).AppendLine();
    }

    /// <summary>Builds the parameter observable expression string for custom binder fallback code.</summary>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <returns>The parameter observable expression to embed in generated code.</returns>
    /// <remarks>
    /// Both parameter forms reach the binder as a stream: <c>withParameter</c> is either the caller's own
    /// observable or the observation of the named property, so a registered binder sees each value the
    /// parameter takes rather than the one it happened to hold when the command arrived.
    /// </remarks>
    internal static string BuildParameterObservableExpression(BindCommandInvocationInfo inv) =>
        inv.HasObservableParameter || inv is { HasExpressionParameter: true, ParameterPropertyPath: not null }
            ? $"new global::ReactiveUI.Primitives.Signals.MapSignal<{inv.ParameterTypeFullName}, object>(withParameter, __p => __p)"
            : "global::ReactiveUI.Primitives.Advanced.ImmutableEmptySignal<object>.Instance";

    /// <summary>Emits the overload and the workers for one group of call sites.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of call sites that share an overload.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void EmitGroup(
        StringBuilder sb,
        BindCommandTypeGroup group,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features)
    {
        var collapsed = features.SupportsCallerArgExpr
            ? group with
            {
                Invocations = CodeGeneratorHelpers.CollapseIndistinguishableCallSites(
                    group.Invocations,
                    static x => $"{x.CommandExpressionText}|{x.ControlExpressionText}"),
            }
            : group;

        GenerateConcreteOverload(
            sb,
            collapsed,
            features.SupportsCallerArgExpr,
            features.SupportsNullable,
            features.StubHasExpressionParameters);
        _ = sb.AppendLine();

        for (var i = 0; i < collapsed.Invocations.Length; i++)
        {
            var inv = collapsed.Invocations[i];
            GenerateBindCommandMethod(
                sb,
                inv,
                CodeGeneratorHelpers.FindClassInfo(allClasses, inv.ViewModelTypeFullName),
                MethodSuffix(inv),
                features.SupportsNullable);
        }
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

            CodeGeneratorHelpers.AppendExpressionDispatchCondition(
                sb,
                CodeGeneratorHelpers.ConditionKeyword(i),
                CommandExpressionParameter,
                inv.CommandExpressionText,
                ControlExpressionParameter,
                inv.ControlExpressionText);
            CodeGeneratorHelpers.AppendDispatchReturn(sb, WorkerMethodPrefix + MethodSuffix(inv), WorkerArguments + extraArgs);
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

            CodeGeneratorHelpers.AppendCallerInfoDispatchCondition(
                sb,
                CodeGeneratorHelpers.ConditionKeyword(i),
                inv.CallerLineNumber,
                CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath));
            CodeGeneratorHelpers.AppendDispatchReturn(sb, WorkerMethodPrefix + MethodSuffix(inv), WorkerArguments + extraArgs);
        }
    }

    /// <summary>Names the generated worker a call site dispatches to.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The stable suffix its worker is named with.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string MethodSuffix(BindCommandInvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.ViewTypeFullName,
            inv.CallerFilePath,
            inv.CallerLineNumber,
            $"{inv.CommandExpressionText}|{inv.ControlExpressionText}");

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

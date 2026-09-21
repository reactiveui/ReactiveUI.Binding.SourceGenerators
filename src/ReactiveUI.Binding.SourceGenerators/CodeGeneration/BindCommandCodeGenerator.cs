// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;
using ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Generates concrete typed extension method overloads and binding methods for BindCommand invocations.</summary>
internal static class BindCommandCodeGenerator
{
    /// <summary>The parameter carrying the text of the selector naming the command.</summary>
    private const string CommandExpressionParameter = "propertyNameExpression";

    /// <summary>The parameter carrying the text of the selector naming the control.</summary>
    private const string ControlExpressionParameter = "controlNameExpression";

    /// <summary>The parameter carrying the text of the selector naming the command parameter.</summary>
    private const string ParameterExpressionParameter = "withParameterExpression";

    /// <summary>Continues a dispatch condition onto its next line.</summary>
    private const string ConditionContinuation = "                && ";

    /// <summary>The generated worker each dispatch branch hands the binding to.</summary>
    private const string WorkerMethodPrefix = "__BindCommand_";

    /// <summary>The two objects a generated worker binds, in its own parameter order.</summary>
    private const string WorkerArguments = "view, viewModel";

    /// <summary>Closes the view parameter of a generated binding worker.</summary>
    private const string ViewParameterSuffix = " view,";

    /// <summary>The caller-supplied parameter stream a worker takes on top of the two bound objects.</summary>
    private const string ObservableParameterArgument = ", withParameter";

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
        AppendOverloadSummary(sb, group, "        /// Uses CallerArgumentExpression for dispatch.");
        AppendParameterList(sb, group, true, supportsNullable, true);

        _ = sb.AppendLine(GeneratedSyntax.MemberBodyOpen);

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
        AppendOverloadSummary(sb, group, "        /// Uses CallerFilePath + CallerLineNumber for dispatch.");
        AppendParameterList(sb, group, false, supportsNullable, stubHasExpressionParameters);

        _ = sb.AppendLine(GeneratedSyntax.MemberBodyOpen);

        EmitFilePathDispatchBranches(sb, group);
        EmitDispatchFallthrough(sb);
    }

    /// <summary>Generates a private BindCommand method for a specific invocation.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="viewModelClassInfo">The view model type class binding info.</param>
    /// <param name="viewClassInfo">The view type class binding info, which says whether it exposes a view model.</param>
    /// <param name="suffix">The stable method name suffix.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void GenerateBindCommandMethod(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        ClassBindingInfo? viewModelClassInfo,
        ClassBindingInfo? viewClassInfo,
        string suffix,
        bool supportsNullable)
    {
        var cmdPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.CommandPropertyPath);
        var ctrlPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.ControlPropertyPath);

        // Only a caller-supplied stream is passed in; a parameter named as a property is observed inside the worker.
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

        EmitViewModelObservations(sb, inv, viewModelClassInfo, viewClassInfo);
        CommandControlEmitter.EmitRebinding(sb, inv, viewClassInfo, suffix);

        var plugin = CommandBindingPluginRegistry.GetBestPlugin(inv);
        var generatedAffinity = plugin is not null ? plugin.Affinity : -1;
        EmitCommandAffinityCheck(sb, inv, "__control", generatedAffinity, inv.HasExplicitEvent);

        if (plugin is not null)
        {
            plugin.EmitBinding(sb, inv, "__control", supportsNullable);
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

    /// <summary>Emits the check that hands the binding to a registered <c>ICreatesCommandBinding</c> with a higher affinity.</summary>
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
            .Append("                        __serial.Disposable = __customBinder.BindCommandToObject<").Append(inv.ControlTypeFullName)
            .Append(hasEvent ? $", {inv.ResolvedEventArgsTypeFullName ?? "global::System.EventArgs"}" : string.Empty).AppendLine(">(")
            .Append("                            __cmd, ").Append(controlAccess).Append(", __paramObs")
            .Append(hasEvent ? $", \"{inv.ResolvedEventName}\"" : string.Empty).AppendLine(")")
            .AppendLine("                            ?? global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;")
            .AppendLine("                    });")
            .AppendLine("                    return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(__binderCmdSub, __serial);")
            .AppendLine("                }").AppendLine(GeneratedSyntax.StatementBlockClose).AppendLine();
    }

    /// <summary>Builds the parameter observable expression string for custom binder fallback code.</summary>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <returns>The parameter observable expression to embed in generated code.</returns>
    internal static string BuildParameterObservableExpression(BindCommandInvocationInfo inv) =>
        inv.HasObservableParameter || inv is { HasExpressionParameter: true, ParameterPropertyPath: not null }
            ? $"new global::ReactiveUI.Primitives.Signals.MapSignal<{inv.ParameterTypeFullName}, object>(withParameter, __p => __p)"
            : "global::ReactiveUI.Primitives.Advanced.ImmutableEmptySignal<object>.Instance";

    /// <summary>Emits the observations of the command, and of a parameter named as a property, through the view's current view model.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="viewModelClassInfo">The view model type class binding info.</param>
    /// <param name="viewClassInfo">The view type class binding info, which says whether it exposes a view model.</param>
    private static void EmitViewModelObservations(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        ClassBindingInfo? viewModelClassInfo,
        ClassBindingInfo? viewClassInfo)
    {
        var commandObservation = BindingEmitterHelpers.ResolveViewModelObservation(
            inv.ViewModelTypeFullName,
            inv.ViewTypeFullName,
            inv.CommandPropertyPath,
            viewModelClassInfo,
            viewClassInfo);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            commandObservation.RootVariable,
            commandObservation.Path,
            inv.CommandTypeFullName,
            commandObservation.RootClassInfo,
            "__commandChanges");

        // Each rebind touches the control, so the command arrives on the view's thread.
        _ = BindingEmitterHelpers.AppendViewThreadCall(sb.Append("            var commandObs = "), "__commandChanges", "view", inv.ViewThreadInvoker)
            .AppendLine(";");

        if (inv is not { HasObservableParameter: false, HasExpressionParameter: true, ParameterPropertyPath: not null })
        {
            return;
        }

        // Observed rather than read once, so the control sees every value the parameter takes.
        var parameterObservation = BindingEmitterHelpers.ResolveViewModelObservation(
            inv.ViewModelTypeFullName,
            inv.ViewTypeFullName,
            inv.ParameterPropertyPath.Value,
            viewModelClassInfo,
            viewClassInfo);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            parameterObservation.RootVariable,
            parameterObservation.Path,
            inv.ParameterTypeFullName ?? "object",
            parameterObservation.RootClassInfo,
            "withParameter");
    }

    /// <summary>Appends the signature both dispatch overloads declare, up to the parameters that identify a call site.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindCommand type group.</param>
    /// <param name="dispatchSummaryLine">The documentation line naming what the overload matches a call site on.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendOverloadSummary(
        StringBuilder sb,
        BindCommandTypeGroup group,
        string dispatchSummaryLine) =>
        sb.AppendLine("        /// <summary>").Append("        /// Concrete typed overload for BindCommand on ").Append(group.ViewTypeFullName)
            .AppendLine(".").AppendLine(dispatchSummaryLine).AppendLine("        /// </summary>")
            .AppendLine("        public static global::System.IDisposable BindCommand(");

    /// <summary>Writes the stub's parameter list, which the overload and the interceptor both have to match exactly.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindCommand type group whose types the parameters are written from.</param>
    /// <param name="dispatchesOnExpressionText">Whether the captured expression text is what identifies a call site.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters.</param>
    private static void AppendParameterList(
        StringBuilder sb,
        BindCommandTypeGroup group,
        bool dispatchesOnExpressionText,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        // The command and parameter selectors are nullable and the control selector is not, as the stub declares them.
        var commandType = CodeGeneratorHelpers.NullableSelectorLeafType(group.Invocations[0].CommandPropertyPath, supportsNullable);

        _ = sb.Append("            this ").Append(group.ViewTypeFullName)
            .AppendLine(ViewParameterSuffix).Append(CodeGeneratorHelpers.ParameterIndent)
            .Append(CodeGeneratorHelpers.NullableSelectorType(group.ViewModelTypeFullName, true, supportsNullable)).AppendLine(" viewModel,")
            .Append(GeneratedSyntax.SelectorParameterOpen).Append(group.ViewModelTypeFullName).Append(", ")
            .Append(commandType).AppendLine(">> propertyName,").Append(GeneratedSyntax.SelectorParameterOpen)
            .Append(group.ViewTypeFullName).Append(", ").Append(group.ControlTypeFullName).AppendLine(">> controlName,");

        if (group.HasObservableParameter)
        {
            _ = sb.Append("            global::System.IObservable<").Append(group.ParameterTypeFullName).AppendLine("> withParameter,");
        }
        else if (group.HasExpressionParameter)
        {
            var withParameterExprType = supportsNullable && group.Invocations[0].ParameterIsReferenceType
                ? $"{group.ParameterTypeFullName}?"
                : group.ParameterTypeFullName;

            _ = sb.Append(GeneratedSyntax.SelectorParameterOpen).Append(group.ViewModelTypeFullName)
                .Append(", ").Append(withParameterExprType).AppendLine(">> withParameter,");
        }

        _ = sb.Append("            string").Append(supportsNullable ? "?" : string.Empty).AppendLine(" toEvent = null,");

        if (dispatchesOnExpressionText || stubHasExpressionParameters)
        {
            CodeGeneratorHelpers.AppendExpressionParameter(sb, "propertyName", CommandExpressionParameter, dispatchesOnExpressionText);
            CodeGeneratorHelpers.AppendExpressionParameter(sb, "controlName", ControlExpressionParameter, dispatchesOnExpressionText);

            if (group.HasExpressionParameter)
            {
                CodeGeneratorHelpers.AppendExpressionParameter(sb, "withParameter", "withParameterExpression", dispatchesOnExpressionText);
            }
        }

        _ = sb.AppendLine(CodeGeneratorHelpers.CallerInfoParameterList);
    }

    /// <summary>Emits one interceptor per generated binding, claiming every call site that reaches it.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of call sites being claimed.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void GenerateInterceptors(StringBuilder sb, BindCommandTypeGroup group, in LanguageFeatures features)
    {
        var extraArgs = group.HasObservableParameter ? ObservableParameterArgument : string.Empty;
        var dispatchesOnExpressionText = features.SupportsCallerArgExpr;
        var supportsNullable = features.SupportsNullable;
        var stubHasExpressionParameters = features.StubHasExpressionParameters;

        foreach (var entry in InterceptorEmitter.GroupCallSites(group.Invocations, static x => x.Interceptor, MethodSuffix))
        {
            foreach (var callSite in entry.Value)
            {
                InterceptorEmitter.AppendAttribute(sb, callSite.Interceptor, InterceptorEmitter.MemberIndent);
            }

            _ = sb.Append("        internal static global::System.IDisposable __Intercept_BindCommand_").Append(entry.Key).AppendLine("(");

            AppendParameterList(sb, group, dispatchesOnExpressionText, supportsNullable, stubHasExpressionParameters);

            _ = sb.Append("            => ").Append(WorkerMethodPrefix).Append(entry.Key).Append('(').Append(WorkerArguments)
                .Append(extraArgs).AppendLine(");").AppendLine();
        }
    }

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
        var collapsed = features.CollapsesIndistinguishableCallSites
            ? group with { Invocations = ExplicitEventsFirst(CodeGeneratorHelpers.CollapseIndistinguishableCallSites(group.Invocations, DispatchKey)) }
            : group;

        if (features.SupportsInterceptors)
        {
            GenerateInterceptors(sb, collapsed, in features);
        }
        else
        {
            GenerateConcreteOverload(
                sb,
                collapsed,
                features.SupportsCallerArgExpr,
                features.SupportsNullable,
                features.StubHasExpressionParameters);
        }

        _ = sb.AppendLine();

        for (var i = 0; i < collapsed.Invocations.Length; i++)
        {
            var inv = collapsed.Invocations[i];
            GenerateBindCommandMethod(
                sb,
                inv,
                CodeGeneratorHelpers.ResolveObservedTypeInfo(allClasses, inv.ViewModelTypeFullName, inv.CommandPropertyPath),
                CodeGeneratorHelpers.ResolveObservedTypeInfo(allClasses, inv.ViewTypeFullName, inv.ControlPropertyPath),
                MethodSuffix(inv),
                features.SupportsNullable);
        }
    }

    /// <summary>Emits one expression-text comparison branch per call site in the group.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindCommand type group.</param>
    private static void EmitExpressionDispatchBranches(StringBuilder sb, BindCommandTypeGroup group)
    {
        var extraArgs = group.HasObservableParameter ? ObservableParameterArgument : string.Empty;

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];

            AppendExpressionCondition(sb, CodeGeneratorHelpers.ConditionKeyword(i), inv, group.HasExpressionParameter);
            CodeGeneratorHelpers.AppendDispatchReturn(sb, WorkerMethodPrefix + MethodSuffix(inv), WorkerArguments + extraArgs);
        }
    }

    /// <summary>Appends the condition that matches a call site by everything that shapes what it binds.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="condition">The conditional keyword this branch opens with.</param>
    /// <param name="inv">The call site the branch stands for.</param>
    /// <param name="comparesParameter">Whether the overload captures the text of the parameter selector.</param>
    /// <remarks>
    /// The command and control selectors are not all of it. A parameter selector picks which property feeds the
    /// control, and an explicit event replaces the mechanism the control would have bound through, so two call
    /// sites that spell the same command and control differently in either bind differently. A call site that
    /// names no event carries no event condition, which is why the ones that do are tried first.
    /// </remarks>
    private static void AppendExpressionCondition(
        StringBuilder sb,
        string condition,
        BindCommandInvocationInfo inv,
        bool comparesParameter)
    {
        _ = sb.Append(CodeGeneratorHelpers.ParameterIndent).Append(condition).Append(" (").Append(CommandExpressionParameter)
            .Append(CodeGeneratorHelpers.ExpressionTextComparison).Append(CodeGeneratorHelpers.EscapeString(inv.CommandExpressionText)).AppendLine("\"")
            .Append(ConditionContinuation).Append(ControlExpressionParameter).Append(CodeGeneratorHelpers.ExpressionTextComparison)
            .Append(CodeGeneratorHelpers.EscapeString(inv.ControlExpressionText)).Append('"');

        if (comparesParameter && inv.ParameterExpressionText is not null)
        {
            _ = sb.AppendLine().Append(ConditionContinuation).Append(ParameterExpressionParameter).Append(CodeGeneratorHelpers.ExpressionTextComparison)
                .Append(CodeGeneratorHelpers.EscapeString(inv.ParameterExpressionText)).Append('"');
        }

        if (inv.HasExplicitEvent && inv.ResolvedEventName is not null)
        {
            _ = sb.AppendLine().Append(ConditionContinuation).Append("toEvent").Append(CodeGeneratorHelpers.ExpressionTextComparison)
                .Append(CodeGeneratorHelpers.EscapeString(inv.ResolvedEventName)).Append('"');
        }

        _ = sb.AppendLine(")").AppendLine(GeneratedSyntax.StatementBlockOpen);
    }

    /// <summary>Keys a call site by everything the condition that dispatches on it compares.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The key two call sites share only when nothing can tell them apart.</returns>
    private static string DispatchKey(BindCommandInvocationInfo inv) =>
        $"{inv.CommandExpressionText}|{inv.ControlExpressionText}|{inv.ParameterExpressionText}|{(inv.HasExplicitEvent ? inv.ResolvedEventName : null)}";

    /// <summary>Puts the call sites that name an event ahead of those that do not, keeping the order within each.</summary>
    /// <param name="invocations">The call sites of one group.</param>
    /// <returns>The same call sites, with the explicit-event ones first.</returns>
    private static BindCommandInvocationInfo[] ExplicitEventsFirst(BindCommandInvocationInfo[] invocations)
    {
        var ordered = new List<BindCommandInvocationInfo>(invocations.Length);

        for (var i = 0; i < invocations.Length; i++)
        {
            if (invocations[i].HasExplicitEvent)
            {
                ordered.Add(invocations[i]);
            }
        }

        for (var i = 0; i < invocations.Length; i++)
        {
            if (!invocations[i].HasExplicitEvent)
            {
                ordered.Add(invocations[i]);
            }
        }

        return [.. ordered];
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
        var extraArgs = group.HasObservableParameter ? ObservableParameterArgument : string.Empty;

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

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

using static ReactiveUI.Binding.SourceGenerators.CodeGeneration.GeneratedTypeNames;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Generates concrete typed extension method overloads and binding methods for BindInteraction invocations.</summary>
internal static class BindInteractionCodeGenerator
{
    /// <summary>The generated worker each dispatch branch hands the binding to.</summary>
    private const string WorkerMethodPrefix = "__BindInteraction_";

    /// <summary>The arguments a generated worker takes, in its own parameter order.</summary>
    private const string WorkerArguments = "viewModel, handler";

    /// <summary>Closes the view model parameter of a generated binding worker.</summary>
    private const string ViewModelParameterSuffix = " viewModel,";

    /// <summary>Declares the local the interaction property is observed into, up to the observation type.</summary>
    private const string InteractionObservationOpen =
        "        var interactionObs = new global::ReactiveUI.Binding.Observables.";

    /// <summary>Generates concrete typed overloads and binding methods for BindInteraction invocations.</summary>
    /// <param name="invocations">All detected BindInteraction invocations.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? Generate(
        ImmutableArray<BindInteractionInvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features) =>
        CodeGeneratorHelpers.GenerateDispatchFile(
            invocations,
            features,
            GroupByTypeSignature,
            (sb, group, snapshot) => EmitGroup(sb, group, allClasses, snapshot));

    /// <summary>Groups BindInteraction invocations by their type signature for overload generation.</summary>
    /// <param name="invocations">The BindInteraction invocations to group.</param>
    /// <returns>A list of grouped invocations sharing the same type signature.</returns>
    internal static List<BindInteractionTypeGroup> GroupByTypeSignature(
        ImmutableArray<BindInteractionInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<BindInteractionInvocationInfo>>(invocations.Length);
        var keySb = new PooledStringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            _ = keySb.Clear()
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

        keySb.Return();

        var result = new List<BindInteractionTypeGroup>();
        foreach (var kvp in groupMap)
        {
            var first = kvp.Value[0];
            result.Add(new(
                first.ViewTypeFullName,
                first.ViewModelTypeFullName,
                first.InputTypeFullName,
                first.OutputTypeFullName,
                first.IsTaskHandler,
                first.DontCareTypeFullName,
                [.. kvp.Value]));
        }

        return result;
    }

    /// <summary>Generates the concrete typed overload using the appropriate dispatch strategy.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindInteraction type group.</param>
    /// <param name="supportsCallerArgExpr">Whether CallerArgumentExpression is available.</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateConcreteOverload(
        StringBuilder sb,
        BindInteractionTypeGroup group,
        bool supportsCallerArgExpr,
        bool stubHasExpressionParameters)
    {
        if (supportsCallerArgExpr)
        {
            GenerateCallerArgExprOverload(sb, group);
        }
        else
        {
            GenerateCallerFilePathOverload(sb, group, stubHasExpressionParameters);
        }
    }

    /// <summary>Generates the CallerArgumentExpression-based overload for BindInteraction dispatch.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindInteraction type group.</param>
    internal static void GenerateCallerArgExprOverload(
        StringBuilder sb,
        BindInteractionTypeGroup group)
    {
        var handlerType = group.IsTaskHandler
            ? $"global::System.Func<global::ReactiveUI.Binding.IInteractionContext<{group.InputTypeFullName}, {group.OutputTypeFullName}>, global::System.Threading.Tasks.Task>"
            : $"global::System.Func<global::ReactiveUI.Binding.IInteractionContext<{group.InputTypeFullName}, {group.OutputTypeFullName}>, global::System.IObservable<{group.DontCareTypeFullName}>>";

        _ = sb.AppendLine("        /// <summary>").Append("        /// Concrete typed overload for BindInteraction on ")
            .Append(group.ViewTypeFullName).AppendLine(".").AppendLine("        /// Uses CallerArgumentExpression for dispatch.")
            .AppendLine("        /// </summary>").AppendLine("        public static global::System.IDisposable BindInteraction(")
            .Append("            this ").Append(group.ViewTypeFullName).AppendLine(" view,").Append("            ").Append(group.ViewModelTypeFullName)
            .AppendLine(ViewModelParameterSuffix).Append("            ").Append(Expression).Append('<').Append(Func).Append('<').Append(group.ViewModelTypeFullName)
            .Append(", ").Append(IInteraction).Append('<').Append(group.InputTypeFullName).Append(", ").Append(group.OutputTypeFullName)
            .AppendLine(">>> propertyName,").Append("            ").Append(handlerType).AppendLine(" handler,")
            .AppendLine("            [global::System.Runtime.CompilerServices.CallerArgumentExpression(\"propertyName\")] string propertyNameExpression = \"\",")
            .AppendLine("            [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = \"\",")
            .AppendLine("            [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)").AppendLine("        {")
            .Append("            propertyNameExpression = propertyNameExpression.StartsWith(\"static \", global::System.StringComparison.Ordinal)")
            .AppendLine(" ? propertyNameExpression.Substring(7) : propertyNameExpression;")
            .AppendLine();

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];

            _ = sb.Append(CodeGeneratorHelpers.ParameterIndent).Append(CodeGeneratorHelpers.ConditionKeyword(i))
                .Append(" (propertyNameExpression == \"").Append(CodeGeneratorHelpers.EscapeString(inv.ExpressionText)).AppendLine("\")")
                .AppendLine(GeneratedSyntax.StatementBlockOpen);
            CodeGeneratorHelpers.AppendDispatchReturn(sb, WorkerMethodPrefix + MethodSuffix(inv), WorkerArguments);
        }

        CodeGeneratorHelpers.AppendBindingDispatchFallthrough(sb);
    }

    /// <summary>Generates the CallerFilePath-based overload for BindInteraction dispatch.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindInteraction type group.</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateCallerFilePathOverload(
        StringBuilder sb,
        BindInteractionTypeGroup group,
        bool stubHasExpressionParameters)
    {
        var handlerType = group.IsTaskHandler
            ? $"global::System.Func<global::ReactiveUI.Binding.IInteractionContext<{group.InputTypeFullName}, {group.OutputTypeFullName}>, global::System.Threading.Tasks.Task>"
            : $"global::System.Func<global::ReactiveUI.Binding.IInteractionContext<{group.InputTypeFullName}, {group.OutputTypeFullName}>, global::System.IObservable<{group.DontCareTypeFullName}>>";

        _ = sb.AppendLine("        /// <summary>").Append("        /// Concrete typed overload for BindInteraction on ")
            .Append(group.ViewTypeFullName).AppendLine(".").AppendLine("        /// Uses CallerFilePath + CallerLineNumber for dispatch.")
            .AppendLine("        /// </summary>").AppendLine("        public static global::System.IDisposable BindInteraction(")
            .Append("            this ").Append(group.ViewTypeFullName).AppendLine(" view,").Append("            ").Append(group.ViewModelTypeFullName)
            .AppendLine(ViewModelParameterSuffix).Append("            ").Append(Expression).Append('<').Append(Func).Append('<').Append(group.ViewModelTypeFullName)
            .Append(", ").Append(IInteraction).Append('<').Append(group.InputTypeFullName).Append(", ").Append(group.OutputTypeFullName)
            .AppendLine(">>> propertyName,").Append("            ").Append(handlerType).AppendLine(" handler,");

        if (stubHasExpressionParameters)
        {
            CodeGeneratorHelpers.AppendExpressionParameter(sb, "propertyName", "propertyNameExpression", false);
        }

        _ = sb.AppendLine("""
            [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
""");

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];

            CodeGeneratorHelpers.AppendCallerInfoDispatchCondition(
                sb,
                CodeGeneratorHelpers.ConditionKeyword(i),
                inv.CallerLineNumber,
                CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath));
            CodeGeneratorHelpers.AppendDispatchReturn(sb, WorkerMethodPrefix + MethodSuffix(inv), WorkerArguments);
        }

        CodeGeneratorHelpers.AppendBindingDispatchFallthrough(sb);
    }

    /// <summary>Generates a private BindInteraction method for a specific invocation.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The BindInteraction invocation info.</param>
    /// <param name="viewModelClassInfo">The view model type class binding info.</param>
    /// <param name="suffix">The stable method name suffix.</param>
    internal static void GenerateBindInteractionMethod(
        StringBuilder sb,
        BindInteractionInvocationInfo inv,
        ClassBindingInfo? viewModelClassInfo,
        string suffix)
    {
        var handlerType = inv.IsTaskHandler
            ? $"global::System.Func<global::ReactiveUI.Binding.IInteractionContext<{inv.InputTypeFullName}, {inv.OutputTypeFullName}>, global::System.Threading.Tasks.Task>"
            : $"global::System.Func<global::ReactiveUI.Binding.IInteractionContext<{inv.InputTypeFullName}, {inv.OutputTypeFullName}>, global::System.IObservable<{inv.DontCareTypeFullName}>>";

        var interactionType =
            $"global::ReactiveUI.Binding.IInteraction<{inv.InputTypeFullName}, {inv.OutputTypeFullName}>";
        var pathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.InteractionPropertyPath);

        _ = sb.Append("        private static global::System.IDisposable __BindInteraction_").Append(suffix).AppendLine("(").Append("            ")
            .Append(inv.ViewModelTypeFullName).AppendLine(ViewModelParameterSuffix).Append("            ").Append(handlerType).AppendLine(" handler)")
            .AppendLine("        {").Append("            // BindInteraction: ").Append(pathComment).AppendLine()
            .AppendLine("            var serial = new global::ReactiveUI.Primitives.Disposables.SwapDisposable();").AppendLine();

        EmitInteractionObservation(sb, inv, viewModelClassInfo, interactionType);

        // Subscribe to the interaction observable and register the handler
        const string registerCall = "interaction.RegisterHandler(handler)";

        _ = sb.AppendLine()
            .AppendLine("            var sub = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(interactionObs, interaction =>")
            .AppendLine(GeneratedSyntax.StatementBlockOpen).AppendLine("                serial.Disposable = interaction != null").Append("                    ? ")
            .Append(registerCall).AppendLine().AppendLine("                    : global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;")
            .AppendLine("            });").AppendLine("            return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(sub, serial);")
            .AppendLine("        }").AppendLine();
    }

    /// <summary>Emits the overload and the workers for one group of call sites.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of call sites that share an overload.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void EmitGroup(
        StringBuilder sb,
        BindInteractionTypeGroup group,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features)
    {
        var collapsed = features.SupportsCallerArgExpr
            ? group with
            {
                Invocations = CodeGeneratorHelpers.CollapseIndistinguishableCallSites(
                    group.Invocations,
                    static x => x.ExpressionText),
            }
            : group;

        GenerateConcreteOverload(sb, collapsed, features.SupportsCallerArgExpr, features.StubHasExpressionParameters);
        _ = sb.AppendLine();

        for (var i = 0; i < collapsed.Invocations.Length; i++)
        {
            var inv = collapsed.Invocations[i];
            GenerateBindInteractionMethod(
                sb,
                inv,
                CodeGeneratorHelpers.FindClassInfo(allClasses, inv.ViewModelTypeFullName),
                MethodSuffix(inv));
        }
    }

    /// <summary>
    /// Emits the null guard and the observation of the interaction property. A single-segment path
    /// observes the property directly; a deeper path delegates to the shared chain emitter.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The BindInteraction invocation info.</param>
    /// <param name="viewModelClassInfo">The view model type's binding info, when known.</param>
    /// <param name="interactionType">The fully qualified interaction type being observed.</param>
    private static void EmitInteractionObservation(
        StringBuilder sb,
        BindInteractionInvocationInfo inv,
        ClassBindingInfo? viewModelClassInfo,
        string interactionType)
    {
        AppendViewModelGuard(sb);

        if (inv.InteractionPropertyPath.Length != 1)
        {
            ObservationCodeGenerator.EmitInlineObservation(
                sb,
                "viewModel",
                inv.InteractionPropertyPath,
                interactionType,
                viewModelClassInfo,
                "interactionObs");
            return;
        }

        var propertyName = inv.InteractionPropertyPath[0].PropertyName;
        _ = sb.AppendLine();

        if (ObservationCodeGenerator.IsINPC(viewModelClassInfo))
        {
            _ = sb.Append(InteractionObservationOpen)
                .Append("PropertyObservable<").Append(interactionType).AppendLine(">(").AppendLine("            viewModel,")
                .Append("            \"").Append(propertyName).AppendLine("\",")
                .Append("            (global::System.ComponentModel.INotifyPropertyChanged __o) => ((").Append(inv.ViewModelTypeFullName)
                .Append(GeneratedSyntax.ObserverCastClose).Append(propertyName).AppendLine(",").AppendLine("            true);");
            return;
        }

        _ = sb.Append(InteractionObservationOpen)
            .Append("UnchangingPropertyObservable<").Append(interactionType).Append(">(viewModel.").Append(propertyName).AppendLine(");");
    }

    /// <summary>Appends the guard that leaves the binding inert until the view is given a view model.</summary>
    /// <param name="sb">The string builder to append to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendViewModelGuard(StringBuilder sb) =>
        sb.AppendLine("""
                              if (viewModel == null)
                              {
                                  return serial;
                              }
                      """);

    /// <summary>Names the generated worker a call site dispatches to.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The stable suffix its worker is named with.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string MethodSuffix(BindInteractionInvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.ViewTypeFullName,
            inv.CallerFilePath,
            inv.CallerLineNumber,
            inv.ExpressionText);

    /// <summary>Groups BindInteraction invocations by type signature for overload generation.</summary>
    /// <param name="ViewTypeFullName">The fully qualified view type.</param>
    /// <param name="ViewModelTypeFullName">The fully qualified view model type.</param>
    /// <param name="InputTypeFullName">The fully qualified interaction input type.</param>
    /// <param name="OutputTypeFullName">The fully qualified interaction output type.</param>
    /// <param name="IsTaskHandler">Whether the handler returns a task rather than an observable.</param>
    /// <param name="DontCareTypeFullName">The fully qualified handler result type that the binding discards, when there is one.</param>
    /// <param name="Invocations">The call sites sharing this group's shape.</param>
    internal sealed record BindInteractionTypeGroup(
        string ViewTypeFullName,
        string ViewModelTypeFullName,
        string InputTypeFullName,
        string OutputTypeFullName,
        bool IsTaskHandler,
        string? DontCareTypeFullName,
        BindInteractionInvocationInfo[] Invocations);
}

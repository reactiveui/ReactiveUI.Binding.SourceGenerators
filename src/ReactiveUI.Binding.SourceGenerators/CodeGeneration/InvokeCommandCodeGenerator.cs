// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

using static ReactiveUI.Binding.SourceGenerators.CodeGeneration.GeneratedTypeNames;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed overloads and the workers behind them for <c>InvokeCommand</c> invocations. The
/// command is reached by observing a property path, and each value the receiver produces is offered to whichever
/// command that path currently holds.
/// </summary>
internal static class InvokeCommandCodeGenerator
{
    /// <summary>The generated worker each dispatch branch hands the invocation to.</summary>
    private const string WorkerMethodPrefix = "__InvokeCommand_";

    /// <summary>The variable the observed command is assigned to inside a worker.</summary>
    private const string CommandVariable = "commandObs";

    /// <summary>The parameter carrying the text of the selector naming the command.</summary>
    private const string CommandExpressionParameter = "commandPropertyExpression";

    /// <summary>The objects a worker takes, in its own parameter order.</summary>
    private const string WorkerArguments = "source, target";

    /// <summary>The indentation and arrow an interceptor forwards to its worker behind.</summary>
    private const string ForwardingBodyPrefix = "            => ";

    /// <summary>Generates concrete typed overloads and workers for <c>InvokeCommand</c> invocations.</summary>
    /// <param name="invocations">All detected <c>InvokeCommand</c> invocations.</param>
    /// <param name="allClasses">All detected class binding info, for the observed type's mechanism.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot.</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? Generate(
        ImmutableArray<InvokeCommandInvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features) =>
        CodeGeneratorHelpers.GenerateDispatchFile(
            invocations,
            features,
            GroupByTypeSignature,
            (sb, group, snapshot) => EmitGroup(sb, group, allClasses, snapshot));

    /// <summary>Groups invocations by the types their overload declares.</summary>
    /// <param name="invocations">The <c>InvokeCommand</c> invocations to group.</param>
    /// <returns>A list of groups, each sharing one overload signature.</returns>
    /// <remarks>
    /// The selector's type is the stub's own <c>ICommand</c> rather than the command property's declared type, so
    /// only the observed value type and the target type shape an overload - a view model exposing a
    /// <c>ReactiveCommand</c> and one exposing a plain <c>ICommand</c> are served by the same one.
    /// </remarks>
    internal static List<InvokeCommandTypeGroup> GroupByTypeSignature(
        ImmutableArray<InvokeCommandInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<InvokeCommandInvocationInfo>>(invocations.Length);
        var keySb = new PooledStringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            _ = keySb.Clear()
                .Append(inv.SourceValueTypeFullName).Append('|')
                .Append(inv.TargetTypeFullName);

            var key = keySb.ToString();

            if (!groupMap.TryGetValue(key, out var list))
            {
                list = [];
                groupMap[key] = list;
            }

            list.Add(inv);
        }

        keySb.Return();

        var result = new List<InvokeCommandTypeGroup>();
        foreach (var kvp in groupMap)
        {
            var first = kvp.Value[0];
            result.Add(new(first.SourceValueTypeFullName, first.TargetTypeFullName, [.. kvp.Value]));
        }

        return result;
    }

    /// <summary>Emits one group: the way its call sites are reached, and one worker per distinct command path.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group being emitted.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <remarks>
    /// Call sites spelling the same selector reach the same worker, so the worker is keyed by the target type and
    /// that text rather than by the call site. Whether the group also collapses to one of those call sites is the
    /// snapshot's to say, because every API answers it the same way.
    /// </remarks>
    private static void EmitGroup(
        StringBuilder sb,
        InvokeCommandTypeGroup group,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features)
    {
        var emitted = features.CollapsesIndistinguishableCallSites
            ? group with
            {
                Invocations = CodeGeneratorHelpers.CollapseIndistinguishableCallSites(
                    group.Invocations,
                    static x => x.CommandExpressionText),
            }
            : group;

        if (features.SupportsInterceptors)
        {
            GenerateInterceptors(sb, emitted, in features);
        }
        else
        {
            GenerateConcreteOverload(sb, emitted, in features);
        }

        _ = sb.AppendLine();

        var emittedWorkers = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < emitted.Invocations.Length; i++)
        {
            var inv = emitted.Invocations[i];
            var suffix = WorkerSuffix(inv);
            if (!emittedWorkers.Add(suffix))
            {
                continue;
            }

            GenerateWorker(sb, inv, allClasses, suffix);
        }
    }

    /// <summary>Emits the concrete overload the call sites of one group resolve to.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group being emitted.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void GenerateConcreteOverload(
        StringBuilder sb,
        InvokeCommandTypeGroup group,
        in LanguageFeatures features)
    {
        var dispatchesOnExpressionText = features.SupportsCallerArgExpr;

        CodeGeneratorHelpers.AppendDispatchSummary(
            sb,
            Constants.InvokeCommandMethodName,
            ObservableOf(group.SourceValueTypeFullName),
            group.TargetTypeFullName,
            dispatchesOnExpressionText);

        _ = sb.Append("        public static ").Append(GeneratedTypeNames.IDisposable).Append(' ')
            .Append(Constants.InvokeCommandMethodName).AppendLine("(");

        AppendParameterList(sb, group, dispatchesOnExpressionText, features.SupportsNullable, features.StubHasExpressionParameters);

        _ = sb.AppendLine(GeneratedSyntax.MemberBodyOpen);

        if (dispatchesOnExpressionText)
        {
            CodeGeneratorHelpers.AppendStaticPrefixNormalization(sb, CommandExpressionParameter);
            _ = sb.AppendLine();
        }

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);

            if (dispatchesOnExpressionText)
            {
                _ = sb.Append(CodeGeneratorHelpers.ParameterIndent).Append(condition).Append(" (")
                    .Append(CommandExpressionParameter).Append(" == \"")
                    .Append(CodeGeneratorHelpers.EscapeString(inv.CommandExpressionText)).AppendLine("\")")
                    .AppendLine(GeneratedSyntax.StatementBlockOpen);
            }
            else
            {
                CodeGeneratorHelpers.AppendCallerInfoDispatchCondition(
                    sb,
                    condition,
                    inv.CallerLineNumber,
                    CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath));
            }

            CodeGeneratorHelpers.AppendDispatchReturn(sb, WorkerMethodPrefix + WorkerSuffix(inv), WorkerArguments);
        }

        CodeGeneratorHelpers.AppendBindingDispatchFallthrough(sb);
    }

    /// <summary>Emits one interceptor per worker, claiming every call site that reaches it.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group whose call sites are being claimed.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void GenerateInterceptors(
        StringBuilder sb,
        InvokeCommandTypeGroup group,
        in LanguageFeatures features)
    {
        var dispatchesOnExpressionText = features.SupportsCallerArgExpr;
        var supportsNullable = features.SupportsNullable;
        var stubHasExpressionParameters = features.StubHasExpressionParameters;

        foreach (var entry in InterceptorEmitter.GroupCallSites(group.Invocations, static x => x.Interceptor, WorkerSuffix))
        {
            foreach (var callSite in entry.Value)
            {
                InterceptorEmitter.AppendAttribute(sb, callSite.Interceptor, InterceptorEmitter.MemberIndent);
            }

            _ = sb.Append("        internal static ").Append(GeneratedTypeNames.IDisposable).Append(" __Intercept_")
                .Append(Constants.InvokeCommandMethodName).Append('_').Append(entry.Key).AppendLine("(");

            AppendParameterList(sb, group, dispatchesOnExpressionText, supportsNullable, stubHasExpressionParameters);

            _ = sb.Append(ForwardingBodyPrefix).Append(WorkerMethodPrefix).Append(entry.Key)
                .Append('(').Append(WorkerArguments).AppendLine(");").AppendLine();
        }
    }

    /// <summary>Writes the parameters an <c>InvokeCommand</c> member declares, closing the list.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group whose types the parameters are written from.</param>
    /// <param name="dispatchesOnExpressionText">Whether the captured expression text is what identifies a call site.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameter.</param>
    /// <remarks>
    /// One list serves the overload and the interceptor, because both have to be the stub's signature: the
    /// overload only wins resolution against a candidate it is otherwise indistinguishable from, and an
    /// interceptor is refused outright unless its signature is the intercepted method's. The selector is the
    /// stub's <c>ICommand</c> whatever the command property is declared as, for the same reason.
    /// </remarks>
    private static void AppendParameterList(
        StringBuilder sb,
        InvokeCommandTypeGroup group,
        bool dispatchesOnExpressionText,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        var commandType = supportsNullable ? $"{ICommand}?" : ICommand;

        _ = sb.Append("            this ").Append(ObservableOf(group.SourceValueTypeFullName)).AppendLine(" source,")
            .Append(CodeGeneratorHelpers.ParameterIndent).Append(group.TargetTypeFullName).AppendLine(" target,")
            .Append(CodeGeneratorHelpers.ParameterIndent)
            .Append(PropertyExpression(group.TargetTypeFullName, commandType)).AppendLine(" commandProperty,");

        if (dispatchesOnExpressionText || stubHasExpressionParameters)
        {
            CodeGeneratorHelpers.AppendExpressionParameter(
                sb,
                "commandProperty",
                CommandExpressionParameter,
                dispatchesOnExpressionText);
        }

        _ = sb.AppendLine(CodeGeneratorHelpers.CallerInfoParameterList);
    }

    /// <summary>Emits the worker that observes the command and offers it each value.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The call site being emitted.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="suffix">The stable method-name suffix for this worker.</param>
    /// <remarks>
    /// An absent target declares no property to observe, so it yields a subscription that does nothing rather
    /// than a fault - the same outcome as a target whose command property is null, which is the ordinary state
    /// before a view model is assigned.
    /// </remarks>
    private static void GenerateWorker(
        StringBuilder sb,
        InvokeCommandInvocationInfo inv,
        ImmutableArray<ClassBindingInfo> allClasses,
        string suffix)
    {
        var classInfo = CodeGeneratorHelpers.ResolveObservedTypeInfo(
            allClasses,
            inv.TargetTypeFullName,
            inv.CommandPropertyPath);

        _ = sb.Append("        private static ").Append(GeneratedTypeNames.IDisposable).Append(' ').Append(WorkerMethodPrefix).Append(suffix).AppendLine("(")
            .Append(CodeGeneratorHelpers.ParameterIndent).Append(ObservableOf(inv.SourceValueTypeFullName)).AppendLine(" source,")
            .Append(CodeGeneratorHelpers.ParameterIndent).Append(inv.TargetTypeFullName).AppendLine(" target)")
            .AppendLine(GeneratedSyntax.MemberBodyOpen)
            .Append("            // InvokeCommand: values -> ")
            .AppendLine(CodeGeneratorHelpers.BuildPropertyPathString(inv.CommandPropertyPath))
            .AppendLine("            if (target == null)")
            .AppendLine(GeneratedSyntax.StatementBlockOpen)
            .Append("                return ").Append(EmptyDisposable).AppendLine(".Instance;")
            .AppendLine(GeneratedSyntax.StatementBlockClose)
            .AppendLine();

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            "target",
            inv.CommandPropertyPath,
            inv.CommandPropertyPath[inv.CommandPropertyPath.Length - 1].PropertyTypeFullName,
            classInfo,
            CommandVariable);

        _ = sb.Append("            return ").Append(CommandInvoker).Append(".Invoke(source, ").Append(CommandVariable).AppendLine(");")
            .AppendLine(GeneratedSyntax.MemberBodyClose).AppendLine();
    }

    /// <summary>Names the worker a call site reaches.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The stable method-name suffix.</returns>
    /// <remarks>
    /// Keyed by the target type and the selector as written, not by the call site: two call sites spelling the
    /// same selector against the same type observe the same path, so one worker serves both.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string WorkerSuffix(InvokeCommandInvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.TargetTypeFullName,
            string.Empty,
            0,
            inv.CommandExpressionText);

    /// <summary>Groups <c>InvokeCommand</c> invocations sharing the observed value type and the target type.</summary>
    /// <param name="SourceValueTypeFullName">The fully qualified observable value type.</param>
    /// <param name="TargetTypeFullName">The fully qualified type declaring the command property.</param>
    /// <param name="Invocations">All invocations sharing this overload signature.</param>
    internal sealed record InvokeCommandTypeGroup(
        string SourceValueTypeFullName,
        string TargetTypeFullName,
        InvokeCommandInvocationInfo[] Invocations);
}

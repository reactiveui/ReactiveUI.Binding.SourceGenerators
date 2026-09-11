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
/// Generates concrete typed extension method overloads and binding methods for <c>BindTo</c> invocations.
/// The source is an observable stream applied to a target property; differing source/target types are
/// coerced at runtime via <c>RuntimeBindingConverter</c> (matching ReactiveUI's converter registry behavior).
/// </summary>
internal static class BindToCodeGenerator
{
    /// <summary>The indentation the direct-assignment subscription body sits at.</summary>
    private const string DirectSubscriptionBodyIndent = "                ";

    /// <summary>The indentation the converted-assignment subscription body sits at, one block deeper.</summary>
    private const string ConvertedSubscriptionBodyIndent = "                    ";

    /// <summary>The indentation and keyword a dispatch branch returns its binding behind.</summary>
    private const string ReturnStatementPrefix = "                return ";

    /// <summary>The indentation and arrow an interceptor forwards to its binding behind.</summary>
    private const string ForwardingBodyPrefix = "            => ";

    /// <summary>The selector a call site the runtime engine serves hands its worker, ahead of the conversion arguments.</summary>
    private const string ReflectionPropertyArgument = ", property";

    /// <summary>The indentation a forwarded argument sits at.</summary>
    private const string ArgumentIndent = "                ";

    /// <summary>The opening of a worker's return statement.</summary>
    private const string ReturnPrefix = "            return ";

    /// <summary>Generates concrete typed overloads and binding methods for <c>BindTo</c> invocations.</summary>
    /// <param name="invocations">All detected <c>BindTo</c> invocations.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    internal static string? Generate(
        ImmutableArray<BindToInvocationInfo> invocations,
        in LanguageFeatures features)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return null;
        }

        var sb = PooledBuilder.Rent(invocations.Length * CodeGeneratorHelpers.PerInvocationBufferCapacity);
        var supportsCallerArgExpr = features.SupportsCallerArgExpr;
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb, features);
        _ = sb.AppendLine();

        var groups = GroupByTypeSignature(invocations);

        for (var g = 0; g < groups.Count; g++)
        {
            var group = features.CollapsesIndistinguishableCallSites
                ? groups[g] with
                {
                    Invocations = CodeGeneratorHelpers.CollapseIndistinguishableCallSites(
                        groups[g].Invocations,
                        static x => x.TargetExpressionText),
                }
                : groups[g];

            if (features.SupportsInterceptors)
            {
                GenerateInterceptors(sb, group, in features);
            }
            else
            {
                GenerateConcreteOverload(sb, group, supportsCallerArgExpr, features.SupportsNullable, features.StubHasExpressionParameters);
            }

            _ = sb.AppendLine();

            for (var i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                    inv.TargetTypeFullName,
                    inv.CallerFilePath,
                    inv.CallerLineNumber,
                    inv.TargetExpressionText);
                GenerateBindToMethod(sb, inv, suffix);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        _ = sb.AppendLine();

        return PooledBuilder.ToStringAndReturn(sb);
    }

    /// <summary>
    /// Groups <c>BindTo</c> invocations by source value type, target type, target property type, and
    /// overload shape (conversion hint / converter override), so each group produces one concrete overload.
    /// </summary>
    /// <param name="invocations">The collection of <c>BindTo</c> invocation details to be grouped.</param>
    /// <returns>A list of grouped invocations, where each group shares the same overload signature.</returns>
    internal static List<BindToTypeGroup> GroupByTypeSignature(ImmutableArray<BindToInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<BindToInvocationInfo>>(invocations.Length);
        var keySb = new PooledStringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            _ = keySb.Clear()
                .Append(inv.SourceValueTypeFullName).Append('|')
                .Append(inv.TargetTypeFullName).Append('|')
                .Append(inv.TargetPropertyTypeFullName).Append('|')
                .Append(inv.HasConversionHint).Append('|')
                .Append(inv.HasConverterOverride);

            var key = keySb.ToString();

            if (!groupMap.TryGetValue(key, out var list))
            {
                list = [];
                groupMap[key] = list;
            }

            list.Add(inv);
        }

        keySb.Return();

        var result = new List<BindToTypeGroup>();
        foreach (var kvp in groupMap)
        {
            var first = kvp.Value[0];
            result.Add(new(
                first.SourceValueTypeFullName,
                first.TargetTypeFullName,
                first.TargetPropertyTypeFullName,
                first.TargetPropertyIsReferenceType,
                first.HasConversionHint,
                first.HasConverterOverride,
                [.. kvp.Value]));
        }

        return result;
    }

    /// <summary>
    /// Generates the concrete typed <c>BindTo</c> overload for a group, choosing the dispatch strategy
    /// based on whether CallerArgumentExpression is available.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of invocations sharing one overload signature.</param>
    /// <param name="supportsCallerArgExpr">Whether CallerArgumentExpression is supported.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateConcreteOverload(
        StringBuilder sb,
        BindToTypeGroup group,
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

    /// <summary>Generates a concrete <c>BindTo</c> overload that dispatches using CallerArgumentExpression.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of invocations sharing one overload signature.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void GenerateCallerArgExprOverload(StringBuilder sb, BindToTypeGroup group, bool supportsNullable)
    {
        _ = sb.AppendLine("        /// <summary>").Append("        /// Concrete typed overload for BindTo of ").Append(IObservable).Append("&lt;")
            .Append(group.SourceValueTypeFullName).Append("&gt; to ").Append(group.TargetTypeFullName).AppendLine(".")
            .AppendLine("        /// Uses CallerArgumentExpression for dispatch.").AppendLine("        /// </summary>").Append("        public static ")
            .Append(GeneratedTypeNames.IDisposable).AppendLine(" BindTo(");

        AppendParameterList(sb, group, true, supportsNullable, true);

        _ = sb.AppendLine(GeneratedSyntax.MemberBodyOpen)
            .AppendLine("            propertyExpression = propertyExpression.StartsWith(\"static \", global::System.StringComparison.Ordinal) ? propertyExpression.Substring(7) : propertyExpression;")
            .AppendLine();

        var extraArguments = FormatExtraArgs(group);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);
            var escapedTargetExpr = CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText);

            _ = sb.Append("            ").Append(condition).Append(" (propertyExpression == \"").Append(escapedTargetExpr).AppendLine("\")")
                .AppendLine(GeneratedSyntax.StatementBlockOpen);
            AppendWorkerInvocation(sb, ReturnStatementPrefix, BindToMethodSuffix(inv), WorkerArgumentsFor(inv, extraArguments));
            _ = sb.AppendLine("            }");
        }

        _ = sb.Append("            throw new ").Append(GeneratedTypeNames.InvalidOperationException).AppendLine("(").Append(ArgumentIndent).Append('"')
            .Append(NoBindingFoundMessage).AppendLine("\");").AppendLine(GeneratedSyntax.MemberBodyClose);
    }

    /// <summary>Generates a concrete <c>BindTo</c> overload that dispatches using CallerFilePath + CallerLineNumber.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of invocations sharing one overload signature.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateCallerFilePathOverload(
        StringBuilder sb,
        BindToTypeGroup group,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        _ = sb.AppendLine("        /// <summary>").Append("        /// Concrete typed overload for BindTo of ").Append(IObservable).Append("&lt;")
            .Append(group.SourceValueTypeFullName).Append("&gt; to ").Append(group.TargetTypeFullName).AppendLine(".")
            .AppendLine("        /// Uses CallerFilePath + CallerLineNumber for dispatch.").AppendLine("        /// </summary>")
            .Append("        public static ").Append(GeneratedTypeNames.IDisposable).AppendLine(" BindTo(");

        AppendParameterList(sb, group, false, supportsNullable, stubHasExpressionParameters);

        _ = sb.AppendLine(GeneratedSyntax.MemberBodyOpen);

        var extraArguments = FormatExtraArgs(group);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var pathSuffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);

            _ = sb.Append("            ").Append(condition).Append(" (callerLineNumber == ").Append(inv.CallerLineNumber).AppendLine()
                .Append("                && callerFilePath.EndsWith(\"").Append(CodeGeneratorHelpers.EscapeString(pathSuffix)).Append("\", ")
                .Append(OrdinalIgnoreCase).AppendLine("))").AppendLine(GeneratedSyntax.StatementBlockOpen);
            AppendWorkerInvocation(sb, ReturnStatementPrefix, BindToMethodSuffix(inv), WorkerArgumentsFor(inv, extraArguments));
            _ = sb.AppendLine("            }");
        }

        _ = sb.Append("            throw new ").Append(GeneratedTypeNames.InvalidOperationException).AppendLine("(").Append(ArgumentIndent).Append('"')
            .Append(NoBindingFoundMessage).AppendLine("\");").AppendLine(GeneratedSyntax.MemberBodyClose);
    }

    /// <summary>
    /// Generates the private worker method that subscribes to the source observable and assigns each
    /// value to the target property, coercing types when required.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="suffix">The stable method-name suffix.</param>
    internal static void GenerateBindToMethod(StringBuilder sb, BindToInvocationInfo inv, string suffix)
    {
        if (inv.ReflectionOnly)
        {
            GenerateReflectionBindToMethod(sb, inv, suffix);
            return;
        }

        var directAssignment = CodeGeneratorHelpers.BuildGuardedAssignment(
            "target",
            inv.TargetPropertyPath,
            "value",
            DirectSubscriptionBodyIndent);
        var convertedAssignment = CodeGeneratorHelpers.BuildGuardedAssignment(
            "target",
            inv.TargetPropertyPath,
            "__converted",
            ConvertedSubscriptionBodyIndent);
        var targetPathComment = CodeGeneratorHelpers.BuildPropertyPathString(inv.TargetPropertyPath);
        var extraParams = FormatExtraMethodParams(inv);

        // Direct assignment is only safe when the value type matches the property type and the caller did
        // not supply an explicit converter. A conversion hint alone is meaningless for identity assignment.
        var directAssign = inv.SourceValueTypeFullName == inv.TargetPropertyTypeFullName && !inv.HasConverterOverride;

        _ = sb.Append("        private static ").Append(GeneratedTypeNames.IDisposable).Append(" __BindTo_").Append(suffix).Append('(')
            .Append(ObservableOf(inv.SourceValueTypeFullName)).Append(" source, ").Append(inv.TargetTypeFullName).Append(" target").Append(extraParams)
            .AppendLine(")").AppendLine(GeneratedSyntax.MemberBodyOpen).Append("            // BindTo: observable -> ").Append(targetPathComment).AppendLine();

        if (directAssign)
        {
            _ = sb.Append(ReturnPrefix).Append(BindingErrors).AppendLine(".Subscribe(source, value =>").AppendLine(GeneratedSyntax.StatementBlockOpen)
                .Append("                ").Append(directAssignment).AppendLine().Append("            }, \"")
                .Append(CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText)).AppendLine("\");").AppendLine(GeneratedSyntax.MemberBodyClose).AppendLine();
        }
        else
        {
            _ = sb.Append(ReturnPrefix).Append(BindingErrors).AppendLine(".Subscribe(source, value =>").AppendLine(GeneratedSyntax.StatementBlockOpen)
                .Append("                if (").Append(RuntimeBindingConverter).Append(".TryConvert<").Append(inv.SourceValueTypeFullName).Append(", ")
                .Append(inv.TargetPropertyTypeFullName).Append(">(value, ").Append(FormatConversionArguments(inv)).AppendLine(", out var __converted))")
                .AppendLine("                {").Append("                    ").Append(convertedAssignment).AppendLine().AppendLine("                }")
                .Append("            }, \"").Append(CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText)).AppendLine("\");")
                .AppendLine(GeneratedSyntax.MemberBodyClose).AppendLine();
        }
    }

    /// <summary>Emits the worker for a call site whose selector only the runtime engine can resolve.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The call site being served.</param>
    /// <param name="suffix">The stable suffix naming this worker.</param>
    /// <remarks>
    /// The attribute is what makes this worth generating rather than leaving the call to the stub: a trimming or
    /// ahead-of-time publish reports this call site and no other, where annotating the shared overload would
    /// report every call site that merely has the same types.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void GenerateReflectionBindToMethod(StringBuilder sb, BindToInvocationInfo inv, string suffix) =>
        sb.Append(InterceptorEmitter.MemberIndent).AppendLine(GeneratedTypeNames.RequiresUnreferencedCodeAttribute)
            .Append("        private static ").Append(GeneratedTypeNames.IDisposable).Append(" __BindTo_").Append(suffix).Append('(')
            .Append(ObservableOf(inv.SourceValueTypeFullName)).Append(" source, ").Append(inv.TargetTypeFullName).Append(" target, ")
            .Append(PropertyExpression(inv.TargetTypeFullName, inv.TargetPropertyTypeFullName)).Append(" property")
            .Append(FormatExtraMethodParams(inv)).AppendLine(")")
            .AppendLine(GeneratedSyntax.MemberBodyOpen)
            .AppendLine("            // BindTo: the selector resolves at run time, so the engine reads the path.")
            .Append(ReturnPrefix).Append(GeneratedTypeNames.RuntimeBindingFallback).AppendLine(".BindTo(")
            .AppendLine("                source,")
            .AppendLine("                target,")
            .AppendLine("                property,")
            .Append(ArgumentIndent).Append(FormatConversionArguments(inv)).AppendLine(",")
            .AppendLine("                null,")
            .Append(ArgumentIndent).Append('"').Append(CodeGeneratorHelpers.EscapeString(inv.TargetExpressionText)).AppendLine("\");")
            .AppendLine(GeneratedSyntax.MemberBodyClose).AppendLine();

    /// <summary>Names the arguments a call site hands its worker, which the runtime path needs the selector in.</summary>
    /// <param name="inv">The call site being dispatched.</param>
    /// <param name="extraArguments">The conversion arguments this group's overload forwards.</param>
    /// <returns>The argument list, as written into the generated call.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string WorkerArgumentsFor(BindToInvocationInfo inv, string extraArguments) =>
        inv.ReflectionOnly ? ReflectionPropertyArgument + extraArguments : extraArguments;

    /// <summary>Appends the conversion-hint and converter-override parameters to the concrete overload signature.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    internal static void AppendExtraParameters(StringBuilder sb, BindToTypeGroup group)
    {
        if (group.HasConversionHint)
        {
            _ = sb.AppendLine("            object conversionHint,");
        }

        if (!group.HasConverterOverride)
        {
            return;
        }

        _ = sb.Append("            ").Append(IBindingTypeConverter).AppendLine(" converterOverride,");
    }

    /// <summary>Formats the extra arguments forwarded from the concrete overload to the worker method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>An argument list fragment like ", conversionHint, converterOverride", or empty.</returns>
    internal static string FormatExtraArgs(BindToTypeGroup group)
    {
        var sb = new PooledStringBuilder();
        if (group.HasConversionHint)
        {
            _ = sb.Append(", conversionHint");
        }

        if (group.HasConverterOverride)
        {
            _ = sb.Append(", converterOverride");
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>Formats the extra parameters for the private worker method signature.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>A parameter list fragment like ", object conversionHint, ... converterOverride", or empty.</returns>
    internal static string FormatExtraMethodParams(BindToInvocationInfo inv)
    {
        var sb = new PooledStringBuilder();
        if (inv.HasConversionHint)
        {
            _ = sb.Append(", object conversionHint");
        }

        if (inv.HasConverterOverride)
        {
            _ = sb.Append(", ").Append(IBindingTypeConverter).Append(" converterOverride");
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>Emits one interceptor per generated binding, claiming every call site that reaches it.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of call sites being claimed.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void GenerateInterceptors(StringBuilder sb, BindToTypeGroup group, in LanguageFeatures features)
    {
        var extraArguments = FormatExtraArgs(group);
        var dispatchesOnExpressionText = features.SupportsCallerArgExpr;
        var supportsNullable = features.SupportsNullable;
        var stubHasExpressionParameters = features.StubHasExpressionParameters;

        foreach (var entry in InterceptorEmitter.GroupCallSites(group.Invocations, static x => x.Interceptor, BindToMethodSuffix))
        {
            foreach (var callSite in entry.Value)
            {
                InterceptorEmitter.AppendAttribute(sb, callSite.Interceptor, InterceptorEmitter.MemberIndent);
            }

            if (entry.Value[0].ReflectionOnly)
            {
                _ = sb.Append(InterceptorEmitter.MemberIndent).AppendLine(GeneratedTypeNames.RequiresUnreferencedCodeAttribute);
            }

            _ = sb.Append("        internal static ").Append(GeneratedTypeNames.IDisposable).Append(" __Intercept_BindTo_")
                .Append(entry.Key).AppendLine("(");

            AppendParameterList(sb, group, dispatchesOnExpressionText, supportsNullable, stubHasExpressionParameters);

            AppendWorkerInvocation(sb, ForwardingBodyPrefix, entry.Key, WorkerArgumentsFor(entry.Value[0], extraArguments));
            _ = sb.AppendLine();
        }
    }

    /// <summary>Writes the parameters a <c>BindTo</c> member declares, closing the list.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group whose types the parameters are written from.</param>
    /// <param name="dispatchesOnExpressionText">Whether the captured expression text is what identifies a call site.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameter.</param>
    /// <remarks>
    /// One list serves the overload and the interceptor, because both have to be the stub's signature: the
    /// overload only wins resolution against a candidate it is otherwise indistinguishable from, and an
    /// interceptor is refused outright unless its signature is the intercepted method's.
    /// </remarks>
    private static void AppendParameterList(
        StringBuilder sb,
        BindToTypeGroup group,
        bool dispatchesOnExpressionText,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        var targetPropType = CodeGeneratorHelpers.NullableSelectorType(
            group.TargetPropertyTypeFullName,
            group.TargetPropertyIsReferenceType,
            supportsNullable);

        _ = sb.Append("            this ").Append(ObservableOf(group.SourceValueTypeFullName))
            .AppendLine(" source,").Append("            ").Append(group.TargetTypeFullName).AppendLine(" target,")
            .Append("            ").Append(PropertyExpression(group.TargetTypeFullName, targetPropType)).AppendLine(" property,");

        AppendExtraParameters(sb, group);

        if (dispatchesOnExpressionText || stubHasExpressionParameters)
        {
            CodeGeneratorHelpers.AppendExpressionParameter(sb, "property", "propertyExpression", dispatchesOnExpressionText);
        }

        _ = sb.AppendLine(CodeGeneratorHelpers.CallerInfoParameterList);
    }

    /// <summary>Appends the call forwarding the bound stream and target on to the generated worker.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="prefix">The indentation and statement keyword the call sits behind.</param>
    /// <param name="methodSuffix">The stable suffix naming the worker.</param>
    /// <param name="extraArguments">The conversion-hint and converter-override arguments, if any.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendWorkerInvocation(StringBuilder sb, string prefix, string methodSuffix, string extraArguments) =>
        sb.Append(prefix).Append("__BindTo_").Append(methodSuffix).Append("(source, target").Append(extraArguments).AppendLine(");");

    /// <summary>Names the generated binding a call site reaches.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The stable method-name suffix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string BindToMethodSuffix(BindToInvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.TargetTypeFullName,
            inv.CallerFilePath,
            inv.CallerLineNumber,
            inv.TargetExpressionText);

    /// <summary>Formats the conversion-hint and converter-override arguments handed to the runtime converter.</summary>
    /// <param name="inv">The invocation info.</param>
    /// <returns>The two arguments, comma separated.</returns>
    private static string FormatConversionArguments(BindToInvocationInfo inv) =>
        $"{(inv.HasConversionHint ? "conversionHint" : "null")}, {(inv.HasConverterOverride ? "converterOverride" : "null")}";

    /// <summary>Groups <c>BindTo</c> invocations sharing source value type, target type, target property type, and overload shape.</summary>
    /// <param name="SourceValueTypeFullName">The fully qualified observable value type.</param>
    /// <param name="TargetTypeFullName">The fully qualified target object type.</param>
    /// <param name="TargetPropertyTypeFullName">The fully qualified target property type.</param>
    /// <param name="TargetPropertyIsReferenceType">Whether that type is a reference type, which annotates the selector parameter.</param>
    /// <param name="HasConversionHint">Whether this group's overload takes a conversion hint.</param>
    /// <param name="HasConverterOverride">Whether this group's overload takes an explicit converter.</param>
    /// <param name="Invocations">All invocations sharing this overload signature.</param>
    internal sealed record BindToTypeGroup(
        string SourceValueTypeFullName,
        string TargetTypeFullName,
        string TargetPropertyTypeFullName,
        bool TargetPropertyIsReferenceType,
        bool HasConversionHint,
        bool HasConverterOverride,
        BindToInvocationInfo[] Invocations);
}

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Generates the typed overloads, interceptors and helper factories for <c>ToProperty</c> invocations.</summary>
/// <remarks>
/// <para>
/// Each generated worker hands <c>ObservableAsPropertyHelper&lt;T&gt;.Create</c> the owning object and two lambdas that
/// receive it as an argument. The lambdas capture nothing, so the compiler caches each as one static delegate and a
/// helper costs no closure. The property name is a literal, and where the raise member takes event args, the args are
/// one static instance per property.
/// </para>
/// <para>
/// A raise the generated class cannot make - an event, or a protected method - is made by an accessor the generator
/// adds to the partial type itself. Those accessors are written after the dispatch class, in the type's own namespace.
/// </para>
/// </remarks>
internal static class ToPropertyCodeGenerator
{
    /// <summary>The generic helper type every worker constructs.</summary>
    internal const string HelperType = "global::ReactiveUI.Binding.ObservableAsPropertyHelper";

    /// <summary>The event-args type a changed raise passes.</summary>
    private const string ChangedEventArgsType = "global::System.ComponentModel.PropertyChangedEventArgs";

    /// <summary>The event-args type a changing raise passes.</summary>
    private const string ChangingEventArgsType = "global::System.ComponentModel.PropertyChangingEventArgs";

    /// <summary>The attribute hiding a generated member from completion lists.</summary>
    private const string EditorBrowsableNever =
        "[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]";

    /// <summary>The spaces one nesting level indents by.</summary>
    private const int IndentWidth = 4;

    /// <summary>The nesting level of a member of the generated class: namespace, then class.</summary>
    private const int MemberIndentLevel = 2;

    /// <summary>The name each raise lambda gives the owning object.</summary>
    private const string OwnerParameter = "__owner";

    /// <summary>The indentation and arrow an interceptor forwards to its worker behind.</summary>
    private const string ForwardingBodyPrefix = "            => ";

    /// <summary>The message a generated overload throws when no call site matches.</summary>
    private const string NoDispatchMessage =
        "No generated ToProperty dispatch matched this call site. Name the property with a lambda of the form x => x.Property or with a constant.";

    /// <summary>Generates the dispatch file for all <c>ToProperty</c> call sites.</summary>
    /// <param name="invocations">All detected <c>ToProperty</c> invocations.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <returns>The generated source, or null when there are no invocations.</returns>
    internal static string? Generate(ImmutableArray<ToPropertyInvocationInfo> invocations, in LanguageFeatures features)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return null;
        }

        var sb = PooledBuilder.Rent(invocations.Length * CodeGeneratorHelpers.PerInvocationBufferCapacity);
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb, features);
        _ = sb.AppendLine();

        var groups = GroupByTypeSignature(invocations);
        for (var g = 0; g < groups.Count; g++)
        {
            // A string name is matched on its value, and a selector on its text under expression dispatch; either way
            // call sites that agree on the key reach the same body, so only the first of them needs a branch.
            var group = groups[g];
            if (group.Shape.NamesPropertyByString)
            {
                group = group with { Invocations = CodeGeneratorHelpers.CollapseIndistinguishableCallSites(group.Invocations, static x => x.PropertyName) };
            }
            else if (features.CollapsesIndistinguishableCallSites)
            {
                group = group with { Invocations = CodeGeneratorHelpers.CollapseIndistinguishableCallSites(group.Invocations, static x => x.PropertyExpressionText) };
            }

            // Each call site's worker name is hashed once here and reused by the overload, the interceptors and the
            // worker, rather than recomputed at each of them.
            var sites = new Site[group.Invocations.Length];
            for (var i = 0; i < sites.Length; i++)
            {
                sites[i] = new(group.Invocations[i], WorkerSuffix(group.Invocations[i]));
            }

            if (features.SupportsInterceptors)
            {
                GenerateInterceptors(sb, group, sites, features);
            }
            else
            {
                GenerateConcreteOverload(sb, group, sites, features);
            }

            _ = sb.AppendLine();

            for (var i = 0; i < sites.Length; i++)
            {
                GenerateWorker(sb, sites[i], group.ValueTypeDisplay, features.SupportsNullable);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        _ = sb.AppendLine();

        AppendAccessors(sb, invocations);

        return PooledBuilder.ToStringAndReturn(sb);
    }

    /// <summary>Groups call sites by the overload they share: source type, value type and stub shape.</summary>
    /// <param name="invocations">The detected call sites.</param>
    /// <returns>One group per generated overload.</returns>
    internal static List<ToPropertyTypeGroup> GroupByTypeSignature(ImmutableArray<ToPropertyInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<GroupKey, List<ToPropertyInvocationInfo>>(invocations.Length);
        var order = new List<GroupKey>(invocations.Length);

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            var key = new GroupKey(inv.SourceTypeFullName, inv.ValueTypeFullName, inv.Shape);
            if (!groupMap.TryGetValue(key, out var list))
            {
                list = [];
                groupMap[key] = list;
                order.Add(key);
            }

            list.Add(inv);
        }

        var result = new List<ToPropertyTypeGroup>(order.Count);
        for (var i = 0; i < order.Count; i++)
        {
            var key = order[i];
            result.Add(new(key.SourceTypeFullName, key.ValueTypeFullName, key.Shape, [.. groupMap[key]]));
        }

        return result;
    }

    /// <summary>Generates the concrete overload that dispatches a group's call sites to their workers.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of call sites sharing one overload.</param>
    /// <param name="sites">The group's call sites, each with its worker suffix.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    internal static void GenerateConcreteOverload(StringBuilder sb, ToPropertyTypeGroup group, Site[] sites, in LanguageFeatures features)
    {
        var dispatchesOnExpressionText = features.SupportsCallerArgExpr;
        var byName = group.Shape.NamesPropertyByString;
        _ = sb.AppendLine("        /// <summary>").Append("        /// Concrete typed overload for ToProperty of ").Append(group.SourceTypeFullName)
            .AppendLine(".")
            .AppendLine(DispatchSummary(byName, dispatchesOnExpressionText))
            .AppendLine("        /// </summary>");

        // A positional initial value of the property's type can also fill a caller-information parameter of the
        // overload without one; the stub breaks that tie the same way, and so must the overloads beside it here.
        if (!byName && group.Shape.InitialValue == ToPropertyInitialValueKind.Value && features.SupportsOverloadResolutionPriority)
        {
            _ = sb.AppendLine("        [global::System.Runtime.CompilerServices.OverloadResolutionPriority(1)]");
        }

        _ = sb.Append("        public static ");
        _ = AppendHelperType(sb, group.ValueTypeDisplay).AppendLine(" ToProperty(");

        AppendParameterList(sb, group, dispatchesOnExpressionText, features);
        _ = sb.AppendLine(GeneratedSyntax.MemberBodyOpen);

        for (var i = 0; i < sites.Length; i++)
        {
            var inv = sites[i].Invocation;
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);
            if (byName)
            {
                _ = sb.Append("            ").Append(condition).Append(" (property == \"")
                    .Append(CodeGeneratorHelpers.EscapeString(inv.PropertyName)).AppendLine("\")")
                    .AppendLine(GeneratedSyntax.StatementBlockOpen);
            }
            else if (dispatchesOnExpressionText)
            {
                _ = sb.Append("            ").Append(condition).Append(" (propertyExpression == \"")
                    .Append(CodeGeneratorHelpers.EscapeString(inv.PropertyExpressionText)).AppendLine("\")")
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

            AppendWorkerCall(sb, group, sites[i].Suffix);
            _ = sb.AppendLine(GeneratedSyntax.StatementBlockClose);
        }

        _ = sb.Append("            throw new ").Append(GeneratedTypeNames.InvalidOperationException).AppendLine("(")
            .Append("                \"").Append(NoDispatchMessage).AppendLine("\");")
            .AppendLine(GeneratedSyntax.MemberBodyClose);
    }

    /// <summary>Generates the worker that creates the helper for one call site.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="site">The call site and its worker suffix.</param>
    /// <param name="valueType">The value type as the group declares it.</param>
    /// <param name="supportsNullable">Whether the consumer compiles with nullable reference types.</param>
    internal static void GenerateWorker(StringBuilder sb, in Site site, string valueType, bool supportsNullable)
    {
        var (inv, suffix) = site;
        var raise = inv.Raise;

        // Event args are cached once per property, in the generated class for a raise made from here; an
        // accessor inside the type caches its own.
        if (raise.Accessor is null)
        {
            AppendArgsField(sb, raise.Changed, isChanging: false, suffix, inv.PropertyName, MemberIndentLevel);
            if (raise.Changing is { } changingCall)
            {
                AppendArgsField(sb, changingCall, isChanging: true, suffix, inv.PropertyName, MemberIndentLevel);
            }
        }

        _ = sb.Append("        private static ");
        _ = AppendHelperType(sb, valueType).Append(" __ToProperty_").Append(suffix).AppendLine("(")
            .Append("            ").Append(GeneratedTypeNames.IObservable).Append('<').Append(valueType).AppendLine("> target,")
            .Append("            ").Append(inv.SourceTypeFullName).AppendLine(" source,")
            .Append("            ");
        _ = AppendInitialValueParameter(sb, inv.Shape.InitialValue, valueType).AppendLine(",")
            .AppendLine("            bool deferSubscription,")
            .Append("            ").Append(GeneratedTypeNames.ISequencer).Append(supportsNullable ? "?" : string.Empty).AppendLine(" scheduler)")
            .AppendLine(GeneratedSyntax.MemberBodyOpen)
            .Append("            // ToProperty: ").Append(inv.SourceTypeFullName).Append('.').Append(inv.PropertyName)
            .Append(" raised through ").AppendLine(raise.Mechanism)
            .Append("            return ");
        _ = AppendHelperType(sb, valueType).AppendLine(".Create(")
            .AppendLine("                target,")
            .AppendLine("                source,");

        AppendRaiseLambda(sb, inv, raise.Changed, isChanging: false, suffix);
        _ = sb.AppendLine(",");
        if (raise.Changing is { } changing)
        {
            AppendRaiseLambda(sb, inv, changing, isChanging: true, suffix);
            _ = sb.AppendLine(",");
        }
        else
        {
            _ = sb.AppendLine("                null,");
        }

        _ = sb.Append("                ").Append(inv.Shape.InitialValue == ToPropertyInitialValueKind.Factory ? "getInitialValue" : "initialValue")
            .AppendLine(",")
            .AppendLine("                deferSubscription,")
            .AppendLine("                scheduler);")
            .AppendLine(GeneratedSyntax.MemberBodyClose)
            .AppendLine();
    }

    /// <summary>Emits one interceptor per worker, claiming every call site that reaches it.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of call sites being claimed.</param>
    /// <param name="sites">The group's call sites, each with its worker suffix.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void GenerateInterceptors(StringBuilder sb, ToPropertyTypeGroup group, Site[] sites, in LanguageFeatures features)
    {
        foreach (var entry in InterceptorEmitter.GroupCallSites(sites, static x => x.Invocation.Interceptor, static x => x.Suffix))
        {
            foreach (var callSite in entry.Value)
            {
                InterceptorEmitter.AppendAttribute(sb, callSite.Invocation.Interceptor, InterceptorEmitter.MemberIndent);
            }

            _ = sb.Append("        internal static ");
            _ = AppendHelperType(sb, group.ValueTypeDisplay).Append(" __Intercept_ToProperty_").Append(entry.Key).AppendLine("(");
            AppendParameterList(sb, group, features.SupportsCallerArgExpr, features);

            _ = sb.Append(ForwardingBodyPrefix);
            if (group.Shape.HasResult)
            {
                _ = sb.Append("result = ");
            }

            _ = sb.Append("__ToProperty_").Append(entry.Key).Append('(');
            _ = AppendWorkerArguments(sb, group).AppendLine(");").AppendLine();
        }
    }

    /// <summary>Writes the stub's parameter list, which the overload and the interceptor both have to match exactly.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group whose types and shape the parameters are written from.</param>
    /// <param name="dispatchesOnExpressionText">Whether the captured expression text identifies a call site.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <remarks>
    /// The property selector is a <c>Func</c>, not an expression tree. A lambda converts to either, and the compiler
    /// ranks the two conversions equally, so the overload still wins; but converting it to a delegate costs nothing
    /// for a lambda that captures nothing, where building an expression tree allocates and reflects at every call.
    /// </remarks>
    private static void AppendParameterList(
        StringBuilder sb,
        ToPropertyTypeGroup group,
        bool dispatchesOnExpressionText,
        in LanguageFeatures features)
    {
        var shape = group.Shape;
        _ = sb.Append("            this ").Append(GeneratedTypeNames.IObservable).Append('<').Append(group.ValueTypeDisplay).Append("> target");
        _ = NextParameter(sb).Append(group.SourceTypeFullName).Append(" source");
        _ = NextParameter(sb);
        if (shape.NamesPropertyByString)
        {
            _ = sb.Append("string property");
        }
        else
        {
            _ = sb.Append(GeneratedTypeNames.Func).Append('<').Append(group.SourceTypeFullName).Append(", ").Append(group.ValueTypeDisplay).Append("> property");
        }

        if (shape.HasResult)
        {
            _ = AppendHelperType(NextParameter(sb).Append("out "), group.ValueTypeDisplay).Append(" result");
        }

        if (shape.InitialValue != ToPropertyInitialValueKind.None)
        {
            _ = AppendInitialValueParameter(NextParameter(sb), shape.InitialValue, group.ValueTypeDisplay);
        }

        if (shape.HasDeferSubscription)
        {
            _ = NextParameter(sb).Append("bool deferSubscription");
        }

        if (shape.HasScheduler)
        {
            _ = NextParameter(sb).Append(GeneratedTypeNames.ISequencer).Append(features.SupportsNullable ? "?" : string.Empty).Append(" scheduler");
        }

        // The stub for a string name declares no caller information, so its list closes here; every other list
        // continues into the caller-information parameters.
        if (shape.NamesPropertyByString)
        {
            _ = sb.AppendLine(")");
            return;
        }

        _ = sb.AppendLine(",");
        if (dispatchesOnExpressionText || features.StubHasExpressionParameters)
        {
            CodeGeneratorHelpers.AppendExpressionParameter(sb, "property", "propertyExpression", dispatchesOnExpressionText);
        }

        _ = sb.AppendLine(CodeGeneratorHelpers.CallerInfoParameterList);
    }

    /// <summary>Appends the arguments an overload forwards to its worker, filling the ones its shape lacks.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group whose shape decides the arguments.</param>
    /// <returns>The builder, for chaining.</returns>
    private static StringBuilder AppendWorkerArguments(StringBuilder sb, ToPropertyTypeGroup group)
    {
        var shape = group.Shape;
        _ = sb.Append("target, source, ");
        _ = shape.InitialValue switch
        {
            ToPropertyInitialValueKind.Value => sb.Append("initialValue"),
            ToPropertyInitialValueKind.Factory => sb.Append("getInitialValue"),
            _ => sb.Append("default(").Append(group.ValueTypeDisplay).Append(')'),
        };
        return sb.Append(shape.HasDeferSubscription ? ", deferSubscription" : ", false")
            .Append(shape.HasScheduler ? ", scheduler" : ", null");
    }

    /// <summary>Appends the call a matched branch hands the helper to, and its return.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group, whose shape decides the arguments and whether <c>result</c> is assigned.</param>
    /// <param name="suffix">The worker's stable suffix.</param>
    private static void AppendWorkerCall(StringBuilder sb, ToPropertyTypeGroup group, string suffix)
    {
        _ = sb.Append(group.Shape.HasResult ? "                result = __ToProperty_" : "                return __ToProperty_").Append(suffix).Append('(');
        _ = AppendWorkerArguments(sb, group).AppendLine(");");
        if (group.Shape.HasResult)
        {
            _ = sb.AppendLine("                return result;");
        }
    }

    /// <summary>Appends the non-capturing lambda that raises one notification for the owning object.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The call site.</param>
    /// <param name="call">The raise call.</param>
    /// <param name="isChanging">Whether this raises the before-change notification.</param>
    /// <param name="suffix">The worker's stable suffix, which names its cached event args.</param>
    private static void AppendRaiseLambda(StringBuilder sb, ToPropertyInvocationInfo inv, in PropertyRaiseCall call, bool isChanging, string suffix)
    {
        _ = sb.Append("                (").Append(OwnerParameter).Append(", __value) => ");

        if (inv.Raise.Accessor is not null)
        {
            _ = AppendOwnerCast(sb, inv.SourceTypeFullName).Append('.');
            _ = AppendAccessorName(sb, inv.PropertyName, isChanging).Append("()");
            return;
        }

        AppendRaiseCall(sb, call, inv.SourceTypeFullName, inv.PropertyName, isChanging, suffix);
    }

    /// <summary>Appends the expression that raises one notification.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="call">The raise call.</param>
    /// <param name="castType">The type the owner parameter is cast to, or null when the receiver is <c>this</c>.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="isChanging">Whether this raises the before-change notification.</param>
    /// <param name="argsKey">What names the cached event-args field: the worker suffix, or the property name in an accessor.</param>
    private static void AppendRaiseCall(StringBuilder sb, in PropertyRaiseCall call, string? castType, string propertyName, bool isChanging, string argsKey)
    {
        switch (call.Kind)
        {
            case PropertyRaiseCallKind.StaticMethod:
            {
                _ = sb.Append(call.Owner).Append('.').Append(call.Member).Append('(');
                _ = AppendReceiver(sb, castType).Append(", ");
                break;
            }

            case PropertyRaiseCallKind.EventInvoke:
            {
                _ = AppendReceiver(sb, castType).Append('.').Append(call.Member).Append("?.Invoke(");
                _ = AppendReceiver(sb, castType).Append(", ");
                break;
            }

            default:
            {
                _ = AppendReceiver(sb, castType).Append('.').Append(call.Member).Append('(');
                break;
            }
        }

        if (call.Argument == PropertyRaiseArgumentKind.EventArgs)
        {
            _ = AppendArgsFieldName(sb, isChanging, argsKey);
        }
        else
        {
            _ = sb.Append('"').Append(CodeGeneratorHelpers.EscapeString(propertyName)).Append('"');
        }

        _ = sb.Append(')');
    }

    /// <summary>Appends the receiver of a raise call: the cast owner parameter, or <c>this</c>.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="castType">The type the owner parameter is cast to, or null for <c>this</c>.</param>
    /// <returns>The builder, for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static StringBuilder AppendReceiver(StringBuilder sb, string? castType) =>
        castType is null ? sb.Append("this") : AppendOwnerCast(sb, castType);

    /// <summary>Appends the owner parameter cast to the source type.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="sourceType">The fully qualified source type.</param>
    /// <returns>The builder, for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static StringBuilder AppendOwnerCast(StringBuilder sb, string sourceType) =>
        sb.Append("((").Append(sourceType).Append(')').Append(OwnerParameter).Append(')');

    /// <summary>Appends the static field caching one property's event args, when the raise call passes them.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="call">The raise call.</param>
    /// <param name="isChanging">Whether the field holds before-change event args.</param>
    /// <param name="argsKey">What names the field: the worker suffix, or the property name in an accessor.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="indentLevel">The member nesting level.</param>
    private static void AppendArgsField(StringBuilder sb, in PropertyRaiseCall call, bool isChanging, string argsKey, string propertyName, int indentLevel)
    {
        if (call.Argument != PropertyRaiseArgumentKind.EventArgs)
        {
            return;
        }

        var argsType = isChanging ? ChangingEventArgsType : ChangedEventArgsType;
        _ = AppendIndent(sb, indentLevel).Append("private static readonly ").Append(argsType).Append(' ');
        _ = AppendArgsFieldName(sb, isChanging, argsKey).Append(" = new ").Append(argsType)
            .Append("(\"").Append(CodeGeneratorHelpers.EscapeString(propertyName)).AppendLine("\");").AppendLine();
    }

    /// <summary>Appends the accessors raise calls reach when they can only run inside the type, one per type and property.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="invocations">All call sites.</param>
    private static void AppendAccessors(StringBuilder sb, ImmutableArray<ToPropertyInvocationInfo> invocations)
    {
        // Most compilations raise without an accessor, so the collections below are only built when one is needed.
        if (!NeedsAccessors(invocations))
        {
            return;
        }

        var seen = new HashSet<(PartialTypeDeclaration Declaration, string Property)>();
        var byDeclaration = new Dictionary<PartialTypeDeclaration, List<ToPropertyInvocationInfo>>();
        var order = new List<PartialTypeDeclaration>();

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            if (inv.Raise.Accessor is not { } declaration || !seen.Add((declaration, inv.PropertyName)))
            {
                continue;
            }

            if (!byDeclaration.TryGetValue(declaration, out var list))
            {
                list = [];
                byDeclaration[declaration] = list;
                order.Add(declaration);
            }

            list.Add(inv);
        }

        for (var d = 0; d < order.Count; d++)
        {
            AppendAccessorDeclaration(sb, order[d], byDeclaration[order[d]]);
        }
    }

    /// <summary>Determines whether any call site raises through an accessor added to its type.</summary>
    /// <param name="invocations">All call sites.</param>
    /// <returns><see langword="true"/> when at least one accessor has to be written.</returns>
    private static bool NeedsAccessors(ImmutableArray<ToPropertyInvocationInfo> invocations)
    {
        for (var i = 0; i < invocations.Length; i++)
        {
            if (invocations[i].Raise.Accessor is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Appends one partial declaration holding the accessors for the properties it backs.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="declaration">The partial declaration.</param>
    /// <param name="invocations">One call site per property.</param>
    private static void AppendAccessorDeclaration(StringBuilder sb, PartialTypeDeclaration declaration, List<ToPropertyInvocationInfo> invocations)
    {
        var baseLevel = 0;
        _ = sb.AppendLine();
        if (declaration.Namespace is { } ns)
        {
            _ = sb.Append("namespace ").AppendLine(ns).AppendLine("{");
            baseLevel = 1;
        }

        var headers = declaration.TypeHeaders;
        for (var i = 0; i < headers.Length; i++)
        {
            _ = AppendIndent(sb, baseLevel + i).AppendLine(headers[i]);
            _ = AppendIndent(sb, baseLevel + i).AppendLine("{");
        }

        var memberLevel = baseLevel + headers.Length;
        for (var i = 0; i < invocations.Count; i++)
        {
            var inv = invocations[i];
            if (i > 0)
            {
                _ = sb.AppendLine();
            }

            AppendAccessor(sb, inv.Raise.Changed, inv.PropertyName, isChanging: false, memberLevel);
            if (inv.Raise.Changing is not { } changing)
            {
                continue;
            }

            _ = sb.AppendLine();
            AppendAccessor(sb, changing, inv.PropertyName, isChanging: true, memberLevel);
        }

        for (var i = headers.Length - 1; i >= 0; i--)
        {
            _ = AppendIndent(sb, baseLevel + i).AppendLine("}");
        }

        if (declaration.Namespace is not null)
        {
            _ = sb.AppendLine("}");
        }
    }

    /// <summary>Appends one accessor and the event args it caches.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="call">The raise call the accessor makes.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="isChanging">Whether the accessor raises the before-change notification.</param>
    /// <param name="indentLevel">The member nesting level.</param>
    private static void AppendAccessor(StringBuilder sb, in PropertyRaiseCall call, string propertyName, bool isChanging, int indentLevel)
    {
        AppendArgsField(sb, call, isChanging, propertyName, propertyName, indentLevel);

        _ = AppendIndent(sb, indentLevel).AppendLine(EditorBrowsableNever);
        _ = AppendIndent(sb, indentLevel).Append("internal void ");
        _ = AppendAccessorName(sb, propertyName, isChanging).Append("() => ");
        AppendRaiseCall(sb, call, null, propertyName, isChanging, propertyName);
        _ = sb.AppendLine(";");
    }

    /// <summary>Appends the name of the accessor that raises one notification for a property.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="isChanging">Whether the accessor raises the before-change notification.</param>
    /// <returns>The builder, for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static StringBuilder AppendAccessorName(StringBuilder sb, string propertyName, bool isChanging) =>
        sb.Append(isChanging ? "__ToPropertyRaiseChanging_" : "__ToPropertyRaiseChanged_").Append(propertyName);

    /// <summary>Appends the name of a cached event-args field.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="isChanging">Whether the field holds before-change event args.</param>
    /// <param name="argsKey">The worker suffix, or the property name in an accessor.</param>
    /// <returns>The builder, for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static StringBuilder AppendArgsFieldName(StringBuilder sb, bool isChanging, string argsKey) =>
        sb.Append(isChanging ? "__ToPropertyChangingArgs_" : "__ToPropertyChangedArgs_").Append(argsKey);

    /// <summary>Ends the previous parameter and indents the next one.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <returns>The builder, for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static StringBuilder NextParameter(StringBuilder sb) => sb.AppendLine(",").Append("            ");

    /// <summary>Appends the indentation for a nesting level, without building a string for it.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="level">The nesting level.</param>
    /// <returns>The builder, for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static StringBuilder AppendIndent(StringBuilder sb, int level) => sb.Append(' ', level * IndentWidth);

    /// <summary>Appends the closed helper type for a value type.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="valueType">The fully qualified value type.</param>
    /// <returns>The builder, for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static StringBuilder AppendHelperType(StringBuilder sb, string valueType) =>
        sb.Append(HelperType).Append('<').Append(valueType).Append('>');

    /// <summary>Appends the worker's initial-value parameter declaration.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="kind">How the call site supplies the initial value.</param>
    /// <param name="valueType">The fully qualified value type.</param>
    /// <returns>The builder, for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static StringBuilder AppendInitialValueParameter(StringBuilder sb, ToPropertyInitialValueKind kind, string valueType) =>
        kind == ToPropertyInitialValueKind.Factory
            ? sb.Append(GeneratedTypeNames.Func).Append('<').Append(valueType).Append("> getInitialValue")
            : sb.Append(valueType).Append(" initialValue");

    /// <summary>Names the worker a call site reaches.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The stable method-name suffix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string WorkerSuffix(ToPropertyInvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.SourceTypeFullName,
            inv.CallerFilePath,
            inv.CallerLineNumber,
            inv.PropertyExpressionText);

    /// <summary>Describes how a concrete overload tells its call sites apart.</summary>
    /// <param name="byName">Whether the overload takes the property's name as a string.</param>
    /// <param name="dispatchesOnExpressionText">Whether a selector's captured text identifies a call site.</param>
    /// <returns>The summary line.</returns>
    private static string DispatchSummary(bool byName, bool dispatchesOnExpressionText)
    {
        if (byName)
        {
            return "        /// Dispatches on the property name.";
        }

        return dispatchesOnExpressionText
            ? "        /// Uses CallerArgumentExpression for dispatch."
            : "        /// Uses CallerFilePath + CallerLineNumber for dispatch.";
    }

    /// <summary>A call site with its worker suffix, hashed once.</summary>
    /// <param name="Invocation">The call site.</param>
    /// <param name="Suffix">The stable suffix naming its worker.</param>
    internal readonly record struct Site(ToPropertyInvocationInfo Invocation, string Suffix);

    /// <summary>The key call sites sharing one overload group under.</summary>
    /// <param name="SourceTypeFullName">The fully qualified source type.</param>
    /// <param name="ValueTypeFullName">The fully qualified value type.</param>
    /// <param name="Shape">The stub overload shape.</param>
    private readonly record struct GroupKey(string SourceTypeFullName, string ValueTypeFullName, ToPropertyOverloadShape Shape);

    /// <summary>Call sites sharing one generated overload.</summary>
    /// <param name="SourceTypeFullName">The fully qualified source type.</param>
    /// <param name="ValueTypeFullName">The fully qualified value type.</param>
    /// <param name="Shape">The stub overload shape.</param>
    /// <param name="Invocations">The call sites.</param>
    /// <remarks>
    /// Call sites that infer <c>string</c> and <c>string?</c> share one group, since overloads differing only by a
    /// nullable annotation would collide. The group is written with the annotated name when every call site in it
    /// agrees, which is the usual case of one call site per property, and without it otherwise.
    /// </remarks>
    internal sealed record ToPropertyTypeGroup(
        string SourceTypeFullName,
        string ValueTypeFullName,
        ToPropertyOverloadShape Shape,
        ToPropertyInvocationInfo[] Invocations)
    {
        /// <summary>Gets the value type as the generated members declare it.</summary>
        public string ValueTypeDisplay { get; } = AgreedDisplay(ValueTypeFullName, Invocations);

        /// <summary>Picks the annotated value type when every call site inferred the same one.</summary>
        /// <param name="plain">The value type without annotations.</param>
        /// <param name="invocations">The group's call sites.</param>
        /// <returns>The annotated name all call sites share, or the plain name.</returns>
        private static string AgreedDisplay(string plain, ToPropertyInvocationInfo[] invocations)
        {
            var display = invocations.Length == 0 ? plain : invocations[0].ValueTypeDisplay;
            for (var i = 1; i < invocations.Length; i++)
            {
                if (!string.Equals(invocations[i].ValueTypeDisplay, display, StringComparison.Ordinal))
                {
                    return plain;
                }
            }

            return display;
        }
    }
}

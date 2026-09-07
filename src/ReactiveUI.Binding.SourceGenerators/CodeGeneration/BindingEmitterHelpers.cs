// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// The emission the four property-binding APIs share: grouping call sites, and the optional conversion and
/// scheduler parameters that trail every overload.
/// </summary>
/// <remarks>
/// <c>BindOneWay</c>, <c>BindTwoWay</c>, <c>OneWayBind</c> and <c>Bind</c> each emit a different signature, but
/// the grouping is the same and the trailing parameters differ only in what the conversion argument is called -
/// a converting binding names it <c>conversionFunc</c>, a projecting one names it <c>selector</c>. Passing that
/// name in is the whole of the difference, so the emission lives here once rather than four times.
/// </remarks>
internal static class BindingEmitterHelpers
{
    /// <summary>The name a view exposes its view model under.</summary>
    private const string ViewModelPropertyName = "ViewModel";

    /// <summary>The type the non-generic view interface declares the view model as, which names no view model.</summary>
    private const string WeaklyTypedViewModel = "object";

    /// <summary>Opens a delegate parameter, ready for the two type arguments and the parameter name.</summary>
    private const string FuncParameterPrefix = ", global::System.Func<";

    /// <summary>
    /// Emits a whole binding dispatch file: the extension class, one concrete overload per group of
    /// call sites, and one binding method per call site.
    /// </summary>
    /// <param name="invocations">The detected call sites for this API.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <param name="emitOverload">Emits the concrete typed overload for one group.</param>
    /// <param name="emitMethod">Emits the binding method for one call site.</param>
    /// <returns>The generated source, or null when there are no call sites.</returns>
    internal static string? Generate(
        ImmutableArray<BindingInvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features,
        Action<StringBuilder, BindingTypeGroup, LanguageFeatures> emitOverload,
        Action<StringBuilder, BindEmitContext> emitMethod)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return null;
        }

        var snapshot = features;
        var sb = PooledBuilder.Rent(invocations.Length * CodeGeneratorHelpers.PerInvocationBufferCapacity);
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb, snapshot);
        _ = sb.AppendLine();

        var groups = GroupByTypeSignature(invocations);

        for (var g = 0; g < groups.Count; g++)
        {
            var group = snapshot.SupportsCallerArgExpr
                ? groups[g] with
                {
                    Invocations = CodeGeneratorHelpers.CollapseIndistinguishableCallSites(
                        groups[g].Invocations,
                        static x => $"{x.SourceExpressionText}|{x.TargetExpressionText}"),
                }
                : groups[g];

            emitOverload(sb, group, snapshot);
            _ = sb.AppendLine();

            for (var i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                    inv.SourceTypeFullName,
                    inv.CallerFilePath,
                    inv.CallerLineNumber,
                    $"{inv.SourceExpressionText}|{inv.TargetExpressionText}");

                emitMethod(
                    sb,
                    new(
                        inv,
                        CodeGeneratorHelpers.FindClassInfo(allClasses, inv.SourceTypeFullName),
                        CodeGeneratorHelpers.FindClassInfo(allClasses, inv.TargetTypeFullName),
                        suffix,
                        snapshot));
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        _ = sb.AppendLine();

        return PooledBuilder.ToStringAndReturn(sb);
    }

    /// <summary>Emits the guard that lets a registered binding hook refuse this binding.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="sourceVar">The generated name of the source object.</param>
    /// <param name="targetVar">The generated name of the target object.</param>
    /// <param name="direction">The binding direction reported to the hook.</param>
    /// <param name="earlyReturn">What the generated method returns when a hook refuses.</param>
    /// <remarks>
    /// The <c>Any</c> test comes first so the two closures are only built once a hook is registered. The
    /// changes handed to a hook carry the bound objects with no expression, the shape ReactiveUI itself
    /// passes when it has no expression to walk, which cannot fault on a chain whose intermediate is null.
    /// </remarks>
    internal static void EmitBindingHookGuard(
        StringBuilder sb,
        string sourceVar,
        string targetVar,
        string direction,
        string earlyReturn) => _ = sb.Append("        if (").Append(GeneratedTypeNames.BindingHooks).AppendLine(".Any").Append("            && !")
            .Append(GeneratedTypeNames.BindingHooks).AppendLine(".ShouldBind(").Append("                ").Append(sourceVar).AppendLine(",")
            .Append("                ").Append(targetVar).AppendLine(",").Append("                () => new ")
            .Append(GeneratedTypeNames.IObservedChange).AppendLine("<object, object>[]").AppendLine("                {")
            .Append("                    new ").Append(GeneratedTypeNames.ObservedChange).Append("<object, object>(").Append(sourceVar)
            .Append(", null, ").Append(sourceVar).AppendLine("),").AppendLine("                },").Append("                () => new ")
            .Append(GeneratedTypeNames.IObservedChange).AppendLine("<object, object>[]").AppendLine("                {")
            .Append("                    new ").Append(GeneratedTypeNames.ObservedChange).Append("<object, object>(").Append(targetVar)
            .Append(", null, ").Append(targetVar).AppendLine("),").AppendLine("                },").Append("                ")
            .Append(GeneratedTypeNames.BindingDirection).Append('.').Append(direction).AppendLine("))").AppendLine("        {")
            .Append("            return ").Append(earlyReturn).AppendLine(";").AppendLine("        }");

    /// <summary>Emits the check that hands a binding to the runtime engine when a registered plugin outranks the generated one.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group, which fixes the bound types for the whole overload.</param>
    /// <param name="fallbackMethod">The <c>RuntimeBindingFallback</c> method that reproduces this binding.</param>
    /// <param name="arguments">The full argument list to forward, in the fallback method's own parameter order.</param>
    /// <param name="observesTarget">Whether the binding observes the target as well as the source.</param>
    /// <remarks>
    /// The generator picks each side's observation mechanism from the types it can see at compile time. A
    /// consumer that registers a higher-affinity <c>ICreatesObservableForProperty</c> expects it to apply to
    /// bindings as well as to <c>WhenChanged</c>, so the affinity is tested before dispatch and the binding
    /// routes to the runtime engine when the registration wins. A two-way binding observes both sides, so
    /// either side's registration is enough to take it.
    /// </remarks>
    internal static void EmitAffinityOverride(
        StringBuilder sb,
        BindingTypeGroup group,
        string fallbackMethod,
        string arguments,
        bool observesTarget)
    {
        var first = group.Invocations[0];

        _ = sb.AppendLine(
            "            // A registered plugin that outranks the generated one drives the binding instead");

        _ = sb.Append("            if (").Append(AffinityTest(group.SourceTypeFullName, first.SourcePropertyPath));

        if (observesTarget)
        {
            _ = sb.AppendLine().Append("                || ").Append(AffinityTest(group.TargetTypeFullName, first.TargetPropertyPath));
        }

        _ = sb.AppendLine(")").AppendLine("            {").Append("                return ").Append(GeneratedTypeNames.RuntimeBindingFallback)
            .Append('.').Append(fallbackMethod).AppendLine("(").Append("                    ").Append(arguments).AppendLine(");")
            .AppendLine("            }").AppendLine();
    }

    /// <summary>Groups call sites that can share one generated overload.</summary>
    /// <param name="invocations">The detected call sites.</param>
    /// <returns>The groups, in the order their signatures were first seen.</returns>
    internal static List<BindingTypeGroup> GroupByTypeSignature(ImmutableArray<BindingInvocationInfo> invocations)
    {
        var groupMap = new Dictionary<string, List<BindingInvocationInfo>>(invocations.Length);
        var keySb = new PooledStringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);

        for (var i = 0; i < invocations.Length; i++)
        {
            var inv = invocations[i];
            _ = keySb.Clear()
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

        keySb.Return();

        var result = new List<BindingTypeGroup>();
        foreach (var kvp in groupMap)
        {
            var first = kvp.Value[0];
            result.Add(new(
                first.SourceTypeFullName,
                first.TargetTypeFullName,
                first.SourcePropertyTypeFullName,
                first.TargetPropertyTypeFullName,
                first.HasConversion,
                first.HasScheduler,
                [.. kvp.Value]));
        }

        return result;
    }

    /// <summary>Appends the optional converters and scheduler for a two-way overload's parameter list.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="forwardName">What this API calls the source-to-target converter.</param>
    /// <param name="reverseName">What this API calls the target-to-source converter.</param>
    internal static void AppendTwoWayExtraParameters(
        StringBuilder sb,
        BindingTypeGroup group,
        string forwardName,
        string reverseName)
    {
        if (group.HasConversion)
        {
            _ = sb.Append(GeneratedSyntax.FuncParameterOpen).Append(group.SourcePropertyTypeFullName).Append(", ")
                .Append(group.TargetPropertyTypeFullName).Append("> ").Append(forwardName).AppendLine(",").Append(GeneratedSyntax.FuncParameterOpen)
                .Append(group.TargetPropertyTypeFullName).Append(", ").Append(group.SourcePropertyTypeFullName).Append("> ").Append(reverseName)
                .AppendLine(",");
        }

        if (!group.HasScheduler)
        {
            return;
        }

        _ = sb.Append("            ").Append(GeneratedTypeNames.ISequencer).AppendLine(" scheduler,");
    }

    /// <summary>Formats the extra arguments a two-way overload forwards to its generated method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <param name="forwardName">What this API calls the source-to-target converter.</param>
    /// <param name="reverseName">What this API calls the target-to-source converter.</param>
    /// <returns>The argument list fragment, or empty when there is nothing extra to forward.</returns>
    internal static string FormatTwoWayExtraArgs(BindingTypeGroup group, string forwardName, string reverseName)
    {
        if (!group.HasConversion && !group.HasScheduler)
        {
            return string.Empty;
        }

        var sb = new PooledStringBuilder();

        if (group.HasConversion)
        {
            _ = sb.Append(", ").Append(forwardName).Append(", ").Append(reverseName);
        }

        if (group.HasScheduler)
        {
            _ = sb.Append(", scheduler");
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>Formats the extra parameters a two-way generated method declares.</summary>
    /// <param name="inv">The call site, whose own flags decide what the method takes.</param>
    /// <param name="forwardName">What this API calls the source-to-target converter.</param>
    /// <param name="reverseName">What this API calls the target-to-source converter.</param>
    /// <returns>The parameter list fragment, or empty when there is nothing extra to declare.</returns>
    internal static string FormatTwoWayExtraMethodParams(
        BindingInvocationInfo inv,
        string forwardName,
        string reverseName)
    {
        if (!inv.HasConversion && !inv.HasScheduler)
        {
            return string.Empty;
        }

        var sb = new PooledStringBuilder();

        if (inv.HasConversion)
        {
            _ = sb.Append(FuncParameterPrefix)
                .Append(inv.SourcePropertyTypeFullName).Append(", ").Append(inv.TargetPropertyTypeFullName)
                .Append("> ").Append(forwardName)
                .Append(FuncParameterPrefix)
                .Append(inv.TargetPropertyTypeFullName).Append(", ").Append(inv.SourcePropertyTypeFullName)
                .Append("> ").Append(reverseName);
        }

        if (inv.HasScheduler)
        {
            _ = sb.Append(", ").Append(GeneratedTypeNames.ISequencer).Append(" scheduler");
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>Appends the optional conversion and scheduler parameters to an overload's parameter list.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="conversionParameterName">What this API calls its conversion argument.</param>
    internal static void AppendExtraParameters(StringBuilder sb, BindingTypeGroup group, string conversionParameterName)
    {
        if (group.HasConversion)
        {
            _ = sb.Append(GeneratedSyntax.FuncParameterOpen).Append(group.SourcePropertyTypeFullName).Append(", ")
                .Append(group.TargetPropertyTypeFullName).Append("> ").Append(conversionParameterName).AppendLine(",");
        }

        if (!group.HasScheduler)
        {
            return;
        }

        _ = sb.Append("            ").Append(GeneratedTypeNames.ISequencer).AppendLine(" scheduler,");
    }

    /// <summary>Formats the extra arguments for forwarding to the generated binding method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <param name="conversionParameterName">What this API calls its conversion argument.</param>
    /// <returns>The argument list fragment, or empty when there is nothing extra to forward.</returns>
    internal static string FormatExtraArgs(BindingTypeGroup group, string conversionParameterName)
    {
        if (!group.HasConversion && !group.HasScheduler)
        {
            return string.Empty;
        }

        var sb = new PooledStringBuilder();

        if (group.HasConversion)
        {
            _ = sb.Append(", ").Append(conversionParameterName);
        }

        if (group.HasScheduler)
        {
            _ = sb.Append(", scheduler");
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>Formats the extra parameters for the generated binding method's own signature.</summary>
    /// <param name="inv">The call site, whose own flags decide what the method takes.</param>
    /// <param name="conversionParameterName">What this API calls its conversion argument.</param>
    /// <returns>The parameter list fragment, or empty when there is nothing extra to declare.</returns>
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv, string conversionParameterName)
    {
        if (!inv.HasConversion && !inv.HasScheduler)
        {
            return string.Empty;
        }

        var sb = new PooledStringBuilder();

        if (inv.HasConversion)
        {
            _ = sb.Append(FuncParameterPrefix)
                .Append(inv.SourcePropertyTypeFullName)
                .Append(", ")
                .Append(inv.TargetPropertyTypeFullName)
                .Append("> ")
                .Append(conversionParameterName);
        }

        if (inv.HasScheduler)
        {
            _ = sb.Append(", ").Append(GeneratedTypeNames.ISequencer).Append(" scheduler");
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>Determines whether the two sides differ in type with no converter supplied to reconcile them.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns><see langword="true"/> when the conversion has to come from the registry.</returns>
    /// <remarks>
    /// A number bound to a text property is the archetypal binding, and the converter registry exists to serve
    /// it. Assigning straight across instead is a type error inside a generated file, where the consumer can
    /// neither see nor fix it.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool RequiresRegistryConversion(BindingInvocationInfo inv) =>
        !inv.HasConversion
        && !string.Equals(inv.SourcePropertyTypeFullName, inv.TargetPropertyTypeFullName, StringComparison.Ordinal);

    /// <summary>Determines whether the two sides of a whole group differ in type with no converter supplied.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns><see langword="true"/> when the conversion has to come from the registry.</returns>
    /// <remarks>
    /// The group fixes both property types, so the affinity override can ask this once for the overload rather
    /// than per call site.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool RequiresRegistryConversion(BindingTypeGroup group) =>
        !group.HasConversion
        && !string.Equals(
            group.SourcePropertyTypeFullName,
            group.TargetPropertyTypeFullName,
            StringComparison.Ordinal);

    /// <summary>Emits a stage that converts observed values to the type the other side declares.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="sourceVar">The variable holding the values to convert.</param>
    /// <param name="resultVar">The name to give the converted observable.</param>
    /// <param name="fromTypeFullName">The type the values arrive as.</param>
    /// <param name="toTypeFullName">The type the assignment needs.</param>
    /// <remarks>
    /// Converting once here rather than at the write keeps the binding's own change stream typed as the stub
    /// declares it, and pays for the conversion once per value rather than twice.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void EmitRegistryConversion(
        StringBuilder sb,
        string sourceVar,
        string resultVar,
        string fromTypeFullName,
        string toTypeFullName) =>
        sb.Append("                var ").Append(resultVar).Append(" = new ").Append(GeneratedTypeNames.MapSignal).Append('<')
            .Append(fromTypeFullName).Append(", ").Append(toTypeFullName).AppendLine(">(").Append("                    ").Append(sourceVar)
            .AppendLine(",").AppendLine("                    __value =>").AppendLine("                    {").Append("                        ")
            .Append(toTypeFullName).AppendLine(" __converted;").Append("                        ").Append(GeneratedTypeNames.RuntimeBindingConverter)
            .Append(".TryConvert<").Append(fromTypeFullName).Append(", ").Append(toTypeFullName)
            .AppendLine(">(__value, null, null, out __converted);").AppendLine("                        return __converted;")
            .AppendLine("                    });");

    /// <summary>Emits the stage that delivers a write to the view on the thread the view belongs to.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="sourceVar">The variable holding the values being written to the view.</param>
    /// <param name="resultVar">The name to give the routed observable.</param>
    /// <returns>The variable to subscribe the write to.</returns>
    /// <remarks>
    /// A view model raises its notifications from whatever thread did the work, and the UI frameworks only allow
    /// a view to be touched from the thread that owns it. Where a call site named its own scheduler the caller
    /// has already said where the write lands, so this stays out of the way; otherwise the routing is decided at
    /// runtime by whichever platform package is present, which is the only place that can know.
    /// </remarks>
    internal static string EmitViewThreadStage(
        StringBuilder sb,
        BindingInvocationInfo inv,
        string sourceVar,
        string resultVar)
    {
        if (inv.HasScheduler)
        {
            return sourceVar;
        }

        _ = sb.Append("            var ").Append(resultVar).Append(" = ").Append(GeneratedTypeNames.BindingSchedulers).Append(".ObserveOnMainThread(")
            .Append(sourceVar).AppendLine(");");

        return resultVar;
    }

    /// <summary>Decides what a view-first binding observes: the view model it was handed, or the view's own.</summary>
    /// <param name="inv">The call site being emitted.</param>
    /// <param name="sourceClassInfo">The view model type's binding info, when it was detected.</param>
    /// <param name="targetClassInfo">The view type's binding info, which lists the properties it declares.</param>
    /// <returns>The root to observe from, the path to walk, and the root type's binding info.</returns>
    /// <remarks>
    /// A view that exposes the view model is observed through that property, so the binding follows whichever
    /// view model the view currently holds. Setting the view model after construction and replacing it later is
    /// the ordinary view lifecycle, and a binding that captured the instance it was handed goes on driving the
    /// view from the replaced one with nothing raised to say so.
    /// <para>
    /// The rewritten path is the same shape the chain emitters already handle - one more segment, whose parent
    /// may be null - so the switch onto the current view model, and the detach from the previous one, come from
    /// the machinery a deep chain already uses.
    /// </para>
    /// </remarks>
    internal static ViewModelObservation ResolveViewModelObservation(
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        ClassBindingInfo? targetClassInfo)
    {
        var declaredType = targetClassInfo is null ? null : FindViewModelPropertyType(targetClassInfo);
        if (declaredType is null)
        {
            return new("viewModel", inv.SourcePropertyPath, sourceClassInfo);
        }

        // The stage below is typed as the view model the call site named, so a view exposing it as a base or
        // an interface has to narrow on the way out. Observables convert the other way, so the read does it.
        var readCast = string.Equals(declaredType, inv.SourceTypeFullName, StringComparison.Ordinal)
            ? null
            : inv.SourceTypeFullName;

        var viewModelSegment = new PropertyPathSegment(
            ViewModelPropertyName,
            inv.SourceTypeFullName,
            inv.TargetTypeFullName,
            true,
            sourceClassInfo,
            readCast);

        var source = inv.SourcePropertyPath;
        var rooted = new PropertyPathSegment[source.Length + 1];
        rooted[0] = viewModelSegment;
        for (var i = 0; i < source.Length; i++)
        {
            rooted[i + 1] = source[i];
        }

        return new("view", new(rooted), targetClassInfo);
    }

    /// <summary>Emits the whole concrete typed overload one binding API dispatches through.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group, which fixes the bound types for the whole overload.</param>
    /// <param name="api">What distinguishes this API's overload from the other three.</param>
    /// <param name="dispatchesOnExpressionText">Whether dispatch keys on expression text rather than file and line.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    /// <remarks>
    /// The four property-binding APIs emit one overload shape between them: a receiver, the object it binds
    /// against, a selector per side, whatever conversion and scheduler arguments the API takes, and then the
    /// dispatch table. What actually differs is the naming and the trailing arguments, which
    /// <paramref name="api"/> carries, so the shape is written here once.
    /// </remarks>
    internal static void GenerateDispatchOverload(
        StringBuilder sb,
        BindingTypeGroup group,
        BindingDispatchApi api,
        bool dispatchesOnExpressionText,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        var first = group.Invocations[0];
        var sourceLeaf = CodeGeneratorHelpers.NullableSelectorLeafType(first.SourcePropertyPath, supportsNullable);
        var targetLeaf = CodeGeneratorHelpers.NullableSelectorLeafType(first.TargetPropertyPath, supportsNullable);

        CodeGeneratorHelpers.AppendDispatchSummary(
            sb,
            api.Name,
            group.SourceTypeFullName,
            group.TargetTypeFullName,
            dispatchesOnExpressionText);

        _ = sb.Append("        public static ").Append(api.FormatReturnType(group)).Append(' ').Append(api.Name).AppendLine("(")
            .Append("            this ").Append(api.ReceiverIsTarget ? group.TargetTypeFullName : group.SourceTypeFullName)
            .Append(' ').Append(api.ReceiverParameterName).AppendLine(",")
            .Append(CodeGeneratorHelpers.ParameterIndent)
            .Append(api.ReceiverIsTarget ? group.SourceTypeFullName : group.TargetTypeFullName)
            .Append(' ').Append(api.OtherParameterName).AppendLine(",")
            .Append(GeneratedSyntax.SelectorParameterOpen).Append(group.SourceTypeFullName).Append(", ").Append(sourceLeaf)
            .Append(">> ").Append(api.SourceSelectorName).AppendLine(",")
            .Append(GeneratedSyntax.SelectorParameterOpen).Append(group.TargetTypeFullName).Append(", ").Append(targetLeaf)
            .Append(">> ").Append(api.TargetSelectorName).AppendLine(",");

        api.AppendExtraParameters(sb, group);

        if (dispatchesOnExpressionText)
        {
            AppendExpressionDispatchBody(sb, group, api);
        }
        else
        {
            AppendCallerInfoDispatchBody(sb, group, api, stubHasExpressionParameters);
        }

        CodeGeneratorHelpers.AppendBindingDispatchFallthrough(sb);
    }

    /// <summary>Emits the head of a generated worker: its signature, the path it binds, and the hook guard.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="api">What distinguishes this API's worker from the other three.</param>
    /// <param name="inv">The call site being emitted.</param>
    /// <param name="suffix">The stable method-name suffix for this call site.</param>
    /// <remarks>
    /// Every binding worker opens the same way - it takes the two objects and whatever conversion and scheduler
    /// arguments its API declares, records the path it binds, and offers the binding to a registered hook. What
    /// varies is the naming and the direction, which <paramref name="api"/> carries.
    /// </remarks>
    internal static void AppendWorkerMethodHeader(
        StringBuilder sb,
        BindingDispatchApi api,
        BindingInvocationInfo inv,
        string suffix)
    {
        _ = sb.Append("        private static ").Append(api.FormatWorkerReturnType(inv)).Append(' ').Append(api.WorkerMethodPrefix).Append(suffix)
            .Append('(').Append(inv.SourceTypeFullName).Append(' ').Append(api.WorkerSourceParameterName).Append(", ")
            .Append(inv.TargetTypeFullName).Append(' ').Append(api.WorkerTargetParameterName).Append(api.FormatWorkerParameters(inv))
            .AppendLine(")").AppendLine(GeneratedSyntax.MemberBodyOpen)
            .Append("            // ").Append(api.Name).Append(": ").Append(CodeGeneratorHelpers.BuildPropertyPathString(inv.SourcePropertyPath))
            .Append(api.IsTwoWay ? " <-> " : " -> ").Append(CodeGeneratorHelpers.BuildPropertyPathString(inv.TargetPropertyPath))
            .Append(inv.HasConversion ? " (with conversion)" : string.Empty)
            .Append(inv.HasScheduler ? " (with scheduler)" : string.Empty).AppendLine();

        EmitBindingHookGuard(
            sb,
            api.WorkerSourceParameterName,
            api.WorkerTargetParameterName,
            api.IsTwoWay ? "TwoWay" : "OneWay",
            api.HookRefusalValue);
    }

    /// <summary>Emits the conversion and scheduler stages a one-directional binding puts its values through.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="api">What this API calls the locals along the way.</param>
    /// <param name="inv">The call site being emitted.</param>
    /// <returns>The local the subscription should read from.</returns>
    internal static string EmitSingleStreamStages(StringBuilder sb, BindingDispatchApi api, BindingInvocationInfo inv)
    {
        var forward = ForwardStage(api, inv);
        var currentVar = forward.ObservableName;

        if (inv.HasConversion)
        {
            currentVar = AppendMapStage(sb, forward, currentVar, inv.HasScheduler);
        }

        if (inv.HasScheduler)
        {
            currentVar = AppendObserveOnStage(sb, forward, currentVar);
        }

        return currentVar;
    }

    /// <summary>Emits those same stages for a binding that drives both sides.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="api">What this API calls the locals along the way.</param>
    /// <param name="inv">The call site being emitted.</param>
    /// <returns>The locals each direction's subscription should read from.</returns>
    /// <remarks>
    /// Each direction assigns across the same type gap in the opposite sense, so the two run the same stages
    /// with their type arguments and converter swapped.
    /// </remarks>
    internal static BindingObservables EmitDualStreamStages(StringBuilder sb, BindingDispatchApi api, BindingInvocationInfo inv)
    {
        var forward = ForwardStage(api, inv);
        var reverse = ReverseStage(api, inv);
        var sourceVar = forward.ObservableName;
        var targetVar = reverse.ObservableName;

        if (inv.HasConversion)
        {
            sourceVar = AppendMapStage(sb, forward, sourceVar, inv.HasScheduler);
            targetVar = AppendMapStage(sb, reverse, targetVar, inv.HasScheduler);
        }

        if (inv.HasScheduler)
        {
            sourceVar = AppendObserveOnStage(sb, forward, sourceVar);
            targetVar = AppendObserveOnStage(sb, reverse, targetVar);
        }

        return new(sourceVar, targetVar);
    }

    /// <summary>Finds the type a view declares its view model property as.</summary>
    /// <param name="targetClassInfo">The view type's binding info.</param>
    /// <returns>The declared property type, or <see langword="null"/> when the view exposes no such property.</returns>
    /// <remarks>
    /// A view implementing <c>IViewFor&lt;T&gt;</c> declares the property twice - once weakly typed for the
    /// non-generic interface, once as the view model itself. Only the typed declaration counts. The weak one
    /// says nothing about which view model the view holds, and following it would point the binding at a
    /// property the view may never have been given, where the call site handed the view model over directly.
    /// </remarks>
    private static string? FindViewModelPropertyType(ClassBindingInfo targetClassInfo)
    {
        var properties = targetClassInfo.Properties;

        for (var i = 0; i < properties.Length; i++)
        {
            if (!string.Equals(properties[i].PropertyName, ViewModelPropertyName, StringComparison.Ordinal))
            {
                continue;
            }

            var declaredType = properties[i].PropertyTypeFullName;
            if (!string.Equals(declaredType, WeaklyTypedViewModel, StringComparison.Ordinal))
            {
                return declaredType;
            }
        }

        return null;
    }

    /// <summary>Renders the affinity test for one side of a binding.</summary>
    /// <param name="typeFullName">The fully qualified name of the observed type.</param>
    /// <param name="propertyPath">The path being observed, each segment carrying how its declaring type notifies.</param>
    /// <returns>The rendered condition, without surrounding parentheses.</returns>
    /// <remarks>
    /// One test per link, because that is how the registration is resolved: a plugin scores a type and a
    /// property together, so a chain can pick a different mechanism at every step and a registration that wins
    /// at any one of them takes the whole binding. Asking about the root alone misses a registration aimed at
    /// the leaf, and asking without the property name makes every mechanism-specific plugin score 0.
    /// </remarks>
    private static string AffinityTest(string typeFullName, EquatableArray<PropertyPathSegment> propertyPath)
    {
        var builder = new StringBuilder();

        for (var i = 0; i < propertyPath.Length; i++)
        {
            var segment = propertyPath[i];
            var declaringType = segment.DeclaringTypeInfo;
            var plugin = declaringType is null
                ? null
                : Plugins.ObservationPluginRegistry.GetBestPlugin(declaringType, segment.PropertyName);

            // The root is the type the call site binds, which is what the observation is rooted on; every
            // later link is observed on the type that declares it.
            var observedType = i == 0 ? typeFullName : segment.DeclaringTypeFullName;

            if (i > 0)
            {
                _ = builder.AppendLine().Append("                || ");
            }

            _ = builder
                .Append(GeneratedTypeNames.ObservationAffinityChecker)
                .Append(".HasHigherAffinityPlugin(typeof(")
                .Append(observedType)
                .Append("), \"")
                .Append(segment.PropertyName)
                .Append("\", ")
                .Append(plugin?.Affinity ?? 0)
                .Append(", false)");
        }

        return builder.ToString();
    }

    /// <summary>Appends the body of an overload that matches a call site by the text of its selectors.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="api">What distinguishes this API's overload from the other three.</param>
    private static void AppendExpressionDispatchBody(StringBuilder sb, BindingTypeGroup group, BindingDispatchApi api)
    {
        CodeGeneratorHelpers.AppendExpressionDispatchParameters(sb, api.SourceSelectorName, api.TargetSelectorName);

        if (api.NormalizesStaticPrefix)
        {
            CodeGeneratorHelpers.AppendStaticPrefixNormalization(sb, api.SourceExpressionParameter);
            CodeGeneratorHelpers.AppendStaticPrefixNormalization(sb, api.TargetExpressionParameter);
            _ = sb.AppendLine();
        }

        api.EmitAffinityOverride(sb, group, api.TargetExpressionParameter);
        var extraArguments = api.FormatExtraArguments(group);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];

            CodeGeneratorHelpers.AppendExpressionDispatchCondition(
                sb,
                CodeGeneratorHelpers.ConditionKeyword(i),
                api.SourceExpressionParameter,
                inv.SourceExpressionText,
                api.TargetExpressionParameter,
                inv.TargetExpressionText);
            CodeGeneratorHelpers.AppendDispatchReturn(
                sb,
                api.WorkerMethodPrefix + BindingMethodSuffix(inv),
                api.WorkerArguments + extraArguments);
        }
    }

    /// <summary>Appends the body of an overload that matches a call site by the file and line it sits on.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="api">What distinguishes this API's overload from the other three.</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    private static void AppendCallerInfoDispatchBody(
        StringBuilder sb,
        BindingTypeGroup group,
        BindingDispatchApi api,
        bool stubHasExpressionParameters)
    {
        if (stubHasExpressionParameters)
        {
            CodeGeneratorHelpers.AppendExpressionParameter(sb, api.SourceSelectorName, api.SourceExpressionParameter, false);
            CodeGeneratorHelpers.AppendExpressionParameter(sb, api.TargetSelectorName, api.TargetExpressionParameter, false);
        }

        CodeGeneratorHelpers.AppendCallerInfoDispatchParameters(sb);

        api.EmitAffinityOverride(
            sb,
            group,
            $"\"{CodeGeneratorHelpers.EscapeString(group.Invocations[0].TargetExpressionText)}\"");
        var extraArguments = api.FormatExtraArguments(group);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];

            CodeGeneratorHelpers.AppendCallerInfoDispatchCondition(
                sb,
                CodeGeneratorHelpers.ConditionKeyword(i),
                inv.CallerLineNumber,
                CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath));
            CodeGeneratorHelpers.AppendDispatchReturn(
                sb,
                api.WorkerMethodPrefix + BindingMethodSuffix(inv),
                api.WorkerArguments + extraArguments);
        }
    }

    /// <summary>Describes the direction that carries the source side's values to the target side.</summary>
    /// <param name="api">What this API calls the locals along the way.</param>
    /// <param name="inv">The call site being emitted.</param>
    /// <returns>The stage description.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BindingStreamStage ForwardStage(BindingDispatchApi api, BindingInvocationInfo inv) =>
        new(
            api.SourceObservableName,
            api.SourceConvertedName,
            api.SourceScheduledName,
            api.ForwardConverterArgument,
            inv.SourcePropertyTypeFullName,
            inv.TargetPropertyTypeFullName);

    /// <summary>Describes the direction that carries the target side's values back to the source side.</summary>
    /// <param name="api">What this API calls the locals along the way.</param>
    /// <param name="inv">The call site being emitted.</param>
    /// <returns>The stage description.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BindingStreamStage ReverseStage(BindingDispatchApi api, BindingInvocationInfo inv) =>
        new(
            api.TargetObservableName,
            api.TargetConvertedName,
            api.TargetScheduledName,
            api.ReverseConverterArgument,
            inv.TargetPropertyTypeFullName,
            inv.SourcePropertyTypeFullName);

    /// <summary>Appends the stage that runs one direction's values through the converter the call site named.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="stage">The direction being emitted.</param>
    /// <param name="sourceVar">The local holding the values to convert.</param>
    /// <param name="hasScheduler">Whether a scheduler stage follows, which decides what the result is called.</param>
    /// <returns>The local holding the converted values.</returns>
    private static string AppendMapStage(StringBuilder sb, in BindingStreamStage stage, string sourceVar, bool hasScheduler)
    {
        var resultVar = hasScheduler ? stage.ConvertedName : stage.ScheduledName;

        _ = sb.Append("        var ").Append(resultVar).Append(" = new ").Append(GeneratedTypeNames.MapSignal).Append('<')
            .Append(stage.FromTypeFullName).Append(", ").Append(stage.ToTypeFullName).Append(">(").Append(sourceVar)
            .Append(", ").Append(stage.ConverterArgument).AppendLine(");");

        return resultVar;
    }

    /// <summary>Appends the stage that delivers one direction's values on the scheduler the call site named.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="stage">The direction being emitted.</param>
    /// <param name="sourceVar">The local holding the values to route.</param>
    /// <returns>The local holding the routed values.</returns>
    private static string AppendObserveOnStage(StringBuilder sb, in BindingStreamStage stage, string sourceVar)
    {
        _ = sb.Append("        var ").Append(stage.ScheduledName).Append(" = ").Append(GeneratedTypeNames.LinqExtensions)
            .Append(".ObserveOn<").Append(stage.ToTypeFullName).Append(">(").Append(sourceVar).AppendLine(", scheduler);");

        return stage.ScheduledName;
    }

    /// <summary>Names the generated worker a binding call site dispatches to.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The stable suffix its worker is named with.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string BindingMethodSuffix(BindingInvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.SourceTypeFullName,
            inv.CallerFilePath,
            inv.CallerLineNumber,
            $"{inv.SourceExpressionText}|{inv.TargetExpressionText}");

    /// <summary>What a view-first binding observes, and from where.</summary>
    /// <param name="RootVariable">The generated method parameter the observation is rooted on.</param>
    /// <param name="Path">The property path walked from that root.</param>
    /// <param name="RootClassInfo">The root type's binding info, when it was detected.</param>
    internal readonly record struct ViewModelObservation(
        string RootVariable,
        EquatableArray<PropertyPathSegment> Path,
        ClassBindingInfo? RootClassInfo);

    /// <summary>One direction of a binding's value flow, and what the generated locals along it are called.</summary>
    /// <param name="ObservableName">The local the direction's observation is held in.</param>
    /// <param name="ConvertedName">The local the converted values are held in, ahead of a scheduler stage.</param>
    /// <param name="ScheduledName">The local the values are held in once the subscription can read them.</param>
    /// <param name="ConverterArgument">What the generated worker calls this direction's converter.</param>
    /// <param name="FromTypeFullName">The type the values arrive as.</param>
    /// <param name="ToTypeFullName">The type the assignment at the far end needs.</param>
    internal readonly record struct BindingStreamStage(
        string ObservableName,
        string ConvertedName,
        string ScheduledName,
        string ConverterArgument,
        string FromTypeFullName,
        string ToTypeFullName);

    /// <summary>Everything a per-call-site binding emitter needs about one resolved call site.</summary>
    /// <param name="Invocation">The call site being emitted.</param>
    /// <param name="SourceClassInfo">The source type's binding info, when it was detected.</param>
    /// <param name="TargetClassInfo">The target type's binding info, when it was detected.</param>
    /// <param name="Suffix">The stable method-name suffix for this call site.</param>
    /// <param name="Features">The consumer compilation's language-feature snapshot.</param>
    internal readonly record struct BindEmitContext(
        BindingInvocationInfo Invocation,
        ClassBindingInfo? SourceClassInfo,
        ClassBindingInfo? TargetClassInfo,
        string Suffix,
        LanguageFeatures Features);

    /// <summary>What distinguishes one binding API's generated dispatch overload from another's.</summary>
    /// <remarks>
    /// Held as an object rather than passed as arguments because the set travels together and would otherwise
    /// widen the emitter's signature past what any one call site can read.
    /// </remarks>
    internal sealed class BindingDispatchApi
    {
        /// <summary>Gets the name of the binding method this overload stands in for.</summary>
        internal string Name { get; init; } = string.Empty;

        /// <summary>Gets what the overload calls the object it extends.</summary>
        internal string ReceiverParameterName { get; init; } = string.Empty;

        /// <summary>Gets what the overload calls the object bound against the receiver.</summary>
        internal string OtherParameterName { get; init; } = string.Empty;

        /// <summary>Gets a value indicating whether the receiver is the side the binding writes to.</summary>
        internal bool ReceiverIsTarget { get; init; }

        /// <summary>Gets what the overload calls the selector for the side it reads from.</summary>
        internal string SourceSelectorName { get; init; } = string.Empty;

        /// <summary>Gets what the overload calls the selector for the side it writes to.</summary>
        internal string TargetSelectorName { get; init; } = string.Empty;

        /// <summary>Gets the prefix the generated worker for each call site is named with.</summary>
        internal string WorkerMethodPrefix { get; init; } = string.Empty;

        /// <summary>Gets what a generated worker calls the object it reads from.</summary>
        internal string WorkerSourceParameterName { get; init; } = string.Empty;

        /// <summary>Gets what a generated worker calls the object it writes to.</summary>
        internal string WorkerTargetParameterName { get; init; } = string.Empty;

        /// <summary>Gets a value indicating whether the binding drives both sides.</summary>
        internal bool IsTwoWay { get; init; }

        /// <summary>Gets what a generated worker returns when a registered hook refuses the binding.</summary>
        internal string HookRefusalValue { get; init; } = string.Empty;

        /// <summary>Gets the local a generated worker holds the source side's observation in.</summary>
        internal string SourceObservableName { get; init; } = string.Empty;

        /// <summary>Gets the local a generated worker holds the target side's observation in.</summary>
        internal string TargetObservableName { get; init; } = string.Empty;

        /// <summary>Gets the local the source side's converted values are held in, ahead of a scheduler stage.</summary>
        internal string SourceConvertedName { get; init; } = string.Empty;

        /// <summary>Gets the local the target side's converted values are held in, ahead of a scheduler stage.</summary>
        internal string TargetConvertedName { get; init; } = string.Empty;

        /// <summary>Gets the local the source side's values are held in once the subscription can read them.</summary>
        internal string SourceScheduledName { get; init; } = string.Empty;

        /// <summary>Gets the local the target side's values are held in once the subscription can read them.</summary>
        internal string TargetScheduledName { get; init; } = string.Empty;

        /// <summary>Gets what a generated worker calls the converter from the source side's type to the target's.</summary>
        internal string ForwardConverterArgument { get; init; } = string.Empty;

        /// <summary>Gets what a generated worker calls the converter from the target side's type to the source's.</summary>
        internal string ReverseConverterArgument { get; init; } = string.Empty;

        /// <summary>Gets a value indicating whether the overload strips a <c>static</c> prefix off captured expressions.</summary>
        internal bool NormalizesStaticPrefix { get; init; }

        /// <summary>Gets the function rendering what the overload returns.</summary>
        internal Func<BindingTypeGroup, string> FormatReturnType { get; init; } =
            static _ => "global::System.IDisposable";

        /// <summary>Gets the function rendering what a generated worker returns.</summary>
        internal Func<BindingInvocationInfo, string> FormatWorkerReturnType { get; init; } =
            static _ => "global::System.IDisposable";

        /// <summary>Gets the function rendering the conversion and scheduler parameters a generated worker declares.</summary>
        internal Func<BindingInvocationInfo, string> FormatWorkerParameters { get; init; } =
            static _ => string.Empty;

        /// <summary>Gets the action appending the conversion and scheduler parameters this API takes.</summary>
        internal Action<StringBuilder, BindingTypeGroup> AppendExtraParameters { get; init; } =
            static (_, _) => { };

        /// <summary>Gets the function rendering the conversion and scheduler arguments the worker takes.</summary>
        internal Func<BindingTypeGroup, string> FormatExtraArguments { get; init; } =
            static _ => string.Empty;

        /// <summary>Gets the action emitting the check that hands the binding to the runtime engine.</summary>
        internal Action<StringBuilder, BindingTypeGroup, string> EmitAffinityOverride { get; init; } =
            static (_, _, _) => { };

        /// <summary>Gets the two objects a generated worker binds, in its own parameter order.</summary>
        internal string WorkerArguments => $"{WorkerSourceParameterName}, {WorkerTargetParameterName}";

        /// <summary>Gets the parameter carrying the text of the selector for the side read from.</summary>
        internal string SourceExpressionParameter => SourceSelectorName + CodeGeneratorHelpers.ExpressionParameterSuffix;

        /// <summary>Gets the parameter carrying the text of the selector for the side written to.</summary>
        internal string TargetExpressionParameter => TargetSelectorName + CodeGeneratorHelpers.ExpressionParameterSuffix;
    }
}

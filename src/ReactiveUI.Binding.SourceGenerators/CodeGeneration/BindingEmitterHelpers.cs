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

    /// <summary>What a view-first binding observes, and from where.</summary>
    /// <param name="RootVariable">The generated method parameter the observation is rooted on.</param>
    /// <param name="Path">The property path walked from that root.</param>
    /// <param name="RootClassInfo">The root type's binding info, when it was detected.</param>
    internal readonly record struct ViewModelObservation(
        string RootVariable,
        EquatableArray<PropertyPathSegment> Path,
        ClassBindingInfo? RootClassInfo);

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
}

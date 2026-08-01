// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
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
    /// <summary>Opens a delegate parameter, ready for the two type arguments and the parameter name.</summary>
    private const string FuncParameterPrefix = ", global::System.Func<";

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
            _ = sb.AppendLine($"""
                                       global::System.Func<{group.SourcePropertyTypeFullName}, {group.TargetPropertyTypeFullName}> {forwardName},
                                       global::System.Func<{group.TargetPropertyTypeFullName}, {group.SourcePropertyTypeFullName}> {reverseName},
                           """);
        }

        if (!group.HasScheduler)
        {
            return;
        }

        _ = sb.AppendLine($"            {GeneratedTypeNames.ISequencer} scheduler,");
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
            _ = sb.AppendLine(
                $"            global::System.Func<{group.SourcePropertyTypeFullName}, {group.TargetPropertyTypeFullName}> {conversionParameterName},");
        }

        if (!group.HasScheduler)
        {
            return;
        }

        _ = sb.AppendLine($"            {GeneratedTypeNames.ISequencer} scheduler,");
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
}

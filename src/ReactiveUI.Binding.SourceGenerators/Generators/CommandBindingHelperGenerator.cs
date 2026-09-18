// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;

namespace ReactiveUI.Binding.SourceGenerators.Generators;

/// <summary>Emits native command bridges only for selected call-site mechanisms.</summary>
internal static class CommandBindingHelperGenerator
{
    /// <summary>Reserves enough space for a native selector bridge.</summary>
    private const int HelperCapacity = 4096;

    /// <summary>Shares each selected plugin's bridge across the consumer's command bindings.</summary>
    /// <param name="context">The output context.</param>
    /// <param name="invocations">The command call sites.</param>
    /// <param name="features">The consumer's language and runtime options.</param>
    internal static void Generate(in SourceProductionContext context, ImmutableArray<BindCommandInvocationInfo> invocations, in LanguageFeatures features)
    {
        HashSet<ICommandBindingHelperPlugin>? selected = null;
        foreach (var invocation in invocations)
        {
            if (CommandBindingPluginRegistry.GetBestPlugin(invocation) is not ICommandBindingHelperPlugin helper)
            {
                continue;
            }

            selected ??= [];
            _ = selected.Add(helper);
        }

        if (selected is null)
        {
            return;
        }

        var sb = PooledBuilder.Rent(HelperCapacity);
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb, features);
        foreach (var helper in selected)
        {
            helper.EmitHelper(sb);
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        CodeGeneratorHelpers.AddGeneratedSource(context, "CommandBindingHelpers.g.cs", PooledBuilder.ToStringAndReturn(sb), features);
    }
}

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

namespace ReactiveUI.Binding.SourceGenerators.Generators;

/// <summary>Declares the invoker classes generated bindings carry, once per compilation.</summary>
internal static class ViewThreadInvokerGenerator
{
    /// <summary>The generated file the invoker classes are declared in.</summary>
    private const string HintName = "ViewThreadInvokers.g.cs";

    /// <summary>Buffer capacity to reserve per invoker class.</summary>
    private const int PerInvokerBufferCapacity = 1_536;

    /// <summary>Declares the named invoker classes.</summary>
    /// <param name="context">The source production context.</param>
    /// <param name="invokerTypeNames">The invoker class names whose platform the compilation references.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    internal static void Generate(
        in SourceProductionContext context,
        EquatableArray<string> invokerTypeNames,
        in LanguageFeatures features)
    {
        if (invokerTypeNames.Length == 0)
        {
            return;
        }

        var sb = PooledBuilder.Rent(invokerTypeNames.Length * PerInvokerBufferCapacity);
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb, features);
        ViewThreadPluginRegistry.EmitInvokers(sb, invokerTypeNames, features.SupportsNullable);
        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        _ = sb.AppendLine();

        CodeGeneratorHelpers.AddGeneratedSource(context, HintName, PooledBuilder.ToStringAndReturn(sb), features);
    }
}

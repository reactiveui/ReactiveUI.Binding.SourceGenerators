// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Invocations;

/// <summary>Wires one API's detected invocations to the emitter that writes its dispatch file.</summary>
/// <remarks>
/// Every API is registered the same way: collect its invocations, pair them with the types the compilation
/// declares and the consumer's language features, and write one file when the emitter produces anything. Only
/// the emitter and the file name differ, so they are the arguments and the pipeline is written once.
/// </remarks>
internal static class InvocationPipeline
{
    /// <summary>Registers a pipeline whose emitter needs the types the compilation declares.</summary>
    /// <typeparam name="TInvocation">The per-call-site model this API extracts.</typeparam>
    /// <param name="context">The generator initialization context.</param>
    /// <param name="invocations">The detected invocations of this API.</param>
    /// <param name="allClasses">The shared type detection pipeline.</param>
    /// <param name="languageFeatures">The consumer compilation's C# language-feature snapshot.</param>
    /// <param name="hintName">The name of the file the emitter's output is added as.</param>
    /// <param name="emit">Produces the file's text, or null when this API claimed no call site.</param>
    internal static void Register<TInvocation>(
        in IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<TInvocation> invocations,
        IncrementalValuesProvider<ClassBindingInfo> allClasses,
        IncrementalValueProvider<LanguageFeatures> languageFeatures,
        string hintName,
        Func<ImmutableArray<TInvocation>, ImmutableArray<ClassBindingInfo>, LanguageFeatures, string?> emit)
    {
        var combined = invocations.Collect()
            .Combine(allClasses.Collect())
            .Combine(languageFeatures);

        context.RegisterSourceOutput(
            combined,
            (ctx, data) =>
            {
                var source = emit(data.Left.Left, data.Left.Right, data.Right);
                if (source is null)
                {
                    return;
                }

                CodeGeneratorHelpers.AddGeneratedSource(ctx, hintName, source, data.Right);
            });
    }

    /// <summary>Registers a pipeline whose emitter reads only the call sites.</summary>
    /// <typeparam name="TInvocation">The per-call-site model this API extracts.</typeparam>
    /// <param name="context">The generator initialization context.</param>
    /// <param name="invocations">The detected invocations of this API.</param>
    /// <param name="languageFeatures">The consumer compilation's C# language-feature snapshot.</param>
    /// <param name="hintName">The name of the file the emitter's output is added as.</param>
    /// <param name="emit">Produces the file's text, or null when this API claimed no call site.</param>
    /// <remarks>
    /// An API that writes to a target it was handed, rather than to a property of an observed type, needs no
    /// notification mechanism and so never asks what the compilation declares.
    /// </remarks>
    internal static void Register<TInvocation>(
        in IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<TInvocation> invocations,
        IncrementalValueProvider<LanguageFeatures> languageFeatures,
        string hintName,
        Func<ImmutableArray<TInvocation>, LanguageFeatures, string?> emit)
    {
        var combined = invocations.Collect().Combine(languageFeatures);

        context.RegisterSourceOutput(
            combined,
            (ctx, data) =>
            {
                var source = emit(data.Left, data.Right);
                if (source is null)
                {
                    return;
                }

                CodeGeneratorHelpers.AddGeneratedSource(ctx, hintName, source, data.Right);
            });
    }
}

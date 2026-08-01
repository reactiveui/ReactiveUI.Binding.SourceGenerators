// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Invocations;

/// <summary>
/// Detects WhenChanging invocations (before-change) and generates per-invocation observation code.
/// Skips generation for types whose notification mechanism doesn't support before-change.
/// </summary>
internal static class WhenChangingInvocationGenerator
{
    /// <summary>Registers the WhenChanging invocation detection pipeline.</summary>
    /// <param name="context">The generator initialization context.</param>
    /// <param name="invocations">The detected invocations of this API.</param>
    /// <param name="allClasses">The shared type detection pipeline.</param>
    /// <param name="languageFeatures">The consumer compilation's C# language-feature snapshot.</param>
    internal static void Register(
        in IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<InvocationInfo> invocations,
        IncrementalValuesProvider<ClassBindingInfo> allClasses,
        IncrementalValueProvider<LanguageFeatures> languageFeatures)
    {
        var combined = invocations.Collect()
            .Combine(allClasses.Collect())
            .Combine(languageFeatures);

        context.RegisterSourceOutput(
            combined,
            static (ctx, data) =>
            {
                var source =
                    ObservationCodeGenerator.Generate(data.Left.Left, data.Left.Right, data.Right, "WhenChanging");
                if (source is null)
                {
                    return;
                }

                CodeGeneration.CodeGeneratorHelpers.AddGeneratedSource(ctx, "WhenChangingDispatch.g.cs", source, data.Right);
            });
    }
}

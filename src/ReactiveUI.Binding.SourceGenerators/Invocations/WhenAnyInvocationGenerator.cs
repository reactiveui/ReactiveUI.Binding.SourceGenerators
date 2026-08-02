// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Invocations;

/// <summary>
/// Detects WhenAny invocations (with IObservedChange selector) and generates per-invocation observation code.
/// Unlike WhenChanged/WhenAnyValue, WhenAny wraps each observed value in an <c>ObservedChange</c> before
/// passing it to the user's selector function.
/// </summary>
internal static class WhenAnyInvocationGenerator
{
    /// <summary>Registers the WhenAny invocation detection pipeline.</summary>
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
                var source = WhenAnyCodeGenerator.Generate(data.Left.Left, data.Left.Right, data.Right);
                if (source is null)
                {
                    return;
                }

                CodeGeneration.CodeGeneratorHelpers.AddGeneratedSource(ctx, "WhenAnyDispatch.g.cs", source, data.Right);
            });
    }
}

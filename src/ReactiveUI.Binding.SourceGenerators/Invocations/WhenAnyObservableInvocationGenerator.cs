// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Invocations;

/// <summary>
/// Detects WhenAnyObservable invocations and generates per-invocation observation code.
/// Generates Switch, Merge, and CombineLatest patterns for observing properties that are
/// themselves observables.
/// </summary>
internal static class WhenAnyObservableInvocationGenerator
{
    /// <summary>
    /// Registers the WhenAnyObservable invocation detection pipeline.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    /// <param name="allClasses">The shared type detection pipeline.</param>
    /// <param name="supportsCallerArgExpr">Whether C# 10+ CallerArgumentExpression is available.</param>
    internal static void Register(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<ClassBindingInfo> allClasses,
        IncrementalValueProvider<bool> supportsCallerArgExpr)
    {
        var invocations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: RoslynHelpers.IsWhenAnyObservableInvocation,
                transform: MetadataExtractor.ExtractWhenAnyObservableInvocation)
            .Where(static x => x is not null)
            .Select(static (x, _) => x!);

        var combined = invocations.Collect()
            .Combine(allClasses.Collect())
            .Combine(supportsCallerArgExpr);

        context.RegisterSourceOutput(
            combined,
            static (ctx, data) =>
            {
                var source = WhenAnyObservableCodeGenerator.Generate(data.Left.Left, data.Left.Right, data.Right);
                if (source != null)
                {
                    ctx.AddSource("WhenAnyObservableDispatch.g.cs", source);
                }
            });
    }
}

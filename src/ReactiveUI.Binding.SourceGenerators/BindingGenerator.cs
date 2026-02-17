// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using ReactiveUI.Binding.SourceGenerators.Generators;
using ReactiveUI.Binding.SourceGenerators.Invocations;

namespace ReactiveUI.Binding.SourceGenerators;

/// <summary>
/// The main incremental source generator entry point for ReactiveUI property observation and binding.
/// Orchestrates two pipelines:
/// Pipeline A (Type Detection): Detects notification mechanisms and generates high-affinity fallback binders.
/// Pipeline B (Invocation Detection): Detects WhenChanged/WhenChanging/Bind calls and generates per-invocation code.
/// </summary>
[Generator]
public class BindingGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Detect C# language version for CallerArgumentExpression support (C# 10+)
        var supportsCallerArgExpr = context.ParseOptionsProvider.Select(static (opts, _) =>
            opts is CSharpParseOptions csharpOpts
            && csharpOpts.LanguageVersion >= LanguageVersion.CSharp10);

        // Conditionally emit CallerArgumentExpression polyfill when C# 10+ but attribute is missing
        var needsPolyfill = supportsCallerArgExpr
            .Combine(context.CompilationProvider)
            .Select(static (data, _) =>
                data.Left && data.Right.GetTypeByMetadataName(
                    Constants.CallerArgumentExpressionAttributeMetadataName) is null);

        context.RegisterSourceOutput(needsPolyfill, static (ctx, needs) =>
        {
            if (needs)
            {
                ctx.AddSource(
                    "CallerArgumentExpressionAttribute.g.cs",
                    Constants.CallerArgumentExpressionAttributeSource);
            }
        });

        // Conditionally emit ModuleInitializer polyfill for runtimes that don't include it (e.g. .NET Framework)
        var needsModuleInitPolyfill = context.CompilationProvider
            .Select(static (compilation, _) =>
                compilation.GetTypeByMetadataName(
                    Constants.ModuleInitializerAttributeMetadataName) is null);

        context.RegisterSourceOutput(needsModuleInitPolyfill, static (ctx, needs) =>
        {
            if (needs)
            {
                ctx.AddSource(
                    "ModuleInitializerAttribute.g.cs",
                    Constants.ModuleInitializerAttributeSource);
            }
        });

        // Pipeline A: Shared type detection
        // One pass: sets flags for IRO, INPC, WpfDP, WinUIDP, KVO, etc.
        var allClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: RoslynHelpers.IsClassWithBaseList,
                transform: MetadataExtractor.ExtractClassBindingInfo)
            .Where(static x => x is not null)
            .Select(static (x, _) => x!);

        // Per-kind fallback generators FILTER from allClasses (no independent RegisterSourceOutput)
        var reactiveObjTypes = ReactiveObjectBindingGenerator.Filter(allClasses);
        var inpcTypes = INPCBindingGenerator.Filter(allClasses);
        var wpfTypes = WpfBindingGenerator.Filter(allClasses);
        var winuiTypes = WinUIBindingGenerator.Filter(allClasses);
        var kvoTypes = KVOBindingGenerator.Filter(allClasses);
        var winformsTypes = WinFormsBindingGenerator.Filter(allClasses);
        var androidTypes = AndroidBindingGenerator.Filter(allClasses);

        // Consolidate all per-kind results → single RegisterSourceOutput
        var consolidated = RegistrationGenerator.Consolidate(
            reactiveObjTypes, inpcTypes, wpfTypes, winuiTypes, kvoTypes, winformsTypes, androidTypes);

        context.RegisterSourceOutput(
            consolidated,
            static (ctx, data) => RegistrationGenerator.Generate(ctx, data));

        // Pipeline B: Invocation detection (separate pipelines per API)
        // Each invocation generator receives supportsCallerArgExpr to control dispatch strategy
        WhenChangedInvocationGenerator.Register(context, allClasses, supportsCallerArgExpr);
        WhenChangingInvocationGenerator.Register(context, allClasses, supportsCallerArgExpr);
        BindOneWayInvocationGenerator.Register(context, allClasses, supportsCallerArgExpr);
        BindTwoWayInvocationGenerator.Register(context, allClasses, supportsCallerArgExpr);
        OneWayBindInvocationGenerator.Register(context, allClasses, supportsCallerArgExpr);
        BindInvocationGenerator.Register(context, allClasses, supportsCallerArgExpr);
        WhenAnyValueInvocationGenerator.Register(context, allClasses, supportsCallerArgExpr);
        WhenAnyInvocationGenerator.Register(context, allClasses, supportsCallerArgExpr);
        WhenAnyObservableInvocationGenerator.Register(context, allClasses, supportsCallerArgExpr);
    }
}

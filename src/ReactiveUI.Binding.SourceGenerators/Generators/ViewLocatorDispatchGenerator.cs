// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Generators;

/// <summary>
/// Generates the AOT-safe view dispatch code for <see cref="ViewRegistrationInfo"/> entries.
/// Emits a type-switch function that resolves views without reflection.
/// </summary>
internal static class ViewLocatorDispatchGenerator
{
    /// <summary>The identifier prefix used for the per-view resolver methods emitted in the generated source.</summary>
    private const string ResolverMethodNamePrefix = "__ResolveView_";

    /// <summary>
    /// Registers the view locator dispatch pipeline into the incremental generator.
    /// Scans for classes implementing <c>IViewFor&lt;T&gt;</c> and generates a dispatch method.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
    /// <param name="languageFeatures">The consumer compilation's C# language-feature snapshot.</param>
    internal static void Register(
        in IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<LanguageFeatures> languageFeatures)
    {
        // Reuse Pipeline A's class-with-base-list predicate
        var viewRegistrations = context.SyntaxProvider
            .CreateSyntaxProvider(
                RoslynHelpers.IsClassWithBaseList,
                ViewRegistrationExtractor.ExtractFromIViewForImplementation)
            .Where(static x => x is not null)
            .Select(static (x, _) => x!);

        // Collect and deduplicate
        var collected = viewRegistrations.Collect();

        context.RegisterSourceOutput(
            collected.Combine(languageFeatures),
            static (ctx, data) => Generate(ctx, data.Left, data.Right));
    }

    /// <summary>Generates the ViewDispatch.g.cs source file from collected view registrations.</summary>
    /// <param name="context">The source production context.</param>
    /// <param name="registrations">All detected view registration infos.</param>
    /// <param name="features">The consumer compilation's language-feature and generation-option snapshot.</param>
    internal static void Generate(in SourceProductionContext context, ImmutableArray<ViewRegistrationInfo> registrations, in LanguageFeatures features)
    {
        if (!ExtractorValidation.HasItems(registrations))
        {
            return;
        }

        // Deduplicate by (ViewModel FQN, Contract) pair — first occurrence wins
        var deduplicated = Deduplicate(registrations);

        const int DispatchPreambleCapacity = 2_048;
        const int PerRegistrationCapacity = 512;

        var sb = CodeGeneration.PooledBuilder.Rent(DispatchPreambleCapacity + (deduplicated.Count * PerRegistrationCapacity));
        GenerateSource(sb, deduplicated, features);
        CodeGeneration.CodeGeneratorHelpers.AddGeneratedSource(
            context,
            "ViewDispatch.g.cs",
            CodeGeneration.PooledBuilder.ToStringAndReturn(sb),
            features);
    }

    /// <summary>Deduplicates view registrations by (view model fully qualified name, contract) pair.</summary>
    /// <param name="registrations">The raw registrations.</param>
    /// <returns>A deduplicated list of registrations.</returns>
    private static List<ViewRegistrationInfo> Deduplicate(ImmutableArray<ViewRegistrationInfo> registrations)
    {
        var seen = new HashSet<(string, string?)>(EqualityComparer<(string, string?)>.Default);
        var result = new List<ViewRegistrationInfo>(registrations.Length);

        for (var i = 0; i < registrations.Length; i++)
        {
            var reg = registrations[i];
            if (seen.Add((reg.ViewModelFullyQualifiedName, reg.Contract)))
            {
                result.Add(reg);
            }
        }

        return result;
    }

    /// <summary>Generates the full source output into the StringBuilder.</summary>
    /// <param name="sb">The string builder to write to.</param>
    /// <param name="registrations">The deduplicated registrations.</param>
    /// <param name="features">The consumer compilation's language-feature and generation-option snapshot.</param>
    private static void GenerateSource(StringBuilder sb, List<ViewRegistrationInfo> registrations, in LanguageFeatures features)
    {
        var supportsNullable = features.SupportsNullable;
        EmitFileHeader(sb, features);

        // Singleton cache fields for [SingleInstanceView] views
        EmitSingletonFields(sb, registrations);

        EmitRegistrationHook(sb, supportsNullable ? "?" : string.Empty);

        // Emit the per-view-model dispatch branches into the dispatch function body.
        EmitDispatchBranches(sb, registrations);

        _ = sb.AppendLine().Append("""

                                               // No compile-time mapping found; fall back to runtime resolution.
                                               return null;
                                           }
                               """);

        // Per-view resolver methods
        for (var i = 0; i < registrations.Count; i++)
        {
            GenerateResolverMethod(sb, registrations[i], i, supportsNullable);
        }

        _ = sb.AppendLine().Append("""
                                   }
                               }
                               """);
    }

    /// <summary>Emits the generated-file markers, nullable directive, and the enclosing namespace and class declarations.</summary>
    /// <param name="sb">The string builder to write to.</param>
    /// <param name="features">The consumer compilation's language-feature and generation-option snapshot.</param>
    private static void EmitFileHeader(StringBuilder sb, in LanguageFeatures features)
    {
        CodeGeneration.CodeGeneratorHelpers.AppendGeneratedFileMarkers(sb, features.EmitGeneratedCodeMarkers);
        if (features.SupportsNullable)
        {
            _ = sb.AppendLine("#nullable enable");
        }

        _ = sb.Append("\nnamespace ")
            .Append(features.GeneratedNamespace)
            .Append("\n{\n    internal static partial class ")
            .Append(Constants.GeneratedExtensionClassName)
            .Append("\n    {");
    }

    /// <summary>
    /// Emits the static field initializer that registers the dispatch function on class load, and
    /// the signature of the dispatch function itself.
    /// </summary>
    /// <param name="sb">The string builder to write to.</param>
    /// <param name="nullable">The nullable annotation to emit, or an empty string when unsupported.</param>
    private static void EmitRegistrationHook(StringBuilder sb, string nullable) =>
        sb.AppendLine().Append($$"""
                                       /// <summary>
                                       /// Triggers view dispatch registration when the generated bindings class is loaded.
                                       /// </summary>
                                       private static readonly bool __viewDispatchRegistered = __RegisterViewDispatch();

                                       /// <summary>
                                       /// Registers the source-generated view dispatch function with
                                       /// <see cref="global::ReactiveUI.Binding.DefaultViewLocator"/>.
                                       /// Called once via static field initializer when this class is first accessed.
                                       /// </summary>
                                       /// <returns>Always returns <see langword="true"/>.</returns>
                                       private static bool __RegisterViewDispatch()
                                       {
                                           global::ReactiveUI.Binding.DefaultViewLocator.SetGeneratedViewDispatch(
                                               __TryResolveView);
                                           return true;
                                       }

                                       /// <summary>
                                       /// Compile-time generated type-switch dispatch for view resolution.
                                       /// Attempts to resolve a view for the given view model instance without reflection.
                                       /// </summary>
                                       /// <param name="instance">The view model instance to resolve a view for.</param>
                                       /// <param name="contract">The contract string (empty string for default).</param>
                                       /// <returns>The resolved view, or <see langword="null"/> if no generated mapping exists.</returns>
                                       private static global::ReactiveUI.Binding.IViewFor{{nullable}} __TryResolveView(
                                           object instance, string contract)
                                       {
                           """);

    /// <summary>Emits the singleton cache fields for <c>[SingleInstanceView]</c> views with a parameterless constructor.</summary>
    /// <param name="sb">The string builder to write to.</param>
    /// <param name="registrations">The deduplicated registrations.</param>
    private static void EmitSingletonFields(StringBuilder sb, List<ViewRegistrationInfo> registrations)
    {
        for (var i = 0; i < registrations.Count; i++)
        {
            var reg = registrations[i];
            if (reg.IsSingleInstance && reg.HasParameterlessConstructor)
            {
                _ = sb.AppendLine().Append($$"""
                                                     /// <summary>
                                                     /// Cached singleton instance for <see cref="{{reg.ViewFullyQualifiedName}}"/> (marked with [SingleInstanceView]).
                                                     /// </summary>
                                                     private static {{reg.ViewFullyQualifiedName}} __singletonView_{{i}};
                                         """);
            }
        }
    }

    /// <summary>
    /// Groups registrations by ViewModel FQN and emits the dispatch branches in order.
    /// Contract-specific checks are emitted before the default (no-contract) branch within a single
    /// type-switch block. Without grouping, a default branch emitted first would unconditionally
    /// match and shadow contract-specific branches.
    /// </summary>
    /// <param name="sb">The string builder to write to.</param>
    /// <param name="registrations">The deduplicated registrations.</param>
    private static void EmitDispatchBranches(StringBuilder sb, List<ViewRegistrationInfo> registrations)
    {
        var viewModelOrder = new List<string>(registrations.Count);
        var viewModelGroupIndices = new Dictionary<string, List<int>>(registrations.Count);
        for (var i = 0; i < registrations.Count; i++)
        {
            var viewModelFqn = registrations[i].ViewModelFullyQualifiedName;
            if (!viewModelGroupIndices.TryGetValue(viewModelFqn, out var indices))
            {
                indices = [];
                viewModelGroupIndices[viewModelFqn] = indices;
                viewModelOrder.Add(viewModelFqn);
            }

            indices.Add(i);
        }

        for (var g = 0; g < viewModelOrder.Count; g++)
        {
            var viewModelFqn = viewModelOrder[g];
            var indices = viewModelGroupIndices[viewModelFqn];

            _ = sb.AppendLine();

            if (indices.Count == 1)
            {
                // Single registration per VM: emit the compact form.
                EmitSingleRegistrationDispatch(sb, registrations, indices[0]);
            }
            else
            {
                // Multiple registrations for same VM: group into single type-switch.
                EmitGroupedDispatch(sb, registrations, viewModelFqn, indices);
            }
        }
    }

    /// <summary>
    /// Emits a dispatch branch for a single registration (one view per VM type).
    /// Preserves the compact output format used by existing tests.
    /// </summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="registrations">All registrations.</param>
    /// <param name="index">The registration index.</param>
    private static void EmitSingleRegistrationDispatch(
        StringBuilder sb,
        List<ViewRegistrationInfo> registrations,
        int index)
    {
        var reg = registrations[index];
        var resolverMethodName = ResolverMethodNamePrefix + index;

        if (reg.Contract is not null)
        {
            var escapedLiteral = SymbolDisplay.FormatLiteral(reg.Contract, true);
            _ = sb.Append($$"""
                                    // {{reg.ViewModelFullyQualifiedName}} -> {{reg.ViewFullyQualifiedName}} [contract: {{escapedLiteral}}]
                                    if (instance is {{reg.ViewModelFullyQualifiedName}})
                                    {
                                        if (contract == {{escapedLiteral}})
                                        {
                                            return {{resolverMethodName}}(contract);
                                        }
                                    }
                        """);
        }
        else
        {
            _ = sb.Append($$"""
                                    // {{reg.ViewModelFullyQualifiedName}} -> {{reg.ViewFullyQualifiedName}}
                                    if (instance is {{reg.ViewModelFullyQualifiedName}})
                                    {
                                        return {{resolverMethodName}}(contract);
                                    }
                        """);
        }
    }

    /// <summary>
    /// Emits a grouped dispatch branch for a VM type with multiple registrations.
    /// Contract-specific checks are emitted first, with the default (no-contract) branch last.
    /// </summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="registrations">All registrations.</param>
    /// <param name="viewModelFqn">The fully qualified VM type name.</param>
    /// <param name="indices">The registration indices for this VM type.</param>
    private static void EmitGroupedDispatch(
        StringBuilder sb,
        List<ViewRegistrationInfo> registrations,
        string viewModelFqn,
        List<int> indices)
    {
        _ = sb.Append($$"""
                                // {{viewModelFqn}} — multiple views
                                if (instance is {{viewModelFqn}})
                                {
                    """);

        // Contract-specific branches first
        for (var j = 0; j < indices.Count; j++)
        {
            var idx = indices[j];
            var reg = registrations[idx];
            if (reg.Contract is not null)
            {
                var escapedLiteral = SymbolDisplay.FormatLiteral(reg.Contract, true);
                var resolverMethodName = ResolverMethodNamePrefix + idx;
                _ = sb.AppendLine().Append($$"""
                                                     // -> {{reg.ViewFullyQualifiedName}} [contract: {{escapedLiteral}}]
                                                     if (contract == {{escapedLiteral}})
                                                     {
                                                         return {{resolverMethodName}}(contract);
                                                     }
                                         """);
            }
        }

        // Default (no-contract) branch last
        for (var j = 0; j < indices.Count; j++)
        {
            var idx = indices[j];
            var reg = registrations[idx];
            if (reg.Contract is null)
            {
                var resolverMethodName = ResolverMethodNamePrefix + idx;
                _ = sb.AppendLine().Append($$"""
                                                     // -> {{reg.ViewFullyQualifiedName}} (default)
                                                     return {{resolverMethodName}}(contract);
                                         """);
                break; // Only one default per VM (deduplicated earlier)
            }
        }

        _ = sb.AppendLine().Append("""
                                           }
                               """);
    }

    /// <summary>Generates a per-view-model resolver method.</summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="reg">The view registration info.</param>
    /// <param name="index">The unique index for method naming.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    private static void GenerateResolverMethod(StringBuilder sb, ViewRegistrationInfo reg, int index, bool supportsNullable)
    {
        var methodName = ResolverMethodNamePrefix + index;
        var nullable = supportsNullable ? "?" : string.Empty;

        var strategyDoc = (reg.IsSingleInstance, reg.HasParameterlessConstructor) switch
        {
            (true, true) => "        /// Returns a cached singleton instance (marked with [SingleInstanceView]).",
            (true, false) =>
                "        /// Service locator only — [SingleInstanceView] without parameterless constructor.",
            (false, true) => "        /// Tries the service locator first, then falls back to direct construction.",
            (false, false) => "        /// Service locator only — no direct construction available."
        };

        _ = sb.AppendLine().Append($$"""

                                             /// <summary>
                                             /// Resolves a view for <see cref="{{reg.ViewModelFullyQualifiedName}}"/>.
                                 {{strategyDoc}}
                                             /// </summary>
                                             /// <param name="contract">The contract string (empty string for default).</param>
                                             /// <returns>The resolved view, or <see langword="null"/> if resolution fails.</returns>
                                             private static global::ReactiveUI.Binding.IViewFor{{nullable}} {{methodName}}(string contract)
                                             {
                                                 // Normalize contract: empty string means no contract (null for Splat lookup).
                                                 string{{nullable}} svcContract = contract.Length == 0 ? null : contract;

                                                 // Prefer service-locator-registered view (supports DI-configured instances).
                                                 var view = global::Splat.AppLocator.Current
                                                     .GetService<global::ReactiveUI.Binding.IViewFor<{{reg.ViewModelFullyQualifiedName}}>>(
                                                         svcContract);
                                                 if (view != null)
                                                 {
                                                     return view;
                                                 }
                                 """);

        EmitResolverFallback(sb, reg, index);

        _ = sb.AppendLine().Append("""
                                           }
                               """);
    }

    /// <summary>
    /// Emits what the resolver does when the service locator has nothing registered: construct the
    /// view directly, cache a singleton, or give up.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="reg">The view registration being emitted.</param>
    /// <param name="index">The registration's index, used to name the singleton field.</param>
    private static void EmitResolverFallback(StringBuilder sb, ViewRegistrationInfo reg, int index)
    {
        if (!reg.HasParameterlessConstructor)
        {
            _ = sb.Append("""

                                      return null;
                      """);
            return;
        }

        if (!reg.IsSingleInstance)
        {
            _ = sb.Append($$"""

                                            // Fallback: direct construction ({{reg.ViewFullyQualifiedName}} has a parameterless constructor).
                                            return new {{reg.ViewFullyQualifiedName}}();
                        """);
            return;
        }

        var fieldName = $"__singletonView_{index}";
        _ = sb.Append($$"""

                                        // Fallback: singleton construction ({{reg.ViewFullyQualifiedName}} has [SingleInstanceView]).
                                        if ({{fieldName}} == null)
                                        {
                                            System.Threading.Interlocked.CompareExchange(
                                                ref {{fieldName}},
                                                new {{reg.ViewFullyQualifiedName}}(),
                                                null);
                                        }

                                        return {{fieldName}};
                    """);
    }
}

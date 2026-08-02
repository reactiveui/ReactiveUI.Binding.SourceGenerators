// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;

namespace ReactiveUI.Binding.SourceGenerators.Generators;

/// <summary>
/// Declares the platform observation helper classes - the fused observables and observer shims that
/// generated observation code instantiates by name, such as the Apple KVO and WinUI dependency-property
/// observables.
/// </summary>
/// <remarks>
/// <para>
/// The helpers are declared once for the whole compilation, in a file of their own, because every dispatch
/// file is another part of the same <c>__ReactiveUIGeneratedBindings</c> class: one part declares them and
/// all the others reach them. Letting each file declare the helpers it happens to use would collide as soon
/// as two files used the same one, and letting one dispatch file own them - which is what used to happen -
/// left every other file referencing types that were never declared.
/// </para>
/// <para>
/// Which helpers to declare is decided from the detected types rather than from the call sites, so the
/// declarations are a superset of the references: observation code can only name a helper for a type this
/// pipeline detected, whichever API the call site used. A future binding API therefore cannot reintroduce
/// the undeclared-helper failure by forgetting to register itself here.
/// </para>
/// </remarks>
internal static class ObservationHelperGenerator
{
    /// <summary>The generated file the helper classes are declared in.</summary>
    private const string HintName = "ObservationHelpers.g.cs";

    /// <summary>Buffer capacity to reserve per helper-requiring observation kind.</summary>
    private const int PerKindBufferCapacity = 4_096;

    /// <summary>
    /// Reduces the per-type observation kinds to the distinct, ordered set of kinds that need helper
    /// declarations, so adding another type of an already-seen kind leaves the generated file untouched.
    /// </summary>
    /// <param name="observationKinds">The observation kind of every detected type, with repeats.</param>
    /// <returns>The kinds requiring helper declarations, ordered for deterministic output.</returns>
    internal static EquatableArray<string> SelectHelperKinds(ImmutableArray<string> observationKinds)
    {
        if (observationKinds.IsDefaultOrEmpty)
        {
            return default;
        }

        var kinds = new SortedSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < observationKinds.Length; i++)
        {
            var plugin = ObservationPluginRegistry.GetPluginByKind(observationKinds[i]);
            if (plugin?.RequiresHelperClasses == true)
            {
                _ = kinds.Add(plugin.ObservationKind);
            }
        }

        if (kinds.Count == 0)
        {
            return default;
        }

        var ordered = new string[kinds.Count];
        kinds.CopyTo(ordered);
        return new(ordered);
    }

    /// <summary>Declares the helper classes for the given observation kinds.</summary>
    /// <param name="context">The source production context.</param>
    /// <param name="helperKinds">The observation kinds requiring helper declarations, in output order.</param>
    /// <param name="features">The consumer compilation's language-feature and generation-option snapshot.</param>
    internal static void Generate(
        in SourceProductionContext context,
        EquatableArray<string> helperKinds,
        in LanguageFeatures features)
    {
        if (helperKinds.Length == 0)
        {
            return;
        }

        var sb = PooledBuilder.Rent(helperKinds.Length * PerKindBufferCapacity);
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb, features);
        AppendHelperDeclarations(sb, helperKinds);
        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        _ = sb.AppendLine();

        CodeGeneratorHelpers.AddGeneratedSource(context, HintName, PooledBuilder.ToStringAndReturn(sb), features);
    }

    /// <summary>Appends the declarations for each of the given observation kinds, in the order given.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="helperKinds">The observation kinds requiring helper declarations.</param>
    /// <remarks>
    /// A kind no plugin answers to contributes nothing. <see cref="SelectHelperKinds"/> only ever yields kinds
    /// it read off a plugin, so the pipeline cannot produce one - but a generator that threw on an unexpected
    /// kind would fail the consumer's build rather than merely generate less, which is the worse of the two.
    /// </remarks>
    internal static void AppendHelperDeclarations(StringBuilder sb, EquatableArray<string> helperKinds)
    {
        for (var i = 0; i < helperKinds.Length; i++)
        {
            ObservationPluginRegistry.GetPluginByKind(helperKinds[i])?.EmitHelperClasses(sb);
        }
    }
}

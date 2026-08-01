// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Generators;

/// <summary>
/// Generates the consolidated registration output for all detected observable types.
/// Produces a registration class listing all per-kind binder implementations.
/// </summary>
internal static class RegistrationGenerator
{
    /// <summary>Generates the consolidated registration output: all per-kind binder classes.</summary>
    /// <param name="context">The source production context.</param>
    /// <param name="allTypes">All detected observable type infos across all notification kinds.</param>
    /// <param name="features">The consumer compilation's language-feature and generation-option snapshot.</param>
    internal static void Generate(in SourceProductionContext context, ImmutableArray<ObservableTypeInfo> allTypes, in LanguageFeatures features)
    {
        if (!Helpers.ExtractorValidation.HasItems(allTypes))
        {
            return;
        }

        // Collect unique observation kinds that were detected
        var uniqueKinds = new HashSet<string>();
        for (var i = 0; i < allTypes.Length; i++)
        {
            _ = uniqueKinds.Add(allTypes[i].ObservationKind);
        }

        var sb = CodeGeneration.PooledBuilder.Rent(
            CodeGeneration.CodeGeneratorHelpers.PerInvocationBufferCapacity
            + (allTypes.Length * CodeGeneration.CodeGeneratorHelpers.FragmentBufferCapacity));
        CodeGeneration.CodeGeneratorHelpers.AppendGeneratedFileMarkers(sb, features.EmitGeneratedCodeMarkers);
        if (features.SupportsNullable)
        {
            _ = sb.AppendLine("#nullable enable");
        }

        _ = sb.AppendLine("""

                      namespace ReactiveUI.Binding.Generated
                      {
                          /// <summary>
                          /// Auto-generated binder registration. Registers high-affinity
                          /// ICreatesObservableForProperty implementations detected at compile time.
                          /// </summary>
                          [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
                          internal static class __GeneratedBinderRegistration
                          {
                              /// <summary>
                              /// Registers all generated binders with the Splat service locator.
                              /// </summary>
                              internal static void Initialize()
                              {
                                  // Generated binder registrations will be added here in future phases.
                                  // Each per-kind binder provides high-affinity observation for detected types.
                      """);

        foreach (var kind in uniqueKinds)
        {
            _ = sb.AppendLine($"            // Detected types for kind: {kind}");
        }

        _ = sb.AppendLine("""
                              }
                          }
                      }
                      """);

        CodeGeneration.CodeGeneratorHelpers.AddGeneratedSource(
            context,
            "GeneratedBinderRegistration.g.cs",
            CodeGeneration.PooledBuilder.ToStringAndReturn(sb),
            features);
    }
}

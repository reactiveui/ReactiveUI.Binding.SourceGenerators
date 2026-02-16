// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads for WhenAnyValue invocations.
/// Delegates to shared generation logic in <see cref="ObservationCodeGenerator"/>.
/// </summary>
internal static class WhenAnyValueCodeGenerator
{
    /// <summary>
    /// Generates concrete typed overloads and observation methods for WhenAnyValue invocations.
    /// </summary>
    /// <param name="invocations">All detected WhenAnyValue invocations.</param>
    /// <param name="allClasses">All detected class binding info for type mechanism lookup.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression (C# 10+).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    internal static string? Generate(
        ImmutableArray<InvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        bool supportsCallerArgExpr)
        => ObservationCodeGenerator.Generate(invocations, allClasses, supportsCallerArgExpr, "WhenAnyValue");
}

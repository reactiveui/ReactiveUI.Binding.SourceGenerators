// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Value-equatable snapshot of the consumer compilation's relevant C# language capabilities and generation
/// options, flowed through the incremental pipeline so generated output can adapt to the target language
/// version and consumer configuration.
/// </summary>
/// <param name="SupportsCallerArgExpr">
/// Whether the target supports <c>CallerArgumentExpression</c> dispatch (C# 10+ and the attribute is available).
/// </param>
/// <param name="SupportsNullable">
/// Whether the target supports nullable reference types (C# 8+), in which case generated files emit an explicit
/// <c>#nullable enable</c> directive. Emitting that directive on C# 7.3 would be a compile error, so it is omitted.
/// </param>
/// <param name="EmitGeneratedCodeMarkers">
/// Whether generated files emit the <c>// &lt;auto-generated/&gt;</c> comment and <c>#pragma warning disable</c>
/// header. This is the shipping default (<see langword="true"/>); consumers set the MSBuild property
/// <c>ReactiveUIBindingEmitGeneratedCodeMarkers</c> to <c>false</c> to surface analyzer diagnostics in the
/// generated code (e.g. when diagnosing the generator itself).
/// </param>
internal readonly record struct LanguageFeatures(
    bool SupportsCallerArgExpr,
    bool SupportsNullable,
    bool EmitGeneratedCodeMarkers);

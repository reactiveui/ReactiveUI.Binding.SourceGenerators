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
/// <param name="GeneratedNamespace">
/// The namespace the dispatch overloads are emitted into. From C# 10 this is the consumer's own root namespace,
/// falling back to <c>ReactiveUI.Binding.Generated.&lt;assembly&gt;</c> when the build exposes no root namespace.
/// Either way it is reached by a generated <c>global using</c>, which is scoped to the compilation that declares
/// it, so two assemblies which both run the generator do not see each other's overloads. That matters whenever
/// one grants the other <c>InternalsVisibleTo</c>: the overloads stay accessible across that boundary, and
/// identical ones from both assemblies make every matching call site ambiguous (CS0121). A shared namespace
/// collides even when the class names differ, because extension lookup considers the methods, not the type.
/// </param>
/// <param name="EmitGeneratedNamespaceImport">
/// Whether to emit the <c>global using</c> that brings <see cref="GeneratedNamespace"/> into scope. Also marks
/// which dispatch tier applies: when false the consumer predates global usings, no overload is emitted, and
/// calls reach the generated code through the registry instead.
/// </param>
/// <param name="StubHasExpressionParameters">
/// Whether the runtime stub the generated overload competes with carries the optional expression parameters -
/// which it does wherever <c>CallerArgumentExpression</c> is available to it. The generated overload must take
/// the same parameters whether or not it dispatches on them, because the tie-break that lets a concrete
/// overload beat the generic stub only applies once the parameter lists match: a shorter one leaves both
/// candidates merely applicable, and every call site is then ambiguous (CS0121). This is independent of
/// <see cref="SupportsCallerArgExpr"/>, which turns on the attributes and the expression-text dispatch, and
/// which additionally needs C# 10 - a consumer can perfectly well target a runtime that has the attribute
/// while compiling at an older language version.
/// </param>
/// <param name="UsesReactiveRuntime">
/// Whether the consumer references the System.Reactive flavour of the runtime library rather than the lean one.
/// The two share no type names, so generated code written against one does not compile against the other.
/// </param>
/// <param name="RuntimeNamespaceMembers">
/// The names the referenced runtime library's own namespace declares, types and nested namespaces alike. These
/// anchor the retargeting of generated code onto the other flavour, so that it shifts the library's names and
/// leaves alone a consumer type that merely happens to sit under the same root. Empty for a lean consumer,
/// where nothing is retargeted.
/// </param>
internal readonly record struct LanguageFeatures(
    bool SupportsCallerArgExpr,
    bool SupportsNullable,
    bool EmitGeneratedCodeMarkers,
    string GeneratedNamespace = Constants.SharedGeneratedNamespace,
    bool EmitGeneratedNamespaceImport = false,
    bool StubHasExpressionParameters = false,
    bool UsesReactiveRuntime = false,
    EquatableArray<string> RuntimeNamespaceMembers = default);

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Represents a single segment in a property path chain (e.g., one step in x.Address.City).
/// Value-equatable POCO for the incremental generator pipeline.
/// Contains no ISymbol, SyntaxNode, or Location references.
/// </summary>
/// <param name="PropertyName">The name of the property for this path segment.</param>
/// <param name="PropertyTypeFullName">The fully qualified type of the property.</param>
/// <param name="DeclaringTypeFullName">The fully qualified type that declares this property.</param>
/// <param name="IsReferenceType">
/// Whether the property's type is a reference type. Used to decide whether the generated
/// <c>Expression&lt;Func&lt;…, T&gt;&gt;</c> selector parameter may be annotated nullable (<c>T?</c>) so it
/// accepts selectors of nullable reference-typed properties; value-type leaves stay non-nullable.
/// </param>
/// <param name="DeclaringTypeInfo">
/// How the declaring type notifies, or <see langword="null"/> when the segment was built without a symbol to
/// read it from. Each link of a chain notifies on its own terms, so the mechanism travels with the segment
/// rather than being inferred from the type the chain started at.
/// <para>
/// Carried here rather than looked up later because extraction already holds the property symbol. Resolving it
/// afterwards means a second pass over the same call sites, and re-binding one of these invocations is the
/// single largest allocation in a generation pass - extension-method overload resolution and generic type
/// inference dominate it.
/// </para>
/// </param>
internal sealed record PropertyPathSegment(
    string PropertyName,
    string PropertyTypeFullName,
    string DeclaringTypeFullName,
    bool IsReferenceType,
    ClassBindingInfo? DeclaringTypeInfo);

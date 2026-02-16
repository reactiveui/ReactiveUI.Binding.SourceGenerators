// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Represents a single segment in a property path chain (e.g., one step in x.Address.City).
/// Value-equatable POCO for the incremental generator pipeline.
/// Contains no ISymbol, SyntaxNode, or Location references.
/// </summary>
/// <param name="PropertyName">The name of the property for this path segment.</param>
/// <param name="PropertyTypeFullName">The fully qualified type of the property.</param>
/// <param name="DeclaringTypeFullName">The fully qualified type that declares this property.</param>
internal sealed record PropertyPathSegment(
    string PropertyName,
    string PropertyTypeFullName,
    string DeclaringTypeFullName) : IEquatable<PropertyPathSegment>;

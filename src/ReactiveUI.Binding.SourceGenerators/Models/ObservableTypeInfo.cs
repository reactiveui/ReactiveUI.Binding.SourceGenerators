// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Per-kind filtered value-equatable POCO for the incremental generator pipeline.
/// Produced by per-kind fallback generators filtering from <see cref="ClassBindingInfo"/>.
/// Contains no ISymbol, SyntaxNode, or Location references.
/// </summary>
/// <param name="FullyQualifiedName">The fully qualified name of the type (global:: prefixed).</param>
/// <param name="MetadataName">The metadata name of the type (without namespace or global:: prefix).</param>
/// <param name="ObservationKind">The observation mechanism kind (e.g., "INPC", "ReactiveObject", "WpfDP").</param>
/// <param name="Affinity">The priority affinity score for this observation kind.</param>
/// <param name="SupportsBeforeChanged">Whether this observation kind supports before-change notifications.</param>
/// <param name="Properties">The observable properties on this type.</param>
internal sealed record ObservableTypeInfo(
    string FullyQualifiedName,
    string MetadataName,
    string ObservationKind,
    int Affinity,
    bool SupportsBeforeChanged,
    EquatableArray<ObservablePropertyInfo> Properties) : IEquatable<ObservableTypeInfo>;

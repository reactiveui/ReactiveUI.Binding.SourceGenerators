// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Property-level value-equatable POCO for the incremental generator pipeline.
/// Contains no ISymbol, SyntaxNode, or Location references.
/// </summary>
/// <param name="PropertyName">The name of the property.</param>
/// <param name="PropertyTypeFullName">The fully qualified type of the property.</param>
/// <param name="HasPublicGetter">Whether the property has a public getter.</param>
/// <param name="IsIndexer">Whether the property is an indexer.</param>
/// <param name="IsDependencyProperty">Whether the property is a dependency property (WPF/WinUI).</param>
internal sealed record ObservablePropertyInfo(
    string PropertyName,
    string PropertyTypeFullName,
    bool HasPublicGetter,
    bool IsIndexer,
    bool IsDependencyProperty) : IEquatable<ObservablePropertyInfo>;

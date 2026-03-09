// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Value-equatable POCO representing a view-to-view-model mapping detected at compile time.
/// Produced by scanning <c>IViewFor&lt;T&gt;</c> implementations and <c>Map&lt;TVM,TView&gt;()</c> call sites.
/// Contains no ISymbol, SyntaxNode, or Location references.
/// </summary>
/// <param name="ViewModelFullyQualifiedName">The fully qualified name of the view model type (global:: prefixed).</param>
/// <param name="ViewFullyQualifiedName">The fully qualified name of the view type (global:: prefixed).</param>
/// <param name="HasParameterlessConstructor">Whether the view type has a parameterless constructor for direct instantiation.</param>
internal sealed record ViewRegistrationInfo(
    string ViewModelFullyQualifiedName,
    string ViewFullyQualifiedName,
    bool HasParameterlessConstructor) : IEquatable<ViewRegistrationInfo>;

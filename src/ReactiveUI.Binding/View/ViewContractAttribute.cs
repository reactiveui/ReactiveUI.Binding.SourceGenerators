// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Allows an additional string to make view resolution more specific than
/// just a type. When applied to your <see cref="IViewFor{T}"/>-derived
/// View, the source generator will register this view under the specified
/// contract, enabling selection between different Views for a single ViewModel.
/// </summary>
/// <param name="contract">The contract value for view resolution.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ViewContractAttribute(string contract) : Attribute
{
    /// <summary>
    /// Gets the contract to use when resolving the view.
    /// </summary>
    public string Contract { get; } = contract;
}

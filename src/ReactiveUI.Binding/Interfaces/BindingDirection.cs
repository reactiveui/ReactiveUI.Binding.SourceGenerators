// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Specifies the direction of a property binding.
/// </summary>
public enum BindingDirection
{
    /// <summary>
    /// One-way binding from source to target.
    /// </summary>
    OneWay,

    /// <summary>
    /// Two-way binding between source and target.
    /// </summary>
    TwoWay,

    /// <summary>
    /// One-way asynchronous binding from source to target.
    /// </summary>
    AsyncOneWay,
}

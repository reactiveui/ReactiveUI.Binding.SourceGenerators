// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Non-generic interface for views that display a view model.
/// </summary>
public interface IViewFor : IActivatableView
{
    /// <summary>
    /// Gets or sets the view model displayed by the view.
    /// </summary>
    object? ViewModel { get; set; }
}

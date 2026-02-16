// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Generic interface for views that display a specific view model type.
/// </summary>
/// <typeparam name="T">The type of the view model.</typeparam>
public interface IViewFor<T> : IViewFor
    where T : class
{
    /// <summary>
    /// Gets or sets the view model displayed by the view.
    /// </summary>
    new T? ViewModel { get; set; }
}

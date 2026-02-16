// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Maui;

/// <summary>
/// Enum that hints at the visibility of a UI element.
/// </summary>
[Flags]
public enum BooleanToVisibilityHint
{
    /// <summary>
    /// Do not modify the boolean type conversion from its default action of using Visibility.Collapsed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Inverse the action of the boolean type conversion; when true, collapse the visibility.
    /// </summary>
    Inverse = 1 << 1,

    /// <summary>
    /// Use the Hidden value rather than Collapsed (MAUI only; ignored on WinUI where Hidden is not available).
    /// </summary>
    UseHidden = 1 << 2,
}

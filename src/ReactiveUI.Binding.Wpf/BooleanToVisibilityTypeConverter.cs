// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;

namespace ReactiveUI.Binding.Wpf;

/// <summary>
/// Converts <see cref="bool"/> to <see cref="Visibility"/>.
/// </summary>
/// <remarks>
/// <para>
/// The conversion supports a <see cref="BooleanToVisibilityHint"/> as the conversion hint parameter:
/// </para>
/// <list type="bullet">
/// <item><description><see cref="BooleanToVisibilityHint.None"/> - True maps to Visible, False maps to Collapsed.</description></item>
/// <item><description><see cref="BooleanToVisibilityHint.Inverse"/> - Inverts the boolean before conversion.</description></item>
/// <item><description><see cref="BooleanToVisibilityHint.UseHidden"/> - Use Hidden instead of Collapsed for false values.</description></item>
/// </list>
/// </remarks>
public sealed class BooleanToVisibilityTypeConverter : BindingTypeConverter<bool, Visibility>
{
    /// <inheritdoc/>
    public override int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public override bool TryConvert(bool from, object? conversionHint, [NotNullWhen(true)] out Visibility result)
    {
        var hint = conversionHint is BooleanToVisibilityHint visibilityHint
            ? visibilityHint
            : BooleanToVisibilityHint.None;

        var value = (hint & BooleanToVisibilityHint.Inverse) != 0 ? !from : from;

        var notVisible = (hint & BooleanToVisibilityHint.UseHidden) != 0
            ? Visibility.Hidden
            : Visibility.Collapsed;

        result = value ? Visibility.Visible : notVisible;
        return true;
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if WINUI_TARGET
using Visibility = Microsoft.UI.Xaml.Visibility;
#else
using Visibility = Microsoft.Maui.Visibility;
#endif

namespace ReactiveUI.Binding.Maui;

/// <summary>
/// Converts <see cref="bool"/> to <see cref="Visibility"/>.
/// </summary>
/// <remarks>
/// <para>
/// The conversion supports a <see cref="BooleanToVisibilityHints"/> as the conversion hint parameter:
/// </para>
/// <list type="bullet">
/// <item><description><see cref="BooleanToVisibilityHints.None"/> - True maps to Visible, False maps to Collapsed.</description></item>
/// <item><description><see cref="BooleanToVisibilityHints.Inverse"/> - Inverts the boolean before conversion.</description></item>
/// <item><description><see cref="BooleanToVisibilityHints.UseHidden"/> - Use Hidden instead of Collapsed for false values (MAUI only, ignored on WinUI).</description></item>
/// </list>
/// </remarks>
public sealed class BooleanToVisibilityTypeConverter : BindingTypeConverter<bool, Visibility>
{
    /// <inheritdoc/>
    public override int GetAffinityForObjects() => BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override bool TryConvert(bool from, object? conversionHint, [NotNullWhen(true)] out Visibility result)
    {
        var hint = conversionHint is BooleanToVisibilityHints visibilityHint
            ? visibilityHint
            : BooleanToVisibilityHints.None;

        var value = (hint & BooleanToVisibilityHints.Inverse) != 0 ? !from : from;

#if !WINUI_TARGET
        var notVisible = (hint & BooleanToVisibilityHints.UseHidden) != 0
            ? Visibility.Hidden
            : Visibility.Collapsed;
#else
        const Visibility notVisible = Visibility.Collapsed;
#endif

        result = value ? Visibility.Visible : notVisible;
        return true;
    }
}

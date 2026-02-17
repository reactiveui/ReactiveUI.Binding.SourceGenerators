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
/// Converts <see cref="Visibility"/> to <see cref="bool"/>.
/// </summary>
/// <remarks>
/// <para>
/// The conversion supports a <see cref="BooleanToVisibilityHint"/> as the conversion hint parameter:
/// </para>
/// <list type="bullet">
/// <item><description><see cref="BooleanToVisibilityHint.None"/> - Visible maps to True, other values map to False.</description></item>
/// <item><description><see cref="BooleanToVisibilityHint.Inverse"/> - Inverts the result (Visible maps to False, other maps to True).</description></item>
/// </list>
/// </remarks>
public sealed class VisibilityToBooleanTypeConverter : BindingTypeConverter<Visibility, bool>
{
    /// <inheritdoc/>
    public override int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public override bool TryConvert(Visibility from, object? conversionHint, [NotNullWhen(true)] out bool result)
    {
        var hint = conversionHint is BooleanToVisibilityHint visibilityHint
            ? visibilityHint
            : BooleanToVisibilityHint.None;

        var isVisible = from == Visibility.Visible;
        result = (hint & BooleanToVisibilityHint.Inverse) != 0 ? !isVisible : isVisible;
        return true;
    }
}

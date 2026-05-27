// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="float"/> values to <see cref="string"/>.
/// </summary>
public sealed class SingleToStringTypeConverter : BindingTypeConverter<float, string>
{
    /// <summary>
    /// The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.
    /// </summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public override bool TryConvert(float from, object? conversionHint, [NotNullWhen(true)] out string? result)
    {
        switch (conversionHint)
        {
            case int decimalPlaces:
                {
                    result = from.ToString($"F{decimalPlaces}");
                    return true;
                }

            case string format:
                {
                    result = from.ToString(format);
                    return true;
                }

            default:
                {
                    result = from.ToString(System.Globalization.CultureInfo.CurrentCulture);
                    return true;
                }
        }
    }
}

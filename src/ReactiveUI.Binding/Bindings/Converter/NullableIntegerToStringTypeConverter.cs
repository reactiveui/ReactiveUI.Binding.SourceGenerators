// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Converts nullable <see cref="int"/> values to <see cref="string"/>.
/// </summary>
public sealed class NullableIntegerToStringTypeConverter : BindingTypeConverter<int?, string>
{
    /// <summary>
    /// The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.
    /// </summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public override bool TryConvert(int? from, object? conversionHint, [MaybeNullWhen(true)] out string? result)
    {
        if (!from.HasValue)
        {
            result = null;
            return true;
        }

        switch (conversionHint)
        {
            case int width:
                {
                    result = from.Value.ToString($"D{width}");
                    return true;
                }

            case string format:
                {
                    result = from.Value.ToString(format);
                    return true;
                }

            default:
                {
                    result = from.Value.ToString();
                    return true;
                }
        }
    }
}

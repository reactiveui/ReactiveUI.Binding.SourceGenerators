// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="string"/> to <see cref="byte"/> using <see cref="byte.TryParse(string?, out byte)"/>.
/// </summary>
public sealed class StringToByteTypeConverter : BindingTypeConverter<string, byte>
{
    /// <summary>
    /// The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.
    /// </summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public override bool TryConvert(string? from, object? conversionHint, [NotNullWhen(true)] out byte result)
    {
        if (from is null)
        {
            result = default;
            return false;
        }

        return byte.TryParse(from, out result);
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding;

/// <summary>
/// Converts nullable <see cref="long"/> values to <see cref="string"/>.
/// </summary>
public sealed class NullableLongToStringTypeConverter : BindingTypeConverter<long?, string>
{
    /// <inheritdoc/>
    public override int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public override bool TryConvert(long? from, object? conversionHint, [MaybeNullWhen(true)] out string? result)
    {
        if (!from.HasValue)
        {
            result = null;
            return true;
        }

        if (conversionHint is int width)
        {
            result = from.Value.ToString($"D{width}");
            return true;
        }

        if (conversionHint is string format)
        {
            result = from.Value.ToString(format);
            return true;
        }

        result = from.Value.ToString();
        return true;
    }
}

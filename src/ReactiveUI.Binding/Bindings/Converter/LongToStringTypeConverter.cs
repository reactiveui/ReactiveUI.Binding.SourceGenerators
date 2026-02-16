// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="long"/> values to <see cref="string"/>.
/// </summary>
public sealed class LongToStringTypeConverter : BindingTypeConverter<long, string>
{
    /// <inheritdoc/>
    public override int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public override bool TryConvert(long from, object? conversionHint, [NotNullWhen(true)] out string? result)
    {
        if (conversionHint is int width)
        {
            result = from.ToString($"D{width}");
            return true;
        }

        if (conversionHint is string format)
        {
            result = from.ToString(format);
            return true;
        }

        result = from.ToString();
        return true;
    }
}

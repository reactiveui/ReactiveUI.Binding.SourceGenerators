// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding;

/// <summary>
/// Converts nullable <see cref="DateTimeOffset"/> to <see cref="string"/>.
/// </summary>
public sealed class NullableDateTimeOffsetToStringTypeConverter : BindingTypeConverter<DateTimeOffset?, string>
{
    /// <inheritdoc/>
    public override int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public override bool TryConvert(DateTimeOffset? from, object? conversionHint, [MaybeNullWhen(true)] out string? result)
    {
        if (!from.HasValue)
        {
            result = null;
            return true;
        }

        result = from.Value.ToString();
        return true;
    }
}

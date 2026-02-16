// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="string"/> to <see cref="DateTimeOffset"/> using <see cref="DateTimeOffset.TryParse(string?, out DateTimeOffset)"/>.
/// </summary>
public sealed class StringToDateTimeOffsetTypeConverter : BindingTypeConverter<string, DateTimeOffset>
{
    /// <inheritdoc/>
    public override int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public override bool TryConvert(string? from, object? conversionHint, [NotNullWhen(true)] out DateTimeOffset result)
    {
        if (from is null)
        {
            result = default;
            return false;
        }

        return DateTimeOffset.TryParse(from, out result);
    }
}

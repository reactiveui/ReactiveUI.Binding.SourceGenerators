// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="string"/> to <see cref="decimal"/> using <see cref="decimal.TryParse(string?, out decimal)"/>.
/// </summary>
public sealed class StringToDecimalTypeConverter : BindingTypeConverter<string, decimal>
{
    /// <inheritdoc/>
    public override int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public override bool TryConvert(string? from, object? conversionHint, [NotNullWhen(true)] out decimal result)
    {
        if (from is null)
        {
            result = default;
            return false;
        }

        return decimal.TryParse(from, out result);
    }
}

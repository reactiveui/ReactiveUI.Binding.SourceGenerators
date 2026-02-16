// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="string"/> to <see cref="bool"/> using <see cref="bool.TryParse(string?, out bool)"/>.
/// </summary>
public sealed class StringToBooleanTypeConverter : BindingTypeConverter<string, bool>
{
    /// <inheritdoc/>
    public override int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public override bool TryConvert(string? from, object? conversionHint, [NotNullWhen(true)] out bool result)
    {
        if (from is null)
        {
            result = default;
            return false;
        }

        return bool.TryParse(from, out result);
    }
}

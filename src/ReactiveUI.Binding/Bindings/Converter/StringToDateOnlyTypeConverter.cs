// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="string"/> to <see cref="DateOnly"/> using <see cref="DateOnly.TryParse(string?, out DateOnly)"/>.
/// </summary>
public sealed class StringToDateOnlyTypeConverter : BindingTypeConverter<string, DateOnly>
{
    /// <inheritdoc/>
    public override int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public override bool TryConvert(string? from, object? conversionHint, [NotNullWhen(true)] out DateOnly result)
    {
        if (from is null)
        {
            result = default;
            return false;
        }

        return DateOnly.TryParse(from, out result);
    }
}
#endif

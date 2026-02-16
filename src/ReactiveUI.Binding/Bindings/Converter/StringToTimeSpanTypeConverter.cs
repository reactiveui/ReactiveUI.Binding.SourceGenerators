// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="string"/> to <see cref="TimeSpan"/> using <see cref="TimeSpan.TryParse(string?, out TimeSpan)"/>.
/// </summary>
public sealed class StringToTimeSpanTypeConverter : BindingTypeConverter<string, TimeSpan>
{
    /// <inheritdoc/>
    public override int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public override bool TryConvert(string? from, object? conversionHint, [NotNullWhen(true)] out TimeSpan result)
    {
        if (from is null)
        {
            result = default;
            return false;
        }

        return TimeSpan.TryParse(from, out result);
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="TimeOnly"/> to <see cref="string"/>.
/// </summary>
public sealed class TimeOnlyToStringTypeConverter : BindingTypeConverter<TimeOnly, string>
{
    /// <inheritdoc/>
    public override int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public override bool TryConvert(TimeOnly from, object? conversionHint, [NotNullWhen(true)] out string? result)
    {
        result = from.ToString();
        return true;
    }
}
#endif

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="Guid"/> to <see cref="string"/> using the "D" format (standard hyphenated format).
/// </summary>
public sealed class GuidToStringTypeConverter : BindingTypeConverter<Guid, string>
{
    /// <inheritdoc/>
    public override int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public override bool TryConvert(Guid from, object? conversionHint, [NotNullWhen(true)] out string? result)
    {
        result = from.ToString("D");
        return true;
    }
}

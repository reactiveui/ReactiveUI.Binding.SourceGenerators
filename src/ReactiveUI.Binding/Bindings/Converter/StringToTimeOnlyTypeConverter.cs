// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER
namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="string"/> to <see cref="TimeOnly"/> using <see cref="TimeOnly.TryParse(string?, out TimeOnly)"/>.
/// </summary>
public sealed class StringToTimeOnlyTypeConverter : BindingTypeConverter<string, TimeOnly>
{
    /// <summary>
    /// The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.
    /// </summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public override bool TryConvert(string? from, object? conversionHint, [NotNullWhen(true)] out TimeOnly result)
    {
        if (from is null)
        {
            result = default;
            return false;
        }

        return TimeOnly.TryParse(from, out result);
    }
}
#endif

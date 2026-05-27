// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="Uri"/> to <see cref="string"/>.
/// </summary>
public sealed class UriToStringTypeConverter : BindingTypeConverter<Uri, string>
{
    /// <summary>
    /// The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.
    /// </summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public override bool TryConvert(Uri? from, object? conversionHint, [NotNullWhen(true)] out string? result)
    {
        if (from is null)
        {
            result = null;
            return false;
        }

        result = from.ToString();
        return true;
    }
}

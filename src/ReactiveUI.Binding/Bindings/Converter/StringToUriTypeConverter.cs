// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="string"/> to <see cref="Uri"/> using <see cref="Uri.TryCreate(string?, UriKind, out Uri?)"/>.
/// </summary>
public sealed class StringToUriTypeConverter : BindingTypeConverter<string, Uri>
{
    /// <summary>
    /// The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.
    /// </summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public override bool TryConvert(string? from, object? conversionHint, [NotNullWhen(true)] out Uri? result)
    {
        if (from is null)
        {
            result = null;
            return false;
        }

        return Uri.TryCreate(from, UriKind.RelativeOrAbsolute, out result);
    }
}

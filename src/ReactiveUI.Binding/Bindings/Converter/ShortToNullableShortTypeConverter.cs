// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="short"/> to <see cref="Nullable{Int16}"/>.
/// </summary>
public sealed class ShortToNullableShortTypeConverter : IBindingTypeConverter<short, short?>
{
    /// <inheritdoc/>
    public Type FromType => typeof(short);

    /// <inheritdoc/>
    public Type ToType => typeof(short?);

    /// <inheritdoc/>
    public int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public bool TryConvert(short from, object? conversionHint, out short? result)
    {
        result = from;
        return true;
    }

    /// <inheritdoc/>
    public bool TryConvertTyped(object? from, object? conversionHint, [NotNullWhen(true)] out object? result)
    {
        if (from is short value)
        {
            result = (short?)value;
            return true;
        }

        result = null;
        return false;
    }
}

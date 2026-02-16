// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="Nullable{Int64}"/> to <see cref="long"/>.
/// </summary>
/// <remarks>
/// When the nullable value is null, the conversion fails and returns false.
/// </remarks>
public sealed class NullableLongToLongTypeConverter : IBindingTypeConverter<long?, long>
{
    /// <inheritdoc/>
    public Type FromType => typeof(long?);

    /// <inheritdoc/>
    public Type ToType => typeof(long);

    /// <inheritdoc/>
    public int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public bool TryConvert(long? from, object? conversionHint, [NotNullWhen(true)] out long result)
    {
        if (from is null)
        {
            result = default;
            return false;
        }

        result = from.Value;
        return true;
    }

    /// <inheritdoc/>
    public bool TryConvertTyped(object? from, object? conversionHint, [NotNullWhen(true)] out object? result)
    {
        if (from is null)
        {
            result = null;
            return TryConvert(null, conversionHint, out _);
        }

        if (from is long value)
        {
            return TryConvert(value, conversionHint, out var typedResult)
                ? (result = typedResult) is not null
                : (result = default) is null && false;
        }

        result = null;
        return false;
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="Nullable{Int32}"/> to <see cref="int"/>.
/// </summary>
/// <remarks>
/// When the nullable value is null, the conversion fails and returns false.
/// </remarks>
public sealed class NullableIntegerToIntegerTypeConverter : IBindingTypeConverter<int?, int>
{
    /// <inheritdoc/>
    public Type FromType => typeof(int?);

    /// <inheritdoc/>
    public Type ToType => typeof(int);

    /// <inheritdoc/>
    public int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public bool TryConvert(int? from, object? conversionHint, [NotNullWhen(true)] out int result)
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

        if (from is int value)
        {
            result = value;
            return true;
        }

        result = null;
        return false;
    }
}

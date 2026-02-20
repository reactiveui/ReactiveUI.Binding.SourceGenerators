// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="Nullable{Double}"/> to <see cref="double"/>.
/// </summary>
/// <remarks>
/// When the nullable value is null, the conversion fails and returns false.
/// </remarks>
public sealed class NullableDoubleToDoubleTypeConverter : IBindingTypeConverter<double?, double>
{
    /// <inheritdoc/>
    public Type FromType => typeof(double?);

    /// <inheritdoc/>
    public Type ToType => typeof(double);

    /// <inheritdoc/>
    public int GetAffinityForObjects() => 2;

    /// <inheritdoc/>
    public bool TryConvert(double? from, object? conversionHint, [NotNullWhen(true)] out double result)
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
        // Handle null by returning false
        if (from is null)
        {
            result = null;
            return TryConvert(null, conversionHint, out _);
        }

        // Handle double by converting through strongly-typed method
        if (from is double value)
        {
            result = value;
            return true;
        }

        result = null;
        return false;
    }
}

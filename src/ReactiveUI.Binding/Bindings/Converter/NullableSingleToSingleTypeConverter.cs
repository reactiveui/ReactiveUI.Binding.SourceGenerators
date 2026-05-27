// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Converts <see cref="Nullable{Single}"/> to <see cref="float"/>.
/// </summary>
/// <remarks>
/// When the nullable value is null, the conversion fails and returns false.
/// </remarks>
public sealed class NullableSingleToSingleTypeConverter : IBindingTypeConverter<float?, float>
{
    /// <summary>
    /// The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.
    /// </summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public Type FromType => typeof(float?);

    /// <inheritdoc/>
    public Type ToType => typeof(float);

    /// <inheritdoc/>
    public int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public bool TryConvert(float? from, object? conversionHint, [NotNullWhen(true)] out float result)
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
        switch (from)
        {
            case null:
                {
                    result = null;
                    return TryConvert(null, conversionHint, out _);
                }

            case float value:
                {
                    result = value;
                    return true;
                }

            default:
                {
                    result = null;
                    return false;
                }
        }
    }
}

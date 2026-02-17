// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A stub implementation of <see cref="IBindingTypeConverter"/> for testing.
/// </summary>
public class StubBindingTypeConverter : IBindingTypeConverter
{
    private readonly Func<object?, object?, (bool success, object? result)> _tryConvert;

    /// <summary>
    /// Initializes a new instance of the <see cref="StubBindingTypeConverter"/> class.
    /// </summary>
    /// <param name="fromType">The source type.</param>
    /// <param name="toType">The target type.</param>
    /// <param name="tryConvert">The conversion logic.</param>
    public StubBindingTypeConverter(Type fromType, Type toType, Func<object?, object?, (bool success, object? result)> tryConvert)
    {
        FromType = fromType;
        ToType = toType;
        _tryConvert = tryConvert;
    }

    /// <inheritdoc/>
    public Type FromType { get; }

    /// <inheritdoc/>
    public Type ToType { get; }

    /// <inheritdoc/>
    public int GetAffinityForObjects() => 10;

    /// <inheritdoc/>
    public bool TryConvertTyped(object? from, object? conversionHint, out object? result)
    {
        var (success, converted) = _tryConvert(from, conversionHint);
        result = converted;
        return success;
    }
}

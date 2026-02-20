// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A stub implementation of <see cref="IBindingFallbackConverter"/> for testing.
/// </summary>
public class StubFallbackConverter : IBindingFallbackConverter
{
    private readonly Func<Type, object, Type, object?, (bool success, object? result)> _tryConvert;

    /// <summary>
    /// Initializes a new instance of the <see cref="StubFallbackConverter"/> class.
    /// </summary>
    /// <param name="tryConvert">The conversion logic.</param>
    public StubFallbackConverter(Func<Type, object, Type, object?, (bool success, object? result)> tryConvert)
    {
        _tryConvert = tryConvert;
    }

    /// <inheritdoc/>
    public int GetAffinityForObjects(Type fromType, Type toType) => 5;

    /// <inheritdoc/>
    public bool TryConvert(Type fromType, object from, Type toType, object? conversionHint, [NotNullWhen(true)] out object? result)
    {
        var (success, converted) = _tryConvert(fromType, from, toType, conversionHint);
        result = converted;
        return success;
    }
}

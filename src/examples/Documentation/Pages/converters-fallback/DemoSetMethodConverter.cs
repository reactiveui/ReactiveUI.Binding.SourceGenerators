// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Documentation.FallbackConverters;

/// <summary>A demo set-method converter for specialized property-setting operations.</summary>
public sealed class DemoSetMethodConverter : ISetMethodBindingConverter
{
    /// <summary>The affinity score for set-method conversions.</summary>
    private const int SetMethodAffinity = 1;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForObjects(Type? fromType, Type? toType) => SetMethodAffinity;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? PerformSet(object? toTarget, object? newValue, object?[]? arguments) => newValue;
}

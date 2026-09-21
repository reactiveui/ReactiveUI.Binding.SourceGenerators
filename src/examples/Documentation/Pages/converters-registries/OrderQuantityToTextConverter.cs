// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding.Documentation.ConverterRegistries;

/// <summary>Converts an order quantity to display text.</summary>
[DebuggerDisplay("int → string for order quantity")]
public sealed class OrderQuantityToTextConverter : BindingTypeConverter<int, string>
{
    /// <summary>The affinity score for the custom order quantity converter.</summary>
    private const int CustomAffinity = 10;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => CustomAffinity;

    /// <inheritdoc/>
    public override bool TryConvert(int from, object? conversionHint, [MaybeNullWhen(true)] out string result)
    {
        result = from switch
        {
            0 => "Out of stock",
            1 => "1 unit",
            _ => $"{from} units"
        };
        return true;
    }
}

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding.Documentation.ConverterRegistries;

/// <summary>Converts task completion status to a badge label.</summary>
[DebuggerDisplay("bool → string for task status")]
public sealed class TaskStatusBadgeConverter : BindingTypeConverter<bool, string>
{
    /// <summary>The affinity score for the custom task status converter.</summary>
    private const int HigherAffinity = 100;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => HigherAffinity;

    /// <inheritdoc/>
    public override bool TryConvert(bool from, object? conversionHint, [MaybeNullWhen(true)] out string result)
    {
        result = from ? "COMPLETE" : "PENDING";
        return true;
    }
}

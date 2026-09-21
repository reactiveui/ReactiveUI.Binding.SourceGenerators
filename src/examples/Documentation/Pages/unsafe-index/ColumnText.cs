// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Documentation.UnsafeIndex;

/// <summary>Shows the values of several columns as one line of text.</summary>
public static class ColumnText
{
    /// <summary>The text placed between two columns.</summary>
    private const string Separator = " | ";

    /// <summary>Joins the values of the columns into one line.</summary>
    /// <param name="values">The values, one for each column.</param>
    /// <returns>The values separated by a bar.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Join(params ReadOnlySpan<string?> values) => string.Join(Separator, values);
}

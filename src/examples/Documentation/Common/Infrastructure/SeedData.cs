// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Documentation.Infrastructure;

/// <summary>Reads the dates and times that the example databases and servers start with.</summary>
public static class SeedData
{
    /// <summary>Reads a calendar date.</summary>
    /// <param name="text">The date as <c>yyyy-MM-dd</c>.</param>
    /// <returns>The date.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly Date(string text) => DateOnly.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>Reads a moment in time.</summary>
    /// <param name="text">The time as an ISO 8601 text that ends in <c>Z</c>, such as <c>2026-03-03T09:00:00Z</c>.</param>
    /// <returns>The moment, in UTC.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset Instant(string text) =>
        DateTimeOffset.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
}

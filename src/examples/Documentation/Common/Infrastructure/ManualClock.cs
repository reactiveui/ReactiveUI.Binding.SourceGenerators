// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Infrastructure;

/// <summary>A clock that only moves when the example advances it.</summary>
/// <param name="start">The time the clock starts at.</param>
[System.Diagnostics.DebuggerDisplay("Now = {GetUtcNow()}")]
public sealed class ManualClock(DateTimeOffset start) : TimeProvider
{
    /// <summary>The time the clock reports.</summary>
    private DateTimeOffset _now = start;

    /// <summary>Creates a clock that starts at 09:00 UTC on Tuesday 3 March 2026.</summary>
    /// <returns>A new clock.</returns>
    public static ManualClock StartOfWorkingDay() => new(SeedData.Instant("2026-03-03T09:00:00Z"));

    /// <inheritdoc/>
    public override DateTimeOffset GetUtcNow() => _now;

    /// <summary>Moves the clock forward.</summary>
    /// <param name="amount">How far to move the clock.</param>
    public void Advance(TimeSpan amount) => _now += amount;
}

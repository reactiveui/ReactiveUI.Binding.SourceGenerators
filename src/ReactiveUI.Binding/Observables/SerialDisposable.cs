// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// A thread-safe serial disposable that disposes the previous inner disposable when a new one is assigned.
/// Lightweight replacement for <c>System.Reactive.Disposables.SerialDisposable</c>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class SerialDisposable : IDisposable
{
    /// <summary>
    /// The current inner disposable. Swapped atomically when a new value is assigned.
    /// </summary>
    private IDisposable? _current;

    /// <summary>
    /// Guard flag to ensure disposal occurs exactly once (0 = not disposed, 1 = disposed).
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Gets or sets the inner disposable. Setting a new value disposes the previous one.
    /// If the serial disposable itself has been disposed, the new value is disposed immediately.
    /// </summary>
    public IDisposable? Disposable
    {
        get => Volatile.Read(ref _current);
        set
        {
            SwapCurrent(value)?.Dispose();

            if (_disposed == 1)
            {
                DisposeCurrentIfRace();
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (TrySetDisposed())
        {
            TakeCurrent()?.Dispose();
        }
    }

    /// <summary>
    /// Atomically marks this instance as disposed.
    /// </summary>
    /// <returns><see langword="true"/> if this is the first disposal; otherwise <see langword="false"/>.</returns>
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TrySetDisposed() => Interlocked.Exchange(ref _disposed, 1) == 0;

    /// <summary>
    /// Atomically swaps the current disposable with the given value.
    /// </summary>
    /// <param name="value">The new disposable.</param>
    /// <returns>The previous disposable, or <see langword="null"/>.</returns>
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IDisposable? SwapCurrent(IDisposable? value) => Interlocked.Exchange(ref _current, value);

    /// <summary>
    /// Atomically takes the current disposable, returning it exactly once.
    /// </summary>
    /// <returns>The current disposable if present; otherwise <see langword="null"/>.</returns>
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IDisposable? TakeCurrent() => Interlocked.Exchange(ref _current, null);

    /// <summary>
    /// Disposes the current disposable during a race condition in the setter.
    /// This path is reached when <see cref="Dispose"/> runs concurrently between
    /// <see cref="SwapCurrent"/> and the <c>_disposed</c> check, causing
    /// <see cref="TakeCurrent"/> to return null because <see cref="Dispose"/> already took it.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private void DisposeCurrentIfRace() => TakeCurrent()?.Dispose();
}

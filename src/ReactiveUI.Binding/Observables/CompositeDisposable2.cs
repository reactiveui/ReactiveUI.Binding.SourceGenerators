// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// A fixed two-slot composite disposable. Disposes both contained disposables exactly once.
/// Lightweight replacement for <c>System.Reactive.Disposables.CompositeDisposable(d1, d2)</c>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class CompositeDisposable2 : IDisposable
{
    /// <summary>
    /// The first disposable resource.
    /// </summary>
    private readonly IDisposable _d1;

    /// <summary>
    /// The second disposable resource.
    /// </summary>
    private readonly IDisposable _d2;

    /// <summary>
    /// Guard flag to ensure disposal occurs exactly once (0 = not disposed, 1 = disposed).
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeDisposable2"/> class.
    /// </summary>
    /// <param name="d1">The first disposable.</param>
    /// <param name="d2">The second disposable.</param>
    public CompositeDisposable2(IDisposable d1, IDisposable d2)
    {
        ArgumentExceptionHelper.ThrowIfNull(d1);
        ArgumentExceptionHelper.ThrowIfNull(d2);
        _d1 = d1;
        _d2 = d2;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (TrySetDisposed())
        {
            _d1.Dispose();
            _d2.Dispose();
        }
    }

    /// <summary>
    /// Atomically marks this instance as disposed.
    /// </summary>
    /// <returns><see langword="true"/> if this is the first disposal; otherwise <see langword="false"/>.</returns>
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TrySetDisposed() => Interlocked.Exchange(ref _disposed, 1) == 0;
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// A thread-safe <see cref="IDisposable"/> that invokes a delegate exactly once on disposal.
/// Lightweight replacement for <c>System.Reactive.Disposables.Disposable.Create(Action)</c>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ActionDisposable : IDisposable
{
    /// <summary>
    /// The action to invoke on disposal. Set to <see langword="null"/> after first invocation.
    /// </summary>
    private Action? _action;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionDisposable"/> class.
    /// </summary>
    /// <param name="action">The action to invoke on disposal.</param>
    public ActionDisposable(Action action)
    {
        ArgumentExceptionHelper.ThrowIfNull(action);
        _action = action;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        var action = TryTakeAction();
        action?.Invoke();
    }

    /// <summary>
    /// Atomically takes the action, returning it exactly once. Subsequent calls return <see langword="null"/>.
    /// </summary>
    /// <returns>The action if this is the first call; otherwise <see langword="null"/>.</returns>
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Action? TryTakeAction() => Interlocked.Exchange(ref _action, null);
}

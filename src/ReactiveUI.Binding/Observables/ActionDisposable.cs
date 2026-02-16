// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Threading;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// A thread-safe <see cref="IDisposable"/> that invokes a delegate exactly once on disposal.
/// Lightweight replacement for <c>System.Reactive.Disposables.Disposable.Create(Action)</c>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ActionDisposable : IDisposable
{
    private Action? _action;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionDisposable"/> class.
    /// </summary>
    /// <param name="action">The action to invoke on disposal.</param>
    public ActionDisposable(Action action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Interlocked.Exchange(ref _action, null)?.Invoke();
    }
}

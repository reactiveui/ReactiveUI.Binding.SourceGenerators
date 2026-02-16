// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// A singleton <see cref="IDisposable"/> that performs no action when disposed.
/// Zero-allocation replacement for <c>System.Reactive.Disposables.Disposable.Empty</c>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class EmptyDisposable : IDisposable
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly EmptyDisposable Instance = new();

    private EmptyDisposable()
    {
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Intentionally empty.
    }
}

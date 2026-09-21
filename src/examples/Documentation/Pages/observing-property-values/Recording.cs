// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.ObservingPropertyValues;

/// <summary>Subscribes to a stream and keeps every value it delivers, in order.</summary>
/// <typeparam name="T">The type of the delivered values.</typeparam>
[System.Diagnostics.DebuggerDisplay("Count = {Count}")]
public sealed class Recording<T> : IDisposable
{
    /// <summary>The values delivered so far.</summary>
    private readonly List<T> _values = [];

    /// <summary>The subscription that fills <see cref="_values"/>.</summary>
    private readonly IDisposable _subscription;

    /// <summary>Initializes a new instance of the <see cref="Recording{T}"/> class and subscribes to the source.</summary>
    /// <param name="source">The stream to record.</param>
    public Recording(IObservable<T> source) => _subscription = source.Subscribe(_values.Add);

    /// <summary>Gets the values delivered so far, oldest first.</summary>
    public IReadOnlyList<T> Values => _values;

    /// <summary>Gets the number of values delivered so far.</summary>
    public int Count => _values.Count;

    /// <summary>Gets the value delivered last.</summary>
    public T Latest => _values[^1];

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _subscription.Dispose();
}

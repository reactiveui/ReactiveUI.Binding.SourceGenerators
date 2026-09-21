// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Documentation.MechanismsObservables;

/// <summary>Keeps every value a stream delivers, and whether the stream completed or failed.</summary>
/// <typeparam name="T">The type of the delivered values.</typeparam>
[System.Diagnostics.DebuggerDisplay("Count = {Values.Count}, IsCompleted = {IsCompleted}")]
public sealed class CapturingObserver<T> : IObserver<T>
{
    /// <summary>The values delivered so far.</summary>
    private readonly List<T> _values = [];

    /// <summary>Gets the values delivered so far, oldest first.</summary>
    public IReadOnlyList<T> Values => _values;

    /// <summary>Gets a value indicating whether the stream completed.</summary>
    public bool IsCompleted { get; private set; }

    /// <summary>Gets the error the stream failed with, or <see langword="null"/> when it did not fail.</summary>
    public Exception? Error { get; private set; }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnNext(T value) => _values.Add(value);

    /// <inheritdoc/>
    public void OnError(Exception error) => Error = error;

    /// <inheritdoc/>
    public void OnCompleted() => IsCompleted = true;
}

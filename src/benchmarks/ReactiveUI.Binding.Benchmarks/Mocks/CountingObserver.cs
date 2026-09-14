// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>Counts what a binding reported without allocating per change.</summary>
internal sealed class CountingObserver : IObserver<BindingChange>
{
    /// <summary>Gets how many changes were reported.</summary>
    public int Count { get; private set; }

    /// <inheritdoc/>
    public void OnNext(BindingChange value) => Count++;

    /// <inheritdoc/>
    public void OnError(Exception error)
    {
    }

    /// <inheritdoc/>
    public void OnCompleted()
    {
    }
}

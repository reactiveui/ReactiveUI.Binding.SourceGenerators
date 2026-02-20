// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A test-only IObservable that stores its observer and returns a no-op disposable,
/// allowing tests to call OnNext/OnError/OnCompleted on the observer even after the
/// subscription has been "disposed" by the subscriber.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
internal sealed class ManualObservable<T> : IObservable<T>
{
    /// <summary>
    /// Gets the observer that was passed to Subscribe.
    /// </summary>
    public IObserver<T>? Observer { get; private set; }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        Observer = observer;
        return new NoOpDisposable();
    }

    private sealed class NoOpDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>Lightweight Skip operator that suppresses the first N elements. Replacement for <c>System.Reactive.Linq.Observable.Skip</c>.</summary>
/// <typeparam name="T">The element type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class SkipObservable<T> : IObservable<T>
{
    /// <summary>The upstream source observable.</summary>
    private readonly IObservable<T> _source;

    /// <summary>The number of elements to skip.</summary>
    private readonly int _count;

    /// <summary>Initializes a new instance of the <see cref="SkipObservable{T}"/> class.</summary>
    /// <param name="source">The source observable.</param>
    /// <param name="count">The number of elements to skip.</param>
    public SkipObservable(IObservable<T> source, int count)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        _source = source;
        _count = count;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return _source.Subscribe(new SkipObserver(observer, _count));
    }

    /// <summary>Observer that skips the first N elements before forwarding to the downstream observer.</summary>
    /// <param name="observer">The downstream observer.</param>
    /// <param name="count">The number of elements to skip.</param>
    private sealed class SkipObserver(IObserver<T> observer, int count) : IObserver<T>
    {
        /// <summary>The downstream observer.</summary>
        private readonly IObserver<T> _observer = observer;

        /// <summary>The number of elements still to skip.</summary>
        private int _remaining = count;

        /// <inheritdoc/>
        public void OnNext(T value)
        {
            if (_remaining > 0)
            {
                _remaining--;
                return;
            }

            _observer.OnNext(value);
        }

        /// <inheritdoc/>
        public void OnError(Exception error) => _observer.OnError(error);

        /// <inheritdoc/>
        public void OnCompleted() => _observer.OnCompleted();
    }
}

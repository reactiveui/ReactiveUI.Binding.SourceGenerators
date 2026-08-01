// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>
/// An observable that never produces any notification (no value, completion, or error).
/// Singleton per <typeparamref name="T"/>. Lightweight replacement for <c>Observable.Never&lt;T&gt;()</c>.
/// </summary>
/// <typeparam name="T">The element type the observable would have produced.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class NeverObservable<T> : IObservable<T>
{
    /// <summary>Gets the singleton instance for this element type.</summary>
    public static readonly NeverObservable<T> Instance = new();

    /// <summary>Initializes a new instance of the <see cref="NeverObservable{T}"/> class. Prevents external instantiation. Use <see cref="Instance"/> instead.</summary>
    private NeverObservable()
    {
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return EmptyDisposable.Instance;
    }
}

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
/// Lightweight extension methods for <see cref="IObservable{T}"/>.
/// These are fully-qualified in generated code to avoid conflicts with
/// <c>System.Reactive.Linq</c> extension methods when both namespaces are in scope.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RxBindingExtensions
{
    /// <summary>Subscribes to the observable with an action for OnNext.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source observable.</param>
    /// <param name="onNext">The action to invoke on each element.</param>
    /// <returns>A disposable that unsubscribes when disposed.</returns>
    public static IDisposable Subscribe<T>(IObservable<T> source, Action<T> onNext)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(onNext);

        return source.Subscribe(new ActionObserver<T>(onNext));
    }

    /// <summary>Skips the first <paramref name="count"/> elements.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source observable.</param>
    /// <param name="count">The number of elements to skip.</param>
    /// <returns>An observable that skips elements.</returns>
    public static IObservable<T> Skip<T>(IObservable<T> source, int count) => new SkipObservable<T>(source, count);

    /// <summary>Merges multiple observables into one.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="sources">The source observables.</param>
    /// <returns>A merged observable.</returns>
    public static IObservable<T> Merge<T>(params IObservable<T>[] sources) => new MergeObservable<T>(sources);

    /// <summary>Provides Switch extension members for <paramref name="source"/>.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source observable of observables.</param>
    extension<T>(IObservable<IObservable<T>> source)
    {
        /// <summary>Flattens an observable of observables by subscribing to the most recent inner observable.</summary>
        /// <returns>A flattened observable.</returns>
        public IObservable<T> Switch() => new SwitchObservable<T>(source);
    }

    /// <summary>Provides Select and DistinctUntilChanged extension members for <paramref name="source"/>.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source observable.</param>
    extension<T>(IObservable<T> source)
    {
        /// <summary>Projects each element using a selector function.</summary>
        /// <typeparam name="TResult">The projected element type.</typeparam>
        /// <param name="selector">The projection function.</param>
        /// <returns>A projected observable.</returns>
        public IObservable<TResult> Select<TResult>(Func<T, TResult> selector) =>
            new SelectObservable<T, TResult>(source, selector);

        /// <summary>Suppresses consecutive duplicate values.</summary>
        /// <returns>An observable with distinct consecutive values.</returns>
        public IObservable<T> DistinctUntilChanged() =>
            new DistinctUntilChangedObservable<T>(source);
    }

    /// <summary>An observer that delegates <see cref="IObserver{T}.OnNext"/> to an <see cref="Action{T}"/>. Errors and completion are intentionally ignored for binding scenarios.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    [ExcludeFromCodeCoverage]
    private sealed class ActionObserver<T> : IObserver<T>
    {
        /// <summary>The action to invoke for each element.</summary>
        private readonly Action<T> _onNext;

        /// <summary>Initializes a new instance of the <see cref="ActionObserver{T}"/> class.</summary>
        /// <param name="onNext">The action to invoke for each element.</param>
        public ActionObserver(Action<T> onNext) => _onNext = onNext;

        /// <inheritdoc/>
        public void OnNext(T value) => _onNext(value);

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
            // In binding scenarios, errors should not crash the app.
            // The caller is responsible for error handling if needed.
        }

        /// <inheritdoc/>
        public void OnCompleted()
        {
            // Completion is intentionally ignored for binding scenarios.
        }
    }
}

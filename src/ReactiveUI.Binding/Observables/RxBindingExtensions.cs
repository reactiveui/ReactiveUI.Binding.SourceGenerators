// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Lightweight extension methods for <see cref="IObservable{T}"/>.
/// These are fully-qualified in generated code to avoid conflicts with
/// <c>System.Reactive.Linq</c> extension methods when both namespaces are in scope.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RxBindingExtensions
{
    /// <summary>
    /// Subscribes to the observable with an action for OnNext.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source observable.</param>
    /// <param name="onNext">The action to invoke on each element.</param>
    /// <returns>A disposable that unsubscribes when disposed.</returns>
    public static IDisposable Subscribe<T>(IObservable<T> source, Action<T> onNext)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (onNext is null)
        {
            throw new ArgumentNullException(nameof(onNext));
        }

        return source.Subscribe(new ActionObserver<T>(onNext));
    }

    /// <summary>
    /// Projects each element using a selector function.
    /// </summary>
    /// <typeparam name="TSource">The source element type.</typeparam>
    /// <typeparam name="TResult">The projected element type.</typeparam>
    /// <param name="source">The source observable.</param>
    /// <param name="selector">The projection function.</param>
    /// <returns>A projected observable.</returns>
    public static IObservable<TResult> Select<TSource, TResult>(this IObservable<TSource> source, Func<TSource, TResult> selector)
    {
        return new SelectObservable<TSource, TResult>(source, selector);
    }

    /// <summary>
    /// Flattens an observable of observables by subscribing to the most recent inner observable.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source observable of observables.</param>
    /// <returns>A flattened observable.</returns>
    public static IObservable<T> Switch<T>(this IObservable<IObservable<T>> source)
    {
        return new SwitchObservable<T>(source);
    }

    /// <summary>
    /// Skips the first <paramref name="count"/> elements.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source observable.</param>
    /// <param name="count">The number of elements to skip.</param>
    /// <returns>An observable that skips elements.</returns>
    public static IObservable<T> Skip<T>(IObservable<T> source, int count)
    {
        return new SkipObservable<T>(source, count);
    }

    /// <summary>
    /// Suppresses consecutive duplicate values.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source observable.</param>
    /// <returns>An observable with distinct consecutive values.</returns>
    public static IObservable<T> DistinctUntilChanged<T>(this IObservable<T> source)
    {
        return new DistinctUntilChangedObservable<T>(source);
    }

    /// <summary>
    /// Merges multiple observables into one.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="sources">The source observables.</param>
    /// <returns>A merged observable.</returns>
    public static IObservable<T> Merge<T>(params IObservable<T>[] sources)
    {
        return new MergeObservable<T>(sources);
    }

    [ExcludeFromCodeCoverage]
    private sealed class ActionObserver<T> : IObserver<T>
    {
        private readonly Action<T> _onNext;

        public ActionObserver(Action<T> onNext)
        {
            _onNext = onNext;
        }

        public void OnNext(T value) => _onNext(value);

        public void OnError(Exception error)
        {
            // In binding scenarios, errors should not crash the app.
            // The caller is responsible for error handling if needed.
        }

        public void OnCompleted()
        {
            // Completion is intentionally ignored for binding scenarios.
        }
    }
}

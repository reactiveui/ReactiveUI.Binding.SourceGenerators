// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>The changes a two-way binding actually wrote, handed to whoever subscribes to the binding.</summary>
/// <remarks>
/// A two-way binding applies each change once and reports the same change to its subscribers, so both have to
/// come from one place. Projecting the two observed sides a second time instead would attach another set of
/// property handlers for every subscriber, and would report values the binding weighed and refused to write -
/// the echo of a write is exactly what a subscriber is trying to tell apart from an edit.
/// <para>
/// Observers are kept in an array that is replaced rather than mutated, so a change already being delivered
/// walks the set it started with and a subscription taken during delivery cannot disturb it.
/// </para>
/// </remarks>
[DebuggerDisplay("AppliedChangeObservable: {_observers.Length} observer(s)")]
public sealed class AppliedChangeObservable : IObservable<BindingChange>
{
    /// <summary>The observers a change is delivered to, replaced whenever the set changes.</summary>
    private IObserver<BindingChange>[] _observers = [];

    /// <summary>Reports a change the binding has written.</summary>
    /// <param name="value">The change that was written.</param>
    public void OnNext(BindingChange value)
    {
        var observers = Volatile.Read(ref _observers);
        for (var i = 0; i < observers.Length; i++)
        {
            observers[i].OnNext(value);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="observer"/> is null.</exception>
    public IDisposable Subscribe(IObserver<BindingChange> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        IObserver<BindingChange>[] updated;
        IObserver<BindingChange>[] current;

        do
        {
            current = Volatile.Read(ref _observers);
            updated = new IObserver<BindingChange>[current.Length + 1];
            Array.Copy(current, updated, current.Length);
            updated[current.Length] = observer;
        }
        while (!ReferenceEquals(Interlocked.CompareExchange(ref _observers, updated, current), current));

        return new Subscription(this, observer);
    }

    /// <summary>Drops one observer without disturbing a change already being delivered.</summary>
    /// <param name="observer">The observer to drop.</param>
    private void Remove(IObserver<BindingChange> observer)
    {
        IObserver<BindingChange>[] current;
        IObserver<BindingChange>[] updated;

        do
        {
            current = Volatile.Read(ref _observers);
            var index = Array.IndexOf(current, observer);
            if (index < 0)
            {
                return;
            }

            updated = new IObserver<BindingChange>[current.Length - 1];
            Array.Copy(current, updated, index);
            Array.Copy(current, index + 1, updated, index, current.Length - index - 1);
        }
        while (!ReferenceEquals(Interlocked.CompareExchange(ref _observers, updated, current), current));
    }

    /// <summary>Releases one observer's place in the change stream.</summary>
    /// <param name="parent">The stream subscribed to.</param>
    /// <param name="observer">The observer to drop on disposal.</param>
    private sealed class Subscription(AppliedChangeObservable parent, IObserver<BindingChange> observer) : IDisposable
    {
        /// <summary>The observer to drop, cleared once dropped so disposing twice drops one place.</summary>
        private IObserver<BindingChange>? _observer = observer;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _observer, null) is not { } dropped)
            {
                return;
            }

            parent.Remove(dropped);
        }
    }
}

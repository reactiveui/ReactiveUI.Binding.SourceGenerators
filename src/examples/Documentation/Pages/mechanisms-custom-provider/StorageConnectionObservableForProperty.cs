// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Primitives.Advanced;

namespace ReactiveUI.Binding.Documentation.MechanismsCustomProvider;

/// <summary>
/// Observes <see cref="StorageConnection.State"/> through the plain <see cref="StorageConnection.StateChanged"/>
/// event. The connection implements no notification interface, so no built-in mechanism can observe it.
/// </summary>
[System.Diagnostics.DebuggerDisplay("Subscriptions = {SubscriptionCount}, Notifications = {NotificationCount}")]
public sealed class StorageConnectionObservableForProperty : ICreatesObservableForProperty
{
    /// <summary>Gets the number of observations this provider has created.</summary>
    public int SubscriptionCount { get; private set; }

    /// <summary>Gets the number of <see cref="StorageConnection.StateChanged"/> events this provider has passed on.</summary>
    public int NotificationCount { get; private set; }

    /// <inheritdoc/>
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged) =>
        !beforeChanged && type == typeof(StorageConnection) && propertyName == nameof(StorageConnection.State)
            ? BindingAffinity.WinFormsEvent
            : 0;

    /// <inheritdoc/>
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        Expression expression,
        string propertyName,
        bool beforeChanged,
        bool suppressWarnings)
    {
        if (beforeChanged || sender is not StorageConnection connection)
        {
            return ImmutableNeverSignal<IObservedChange<object, object?>>.Instance;
        }

        SubscriptionCount++;
        return new StateChangedObservable(this, connection, expression);
    }

    /// <summary>Reports each <see cref="StorageConnection.StateChanged"/> event as an observed change.</summary>
    /// <param name="owner">The provider that counts the notifications.</param>
    /// <param name="connection">The connection being observed.</param>
    /// <param name="expression">The property expression the change reports.</param>
    private sealed class StateChangedObservable(
        StorageConnectionObservableForProperty owner,
        StorageConnection connection,
        Expression expression) : IObservable<IObservedChange<object, object?>>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable Subscribe(IObserver<IObservedChange<object, object?>> observer) =>
            new StateChangedSubscription(owner, connection, expression, observer);
    }

    /// <summary>Holds the <see cref="StorageConnection.StateChanged"/> handler of one observer.</summary>
    [System.Diagnostics.DebuggerDisplay("Connection = {_connection}")]
    private sealed class StateChangedSubscription : IDisposable
    {
        /// <summary>The provider that counts the notifications.</summary>
        private readonly StorageConnectionObservableForProperty _owner;

        /// <summary>The connection being observed.</summary>
        private readonly StorageConnection _connection;

        /// <summary>The property expression each change reports.</summary>
        private readonly Expression _expression;

        /// <summary>The observer that receives the changes.</summary>
        private readonly IObserver<IObservedChange<object, object?>> _observer;

        /// <summary>Initializes a new instance of the <see cref="StateChangedSubscription"/> class and attaches the handler.</summary>
        /// <param name="owner">The provider that counts the notifications.</param>
        /// <param name="connection">The connection being observed.</param>
        /// <param name="expression">The property expression each change reports.</param>
        /// <param name="observer">The observer that receives the changes.</param>
        public StateChangedSubscription(
            StorageConnectionObservableForProperty owner,
            StorageConnection connection,
            Expression expression,
            IObserver<IObservedChange<object, object?>> observer)
        {
            _owner = owner;
            _connection = connection;
            _expression = expression;
            _observer = observer;
            connection.StateChanged += OnStateChanged;
        }

        /// <inheritdoc/>
        public void Dispose() => _connection.StateChanged -= OnStateChanged;

        /// <summary>Passes the new state to the observer.</summary>
        /// <param name="sender">The connection.</param>
        /// <param name="e">The event data.</param>
        private void OnStateChanged(object? sender, EventArgs e)
        {
            _owner.NotificationCount++;
            _observer.OnNext(new ObservedChange<object, object?>(_connection, _expression, _connection.State));
        }
    }
}

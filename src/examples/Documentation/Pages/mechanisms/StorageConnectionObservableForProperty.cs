// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Primitives.Advanced;

namespace ReactiveUI.Binding.Documentation.Mechanisms;

/// <summary>
/// Observes <see cref="StorageConnection.State"/> through the plain <see cref="StorageConnection.StateChanged"/>
/// event. The connection implements no notification interface, so no built-in mechanism can observe it.
/// </summary>
public sealed class StorageConnectionObservableForProperty : ICreatesObservableForProperty
{
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

        Console.WriteLine("The connection state is observed by the StateChanged provider");
        return new StateChangedObservable(connection, expression);
    }

    /// <summary>Reports each <see cref="StorageConnection.StateChanged"/> event as an observed change.</summary>
    /// <param name="connection">The connection being observed.</param>
    /// <param name="expression">The property expression the change reports.</param>
    private sealed class StateChangedObservable(StorageConnection connection, Expression expression) : IObservable<IObservedChange<object, object?>>
    {
        /// <inheritdoc/>
                public IDisposable Subscribe(IObserver<IObservedChange<object, object?>> observer) =>
            new StateChangedSubscription(connection, expression, observer);
    }

    /// <summary>Holds the <see cref="StorageConnection.StateChanged"/> handler of one observer.</summary>
    [System.Diagnostics.DebuggerDisplay("Connection = {_connection}")]
    private sealed class StateChangedSubscription : IDisposable
    {
        /// <summary>The connection being observed.</summary>
        private readonly StorageConnection _connection;

        /// <summary>The property expression each change reports.</summary>
        private readonly Expression _expression;

        /// <summary>The observer that receives the changes.</summary>
        private readonly IObserver<IObservedChange<object, object?>> _observer;

        /// <summary>Initializes a new instance of the <see cref="StateChangedSubscription"/> class and attaches the handler.</summary>
        /// <param name="connection">The connection being observed.</param>
        /// <param name="expression">The property expression each change reports.</param>
        /// <param name="observer">The observer that receives the changes.</param>
        public StateChangedSubscription(
            StorageConnection connection,
            Expression expression,
            IObserver<IObservedChange<object, object?>> observer)
        {
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
        private void OnStateChanged(object? sender, EventArgs e) =>
            _observer.OnNext(new ObservedChange<object, object?>(_connection, _expression, _connection.State));
    }
}

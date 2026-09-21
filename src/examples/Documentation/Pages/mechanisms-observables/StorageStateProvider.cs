// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Documentation.CloudStorage;

namespace ReactiveUI.Binding.Documentation.MechanismsObservables;

/// <summary>Observes <see cref="StorageConnection.State"/> through the plain <see cref="StorageConnection.StateChanged"/> event.</summary>
[System.Diagnostics.DebuggerDisplay("Notifications = {NotificationCount}")]
public sealed class StorageStateProvider : ICreatesObservableForProperty
{
    /// <summary>Gets the number of <see cref="StorageConnection.StateChanged"/> events this provider has passed on.</summary>
    public int NotificationCount { get; private set; }

    /// <inheritdoc/>
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged) =>
        !beforeChanged && type == typeof(StorageConnection) && propertyName == nameof(StorageConnection.State)
            ? BindingAffinity.WinFormsEvent
            : 0;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        Expression expression,
        string propertyName,
        bool beforeChanged,
        bool suppressWarnings) =>
        new StateChangedObservable(this, (StorageConnection)sender, expression);

    /// <summary>Reports each <see cref="StorageConnection.StateChanged"/> event as an observed change.</summary>
    /// <param name="owner">The provider that counts the notifications.</param>
    /// <param name="connection">The connection being observed.</param>
    /// <param name="expression">The property expression each change reports.</param>
    private sealed class StateChangedObservable(
        StorageStateProvider owner,
        StorageConnection connection,
        Expression expression) : IObservable<IObservedChange<object, object?>>
    {
        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<IObservedChange<object, object?>> observer)
        {
            EventHandler handler = (_, _) =>
            {
                owner.NotificationCount++;
                observer.OnNext(new ObservedChange<object, object?>(connection, expression, connection.State));
            };

            connection.StateChanged += handler;
            return new Detach(connection, handler);
        }
    }

    /// <summary>Removes a <see cref="StorageConnection.StateChanged"/> handler.</summary>
    /// <param name="connection">The connection the handler is attached to.</param>
    /// <param name="handler">The handler to remove.</param>
    private sealed class Detach(StorageConnection connection, EventHandler handler) : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose() => connection.StateChanged -= handler;
    }
}

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Primitives;
using Splat;

namespace ReactiveUI.Binding.Documentation.MechanismsCustomProvider;

/// <summary>
/// Shows how a provider for <see cref="StorageConnection"/> is registered and how every part of the runtime
/// (the generated bindings, the chain engine and the plugin observables) consults it.
/// </summary>
public static class MechanismsCustomProviderExamples
{
    /// <summary>The number of observation providers once the core services and the storage provider are registered.</summary>
    private const int RegisteredProviderCount = 3;

    /// <summary>The number of state changes the direct-call example raises.</summary>
    private const int ConnectionChangeCount = 2;

    /// <summary>The number of state changes the reconnect example raises.</summary>
    private const int ReconnectChangeCount = 3;

    /// <summary>The number of values the chain example reports: the current state and two changes.</summary>
    private const int ChainChangeCount = 3;

    /// <summary>The name of the observed property.</summary>
    private const string StatePropertyName = nameof(StorageConnection.State);

    /// <summary>Registers the provider through the builder and clears the cached votes so the next observation sees it.</summary>
    /// <param name="provider">The provider to register.</param>
    /// <returns>The built application, which holds the registered providers.</returns>
    public static IReactiveUIBindingInstance RegisterProviderThroughBuilder(StorageConnectionObservableForProperty provider)
    {
        SampleCheck.Equal(false, ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(StorageConnection), StatePropertyName, BindingAffinity.Fallback, false));

        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        var app = builder
            .WithCoreServices()
            .WithRegistration(resolver => resolver.RegisterConstant<ICreatesObservableForProperty>(provider))
            .BuildApp();

        ObservationAffinityChecker.Refresh();

        List<ICreatesObservableForProperty> providers = [.. AppLocator.Current.GetServices<ICreatesObservableForProperty>()];

        SampleCheck.Equal(RegisteredProviderCount, providers.Count);
        SampleCheck.Equal(true, providers.Contains(provider));
        SampleCheck.Equal(true, ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(StorageConnection), StatePropertyName, BindingAffinity.Fallback, false));
        return app;
    }

    /// <summary>
    /// Observes the connection at run time; each state change reaches the subscriber through the provider. The
    /// generator writes no dispatch for a type with no built-in mechanism, so this observation is the run-time one.
    /// </summary>
    /// <param name="browser">The browser whose connection is observed.</param>
    /// <param name="storage">The storage service that owns the connection.</param>
    /// <param name="provider">The registered provider.</param>
    /// <returns>A task that completes when the reconnect has finished.</returns>
    public static async Task ObserveConnectionThroughProvider(
        StorageBrowserViewModel browser,
        InMemoryObjectStorage storage,
        StorageConnectionObservableForProperty provider)
    {
        List<ConnectionState> states = [];

        using (browser.Connection.ObservableForProperty(x => x.State, false).Subscribe(change => states.Add(change.Value)))
        {
            storage.Disconnect();
            storage.Gate.Hold();
            browser.ConnectCommand.Execute(null);
            storage.Gate.ReleaseAll();
            await browser.ConnectCommand.Completion.ConfigureAwait(false);
        }

        SampleCheck.SequenceEqual(
            [ConnectionState.Connected, ConnectionState.Disconnected, ConnectionState.Connecting, ConnectionState.Connected],
            states);
        SampleCheck.Equal(1, provider.SubscriptionCount);
        SampleCheck.Equal(ReconnectChangeCount, provider.NotificationCount);
    }

    /// <summary>Observes the connection with <c>WhenChanged</c>; the registered provider delivers each state change.</summary>
    /// <param name="browser">The browser whose connection is observed.</param>
    /// <param name="storage">The storage service that owns the connection.</param>
    /// <param name="provider">The registered provider.</param>
    public static void ObserveConnectionWithWhenChanged(
        StorageBrowserViewModel browser,
        InMemoryObjectStorage storage,
        StorageConnectionObservableForProperty provider)
    {
        var notificationsBefore = provider.NotificationCount;
        List<ConnectionState> states = [];

        using (browser.Connection.WhenChanged(x => x.State).Subscribe(states.Add))
        {
            storage.Disconnect();
            storage.Connection.State = ConnectionState.Connected;
        }

        SampleCheck.SequenceEqual([ConnectionState.Connected, ConnectionState.Disconnected, ConnectionState.Connected], states);
        SampleCheck.Equal(ConnectionChangeCount, provider.NotificationCount - notificationsBefore);
    }

    /// <summary>Calls the provider directly, with the overloads that fill in the after-change defaults.</summary>
    /// <param name="connection">The connection to observe.</param>
    /// <param name="provider">The registered provider.</param>
    public static void CallProviderThroughMixins(StorageConnection connection, StorageConnectionObservableForProperty provider)
    {
        Expression<Func<StorageConnection, ConnectionState>> property = x => x.State;

        SampleCheck.Equal(BindingAffinity.WinFormsEvent, provider.GetAffinityForObject(typeof(StorageConnection), StatePropertyName));
        SampleCheck.Equal(0, provider.GetAffinityForObject(typeof(StorageConnection), nameof(StorageConnection.Endpoint)));
        SampleCheck.Equal(0, provider.GetAffinityForObject(typeof(StorageConnection), StatePropertyName, true));

        List<IObservedChange<object, object?>> after = [];
        using (provider.GetNotificationForProperty(connection, property.Body, StatePropertyName).Subscribe(after.Add))
        {
            connection.State = ConnectionState.Connecting;
            connection.State = ConnectionState.Connected;
        }

        SampleCheck.Equal(ConnectionChangeCount, after.Count);
        SampleCheck.Equal(true, ReferenceEquals(connection, after[0].Sender));
        SampleCheck.Equal(ConnectionState.Connecting, after[0].Value);
        SampleCheck.Equal(ConnectionState.Connected, after[1].Value);

        List<IObservedChange<object, object?>> before = [];
        using (provider.GetNotificationForProperty(connection, property.Body, StatePropertyName, true).Subscribe(before.Add))
        {
            connection.State = ConnectionState.Connecting;
            connection.State = ConnectionState.Connected;
        }

        SampleCheck.Equal(0, before.Count);
    }

    /// <summary>Asks which registered provider outranks a mechanism the generator picked, at several generated scores.</summary>
    /// <param name="provider">The registered provider.</param>
    public static void FindProviderThatOutranksGeneratedMechanism(StorageConnectionObservableForProperty provider)
    {
        var connectionType = typeof(StorageConnection);

        SampleCheck.Equal(true, ObservationAffinityChecker.HasHigherAffinityPlugin(connectionType, StatePropertyName, BindingAffinity.Fallback, false));
        SampleCheck.Equal(true, ReferenceEquals(provider, ObservationAffinityChecker.FindHigherAffinityPlugin(connectionType, StatePropertyName, BindingAffinity.Explicit, false)));

        // A tie goes to the generated mechanism, and so does a higher generated score.
        SampleCheck.Equal(false, ObservationAffinityChecker.HasHigherAffinityPlugin(connectionType, StatePropertyName, BindingAffinity.WinFormsEvent, false));
        SampleCheck.Equal(true, ObservationAffinityChecker.FindHigherAffinityPlugin(connectionType, StatePropertyName, BindingAffinity.Kvo, false) is null);

        // The provider answers for one property and only after a change.
        SampleCheck.Equal(true, ObservationAffinityChecker.FindHigherAffinityPlugin(connectionType, nameof(StorageConnection.Endpoint), BindingAffinity.Fallback, false) is null);
        SampleCheck.Equal(true, ObservationAffinityChecker.FindHigherAffinityPlugin(connectionType, StatePropertyName, BindingAffinity.Fallback, true) is null);
    }

    /// <summary>Observes the state through <see cref="PluginPropertyObservable{T}"/>, which reads each value with a getter.</summary>
    /// <param name="connection">The connection to observe.</param>
    /// <param name="provider">The registered provider.</param>
    public static void ObserveThroughPluginPropertyObservable(StorageConnection connection, StorageConnectionObservableForProperty provider)
    {
        Expression<Func<StorageConnection, ConnectionState>> property = x => x.State;
        PluginPropertyObservable<ConnectionState> observable = new(
            provider,
            connection,
            property.Body,
            StatePropertyName,
            static source => ((StorageConnection)source).State,
            false,
            true);
        List<ConnectionState> states = [];

        using (observable.Subscribe(states.Add))
        {
            connection.State = ConnectionState.Disconnected;
            connection.State = ConnectionState.Connected;
        }

        SampleCheck.SequenceEqual([ConnectionState.Connected, ConnectionState.Disconnected, ConnectionState.Connected], states);
    }

    /// <summary>Lets <see cref="PluginObservationSource"/> choose between the generated observation and the provider.</summary>
    /// <param name="connection">The connection to observe.</param>
    public static void ChooseBetweenGeneratedObservationAndProvider(StorageConnection connection)
    {
        Expression<Func<StorageConnection, ConnectionState>> property = x => x.State;
        UnchangingPropertyObservable<ConnectionState> generated = new(connection.State);

        var lowGenerated = PluginObservationSource.Choose(
            connection,
            property.Body,
            StatePropertyName,
            false,
            BindingAffinity.Fallback,
            static source => ((StorageConnection)source).State,
            generated);
        var highGenerated = PluginObservationSource.Choose(
            connection,
            property.Body,
            StatePropertyName,
            false,
            BindingAffinity.Kvo,
            static source => ((StorageConnection)source).State,
            generated);

        SampleCheck.Equal(true, lowGenerated is PluginPropertyObservable<ConnectionState>);
        SampleCheck.Equal(true, ReferenceEquals(generated, highGenerated));

        List<ConnectionState> states = [];
        using (lowGenerated.Subscribe(states.Add))
        {
            connection.State = ConnectionState.Disconnected;
            connection.State = ConnectionState.Connected;
        }

        SampleCheck.SequenceEqual([ConnectionState.Connected, ConnectionState.Disconnected, ConnectionState.Connected], states);
    }

    /// <summary>Walks <c>x.Connection.State</c> with the chain engine; the last link is observed through the provider.</summary>
    /// <param name="browser">The browser whose connection is observed.</param>
    public static void ObserveThroughExpressionChain(StorageBrowserViewModel browser)
    {
        Expression<Func<StorageBrowserViewModel, ConnectionState>> property = x => x.Connection.State;
        Expression[] links = [.. Reflection.Rewrite(property.Body).GetExpressionChain()];
        ExpressionChainParameters<StorageBrowserViewModel> parameters = new(browser, property.Body, links, false, false, true, true);
        ExpressionChainSink<StorageBrowserViewModel, ConnectionState> sink = new(
            parameters.Source,
            parameters.Expression,
            parameters.Links,
            parameters.BeforeChange,
            parameters.SkipInitial,
            parameters.IsDistinct,
            parameters.SuppressWarnings);
        List<IObservedChange<StorageBrowserViewModel, ConnectionState>> changes = [];

        using (sink.Subscribe(changes.Add))
        {
            browser.Connection.State = ConnectionState.Disconnected;
            browser.Connection.State = ConnectionState.Connected;
        }

        SampleCheck.Equal(ChainChangeCount, changes.Count);
        SampleCheck.Equal(ConnectionState.Connected, changes[0].Value);
        SampleCheck.Equal(ConnectionState.Disconnected, changes[1].Value);
        SampleCheck.Equal(ConnectionState.Connected, changes[2].Value);
        SampleCheck.Equal(true, ReferenceEquals(browser, changes[0].Sender));
    }

    /// <summary>Wraps the provider's notifications in <see cref="ObservableForPropertySink{TSender, TValue}"/>, which reads the value on each one.</summary>
    /// <param name="connection">The connection to observe.</param>
    /// <param name="provider">The registered provider.</param>
    public static void ObserveThroughObservableForPropertySink(StorageConnection connection, StorageConnectionObservableForProperty provider)
    {
        Expression<Func<StorageConnection, ConnectionState>> property = x => x.State;
        var notifications = provider.GetNotificationForProperty(connection, property.Body, StatePropertyName);
        ObservableForPropertySink<StorageConnection, ConnectionState> sink = new(
            connection,
            property.Body,
            notifications,
            () => connection.State,
            false,
            true);
        List<ConnectionState> states = [];

        using (sink.Subscribe(change => states.Add(change.Value)))
        {
            connection.State = ConnectionState.Disconnected;
            connection.State = ConnectionState.Connected;
        }

        SampleCheck.SequenceEqual([ConnectionState.Connected, ConnectionState.Disconnected, ConnectionState.Connected], states);
    }
}

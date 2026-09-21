// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.MechanismsObservables;

/// <summary>
/// Builds and subscribes to the observable types in <c>ReactiveUI.Binding.Observables</c>. Generated code and the
/// run-time engine construct these types; each example builds one by hand and shows what it delivers.
/// </summary>
public static class MechanismsObservablesExamples
{
    /// <summary>The number of notifications a single edit raises.</summary>
    private const int OneNotification = 1;

    /// <summary>The number of notifications the provider passes on for two state changes.</summary>
    private const int TwoNotifications = 2;

    /// <summary>The title of the registration task.</summary>
    private const string RegistrationTitle = "Renew car registration";

    /// <summary>The title the registration task is renamed to.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The title the registration task is renamed to a second time.</summary>
    private const string FinalTitle = "Renew car registration by post";

    /// <summary>The notes of the registration task.</summary>
    private const string RegistrationNotes = "Bring the insurance certificate";

    /// <summary>The address of the storage service.</summary>
    private const string StorageEndpoint = "https://storage.example.test";

    /// <summary>The name of the observed connection state.</summary>
    private const string StateName = nameof(StorageConnection.State);

    /// <summary>Observes whether a to-do item is urgent; a repeated value is dropped.</summary>
    public static void ObserveTodoUrgencyWithoutRepeats()
    {
        TodoItem item = new() { Title = RegistrationTitle };
        CapturingObserver<bool> urgency = new();
        PropertyObservable<bool> isUrgent = new(item, nameof(TodoItem.Priority), static source => ((TodoItem)source).Priority == TodoPriority.High, true);

        using (isUrgent.Subscribe(urgency))
        {
            item.Priority = TodoPriority.Low;
            item.Priority = TodoPriority.High;
        }

        SampleCheck.SequenceEqual([false, true], urgency.Values);
    }

    /// <summary>Observes whether a to-do item is urgent; every priority change delivers a value.</summary>
    public static void ObserveTodoUrgencyOnEveryChange()
    {
        TodoItem item = new() { Title = RegistrationTitle };
        CapturingObserver<bool> urgency = new();
        PropertyObservable<bool> isUrgent = new(item, nameof(TodoItem.Priority), static source => ((TodoItem)source).Priority == TodoPriority.High, false);

        using (isUrgent.Subscribe(urgency))
        {
            item.Priority = TodoPriority.Low;
            item.Priority = TodoPriority.High;
        }

        SampleCheck.SequenceEqual([false, false, true], urgency.Values);
    }

    /// <summary>Reads the title of an item before each edit is applied.</summary>
    public static void ObserveTitleBeforeEachEdit()
    {
        DraftTodo draft = new() { Title = RegistrationTitle };
        CapturingObserver<string> titles = new();
        PropertyChangingObservable<string> beforeChange = new(draft, nameof(DraftTodo.Title), static source => ((DraftTodo)source).Title);

        using (beforeChange.Subscribe(titles))
        {
            draft.Title = RenamedTitle;
            draft.Title = FinalTitle;
            draft.Notes = RegistrationNotes;
        }

        SampleCheck.SequenceEqual([RegistrationTitle, RegistrationTitle, RenamedTitle], titles.Values);
    }

    /// <summary>Reports each after-change notification for one property as an observed change that names the sender and the expression.</summary>
    public static void ObserveDraftTitleAfterChange()
    {
        DraftTodo draft = new() { Title = RegistrationTitle };
        Expression<Func<DraftTodo, string>> title = x => x.Title;
        CapturingObserver<IObservedChange<object, object?>> changes = new();
        List<string> titlesSeen = [];
        NotifyPropertyChangedObservable afterChange = new(draft, title, nameof(DraftTodo.Title), false);

        using (afterChange.Subscribe(changes))
        using (afterChange.Subscribe(change => titlesSeen.Add(((DraftTodo)change.Sender).Title)))
        {
            draft.Title = RenamedTitle;
            draft.Notes = RegistrationNotes;
        }

        SampleCheck.Equal(OneNotification, changes.Values.Count);
        SampleCheck.Equal(true, ReferenceEquals(draft, changes.Values[0].Sender));
        SampleCheck.Equal(true, ReferenceEquals(title, changes.Values[0].Expression));
        SampleCheck.SequenceEqual([RenamedTitle], titlesSeen);
    }

    /// <summary>Reports each before-change notification for one property; the sender still holds the old value.</summary>
    public static void ObserveDraftTitleBeforeChange()
    {
        DraftTodo draft = new() { Title = RegistrationTitle };
        Expression<Func<DraftTodo, string>> title = x => x.Title;
        CapturingObserver<IObservedChange<object, object?>> changes = new();
        List<string> titlesSeen = [];
        NotifyPropertyChangedObservable beforeChange = new(draft, title, nameof(DraftTodo.Title), true);

        using (beforeChange.Subscribe(changes))
        using (beforeChange.Subscribe(change => titlesSeen.Add(((DraftTodo)change.Sender).Title)))
        {
            draft.Title = RenamedTitle;
            draft.Notes = RegistrationNotes;
        }

        SampleCheck.Equal(OneNotification, changes.Values.Count);
        SampleCheck.SequenceEqual([RegistrationTitle], titlesSeen);
    }

    /// <summary>Observes a property whose owner raises no notification: its value once, and then nothing, without completing.</summary>
    public static void ObserveStorageObjectKey()
    {
        StorageObject file = new() { Key = "photos/2026/launch.png" };
        CapturingObserver<string> keys = new();
        UnchangingPropertyObservable<string> key = new(file.Key);

        using (key.Subscribe(keys))
        {
            file.Key = "photos/2026/renamed.png";
        }

        SampleCheck.SequenceEqual(["photos/2026/launch.png"], keys.Values);
        SampleCheck.Equal(false, keys.IsCompleted);
        SampleCheck.Equal(true, keys.Error is null);
    }

    /// <summary>Observes whether a storage connection is open through its plain event; a repeated value is dropped.</summary>
    public static void ObserveConnectionOpenWithoutRepeats()
    {
        StorageConnection connection = new() { Endpoint = StorageEndpoint };
        CapturingObserver<bool> openStates = new();
        EventObservable<bool> isOpen = new(
            handler => connection.StateChanged += handler,
            handler => connection.StateChanged -= handler,
            () => connection.State == ConnectionState.Connected,
            true);

        using (isOpen.Subscribe(openStates))
        {
            connection.State = ConnectionState.Connecting;
            connection.State = ConnectionState.Connected;
            connection.State = ConnectionState.Disconnected;
        }

        connection.State = ConnectionState.Connected;

        SampleCheck.SequenceEqual([false, true, false], openStates.Values);
    }

    /// <summary>Observes whether a storage connection is open through its plain event; every event delivers a value.</summary>
    public static void ObserveConnectionOpenOnEveryEvent()
    {
        StorageConnection connection = new() { Endpoint = StorageEndpoint };
        CapturingObserver<bool> openStates = new();
        EventObservable<bool> isOpen = new(
            handler => connection.StateChanged += handler,
            handler => connection.StateChanged -= handler,
            () => connection.State == ConnectionState.Connected,
            false);

        using (isOpen.Subscribe(openStates))
        {
            connection.State = ConnectionState.Connecting;
            connection.State = ConnectionState.Connected;
            connection.State = ConnectionState.Disconnected;
        }

        SampleCheck.SequenceEqual([false, false, true, false], openStates.Values);
    }

    /// <summary>Reports the changes a binding wrote, and stops reporting to a subscriber that unsubscribed.</summary>
    public static void ReportAppliedChanges()
    {
        AppliedChangeObservable applied = new();
        CapturingObserver<BindingChange> changes = new();

        SampleCheck.Equal(false, applied.HasObservers);

        using (applied.Subscribe(changes))
        {
            SampleCheck.Equal(true, applied.HasObservers);

            applied.OnNext(new(RenamedTitle, true));
            applied.OnNext(new(FinalTitle, false));
        }

        applied.OnNext(new(RegistrationTitle, true));

        SampleCheck.Equal(false, applied.HasObservers);
        SampleCheck.SequenceEqual([new BindingChange(RenamedTitle, true), new BindingChange(FinalTitle, false)], changes.Values);
    }

    /// <summary>Reads the changes of a two-way binding, each tagged with the side that produced it.</summary>
    public static void ReadTwoWayBindingChanges()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        TodoView view = new() { ViewModel = viewModel };
        CapturingObserver<BindingChange> changes = new();

        using var binding = view.Bind(viewModel, x => x.NewTitle, v => v.NewTitleTextBox.Text);
        using var subscription = binding.Changed.Subscribe(changes);

        view.NewTitleTextBox.Text = RenamedTitle;
        SampleCheck.Equal(RenamedTitle, viewModel.NewTitle);
        viewModel.NewTitle = FinalTitle;

        SampleCheck.SequenceEqual([new BindingChange(RenamedTitle, false), new BindingChange(FinalTitle, true)], changes.Values);
        SampleCheck.Equal(FinalTitle, view.NewTitleTextBox.Text);
    }

    /// <summary>Keeps the generated observation while no registered provider outranks it.</summary>
    public static void ChooseGeneratedObservationWithoutProvider()
    {
        StorageConnection connection = new() { Endpoint = StorageEndpoint };
        Expression<Func<StorageConnection, ConnectionState>> state = x => x.State;
        var generated = CreateGeneratedStateObservation(connection);

        var chosen = PluginObservationSource.Choose(
            connection,
            state,
            StateName,
            false,
            BindingAffinity.Explicit,
            static source => ((StorageConnection)source).State,
            generated);

        SampleCheck.Equal(true, ReferenceEquals(generated, chosen));
    }

    /// <summary>Registers the storage provider through the builder and clears the cached votes so the next observation sees it.</summary>
    /// <returns>The registered provider.</returns>
    public static StorageStateProvider RegisterStateProvider()
    {
        StorageStateProvider provider = new();
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder
            .WithCoreServices()
            .WithRegistration(resolver => resolver.RegisterConstant<ICreatesObservableForProperty>(provider))
            .BuildApp();

        ObservationAffinityChecker.Refresh();

        SampleCheck.Equal(true, ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(StorageConnection), StateName, BindingAffinity.Explicit, false));
        return provider;
    }

    /// <summary>Observes a property through a provider: the value now, then the value after each notification the provider raises.</summary>
    /// <param name="provider">The registered provider.</param>
    public static void ObserveThroughProvider(StorageStateProvider provider)
    {
        StorageConnection connection = new() { Endpoint = StorageEndpoint };
        Expression<Func<StorageConnection, ConnectionState>> state = x => x.State;
        CapturingObserver<ConnectionState> states = new();
        var notificationsBefore = provider.NotificationCount;
        PluginPropertyObservable<ConnectionState> observed = new(
            provider,
            connection,
            state,
            StateName,
            static source => ((StorageConnection)source).State,
            false,
            true);

        using (observed.Subscribe(states))
        {
            connection.State = ConnectionState.Connecting;
            connection.State = ConnectionState.Connected;
        }

        SampleCheck.SequenceEqual([ConnectionState.Disconnected, ConnectionState.Connecting, ConnectionState.Connected], states.Values);
        SampleCheck.Equal(TwoNotifications, provider.NotificationCount - notificationsBefore);
    }

    /// <summary>Switches to the registered provider when it outranks the generated mechanism.</summary>
    /// <param name="provider">The registered provider.</param>
    public static void ChooseProviderThatOutranksGeneratedObservation(StorageStateProvider provider)
    {
        StorageConnection connection = new() { Endpoint = StorageEndpoint };
        Expression<Func<StorageConnection, ConnectionState>> state = x => x.State;
        CapturingObserver<ConnectionState> states = new();
        var notificationsBefore = provider.NotificationCount;

        var chosen = PluginObservationSource.Choose(
            connection,
            state,
            StateName,
            false,
            BindingAffinity.Explicit,
            static source => ((StorageConnection)source).State,
            CreateGeneratedStateObservation(connection));

        using (chosen.Subscribe(states))
        {
            connection.State = ConnectionState.Connected;
        }

        SampleCheck.Equal(true, chosen is PluginPropertyObservable<ConnectionState>);
        SampleCheck.SequenceEqual([ConnectionState.Disconnected, ConnectionState.Connected], states.Values);
        SampleCheck.Equal(OneNotification, provider.NotificationCount - notificationsBefore);
    }

    /// <summary>Keeps the generated observation when the provider only ties its affinity.</summary>
    /// <param name="provider">The registered provider.</param>
    public static void ChooseGeneratedObservationOnEqualAffinity(StorageStateProvider provider)
    {
        StorageConnection connection = new() { Endpoint = StorageEndpoint };
        Expression<Func<StorageConnection, ConnectionState>> state = x => x.State;
        var generated = CreateGeneratedStateObservation(connection);

        var chosen = PluginObservationSource.Choose(
            connection,
            state,
            StateName,
            false,
            provider.GetAffinityForObject(typeof(StorageConnection), StateName, false),
            static source => ((StorageConnection)source).State,
            generated);

        SampleCheck.Equal(true, ReferenceEquals(generated, chosen));
    }

    /// <summary>Creates the observation a generator would emit for the state of a connection when it knows no better mechanism.</summary>
    /// <param name="connection">The connection to observe.</param>
    /// <returns>The state now and after each event.</returns>
    private static EventObservable<ConnectionState> CreateGeneratedStateObservation(StorageConnection connection) => new(
        handler => connection.StateChanged += handler,
        handler => connection.StateChanged -= handler,
        () => connection.State,
        true);
}

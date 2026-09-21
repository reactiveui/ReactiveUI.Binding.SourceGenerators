// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Primitives;
using Splat;

namespace ReactiveUI.Binding.Documentation.MechanismsAffinity;

/// <summary>
/// Shows how affinity decides which mechanism observes a property: the highest score wins, and a provider
/// registered at run time outranks the generated mechanism only with a strictly higher score.
/// </summary>
public static class MechanismsAffinityExamples
{
    /// <summary>The number of providers registered by the core services and by the example.</summary>
    private const int RegisteredProviderCount = 4;

    /// <summary>The size given to a stored object while it is observed.</summary>
    private const long ResizedBytes = 1_024;

    /// <summary>The notes written to an item while they are observed.</summary>
    private const string EditedNotes = "Bring the insurance certificate and the old plate.";

    /// <summary>The title written to an item while it is observed.</summary>
    private const string EditedTitle = "Renew car registration online";

    /// <summary>The name of the property that carries the notes.</summary>
    private const string NotesPropertyName = nameof(TodoItem.Notes);

    /// <summary>The name of the property that carries the title.</summary>
    private const string TitlePropertyName = nameof(TodoItem.Title);

    /// <summary>Registers the core mechanisms and two providers that bid for properties of <see cref="TodoItem"/>.</summary>
    /// <param name="tied">The provider whose bid equals the generated mechanism's affinity.</param>
    /// <param name="outranking">The provider whose bid is one above the generated mechanism's affinity.</param>
    /// <returns>The built application.</returns>
    public static IReactiveUIBindingInstance RegisterProviders(TodoPropertyObservableForProperty tied, TodoPropertyObservableForProperty outranking)
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        var app = builder
            .WithCoreServices()
            .WithRegistration(resolver => resolver.RegisterConstant<ICreatesObservableForProperty>(tied))
            .WithRegistration(resolver => resolver.RegisterConstant<ICreatesObservableForProperty>(outranking))
            .BuildApp();

        ObservationAffinityChecker.Refresh();

        List<ICreatesObservableForProperty> providers = [.. AppLocator.Current.GetServices<ICreatesObservableForProperty>()];

        SampleCheck.Equal(RegisteredProviderCount, providers.Count);
        return app;
    }

    /// <summary>Observes a property of a class that raises <c>PropertyChanged</c>; the generated mechanism delivers each change.</summary>
    /// <param name="item">The item to observe.</param>
    public static void ObserveNotifyingClass(TodoItem item)
    {
        List<bool> states = [];

        using (item.WhenChanged(x => x.IsDone).Subscribe(states.Add))
        {
            item.IsDone = true;
        }

        SampleCheck.SequenceEqual([false, true], states);
    }

    /// <summary>Observes a property of a plain class; only the value at subscription arrives, and later changes do not.</summary>
    /// <param name="stored">The stored object to observe.</param>
    public static void ObservePlainClass(StorageObject stored)
    {
        var originalSize = stored.Size;
        List<long> sizes = [];

        using (stored.ObservableForProperty(x => x.Size, false).Subscribe(change => sizes.Add(change.Value)))
        {
            var countAtSubscription = sizes.Count;

            stored.Size = ResizedBytes;

            SampleCheck.Equal(countAtSubscription, sizes.Count);
        }

        SampleCheck.Equal(originalSize, sizes[0]);
        SampleCheck.Equal(originalSize, sizes[^1]);
    }

    /// <summary>Scores three properties with the core providers and the example providers, and names the winner of each.</summary>
    /// <param name="outranking">The provider that bids one above <c>PropertyChanged</c>.</param>
    public static void PickWinningMechanism(TodoPropertyObservableForProperty outranking)
    {
        SampleCheck.Equal(BindingAffinity.Explicit, new INPCObservableForProperty().GetAffinityForObject(typeof(TodoItem), nameof(TodoItem.IsDone)));
        SampleCheck.Equal(0, new INPCObservableForProperty().GetAffinityForObject(typeof(StorageObject), nameof(StorageObject.Size)));
        SampleCheck.Equal(BindingAffinity.Fallback, new POCOObservableForProperty().GetAffinityForObject(typeof(StorageObject), nameof(StorageObject.Size)));

        SampleCheck.Equal(true, SelectHighestAffinity(typeof(TodoItem), nameof(TodoItem.IsDone)) is INPCObservableForProperty);
        SampleCheck.Equal(true, SelectHighestAffinity(typeof(StorageObject), nameof(StorageObject.Size)) is POCOObservableForProperty);
        SampleCheck.Equal(true, ReferenceEquals(outranking, SelectHighestAffinity(typeof(TodoItem), TitlePropertyName)));
    }

    /// <summary>
    /// Compares every affinity score with a provider that bids <see cref="BindingAffinity.Explicit"/>: the
    /// provider outranks the scores below its bid, and loses to its own score and every score above it.
    /// </summary>
    /// <param name="tied">The provider that bids <see cref="BindingAffinity.Explicit"/> for the notes.</param>
    public static void CompareEveryAffinityScore(TodoPropertyObservableForProperty tied)
    {
        int[] belowBid =
        [
            BindingAffinity.Fallback,
            BindingAffinity.DefaultInternalTypeConverter,
            BindingAffinity.DefaultEvent,
            BindingAffinity.WpfDependencyObject,
            BindingAffinity.EventEnabledControl,
        ];
        int[] atOrAboveBid =
        [
            BindingAffinity.Explicit,
            BindingAffinity.WinUiDependencyObject,
            BindingAffinity.WinFormsEvent,
            BindingAffinity.ExactType,
            BindingAffinity.Kvo,
        ];

        foreach (var generated in belowBid)
        {
            SampleCheck.Equal(true, ReferenceEquals(tied, ObservationAffinityChecker.FindHigherAffinityPlugin(typeof(TodoItem), NotesPropertyName, generated, false)));
        }

        foreach (var generated in atOrAboveBid)
        {
            SampleCheck.Equal(false, ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(TodoItem), NotesPropertyName, generated, false));
        }
    }

    /// <summary>Observes two properties of the same item; the provider that ties the generated mechanism is never used, and the one that outranks it is.</summary>
    /// <param name="item">The item to observe.</param>
    /// <param name="tied">The provider that bids <see cref="BindingAffinity.Explicit"/> for the notes.</param>
    /// <param name="outranking">The provider that bids one above <see cref="BindingAffinity.Explicit"/> for the title.</param>
    public static void ObserveWithTiedAndOutrankingProviders(
        TodoItem item,
        TodoPropertyObservableForProperty tied,
        TodoPropertyObservableForProperty outranking)
    {
        var originalNotes = item.Notes;
        var originalTitle = item.Title;
        List<string> notes = [];
        List<string> titles = [];

        using (item.WhenChanged(x => x.Notes).Subscribe(notes.Add))
        using (item.WhenChanged(x => x.Title).Subscribe(titles.Add))
        {
            item.Notes = EditedNotes;
            item.Title = EditedTitle;
        }

        SampleCheck.SequenceEqual([originalNotes, EditedNotes], notes);
        SampleCheck.SequenceEqual([originalTitle, EditedTitle], titles);
        SampleCheck.Equal(0, tied.ObservationCount);
        SampleCheck.Equal(1, outranking.ObservationCount);
    }

    /// <summary>Finds the registered provider with the highest affinity for a property, keeping the first on a tie.</summary>
    /// <param name="type">The type being observed.</param>
    /// <param name="propertyName">The property being observed.</param>
    /// <returns>The winning provider, or <see langword="null"/> when none supports the property.</returns>
    private static ICreatesObservableForProperty? SelectHighestAffinity(Type type, string propertyName)
    {
        ICreatesObservableForProperty? winner = null;
        var highest = 0;

        foreach (var provider in AppLocator.Current.GetServices<ICreatesObservableForProperty>())
        {
            var affinity = provider.GetAffinityForObject(type, propertyName);
            if (affinity <= highest)
            {
                continue;
            }

            highest = affinity;
            winner = provider;
        }

        return winner;
    }
}

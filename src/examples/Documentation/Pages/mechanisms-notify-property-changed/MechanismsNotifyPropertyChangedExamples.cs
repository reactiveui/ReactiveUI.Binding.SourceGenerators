// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Primitives;
using Splat;

namespace ReactiveUI.Binding.Documentation.MechanismsNotifyPropertyChanged;

/// <summary>
/// Shows the two built-in observation mechanisms: <see cref="INPCObservableForProperty"/> for a class that raises
/// <c>PropertyChanged</c>, and <see cref="POCOObservableForProperty"/> for a plain class that raises nothing.
/// </summary>
public static class MechanismsNotifyPropertyChangedExamples
{
    /// <summary>The number of observation providers the core services register.</summary>
    private const int CoreProviderCount = 2;

    /// <summary>The title of the first seeded item.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title given to an item while it is observed.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The title given to an item while its <c>PropertyChanged</c> notifications are observed directly.</summary>
    private const string RetitledByPost = "Renew car registration by post";

    /// <summary>The notes written to an item while a different property is observed.</summary>
    private const string EditedNotes = "Bring the old plate as well.";

    /// <summary>The name of the observed title property.</summary>
    private const string TitlePropertyName = nameof(TodoItem.Title);

    /// <summary>The name of the observed size property.</summary>
    private const string SizePropertyName = nameof(StorageObject.Size);

    /// <summary>The size given to a stored object while it is observed.</summary>
    private const long ResizedBytes = 4_096;

    /// <summary>Registers the core observation providers, which the run-time engine resolves by affinity.</summary>
    public static void RegisterCoreObservationServices()
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices().BuildApp();

        List<Type> providers = [];
        foreach (var provider in AppLocator.Current.GetServices<ICreatesObservableForProperty>())
        {
            providers.Add(provider.GetType());
        }

        SampleCheck.Equal(CoreProviderCount, providers.Count);
        SampleCheck.Equal(true, providers.Contains(typeof(INPCObservableForProperty)));
        SampleCheck.Equal(true, providers.Contains(typeof(POCOObservableForProperty)));
    }

    /// <summary>Observes an item through <c>WhenChanged</c>; the generated code listens to <c>PropertyChanged</c>.</summary>
    /// <param name="item">The item to observe.</param>
    public static void ObserveNotifyingClassThroughWhenChanged(TodoItem item)
    {
        List<string> titles = [];

        using (item.WhenChanged(x => x.Title).Subscribe(titles.Add))
        {
            item.Title = RenamedTitle;
        }

        SampleCheck.SequenceEqual([OriginalTitle, RenamedTitle], titles);
    }

    /// <summary>Asks the <c>PropertyChanged</c> provider about an item and listens to it directly.</summary>
    /// <param name="item">The item to observe.</param>
    public static void ObserveNotifyingClassWithInpcProvider(TodoItem item)
    {
        INPCObservableForProperty provider = new();
        Expression<Func<TodoItem, string>> property = x => x.Title;

        SampleCheck.Equal(BindingAffinity.Explicit, provider.GetAffinityForObject(typeof(TodoItem), TitlePropertyName));
        SampleCheck.Equal(BindingAffinity.Explicit, provider.GetAffinityForObject(typeof(TodoItem), TitlePropertyName, false));

        // TodoItem does not implement INotifyPropertyChanging, so the provider bids nothing for before-change observation.
        SampleCheck.Equal(0, provider.GetAffinityForObject(typeof(TodoItem), TitlePropertyName, true));

        List<IObservedChange<object, object?>> changes = [];
        using (provider.GetNotificationForProperty(item, property.Body, TitlePropertyName).Subscribe(changes.Add))
        {
            item.Notes = EditedNotes;
            item.Title = RetitledByPost;
        }

        SampleCheck.Equal(1, changes.Count);
        SampleCheck.Equal(true, ReferenceEquals(item, changes[0].Sender));
        SampleCheck.Equal(TitlePropertyName, changes[0].GetPropertyName());
    }

    /// <summary>Asks the <c>PropertyChanged</c> provider about a plain class; it bids nothing and reports nothing.</summary>
    /// <param name="stored">The stored object to observe.</param>
    public static void ObservePlainClassWithInpcProvider(StorageObject stored)
    {
        INPCObservableForProperty provider = new();
        Expression<Func<StorageObject, long>> property = x => x.Size;

        SampleCheck.Equal(0, provider.GetAffinityForObject(typeof(StorageObject), SizePropertyName));

        List<IObservedChange<object, object?>> changes = [];
        using (provider.GetNotificationForProperty(stored, property.Body, SizePropertyName).Subscribe(changes.Add))
        {
            stored.Size = ResizedBytes;
        }

        SampleCheck.Equal(0, changes.Count);
    }

    /// <summary>Asks the fallback provider about a plain class; it delivers one notification when observation starts and no more.</summary>
    /// <param name="stored">The stored object to observe.</param>
    public static void ObservePlainClassWithPocoProvider(StorageObject stored)
    {
        POCOObservableForProperty provider = new();
        Expression<Func<StorageObject, long>> property = x => x.Size;

        SampleCheck.Equal(BindingAffinity.Fallback, provider.GetAffinityForObject(typeof(StorageObject), SizePropertyName));
        SampleCheck.Equal(BindingAffinity.Fallback, provider.GetAffinityForObject(typeof(TodoItem), TitlePropertyName));

        List<IObservedChange<object, object?>> changes = [];
        using (provider.GetNotificationForProperty(stored, property.Body, SizePropertyName, false, true).Subscribe(changes.Add))
        {
            SampleCheck.Equal(1, changes.Count);

            stored.Size = ResizedBytes;
        }

        SampleCheck.Equal(1, changes.Count);
        SampleCheck.Equal(true, ReferenceEquals(stored, changes[0].Sender));
        SampleCheck.Equal(SizePropertyName, changes[0].GetPropertyName());
    }

    /// <summary>Observes a notifying view model's selection through <c>WhenChanged</c> and reads the plain class's property on each delivery.</summary>
    /// <param name="browser">The browser whose selection is observed.</param>
    /// <param name="first">The object selected first.</param>
    /// <param name="second">The object selected second.</param>
    public static void ObservePlainClassThroughWhenChanged(StorageBrowserViewModel browser, StorageObject first, StorageObject second)
    {
        var firstSize = first.Size;
        List<long> sizes = [];

        browser.SelectedObject = first;

        using (browser.WhenChanged(x => x.SelectedObject!).Subscribe(selected => sizes.Add(selected.Size)))
        {
            first.Size = ResizedBytes;
            browser.SelectedObject = second;
        }

        SampleCheck.SequenceEqual([firstSize, second.Size], sizes);
    }

    /// <summary>Follows a path from a notifying view model into a plain class: the selection is followed, the plain object's own changes are not.</summary>
    /// <param name="browser">The browser whose selection is observed.</param>
    /// <param name="first">The object selected first.</param>
    /// <param name="second">The object selected second.</param>
    public static void ObservePathThroughPlainClass(StorageBrowserViewModel browser, StorageObject first, StorageObject second)
    {
        List<long> sizes = [];

        browser.SelectedObject = first;

        using (browser.ObservableForProperty(x => x.SelectedObject!.Size, false).Subscribe(change => sizes.Add(change.Value)))
        {
            var countAtSubscription = sizes.Count;

            first.Size = ResizedBytes;

            SampleCheck.Equal(countAtSubscription, sizes.Count);

            browser.SelectedObject = second;

            SampleCheck.Equal(second.Size, sizes[^1]);
        }
    }
}

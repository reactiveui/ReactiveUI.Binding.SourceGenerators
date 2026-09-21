// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Todo;
using Splat;

namespace ReactiveUI.Binding.Documentation.ViewsMapping;

/// <summary>Shows how views are registered: <c>Map</c>, <c>ViewMappingBuilder</c>, the view attributes, and the order in which the locator looks.</summary>
public static class ViewsMappingExamples
{
    /// <summary>The title of the to-do item the examples show.</summary>
    private const string ItemTitle = "Renew car registration";

    /// <summary>The balance of the account the examples show.</summary>
    private const decimal AccountBalance = 2450.75M;

    /// <summary>The contract of the preview layout.</summary>
    private const string PreviewContract = "preview";

    /// <summary>The contract of the detail layout.</summary>
    private const string DetailContract = "detail";

    /// <summary>The contract of the compact layout.</summary>
    private const string CompactContract = "compact";

    /// <summary>Registers a preview screen for a to-do item with its parameterless constructor.</summary>
    public static void MapView()
    {
        var item = CreateItem();
        DefaultViewLocator locator = new();

        locator.Map<TodoItem, TodoItemPreviewView>();

        var view = locator.ResolveView(item);

        SampleCheck.Equal(typeof(TodoItemPreviewView), view?.GetType());
        SampleCheck.Equal(item, view?.ViewModel);
    }

    /// <summary>Registers a preview screen under a contract, so it answers only to that contract.</summary>
    public static void MapViewWithContract()
    {
        var item = CreateItem();
        DefaultViewLocator locator = new();

        locator.Map<TodoItem, TodoItemPreviewView>(PreviewContract);

        var preview = locator.ResolveView(item, PreviewContract);
        var plain = locator.ResolveView(item, null);

        SampleCheck.Equal(typeof(TodoItemPreviewView), preview?.GetType());
        SampleCheck.Equal(true, plain is null);
    }

    /// <summary>Registers a factory, for a screen that needs setup before it is shown.</summary>
    public static void MapViewWithFactory()
    {
        var item = CreateItem();
        DefaultViewLocator locator = new();

        locator.Map<TodoItem>(static () => new TodoItemPreviewView { IsCompact = true });

        var view = (TodoItemPreviewView?)locator.ResolveView(item);

        SampleCheck.Equal(true, view?.IsCompact);
        SampleCheck.Equal(item, view?.ViewModel);
    }

    /// <summary>Registers a factory under a contract.</summary>
    public static void MapViewWithFactoryAndContract()
    {
        var item = CreateItem();
        DefaultViewLocator locator = new();

        locator.Map<TodoItem>(static () => new TodoItemPreviewView { IsCompact = true }, CompactContract);

        var view = (TodoItemPreviewView?)locator.ResolveView(item, CompactContract);

        SampleCheck.Equal(true, view?.IsCompact);
        SampleCheck.Equal(true, locator.ResolveView(item, null) is null);
    }

    /// <summary>Removes a registration, with and without a contract; the result says whether one existed.</summary>
    public static void UnmapView()
    {
        var item = CreateItem();
        DefaultViewLocator locator = new();
        locator.Map<TodoItem, TodoItemPreviewView>();
        locator.Map<TodoItem, TodoItemPreviewView>(PreviewContract);

        var removedDefault = locator.Unmap<TodoItem>();
        var removedAgain = locator.Unmap<TodoItem>();
        var removedContract = locator.Unmap<TodoItem>(PreviewContract);

        SampleCheck.Equal(true, removedDefault);
        SampleCheck.Equal(false, removedAgain);
        SampleCheck.Equal(true, removedContract);
        SampleCheck.Equal(true, locator.ResolveView(item) is null);
    }

    /// <summary>Registers views in a chain with the fluent builder of a locator.</summary>
    public static void MapViewsWithFluentBuilder()
    {
        var item = CreateItem();
        DefaultViewLocator locator = new();

        _ = locator.CreateMappingBuilder()
            .Map<TodoItem, TodoItemPreviewView>()
            .Map<TodoItem, TodoItemDetailView>(DetailContract)
            .Map<TodoItem>(static () => new TodoItemPreviewView { IsCompact = true }, CompactContract);

        var standard = locator.ResolveView(item, null);
        var detail = locator.ResolveView(item, DetailContract);
        var compact = (TodoItemPreviewView?)locator.ResolveView(item, CompactContract);

        SampleCheck.Equal(typeof(TodoItemPreviewView), standard?.GetType());
        SampleCheck.Equal(typeof(TodoItemDetailView), detail?.GetType());
        SampleCheck.Equal(true, compact?.IsCompact);
    }

    /// <summary>Registers a factory for the default contract in a chain.</summary>
    public static void MapFactoryWithFluentBuilder()
    {
        var item = CreateItem();
        DefaultViewLocator locator = new();

        _ = locator.CreateMappingBuilder().Map<TodoItem>(static () => new TodoItemPreviewView { IsCompact = true });

        var view = (TodoItemPreviewView?)locator.ResolveView(item);

        SampleCheck.Equal(true, view?.IsCompact);
    }

    /// <summary>Registers the mappings while the application starts, before the locator is first used.</summary>
    public static void ConfigureViewLocatorInBuilder()
    {
        var item = CreateItem();
        var builder = (IReactiveUIBindingBuilder)RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder
            .WithCoreServices()
            .ConfigureViewLocator(static mappings => mappings
                .Map<TodoItem, TodoItemPreviewView>()
                .Map<TodoItem, TodoItemDetailView>(DetailContract))
            .BuildApp();

        var locator = ViewLocator.GetCurrent();

        var standard = locator.ResolveView(item);
        var detail = locator.ResolveView(item, DetailContract);

        SampleCheck.Equal(typeof(TodoItemPreviewView), standard?.GetType());
        SampleCheck.Equal(typeof(TodoItemDetailView), detail?.GetType());
    }

    /// <summary>Resolves a view that leaves the locator's registration out: nothing answers until the application maps it.</summary>
    public static void ResolveViewLeftOutOfRegistration()
    {
        var item = CreateItem();
        DefaultViewLocator locator = new();

        var before = locator.ResolveView(item);
        locator.Map<TodoItem, TodoItemPreviewView>();
        var after = locator.ResolveView(item);

        SampleCheck.Equal(true, before is null);
        SampleCheck.Equal(typeof(TodoItemPreviewView), after?.GetType());
    }

    /// <summary>Resolves the same account twice: a view marked as a single instance is built once and shared.</summary>
    public static void ResolveSingleInstanceView()
    {
        var account = CreateAccount();
        DefaultViewLocator locator = new();

        var first = locator.ResolveView(account);
        var second = locator.ResolveView(account);

        SampleCheck.Equal(typeof(AccountSummaryView), first?.GetType());
        SampleCheck.Equal(first, second);
    }

    /// <summary>Resolves the account screens by contract: the contract picks the statement, and no contract picks the summary.</summary>
    public static void ResolveViewByContract()
    {
        var account = CreateAccount();
        DefaultViewLocator locator = new();

        var statement = locator.ResolveView(account, AccountViewContracts.Statement);
        var summary = locator.ResolveView(account, null);

        SampleCheck.Equal(typeof(AccountStatementView), statement?.GetType());
        SampleCheck.Equal(typeof(AccountSummaryView), summary?.GetType());
    }

    /// <summary>Resolves with the locator's three sources registered: the generated lookup comes first, then <c>Map</c>, then the service locator.</summary>
    public static void ResolveInOrder()
    {
        var item = CreateItem();
        var account = CreateAccount();
        DefaultViewLocator locator = new();
        locator.Map<Account, AccountStatementView>();
        locator.Map<TodoItem, TodoItemPreviewView>();
        AppLocator.CurrentMutable.Register<IViewFor<TodoItem>>(static () => new TodoItemDetailView());

        try
        {
            var generated = locator.ResolveView(account);
            var mapped = locator.ResolveView(item);
            _ = locator.Unmap<TodoItem>();
            var registered = locator.ResolveView(item);

            SampleCheck.Equal(typeof(AccountSummaryView), generated?.GetType());
            SampleCheck.Equal(typeof(TodoItemPreviewView), mapped?.GetType());
            SampleCheck.Equal(typeof(TodoItemDetailView), registered?.GetType());
        }
        finally
        {
            AppLocator.CurrentMutable.UnregisterAll<IViewFor<TodoItem>>();
        }
    }

    /// <summary>Creates the to-do item the examples show.</summary>
    /// <returns>A new item.</returns>
    private static TodoItem CreateItem() => new() { Title = ItemTitle };

    /// <summary>Creates the account the examples show.</summary>
    /// <returns>A new account.</returns>
    private static Account CreateAccount() => new() { Id = "ACC-1001", Name = "Everyday Account", Kind = AccountKind.Everyday, Currency = "AUD", Balance = AccountBalance };
}

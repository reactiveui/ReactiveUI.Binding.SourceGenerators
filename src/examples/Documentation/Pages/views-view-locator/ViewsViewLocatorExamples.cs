// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.ViewsViewLocator;

/// <summary>Shows how a view model finds its screen: <c>IViewLocator</c>, <c>DefaultViewLocator</c>, <c>ViewLocator</c> and <c>ViewLocatorNotFoundException</c>.</summary>
public static class ViewsViewLocatorExamples
{
    /// <summary>The message of the layout failure a view factory reports.</summary>
    private const string LayoutFailure = "The layout file is missing.";

    /// <summary>Finds the screen for a view model, and explains a miss to the user rather than returning nothing.</summary>
    /// <param name="locator">The locator that knows the screens.</param>
    /// <param name="viewModel">The view model to show.</param>
    /// <returns>The screen, with its view model set.</returns>
    /// <exception cref="ViewLocatorNotFoundException">No screen is registered for the view model.</exception>
    public static IViewFor RequireView(IViewLocator locator, object viewModel)
    {
        var view = locator.ResolveView(viewModel);
        return view ?? throw new ViewLocatorNotFoundException($"No screen is registered for {viewModel.GetType().Name}.");
    }

    /// <summary>Finds the screen for a view model, and reports a screen that fails to build as a missing screen.</summary>
    /// <param name="locator">The locator that knows the screens.</param>
    /// <param name="viewModel">The view model to show.</param>
    /// <returns>The screen, with its view model set.</returns>
    /// <exception cref="ViewLocatorNotFoundException">The screen could not be built.</exception>
    public static IViewFor? ResolveOrExplain(IViewLocator locator, TodoItem viewModel)
    {
        try
        {
            return locator.ResolveView(viewModel);
        }
        catch (InvalidOperationException ex)
        {
            throw new ViewLocatorNotFoundException($"The screen for {nameof(TodoItem)} could not be built.", ex);
        }
    }

    /// <summary>Asks for the current locator before any is registered, which fails with the message that names the fix.</summary>
    public static void RequireLocatorBeforeRegistration()
    {
        ViewLocatorNotFoundException? failure = null;

        try
        {
            _ = ViewLocator.GetCurrent();
        }
        catch (ViewLocatorNotFoundException ex)
        {
            failure = ex;
        }

        SampleCheck.Equal(true, failure is not null);
        SampleCheck.Equal(true, failure!.Message.Contains("WithCoreServices", StringComparison.Ordinal));
    }

    /// <summary>Registers the core services, which include the default view locator, and reads it back from <see cref="ViewLocator"/>.</summary>
    public static void RegisterDefaultLocator()
    {
        var builder = (IReactiveUIBindingBuilder)RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices().BuildApp();

        var locator = ViewLocator.GetCurrent();

        SampleCheck.Equal(true, locator is DefaultViewLocator);
    }

    /// <summary>Resolves the to-do screen from its view model with the extension that uses the default contract.</summary>
    public static void ResolveTodoView()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        var locator = ViewLocator.GetCurrent();

        var view = locator.ResolveView(viewModel);

        SampleCheck.Equal(typeof(TodoView), view?.GetType());
        SampleCheck.Equal(viewModel, view?.ViewModel);
    }

    /// <summary>Resolves the issue board screen from a view model held as an object, as a navigation stack holds it.</summary>
    public static void ResolveGitHubViewFromObject()
    {
        object viewModel = new IssueBoardViewModel(InMemoryGitHubServer.CreateSeeded(ManualClock.StartOfWorkingDay()));
        var locator = ViewLocator.GetCurrent();

        var view = locator.ResolveView(viewModel);

        SampleCheck.Equal(typeof(IssueBoardView), view?.GetType());
        SampleCheck.Equal(viewModel, view?.ViewModel);
    }

    /// <summary>Resolves a banking screen with and without a contract: the contract picks the compact screen.</summary>
    public static void ResolveBankingViewByContract()
    {
        AccountsViewModel viewModel = new(new InMemoryBankingBackend(ManualClock.StartOfWorkingDay()));
        var locator = ViewLocator.GetCurrent();

        var standard = locator.ResolveView(viewModel, null);
        var compact = locator.ResolveView(viewModel, AccountViewContracts.Compact);

        SampleCheck.Equal(typeof(AccountsView), standard?.GetType());
        SampleCheck.Equal(typeof(CompactAccountsView), compact?.GetType());
        SampleCheck.Equal(viewModel, compact?.ViewModel);
    }

    /// <summary>Resolves the transfer screen through the object overload of the interface, which takes a contract.</summary>
    public static void ResolveTransferViewFromObject()
    {
        object viewModel = new TransferViewModel(new InMemoryBankingBackend(ManualClock.StartOfWorkingDay()));
        DefaultViewLocator locator = new();

        var view = locator.ResolveView(viewModel, null);

        SampleCheck.Equal(typeof(TransferView), view?.GetType());
    }

    /// <summary>Resolves a view model that has no screen: the locator answers null, and a host may turn that into an exception.</summary>
    public static void ResolveMissingView()
    {
        TodoItem note = new() { Title = "Renew car registration" };
        var locator = ViewLocator.GetCurrent();
        ViewLocatorNotFoundException? failure = null;

        var view = locator.ResolveView(note);

        try
        {
            _ = RequireView(locator, note);
        }
        catch (ViewLocatorNotFoundException ex)
        {
            failure = ex;
        }

        SampleCheck.Equal(true, view is null);
        SampleCheck.Equal("No screen is registered for TodoItem.", failure?.Message);
    }

    /// <summary>Resolves nothing for a missing view model.</summary>
    public static void ResolveNullViewModel()
    {
        var locator = ViewLocator.GetCurrent();

        var view = locator.ResolveView((object?)null);

        SampleCheck.Equal(true, view is null);
    }

    /// <summary>Wraps a failure to build a screen in the not-found exception, keeping the cause.</summary>
    public static void ExplainScreenThatFailsToBuild()
    {
        TodoItem viewModel = new() { Title = "Renew car registration" };
        DefaultViewLocator locator = new();
        locator.Map<TodoItem>(static () => throw new InvalidOperationException(LayoutFailure));
        ViewLocatorNotFoundException? failure = null;

        try
        {
            _ = ResolveOrExplain(locator, viewModel);
        }
        catch (ViewLocatorNotFoundException ex)
        {
            failure = ex;
        }

        SampleCheck.Equal("The screen for TodoItem could not be built.", failure?.Message);
        SampleCheck.Equal(LayoutFailure, failure?.InnerException?.Message);
    }

    /// <summary>Creates the exception without a message, where the message names the call that registers the locator.</summary>
    public static void CreateNotFoundExceptionWithDefaultMessage()
    {
        ViewLocatorNotFoundException failure = new();

        SampleCheck.Equal(true, failure.Message.Contains("BuildApp", StringComparison.Ordinal));
    }
}

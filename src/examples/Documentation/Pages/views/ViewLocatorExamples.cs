// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>Shows how a view model finds its screen: <c>IViewLocator</c>, <c>DefaultViewLocator</c>, <c>ViewLocator</c> and <c>ViewLocatorNotFoundException</c>.</summary>
public static class ViewLocatorExamples
{
    /// <summary>The title of the to-do item that has no screen.</summary>
    private const string NoteTitle = "Renew car registration";

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

        Console.WriteLine(failure is not null);
        Console.WriteLine(failure!.Message.Contains("WithCoreServices", StringComparison.Ordinal));

        // Output:
        // True
        // True
    }

    /// <summary>Registers the core services, which include the default view locator, and reads it back from <see cref="ViewLocator"/>.</summary>
    public static void RegisterDefaultLocator()
    {
        var builder = (IReactiveUIBindingBuilder)RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices().BuildApp();

        var locator = ViewLocator.GetCurrent();

        Console.WriteLine(locator is DefaultViewLocator);

        // Output:
        // True
    }

    /// <summary>Resolves the to-do screen from its view model with the extension that uses the default contract.</summary>
    public static void ResolveTodoView()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        var locator = ViewLocator.GetCurrent();

        var view = locator.ResolveView(viewModel);

        Console.WriteLine(view?.GetType().Name);
        Console.WriteLine(ReferenceEquals(view?.ViewModel, viewModel));

        // Output:
        // TodoView
        // True
    }

    /// <summary>Resolves the issue board screen from a view model held as an object, as a navigation stack holds it.</summary>
    public static void ResolveGitHubViewFromObject()
    {
        object viewModel = new IssueBoardViewModel(InMemoryGitHubServer.CreateSeeded());
        var locator = ViewLocator.GetCurrent();

        var view = locator.ResolveView(viewModel);

        Console.WriteLine(view?.GetType().Name);
        Console.WriteLine(ReferenceEquals(view?.ViewModel, viewModel));

        // Output:
        // IssueBoardView
        // True
    }

    /// <summary>Resolves the transfer screen through the object overload of the interface, which takes a contract.</summary>
    public static void ResolveTransferViewFromObject()
    {
        object viewModel = new TransferViewModel(new InMemoryBankingBackend());
        DefaultViewLocator locator = new();

        var view = locator.ResolveView(viewModel, null);

        Console.WriteLine(view?.GetType().Name);

        // Output:
        // TransferView
    }

    /// <summary>Resolves a view model that has no screen: the locator answers null, and a host may turn that into an exception.</summary>
    public static void ResolveMissingView()
    {
        TodoItem note = new() { Title = NoteTitle };
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

        Console.WriteLine(view is null);
        Console.WriteLine(failure?.Message);

        // Output:
        // True
        // No screen is registered for TodoItem.
    }

    /// <summary>Resolves nothing for a missing view model.</summary>
    public static void ResolveNullViewModel()
    {
        var locator = ViewLocator.GetCurrent();

        var view = locator.ResolveView((object?)null);

        Console.WriteLine(view is null);

        // Output:
        // True
    }

    /// <summary>Wraps a failure to build a screen in the not-found exception, keeping the cause.</summary>
    public static void ExplainScreenThatFailsToBuild()
    {
        TodoItem viewModel = new() { Title = NoteTitle };
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

        Console.WriteLine(failure?.Message);
        Console.WriteLine(failure?.InnerException?.Message);

        // Output:
        // The screen for TodoItem could not be built.
        // The layout file is missing.
    }

    /// <summary>Creates the exception without a message, where the message names the call that registers the locator.</summary>
    public static void CreateNotFoundExceptionWithDefaultMessage()
    {
        ViewLocatorNotFoundException failure = new();

        Console.WriteLine(failure.Message.Contains("BuildApp", StringComparison.Ordinal));

        // Output:
        // True
    }

    /// <summary>Implements <see cref="IViewLocator"/> for an application with one screen, and resolves through it like any other locator.</summary>
    public static void ResolveWithCustomLocator()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        TodoViewLocator locator = new();

        var view = locator.ResolveView(viewModel, null);
        var missing = locator.ResolveView(new TodoItem(), null);

        Console.WriteLine(view?.GetType().Name);
        Console.WriteLine(ReferenceEquals(view?.ViewModel, viewModel));
        Console.WriteLine(missing is null);

        // Output:
        // TodoView
        // True
        // True
    }
}

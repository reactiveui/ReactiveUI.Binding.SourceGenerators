// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.ViewsViewFor;

/// <summary>Shows how a screen declares the view model it displays: <c>IViewFor</c>, <c>IViewFor&lt;T&gt;</c> and <c>IActivatableView</c>.</summary>
public static class ViewsViewForExamples
{
    /// <summary>The title the user types into the new item box.</summary>
    private const string NewItemTitle = "Book vet appointment";

    /// <summary>The number of values the view model observation reports: the initial null and the assigned view model.</summary>
    private const int ObservedViewModelCount = 2;

    /// <summary>The number of screens in the activation check that a host may activate.</summary>
    private const int ActivatableScreenCount = 2;

    /// <summary>Gives a screen its view model through the typed interface, as a navigation service does.</summary>
    /// <typeparam name="TViewModel">The view model type the screen shows.</typeparam>
    /// <param name="view">The screen.</param>
    /// <param name="viewModel">The view model to show.</param>
    /// <returns>The view model the screen now shows.</returns>
    public static TViewModel? Present<TViewModel>(IViewFor<TViewModel> view, TViewModel viewModel)
        where TViewModel : class
    {
        view.ViewModel = viewModel;
        return view.ViewModel;
    }

    /// <summary>Counts the objects a host may activate, whatever else they are.</summary>
    /// <param name="candidates">The objects the host holds.</param>
    /// <returns>The number of objects that implement <see cref="IActivatableView"/>.</returns>
    public static int CountActivatable(IEnumerable<object> candidates)
    {
        var count = 0;
        foreach (var candidate in candidates)
        {
            if (candidate is IActivatableView)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Assigns a to-do list to the to-do screen through <see cref="IViewFor{T}"/>, where the view model is typed.</summary>
    public static void AssignTypedViewModel()
    {
        TodoView view = new();
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());

        SampleCheck.Equal(null, view.ViewModel);

        var shown = Present(view, viewModel);

        SampleCheck.Equal(viewModel, shown);
        SampleCheck.Equal(viewModel, view.ViewModel);
    }

    /// <summary>Observes the view model of the screen, which a view reports like any other property.</summary>
    public static void ObserveViewModelOfView()
    {
        TodoView view = new();
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        List<TodoListViewModel?> shown = [];

        using (view.WhenChanged(x => x.ViewModel!).Subscribe(shown.Add))
        {
            view.ViewModel = viewModel;
        }

        SampleCheck.Equal(ObservedViewModelCount, shown.Count);
        SampleCheck.Equal(null, shown[0]);
        SampleCheck.Equal(viewModel, shown[1]);
    }

    /// <summary>Assigns an issue board through the non-generic <see cref="IViewFor"/>, where the view model is an object.</summary>
    public static void AssignThroughNonGenericInterface()
    {
        IssueBoardView view = new();
        IssueBoardViewModel viewModel = new(InMemoryGitHubServer.CreateSeeded(ManualClock.StartOfWorkingDay()));
        var screen = (IViewFor)view;

        screen.ViewModel = viewModel;

        SampleCheck.Equal(viewModel, screen.ViewModel);
        SampleCheck.Equal(viewModel, view.ViewModel);
    }

    /// <summary>Shows that a screen refuses a view model of the wrong type when it is assigned through <see cref="IViewFor"/>.</summary>
    public static void RejectWrongViewModelType()
    {
        var screen = (IViewFor)new IssueBoardView();
        TodoListViewModel wrongViewModel = new(InMemoryTodoStore.CreateSeeded());
        var refused = false;

        try
        {
            screen.ViewModel = wrongViewModel;
        }
        catch (InvalidCastException)
        {
            refused = true;
        }

        SampleCheck.Equal(true, refused);
        SampleCheck.Equal(null, screen.ViewModel);
    }

    /// <summary>Recognizes the screens a host may activate: a view is activatable, a view model is not.</summary>
    public static void RecognizeActivatableViews()
    {
        List<object> candidates =
        [
            new TodoView(),
            new TodoListViewModel(InMemoryTodoStore.CreateSeeded()),
            new AccountsView(),
        ];

        SampleCheck.Equal(ActivatableScreenCount, CountActivatable(candidates));
    }

    /// <summary>Binds the to-do screen to its view model; the bindings need a view that implements <see cref="IViewFor"/>.</summary>
    public static void BindTodoView()
    {
        var store = InMemoryTodoStore.CreateSeeded();
        TodoListViewModel viewModel = new(store);
        TodoView view = new();
        viewModel.LoadCommand.Execute(null);

        // The bindings follow view.ViewModel, so the screen has to show the view model first.
        _ = Present(view, viewModel);

        using (view.OneWayBind(viewModel, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture)))
        using (view.Bind(viewModel, x => x.NewTitle, v => v.NewTitleTextBox.Text))
        {
            SampleCheck.Equal("3", view.RemainingLabel.Text);

            view.NewTitleTextBox.Text = NewItemTitle;

            SampleCheck.Equal(NewItemTitle, viewModel.NewTitle);
        }
    }

    /// <summary>Reads the labels of a banking screen from its view model through <see cref="IViewFor{T}"/>.</summary>
    /// <returns>A task that completes when the accounts are loaded and shown.</returns>
    public static async Task BindAccountsView()
    {
        InMemoryBankingBackend backend = new(ManualClock.StartOfWorkingDay());
        AccountsViewModel viewModel = new(backend);
        AccountsView view = new();
        _ = Present(view, viewModel);

        using (view.OneWayBind(viewModel, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, static total => total.ToString("N2", CultureInfo.InvariantCulture)))
        {
            viewModel.LoadAccountsCommand.Execute(null);
            await viewModel.LoadAccountsCommand.Completion;

            SampleCheck.Equal("17,680.75", view.TotalBalanceLabel.Text);
        }
    }
}

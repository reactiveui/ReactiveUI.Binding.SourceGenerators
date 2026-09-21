// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.UnsafeTriggerUpdate;

/// <summary>
/// Demonstrates <c>BindUnsafe</c> with an update stream. The stream decides when one direction of the binding runs, so
/// a view can hold an edit until the user commits it, or a view model change can wait until the screen refreshes.
/// </summary>
public static class UnsafeTriggerUpdateExamples
{
    /// <summary>The text a user types into the filter box.</summary>
    private const string FilterQuery = "car";

    /// <summary>The text a user types over <see cref="FilterQuery"/>.</summary>
    private const string SecondFilterQuery = "tax";

    /// <summary>The amount of a transfer, as the view shows it.</summary>
    private const string AmountText = "125.50";

    /// <summary>The amount of a transfer.</summary>
    private const decimal AmountValue = 125.50M;

    /// <summary>The name of the <c>Text</c> property of a text box.</summary>
    private const string TextPropertyName = nameof(Controls.TextBoxControl.Text);

    /// <summary>Registers the default property observation that a binding built at run time looks up.</summary>
    public static void BuildApplication()
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices().BuildApp();
    }

    /// <summary>
    /// Holds a view edit until the stream fires, then writes it to the view model. The view model is not held back.
    /// <see cref="TriggerUpdate.ViewToViewModel"/> is the default direction, so the call names none.
    /// </summary>
    public static void CommitViewEditOnSignal()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };
        Signal<EventArgs> commit = new();

        using (view.BindUnsafe(viewModel, x => x.FilterText, v => v.FilterTextBox.Text, commit))
        {
            view.FilterTextBox.Text = FilterQuery;

            SampleCheck.Equal(string.Empty, viewModel.FilterText);

            commit.OnNext(EventArgs.Empty);

            SampleCheck.Equal(FilterQuery, viewModel.FilterText);

            viewModel.FilterText = SecondFilterQuery;

            SampleCheck.Equal(SecondFilterQuery, view.FilterTextBox.Text);
        }
    }

    /// <summary>Holds a view model change until the stream fires, then writes it to the view. The view is not held back.</summary>
    public static void RefreshViewOnSignal()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };
        Signal<EventArgs> refresh = new();

        using (view.BindUnsafe(viewModel, x => x.FilterText, v => v.FilterTextBox.Text, refresh, TriggerUpdate.ViewModelToView))
        {
            viewModel.FilterText = FilterQuery;

            SampleCheck.Equal(string.Empty, view.FilterTextBox.Text);

            refresh.OnNext(EventArgs.Empty);

            SampleCheck.Equal(FilterQuery, view.FilterTextBox.Text);

            view.FilterTextBox.Text = SecondFilterQuery;

            SampleCheck.Equal(SecondFilterQuery, viewModel.FilterText);
        }
    }

    /// <summary>Reads the view model when the stream fires, so several changes in between reach the view as one write.</summary>
    public static void DeliverLatestViewModelValueOnSignal()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };
        Signal<EventArgs> refresh = new();
        var writes = 0;

        view.FilterTextBox.PropertyChanged += (_, e) => writes += e.PropertyName == TextPropertyName ? 1 : 0;

        using (view.BindUnsafe(viewModel, x => x.FilterText, v => v.FilterTextBox.Text, refresh, TriggerUpdate.ViewModelToView))
        {
            viewModel.FilterText = FilterQuery;
            viewModel.FilterText = SecondFilterQuery;
            refresh.OnNext(EventArgs.Empty);

            SampleCheck.Equal(SecondFilterQuery, view.FilterTextBox.Text);
            SampleCheck.Equal(1, writes);
        }
    }

    /// <summary>Delivers the view model value to the view when the binding starts, whichever direction the stream drives.</summary>
    public static void DeliverInitialViewModelValue()
    {
        var viewModel = CreateLoadedViewModel();
        viewModel.FilterText = FilterQuery;
        TodoView view = new() { ViewModel = viewModel };
        Signal<EventArgs> commit = new();

        using (view.BindUnsafe(viewModel, x => x.FilterText, v => v.FilterTextBox.Text, commit))
        {
            SampleCheck.Equal(FilterQuery, view.FilterTextBox.Text);
        }
    }

    /// <summary>Observes both properties when no stream is given, as <c>Bind</c> does.</summary>
    public static void ObserveBothPropertiesWithoutSignal()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };
        IObservable<EventArgs>? noSignal = default;

        using (view.BindUnsafe(viewModel, x => x.FilterText, v => v.FilterTextBox.Text, noSignal))
        {
            view.FilterTextBox.Text = FilterQuery;

            SampleCheck.Equal(FilterQuery, viewModel.FilterText);

            viewModel.FilterText = SecondFilterQuery;

            SampleCheck.Equal(SecondFilterQuery, view.FilterTextBox.Text);
        }
    }

    /// <summary>Commits an amount typed as text, with a conversion function for each direction.</summary>
    public static void CommitConvertedViewEditOnSignal()
    {
        TransferViewModel viewModel = new(CreateBackend());
        TransferView view = new() { ViewModel = viewModel };
        Signal<EventArgs> commit = new();

        using (view.BindUnsafe(
            viewModel,
            x => x.Draft.Amount,
            v => v.AmountTextBox.Text,
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture),
            commit))
        {
            view.AmountTextBox.Text = AmountText;

            SampleCheck.Equal(decimal.Zero, viewModel.Draft.Amount);

            commit.OnNext(EventArgs.Empty);

            SampleCheck.Equal(AmountValue, viewModel.Draft.Amount);
        }
    }

    /// <summary>Refreshes an amount shown as text when the stream fires, with a conversion function for each direction.</summary>
    public static void RefreshConvertedViewOnSignal()
    {
        TransferViewModel viewModel = new(CreateBackend());
        TransferView view = new() { ViewModel = viewModel };
        Signal<EventArgs> refresh = new();

        using (view.BindUnsafe(
            viewModel,
            x => x.Draft.Amount,
            v => v.AmountTextBox.Text,
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture),
            refresh,
            TriggerUpdate.ViewModelToView))
        {
            viewModel.Draft.Amount = AmountValue;

            SampleCheck.Equal("0.00", view.AmountTextBox.Text);

            refresh.OnNext(EventArgs.Empty);

            SampleCheck.Equal(AmountText, view.AmountTextBox.Text);
        }
    }

    /// <summary>Creates a view model that has loaded the seeded items.</summary>
    /// <returns>The loaded view model.</returns>
    private static TodoListViewModel CreateLoadedViewModel()
    {
        var viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());

        viewModel.LoadCommand.Execute(null);
        return viewModel;
    }

    /// <summary>Creates a bank whose accounts are ready to read.</summary>
    /// <returns>The banking backend.</returns>
    private static InMemoryBankingBackend CreateBackend() => new(ManualClock.StartOfWorkingDay());
}

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.UnsafeIndex;

/// <summary>
/// Demonstrates the <c>Unsafe</c> binding overloads that take a sequencer and the overloads that take converters.
/// Each write to the target waits on the sequencer until the example runs it, as a UI thread does between two turns
/// of its message loop. Each property path here is built while the app runs.
/// </summary>
public static class UnsafeSequencerExamples
{
    /// <summary>The title of the first seeded item.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The text of a filter.</summary>
    private const string FilterQuery = "car";

    /// <summary>The text that replaces <see cref="FilterQuery"/>.</summary>
    private const string SecondFilterQuery = "tax";

    /// <summary>The amount of a transfer, as the view shows it.</summary>
    private const string AmountText = "125.50";

    /// <summary>The amount of a transfer.</summary>
    private const decimal AmountValue = 125.50M;

    /// <summary>The amount a user types over <see cref="AmountText"/>.</summary>
    private const string SecondAmountText = "10.00";

    /// <summary>The amount a user types over <see cref="AmountValue"/>.</summary>
    private const decimal SecondAmountValue = 10.00M;

    /// <summary>The name of the <see cref="TodoView.FilterTextBox"/> property.</summary>
    private const string FilterTextBoxName = nameof(TodoView.FilterTextBox);

    /// <summary>The name of the <see cref="TodoView.RemainingLabel"/> property.</summary>
    private const string RemainingLabelName = nameof(TodoView.RemainingLabel);

    /// <summary>The name of the <see cref="TodoView.SelectedTitleTextBox"/> property.</summary>
    private const string SelectedTitleTextBoxName = nameof(TodoView.SelectedTitleTextBox);

    /// <summary>The name of the <see cref="TransferViewModel.Draft"/> property.</summary>
    private const string DraftName = nameof(TransferViewModel.Draft);

    /// <summary>The name of the <see cref="TransferView.AmountTextBox"/> property.</summary>
    private const string AmountTextBoxName = nameof(TransferView.AmountTextBox);

    /// <summary>The name of the <c>Text</c> property of a control.</summary>
    private const string TextName = nameof(Controls.TextBoxControl.Text);

    /// <summary>Writes a source property to a target property on a sequencer.</summary>
    public static void BindOneWayOnSequencer()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();
        QueuedSequencer sequencer = new();
        var source = RuntimePath<TodoListViewModel, string>.Of(nameof(TodoListViewModel.FilterText));
        var target = RuntimePath<TodoView, string>.Of(FilterTextBoxName, TextName);

        using (viewModel.BindOneWayUnsafe(view, source, target, sequencer))
        {
            viewModel.FilterText = FilterQuery;

            SampleCheck.Equal(string.Empty, view.FilterTextBox.Text);

            _ = sequencer.RunPending();

            SampleCheck.Equal(FilterQuery, view.FilterTextBox.Text);
        }
    }

    /// <summary>Writes a count to a label as text on a sequencer, through a conversion function.</summary>
    public static void BindOneWayWithConversionOnSequencer()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();
        QueuedSequencer sequencer = new();
        var source = RuntimePath<TodoListViewModel, int>.Of(nameof(TodoListViewModel.RemainingCount));
        var target = RuntimePath<TodoView, string>.Of(RemainingLabelName, TextName);

        using (viewModel.BindOneWayUnsafe(view, source, target, static count => count.ToString(CultureInfo.InvariantCulture), sequencer))
        {
            SampleCheck.Equal(string.Empty, view.RemainingLabel.Text);

            _ = sequencer.RunPending();

            SampleCheck.Equal("3", view.RemainingLabel.Text);
        }
    }

    /// <summary>Writes the selected item to a text property on a sequencer, through a converter you name and a conversion hint.</summary>
    public static void BindOneWayWithConverterOnSequencer()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();
        QueuedSequencer sequencer = new();
        var source = RuntimePath<TodoListViewModel, TodoItem?>.Of(nameof(TodoListViewModel.SelectedItem));
        var target = RuntimePath<TodoView, string>.Of(SelectedTitleTextBoxName, TextName);

        using (viewModel.BindOneWayUnsafe(view, source, target, new TodoItemTitleConverter(), sequencer, TodoItemTitleConverter.UpperCaseHint))
        {
            viewModel.SelectedItem = viewModel.Items[0];
            _ = sequencer.RunPending();

            SampleCheck.Equal(OriginalTitle.ToUpperInvariant(), view.SelectedTitleTextBox.Text);
        }
    }

    /// <summary>Carries a property both ways; the write to the target waits on a sequencer and the write back does not.</summary>
    public static void BindTwoWayOnSequencer()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();
        QueuedSequencer sequencer = new();
        var source = RuntimePath<TodoListViewModel, string>.Of(nameof(TodoListViewModel.FilterText));
        var target = RuntimePath<TodoView, string>.Of(FilterTextBoxName, TextName);

        using (viewModel.BindTwoWayUnsafe(view, source, target, sequencer))
        {
            viewModel.FilterText = FilterQuery;

            SampleCheck.Equal(string.Empty, view.FilterTextBox.Text);

            _ = sequencer.RunPending();

            SampleCheck.Equal(FilterQuery, view.FilterTextBox.Text);

            view.FilterTextBox.Text = SecondFilterQuery;

            SampleCheck.Equal(SecondFilterQuery, viewModel.FilterText);
        }
    }

    /// <summary>Carries an amount both ways as text on a sequencer, with a conversion function for each direction.</summary>
    public static void BindTwoWayWithConversionsOnSequencer()
    {
        TransferViewModel viewModel = new(CreateBackend());
        TransferView view = new();
        QueuedSequencer sequencer = new();
        var source = RuntimePath<TransferViewModel, decimal>.Of(DraftName, nameof(TransferDraft.Amount));
        var target = RuntimePath<TransferView, string>.Of(AmountTextBoxName, TextName);

        using (viewModel.BindTwoWayUnsafe(
            view,
            source,
            target,
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture),
            sequencer))
        {
            _ = sequencer.RunPending();
            viewModel.Draft.Amount = AmountValue;
            _ = sequencer.RunPending();

            SampleCheck.Equal(AmountText, view.AmountTextBox.Text);
        }
    }

    /// <summary>Carries an amount both ways as text on a sequencer, through a converter for each direction and a conversion hint.</summary>
    public static void BindTwoWayWithConvertersOnSequencer()
    {
        TransferViewModel viewModel = new(CreateBackend());
        TransferView view = new();
        QueuedSequencer sequencer = new();
        var source = RuntimePath<TransferViewModel, decimal>.Of(DraftName, nameof(TransferDraft.Amount));
        var target = RuntimePath<TransferView, string>.Of(AmountTextBoxName, TextName);

        using (viewModel.BindTwoWayUnsafe(view, source, target, new DecimalToStringTypeConverter(), new StringToDecimalTypeConverter(), sequencer, null))
        {
            _ = sequencer.RunPending();
            viewModel.Draft.Amount = AmountValue;
            _ = sequencer.RunPending();

            SampleCheck.Equal(AmountText, view.AmountTextBox.Text);

            view.AmountTextBox.Text = SecondAmountText;

            SampleCheck.Equal(SecondAmountValue, viewModel.Draft.Amount);
        }
    }

    /// <summary>Carries an amount both ways as text, written view first, on a sequencer, with a conversion function for each direction.</summary>
    public static void BindWithConversionsOnSequencer()
    {
        TransferViewModel viewModel = new(CreateBackend());
        TransferView view = new() { ViewModel = viewModel };
        QueuedSequencer sequencer = new();
        var source = RuntimePath<TransferViewModel, decimal>.Of(DraftName, nameof(TransferDraft.Amount));
        var target = RuntimePath<TransferView, string>.Of(AmountTextBoxName, TextName);

        using (view.BindUnsafe(
            viewModel,
            source,
            target,
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture),
            sequencer))
        {
            _ = sequencer.RunPending();
            viewModel.Draft.Amount = AmountValue;
            _ = sequencer.RunPending();

            SampleCheck.Equal(AmountText, view.AmountTextBox.Text);
        }
    }

    /// <summary>Carries an amount both ways as text, written view first, on a sequencer, through a converter for each direction.</summary>
    public static void BindWithConvertersOnSequencer()
    {
        TransferViewModel viewModel = new(CreateBackend());
        TransferView view = new() { ViewModel = viewModel };
        QueuedSequencer sequencer = new();
        var source = RuntimePath<TransferViewModel, decimal>.Of(DraftName, nameof(TransferDraft.Amount));
        var target = RuntimePath<TransferView, string>.Of(AmountTextBoxName, TextName);

        using (view.BindUnsafe(viewModel, source, target, new DecimalToStringTypeConverter(), new StringToDecimalTypeConverter(), sequencer, null))
        {
            _ = sequencer.RunPending();
            viewModel.Draft.Amount = AmountValue;
            _ = sequencer.RunPending();

            SampleCheck.Equal(AmountText, view.AmountTextBox.Text);
        }
    }

    /// <summary>Binds a count to a label as text, written view first, on a sequencer, through a selector.</summary>
    public static void OneWayBindWithSelectorOnSequencer()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };
        QueuedSequencer sequencer = new();
        var source = RuntimePath<TodoListViewModel, int>.Of(nameof(TodoListViewModel.RemainingCount));
        var target = RuntimePath<TodoView, string>.Of(RemainingLabelName, TextName);

        using (view.OneWayBindUnsafe(viewModel, source, target, static count => $"{count} left", sequencer))
        {
            SampleCheck.Equal(string.Empty, view.RemainingLabel.Text);

            _ = sequencer.RunPending();

            SampleCheck.Equal("3 left", view.RemainingLabel.Text);
        }
    }

    /// <summary>Binds the selected item to a text property, written view first, on a sequencer, through a converter you name and a conversion hint.</summary>
    public static void OneWayBindWithConverterOnSequencer()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };
        QueuedSequencer sequencer = new();
        var source = RuntimePath<TodoListViewModel, TodoItem?>.Of(nameof(TodoListViewModel.SelectedItem));
        var target = RuntimePath<TodoView, string>.Of(SelectedTitleTextBoxName, TextName);

        using (view.OneWayBindUnsafe(viewModel, source, target, new TodoItemTitleConverter(), sequencer, TodoItemTitleConverter.UpperCaseHint))
        {
            viewModel.SelectedItem = viewModel.Items[0];
            _ = sequencer.RunPending();

            SampleCheck.Equal(OriginalTitle.ToUpperInvariant(), view.SelectedTitleTextBox.Text);
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

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.FallbackRuntime;

/// <summary>
/// Demonstrates the runtime fallback that serves a call the generator could not read. The fallback types are the
/// engine behind every <c>Unsafe</c> method; you can call them directly with an expression and the text to report when
/// a write fails.
/// </summary>
public static class FallbackRuntimeExamples
{
    /// <summary>The title of the first seeded item.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The text of a filter.</summary>
    private const string FilterQuery = "car";

    /// <summary>The text that replaces <see cref="FilterQuery"/>.</summary>
    private const string SecondFilterQuery = "tax";

    /// <summary>The title an example adds through a bound button.</summary>
    private const string NewItemTitle = "Water the plants";

    /// <summary>The amount of a transfer, as the view shows it.</summary>
    private const string AmountText = "125.50";

    /// <summary>The amount of a transfer.</summary>
    private const decimal AmountValue = 125.50M;

    /// <summary>The number of items after the seeded four and one added one.</summary>
    private const int ItemsAfterAdd = 5;

    /// <summary>The number of unfinished items after the first seeded item is finished.</summary>
    private const int RemainingAfterComplete = 2;

    /// <summary>The text a failed write reports as the bound expression, for the filter.</summary>
    private const string FilterExpression = "x => x.FilterText";

    /// <summary>The text a failed write reports as the bound expression, for the amount.</summary>
    private const string AmountExpression = "x => x.Draft.Amount";

    /// <summary>The text a failed write reports as the bound expression, for the selected title.</summary>
    private const string SelectedTitleExpression = "v => v.SelectedTitleTextBox.Text";

    /// <summary>The text a failed write reports as the bound expression, for the add command.</summary>
    private const string AddCommandExpression = "x => x.AddCommand";

    /// <summary>The name of the click event of a button.</summary>
    private const string ClickEventName = "Click";

    /// <summary>Registers the default property observation, a converter and a command binder that the runtime path looks up.</summary>
    public static void BuildApplication()
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder
            .WithCoreServices()
            .WithConverter(new TodoItemTitleConverter())
            .WithCommandBinder(new ButtonClickCommandBinder())
            .BuildApp();
    }

    /// <summary>Writes a source property to a target property on the thread that owns the target.</summary>
    public static void BindOneWayBetweenProperties()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();

        using (RuntimeBindingFallback.BindOneWay(viewModel, view, x => x.FilterText, v => v.FilterTextBox.Text, null, FilterExpression))
        {
            viewModel.FilterText = FilterQuery;
        }

        SampleCheck.Equal(FilterQuery, view.FilterTextBox.Text);
    }

    /// <summary>Delivers each write on a sequencer you supply, here one that runs its work when the example asks.</summary>
    public static void BindOneWayOnSequencer()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();
        QueuedSequencer sequencer = new();

        using (RuntimeBindingFallback.BindOneWay(viewModel, view, x => x.FilterText, v => v.FilterTextBox.Text, sequencer, FilterExpression))
        {
            viewModel.FilterText = FilterQuery;

            SampleCheck.Equal(string.Empty, view.FilterTextBox.Text);

            _ = sequencer.RunPending();

            SampleCheck.Equal(FilterQuery, view.FilterTextBox.Text);
        }
    }

    /// <summary>Writes a count to a label as text, through a conversion function.</summary>
    public static void BindOneWayWithConversion()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();

        using (RuntimeBindingFallback.BindOneWay(
            viewModel,
            view,
            x => x.RemainingCount,
            v => v.RemainingLabel.Text,
            static count => count.ToString(CultureInfo.InvariantCulture),
            null,
            "x => x.RemainingCount"))
        {
            SampleCheck.Equal("3", view.RemainingLabel.Text);
        }
    }

    /// <summary>Carries a property both ways between two objects.</summary>
    public static void BindTwoWayBetweenProperties()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();

        using (RuntimeBindingFallback.BindTwoWay(viewModel, view, x => x.FilterText, v => v.FilterTextBox.Text, null, FilterExpression))
        {
            view.FilterTextBox.Text = FilterQuery;

            SampleCheck.Equal(FilterQuery, viewModel.FilterText);

            viewModel.FilterText = SecondFilterQuery;

            SampleCheck.Equal(SecondFilterQuery, view.FilterTextBox.Text);
        }
    }

    /// <summary>Carries an amount both ways as text, with a pair of conversions.</summary>
    public static void BindTwoWayWithConverterPair()
    {
        TransferViewModel viewModel = new(CreateBackend());
        TransferView view = new();

        using (RuntimeBindingFallback.BindTwoWay(viewModel, view, x => x.Draft.Amount, v => v.AmountTextBox.Text, CreateAmountConverters(), null, AmountExpression))
        {
            view.AmountTextBox.Text = AmountText;

            SampleCheck.Equal(AmountValue, viewModel.Draft.Amount);
        }
    }

    /// <summary>Binds a view model property to a view property, written view first.</summary>
    public static void OneWayBindToView()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };

        using (RuntimeBindingFallback.OneWayBind(view, viewModel, x => x.FilterText, v => v.FilterTextBox.Text, null, FilterExpression))
        {
            viewModel.FilterText = FilterQuery;
        }

        SampleCheck.Equal(FilterQuery, view.FilterTextBox.Text);
    }

    /// <summary>Binds a count to a label as text, written view first, through a conversion function.</summary>
    public static void OneWayBindToViewWithConversion()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };

        using (RuntimeBindingFallback.OneWayBind(
            view,
            viewModel,
            x => x.RemainingCount,
            v => v.RemainingLabel.Text,
            static count => $"{count} left",
            null,
            "x => x.RemainingCount"))
        {
            SampleCheck.Equal("3 left", view.RemainingLabel.Text);
        }
    }

    /// <summary>Carries a property both ways, written view first.</summary>
    public static void BindViewAndViewModel()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };

        using (RuntimeBindingFallback.Bind(view, viewModel, x => x.FilterText, v => v.FilterTextBox.Text, null, FilterExpression))
        {
            view.FilterTextBox.Text = FilterQuery;

            SampleCheck.Equal(FilterQuery, viewModel.FilterText);
        }
    }

    /// <summary>Carries an amount both ways as text, written view first, with a pair of conversions.</summary>
    public static void BindViewAndViewModelWithConverterPair()
    {
        TransferViewModel viewModel = new(CreateBackend());
        TransferView view = new() { ViewModel = viewModel };

        using (RuntimeBindingFallback.Bind(view, viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, CreateAmountConverters(), null, AmountExpression))
        {
            view.AmountTextBox.Text = AmountText;

            SampleCheck.Equal(AmountValue, viewModel.Draft.Amount);
        }
    }

    /// <summary>Holds a view edit until a stream fires, then writes it to the view model.</summary>
    public static void BindViewAndViewModelOnSignal()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };
        Signal<EventArgs> commit = new();

        using (RuntimeBindingFallback.Bind(view, viewModel, x => x.FilterText, v => v.FilterTextBox.Text, commit, TriggerUpdate.ViewToViewModel))
        {
            view.FilterTextBox.Text = FilterQuery;

            SampleCheck.Equal(string.Empty, viewModel.FilterText);

            commit.OnNext(EventArgs.Empty);

            SampleCheck.Equal(FilterQuery, viewModel.FilterText);
        }
    }

    /// <summary>Holds an amount until a stream fires, then writes it to the view as text, with a pair of conversions.</summary>
    public static void BindViewAndViewModelWithConverterPairOnSignal()
    {
        TransferViewModel viewModel = new(CreateBackend());
        TransferView view = new() { ViewModel = viewModel };
        Signal<EventArgs> refresh = new();

        using (RuntimeBindingFallback.Bind(view, viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, CreateAmountConverters(), refresh, TriggerUpdate.ViewModelToView))
        {
            viewModel.Draft.Amount = AmountValue;

            SampleCheck.Equal("0.00", view.AmountTextBox.Text);

            refresh.OnNext(EventArgs.Empty);

            SampleCheck.Equal(AmountText, view.AmountTextBox.Text);
        }
    }

    /// <summary>Writes each selected item of a stream to a text property, through the converter registered for the two types.</summary>
    public static void BindStreamToProperty()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();
        var selected = RuntimeObservationFallback.WhenChanged(viewModel, x => x.SelectedItem);

        using (RuntimeBindingFallback.BindTo(selected, view, v => v.SelectedTitleTextBox.Text, null, null, null, SelectedTitleExpression))
        {
            viewModel.SelectedItem = viewModel.Items[0];

            SampleCheck.Equal(OriginalTitle, view.SelectedTitleTextBox.Text);
        }
    }

    /// <summary>Writes each selected item of a stream to a text property, through a converter you name and a conversion hint.</summary>
    public static void BindStreamToPropertyWithHintAndConverter()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();
        var selected = RuntimeObservationFallback.WhenChanged(viewModel, x => x.SelectedItem);

        using (RuntimeBindingFallback.BindTo(
            selected,
            view,
            v => v.SelectedTitleTextBox.Text,
            TodoItemTitleConverter.UpperCaseHint,
            new TodoItemTitleConverter(),
            null,
            SelectedTitleExpression))
        {
            viewModel.SelectedItem = viewModel.Items[0];

            SampleCheck.Equal(OriginalTitle.ToUpperInvariant(), view.SelectedTitleTextBox.Text);
        }
    }

    /// <summary>Drops the values of a stream when the target is null, as a view has no property to write before it exists.</summary>
    public static void BindStreamToNullTarget()
    {
        var viewModel = CreateLoadedViewModel();
        var selected = RuntimeObservationFallback.WhenChanged(viewModel, x => x.SelectedItem);
        TodoView? missing = default;

        using (RuntimeBindingFallback.BindTo(selected, missing, v => v.SelectedTitleTextBox.Text, null, null, null, SelectedTitleExpression))
        {
            viewModel.SelectedItem = viewModel.Items[0];

            SampleCheck.Equal(true, ReferenceEquals(viewModel.Items[0], viewModel.SelectedItem));
        }
    }

    /// <summary>Converts a value with the converter registered for the two types.</summary>
    public static void ConvertWithRegisteredConverter()
    {
        var item = new TodoItem { Title = OriginalTitle };

        var converted = RuntimeBindingConverter.TryConvert<TodoItem, string>(item, null, null, out var text);

        SampleCheck.Equal(true, converted);
        SampleCheck.Equal(OriginalTitle, text);
    }

    /// <summary>Converts a value with a conversion hint that the converter reads.</summary>
    public static void ConvertWithConversionHint()
    {
        var item = new TodoItem { Title = OriginalTitle };

        var converted = RuntimeBindingConverter.TryConvert<TodoItem, string>(item, TodoItemTitleConverter.UpperCaseHint, null, out var text);

        SampleCheck.Equal(true, converted);
        SampleCheck.Equal(OriginalTitle.ToUpperInvariant(), text);
    }

    /// <summary>Converts a value with a converter you name, which takes precedence over the registered ones.</summary>
    public static void ConvertWithConverterOverride()
    {
        var item = new TodoItem { Title = OriginalTitle };

        var converted = RuntimeBindingConverter.TryConvert<TodoItem, string>(item, TodoItemTitleConverter.UpperCaseHint, new TodoItemTitleConverter(), out var text);

        SampleCheck.Equal(true, converted);
        SampleCheck.Equal(OriginalTitle.ToUpperInvariant(), text);
    }

    /// <summary>Reports failure when no converter is registered for the two types.</summary>
    public static void ConvertWithoutConverterFails()
    {
        var item = new TodoItem { Title = OriginalTitle };

        var converted = RuntimeBindingConverter.TryConvert<TodoItem, int>(item, null, null, out var number);

        SampleCheck.Equal(false, converted);
        SampleCheck.Equal(0, number);
    }

    /// <summary>Pairs a forward and a reverse conversion with <see cref="TwoWayConverters.Create"/>, which infers both types.</summary>
    public static void CreateConverterPairWithFactory()
    {
        var pair = TwoWayConverters.Create<decimal, string>(
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture));

        SampleCheck.Equal(AmountText, pair.Forward(AmountValue));
        SampleCheck.Equal(AmountValue, pair.Reverse(AmountText));
    }

    /// <summary>Pairs a forward and a reverse conversion with the constructor; a pair is a value that compares by its two functions.</summary>
    public static void CreateConverterPairWithConstructor()
    {
        Func<decimal, string> forward = static amount => amount.ToString("F2", CultureInfo.InvariantCulture);
        Func<string, decimal> reverse = static text => decimal.Parse(text, CultureInfo.InvariantCulture);
        TwoWayConverterPair<decimal, string> pair = new(forward, reverse);
        TwoWayConverterPair<decimal, string> same = new(forward, reverse);
        var swapped = pair with { Forward = static amount => amount.ToString("F0", CultureInfo.InvariantCulture) };

        SampleCheck.Equal(pair, same);
        SampleCheck.Equal(false, pair.Equals(swapped));
        SampleCheck.Equal("126", swapped.Forward(AmountValue));
    }

    /// <summary>Binds a command to a button; each press runs the command.</summary>
    /// <returns>A task that completes when the command has run.</returns>
    public static async Task BindCommandToButton()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };

        using (RuntimeCommandBindingFallback.BindCommand(view, viewModel, x => x.AddCommand, v => v.AddButton, Signal.Never<object?>(), null, AddCommandExpression))
        {
            viewModel.NewTitle = NewItemTitle;
            view.AddButton.Press();
            await viewModel.AddCommand.Completion;
        }

        SampleCheck.Equal(ItemsAfterAdd, viewModel.Items.Count);
    }

    /// <summary>Binds a command to a named event of a button.</summary>
    /// <returns>A task that completes when the command has run.</returns>
    public static async Task BindCommandToButtonEvent()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };

        using (RuntimeCommandBindingFallback.BindCommand(view, viewModel, x => x.AddCommand, v => v.AddButton, Signal.Never<object?>(), ClickEventName, AddCommandExpression))
        {
            viewModel.NewTitle = NewItemTitle;
            view.AddButton.Press();
            await viewModel.AddCommand.Completion;
        }

        SampleCheck.Equal(ItemsAfterAdd, viewModel.Items.Count);
    }

    /// <summary>Does nothing while there is no view model, the ordinary state before one is assigned.</summary>
    public static void BindCommandWithoutViewModel()
    {
        TodoView view = new();
        TodoListViewModel? missing = default;

        using (RuntimeCommandBindingFallback.BindCommand(view, missing, x => x.AddCommand, v => v.AddButton, Signal.Never<object?>(), null, AddCommandExpression))
        {
            SampleCheck.Equal(true, view.AddButton.Command is null);
        }
    }

    /// <summary>Runs a command for each selected item of a stream.</summary>
    /// <returns>A task that completes when the command has run.</returns>
    public static async Task InvokeCommandForStream()
    {
        var viewModel = CreateLoadedViewModel();
        var selected = RuntimeObservationFallback.WhenAnyValue(viewModel, x => x.SelectedItem);

        using (RuntimeCommandFallback.InvokeCommand(selected, viewModel, x => x.CompleteCommand))
        {
            viewModel.SelectedItem = viewModel.Items[0];
            await viewModel.CompleteCommand.Completion;
        }

        SampleCheck.Equal(RemainingAfterComplete, viewModel.RemainingCount);
    }

    /// <summary>Registers a handler against the interaction a property holds.</summary>
    /// <returns>A task that completes when the interaction has been answered.</returns>
    public static async Task BindInteractionToHandler()
    {
        TransferViewModel viewModel = new(CreateBackend());

        using (RuntimeInteractionFallback.BindInteraction(
            viewModel,
            x => x.ConfirmTransfer,
            static interaction => interaction.RegisterHandler(static context =>
            {
                context.SetOutput(true);
                return Task.CompletedTask;
            }),
            "x => x.ConfirmTransfer"))
        {
            var confirmed = await viewModel.ConfirmTransfer.Handle(viewModel.Draft);

            SampleCheck.Equal(true, confirmed);
        }
    }

    /// <summary>Builds the conversions that show an amount as text.</summary>
    /// <returns>The forward and reverse conversions.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TwoWayConverterPair<decimal, string> CreateAmountConverters() =>
        TwoWayConverters.Create<decimal, string>(
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture));

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

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;
using Splat;

namespace ReactiveUI.Binding.Documentation.ThreadingSchedulers;

/// <summary>Shows where a binding delivers its writes: <c>BindingSchedulers</c> and the scheduler overloads of <c>ReactiveSchedulerExtensions</c>.</summary>
public static class ThreadingSchedulersExamples
{
    /// <summary>The title of the item before the sync.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title the user types on the UI thread.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The title a background sync delivers.</summary>
    private const string SyncedTitle = "Renew car registration (reviewed)";

    /// <summary>The reference of the transfer the customer is filling in.</summary>
    private const string RentReference = "Rent March";

    /// <summary>The reference the customer types over the first one.</summary>
    private const string RentAprilReference = "Rent April";

    /// <summary>The reference that replaces the April one before the sequencer runs.</summary>
    private const string RentMayReference = "Rent May";

    /// <summary>The format that shows an amount with two decimal places.</summary>
    private const string TwoDecimalPlaces = "F2";

    /// <summary>The amount of the transfer the customer is filling in.</summary>
    private const decimal RentAmount = 1200M;

    /// <summary>The amount the customer types over the first one.</summary>
    private const decimal RaisedRentAmount = 1250.50M;

    /// <summary>The text of the amount the customer types over the first one.</summary>
    private const string RaisedRentAmountText = "1250.50";

    /// <summary>The text of the first amount.</summary>
    private const string RentAmountText = "1200.00";

    /// <summary>The number of remaining items in the seeded to-do list.</summary>
    private const string SeededRemainingCount = "3";

    /// <summary>Runs work on another thread and waits for it to finish.</summary>
    /// <param name="work">The work to run.</param>
    public static void RunOnWorkerThread(Action work)
    {
        Thread worker = new(work.Invoke);
        worker.Start();
        worker.Join();
    }

    /// <summary>Registers the default property observation that a binding resolved at run time looks up.</summary>
    public static void BuildApplication()
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices().BuildApp();
    }

    /// <summary>Delivers writes from another thread on the host's main-thread sequencer, when the control is one the UI thread owns.</summary>
    public static void SetMainThread()
    {
        UiDispatcher dispatcher = new();
        DispatcherLabelControl titleLabel = new(dispatcher);
        TodoItem item = new() { Title = OriginalTitle };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());
        AppLocator.CurrentMutable.RegisterConstant<IViewThreadInvoker>(new UiDispatcherViewThreadInvoker(dispatcher));
        ViewThreadInvokers.Refresh();
        BindingSchedulers.MainThread = sequencer;

        try
        {
            using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text))
            {
                RunOnWorkerThread(() => item.Title = SyncedTitle);

                SampleCheck.Equal(sequencer, BindingSchedulers.MainThread);
                SampleCheck.Equal(OriginalTitle, titleLabel.Text);
                SampleCheck.Equal(1, sequencer.PendingCount);
                SampleCheck.Equal(0, dispatcher.PendingCount);

                _ = sequencer.RunPending();

                SampleCheck.Equal(SyncedTitle, titleLabel.Text);
            }
        }
        finally
        {
            BindingSchedulers.MainThread = null;
            AppLocator.CurrentMutable.UnregisterAll<IViewThreadInvoker>();
            ViewThreadInvokers.Refresh();
        }
    }

    /// <summary>Writes on the owning thread: the sequencer never sees the write, because there is nothing to carry.</summary>
    public static void WriteOnOwningThreadSkipsMainThread()
    {
        UiDispatcher dispatcher = new();
        DispatcherLabelControl titleLabel = new(dispatcher);
        TodoItem item = new() { Title = OriginalTitle };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());
        AppLocator.CurrentMutable.RegisterConstant<IViewThreadInvoker>(new UiDispatcherViewThreadInvoker(dispatcher));
        ViewThreadInvokers.Refresh();
        BindingSchedulers.MainThread = sequencer;

        try
        {
            using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text))
            {
                item.Title = RenamedTitle;

                SampleCheck.Equal(RenamedTitle, titleLabel.Text);
                SampleCheck.Equal(0, sequencer.PendingCount);
            }
        }
        finally
        {
            BindingSchedulers.MainThread = null;
            AppLocator.CurrentMutable.UnregisterAll<IViewThreadInvoker>();
            ViewThreadInvokers.Refresh();
        }
    }

    /// <summary>Writes to a control no invoker claims: the write runs on the thread that raised it, and the sequencer never sees it.</summary>
    public static void WriteToUnclaimedControlSkipsMainThread()
    {
        LabelControl titleLabel = new();
        TodoItem item = new() { Title = OriginalTitle };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());
        BindingSchedulers.MainThread = sequencer;

        try
        {
            using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text))
            {
                RunOnWorkerThread(() => item.Title = SyncedTitle);

                SampleCheck.Equal(SyncedTitle, titleLabel.Text);
                SampleCheck.Equal(0, sequencer.PendingCount);
            }
        }
        finally
        {
            BindingSchedulers.MainThread = null;
        }
    }

    /// <summary>Delivers writes from another thread through the synchronization context of the UI thread.</summary>
    public static void UseSynchronizationContext()
    {
        UiDispatcher dispatcher = new();
        DispatcherLabelControl titleLabel = new(dispatcher);
        TodoItem item = new() { Title = OriginalTitle };
        DispatcherSynchronizationContext context = new(dispatcher);
        AppLocator.CurrentMutable.RegisterConstant<IViewThreadInvoker>(new UiDispatcherViewThreadInvoker(dispatcher));
        ViewThreadInvokers.Refresh();
        BindingSchedulers.UseSynchronizationContext(context);

        try
        {
            SampleCheck.Equal(true, BindingSchedulers.MainThread is SynchronizationContextSequencer);

            using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text))
            {
                RunOnWorkerThread(() => item.Title = SyncedTitle);

                SampleCheck.Equal(OriginalTitle, titleLabel.Text);
                SampleCheck.Equal(1, dispatcher.PendingCount);

                _ = dispatcher.RunPending();

                SampleCheck.Equal(SyncedTitle, titleLabel.Text);
            }
        }
        finally
        {
            BindingSchedulers.UseSynchronizationContext(null);
            AppLocator.CurrentMutable.UnregisterAll<IViewThreadInvoker>();
            ViewThreadInvokers.Refresh();
        }

        SampleCheck.Equal(true, BindingSchedulers.MainThread is null);
    }

    /// <summary>Routes a stream onto the thread that owns a control, with the registered invokers.</summary>
    public static void ObserveOnViewThread()
    {
        UiDispatcher dispatcher = new();
        DispatcherLabelControl titleLabel = new(dispatcher);
        TodoItem item = new() { Title = OriginalTitle };
        AppLocator.CurrentMutable.RegisterConstant<IViewThreadInvoker>(new UiDispatcherViewThreadInvoker(dispatcher));
        ViewThreadInvokers.Refresh();
        List<string> delivered = [];

        try
        {
            var routed = BindingSchedulers.ObserveOnViewThread(item.WhenChanged(x => x.Title), titleLabel);

            using (routed.Subscribe(delivered.Add))
            {
                RunOnWorkerThread(() => item.Title = SyncedTitle);

                SampleCheck.SequenceEqual([OriginalTitle], delivered);

                _ = dispatcher.RunPending();

                SampleCheck.SequenceEqual([OriginalTitle, SyncedTitle], delivered);
            }
        }
        finally
        {
            AppLocator.CurrentMutable.UnregisterAll<IViewThreadInvoker>();
            ViewThreadInvokers.Refresh();
        }
    }

    /// <summary>Routes a stream with a fallback invoker, for a control that no registered invoker claims.</summary>
    public static void ObserveOnViewThreadWithFallback()
    {
        UiDispatcher dispatcher = new();
        LabelControl titleLabel = new();
        TodoItem item = new() { Title = OriginalTitle };
        UiDispatcherViewThreadInvoker fallback = new(dispatcher);
        List<string> delivered = [];

        var routed = BindingSchedulers.ObserveOnViewThread(item.WhenChanged(x => x.Title), titleLabel, fallback);

        using (routed.Subscribe(delivered.Add))
        {
            RunOnWorkerThread(() => item.Title = SyncedTitle);

            SampleCheck.SequenceEqual([OriginalTitle], delivered);

            _ = dispatcher.RunPending();

            SampleCheck.SequenceEqual([OriginalTitle, SyncedTitle], delivered);
        }
    }

    /// <summary>Delivers a one-way binding on a sequencer of your choice: the write, including the first one, waits for the sequencer.</summary>
    public static void BindOneWayOnSequencer()
    {
        LabelControl titleLabel = new();
        TodoItem item = new() { Title = OriginalTitle };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());

        using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text, sequencer))
        {
            SampleCheck.Equal(string.Empty, titleLabel.Text);

            _ = sequencer.RunPending();

            SampleCheck.Equal(OriginalTitle, titleLabel.Text);

            item.Title = RenamedTitle;

            SampleCheck.Equal(OriginalTitle, titleLabel.Text);

            _ = sequencer.RunPending();

            SampleCheck.Equal(RenamedTitle, titleLabel.Text);
        }
    }

    /// <summary>Converts with a function and delivers on a sequencer.</summary>
    public static void BindOneWayWithConversionOnSequencer()
    {
        LabelControl remainingLabel = new();
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());

        using (viewModel.BindOneWay(remainingLabel, x => x.RemainingCount, x => x.Text, static count => count.ToString(CultureInfo.InvariantCulture), sequencer))
        {
            viewModel.LoadCommand.Execute(null);
            _ = sequencer.RunPending();

            SampleCheck.Equal(SeededRemainingCount, remainingLabel.Text);
        }
    }

    /// <summary>Binds two ways on a sequencer when the two sides start with different values: the view shows the view model once the sequencer runs.</summary>
    public static void BindTwoWayOnSequencer()
    {
        TextBoxControl referenceBox = new();
        TransferDraft draft = new() { Reference = RentReference };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());

        using (draft.BindTwoWay(referenceBox, x => x.Reference, x => x.Text, sequencer))
        {
            SampleCheck.Equal(string.Empty, referenceBox.Text);

            _ = sequencer.RunPending();

            SampleCheck.Equal(RentReference, referenceBox.Text);
            SampleCheck.Equal(RentReference, draft.Reference);

            referenceBox.Text = RentAprilReference;
            _ = sequencer.RunPending();

            SampleCheck.Equal(RentAprilReference, draft.Reference);
        }
    }

    /// <summary>Edits the view model twice before the sequencer runs: the view gets the second value and the binding goes idle.</summary>
    public static void BindTwoWayBurstOnSequencer()
    {
        TextBoxControl referenceBox = new();
        TransferDraft draft = new() { Reference = RentReference };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());

        using (draft.BindTwoWay(referenceBox, x => x.Reference, x => x.Text, sequencer))
        {
            _ = sequencer.RunPending();

            draft.Reference = RentAprilReference;
            draft.Reference = RentMayReference;
            _ = sequencer.RunPending();

            SampleCheck.Equal(RentMayReference, referenceBox.Text);
            SampleCheck.Equal(RentMayReference, draft.Reference);
            SampleCheck.Equal(0, sequencer.PendingCount);
            SampleCheck.Equal(0, sequencer.RunPending());
        }
    }

    /// <summary>Binds two ways with a conversion function for each direction, on a sequencer.</summary>
    public static void BindTwoWayWithConversionOnSequencer()
    {
        TextBoxControl amountBox = new();
        TransferDraft draft = new() { Amount = RentAmount };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());

        using (draft.BindTwoWay(
            amountBox,
            x => x.Amount,
            x => x.Text,
            static amount => amount.ToString(TwoDecimalPlaces, CultureInfo.InvariantCulture),
            ParseAmount,
            sequencer))
        {
            _ = sequencer.RunPending();

            SampleCheck.Equal(RentAmountText, amountBox.Text);

            amountBox.Text = RaisedRentAmountText;
            _ = sequencer.RunPending();

            SampleCheck.Equal(RaisedRentAmount, draft.Amount);
        }
    }

    /// <summary>Binds a view to its view model two ways, with a conversion function for each direction, on a sequencer.</summary>
    public static void BindViewOnSequencer()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend(ManualClock.StartOfWorkingDay()));
        TransferView view = new() { ViewModel = viewModel };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());
        viewModel.Draft.Amount = RentAmount;

        using (view.Bind(
            viewModel,
            x => x.Draft.Amount,
            v => v.AmountTextBox.Text,
            static amount => amount.ToString(TwoDecimalPlaces, CultureInfo.InvariantCulture),
            ParseAmount,
            sequencer))
        {
            _ = sequencer.RunPending();

            SampleCheck.Equal(RentAmountText, view.AmountTextBox.Text);

            view.AmountTextBox.Text = RaisedRentAmountText;
            _ = sequencer.RunPending();

            SampleCheck.Equal(RaisedRentAmount, viewModel.Draft.Amount);
        }
    }

    /// <summary>Binds a view to its view model one way, with a selector, on a sequencer.</summary>
    public static void OneWayBindViewOnSequencer()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        TodoView view = new() { ViewModel = viewModel };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());

        using (view.OneWayBind(viewModel, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture), sequencer))
        {
            viewModel.LoadCommand.Execute(null);
            _ = sequencer.RunPending();

            SampleCheck.Equal(SeededRemainingCount, view.RemainingLabel.Text);
        }
    }

    /// <summary>Resolves a one-way binding on a sequencer at run time, for an expression the generator cannot read.</summary>
    public static void BindOneWayUnsafeOnSequencer()
    {
        LabelControl titleLabel = new();
        TodoItem item = new() { Title = OriginalTitle };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());

        using (item.BindOneWayUnsafe(titleLabel, x => x.Title, x => x.Text, sequencer))
        {
            _ = sequencer.RunPending();

            SampleCheck.Equal(OriginalTitle, titleLabel.Text);
        }
    }

    /// <summary>Resolves a one-way binding with a conversion function on a sequencer at run time.</summary>
    public static void BindOneWayUnsafeWithConversionOnSequencer()
    {
        LabelControl remainingLabel = new();
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());

        using (viewModel.BindOneWayUnsafe(remainingLabel, x => x.RemainingCount, x => x.Text, static count => count.ToString(CultureInfo.InvariantCulture), sequencer))
        {
            viewModel.LoadCommand.Execute(null);
            _ = sequencer.RunPending();

            SampleCheck.Equal(SeededRemainingCount, remainingLabel.Text);
        }
    }

    /// <summary>Resolves a one-way binding with a converter and a format hint on a sequencer at run time.</summary>
    public static void BindOneWayUnsafeWithConverterOnSequencer()
    {
        TextBoxControl amountBox = new();
        TransferDraft draft = new() { Amount = RentAmount };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());
        DecimalToStringTypeConverter converter = new();

        using (draft.BindOneWayUnsafe(amountBox, x => x.Amount, x => x.Text, converter, sequencer, TwoDecimalPlaces))
        {
            _ = sequencer.RunPending();

            SampleCheck.Equal(RentAmountText, amountBox.Text);
        }
    }

    /// <summary>Resolves a two-way binding on a sequencer at run time.</summary>
    public static void BindTwoWayUnsafeOnSequencer()
    {
        TextBoxControl referenceBox = new() { Text = RentReference };
        TransferDraft draft = new() { Reference = RentReference };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());

        using (draft.BindTwoWayUnsafe(referenceBox, x => x.Reference, x => x.Text, sequencer))
        {
            _ = sequencer.RunPending();

            referenceBox.Text = RentAprilReference;
            _ = sequencer.RunPending();

            SampleCheck.Equal(RentAprilReference, draft.Reference);
        }
    }

    /// <summary>Resolves a two-way binding with a conversion function for each direction on a sequencer at run time.</summary>
    public static void BindTwoWayUnsafeWithConversionOnSequencer()
    {
        TextBoxControl amountBox = new() { Text = RentAmountText };
        TransferDraft draft = new() { Amount = RentAmount };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());

        using (draft.BindTwoWayUnsafe(
            amountBox,
            x => x.Amount,
            x => x.Text,
            static amount => amount.ToString(TwoDecimalPlaces, CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture),
            sequencer))
        {
            _ = sequencer.RunPending();

            amountBox.Text = RaisedRentAmountText;
            _ = sequencer.RunPending();

            SampleCheck.Equal(RaisedRentAmount, draft.Amount);
        }
    }

    /// <summary>Resolves a two-way binding with a converter for each direction and a format hint on a sequencer at run time.</summary>
    public static void BindTwoWayUnsafeWithConvertersOnSequencer()
    {
        TextBoxControl amountBox = new() { Text = RentAmountText };
        TransferDraft draft = new() { Amount = RentAmount };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toAmount = new();

        using (draft.BindTwoWayUnsafe(amountBox, x => x.Amount, x => x.Text, toText, toAmount, sequencer, TwoDecimalPlaces))
        {
            _ = sequencer.RunPending();

            amountBox.Text = RaisedRentAmountText;
            _ = sequencer.RunPending();

            SampleCheck.Equal(RaisedRentAmount, draft.Amount);
        }
    }

    /// <summary>Resolves a two-way view binding with a conversion function for each direction on a sequencer at run time.</summary>
    public static void BindViewUnsafeOnSequencer()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend(ManualClock.StartOfWorkingDay()));
        TransferView view = new() { ViewModel = viewModel };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());
        viewModel.Draft.Amount = RentAmount;
        view.AmountTextBox.Text = RentAmountText;

        using (view.BindUnsafe(
            viewModel,
            x => x.Draft.Amount,
            v => v.AmountTextBox.Text,
            static amount => amount.ToString(TwoDecimalPlaces, CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture),
            sequencer))
        {
            _ = sequencer.RunPending();

            SampleCheck.Equal(RentAmountText, view.AmountTextBox.Text);
        }
    }

    /// <summary>Resolves a two-way view binding with a converter for each direction and a format hint on a sequencer at run time.</summary>
    public static void BindViewUnsafeWithConvertersOnSequencer()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend(ManualClock.StartOfWorkingDay()));
        TransferView view = new() { ViewModel = viewModel };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toAmount = new();
        viewModel.Draft.Amount = RentAmount;
        view.AmountTextBox.Text = RentAmountText;

        using (view.BindUnsafe(viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, toText, toAmount, sequencer, TwoDecimalPlaces))
        {
            _ = sequencer.RunPending();

            SampleCheck.Equal(RentAmountText, view.AmountTextBox.Text);
        }
    }

    /// <summary>Resolves a one-way view binding with a selector on a sequencer at run time.</summary>
    public static void OneWayBindViewUnsafeOnSequencer()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        TodoView view = new() { ViewModel = viewModel };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());

        using (view.OneWayBindUnsafe(viewModel, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture), sequencer))
        {
            viewModel.LoadCommand.Execute(null);
            _ = sequencer.RunPending();

            SampleCheck.Equal(SeededRemainingCount, view.RemainingLabel.Text);
        }
    }

    /// <summary>Resolves a one-way view binding with a converter and a format hint on a sequencer at run time.</summary>
    public static void OneWayBindViewUnsafeWithConverterOnSequencer()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend(ManualClock.StartOfWorkingDay()));
        TransferView view = new() { ViewModel = viewModel };
        ManualSequencer sequencer = new(ManualClock.StartOfWorkingDay());
        DecimalToStringTypeConverter converter = new();
        viewModel.Draft.Amount = RentAmount;

        using (view.OneWayBindUnsafe(viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, converter, sequencer, TwoDecimalPlaces))
        {
            _ = sequencer.RunPending();

            SampleCheck.Equal(RentAmountText, view.AmountTextBox.Text);
        }
    }

    /// <summary>Reads an amount of money the customer typed.</summary>
    /// <param name="text">The text in the box.</param>
    /// <returns>The amount, or zero when the text is not a number.</returns>
    private static decimal ParseAmount(string text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ? amount : 0M;
}

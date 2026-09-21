// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Education;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.BindingsReactiveBinding;

/// <summary>Shows the binding that <c>OneWayBind</c> and <c>Bind</c> return, and the update streams that decide when a two-way binding writes.</summary>
public static class BindingsReactiveBindingExamples
{
    /// <summary>The number of 4 MiB parts in the file the upload example sends.</summary>
    private const int UploadPartCount = 4;

    /// <summary>The size of the file the upload example sends: four 4 MiB parts.</summary>
    private const long UploadSizeBytes = 16_777_216;

    /// <summary>The percentage shown before any part of the upload has arrived.</summary>
    private const double EmptyPercent = 0D;

    /// <summary>The percentage shown when a quarter of the upload has arrived.</summary>
    private const double QuarterPercent = 25D;

    /// <summary>The percentage shown when half of the upload has arrived.</summary>
    private const double HalfPercent = 50D;

    /// <summary>The percentage shown when three quarters of the upload have arrived.</summary>
    private const double ThreeQuartersPercent = 75D;

    /// <summary>The percentage shown when the upload is complete.</summary>
    private const double FullPercent = 100D;

    /// <summary>The text of the filter the customer types.</summary>
    private const string CarFilter = "car";

    /// <summary>The text of a filter the view model sets.</summary>
    private const string DentistFilter = "dentist";

    /// <summary>The reference the draft starts with.</summary>
    private const string MarchReference = "Rent March";

    /// <summary>The reference the customer types.</summary>
    private const string AprilReference = "Rent April";

    /// <summary>The reference the view model sets.</summary>
    private const string MayReference = "Rent May";

    /// <summary>The amount the draft starts with.</summary>
    private const decimal RentAmount = 1200M;

    /// <summary>The amount the customer types, as the box shows it.</summary>
    private const string TypedAmountText = "85.50";

    /// <summary>The amount the customer types.</summary>
    private const decimal TypedAmount = 85.50M;

    /// <summary>The score the view model starts with.</summary>
    private const decimal FirstScore = 64M;

    /// <summary>The first score, as the view shows it.</summary>
    private const string FirstScoreText = "64.0";

    /// <summary>The score after a re-mark.</summary>
    private const decimal RemarkedScore = 91M;

    /// <summary>The re-marked score, as the view shows it.</summary>
    private const string RemarkedScoreText = "91.0";

    /// <summary>The score the teacher types.</summary>
    private const string TypedScoreText = "82.5";

    /// <summary>The score the teacher types, as a number.</summary>
    private const decimal TypedScore = 82.5M;

    /// <summary>The format the amount converters use to show two decimal places.</summary>
    private const string TwoDecimalPlaces = "F2";

    /// <summary>The format the score converters use to show one decimal place.</summary>
    private const string OneDecimalPlace = "F1";

    /// <summary>The number of times a disposed binding disposes what it wraps.</summary>
    private const int SingleDisposal = 1;

    /// <summary>Registers the core services the <c>Unsafe</c> bindings look up while the app runs.</summary>
    public static void BuildApplication()
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices().BuildApp();
    }

    /// <summary>Reads what a one-way binding reports: its view, its direction and every value it writes, starting with the current one.</summary>
    public static void ReadOneWayBinding()
    {
        var storage = InMemoryObjectStorage.CreateSeeded(ManualClock.StartOfWorkingDay());
        var browser = OpenBucket(storage);
        StorageBrowserView view = new() { ViewModel = browser };
        List<double> written = [];

        using (var binding = view.OneWayBind(browser, x => x.UploadPercent, v => v.UploadProgressBar.Value))
        {
            SampleCheck.Equal(BindingDirection.OneWay, binding.Direction);
            SampleCheck.Equal(true, ReferenceEquals(view, binding.View));

            using (binding.Changed.Subscribe(written.Add))
            {
                storage.Gate.Hold();
                browser.UploadCommand.Execute(new UploadRequest("launch-video.mp4", UploadSizeBytes, "video/mp4"));

                // The first release lets the request start; each release after that delivers one 4 MiB part.
                storage.Gate.ReleaseNext();
                for (var part = 1; part <= UploadPartCount; part++)
                {
                    storage.Gate.ReleaseNext();
                }

                storage.Gate.ReleaseAll();
            }
        }

        // Changed replays the bar's starting value, so the empty bar is the first value a subscriber sees.
        SampleCheck.SequenceEqual([EmptyPercent, QuarterPercent, HalfPercent, ThreeQuartersPercent, FullPercent], written);
    }

    /// <summary>Reads what a two-way binding reports: each <see cref="BindingChange"/> says which side produced the value.</summary>
    public static void ReadTwoWayBinding()
    {
        var list = OpenTodoList();
        TodoView view = new() { ViewModel = list };
        List<BindingChange> changes = [];

        using (var binding = view.Bind(list, x => x.FilterText, v => v.FilterTextBox.Text))
        using (binding.Changed.Subscribe(changes.Add))
        {
            SampleCheck.Equal(BindingDirection.TwoWay, binding.Direction);
            SampleCheck.Equal(true, ReferenceEquals(view, binding.View));

            list.FilterText = CarFilter;
            view.FilterTextBox.Text = DentistFilter;
        }

        SampleCheck.SequenceEqual([new(CarFilter, true), new(DentistFilter, false)], changes);

        // A change is a value with two parts, so it compares, deconstructs and copies like any record struct.
        var (value, fromViewModel) = changes[0];
        SampleCheck.Equal(CarFilter, (string?)value);
        SampleCheck.Equal(true, fromViewModel);
        SampleCheck.Equal(new(CarFilter, false), changes[0] with { FromViewModel = false });
    }

    /// <summary>Wraps a subscription in a <see cref="ReactiveBinding{TView, TValue}"/>, which disposes it once however often the binding is disposed.</summary>
    public static void CreateReactiveBinding()
    {
        var list = OpenTodoList();
        TodoView view = new() { ViewModel = list };
        Signal<int> remainingChanges = new();
        CountingDisposable subscription = new();

        ReactiveBinding<TodoView, int> binding = new(view, remainingChanges, BindingDirection.AsyncOneWay, subscription);
        List<int> seen = [];
        using var reader = binding.Changed.Subscribe(seen.Add);

        remainingChanges.OnNext(list.RemainingCount);

        SampleCheck.Equal(BindingDirection.AsyncOneWay, binding.Direction);
        SampleCheck.Equal(true, ReferenceEquals(view, binding.View));
        SampleCheck.Equal(true, binding.ViewExpression is null);
        SampleCheck.Equal(true, binding.ViewModelExpression is null);
        SampleCheck.SequenceEqual([list.RemainingCount], seen);

        binding.Dispose();
        binding.Dispose();

        SampleCheck.Equal(SingleDisposal, subscription.DisposeCount);
    }

    /// <summary>
    /// Drives the view-to-view-model direction from a stream: the customer's typing stays in the box until the box is
    /// left, and view model changes still reach the box at once. <see cref="TriggerUpdate.ViewToViewModel"/> is the default.
    /// </summary>
    public static void CommitReferenceWhenBoxIsLeft()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend(ManualClock.StartOfWorkingDay()));
        TransferView view = new() { ViewModel = viewModel };
        Signal<RxVoid> boxLeft = new();
        viewModel.Draft.Reference = MarchReference;

        using (view.BindUnsafe(viewModel, x => x.Draft.Reference, v => v.ReferenceTextBox.Text, boxLeft))
        {
            SampleCheck.Equal(MarchReference, view.ReferenceTextBox.Text);

            view.ReferenceTextBox.Text = AprilReference;
            SampleCheck.Equal(MarchReference, viewModel.Draft.Reference);

            boxLeft.OnNext(RxVoid.Default);
            SampleCheck.Equal(AprilReference, viewModel.Draft.Reference);

            viewModel.Draft.Reference = MayReference;
            SampleCheck.Equal(MayReference, view.ReferenceTextBox.Text);
        }
    }

    /// <summary>Converts the amount in each direction; the typed amount is read when the box is left, as <see cref="TriggerUpdate.ViewToViewModel"/> does by default.</summary>
    public static void CommitAmountWhenBoxIsLeft()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend(ManualClock.StartOfWorkingDay()));
        TransferView view = new() { ViewModel = viewModel };
        Signal<RxVoid> boxLeft = new();
        viewModel.Draft.Amount = RentAmount;

        using (view.BindUnsafe(viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, FormatAmount, ParseAmount, boxLeft))
        {
            SampleCheck.Equal("1200.00", view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            SampleCheck.Equal(RentAmount, viewModel.Draft.Amount);

            boxLeft.OnNext(RxVoid.Default);
            SampleCheck.Equal(TypedAmount, viewModel.Draft.Amount);
        }
    }

    /// <summary>
    /// Drives the view-model-to-view direction from a stream: the view model's score reaches the box when the stream
    /// signals a refresh, while the teacher's typing reaches the view model at once.
    /// </summary>
    public static void RefreshScoreOnSignal()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded(ManualClock.StartOfWorkingDay())) { ScoreToRecord = FirstScore };
        GradebookView view = new() { ViewModel = viewModel };
        Signal<RxVoid> refresh = new();

        using (view.BindUnsafe(viewModel, x => x.ScoreToRecord, v => v.ScoreTextBox.Text, FormatScore, ParseScore, refresh, TriggerUpdate.ViewModelToView))
        {
            SampleCheck.Equal(FirstScoreText, view.ScoreTextBox.Text);

            viewModel.ScoreToRecord = RemarkedScore;
            SampleCheck.Equal(FirstScoreText, view.ScoreTextBox.Text);

            refresh.OnNext(RxVoid.Default);
            SampleCheck.Equal(RemarkedScoreText, view.ScoreTextBox.Text);

            view.ScoreTextBox.Text = TypedScoreText;
            SampleCheck.Equal(TypedScore, viewModel.ScoreToRecord);
        }
    }

    /// <summary>Uses <see cref="TriggerUpdate.ViewModelToView"/> without converters; the registered converter for the pair runs.</summary>
    public static void RefreshFilterOnSignal()
    {
        var list = OpenTodoList();
        TodoView view = new() { ViewModel = list };
        Signal<RxVoid> refresh = new();

        using (view.BindUnsafe(list, x => x.FilterText, v => v.FilterTextBox.Text, refresh, TriggerUpdate.ViewModelToView))
        {
            list.FilterText = CarFilter;
            SampleCheck.Equal(string.Empty, view.FilterTextBox.Text);

            refresh.OnNext(RxVoid.Default);
            SampleCheck.Equal(CarFilter, view.FilterTextBox.Text);

            view.FilterTextBox.Text = DentistFilter;
            SampleCheck.Equal(DentistFilter, list.FilterText);
        }
    }

    /// <summary>Passes no stream: both properties are observed, so the binding writes in both directions as they change.</summary>
    public static void ObserveBothWithoutStream()
    {
        var list = OpenTodoList();
        TodoView view = new() { ViewModel = list };

        using (view.BindUnsafe(list, x => x.FilterText, v => v.FilterTextBox.Text, (IObservable<RxVoid>?)null))
        {
            list.FilterText = CarFilter;
            SampleCheck.Equal(CarFilter, view.FilterTextBox.Text);

            view.FilterTextBox.Text = DentistFilter;
            SampleCheck.Equal(DentistFilter, list.FilterText);
        }
    }

    /// <summary>Passes no stream to the converting overload, so both properties are observed.</summary>
    public static void ObserveBothWithConvertersAndWithoutStream()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend(ManualClock.StartOfWorkingDay()));
        TransferView view = new() { ViewModel = viewModel };
        viewModel.Draft.Amount = RentAmount;

        using (view.BindUnsafe(viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, FormatAmount, ParseAmount, (IObservable<RxVoid>?)null))
        {
            SampleCheck.Equal("1200.00", view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            SampleCheck.Equal(TypedAmount, viewModel.Draft.Amount);
        }
    }

    /// <summary>Shows the amount of money with two decimal places.</summary>
    /// <param name="amount">The amount of money.</param>
    /// <returns>The amount as text.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string FormatAmount(decimal amount) => amount.ToString(TwoDecimalPlaces, CultureInfo.InvariantCulture);

    /// <summary>Reads an amount of money the customer typed.</summary>
    /// <param name="text">The text in the box.</param>
    /// <returns>The amount, or zero when the text is not a number.</returns>
    private static decimal ParseAmount(string text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ? amount : 0M;

    /// <summary>Shows a score with one decimal place.</summary>
    /// <param name="score">The score.</param>
    /// <returns>The score as text.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string FormatScore(decimal score) => score.ToString(OneDecimalPlace, CultureInfo.InvariantCulture);

    /// <summary>Reads a score the teacher typed.</summary>
    /// <param name="text">The text in the box.</param>
    /// <returns>The score, or zero when the text is not a number.</returns>
    private static decimal ParseScore(string text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var score) ? score : 0M;

    /// <summary>Loads the seeded to-do list.</summary>
    /// <returns>The list with three unfinished items and one finished item.</returns>
    private static TodoListViewModel OpenTodoList()
    {
        TodoListViewModel list = new(InMemoryTodoStore.CreateSeeded());
        list.LoadCommand.Execute(null);
        return list;
    }

    /// <summary>Lists the buckets of the storage service and picks the first.</summary>
    /// <param name="storage">The storage service.</param>
    /// <returns>The browser with a bucket selected.</returns>
    private static StorageBrowserViewModel OpenBucket(InMemoryObjectStorage storage)
    {
        StorageBrowserViewModel browser = new(storage);
        browser.LoadBucketsCommand.Execute(null);
        browser.SelectedBucket = browser.Buckets[0];
        return browser;
    }
}

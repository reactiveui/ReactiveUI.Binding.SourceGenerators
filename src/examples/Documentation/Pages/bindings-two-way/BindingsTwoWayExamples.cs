// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Education;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.BindingsTwoWay;

/// <summary>Shows how <c>BindTwoWay</c> and <c>Bind</c> keep a view model property and a view property equal in both directions.</summary>
public static class BindingsTwoWayExamples
{
    /// <summary>The text the user types to narrow the to-do list to one item.</summary>
    private const string DentistFilter = "dentist";

    /// <summary>The text a view model sets to narrow the to-do list to one item.</summary>
    private const string CarFilter = "car";

    /// <summary>The access token of the signed-in user, as the user types it.</summary>
    private const string TypedToken = "token-priya";

    /// <summary>The access token a view model sets after a token refresh.</summary>
    private const string RefreshedToken = "token-refreshed";

    /// <summary>The amount of the transfer in the view model.</summary>
    private const decimal RentAmount = 1200M;

    /// <summary>The amount of the transfer as the view shows it.</summary>
    private const string RentAmountText = "1200.00";

    /// <summary>The amount the customer types.</summary>
    private const string TypedAmountText = "85.50";

    /// <summary>The amount the customer types, as a number.</summary>
    private const decimal TypedAmount = 85.50M;

    /// <summary>The text of an amount that is not a number.</summary>
    private const string NotAnAmountText = "twelve";

    /// <summary>The score the teacher typed.</summary>
    private const string TypedScoreText = "82.5";

    /// <summary>The score the teacher typed, as a number.</summary>
    private const decimal TypedScore = 82.5M;

    /// <summary>The score the view model starts with.</summary>
    private const decimal FirstScore = 64M;

    /// <summary>The first score as the view shows it.</summary>
    private const string FirstScoreText = "64.0";

    /// <summary>The score a teacher records after a re-mark.</summary>
    private const decimal RemarkedScore = 91M;

    /// <summary>The text of a score as the view shows it.</summary>
    private const string RemarkedScoreText = "91.0";

    /// <summary>The reference of the transfer the customer is filling in.</summary>
    private const string RentMarchReference = "Rent March";

    /// <summary>The reference the customer types over the first one.</summary>
    private const string RentAprilReference = "Rent April";

    /// <summary>The reference that replaces the April one before the sequencer runs.</summary>
    private const string RentMayReference = "Rent May";

    /// <summary>The amount of the transfer as the decimal converter shows it without a hint.</summary>
    private const string RentAmountPlainText = "1200";

    /// <summary>The first score as the decimal converter shows it without a hint.</summary>
    private const string FirstScorePlainText = "64";

    /// <summary>The format the amount converters use to show two decimal places.</summary>
    private const string TwoDecimalPlaces = "F2";

    /// <summary>The format the score converters use to show one decimal place.</summary>
    private const string OneDecimalPlace = "F1";

    /// <summary>Registers the core services the <c>Unsafe</c> bindings look up while the app runs.</summary>
    public static void BuildApplication()
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices().BuildApp();
    }

    /// <summary>Keeps the filter of the to-do list and the filter box equal with <c>BindTwoWay</c>.</summary>
    public static void BindTodoFilterToTextBox()
    {
        var list = OpenTodoList();
        TodoView view = new();

        using (list.BindTwoWay(view, x => x.FilterText, v => v.FilterTextBox.Text))
        {
            list.FilterText = CarFilter;
            SampleCheck.Equal(CarFilter, view.FilterTextBox.Text);

            view.FilterTextBox.Text = DentistFilter;
            SampleCheck.Equal(DentistFilter, list.FilterText);
            SampleCheck.Equal(1, list.Items.Count);
        }

        // Disposing the binding disconnects both directions.
        view.FilterTextBox.Text = string.Empty;
        SampleCheck.Equal(DentistFilter, list.FilterText);

        list.FilterText = CarFilter;
        SampleCheck.Equal(string.Empty, view.FilterTextBox.Text);
    }

    /// <summary>Keeps the amount of a transfer and the amount box equal, converting with a function in each direction.</summary>
    public static void BindTransferAmountWithConverters()
    {
        TransferDraft draft = new() { Amount = RentAmount };
        TransferView view = new();

        using (draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, static amount => amount.ToString(TwoDecimalPlaces, CultureInfo.InvariantCulture), ParseAmount))
        {
            SampleCheck.Equal(RentAmountText, view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            SampleCheck.Equal(TypedAmount, draft.Amount);

            // The reverse converter decides what text that is not a number means.
            view.AmountTextBox.Text = NotAnAmountText;
            SampleCheck.Equal(0M, draft.Amount);
        }
    }

    /// <summary>Delivers the writes of a two-way binding on a named sequencer, which waits until the host runs it. The box starts empty and the view model holds a token.</summary>
    public static void BindTokenOnSequencer()
    {
        var board = OpenSignedOutBoard();
        IssueBoardView view = new();
        QueuedSequencer uiThread = new();

        using (board.BindTwoWay(view, x => x.Token, v => v.TokenTextBox.Text, uiThread))
        {
            SampleCheck.Equal(string.Empty, view.TokenTextBox.Text);

            _ = uiThread.RunPending();
            SampleCheck.Equal(InMemoryGitHubServer.PriyaToken, view.TokenTextBox.Text);
            SampleCheck.Equal(InMemoryGitHubServer.PriyaToken, board.Token);

            board.Token = RefreshedToken;
            SampleCheck.Equal(InMemoryGitHubServer.PriyaToken, view.TokenTextBox.Text);

            _ = uiThread.RunPending();
            SampleCheck.Equal(RefreshedToken, view.TokenTextBox.Text);

            view.TokenTextBox.Text = TypedToken;
            _ = uiThread.RunPending();
            SampleCheck.Equal(TypedToken, board.Token);
        }
    }

    /// <summary>Edits the view model twice before the sequencer runs: the view shows the second value and the binding goes idle.</summary>
    public static void BindReferenceBurstOnSequencer()
    {
        TransferDraft draft = new() { Reference = RentMarchReference };
        TransferView view = new();
        QueuedSequencer uiThread = new();

        using (draft.BindTwoWay(view, x => x.Reference, v => v.ReferenceTextBox.Text, uiThread))
        {
            // The view is empty and the view model holds "Rent March": the view shows the view model once the sequencer runs.
            SampleCheck.Equal(string.Empty, view.ReferenceTextBox.Text);

            _ = uiThread.RunPending();
            SampleCheck.Equal(RentMarchReference, view.ReferenceTextBox.Text);

            draft.Reference = RentAprilReference;
            draft.Reference = RentMayReference;
            _ = uiThread.RunPending();

            SampleCheck.Equal(RentMayReference, view.ReferenceTextBox.Text);
            SampleCheck.Equal(RentMayReference, draft.Reference);
            SampleCheck.Equal(0, uiThread.PendingCount);
            SampleCheck.Equal(0, uiThread.RunPending());
        }
    }

    /// <summary>Converts the amount in each direction and delivers the writes on a named sequencer. The box starts empty.</summary>
    public static void BindTransferAmountWithConvertersOnSequencer()
    {
        TransferDraft draft = new() { Amount = RentAmount };
        TransferView view = new();
        QueuedSequencer uiThread = new();

        using (draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, FormatAmount, ParseAmount, uiThread))
        {
            _ = uiThread.RunPending();
            SampleCheck.Equal(RentAmountText, view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            _ = uiThread.RunPending();
            SampleCheck.Equal(TypedAmount, draft.Amount);
        }
    }

    /// <summary>Converts with a converter object in each direction; without a hint the decimal converter uses the current culture's default text.</summary>
    public static void BindTransferAmountWithConverterObjects()
    {
        TransferDraft draft = new() { Amount = RentAmount };
        TransferView view = new();
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toAmount = new();

        using (draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, toText, toAmount))
        {
            SampleCheck.Equal(RentAmountPlainText, view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            SampleCheck.Equal(TypedAmount, draft.Amount);
        }
    }

    /// <summary>Passes a hint to both converters: the text is the number format the amount is shown in.</summary>
    public static void BindTransferAmountWithConverterObjectsAndHint()
    {
        TransferDraft draft = new() { Amount = RentAmount };
        TransferView view = new();
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toAmount = new();

        using (draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, toText, toAmount, TwoDecimalPlaces))
        {
            SampleCheck.Equal(RentAmountText, view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            SampleCheck.Equal(TypedAmount, draft.Amount);
        }
    }

    /// <summary>Passes converter objects, a hint and a named sequencer.</summary>
    public static void BindTransferAmountWithConverterObjectsOnSequencer()
    {
        TransferDraft draft = new() { Amount = RentAmount };
        TransferView view = new();
        QueuedSequencer uiThread = new();
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toAmount = new();

        using (draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, toText, toAmount, TwoDecimalPlaces, uiThread))
        {
            SampleCheck.Equal(string.Empty, view.AmountTextBox.Text);

            _ = uiThread.RunPending();
            SampleCheck.Equal(RentAmountText, view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            _ = uiThread.RunPending();
            SampleCheck.Equal(TypedAmount, draft.Amount);
        }
    }

    /// <summary>
    /// Resolves the paths of the same converter-object binding while the app runs, with <c>BindTwoWayUnsafe</c>. Use it
    /// when a path is not an inline lambda.
    /// </summary>
    public static void BindTransferAmountUnsafeWithConverterObjects()
    {
        TransferDraft draft = new() { Amount = RentAmount };
        TransferView view = new() { AmountTextBox = { Text = RentAmountText } };
        QueuedSequencer uiThread = new();
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toAmount = new();

        using (draft.BindTwoWayUnsafe(view, x => x.Amount, v => v.AmountTextBox.Text, toText, toAmount, uiThread, TwoDecimalPlaces))
        {
            _ = uiThread.RunPending();
            SampleCheck.Equal(RentAmountText, view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            _ = uiThread.RunPending();
            SampleCheck.Equal(TypedAmount, draft.Amount);
        }
    }

    /// <summary>Binds the view's filter box with the view-first <c>Bind</c>, and reads each change from the returned binding.</summary>
    public static void BindTodoFilterInView()
    {
        var list = OpenTodoList();
        TodoView view = new() { ViewModel = list };
        List<BindingChange> changes = [];

        using (var binding = view.Bind(list, x => x.FilterText, v => v.FilterTextBox.Text))
        using (binding.Changed.Subscribe(changes.Add))
        {
            SampleCheck.Equal(BindingDirection.TwoWay, binding.Direction);

            list.FilterText = CarFilter;
            SampleCheck.Equal(CarFilter, view.FilterTextBox.Text);

            view.FilterTextBox.Text = DentistFilter;
            SampleCheck.Equal(DentistFilter, list.FilterText);
        }

        // FromViewModel tells an edit apart from the echo of the binding's own write.
        SampleCheck.SequenceEqual([new(CarFilter, true), new(DentistFilter, false)], changes);
    }

    /// <summary>Keeps the amount of the transfer draft and the amount box equal through the view-first <c>Bind</c>, with a function in each direction.</summary>
    public static void BindTransferAmountInView()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend(ManualClock.StartOfWorkingDay()));
        TransferView view = new() { ViewModel = viewModel };
        viewModel.Draft.Amount = RentAmount;

        using (view.Bind(viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, FormatAmount, ParseAmount))
        {
            SampleCheck.Equal(RentAmountText, view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            SampleCheck.Equal(TypedAmount, viewModel.Draft.Amount);
        }
    }

    /// <summary>Converts the score in each direction through the view-first <c>Bind</c> and delivers the writes on a named sequencer.</summary>
    public static void BindScoreInViewOnSequencer()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded(ManualClock.StartOfWorkingDay())) { ScoreToRecord = FirstScore };
        GradebookView view = new() { ViewModel = viewModel };
        QueuedSequencer uiThread = new();

        using (view.Bind(viewModel, x => x.ScoreToRecord, v => v.ScoreTextBox.Text, FormatScore, ParseScore, uiThread))
        {
            SampleCheck.Equal(string.Empty, view.ScoreTextBox.Text);

            _ = uiThread.RunPending();
            SampleCheck.Equal(FirstScoreText, view.ScoreTextBox.Text);

            viewModel.ScoreToRecord = RemarkedScore;
            _ = uiThread.RunPending();
            SampleCheck.Equal(RemarkedScoreText, view.ScoreTextBox.Text);

            view.ScoreTextBox.Text = TypedScoreText;
            _ = uiThread.RunPending();
            SampleCheck.Equal(TypedScore, viewModel.ScoreToRecord);
        }
    }

    /// <summary>Converts the score with a converter object in each direction through the view-first <c>Bind</c>.</summary>
    public static void BindScoreWithConverterObjects()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded(ManualClock.StartOfWorkingDay())) { ScoreToRecord = FirstScore };
        GradebookView view = new() { ViewModel = viewModel };
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toScore = new();

        using (view.Bind(viewModel, x => x.ScoreToRecord, v => v.ScoreTextBox.Text, toText, toScore))
        {
            SampleCheck.Equal(FirstScorePlainText, view.ScoreTextBox.Text);

            view.ScoreTextBox.Text = TypedScoreText;
            SampleCheck.Equal(TypedScore, viewModel.ScoreToRecord);
        }
    }

    /// <summary>Passes a hint to both converters of the view-first <c>Bind</c>.</summary>
    public static void BindScoreWithConverterObjectsAndHint()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded(ManualClock.StartOfWorkingDay())) { ScoreToRecord = FirstScore };
        GradebookView view = new() { ViewModel = viewModel };
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toScore = new();

        using (view.Bind(viewModel, x => x.ScoreToRecord, v => v.ScoreTextBox.Text, toText, toScore, OneDecimalPlace))
        {
            SampleCheck.Equal(FirstScoreText, view.ScoreTextBox.Text);

            viewModel.ScoreToRecord = RemarkedScore;
            SampleCheck.Equal(RemarkedScoreText, view.ScoreTextBox.Text);
        }
    }

    /// <summary>Passes converter objects, a hint and a named sequencer to the view-first <c>Bind</c>.</summary>
    public static void BindScoreWithConverterObjectsOnSequencer()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded(ManualClock.StartOfWorkingDay())) { ScoreToRecord = FirstScore };
        GradebookView view = new() { ViewModel = viewModel };
        QueuedSequencer uiThread = new();
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toScore = new();

        using (view.Bind(viewModel, x => x.ScoreToRecord, v => v.ScoreTextBox.Text, toText, toScore, OneDecimalPlace, uiThread))
        {
            SampleCheck.Equal(string.Empty, view.ScoreTextBox.Text);

            _ = uiThread.RunPending();
            SampleCheck.Equal(FirstScoreText, view.ScoreTextBox.Text);

            view.ScoreTextBox.Text = TypedScoreText;
            _ = uiThread.RunPending();
            SampleCheck.Equal(TypedScore, viewModel.ScoreToRecord);
        }
    }

    /// <summary>Passes property paths as <c>static</c> lambdas to the view-first <c>Bind</c>.</summary>
    public static void BindTodoFilterWithStaticLambdas()
    {
        var list = OpenTodoList();
        TodoView view = new() { ViewModel = list };

        using (view.Bind(list, static x => x.FilterText, static v => v.FilterTextBox.Text))
        {
            list.FilterText = CarFilter;
            SampleCheck.Equal(CarFilter, view.FilterTextBox.Text);

            view.FilterTextBox.Text = DentistFilter;
            SampleCheck.Equal(DentistFilter, list.FilterText);
        }
    }

    /// <summary>
    /// Resolves the paths of the same converter-object binding while the app runs, with <c>BindUnsafe</c>. Use it when a
    /// path is not an inline lambda.
    /// </summary>
    public static void BindScoreUnsafeWithConverterObjects()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded(ManualClock.StartOfWorkingDay())) { ScoreToRecord = FirstScore };
        GradebookView view = new() { ViewModel = viewModel, ScoreTextBox = { Text = FirstScoreText } };
        QueuedSequencer uiThread = new();
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toScore = new();

        using (view.BindUnsafe(viewModel, x => x.ScoreToRecord, v => v.ScoreTextBox.Text, toText, toScore, uiThread, OneDecimalPlace))
        {
            _ = uiThread.RunPending();
            SampleCheck.Equal(FirstScoreText, view.ScoreTextBox.Text);

            view.ScoreTextBox.Text = TypedScoreText;
            _ = uiThread.RunPending();
            SampleCheck.Equal(TypedScore, viewModel.ScoreToRecord);
        }
    }

    /// <summary>
    /// Passes a null scheduler to <c>BindTwoWay</c>. Null means the same as naming no scheduler: each write happens on the
    /// thread that owns its object, so an object with no owning thread is written inline in both directions. The argument
    /// is named because a bare <c>null</c> also fits the string parameters of the overload that takes no scheduler.
    /// </summary>
    public static void BindTodoFilterWithNullScheduler()
    {
        var list = OpenTodoList();
        TodoView view = new();

        using (list.BindTwoWay(view, x => x.FilterText, v => v.FilterTextBox.Text, scheduler: null))
        {
            list.FilterText = CarFilter;
            SampleCheck.Equal(CarFilter, view.FilterTextBox.Text);

            view.FilterTextBox.Text = DentistFilter;
            SampleCheck.Equal(DentistFilter, list.FilterText);
        }
    }

    /// <summary>Converts the amount in each direction with functions and passes a null scheduler; both writes land before the next line runs.</summary>
    public static void BindTransferAmountWithConvertersAndNullScheduler()
    {
        TransferDraft draft = new() { Amount = RentAmount };
        TransferView view = new();

        using (draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, FormatAmount, ParseAmount, scheduler: null))
        {
            SampleCheck.Equal(RentAmountText, view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            SampleCheck.Equal(TypedAmount, draft.Amount);

            draft.Amount = RentAmount;
            SampleCheck.Equal(RentAmountText, view.AmountTextBox.Text);
        }
    }

    /// <summary>Converts the score in each direction through the view-first <c>Bind</c> and passes a null scheduler.</summary>
    public static void BindScoreInViewWithNullScheduler()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded(ManualClock.StartOfWorkingDay())) { ScoreToRecord = FirstScore };
        GradebookView view = new() { ViewModel = viewModel };

        using (view.Bind(viewModel, x => x.ScoreToRecord, v => v.ScoreTextBox.Text, FormatScore, ParseScore, scheduler: null))
        {
            SampleCheck.Equal(FirstScoreText, view.ScoreTextBox.Text);

            viewModel.ScoreToRecord = RemarkedScore;
            SampleCheck.Equal(RemarkedScoreText, view.ScoreTextBox.Text);

            view.ScoreTextBox.Text = TypedScoreText;
            SampleCheck.Equal(TypedScore, viewModel.ScoreToRecord);
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

    /// <summary>Creates the issue board with the access token of Priya Nair typed in but not yet used to sign in.</summary>
    /// <returns>The board.</returns>
    private static IssueBoardViewModel OpenSignedOutBoard() =>
        new(InMemoryGitHubServer.CreateSeeded(ManualClock.StartOfWorkingDay())) { Token = InMemoryGitHubServer.PriyaToken };
}

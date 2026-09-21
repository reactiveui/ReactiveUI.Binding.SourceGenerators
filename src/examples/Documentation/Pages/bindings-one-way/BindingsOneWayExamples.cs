// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.BindingsOneWay;

/// <summary>Shows how <c>BindOneWay</c> and <c>OneWayBind</c> copy a view model property onto a view property.</summary>
public static class BindingsOneWayExamples
{
    /// <summary>The number of 4 MiB parts in the file the upload examples send.</summary>
    private const int UploadPartCount = 4;

    /// <summary>The size of the file the upload examples send: four 4 MiB parts.</summary>
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

    /// <summary>The balance the everyday account holds after a deposit.</summary>
    private const decimal DepositedBalance = 1200.50M;

    /// <summary>The text shown for the everyday balance.</summary>
    private const string EverydayBalanceText = "A$2,450.75";

    /// <summary>The title of the seeded issue that has an assignee.</summary>
    private const string SafariIssueTitle = "Checkout button unresponsive on Safari";

    /// <summary>The title of the seeded issue that has no assignee.</summary>
    private const string GiftCardIssueTitle = "Add gift-card support";

    /// <summary>The account name of the person assigned to the Safari issue.</summary>
    private const string SafariAssignee = "priya-nair";

    /// <summary>The account name of the person the Safari issue is reassigned to.</summary>
    private const string ReassignedLogin = "tomas-berg";

    /// <summary>The text shown for an open issue.</summary>
    private const string OpenText = "Open";

    /// <summary>The text shown for a closed issue.</summary>
    private const string ClosedText = "Closed";

    /// <summary>The index of the Safari issue in the loaded list, which is newest first.</summary>
    private const int SafariIssueIndex = 1;

    /// <summary>The total balance of both accounts, shown by the currency converter with the hint <c>N0</c>.</summary>
    private const string GroupedTotalText = "$17,681";

    /// <summary>The title of the to-do item whose priority the badge examples show.</summary>
    private const string ItemTitle = "Renew car registration";

    /// <summary>The hint that asks the priority converter for its dark palette.</summary>
    private const string DarkThemeHint = "dark";

    /// <summary>The text shown for the everyday balance after a deposit.</summary>
    private const string DepositedBalanceText = "A$1,200.50";

    /// <summary>The number of unfinished to-do items, as text.</summary>
    private const string RemainingCountText = "3";

    /// <summary>The number of unfinished to-do items after one is completed, as text.</summary>
    private const string RemainingAfterCompleteText = "2";

    /// <summary>The combined balance of both accounts, shown by the currency converter without a hint.</summary>
    private const string PlainTotalText = "$17,680.75";

    /// <summary>Registers the core services the <c>Unsafe</c> bindings look up while the app runs.</summary>
    public static void BuildApplication()
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices().BuildApp();
    }

    /// <summary>Copies one property to another of the same type with <c>BindOneWay</c>.</summary>
    public static void BindSelectedIssueTitleToLabel()
    {
        var board = OpenIssueBoard();
        IssueBoardView view = new();

        // The path passes through SelectedIssue, which is null until the user picks an issue, so the label is set to null.
        using (board.BindOneWay(view, x => x.SelectedIssue!.Title, v => v.IssueTitleLabel.Text))
        {
            SampleCheck.Equal(null, view.IssueTitleLabel.Text);

            board.SelectedIssue = board.Issues[SafariIssueIndex];
            SampleCheck.Equal(SafariIssueTitle, view.IssueTitleLabel.Text);

            board.SelectedIssue = board.Issues[0];
            SampleCheck.Equal(GiftCardIssueTitle, view.IssueTitleLabel.Text);
        }
    }

    /// <summary>Follows a path through an optional property: an issue without an assignee has no login to show.</summary>
    public static void BindSelectedIssueAssigneeToLabel()
    {
        var board = OpenIssueBoard();
        IssueBoardView view = new();

        using (board.BindOneWay(view, x => x.SelectedIssue!.Assignee!.Login, v => v.AssigneeLabel.Text))
        {
            board.SelectedIssue = board.Issues[SafariIssueIndex];
            SampleCheck.Equal(SafariAssignee, view.AssigneeLabel.Text);

            board.SelectedIssue.Assignee = new User { Login = ReassignedLogin, DisplayName = "Tomas Berg" };
            SampleCheck.Equal(ReassignedLogin, view.AssigneeLabel.Text);

            board.SelectedIssue = board.Issues[0];
            SampleCheck.Equal(null, view.AssigneeLabel.Text);
        }
    }

    /// <summary>Turns an enum into text with a converter function, the second <c>BindOneWay</c> overload.</summary>
    public static void BindSelectedIssueStateToLabel()
    {
        var board = OpenIssueBoard();
        IssueBoardView view = new();

        using (board.BindOneWay(view, x => x.SelectedIssue!.State, v => v.StateLabel.Text, static state => state == IssueState.Open ? OpenText : ClosedText))
        {
            board.SelectedIssue = board.Issues[SafariIssueIndex];
            SampleCheck.Equal(OpenText, view.StateLabel.Text);

            board.SelectedIssue.State = IssueState.Closed;
            SampleCheck.Equal(ClosedText, view.StateLabel.Text);
        }
    }

    /// <summary>Shows how many to-do items are left, and stops updating once the binding is disposed.</summary>
    public static void BindRemainingCountToLabel()
    {
        var list = OpenTodoList();
        TodoView view = new();

        using (list.BindOneWay(view, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture)))
        {
            SampleCheck.Equal(RemainingCountText, view.RemainingLabel.Text);

            list.SelectedItem = list.Items[0];
            list.CompleteCommand.Execute(null);
            SampleCheck.Equal(RemainingAfterCompleteText, view.RemainingLabel.Text);
        }

        // Disposing the binding disconnects it: the label keeps the last text it was given.
        list.SelectedItem = list.Items[1];
        list.CompleteCommand.Execute(null);
        SampleCheck.Equal(RemainingAfterCompleteText, view.RemainingLabel.Text);
    }

    /// <summary>Formats a balance as currency with a converter function.</summary>
    public static void BindBalanceToLabelWithCurrencyFormat()
    {
        var accounts = OpenAccounts();
        AccountsView view = new();

        using (accounts.BindOneWay(view, x => x.SelectedAccount!.Balance, v => v.BalanceLabel.Text, FormatBalance))
        {
            accounts.SelectedAccount = accounts.Accounts[0];
            SampleCheck.Equal(EverydayBalanceText, view.BalanceLabel.Text);

            accounts.SelectedAccount.Balance = DepositedBalance;
            SampleCheck.Equal(DepositedBalanceText, view.BalanceLabel.Text);

            accounts.SelectedAccount = accounts.Accounts[1];
            SampleCheck.Equal("A$15,230.00", view.BalanceLabel.Text);
        }
    }

    /// <summary>Fills a progress bar from the upload percentage; the view-first <c>OneWayBind</c> returns the binding.</summary>
    public static void OneWayBindUploadPercentToProgressBar()
    {
        var storage = InMemoryObjectStorage.CreateSeeded(ManualClock.StartOfWorkingDay());
        var browser = OpenBucket(storage);
        StorageBrowserView view = new() { ViewModel = browser };
        List<double> changes = [];

        // A view-first binding reads its view model from the view's ViewModel property.
        using (var binding = view.OneWayBind(browser, x => x.UploadPercent, v => v.UploadProgressBar.Value))
        {
            SampleCheck.Equal(BindingDirection.OneWay, binding.Direction);
            SampleCheck.Equal(true, ReferenceEquals(view, binding.View));

            using (binding.Changed.Subscribe(changes.Add))
            {
                storage.Gate.Hold();
                browser.UploadCommand.Execute(new UploadRequest("launch-video.mp4", UploadSizeBytes, "video/mp4"));

                // The first release lets the request start; each release after that delivers one 4 MiB part.
                storage.Gate.ReleaseNext();
                for (var part = 1; part <= UploadPartCount; part++)
                {
                    storage.Gate.ReleaseNext();
                }

                SampleCheck.Equal(FullPercent, view.UploadProgressBar.Value);
                storage.Gate.ReleaseAll();
            }
        }

        // Changed replays the bar's starting value, so the empty bar is the first value a subscriber sees.
        SampleCheck.SequenceEqual([EmptyPercent, QuarterPercent, HalfPercent, ThreeQuartersPercent, FullPercent], changes);
    }

    /// <summary>Formats a balance for a label with a selector, the second <c>OneWayBind</c> overload.</summary>
    public static void OneWayBindAvailableBalanceWithSelector()
    {
        var accounts = OpenAccounts();
        AccountsView view = new() { ViewModel = accounts };

        using (view.OneWayBind(accounts, x => x.SelectedAccount!.AvailableBalance, v => v.AvailableBalanceLabel.Text, FormatBalance))
        {
            accounts.SelectedAccount = accounts.Accounts[0];

            // The everyday account has a 500 overdraft on top of its balance.
            SampleCheck.Equal("A$2,950.75", view.AvailableBalanceLabel.Text);
        }
    }

    /// <summary>Names the sequencer that delivers the write; work waits in the sequencer until the host runs it.</summary>
    public static void BindOneWayOnSequencer()
    {
        var board = OpenIssueBoard();
        IssueBoardView view = new();
        QueuedSequencer uiThread = new();

        using (board.BindOneWay(view, x => x.SelectedIssue!.Title, v => v.IssueTitleLabel.Text, uiThread))
        {
            board.SelectedIssue = board.Issues[SafariIssueIndex];

            SampleCheck.Equal(string.Empty, view.IssueTitleLabel.Text);

            _ = uiThread.RunPending();

            SampleCheck.Equal(SafariIssueTitle, view.IssueTitleLabel.Text);
        }
    }

    /// <summary>Converts with a function and delivers on a named sequencer.</summary>
    public static void BindOneWayConvertedOnSequencer()
    {
        var list = OpenTodoList();
        TodoView view = new();
        QueuedSequencer uiThread = new();

        using (list.BindOneWay(view, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture), uiThread))
        {
            SampleCheck.Equal(string.Empty, view.RemainingLabel.Text);

            _ = uiThread.RunPending();

            SampleCheck.Equal(RemainingCountText, view.RemainingLabel.Text);
        }
    }

    /// <summary>Converts with a converter object. Without a hint <see cref="CurrencyTextConverter"/> shows two decimal places.</summary>
    public static void BindOneWayWithConverterObject()
    {
        var accounts = OpenAccounts();
        AccountsView view = new();

        using (accounts.BindOneWay(view, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, new CurrencyTextConverter()))
        {
            SampleCheck.Equal(PlainTotalText, view.TotalBalanceLabel.Text);
        }
    }

    /// <summary>Passes a hint to the converter: the hint is the number format the total is shown in.</summary>
    public static void BindOneWayWithConverterObjectAndHint()
    {
        var accounts = OpenAccounts();
        AccountsView view = new();

        using (accounts.BindOneWay(view, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, new CurrencyTextConverter(), "N0"))
        {
            SampleCheck.Equal(GroupedTotalText, view.TotalBalanceLabel.Text);
        }
    }

    /// <summary>Passes a converter object, a hint and a named sequencer.</summary>
    public static void BindOneWayWithConverterObjectOnSequencer()
    {
        var accounts = OpenAccounts();
        AccountsView view = new();
        QueuedSequencer uiThread = new();

        using (accounts.BindOneWay(view, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, new CurrencyTextConverter(), "N0", uiThread))
        {
            SampleCheck.Equal(string.Empty, view.TotalBalanceLabel.Text);

            _ = uiThread.RunPending();

            SampleCheck.Equal(GroupedTotalText, view.TotalBalanceLabel.Text);
        }
    }

    /// <summary>Colours a badge from the priority of a to-do item with a converter object in the view-first <c>OneWayBind</c>.</summary>
    public static void OneWayBindPriorityToBadgeColour()
    {
        TodoItem item = new() { Title = ItemTitle, Priority = TodoPriority.High };
        TodoItemBadgeView view = new() { ViewModel = item };

        using (view.OneWayBind(item, x => x.Priority, v => v.PriorityBadge.Colour, new PriorityColourConverter()))
        {
            SampleCheck.Equal("#C62828", view.PriorityBadge.Colour);

            item.Priority = TodoPriority.Low;
            SampleCheck.Equal("#2E7D32", view.PriorityBadge.Colour);
        }
    }

    /// <summary>Passes the dark-theme hint to the priority converter.</summary>
    public static void OneWayBindPriorityToBadgeColourWithHint()
    {
        TodoItem item = new() { Title = ItemTitle, Priority = TodoPriority.High };
        TodoItemBadgeView view = new() { ViewModel = item };

        using (view.OneWayBind(item, x => x.Priority, v => v.PriorityBadge.Colour, new PriorityColourConverter(), DarkThemeHint))
        {
            SampleCheck.Equal("#EF5350", view.PriorityBadge.Colour);

            item.Priority = TodoPriority.Normal;
            SampleCheck.Equal("#BDBDBD", view.PriorityBadge.Colour);
        }
    }

    /// <summary>Passes the converter object, the hint and a named sequencer to <c>OneWayBind</c>.</summary>
    public static void OneWayBindPriorityToBadgeColourOnSequencer()
    {
        TodoItem item = new() { Title = ItemTitle, Priority = TodoPriority.High };
        TodoItemBadgeView view = new() { ViewModel = item };
        QueuedSequencer uiThread = new();

        using (view.OneWayBind(item, x => x.Priority, v => v.PriorityBadge.Colour, new PriorityColourConverter(), DarkThemeHint, uiThread))
        {
            SampleCheck.Equal(string.Empty, view.PriorityBadge.Colour);

            _ = uiThread.RunPending();

            SampleCheck.Equal("#EF5350", view.PriorityBadge.Colour);
        }
    }

    /// <summary>Writes the property paths of <c>OneWayBind</c> as <c>static</c> lambdas, with and without a selector.</summary>
    public static void OneWayBindWithStaticLambdas()
    {
        var accounts = OpenAccounts();
        AccountsView accountsView = new() { ViewModel = accounts };
        var list = OpenTodoList();
        TodoView todoView = new() { ViewModel = list };

        using (todoView.OneWayBind(list, static x => x.FilterText, static v => v.FilterTextBox.Text))
        using (accountsView.OneWayBind(accounts, static x => x.TotalBalance, static v => v.TotalBalanceLabel.Text, FormatBalance))
        {
            list.FilterText = "car";

            SampleCheck.Equal("car", todoView.FilterTextBox.Text);
            SampleCheck.Equal("A$17,680.75", accountsView.TotalBalanceLabel.Text);
        }
    }

    /// <summary>Resolves the same converter-object binding while the app runs, with <c>BindOneWayUnsafe</c>. Use it when a path is not an inline lambda.</summary>
    public static void BindOneWayUnsafeWithConverterObject()
    {
        var accounts = OpenAccounts();
        AccountsView view = new();
        QueuedSequencer uiThread = new();

        using (accounts.BindOneWayUnsafe(view, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, new CurrencyTextConverter(), uiThread, "N0"))
        {
            _ = uiThread.RunPending();

            SampleCheck.Equal(GroupedTotalText, view.TotalBalanceLabel.Text);
        }

        // Without a hint the converter falls back to two decimal places.
        using (accounts.BindOneWayUnsafe(view, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, new CurrencyTextConverter(), uiThread, null))
        {
            _ = uiThread.RunPending();

            SampleCheck.Equal(PlainTotalText, view.TotalBalanceLabel.Text);
        }
    }

    /// <summary>Formats with a selector and delivers the write on a named sequencer.</summary>
    public static void OneWayBindWithSelectorOnSequencer()
    {
        var accounts = OpenAccounts();
        AccountsView view = new() { ViewModel = accounts };
        QueuedSequencer uiThread = new();

        using (view.OneWayBind(accounts, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, FormatBalance, uiThread))
        {
            SampleCheck.Equal(string.Empty, view.TotalBalanceLabel.Text);

            _ = uiThread.RunPending();

            SampleCheck.Equal("A$17,680.75", view.TotalBalanceLabel.Text);
        }
    }

    /// <summary>Resolves the same view-first converter-object binding while the app runs, with <c>OneWayBindUnsafe</c>.</summary>
    public static void OneWayBindUnsafeWithConverterObject()
    {
        var accounts = OpenAccounts();
        AccountsView view = new() { ViewModel = accounts };
        QueuedSequencer uiThread = new();

        using (view.OneWayBindUnsafe(accounts, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, new CurrencyTextConverter(), uiThread, "N1"))
        {
            _ = uiThread.RunPending();

            SampleCheck.Equal("$17,680.8", view.TotalBalanceLabel.Text);
        }
    }

    /// <summary>
    /// Passes a null scheduler to <c>BindOneWay</c>. Null means the same as naming no scheduler: the write happens on the
    /// thread that owns the target, so a target with no owning thread is written inline. The argument is named because a
    /// bare <c>null</c> also fits the string parameters of the overload that takes no scheduler.
    /// </summary>
    public static void BindOneWayWithNullScheduler()
    {
        var board = OpenIssueBoard();
        IssueBoardView view = new();

        using (board.BindOneWay(view, x => x.SelectedIssue!.Title, v => v.IssueTitleLabel.Text, scheduler: null))
        {
            board.SelectedIssue = board.Issues[SafariIssueIndex];
            SampleCheck.Equal(SafariIssueTitle, view.IssueTitleLabel.Text);

            board.SelectedIssue = board.Issues[0];
            SampleCheck.Equal(GiftCardIssueTitle, view.IssueTitleLabel.Text);
        }
    }

    /// <summary>Converts with a function and passes a null scheduler; the label holds the text as soon as the binding is made.</summary>
    public static void BindOneWayConvertedWithNullScheduler()
    {
        var list = OpenTodoList();
        TodoView view = new();

        using (list.BindOneWay(view, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture), scheduler: null))
        {
            SampleCheck.Equal(RemainingCountText, view.RemainingLabel.Text);

            list.SelectedItem = list.Items[0];
            list.CompleteCommand.Execute(null);
            SampleCheck.Equal(RemainingAfterCompleteText, view.RemainingLabel.Text);
        }
    }

    /// <summary>Formats with a selector and passes a null scheduler to the view-first <c>OneWayBind</c>.</summary>
    public static void OneWayBindWithSelectorAndNullScheduler()
    {
        var accounts = OpenAccounts();
        AccountsView view = new() { ViewModel = accounts };

        using (view.OneWayBind(accounts, x => x.SelectedAccount!.Balance, v => v.BalanceLabel.Text, FormatBalance, scheduler: null))
        {
            accounts.SelectedAccount = accounts.Accounts[0];
            SampleCheck.Equal(EverydayBalanceText, view.BalanceLabel.Text);

            accounts.SelectedAccount.Balance = DepositedBalance;
            SampleCheck.Equal(DepositedBalanceText, view.BalanceLabel.Text);
        }
    }

    /// <summary>Formats an amount for a label.</summary>
    /// <param name="amount">The amount of money.</param>
    /// <returns>The amount with a currency sign and two decimal places.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string FormatBalance(decimal amount) => FormattableString.Invariant($"A${amount:N2}");

    /// <summary>Signs in as Priya, opens the web shop repository and loads its open issues.</summary>
    /// <returns>The board with two open issues, newest first.</returns>
    private static IssueBoardViewModel OpenIssueBoard()
    {
        var server = InMemoryGitHubServer.CreateSeeded(ManualClock.StartOfWorkingDay());
        IssueBoardViewModel board = new(server) { Token = InMemoryGitHubServer.PriyaToken };

        // The server answers at once until an example holds its gate, so each command has finished when Execute returns.
        board.SignInCommand.Execute(null);
        board.SelectedRepository = board.Repositories[0];
        board.LoadIssuesCommand.Execute(null);
        return board;
    }

    /// <summary>Loads the seeded to-do list.</summary>
    /// <returns>The list with three unfinished items and one finished item.</returns>
    private static TodoListViewModel OpenTodoList()
    {
        TodoListViewModel list = new(InMemoryTodoStore.CreateSeeded());
        list.LoadCommand.Execute(null);
        return list;
    }

    /// <summary>Loads the accounts of the seeded bank.</summary>
    /// <returns>The accounts screen with an everyday and a savings account and nothing selected.</returns>
    private static AccountsViewModel OpenAccounts()
    {
        AccountsViewModel accounts = new(new InMemoryBankingBackend(ManualClock.StartOfWorkingDay()));
        accounts.LoadAccountsCommand.Execute(null);
        return accounts;
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

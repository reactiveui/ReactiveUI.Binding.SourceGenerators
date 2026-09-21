// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.BindingsInvokeCommandFromStreams;

/// <summary>Shows <c>InvokeCommand</c> driven by streams: a selection, a timer, a trigger and a start-up signal, none of them a button.</summary>
public static class InvokeCommandFromStreamsExamples
{
    /// <summary>The seconds between two refreshes of the to-do list.</summary>
    private const int RefreshSeconds = 30;

    /// <summary>The number of items in the seeded to-do list.</summary>
    private const int SeededItemCount = 4;

    /// <summary>The number of unfinished items in the seeded to-do list.</summary>
    private const int SeededRemainingCount = 3;

    /// <summary>The number of items once one has been added to the seeded to-do list.</summary>
    private const int ItemCountAfterAdd = 5;

    /// <summary>The amount of the transfer the customer sends.</summary>
    private const decimal TransferAmount = 50M;

    /// <summary>The balance of the account after the transfer.</summary>
    private const decimal BalanceAfterTransfer = 2400.75M;

    /// <summary>The title of an item another device adds to the to-do list.</summary>
    private const string RatesTitle = "Pay council rates";

    /// <summary>The title of an item another device adds after the refresh stops.</summary>
    private const string PassportTitle = "Renew passport";

    /// <summary>The title of the open issue in the webshop repository that was reported last.</summary>
    private const string GiftCardIssue = "Add gift-card support";

    /// <summary>The title of the open issue in the webshop repository that was reported first.</summary>
    private const string SafariIssue = "Checkout button unresponsive on Safari";

    /// <summary>The title of the open issue in the payments repository that was reported last.</summary>
    private const string RefundIssue = "Document the refund endpoint";

    /// <summary>The title of the open issue in the payments repository that was reported first.</summary>
    private const string GatewayIssue = "Card gateway times out under load";

    /// <summary>The moment the virtual clock starts at.</summary>
    private const string StartInstant = "2026-03-03T09:00:00Z";

    /// <summary>Loads the issues of a repository each time the user selects one.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task LoadIssuesWhenRepositoryIsSelected()
    {
        var clock = ManualClock.StartOfWorkingDay();
        var server = InMemoryGitHubServer.CreateSeeded(clock);
        IssueBoardViewModel viewModel = new(server) { Token = InMemoryGitHubServer.PriyaToken };
        viewModel.SignInCommand.Execute(null);
        await viewModel.SignInCommand.Completion;

        var webshop = viewModel.Repositories[0];
        var payments = viewModel.Repositories[1];

        // The stream starts with the current selection, which is null. The command refuses it, so nothing loads yet.
        using (viewModel.WhenChanged(x => x.SelectedRepository!).InvokeCommand(viewModel.LoadIssuesCommand))
        {
            SampleCheck.Equal(0, viewModel.Issues.Count);

            viewModel.SelectedRepository = webshop;
            await viewModel.LoadIssuesCommand.Completion;

            SampleCheck.SequenceEqual([GiftCardIssue, SafariIssue], IssueTitles(viewModel));

            viewModel.SelectedRepository = payments;
            await viewModel.LoadIssuesCommand.Completion;

            SampleCheck.SequenceEqual([RefundIssue, GatewayIssue], IssueTitles(viewModel));
        }
    }

    /// <summary>Refreshes the to-do list on a fixed interval, with the interval driven by a virtual clock.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task RefreshTodoItemsOnAnInterval()
    {
        var store = InMemoryTodoStore.CreateSeeded();
        TodoListViewModel viewModel = new(store);
        VirtualClock clock = new(SeedData.Instant(StartInstant));
        var interval = TimeSpan.FromSeconds(RefreshSeconds);

        using (Signal.Every(interval, clock).InvokeCommand(viewModel, x => x.LoadCommand))
        {
            SampleCheck.Equal(0, viewModel.Items.Count);

            clock.AdvanceBy(interval);
            await viewModel.LoadCommand.Completion;

            SampleCheck.Equal(SeededItemCount, viewModel.Items.Count);

            _ = await store.AddAsync(new TodoItem { Title = RatesTitle });

            SampleCheck.Equal(SeededItemCount, viewModel.Items.Count);

            clock.AdvanceBy(interval);
            await viewModel.LoadCommand.Completion;

            SampleCheck.Equal(ItemCountAfterAdd, viewModel.Items.Count);
        }

        // Disposing the subscription stops the timer from reaching the command.
        _ = await store.AddAsync(new TodoItem { Title = PassportTitle });
        clock.AdvanceBy(interval);

        SampleCheck.Equal(ItemCountAfterAdd, viewModel.Items.Count);
    }

    /// <summary>Drops the ticks that arrive while the previous refresh is still running, because the command reports it cannot execute.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task SkipTicksWhileTheCommandIsRunning()
    {
        var store = InMemoryTodoStore.CreateSeeded();
        TodoListViewModel viewModel = new(store);
        VirtualClock clock = new(SeedData.Instant(StartInstant));
        var interval = TimeSpan.FromSeconds(RefreshSeconds);

        store.Gate.Hold();

        using (Signal.Every(interval, clock).InvokeCommand(viewModel.LoadCommand))
        {
            clock.AdvanceBy(interval);

            SampleCheck.Equal(true, viewModel.LoadCommand.IsRunning);
            SampleCheck.Equal(1, store.Gate.PendingCount);

            // Two more ticks while the first load waits for the server: neither starts a request.
            clock.AdvanceBy(interval);
            clock.AdvanceBy(interval);

            SampleCheck.Equal(1, store.Gate.PendingCount);

            store.Gate.ReleaseAll();
            await viewModel.LoadCommand.Completion;

            SampleCheck.Equal(false, viewModel.LoadCommand.IsRunning);
            SampleCheck.Equal(SeededItemCount, viewModel.Items.Count);

            // The next tick after the load has finished runs the command again.
            _ = await store.AddAsync(new TodoItem { Title = RatesTitle });
            clock.AdvanceBy(interval);
            await viewModel.LoadCommand.Completion;

            SampleCheck.Equal(ItemCountAfterAdd, viewModel.Items.Count);
        }
    }

    /// <summary>Drops the values that arrive while the command is disabled, and runs the command once the draft is valid.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task SkipValuesWhileTheCommandIsDisabled()
    {
        var clock = ManualClock.StartOfWorkingDay();
        InMemoryBankingBackend backend = new(clock);
        TransferViewModel viewModel = new(backend);
        viewModel.LoadCommand.Execute(null);
        await viewModel.LoadCommand.Completion;

        var confirmations = 0;
        using var handler = viewModel.ConfirmTransfer.RegisterHandler(context =>
        {
            confirmations++;
            context.SetOutput(true);
        });

        using Signal<RxVoid> submit = new();

        using (submit.InvokeCommand(viewModel.TransferCommand))
        {
            // The draft has no account, payee or amount, so the command cannot execute.
            SampleCheck.Equal(false, viewModel.IsValid);

            submit.OnNext(RxVoid.Default);

            SampleCheck.Equal(0, confirmations);
            SampleCheck.Equal(false, viewModel.TransferCommand.IsRunning);
            SampleCheck.Equal(true, viewModel.LastReceipt is null);

            viewModel.Draft.Source = viewModel.Accounts[0];
            viewModel.Draft.Payee = viewModel.Payees[0];
            viewModel.Draft.Amount = TransferAmount;

            SampleCheck.Equal(true, viewModel.IsValid);

            submit.OnNext(RxVoid.Default);
            await viewModel.TransferCommand.Completion;

            SampleCheck.Equal(1, confirmations);
            SampleCheck.Equal(BalanceAfterTransfer, viewModel.LastReceipt!.NewBalance);
        }
    }

    /// <summary>Runs a command once at start-up from a single-value stream, as a view-model unit test does.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task LoadOnceAtStartUp()
    {
        var store = InMemoryTodoStore.CreateSeeded();
        TodoListViewModel viewModel = new(store);

        using (Signal.Emit(RxVoid.Default).InvokeCommand(viewModel.LoadCommand))
        {
            await viewModel.LoadCommand.Completion;

            SampleCheck.Equal(SeededItemCount, viewModel.Items.Count);
            SampleCheck.Equal(SeededRemainingCount, viewModel.RemainingCount);
        }
    }

    /// <summary>Reads the titles of the issues a view model shows.</summary>
    /// <param name="viewModel">The view model.</param>
    /// <returns>The titles, in the order the view model lists them.</returns>
    private static List<string> IssueTitles(IssueBoardViewModel viewModel)
    {
        List<string> titles = [];
        foreach (var issue in viewModel.Issues)
        {
            titles.Add(issue.Title);
        }

        return titles;
    }
}

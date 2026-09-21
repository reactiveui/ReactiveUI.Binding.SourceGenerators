// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Education;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.BindingsInteractions;

/// <summary>Demonstrates <c>BindInteraction</c> and the interaction types a view model and a view share.</summary>
public static class BindingsInteractionsExamples
{
    /// <summary>The amount of a transfer large enough to need an approval code.</summary>
    private const decimal LargeTransferAmount = 1200M;

    /// <summary>The number of students on the Introduction to Programming roster.</summary>
    private const int ProgrammingRosterCount = 3;

    /// <summary>The position of Ben Carter on the Introduction to Programming roster.</summary>
    private const int BenRosterIndex = 1;

    /// <summary>The number of the issue the interaction examples ask about.</summary>
    private const int SafariIssueNumber = 101;

    /// <summary>The message an interaction carries when nothing handled it.</summary>
    private const string UnhandledMessage = "Failed to find a registration for an Interaction.";

    /// <summary>The message of an exception a host raises when nobody answers close confirmations.</summary>
    private const string NobodyAnswersMessage = "Nobody answers close confirmations.";

    /// <summary>The answer of the handler registered first.</summary>
    private const string FirstAnswer = "first";

    /// <summary>The answer of the handler registered last.</summary>
    private const string LatestAnswer = "latest";

    /// <summary>Answers the close confirmation through a task-based handler that waits for the user, so the command stays pending until the dialog is answered.</summary>
    /// <returns>A task that completes when the issue is closed.</returns>
    public static async Task ConfirmCloseWithTaskHandler()
    {
        var viewModel = await OpenWebshopIssuesAsync().ConfigureAwait(false);
        IssueBoardView view = new() { ViewModel = viewModel };
        ResponseGate dialog = new();
        dialog.Hold();

        using var binding = view.BindInteraction(viewModel, x => x.ConfirmClose, async context =>
        {
            await dialog.WaitAsync().ConfigureAwait(false);
            context.SetOutput(true);
        });

        var issue = viewModel.Issues[0];
        viewModel.SelectedIssue = issue;
        viewModel.CloseIssueCommand.Execute(null);

        SampleCheck.Equal(true, viewModel.CloseIssueCommand.IsRunning);
        SampleCheck.Equal(IssueState.Open, issue.State);

        dialog.ReleaseAll();
        await viewModel.CloseIssueCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal(IssueState.Closed, issue.State);
    }

    /// <summary>Answers the drop confirmation through an observable-based handler; the handler's sequence completes once the answer is set.</summary>
    /// <returns>A task that completes when the second attempt has dropped the student.</returns>
    public static async Task ConfirmDropWithObservableHandler()
    {
        var records = InMemoryStudentRecords.CreateSeeded(ManualClock.StartOfWorkingDay());
        GradebookViewModel viewModel = new(records);
        viewModel.LoadCoursesCommand.Execute(null);
        await viewModel.LoadCoursesCommand.Completion.ConfigureAwait(false);
        viewModel.SelectedCourse = viewModel.Courses[0];
        viewModel.OpenCourseCommand.Execute(null);
        await viewModel.OpenCourseCommand.Completion.ConfigureAwait(false);
        viewModel.SelectedStudent = viewModel.Roster[BenRosterIndex];
        GradebookView view = new() { ViewModel = viewModel };
        var teacherConfirms = false;

        using var binding = view.BindInteraction(viewModel, x => x.ConfirmDrop, context =>
        {
            context.SetOutput(teacherConfirms);
            return Signal.Return(context.Input.Student.Id);
        });

        viewModel.DropCommand.Execute(null);
        await viewModel.DropCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal(ProgrammingRosterCount, viewModel.Roster.Count);

        teacherConfirms = true;
        viewModel.DropCommand.Execute(null);
        await viewModel.DropCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal(ProgrammingRosterCount - 1, viewModel.Roster.Count);
    }

    /// <summary>Binds an interaction that the view model exposes as <see cref="IInteraction{TInput, TOutput}"/>.</summary>
    /// <returns>A task that completes when the issue is closed.</returns>
    public static async Task ConfirmCloseThroughInterfaceProperty()
    {
        var board = await OpenWebshopIssuesAsync().ConfigureAwait(false);
        IssueTriageViewModel viewModel = new(board);
        IssueTriageView view = new() { ViewModel = viewModel };

        using var binding = view.BindInteraction(viewModel, x => x.ConfirmClose, static context =>
        {
            context.SetOutput(context.Input.State == IssueState.Open);
            return Task.CompletedTask;
        });

        var issue = board.Issues[0];
        board.SelectedIssue = issue;
        board.CloseIssueCommand.Execute(null);
        await board.CloseIssueCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal(IssueState.Closed, issue.State);
    }

    /// <summary>Approves a large transfer in two steps: the view confirms the draft, and then the bank asks the view for a one-time code.</summary>
    /// <returns>A task that completes when the transfer is sent.</returns>
    public static async Task ApproveLargeTransferInTwoSteps()
    {
        var (viewModel, view) = await OpenTransferScreenAsync().ConfigureAwait(false);
        List<string> steps = [];

        using var confirmation = view.BindInteraction(viewModel, x => x.ConfirmTransfer, context =>
        {
            steps.Add("confirm");
            context.SetOutput(true);
            return Task.CompletedTask;
        });

        using var approval = view.BindInteraction(viewModel, x => x.ApproveTransfer, context =>
        {
            steps.Add("approve");
            context.SetOutput(InMemoryBankingBackend.ApprovalCode);
            return Task.CompletedTask;
        });

        FillLargeTransfer(viewModel);
        viewModel.TransferCommand.Execute(null);
        await viewModel.TransferCommand.Completion.ConfigureAwait(false);

        SampleCheck.SequenceEqual(["confirm", "approve"], steps);
        SampleCheck.Equal(LargeTransferAmount, viewModel.LastReceipt!.Amount);
    }

    /// <summary>Cancels a large transfer at the second step: an empty approval code tells the view model the customer gave up.</summary>
    /// <returns>A task that completes when the view model has reported the cancellation.</returns>
    public static async Task CancelTransferAtApprovalStep()
    {
        var (viewModel, view) = await OpenTransferScreenAsync().ConfigureAwait(false);

        using var confirmation = view.BindInteraction(viewModel, x => x.ConfirmTransfer, static context =>
        {
            context.SetOutput(true);
            return Task.CompletedTask;
        });

        using var approval = view.BindInteraction(viewModel, x => x.ApproveTransfer, static context =>
        {
            context.SetOutput(string.Empty);
            return Task.CompletedTask;
        });

        FillLargeTransfer(viewModel);
        viewModel.TransferCommand.Execute(null);
        await viewModel.TransferCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal("The transfer was cancelled.", viewModel.ErrorMessage);
        SampleCheck.Equal(true, viewModel.LastReceipt is null);
    }

    /// <summary>Registers the three kinds of handler on an interaction; the handler registered last answers first, and disposing a registration removes it.</summary>
    /// <returns>A task that completes when the last answer has been read.</returns>
    public static async Task RegisterHandlersInReverseOrder()
    {
        Interaction<TransferDraft, string> approval = new();
        TransferDraft draft = new();
        using var first = approval.RegisterHandler(static context => context.SetOutput(FirstAnswer));
        using var ignoring = approval.RegisterHandler(static context => Signal.Return(context.IsHandled));
        var latest = approval.RegisterHandler(static context =>
        {
            context.SetOutput(LatestAnswer);
            return Task.CompletedTask;
        });

        SampleCheck.Equal(LatestAnswer, await approval.Handle(draft).ConfigureAwait(false));

        latest.Dispose();

        SampleCheck.Equal(FirstAnswer, await approval.Handle(draft).ConfigureAwait(false));
    }

    /// <summary>Reads what a handler receives: the input, whether the interaction is handled, and the output once it is set.</summary>
    /// <returns>A task that completes when the handler has run.</returns>
    public static async Task InspectInteractionContext()
    {
        Interaction<Issue, bool> confirmClose = new();
        Issue issue = new() { Number = SafariIssueNumber };
        Issue? receivedInput = null;
        var handledBefore = true;
        var handledAfter = false;
        var isInteractionContext = false;
        var earlyReadRefused = false;
        var readBack = false;
        var secondAnswerRefused = false;

        using var registration = confirmClose.RegisterHandler(context =>
        {
            receivedInput = context.Input;
            handledBefore = context.IsHandled;
            isInteractionContext = context is InteractionContext<Issue, bool>;

            var output = (IOutputContext<Issue, bool>)context;
            earlyReadRefused = RefusesWithInvalidOperation(() => output.GetOutput());

            context.SetOutput(true);
            handledAfter = context.IsHandled;
            readBack = output.GetOutput();
            secondAnswerRefused = RefusesWithInvalidOperation(() => context.SetOutput(false));
        });

        var answer = await confirmClose.Handle(issue).ConfigureAwait(false);

        SampleCheck.Equal(true, ReferenceEquals(issue, receivedInput));
        SampleCheck.Equal(false, handledBefore);
        SampleCheck.Equal(true, handledAfter);
        SampleCheck.Equal(true, isInteractionContext);
        SampleCheck.Equal(true, earlyReadRefused);
        SampleCheck.Equal(true, readBack);
        SampleCheck.Equal(true, secondAnswerRefused);
        SampleCheck.Equal(true, answer);
    }

    /// <summary>Closes an issue while no view has bound a handler; the command's task faults with <see cref="UnhandledInteractionException{TInput, TOutput}"/>.</summary>
    /// <returns>A task that completes when the failure has been read.</returns>
    public static async Task CloseIssueWithoutHandler()
    {
        var viewModel = await OpenWebshopIssuesAsync().ConfigureAwait(false);
        var issue = viewModel.Issues[0];
        viewModel.SelectedIssue = issue;
        UnhandledInteractionException<Issue, bool>? failure = null;

        viewModel.CloseIssueCommand.Execute(null);

        try
        {
            await viewModel.CloseIssueCommand.Completion.ConfigureAwait(false);
        }
        catch (UnhandledInteractionException<Issue, bool> ex)
        {
            failure = ex;
        }

        SampleCheck.Equal(true, failure is not null);
        SampleCheck.Equal(UnhandledMessage, failure!.Message);
        SampleCheck.Equal(true, ReferenceEquals(issue, failure.Input));
        SampleCheck.Equal(true, ReferenceEquals(viewModel.ConfirmClose, failure.Interaction));
        SampleCheck.Equal(IssueState.Open, issue.State);
    }

    /// <summary>
    /// Derives from <see cref="Interaction{TInput, TOutput}"/> to keep an audit trail: the subclass overrides <c>Handle</c>,
    /// reads the registered handlers with <c>GetHandlers</c> and wraps the context that <c>GenerateContext</c> creates.
    /// </summary>
    /// <returns>A task that completes when the approval has been given.</returns>
    public static async Task AuditApprovalWithDerivedInteraction()
    {
        AuditedInteraction<decimal, string> approval = new();
        using var issuer = approval.RegisterHandler(static context => context.SetOutput(InMemoryBankingBackend.ApprovalCode));
        using var observer = approval.RegisterHandler(static context => Signal.Return(context.IsHandled));

        var code = await approval.Handle(LargeTransferAmount).ConfigureAwait(false);

        SampleCheck.Equal(InMemoryBankingBackend.ApprovalCode, code);
        SampleCheck.SequenceEqual([$"ask {LargeTransferAmount} of 2 handlers", $"answer {InMemoryBankingBackend.ApprovalCode}"], approval.Audit);
    }

    /// <summary>Stops handling once the binding is disposed: the next question goes unanswered.</summary>
    /// <returns>A task that completes when the second question has failed.</returns>
    public static async Task DisposeBindingRemovesHandler()
    {
        var viewModel = await OpenWebshopIssuesAsync().ConfigureAwait(false);
        IssueBoardView view = new() { ViewModel = viewModel };
        var issue = viewModel.Issues[0];
        var binding = view.BindInteraction(viewModel, x => x.ConfirmClose, static context =>
        {
            context.SetOutput(false);
            return Task.CompletedTask;
        });

        SampleCheck.Equal(false, await viewModel.ConfirmClose.Handle(issue).ConfigureAwait(false));

        binding.Dispose();

        var unanswered = false;
        try
        {
            _ = await viewModel.ConfirmClose.Handle(issue).ConfigureAwait(false);
        }
        catch (UnhandledInteractionException<Issue, bool>)
        {
            unanswered = true;
        }

        SampleCheck.Equal(true, unanswered);
    }

    /// <summary>Binds while the view has no view model yet: nothing is registered, nothing throws, and disposing the binding is safe.</summary>
    public static void BindInteractionWithoutViewModel()
    {
        IssueBoardView view = new();
        IssueBoardViewModel? viewModel = null;
        var handlerRuns = 0;

        var binding = view.BindInteraction(viewModel, x => x.ConfirmClose, context =>
        {
            handlerRuns++;
            context.SetOutput(true);
            return Task.CompletedTask;
        });

        binding.Dispose();

        SampleCheck.Equal(0, handlerRuns);
    }

    /// <summary>Creates <see cref="UnhandledInteractionException{TInput, TOutput}"/> with each constructor it offers.</summary>
    public static void ConstructUnhandledInteractionException()
    {
        Interaction<Issue, bool> confirmClose = new();
        Issue issue = new() { Number = SafariIssueNumber };
        InvalidOperationException cause = new("The dialog host has shut down.");

        UnhandledInteractionException<Issue, bool> empty = new();
        UnhandledInteractionException<Issue, bool> withMessage = new(NobodyAnswersMessage);
        UnhandledInteractionException<Issue, bool> withCause = new(NobodyAnswersMessage, cause);
        UnhandledInteractionException<Issue, bool> forQuestion = new(confirmClose, issue);

        SampleCheck.Equal(true, empty.Interaction is null);
        SampleCheck.Equal(NobodyAnswersMessage, withMessage.Message);
        SampleCheck.Equal(true, ReferenceEquals(cause, withCause.InnerException));
        SampleCheck.Equal(UnhandledMessage, forQuestion.Message);
        SampleCheck.Equal(true, ReferenceEquals(confirmClose, forQuestion.Interaction));
        SampleCheck.Equal(true, ReferenceEquals(issue, forQuestion.Input));
    }

    /// <summary>Registers each kind of handler on an interaction, asks a question and removes a handler, using only <see cref="IInteraction{TInput, TOutput}"/>.</summary>
    /// <param name="confirmClose">The interaction, as its interface.</param>
    /// <returns>A task that completes when the last answer has been read.</returns>
    public static async Task HandleThroughInterface(IInteraction<Issue, bool> confirmClose)
    {
        Issue issue = new() { Number = SafariIssueNumber };
        using var declines = confirmClose.RegisterHandler(static context => context.SetOutput(false));
        using var asksAgain = confirmClose.RegisterHandler(static context => Signal.Return(context.IsHandled));
        var confirms = confirmClose.RegisterHandler(static context =>
        {
            context.SetOutput(true);
            return Task.CompletedTask;
        });

        SampleCheck.Equal(true, await confirmClose.Handle(issue).ConfigureAwait(false));

        confirms.Dispose();

        SampleCheck.Equal(false, await confirmClose.Handle(issue).ConfigureAwait(false));
    }

    /// <summary>Runs an action and reports whether it threw <see cref="InvalidOperationException"/>.</summary>    /// <param name="action">The action to run.</param>
    /// <returns><see langword="true"/> when the action threw.</returns>
    private static bool RefusesWithInvalidOperation(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    /// <summary>Fills the transfer draft with a payment to the utility that needs an approval code.</summary>
    /// <param name="viewModel">The transfer view model.</param>
    private static void FillLargeTransfer(TransferViewModel viewModel)
    {
        viewModel.Draft.Source = viewModel.Accounts[0];
        viewModel.Draft.Payee = viewModel.Payees[0];
        viewModel.Draft.Amount = LargeTransferAmount;
    }

    /// <summary>Loads the accounts and payees of the transfer screen.</summary>
    /// <returns>A task that completes with the view model and its view.</returns>
    private static async Task<(TransferViewModel ViewModel, TransferView View)> OpenTransferScreenAsync()
    {
        InMemoryBankingBackend backend = new(ManualClock.StartOfWorkingDay());
        TransferViewModel viewModel = new(backend);
        viewModel.LoadCommand.Execute(null);
        await viewModel.LoadCommand.Completion.ConfigureAwait(false);
        return (viewModel, new() { ViewModel = viewModel });
    }

    /// <summary>Signs in as Priya Nair and loads the open issues of the webshop repository.</summary>
    /// <returns>A task that completes with the view model once the issues are loaded.</returns>
    private static async Task<IssueBoardViewModel> OpenWebshopIssuesAsync()
    {
        var server = InMemoryGitHubServer.CreateSeeded(ManualClock.StartOfWorkingDay());
        IssueBoardViewModel viewModel = new(server) { Token = InMemoryGitHubServer.PriyaToken };
        viewModel.SignInCommand.Execute(null);
        await viewModel.SignInCommand.Completion.ConfigureAwait(false);
        viewModel.SelectedRepository = viewModel.Repositories[0];
        viewModel.LoadIssuesCommand.Execute(null);
        await viewModel.LoadIssuesCommand.Completion.ConfigureAwait(false);
        return viewModel;
    }
}

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.ObservingWhenAnyValue;

/// <summary>
/// Shows every <c>WhenAnyValue</c> overload, from one observed property to sixteen, each with and without a value
/// selector. <c>WhenAnyValue</c> delivers the current values when you subscribe and again after each change.
/// </summary>
public static class WhenAnyValueExamples
{
    /// <summary>The number of values delivered by the subscription and one change.</summary>
    private const int AfterOneChange = 2;

    /// <summary>The identifier of the registration task.</summary>
    private const int RegistrationId = 7;

    /// <summary>The title of the registration task.</summary>
    private const string RegistrationTitle = "Renew car registration";

    /// <summary>The title the registration task is renamed to.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The notes of the registration task.</summary>
    private const string RegistrationNotes = "Bring the insurance certificate";

    /// <summary>The label of an errand.</summary>
    private const string ErrandsTag = "errands";

    /// <summary>The label of a car task.</summary>
    private const string CarTag = "car";

    /// <summary>The label of an urgent task.</summary>
    private const string UrgentTag = "urgent";

    /// <summary>The prefix of a finished checklist line.</summary>
    private const string DoneMark = "[x] ";

    /// <summary>The prefix of an open checklist line.</summary>
    private const string OpenMark = "[ ] ";

    /// <summary>The format of a deadline in a headline.</summary>
    private const string IsoDay = "yyyy-MM-dd";

    /// <summary>The number of the checkout issue.</summary>
    private const int CheckoutNumber = 101;

    /// <summary>The title of the checkout issue.</summary>
    private const string CheckoutTitle = "Checkout button unresponsive on Safari";

    /// <summary>The title the checkout issue is renamed to.</summary>
    private const string RenamedIssueTitle = "Checkout button unresponsive on Safari 17";

    /// <summary>The account name of the person the issue is assigned to.</summary>
    private const string PriyaLogin = "priya-nair";

    /// <summary>The account name of the person the issue is reassigned to.</summary>
    private const string TomasLogin = "tomas-berg";

    /// <summary>The label of a defect.</summary>
    private const string BugLabel = "bug";

    /// <summary>The label of the checkout area.</summary>
    private const string CheckoutLabel = "checkout";

    /// <summary>The comment the user is typing on the selected issue.</summary>
    private const string CommentDraft = "Fixed in the next release.";

    /// <summary>The identifier of the account the money leaves.</summary>
    private const string EverydayId = "ACC-1001";

    /// <summary>The name of the account the money leaves.</summary>
    private const string EverydayName = "Everyday";

    /// <summary>The currency of the account the money leaves.</summary>
    private const string Currency = "AUD";

    /// <summary>The balance of the account the money leaves.</summary>
    private const decimal EverydayBalance = 4200.50M;

    /// <summary>How far below zero the account may go.</summary>
    private const decimal EverydayOverdraft = 500M;

    /// <summary>The money the customer can spend: the balance plus the overdraft limit.</summary>
    private const decimal EverydayAvailable = 4700.50M;

    /// <summary>The identifier of the payee.</summary>
    private const int PayeeId = 3;

    /// <summary>The most the customer allows in one transfer to the payee.</summary>
    private const decimal PayeeLimit = 1000M;

    /// <summary>The amount the customer starts with.</summary>
    private const decimal RentAmount = 250M;

    /// <summary>An amount above the payee's limit and below the available balance.</summary>
    private const decimal AbovePayeeLimit = 1500M;

    /// <summary>An amount above the available balance.</summary>
    private const decimal AboveAvailable = 9000M;

    /// <summary>The reference the customer starts with.</summary>
    private const string RentReference = "Rent March";

    /// <summary>The reference the customer changes to.</summary>
    private const string RentReferenceRevised = "Rent March, paid early";

    /// <summary>The deadline of the registration task.</summary>
    private static readonly DateOnly _registrationDue = SeedData.Date("2026-03-20");

    /// <summary>The deadline the registration task moves to.</summary>
    private static readonly DateOnly _movedDue = SeedData.Date("2026-03-27");

    /// <summary>The labels of the registration task.</summary>
    private static readonly IReadOnlyList<string> _registrationTags = [ErrandsTag, CarTag];

    /// <summary>The labels of the registration task once it is urgent.</summary>
    private static readonly IReadOnlyList<string> _urgentTags = [ErrandsTag, CarTag, UrgentTag];

    /// <summary>The labels of the checkout issue.</summary>
    private static readonly IReadOnlyList<string> _checkoutLabels = [BugLabel, CheckoutLabel];

    /// <summary>The comments of a new issue.</summary>
    private static readonly IReadOnlyList<IssueComment> _noComments = [];

    /// <summary>When the checkout issue last changed.</summary>
    private static readonly DateTimeOffset _checkoutUpdatedAt = SeedData.Instant("2026-03-02T15:05:00Z");

    /// <summary>The person the issue is assigned to.</summary>
    private static readonly User _priya = new() { Login = PriyaLogin, DisplayName = "Priya Nair" };

    /// <summary>The person who reported the issue.</summary>
    private static readonly User _maria = new() { Login = "maria-santos", DisplayName = "Maria Santos" };

    /// <summary>The person the issue is reassigned to.</summary>
    private static readonly User _tomas = new() { Login = TomasLogin, DisplayName = "Tomas Berg" };

    /// <summary>The payee of the transfer.</summary>
    private static readonly Payee _landlord = new(PayeeId, "Harbour Realty", "062-000 1234 5678", PayeeLimit);

    /// <summary>Observes the title of a to-do item; a single property delivers the value itself.</summary>
    public static void ObserveTodoTitle()
    {
        var item = CreateRegistrationTask();

        using var titles = item.WhenAnyValue(x => x.Title).Record();

        SampleCheck.SequenceEqual([RegistrationTitle], titles.Values);

        item.Title = RenamedTitle;

        SampleCheck.SequenceEqual([RegistrationTitle, RenamedTitle], titles.Values);
    }

    /// <summary>Turns the title of a to-do item into its length with a value selector, as a character counter does.</summary>
    public static void ProjectTodoTitleLength()
    {
        var item = CreateRegistrationTask();

        using var lengths = item.WhenAnyValue(x => x.Title, static title => title.Length).Record();

        item.Title = RenamedTitle;

        SampleCheck.SequenceEqual([RegistrationTitle.Length, RenamedTitle.Length], lengths.Values);
    }

    /// <summary>Observes the title and the done flag of a to-do item together.</summary>
    public static void ObserveTodoTitleAndStatus()
    {
        var item = CreateRegistrationTask();

        using var seen = item.WhenAnyValue(x => x.Title, x => x.IsDone).Record();

        SampleCheck.Equal(new(RegistrationTitle, false), seen.Latest);

        item.IsDone = true;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(true, seen.Latest.Property2);
    }

    /// <summary>Turns the title and the done flag into one checklist line with a value selector.</summary>
    public static void ProjectTodoChecklistLine()
    {
        var item = CreateRegistrationTask();

        using var lines = item.WhenAnyValue(x => x.Title, x => x.IsDone, static (title, isDone) => (isDone ? DoneMark : OpenMark) + title).Record();

        SampleCheck.SequenceEqual([OpenMark + RegistrationTitle], lines.Values);

        item.IsDone = true;

        SampleCheck.SequenceEqual([OpenMark + RegistrationTitle, DoneMark + RegistrationTitle], lines.Values);
    }

    /// <summary>Observes the title, the done flag and the priority of a to-do item.</summary>
    public static void ObserveTodoTitleStatusAndPriority()
    {
        var item = CreateRegistrationTask();

        using var seen = item.WhenAnyValue(x => x.Title, x => x.IsDone, x => x.Priority).Record();

        SampleCheck.Equal(new(RegistrationTitle, false, TodoPriority.Normal), seen.Latest);

        item.Priority = TodoPriority.High;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(TodoPriority.High, seen.Latest.Property3);
    }

    /// <summary>Works out how urgent a to-do item is from three properties; a finished item is never urgent.</summary>
    public static void ProjectTodoEffectivePriority()
    {
        var item = CreateRegistrationTask();

        using var priorities = item
            .WhenAnyValue(x => x.Title, x => x.IsDone, x => x.Priority, static (_, isDone, priority) => isDone ? TodoPriority.Low : priority)
            .Record();

        item.Priority = TodoPriority.High;
        item.IsDone = true;

        SampleCheck.SequenceEqual([TodoPriority.Normal, TodoPriority.High, TodoPriority.Low], priorities.Values);
    }

    /// <summary>Observes the title, notes, done flag and deadline of a to-do item.</summary>
    public static void ObserveTodoDeadline()
    {
        var item = CreateRegistrationTask();

        using var seen = item.WhenAnyValue(x => x.Title, x => x.Notes, x => x.IsDone, x => x.DueDate).Record();

        SampleCheck.Equal(new(RegistrationTitle, RegistrationNotes, false, _registrationDue), seen.Latest);

        item.DueDate = _movedDue;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(_movedDue, seen.Latest.Property4);
    }

    /// <summary>Builds a headline with the deadline from four properties.</summary>
    public static void ProjectTodoHeadline()
    {
        var item = CreateRegistrationTask();

        using var headlines = item
            .WhenAnyValue(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                static (title, _, isDone, due) => isDone || due is null ? title : $"{title} due {due.Value.ToString(IsoDay, CultureInfo.InvariantCulture)}")
            .Record();

        item.DueDate = _movedDue;

        SampleCheck.SequenceEqual([$"{RegistrationTitle} due 2026-03-20", $"{RegistrationTitle} due 2026-03-27"], headlines.Values);
    }

    /// <summary>Observes the title, notes, done flag, deadline and priority of a to-do item.</summary>
    public static void ObserveTodoWithPriority()
    {
        var item = CreateRegistrationTask();

        using var seen = item.WhenAnyValue(x => x.Title, x => x.Notes, x => x.IsDone, x => x.DueDate, x => x.Priority).Record();

        SampleCheck.Equal(new(RegistrationTitle, RegistrationNotes, false, _registrationDue, TodoPriority.Normal), seen.Latest);

        item.Priority = TodoPriority.High;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(TodoPriority.High, seen.Latest.Property5);
    }

    /// <summary>Decides from five properties whether a to-do item needs attention now.</summary>
    public static void ProjectTodoNeedsAttention()
    {
        var item = CreateRegistrationTask();

        using var flags = item
            .WhenAnyValue(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority,
                static (_, _, isDone, due, priority) => !isDone && due is not null && priority == TodoPriority.High)
            .Record();

        item.Priority = TodoPriority.High;

        SampleCheck.SequenceEqual([false, true], flags.Values);
    }

    /// <summary>Observes every editable property of a to-do item; six properties is the whole form.</summary>
    public static void ObserveWholeTodo()
    {
        var item = CreateRegistrationTask();

        using var seen = item.WhenAnyValue(x => x.Title, x => x.Notes, x => x.IsDone, x => x.DueDate, x => x.Priority, x => x.Tags).Record();

        SampleCheck.Equal(new(RegistrationTitle, RegistrationNotes, false, _registrationDue, TodoPriority.Normal, _registrationTags), seen.Latest);

        item.Tags = _urgentTags;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(_urgentTags, seen.Latest.Property6);
    }

    /// <summary>Lists the labels after the title from six properties.</summary>
    public static void ProjectTodoTitleWithLabels()
    {
        var item = CreateRegistrationTask();

        using var lines = item
            .WhenAnyValue(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority,
                x => x.Tags,
                static (title, _, _, _, _, tags) => $"{title} #{string.Join(" #", tags)}")
            .Record();

        item.Tags = _urgentTags;

        SampleCheck.SequenceEqual([$"{RegistrationTitle} #errands #car", $"{RegistrationTitle} #errands #car #urgent"], lines.Values);
    }

    /// <summary>Observes seven properties of a GitHub issue.</summary>
    public static void ObserveIssueSummary()
    {
        var issue = CreateCheckoutIssue();

        using var seen = issue.WhenAnyValue(x => x.Number, x => x.Title, x => x.State, x => x.Author, x => x.Assignee!.Login, x => x.Labels, x => x.UpdatedAt).Record();

        SampleCheck.Equal(new(CheckoutNumber, CheckoutTitle, IssueState.Open, _maria, PriyaLogin, _checkoutLabels, _checkoutUpdatedAt), seen.Latest);

        issue.Title = RenamedIssueTitle;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(RenamedIssueTitle, seen.Latest.Property2);
    }

    /// <summary>Names the number, title and assignee of an issue from seven properties.</summary>
    public static void ProjectIssueAssignment()
    {
        var issue = CreateCheckoutIssue();

        using var lines = issue
            .WhenAnyValue(
                x => x.Number,
                x => x.Title,
                x => x.State,
                x => x.Author,
                x => x.Assignee!.Login,
                x => x.Labels,
                x => x.UpdatedAt,
                static (number, title, _, _, assignee, _, _) => $"#{number} {title} -> {assignee}")
            .Record();

        issue.Assignee = _tomas;

        SampleCheck.SequenceEqual([$"#{CheckoutNumber} {CheckoutTitle} -> {PriyaLogin}", $"#{CheckoutNumber} {CheckoutTitle} -> {TomasLogin}"], lines.Values);
    }

    /// <summary>Observes eight properties of a GitHub issue.</summary>
    public static void ObserveIssueWithComments()
    {
        var issue = CreateCheckoutIssue();

        using var seen = issue
            .WhenAnyValue(x => x.Number, x => x.Title, x => x.State, x => x.Author, x => x.Assignee!.Login, x => x.Labels, x => x.UpdatedAt, x => x.Comments)
            .Record();

        SampleCheck.Equal(new(CheckoutNumber, CheckoutTitle, IssueState.Open, _maria, PriyaLogin, _checkoutLabels, _checkoutUpdatedAt, _noComments), seen.Latest);

        issue.State = IssueState.Closed;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(IssueState.Closed, seen.Latest.Property3);
    }

    /// <summary>Finds out from eight properties whether an open issue still waits for its first reply.</summary>
    public static void ProjectIssueAwaitsFirstReply()
    {
        var issue = CreateCheckoutIssue();

        using var flags = issue
            .WhenAnyValue(
                x => x.Number,
                x => x.Title,
                x => x.State,
                x => x.Author,
                x => x.Assignee!.Login,
                x => x.Labels,
                x => x.UpdatedAt,
                x => x.Comments,
                static (_, _, state, _, _, _, _, comments) => state == IssueState.Open && comments.Count == 0)
            .Record();

        issue.State = IssueState.Closed;

        SampleCheck.SequenceEqual([true, false], flags.Values);
    }

    /// <summary>Observes a transfer draft and the fields of its source account, nine properties in all.</summary>
    public static void ObserveTransferDraftAndSourceAccount()
    {
        var draft = CreateTransferDraft();

        using var seen = draft
            .WhenAnyValue(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Id,
                x => x.Source!.Name,
                x => x.Source!.Kind,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance)
            .Record();

        SampleCheck.Equal(
            new(RentAmount, RentReference, EverydayId, EverydayName, AccountKind.Everyday, Currency, EverydayBalance, EverydayOverdraft, EverydayAvailable),
            seen.Latest);

        draft.Reference = RentReferenceRevised;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(RentReferenceRevised, seen.Latest.Property2);
    }

    /// <summary>Checks from nine properties whether the account can cover the amount.</summary>
    public static void ProjectTransferIsAffordable()
    {
        var draft = CreateTransferDraft();

        using var flags = draft
            .WhenAnyValue(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Id,
                x => x.Source!.Name,
                x => x.Source!.Kind,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance,
                static (amount, _, _, _, _, _, _, _, available) => amount <= available)
            .Record();

        draft.Amount = AboveAvailable;

        SampleCheck.SequenceEqual([true, false], flags.Values);
    }

    /// <summary>Adds the payee to the transfer draft and its source account fields, ten properties in all; the payee is replaced as a whole.</summary>
    public static void ObserveTransferDraftWithPayee()
    {
        var draft = CreateTransferDraft();

        using var seen = draft
            .WhenAnyValue(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Id,
                x => x.Source!.Name,
                x => x.Source!.Kind,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance,
                x => x.Payee!)
            .Record();

        SampleCheck.Equal(
            new(RentAmount, RentReference, EverydayId, EverydayName, AccountKind.Everyday, Currency, EverydayBalance, EverydayOverdraft, EverydayAvailable, _landlord),
            seen.Latest);

        draft.Reference = RentReferenceRevised;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(RentReferenceRevised, seen.Latest.Property2);
    }

    /// <summary>Checks from ten properties whether the amount fits the account and the payee's limit.</summary>
    public static void ProjectTransferWithinPayeeLimit()
    {
        var draft = CreateTransferDraft();

        using var flags = draft
            .WhenAnyValue(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Id,
                x => x.Source!.Name,
                x => x.Source!.Kind,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance,
                x => x.Payee!,
                static (amount, _, _, _, _, _, _, _, available, payee) => amount <= available && (payee.TransferLimit is null || amount <= payee.TransferLimit))
            .Record();

        draft.Amount = AbovePayeeLimit;

        SampleCheck.SequenceEqual([true, false], flags.Values);
    }

    /// <summary>Adds the whole source account next to its fields, eleven properties in all.</summary>
    public static void ObserveTransferDraftWithSourceAccount()
    {
        var draft = CreateTransferDraft();

        using var seen = draft
            .WhenAnyValue(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Id,
                x => x.Source!.Name,
                x => x.Source!.Kind,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance,
                x => x.Payee!,
                x => x.Source!)
            .Record();

        SampleCheck.Equal(
            new(RentAmount, RentReference, EverydayId, EverydayName, AccountKind.Everyday, Currency, EverydayBalance, EverydayOverdraft, EverydayAvailable, _landlord, draft.Source!),
            seen.Latest);

        draft.Reference = RentReferenceRevised;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(RentReferenceRevised, seen.Latest.Property2);
    }

    /// <summary>Checks from eleven properties whether a transfer names a payee and the account can cover it.</summary>
    public static void ProjectTransferIsReady()
    {
        var draft = CreateTransferDraft();

        using var flags = draft
            .WhenAnyValue(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Id,
                x => x.Source!.Name,
                x => x.Source!.Kind,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance,
                x => x.Payee!,
                x => x.Source!,
                static (amount, _, _, _, _, _, _, _, available, payee, _) => payee.Name.Length > 0 && amount <= available)
            .Record();

        draft.Amount = AboveAvailable;

        SampleCheck.SequenceEqual([true, false], flags.Values);
    }

    /// <summary>Observes the session, the selected issue and the rate limit of the issue board, twelve properties in all.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ObserveIssueBoardRateLimit()
    {
        var board = await CreateBoardWithSelectedIssueAsync();
        var issue = board.SelectedIssue!;

        using var seen = board
            .WhenAnyValue(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.SelectedRepository!,
                x => x.SelectedIssue!.Author.Login,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining)
            .Record();

        SampleCheck.Equal(
            new(
                true,
                board.CurrentUser!.Login,
                board.SelectedRepository!,
                issue.Author.Login,
                CheckoutNumber,
                CheckoutTitle,
                IssueState.Open,
                issue.Assignee!.Login,
                issue.Labels,
                issue.UpdatedAt,
                string.Empty,
                board.RateLimitRemaining),
            seen.Latest);

        board.NewCommentText = CommentDraft;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(CommentDraft, seen.Latest.Property11);
    }

    /// <summary>Decides from twelve properties whether the board can accept a comment.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ProjectBoardAcceptsComments()
    {
        var board = await CreateBoardWithSelectedIssueAsync();

        using var flags = board
            .WhenAnyValue(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.SelectedRepository!,
                x => x.SelectedIssue!.Author.Login,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                static (signedIn, _, _, _, _, _, state, _, _, _, _, remaining) => signedIn && state == IssueState.Open && remaining > 0)
            .Record();

        board.SelectedIssue!.State = IssueState.Closed;

        SampleCheck.SequenceEqual([true, false], flags.Values);
    }

    /// <summary>Adds the last error message to the issue board properties, thirteen in all.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ObserveIssueBoardErrorMessage()
    {
        var board = await CreateBoardWithSelectedIssueAsync();
        var issue = board.SelectedIssue!;

        using var seen = board
            .WhenAnyValue(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.SelectedRepository!,
                x => x.SelectedIssue!.Author.Login,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage)
            .Record();

        SampleCheck.Equal(
            new(
                true,
                board.CurrentUser!.Login,
                board.SelectedRepository!,
                issue.Author.Login,
                CheckoutNumber,
                CheckoutTitle,
                IssueState.Open,
                issue.Assignee!.Login,
                issue.Labels,
                issue.UpdatedAt,
                string.Empty,
                board.RateLimitRemaining,
                string.Empty),
            seen.Latest);

        issue.Title = RenamedIssueTitle;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(RenamedIssueTitle, seen.Latest.Property6);
    }

    /// <summary>Decides from thirteen properties whether the board can accept a comment and has no error.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ProjectBoardAcceptsCommentsWithoutError()
    {
        var board = await CreateBoardWithSelectedIssueAsync();

        using var flags = board
            .WhenAnyValue(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.SelectedRepository!,
                x => x.SelectedIssue!.Author.Login,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage,
                static (signedIn, _, _, _, _, _, state, _, _, _, _, remaining, error) =>
                    signedIn && state == IssueState.Open && remaining > 0 && error.Length == 0)
            .Record();

        board.SelectedIssue!.State = IssueState.Closed;

        SampleCheck.SequenceEqual([true, false], flags.Values);
    }

    /// <summary>Adds the access token to the issue board properties, fourteen in all.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ObserveIssueBoardToken()
    {
        var board = await CreateBoardWithSelectedIssueAsync();
        var issue = board.SelectedIssue!;

        using var seen = board
            .WhenAnyValue(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.SelectedRepository!,
                x => x.SelectedIssue!.Author.Login,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage,
                x => x.Token)
            .Record();

        SampleCheck.Equal(
            new(
                true,
                board.CurrentUser!.Login,
                board.SelectedRepository!,
                issue.Author.Login,
                CheckoutNumber,
                CheckoutTitle,
                IssueState.Open,
                issue.Assignee!.Login,
                issue.Labels,
                issue.UpdatedAt,
                string.Empty,
                board.RateLimitRemaining,
                string.Empty,
                InMemoryGitHubServer.PriyaToken),
            seen.Latest);

        issue.Assignee = _tomas;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(TomasLogin, seen.Latest.Property8);
    }

    /// <summary>Decides from fourteen properties whether the board can accept a comment with a token in place.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ProjectBoardHasTokenAndAcceptsComments()
    {
        var board = await CreateBoardWithSelectedIssueAsync();

        using var flags = board
            .WhenAnyValue(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.SelectedRepository!,
                x => x.SelectedIssue!.Author.Login,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage,
                x => x.Token,
                static (signedIn, _, _, _, _, _, state, _, _, _, _, remaining, error, token) =>
                    signedIn && state == IssueState.Open && remaining > 0 && error.Length == 0 && token.Length > 0)
            .Record();

        board.SelectedIssue!.State = IssueState.Closed;

        SampleCheck.SequenceEqual([true, false], flags.Values);
    }

    /// <summary>Adds the open issues of the repository to the issue board properties, fifteen in all.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ObserveIssueBoardIssues()
    {
        var board = await CreateBoardWithSelectedIssueAsync();
        var issue = board.SelectedIssue!;

        using var seen = board
            .WhenAnyValue(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.SelectedRepository!,
                x => x.SelectedIssue!.Author.Login,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage,
                x => x.Token,
                x => x.Issues)
            .Record();

        SampleCheck.Equal(
            new(
                true,
                board.CurrentUser!.Login,
                board.SelectedRepository!,
                issue.Author.Login,
                CheckoutNumber,
                CheckoutTitle,
                IssueState.Open,
                issue.Assignee!.Login,
                issue.Labels,
                issue.UpdatedAt,
                string.Empty,
                board.RateLimitRemaining,
                string.Empty,
                InMemoryGitHubServer.PriyaToken,
                board.Issues),
            seen.Latest);

        board.NewCommentText = CommentDraft;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(CommentDraft, seen.Latest.Property11);
    }

    /// <summary>Decides from fifteen properties whether the board can accept a comment and lists issues.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ProjectBoardListsIssuesAndAcceptsComments()
    {
        var board = await CreateBoardWithSelectedIssueAsync();

        using var flags = board
            .WhenAnyValue(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.SelectedRepository!,
                x => x.SelectedIssue!.Author.Login,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage,
                x => x.Token,
                x => x.Issues,
                static (signedIn, _, _, _, _, _, state, _, _, _, _, remaining, error, token, issues) =>
                    signedIn && state == IssueState.Open && remaining > 0 && error.Length == 0 && token.Length > 0 && issues.Count > 0)
            .Record();

        board.SelectedIssue!.State = IssueState.Closed;

        SampleCheck.SequenceEqual([true, false], flags.Values);
    }

    /// <summary>Observes sixteen properties of the issue board, the most one call accepts.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ObserveIssueBoardRepositories()
    {
        var board = await CreateBoardWithSelectedIssueAsync();
        var issue = board.SelectedIssue!;

        using var seen = board
            .WhenAnyValue(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.SelectedRepository!,
                x => x.SelectedIssue!.Author.Login,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage,
                x => x.Token,
                x => x.Issues,
                x => x.Repositories)
            .Record();

        SampleCheck.Equal(
            new(
                true,
                board.CurrentUser!.Login,
                board.SelectedRepository!,
                issue.Author.Login,
                CheckoutNumber,
                CheckoutTitle,
                IssueState.Open,
                issue.Assignee!.Login,
                issue.Labels,
                issue.UpdatedAt,
                string.Empty,
                board.RateLimitRemaining,
                string.Empty,
                InMemoryGitHubServer.PriyaToken,
                board.Issues,
                board.Repositories),
            seen.Latest);

        board.NewCommentText = CommentDraft;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(CommentDraft, seen.Latest.Property11);
    }

    /// <summary>Decides from sixteen properties whether the board can accept a comment, has repositories and lists issues.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ProjectBoardIsReadyForComments()
    {
        var board = await CreateBoardWithSelectedIssueAsync();

        using var flags = board
            .WhenAnyValue(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.SelectedRepository!,
                x => x.SelectedIssue!.Author.Login,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage,
                x => x.Token,
                x => x.Issues,
                x => x.Repositories,
                static (signedIn, _, _, _, _, _, state, _, _, _, _, remaining, error, token, issues, repositories) =>
                    signedIn && state == IssueState.Open && remaining > 0 && error.Length == 0 && token.Length > 0 && issues.Count > 0 && repositories.Count > 0)
            .Record();

        board.SelectedIssue!.State = IssueState.Closed;

        SampleCheck.SequenceEqual([true, false], flags.Values);
    }

    /// <summary>Creates the to-do item the examples observe.</summary>
    /// <returns>An open, unfinished item.</returns>
    private static TodoItem CreateRegistrationTask() => new()
    {
        Id = RegistrationId,
        Title = RegistrationTitle,
        Notes = RegistrationNotes,
        DueDate = _registrationDue,
        Priority = TodoPriority.Normal,
        Tags = _registrationTags,
    };

    /// <summary>Creates the issue the examples observe.</summary>
    /// <returns>An open issue reported by Maria and assigned to Priya.</returns>
    private static Issue CreateCheckoutIssue() => new()
    {
        Number = CheckoutNumber,
        Title = CheckoutTitle,
        State = IssueState.Open,
        Author = _maria,
        Assignee = _priya,
        Labels = _checkoutLabels,
        Comments = _noComments,
        UpdatedAt = _checkoutUpdatedAt,
    };

    /// <summary>Creates the transfer the examples observe.</summary>
    /// <returns>A draft that sends the rent from the everyday account to the landlord.</returns>
    private static TransferDraft CreateTransferDraft() =>
        new() { Source = CreateEverydayAccount(), Payee = _landlord, Amount = RentAmount, Reference = RentReference };

    /// <summary>Creates the account the transfer draws on.</summary>
    /// <returns>An everyday account with an overdraft.</returns>
    private static Account CreateEverydayAccount() => new()
    {
        Id = EverydayId,
        Name = EverydayName,
        Kind = AccountKind.Everyday,
        Currency = Currency,
        Balance = EverydayBalance,
        OverdraftLimit = EverydayOverdraft,
    };

    /// <summary>Signs in to a seeded server, loads the open issues of the webshop repository and selects the checkout issue.</summary>
    /// <returns>A task that returns the board once the issue is selected.</returns>
    private static async Task<IssueBoardViewModel> CreateBoardWithSelectedIssueAsync()
    {
        var server = InMemoryGitHubServer.CreateSeeded(ManualClock.StartOfWorkingDay());
        IssueBoardViewModel board = new(server) { Token = InMemoryGitHubServer.PriyaToken };

        board.SignInCommand.Execute(null);
        await board.SignInCommand.Completion;

        board.SelectedRepository = board.Repositories[0];
        board.LoadIssuesCommand.Execute(null);
        await board.LoadIssuesCommand.Completion;

        board.SelectedIssue = board.Issues[1];
        return board;
    }
}

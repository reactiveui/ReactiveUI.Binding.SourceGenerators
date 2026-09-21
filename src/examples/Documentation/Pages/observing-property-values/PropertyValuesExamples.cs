// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.ObservingPropertyValues;

/// <summary>
/// Shows <c>PropertyValues</c>, the value <c>WhenChanged</c> and <c>WhenChanging</c> deliver when you observe two to
/// sixteen properties without a value selector. It is a read-only record struct with one member per observed property
/// (<c>Property1</c> to <c>Property16</c>), a constructor and a deconstructor that take the values in the same order,
/// and value equality.
/// </summary>
public static class PropertyValuesExamples
{
    /// <summary>The number of values delivered by the subscription and one change.</summary>
    private const int AfterOneChange = 2;

    /// <summary>The identifier of the registration task.</summary>
    private const int RegistrationId = 7;

    /// <summary>The title of the registration task.</summary>
    private const string RegistrationTitle = "Renew car registration";

    /// <summary>The notes of the registration task.</summary>
    private const string RegistrationNotes = "Bring the insurance certificate";

    /// <summary>The label of an errand.</summary>
    private const string ErrandsTag = "errands";

    /// <summary>The label of a car task.</summary>
    private const string CarTag = "car";

    /// <summary>The label of an urgent task.</summary>
    private const string UrgentTag = "urgent";

    /// <summary>The number of the checkout issue.</summary>
    private const int CheckoutNumber = 101;

    /// <summary>The title of the checkout issue.</summary>
    private const string CheckoutTitle = "Checkout button unresponsive on Safari";

    /// <summary>The title the checkout issue is renamed to.</summary>
    private const string RenamedIssueTitle = "Checkout button unresponsive on Safari 17";

    /// <summary>The account name of the person the issue is assigned to.</summary>
    private const string PriyaLogin = "priya-nair";

    /// <summary>The display name of the person the issue is assigned to.</summary>
    private const string PriyaName = "Priya Nair";

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
    private static readonly User _priya = new() { Login = PriyaLogin, DisplayName = PriyaName };

    /// <summary>The person who reported the issue.</summary>
    private static readonly User _maria = new() { Login = "maria-santos", DisplayName = "Maria Santos" };

    /// <summary>The payee of the transfer.</summary>
    private static readonly Payee _landlord = new(PayeeId, "Harbour Realty", "062-000 1234 5678", PayeeLimit);

    /// <summary>Handles each emission in a subscription that reads the members it needs.</summary>
    public static void SubscribeAndReadTodoValues()
    {
        var item = CreateRegistrationTask();
        List<string> lines = [];

        using (item
            .WhenChanged(x => x.Title, x => x.IsDone, x => x.Priority)
            .Subscribe(values => lines.Add($"{values.Property1} ({values.Property3}, done: {values.Property2})")))
        {
            item.Priority = TodoPriority.High;
        }

        SampleCheck.SequenceEqual([$"{RegistrationTitle} (Normal, done: False)", $"{RegistrationTitle} (High, done: False)"], lines);
    }

    /// <summary>Compares emissions of two items; equal members make equal values.</summary>
    public static void CompareTodoValues()
    {
        var first = CreateRegistrationTask();
        var second = CreateRegistrationTask();

        using var firstSeen = first.WhenChanged(x => x.Title, x => x.IsDone).Record();
        using var secondSeen = second.WhenChanged(x => x.Title, x => x.IsDone).Record();

        var left = firstSeen.Latest;
        var right = secondSeen.Latest;
        var boxed = (object)right;

        SampleCheck.Equal(true, left == right);
        SampleCheck.Equal(false, left != right);
        SampleCheck.Equal(true, left.Equals(right));
        SampleCheck.Equal(true, left.Equals(boxed));
        SampleCheck.Equal(left.GetHashCode(), right.GetHashCode());

        second.IsDone = true;

        SampleCheck.Equal(false, left == secondSeen.Latest);
        SampleCheck.Equal(true, left != secondSeen.Latest);
        SampleCheck.Equal(false, left.Equals(secondSeen.Latest));
    }

    /// <summary>Copies an emission with one member replaced; the original keeps its values.</summary>
    public static void ReplaceOneTodoValue()
    {
        var item = CreateRegistrationTask();

        using var seen = item.WhenChanged(x => x.Title, x => x.IsDone).Record();

        var original = seen.Latest;
        var finished = original with { Property2 = true };

        SampleCheck.Equal(new(RegistrationTitle, true), finished);
        SampleCheck.Equal(new(RegistrationTitle, false), original);
    }

    /// <summary>Prints an emission; the text lists each member by name.</summary>
    public static void PrintTodoValues()
    {
        var item = CreateRegistrationTask();

        using var seen = item.WhenChanged(x => x.Title, x => x.IsDone).Record();

        const string Printed = $"PropertyValues {{ Property1 = {RegistrationTitle}, Property2 = False }}";

        SampleCheck.Equal(Printed, seen.Latest.ToString());
    }

    /// <summary>Reads the title and done flag of a to-do item from a two-value emission.</summary>
    public static void ReadTwoTodoValues()
    {
        var item = CreateRegistrationTask();

        using var seen = item.WhenChanged(x => x.Title, x => x.IsDone).Record();

        var values = seen.Latest;

        SampleCheck.Equal(RegistrationTitle, values.Property1);
        SampleCheck.Equal(false, values.Property2);

        var (title, isDone) = values;

        SampleCheck.Equal(values, new(title, isDone));

        item.IsDone = true;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(true, seen.Latest.Property2);
        SampleCheck.Equal(true, seen.Latest != values);
    }

    /// <summary>Reads the title, done flag and priority of a to-do item from a three-value emission.</summary>
    public static void ReadThreeTodoValues()
    {
        var item = CreateRegistrationTask();

        using var seen = item.WhenChanged(x => x.Title, x => x.IsDone, x => x.Priority).Record();

        var values = seen.Latest;

        SampleCheck.Equal(RegistrationTitle, values.Property1);
        SampleCheck.Equal(false, values.Property2);
        SampleCheck.Equal(TodoPriority.Normal, values.Property3);

        var (title, isDone, priority) = values;

        SampleCheck.Equal(values, new(title, isDone, priority));

        item.Priority = TodoPriority.High;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(TodoPriority.High, seen.Latest.Property3);
        SampleCheck.Equal(true, seen.Latest != values);
    }

    /// <summary>Reads the title, notes, done flag and deadline of a to-do item from a four-value emission.</summary>
    public static void ReadFourTodoValues()
    {
        var item = CreateRegistrationTask();

        using var seen = item
            .WhenChanged(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate)
            .Record();

        var values = seen.Latest;

        SampleCheck.Equal(RegistrationTitle, values.Property1);
        SampleCheck.Equal(RegistrationNotes, values.Property2);
        SampleCheck.Equal(false, values.Property3);
        SampleCheck.Equal(_registrationDue, values.Property4);

        var (title, notes, isDone, dueDate) = values;

        SampleCheck.Equal(values, new(title, notes, isDone, dueDate));

        item.DueDate = _movedDue;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(_movedDue, seen.Latest.Property4);
        SampleCheck.Equal(true, seen.Latest != values);
    }

    /// <summary>Reads the title, notes, done flag, deadline and priority of a to-do item from a five-value emission.</summary>
    public static void ReadFiveTodoValues()
    {
        var item = CreateRegistrationTask();

        using var seen = item
            .WhenChanged(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority)
            .Record();

        var values = seen.Latest;

        SampleCheck.Equal(RegistrationTitle, values.Property1);
        SampleCheck.Equal(RegistrationNotes, values.Property2);
        SampleCheck.Equal(false, values.Property3);
        SampleCheck.Equal(_registrationDue, values.Property4);
        SampleCheck.Equal(TodoPriority.Normal, values.Property5);

        var (title, notes, isDone, dueDate, priority) = values;

        SampleCheck.Equal(values, new(title, notes, isDone, dueDate, priority));

        item.Priority = TodoPriority.High;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(TodoPriority.High, seen.Latest.Property5);
        SampleCheck.Equal(true, seen.Latest != values);
    }

    /// <summary>Reads every editable property of a to-do item from a six-value emission.</summary>
    public static void ReadSixTodoValues()
    {
        var item = CreateRegistrationTask();

        using var seen = item
            .WhenChanged(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority,
                x => x.Tags)
            .Record();

        var values = seen.Latest;

        SampleCheck.Equal(RegistrationTitle, values.Property1);
        SampleCheck.Equal(RegistrationNotes, values.Property2);
        SampleCheck.Equal(false, values.Property3);
        SampleCheck.Equal(_registrationDue, values.Property4);
        SampleCheck.Equal(TodoPriority.Normal, values.Property5);
        SampleCheck.Equal(_registrationTags, values.Property6);

        var (title, notes, isDone, dueDate, priority, tags) = values;

        SampleCheck.Equal(values, new(title, notes, isDone, dueDate, priority, tags));

        item.Tags = _urgentTags;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(_urgentTags, seen.Latest.Property6);
        SampleCheck.Equal(true, seen.Latest != values);
    }

    /// <summary>Reads seven properties of an issue from a seven-value emission.</summary>
    public static void ReadSevenIssueValues()
    {
        var issue = CreateCheckoutIssue();

        using var seen = issue
            .WhenChanged(
                x => x.Number,
                x => x.Title,
                x => x.State,
                x => x.Author,
                x => x.Assignee!,
                x => x.Labels,
                x => x.UpdatedAt)
            .Record();

        var values = seen.Latest;

        SampleCheck.Equal(CheckoutNumber, values.Property1);
        SampleCheck.Equal(CheckoutTitle, values.Property2);
        SampleCheck.Equal(IssueState.Open, values.Property3);
        SampleCheck.Equal(_maria, values.Property4);
        SampleCheck.Equal(_priya, values.Property5);
        SampleCheck.Equal(_checkoutLabels, values.Property6);
        SampleCheck.Equal(_checkoutUpdatedAt, values.Property7);

        var (number, title, state, author, assignee, labels, updatedAt) = values;

        SampleCheck.Equal(values, new(number, title, state, author, assignee, labels, updatedAt));

        issue.Title = RenamedIssueTitle;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(RenamedIssueTitle, seen.Latest.Property2);
        SampleCheck.Equal(true, seen.Latest != values);
    }

    /// <summary>Reads eight properties of an issue from an eight-value emission.</summary>
    public static void ReadEightIssueValues()
    {
        var issue = CreateCheckoutIssue();

        using var seen = issue
            .WhenChanged(
                x => x.Number,
                x => x.Title,
                x => x.State,
                x => x.Author,
                x => x.Assignee!,
                x => x.Labels,
                x => x.UpdatedAt,
                x => x.Comments)
            .Record();

        var values = seen.Latest;

        SampleCheck.Equal(CheckoutNumber, values.Property1);
        SampleCheck.Equal(CheckoutTitle, values.Property2);
        SampleCheck.Equal(IssueState.Open, values.Property3);
        SampleCheck.Equal(_maria, values.Property4);
        SampleCheck.Equal(_priya, values.Property5);
        SampleCheck.Equal(_checkoutLabels, values.Property6);
        SampleCheck.Equal(_checkoutUpdatedAt, values.Property7);
        SampleCheck.Equal(_noComments, values.Property8);

        var (number, title, state, author, assignee, labels, updatedAt, comments) = values;

        SampleCheck.Equal(values, new(number, title, state, author, assignee, labels, updatedAt, comments));

        issue.State = IssueState.Closed;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(IssueState.Closed, seen.Latest.Property3);
        SampleCheck.Equal(true, seen.Latest != values);
    }

    /// <summary>Reads a transfer draft and the fields of its source account from a nine-value emission.</summary>
    public static void ReadNineTransferValues()
    {
        var draft = CreateTransferDraft();

        using var seen = draft
            .WhenChanged(
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

        var values = seen.Latest;

        SampleCheck.Equal(RentAmount, values.Property1);
        SampleCheck.Equal(RentReference, values.Property2);
        SampleCheck.Equal(EverydayId, values.Property3);
        SampleCheck.Equal(EverydayName, values.Property4);
        SampleCheck.Equal(AccountKind.Everyday, values.Property5);
        SampleCheck.Equal(Currency, values.Property6);
        SampleCheck.Equal(EverydayBalance, values.Property7);
        SampleCheck.Equal(EverydayOverdraft, values.Property8);
        SampleCheck.Equal(EverydayAvailable, values.Property9);

        var (amount, reference, sourceId, sourceName, sourceKind, currency, balance, overdraftLimit, availableBalance) = values;

        SampleCheck.Equal(values, new(amount, reference, sourceId, sourceName, sourceKind, currency, balance, overdraftLimit, availableBalance));

        draft.Reference = RentReferenceRevised;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(RentReferenceRevised, seen.Latest.Property2);
        SampleCheck.Equal(true, seen.Latest != values);
    }

    /// <summary>Reads the payee of a transfer draft besides its source account fields from a ten-value emission.</summary>
    public static void ReadTenTransferValues()
    {
        var draft = CreateTransferDraft();

        using var seen = draft
            .WhenChanged(
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

        var values = seen.Latest;

        SampleCheck.Equal(RentAmount, values.Property1);
        SampleCheck.Equal(RentReference, values.Property2);
        SampleCheck.Equal(EverydayId, values.Property3);
        SampleCheck.Equal(EverydayName, values.Property4);
        SampleCheck.Equal(AccountKind.Everyday, values.Property5);
        SampleCheck.Equal(Currency, values.Property6);
        SampleCheck.Equal(EverydayBalance, values.Property7);
        SampleCheck.Equal(EverydayOverdraft, values.Property8);
        SampleCheck.Equal(EverydayAvailable, values.Property9);
        SampleCheck.Equal(_landlord, values.Property10);

        var (amount, reference, sourceId, sourceName, sourceKind, currency, balance, overdraftLimit, availableBalance, payee) = values;

        SampleCheck.Equal(values, new(amount, reference, sourceId, sourceName, sourceKind, currency, balance, overdraftLimit, availableBalance, payee));

        draft.Reference = RentReferenceRevised;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(RentReferenceRevised, seen.Latest.Property2);
        SampleCheck.Equal(true, seen.Latest != values);
    }

    /// <summary>Reads the source account itself besides its fields from an eleven-value emission.</summary>
    public static void ReadElevenTransferValues()
    {
        var draft = CreateTransferDraft();

        using var seen = draft
            .WhenChanged(
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

        var values = seen.Latest;

        SampleCheck.Equal(RentAmount, values.Property1);
        SampleCheck.Equal(RentReference, values.Property2);
        SampleCheck.Equal(EverydayId, values.Property3);
        SampleCheck.Equal(EverydayName, values.Property4);
        SampleCheck.Equal(AccountKind.Everyday, values.Property5);
        SampleCheck.Equal(Currency, values.Property6);
        SampleCheck.Equal(EverydayBalance, values.Property7);
        SampleCheck.Equal(EverydayOverdraft, values.Property8);
        SampleCheck.Equal(EverydayAvailable, values.Property9);
        SampleCheck.Equal(_landlord, values.Property10);
        SampleCheck.Equal(draft.Source!, values.Property11);

        var (amount, reference, sourceId, sourceName, sourceKind, currency, balance, overdraftLimit, availableBalance, payee, source) = values;

        SampleCheck.Equal(values, new(amount, reference, sourceId, sourceName, sourceKind, currency, balance, overdraftLimit, availableBalance, payee, source));

        draft.Reference = RentReferenceRevised;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(RentReferenceRevised, seen.Latest.Property2);
        SampleCheck.Equal(true, seen.Latest != values);
    }

    /// <summary>Reads the session, selected issue and rate limit of the issue board from a twelve-value emission.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReadTwelveIssueBoardValues()
    {
        var board = await CreateBoardWithSelectedIssueAsync();
        var issue = board.SelectedIssue!;

        using var seen = board
            .WhenChanged(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.CurrentUser!.DisplayName,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Assignee!.DisplayName,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining)
            .Record();

        var values = seen.Latest;

        SampleCheck.Equal(true, values.Property1);
        SampleCheck.Equal(PriyaLogin, values.Property2);
        SampleCheck.Equal(PriyaName, values.Property3);
        SampleCheck.Equal(CheckoutNumber, values.Property4);
        SampleCheck.Equal(CheckoutTitle, values.Property5);
        SampleCheck.Equal(IssueState.Open, values.Property6);
        SampleCheck.Equal(PriyaLogin, values.Property7);
        SampleCheck.Equal(PriyaName, values.Property8);
        SampleCheck.Equal(issue.Labels, values.Property9);
        SampleCheck.Equal(issue.UpdatedAt, values.Property10);
        SampleCheck.Equal(string.Empty, values.Property11);
        SampleCheck.Equal(board.RateLimitRemaining, values.Property12);

        var (signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining) = values;

        SampleCheck.Equal(values, new(signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining));

        board.NewCommentText = CommentDraft;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(CommentDraft, seen.Latest.Property11);
        SampleCheck.Equal(true, seen.Latest != values);
    }

    /// <summary>Reads the last error message of the issue board from a thirteen-value emission.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReadThirteenIssueBoardValues()
    {
        var board = await CreateBoardWithSelectedIssueAsync();
        var issue = board.SelectedIssue!;

        using var seen = board
            .WhenChanged(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.CurrentUser!.DisplayName,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Assignee!.DisplayName,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage)
            .Record();

        var values = seen.Latest;

        SampleCheck.Equal(true, values.Property1);
        SampleCheck.Equal(PriyaLogin, values.Property2);
        SampleCheck.Equal(PriyaName, values.Property3);
        SampleCheck.Equal(CheckoutNumber, values.Property4);
        SampleCheck.Equal(CheckoutTitle, values.Property5);
        SampleCheck.Equal(IssueState.Open, values.Property6);
        SampleCheck.Equal(PriyaLogin, values.Property7);
        SampleCheck.Equal(PriyaName, values.Property8);
        SampleCheck.Equal(issue.Labels, values.Property9);
        SampleCheck.Equal(issue.UpdatedAt, values.Property10);
        SampleCheck.Equal(string.Empty, values.Property11);
        SampleCheck.Equal(board.RateLimitRemaining, values.Property12);
        SampleCheck.Equal(string.Empty, values.Property13);

        var (signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error) = values;

        SampleCheck.Equal(values, new(signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error));

        board.NewCommentText = CommentDraft;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(CommentDraft, seen.Latest.Property11);
        SampleCheck.Equal(true, seen.Latest != values);
    }

    /// <summary>Reads the access token of the issue board from a fourteen-value emission.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReadFourteenIssueBoardValues()
    {
        var board = await CreateBoardWithSelectedIssueAsync();
        var issue = board.SelectedIssue!;

        using var seen = board
            .WhenChanged(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.CurrentUser!.DisplayName,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Assignee!.DisplayName,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage,
                x => x.Token)
            .Record();

        var values = seen.Latest;

        SampleCheck.Equal(true, values.Property1);
        SampleCheck.Equal(PriyaLogin, values.Property2);
        SampleCheck.Equal(PriyaName, values.Property3);
        SampleCheck.Equal(CheckoutNumber, values.Property4);
        SampleCheck.Equal(CheckoutTitle, values.Property5);
        SampleCheck.Equal(IssueState.Open, values.Property6);
        SampleCheck.Equal(PriyaLogin, values.Property7);
        SampleCheck.Equal(PriyaName, values.Property8);
        SampleCheck.Equal(issue.Labels, values.Property9);
        SampleCheck.Equal(issue.UpdatedAt, values.Property10);
        SampleCheck.Equal(string.Empty, values.Property11);
        SampleCheck.Equal(board.RateLimitRemaining, values.Property12);
        SampleCheck.Equal(string.Empty, values.Property13);
        SampleCheck.Equal(InMemoryGitHubServer.PriyaToken, values.Property14);

        var (signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error, token) = values;

        SampleCheck.Equal(values, new(signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error, token));

        board.NewCommentText = CommentDraft;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(CommentDraft, seen.Latest.Property11);
        SampleCheck.Equal(true, seen.Latest != values);
    }

    /// <summary>Reads the open issues of the issue board from a fifteen-value emission.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReadFifteenIssueBoardValues()
    {
        var board = await CreateBoardWithSelectedIssueAsync();
        var issue = board.SelectedIssue!;

        using var seen = board
            .WhenChanged(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.CurrentUser!.DisplayName,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Assignee!.DisplayName,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage,
                x => x.Token,
                x => x.Issues)
            .Record();

        var values = seen.Latest;

        SampleCheck.Equal(true, values.Property1);
        SampleCheck.Equal(PriyaLogin, values.Property2);
        SampleCheck.Equal(PriyaName, values.Property3);
        SampleCheck.Equal(CheckoutNumber, values.Property4);
        SampleCheck.Equal(CheckoutTitle, values.Property5);
        SampleCheck.Equal(IssueState.Open, values.Property6);
        SampleCheck.Equal(PriyaLogin, values.Property7);
        SampleCheck.Equal(PriyaName, values.Property8);
        SampleCheck.Equal(issue.Labels, values.Property9);
        SampleCheck.Equal(issue.UpdatedAt, values.Property10);
        SampleCheck.Equal(string.Empty, values.Property11);
        SampleCheck.Equal(board.RateLimitRemaining, values.Property12);
        SampleCheck.Equal(string.Empty, values.Property13);
        SampleCheck.Equal(InMemoryGitHubServer.PriyaToken, values.Property14);
        SampleCheck.Equal(board.Issues, values.Property15);

        var (signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error, token, issues) = values;

        SampleCheck.Equal(values, new(signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error, token, issues));

        board.NewCommentText = CommentDraft;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(CommentDraft, seen.Latest.Property11);
        SampleCheck.Equal(true, seen.Latest != values);
    }

    /// <summary>Reads the repositories of the issue board from a sixteen-value emission, the most one emission holds.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReadSixteenIssueBoardValues()
    {
        var board = await CreateBoardWithSelectedIssueAsync();
        var issue = board.SelectedIssue!;

        using var seen = board
            .WhenChanged(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.CurrentUser!.DisplayName,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Assignee!.DisplayName,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage,
                x => x.Token,
                x => x.Issues,
                x => x.Repositories)
            .Record();

        var values = seen.Latest;

        SampleCheck.Equal(true, values.Property1);
        SampleCheck.Equal(PriyaLogin, values.Property2);
        SampleCheck.Equal(PriyaName, values.Property3);
        SampleCheck.Equal(CheckoutNumber, values.Property4);
        SampleCheck.Equal(CheckoutTitle, values.Property5);
        SampleCheck.Equal(IssueState.Open, values.Property6);
        SampleCheck.Equal(PriyaLogin, values.Property7);
        SampleCheck.Equal(PriyaName, values.Property8);
        SampleCheck.Equal(issue.Labels, values.Property9);
        SampleCheck.Equal(issue.UpdatedAt, values.Property10);
        SampleCheck.Equal(string.Empty, values.Property11);
        SampleCheck.Equal(board.RateLimitRemaining, values.Property12);
        SampleCheck.Equal(string.Empty, values.Property13);
        SampleCheck.Equal(InMemoryGitHubServer.PriyaToken, values.Property14);
        SampleCheck.Equal(board.Issues, values.Property15);
        SampleCheck.Equal(board.Repositories, values.Property16);

        var (signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error, token, issues, repos) = values;

        SampleCheck.Equal(values, new(signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error, token, issues, repos));

        board.NewCommentText = CommentDraft;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(CommentDraft, seen.Latest.Property11);
        SampleCheck.Equal(true, seen.Latest != values);
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

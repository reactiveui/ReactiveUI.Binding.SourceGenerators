// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.ObservingWhenChanging;

/// <summary>
/// Shows every <c>WhenChanging</c> overload, from one observed property to sixteen, each with and without a value
/// selector. <c>WhenChanging</c> delivers the current values when you subscribe. It delivers them again just before each
/// change, so the values are the ones the change is about to replace.
/// </summary>
public static class WhenChangingExamples
{
    /// <summary>The number of values delivered by the subscription and one change.</summary>
    private const int AfterOneChange = 2;

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

    /// <summary>The status text of a finished task.</summary>
    private const string DoneText = "done";

    /// <summary>The status text of an open task.</summary>
    private const string OpenText = "open";

    /// <summary>The format of a day in a headline.</summary>
    private const string IsoDay = "yyyy-MM-dd";

    /// <summary>The number of the checkout issue.</summary>
    private const int CheckoutNumber = 101;

    /// <summary>The title of the checkout issue.</summary>
    private const string CheckoutTitle = "Checkout button unresponsive on Safari";

    /// <summary>The account name of the person the issue is assigned to.</summary>
    private const string PriyaLogin = "priya-nair";

    /// <summary>The display name of the person the issue is assigned to.</summary>
    private const string PriyaName = "Priya Nair";

    /// <summary>The account name of the person the issue is reassigned to.</summary>
    private const string TomasLogin = "tomas-berg";

    /// <summary>The text that stands for nobody working on an issue.</summary>
    private const string Unassigned = "unassigned";

    /// <summary>The label of a defect.</summary>
    private const string BugLabel = "bug";

    /// <summary>The label of the checkout area.</summary>
    private const string CheckoutLabel = "checkout";

    /// <summary>The first reply on the checkout issue.</summary>
    private const string FirstReplyText = "Reproduced on Safari 17.";

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

    /// <summary>The identifier of the payee the customer switches to.</summary>
    private const int OtherPayeeId = 4;

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

    /// <summary>The private note the customer starts with.</summary>
    private const string MemoText = "First payment under the new lease";

    /// <summary>The private note the customer changes to.</summary>
    private const string MemoRevised = "Second payment under the new lease";

    /// <summary>What the transfer pays for.</summary>
    private const string PurposeText = "Housing";

    /// <summary>What the transfer pays for once the customer changes it.</summary>
    private const string PurposeRevised = "Rent";

    /// <summary>The fee the bank charges for the transfer.</summary>
    private const decimal TransferFee = 2.50M;

    /// <summary>The text of a transfer that repeats.</summary>
    private const string MonthlyText = "monthly";

    /// <summary>The text of a transfer that is sent once.</summary>
    private const string OnceText = "once";

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

    /// <summary>When the checkout issue last changed once it has a reply.</summary>
    private static readonly DateTimeOffset _repliedAt = SeedData.Instant("2026-03-03T09:00:00Z");

    /// <summary>The comments of the checkout issue once it has a reply.</summary>
    private static readonly IReadOnlyList<IssueComment> _firstReply = [new(PriyaLogin, FirstReplyText, _repliedAt)];

    /// <summary>The person the issue is assigned to.</summary>
    private static readonly User _priya = new() { Login = PriyaLogin, DisplayName = PriyaName };

    /// <summary>The person who reported the issue.</summary>
    private static readonly User _maria = new() { Login = "maria-santos", DisplayName = "Maria Santos" };

    /// <summary>The person the issue is reassigned to.</summary>
    private static readonly User _tomas = new() { Login = TomasLogin, DisplayName = "Tomas Berg" };

    /// <summary>The payee of the transfer.</summary>
    private static readonly Payee _landlord = new(PayeeId, "Harbour Realty", "062-000 1234 5678", PayeeLimit);

    /// <summary>The payee the customer switches to.</summary>
    private static readonly Payee _strata = new(OtherPayeeId, "Bayside Strata", "062-000 8765 4321", null);

    /// <summary>The day the transfer is sent.</summary>
    private static readonly DateOnly _scheduledFor = SeedData.Date("2026-04-01");

    /// <summary>The day the transfer is moved to.</summary>
    private static readonly DateOnly _rescheduledFor = SeedData.Date("2026-04-08");

    /// <summary>Observes the title of a to-do item; a single property delivers the value itself.</summary>
    public static void ObserveTodoTitle()
    {
        var item = CreateRegistrationTask();

        using var titles = item.WhenChanging(x => x.Title).Record();

        SampleCheck.SequenceEqual([RegistrationTitle], titles.Values);

        item.Title = RenamedTitle;

        SampleCheck.SequenceEqual([RegistrationTitle, RegistrationTitle], titles.Values);
        SampleCheck.Equal(RenamedTitle, item.Title);
    }

    /// <summary>Observes the title and the done flag of a to-do item together.</summary>
    public static void ObserveTodoTitleAndStatus()
    {
        var item = CreateRegistrationTask();

        using var seen = item.WhenChanging(x => x.Title, x => x.IsDone).Record();

        SampleCheck.Equal(new(RegistrationTitle, false), seen.Latest);

        item.IsDone = true;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(false, seen.Latest.Property2);
        SampleCheck.Equal(true, item.IsDone);
    }

    /// <summary>Records the status line a to-do item had before its done flag changed, with a value selector.</summary>
    public static void ProjectTodoStatusLine()
    {
        var item = CreateRegistrationTask();

        using var lines = item.WhenChanging(x => x.Title, x => x.IsDone, static (title, isDone) => $"{title} ({(isDone ? DoneText : OpenText)})").Record();

        item.IsDone = true;

        const string OpenLine = $"{RegistrationTitle} ({OpenText})";

        SampleCheck.SequenceEqual([OpenLine, OpenLine], lines.Values);
    }

    /// <summary>Observes the title, the done flag and the priority of a to-do item.</summary>
    public static void ObserveTodoTitleStatusAndPriority()
    {
        var item = CreateRegistrationTask();

        using var seen = item.WhenChanging(x => x.Title, x => x.IsDone, x => x.Priority).Record();

        SampleCheck.Equal(new(RegistrationTitle, false, TodoPriority.Normal), seen.Latest);

        item.Priority = TodoPriority.High;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(TodoPriority.Normal, seen.Latest.Property3);
        SampleCheck.Equal(TodoPriority.High, item.Priority);
    }

    /// <summary>Records how urgent a to-do item was before its priority changed; a finished item is never urgent.</summary>
    public static void ProjectTodoEffectivePriority()
    {
        var item = CreateRegistrationTask();

        using var priorities = item
            .WhenChanging(x => x.Title, x => x.IsDone, x => x.Priority, static (_, isDone, priority) => isDone ? TodoPriority.Low : priority)
            .Record();

        item.Priority = TodoPriority.High;

        SampleCheck.SequenceEqual([TodoPriority.Normal, TodoPriority.Normal], priorities.Values);
    }

    /// <summary>Observes the title, notes, done flag and deadline of a to-do item.</summary>
    public static void ObserveTodoDeadline()
    {
        var item = CreateRegistrationTask();

        using var seen = item.WhenChanging(x => x.Title, x => x.Notes, x => x.IsDone, x => x.DueDate).Record();

        SampleCheck.Equal(new(RegistrationTitle, RegistrationNotes, false, _registrationDue), seen.Latest);

        item.DueDate = _movedDue;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(_registrationDue, seen.Latest.Property4);
        SampleCheck.Equal(_movedDue, item.DueDate);
    }

    /// <summary>Records the headline a to-do item had before its deadline moved.</summary>
    public static void ProjectTodoHeadline()
    {
        var item = CreateRegistrationTask();

        using var headlines = item
            .WhenChanging(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                static (title, _, isDone, due) => isDone || due is null ? title : $"{title} due {due.Value.ToString(IsoDay, CultureInfo.InvariantCulture)}")
            .Record();

        item.DueDate = _movedDue;

        const string Headline = $"{RegistrationTitle} due 2026-03-20";

        SampleCheck.SequenceEqual([Headline, Headline], headlines.Values);
    }

    /// <summary>Observes the title, notes, done flag, deadline and priority of a to-do item.</summary>
    public static void ObserveTodoWithPriority()
    {
        var item = CreateRegistrationTask();

        using var seen = item.WhenChanging(x => x.Title, x => x.Notes, x => x.IsDone, x => x.DueDate, x => x.Priority).Record();

        SampleCheck.Equal(new(RegistrationTitle, RegistrationNotes, false, _registrationDue, TodoPriority.Normal), seen.Latest);

        item.Priority = TodoPriority.High;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(TodoPriority.Normal, seen.Latest.Property5);
        SampleCheck.Equal(TodoPriority.High, item.Priority);
    }

    /// <summary>Records whether a high-priority item needed attention before it was finished.</summary>
    public static void ProjectTodoNeedsAttention()
    {
        var item = CreateRegistrationTask();

        item.Priority = TodoPriority.High;

        using var flags = item
            .WhenChanging(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority,
                static (_, _, isDone, due, priority) => !isDone && due is not null && priority == TodoPriority.High)
            .Record();

        item.IsDone = true;

        SampleCheck.SequenceEqual([true, true], flags.Values);
    }

    /// <summary>Observes every editable property of a to-do item; six properties is the whole form.</summary>
    public static void ObserveWholeTodo()
    {
        var item = CreateRegistrationTask();

        using var seen = item.WhenChanging(x => x.Title, x => x.Notes, x => x.IsDone, x => x.DueDate, x => x.Priority, x => x.Tags).Record();

        SampleCheck.Equal(new(RegistrationTitle, RegistrationNotes, false, _registrationDue, TodoPriority.Normal, _registrationTags), seen.Latest);

        item.Tags = _urgentTags;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(_registrationTags, seen.Latest.Property6);
        SampleCheck.Equal(_urgentTags, item.Tags);
    }

    /// <summary>Records the title with its labels as it was before the labels changed.</summary>
    public static void ProjectTodoTitleWithLabels()
    {
        var item = CreateRegistrationTask();

        using var lines = item
            .WhenChanging(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority,
                x => x.Tags,
                static (title, _, _, _, _, tags) => $"{title} #{string.Join(" #", tags)}")
            .Record();

        item.Tags = _urgentTags;

        const string Line = $"{RegistrationTitle} #errands #car";

        SampleCheck.SequenceEqual([Line, Line], lines.Values);
    }

    /// <summary>Observes seven properties of an issue.</summary>
    public static void ObserveIssueSummary()
    {
        var issue = CreateCheckoutIssue();

        using var seen = issue.WhenChanging(x => x.Number, x => x.Title, x => x.State, x => x.Author, x => x.Assignee!, x => x.Labels, x => x.UpdatedAt).Record();

        SampleCheck.Equal(new(CheckoutNumber, CheckoutTitle, IssueState.Open, _maria, _priya, _checkoutLabels, _checkoutUpdatedAt), seen.Latest);

        issue.UpdatedAt = _repliedAt;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(_checkoutUpdatedAt, seen.Latest.Property7);
        SampleCheck.Equal(_repliedAt, issue.UpdatedAt);
    }

    /// <summary>Records the assignment line an issue had before it was reassigned, from seven properties.</summary>
    public static void ProjectIssueAssignment()
    {
        var issue = CreateCheckoutIssue();

        using var lines = issue
            .WhenChanging(
                x => x.Number,
                x => x.Title,
                x => x.State,
                x => x.Author,
                x => x.Assignee!,
                x => x.Labels,
                x => x.UpdatedAt,
                static (number, title, _, _, assignee, _, _) => $"#{number} {title} -> {assignee?.Login ?? Unassigned}")
            .Record();

        issue.Assignee = _tomas;

        var line = $"#{CheckoutNumber} {CheckoutTitle} -> {PriyaLogin}";

        SampleCheck.SequenceEqual([line, line], lines.Values);
        SampleCheck.Equal(TomasLogin, issue.Assignee!.Login);
    }

    /// <summary>Observes eight properties of an issue.</summary>
    public static void ObserveIssueWithComments()
    {
        var issue = CreateCheckoutIssue();

        using var seen = issue
            .WhenChanging(x => x.Number, x => x.Title, x => x.State, x => x.Author, x => x.Assignee!, x => x.Labels, x => x.UpdatedAt, x => x.Comments)
            .Record();

        SampleCheck.Equal(new(CheckoutNumber, CheckoutTitle, IssueState.Open, _maria, _priya, _checkoutLabels, _checkoutUpdatedAt, _noComments), seen.Latest);

        issue.Comments = _firstReply;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(_noComments, seen.Latest.Property8);
        SampleCheck.Equal(_firstReply, issue.Comments);
    }

    /// <summary>Records whether an open issue still waited for its first reply before the reply arrived.</summary>
    public static void ProjectIssueAwaitsFirstReply()
    {
        var issue = CreateCheckoutIssue();

        using var flags = issue
            .WhenChanging(
                x => x.Number,
                x => x.Title,
                x => x.State,
                x => x.Author,
                x => x.Assignee!,
                x => x.Labels,
                x => x.UpdatedAt,
                x => x.Comments,
                static (_, _, state, _, _, _, _, comments) => state == IssueState.Open && comments.Count == 0)
            .Record();

        issue.Comments = _firstReply;

        SampleCheck.SequenceEqual([true, true], flags.Values);
    }

    /// <summary>Observes a transfer and the fields of its source account, nine properties in all.</summary>
    public static void ObserveTransferAndSourceAccount()
    {
        var form = CreateTransferForm();

        using var seen = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance)
            .Record();

        SampleCheck.Equal(
            new(RentAmount, RentReference, EverydayId, EverydayName, AccountKind.Everyday, Currency, EverydayBalance, EverydayOverdraft, EverydayAvailable),
            seen.Latest);

        form.AvailableBalance = EverydayBalance;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(EverydayAvailable, seen.Latest.Property9);
        SampleCheck.Equal(EverydayBalance, form.AvailableBalance);
    }

    /// <summary>Records whether the account could cover the amount before the amount changed.</summary>
    public static void ProjectTransferIsAffordable()
    {
        var form = CreateTransferForm();

        using var flags = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                static (amount, _, _, _, _, _, _, _, available) => amount <= available)
            .Record();

        form.Amount = AboveAvailable;

        SampleCheck.SequenceEqual([true, true], flags.Values);
    }

    /// <summary>Adds the payee to the transfer and its source account fields, ten properties in all.</summary>
    public static void ObserveTransferWithPayee()
    {
        var form = CreateTransferForm();

        using var seen = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!)
            .Record();

        SampleCheck.Equal(
            new(RentAmount, RentReference, EverydayId, EverydayName, AccountKind.Everyday, Currency, EverydayBalance, EverydayOverdraft, EverydayAvailable, _landlord),
            seen.Latest);

        form.Payee = _strata;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(_landlord, seen.Latest.Property10);
        SampleCheck.Equal(_strata, form.Payee);
    }

    /// <summary>Records whether the amount fitted the account and the payee's limit before the amount changed.</summary>
    public static void ProjectTransferWithinPayeeLimit()
    {
        var form = CreateTransferForm();

        using var flags = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                static (amount, _, _, _, _, _, _, _, available, payee) =>
                    payee is not null && amount <= available && (payee.TransferLimit is not { } limit || amount <= limit))
            .Record();

        form.Amount = AbovePayeeLimit;

        SampleCheck.SequenceEqual([true, true], flags.Values);
    }

    /// <summary>Adds the day the transfer is sent, eleven properties in all.</summary>
    public static void ObserveTransferSchedule()
    {
        var form = CreateTransferForm();

        using var seen = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor)
            .Record();

        SampleCheck.Equal(
            new(RentAmount, RentReference, EverydayId, EverydayName, AccountKind.Everyday, Currency, EverydayBalance, EverydayOverdraft, EverydayAvailable, _landlord, _scheduledFor),
            seen.Latest);

        form.ScheduledFor = _rescheduledFor;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(_scheduledFor, seen.Latest.Property11);
        SampleCheck.Equal(_rescheduledFor, form.ScheduledFor);
    }

    /// <summary>Records the statement line the transfer had before it was rescheduled.</summary>
    public static void ProjectTransferStatementLine()
    {
        var form = CreateTransferForm();

        using var lines = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                static (_, reference, _, _, _, _, _, _, _, _, scheduledFor) => $"{reference} on {scheduledFor.ToString(IsoDay, CultureInfo.InvariantCulture)}")
            .Record();

        form.ScheduledFor = _rescheduledFor;

        const string Line = $"{RentReference} on 2026-04-01";

        SampleCheck.SequenceEqual([Line, Line], lines.Values);
    }

    /// <summary>Adds whether the transfer repeats, twelve properties in all.</summary>
    public static void ObserveTransferRecurrence()
    {
        var form = CreateTransferForm();

        using var seen = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring)
            .Record();

        SampleCheck.Equal(
            new(RentAmount, RentReference, EverydayId, EverydayName, AccountKind.Everyday, Currency, EverydayBalance, EverydayOverdraft, EverydayAvailable, _landlord, _scheduledFor, true),
            seen.Latest);

        form.IsRecurring = false;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(true, seen.Latest.Property12);
        SampleCheck.Equal(false, form.IsRecurring);
    }

    /// <summary>Records how the transfer was described before it stopped repeating.</summary>
    public static void ProjectTransferRecurrenceLabel()
    {
        var form = CreateTransferForm();

        using var labels = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                static (_, reference, _, _, _, _, _, _, _, _, _, isRecurring) => $"{reference} ({(isRecurring ? MonthlyText : OnceText)})")
            .Record();

        form.IsRecurring = false;

        const string Label = $"{RentReference} ({MonthlyText})";

        SampleCheck.SequenceEqual([Label, Label], labels.Values);
    }

    /// <summary>Adds whether the payee is told, thirteen properties in all.</summary>
    public static void ObserveTransferNotification()
    {
        var form = CreateTransferForm();

        using var seen = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee)
            .Record();

        SampleCheck.Equal(
            new(RentAmount, RentReference, EverydayId, EverydayName, AccountKind.Everyday, Currency, EverydayBalance, EverydayOverdraft, EverydayAvailable, _landlord, _scheduledFor, true, true),
            seen.Latest);

        form.NotifyPayee = false;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(true, seen.Latest.Property13);
        SampleCheck.Equal(false, form.NotifyPayee);
    }

    /// <summary>Records whether the payee was going to be told before the customer switched the notice off.</summary>
    public static void ProjectTransferNotifiesPayee()
    {
        var form = CreateTransferForm();

        using var flags = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                static (_, _, _, _, _, _, _, _, _, payee, _, _, notifyPayee) => notifyPayee && payee is not null)
            .Record();

        form.NotifyPayee = false;

        SampleCheck.SequenceEqual([true, true], flags.Values);
    }

    /// <summary>Adds the private note, fourteen properties in all.</summary>
    public static void ObserveTransferMemo()
    {
        var form = CreateTransferForm();

        using var seen = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo)
            .Record();

        SampleCheck.Equal(
            new(
                RentAmount,
                RentReference,
                EverydayId,
                EverydayName,
                AccountKind.Everyday,
                Currency,
                EverydayBalance,
                EverydayOverdraft,
                EverydayAvailable,
                _landlord,
                _scheduledFor,
                true,
                true,
                MemoText),
            seen.Latest);

        form.Memo = MemoRevised;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(MemoText, seen.Latest.Property14);
        SampleCheck.Equal(MemoRevised, form.Memo);
    }

    /// <summary>Records the note the customer was about to replace, or the reference when there was none.</summary>
    public static void ProjectTransferNote()
    {
        var form = CreateTransferForm();

        using var notes = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo,
                static (_, reference, _, _, _, _, _, _, _, _, _, _, _, memo) => memo.Length > 0 ? memo : reference)
            .Record();

        form.Memo = MemoRevised;

        SampleCheck.SequenceEqual([MemoText, MemoText], notes.Values);
    }

    /// <summary>Adds what the transfer pays for, fifteen properties in all.</summary>
    public static void ObserveTransferPurpose()
    {
        var form = CreateTransferForm();

        using var seen = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo,
                x => x.Purpose)
            .Record();

        SampleCheck.Equal(
            new(
                RentAmount,
                RentReference,
                EverydayId,
                EverydayName,
                AccountKind.Everyday,
                Currency,
                EverydayBalance,
                EverydayOverdraft,
                EverydayAvailable,
                _landlord,
                _scheduledFor,
                true,
                true,
                MemoText,
                PurposeText),
            seen.Latest);

        form.Purpose = PurposeRevised;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(PurposeText, seen.Latest.Property15);
        SampleCheck.Equal(PurposeRevised, form.Purpose);
    }

    /// <summary>Records the purpose and reference the transfer had before the purpose changed.</summary>
    public static void ProjectTransferPurposeLine()
    {
        var form = CreateTransferForm();

        using var lines = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo,
                x => x.Purpose,
                static (_, reference, _, _, _, _, _, _, _, _, _, _, _, _, purpose) => $"{purpose}: {reference}")
            .Record();

        form.Purpose = PurposeRevised;

        const string Line = $"{PurposeText}: {RentReference}";

        SampleCheck.SequenceEqual([Line, Line], lines.Values);
    }

    /// <summary>Observes sixteen properties of the transfer screen, the most one call accepts.</summary>
    public static void ObserveTransferFee()
    {
        var form = CreateTransferForm();

        using var seen = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo,
                x => x.Purpose,
                x => x.Fee)
            .Record();

        SampleCheck.Equal(
            new(
                RentAmount,
                RentReference,
                EverydayId,
                EverydayName,
                AccountKind.Everyday,
                Currency,
                EverydayBalance,
                EverydayOverdraft,
                EverydayAvailable,
                _landlord,
                _scheduledFor,
                true,
                true,
                MemoText,
                PurposeText,
                TransferFee),
            seen.Latest);

        form.Fee = 0M;

        SampleCheck.Equal(AfterOneChange, seen.Count);
        SampleCheck.Equal(TransferFee, seen.Latest.Property16);
        SampleCheck.Equal(0M, form.Fee);
    }

    /// <summary>Records whether the amount and fee fitted the account before the fee changed.</summary>
    public static void ProjectTransferTotalIsAffordable()
    {
        var form = CreateTransferForm();

        using var flags = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo,
                x => x.Purpose,
                x => x.Fee,
                static (amount, _, _, _, _, _, _, _, available, _, _, _, _, _, _, fee) => amount + fee <= available)
            .Record();

        form.Fee = AboveAvailable;

        SampleCheck.SequenceEqual([true, true], flags.Values);
    }

    /// <summary>Creates the to-do item form the examples observe.</summary>
    /// <returns>An open, unfinished item.</returns>
    private static TodoItemDraft CreateRegistrationTask() => new()
    {
        Title = RegistrationTitle,
        Notes = RegistrationNotes,
        DueDate = _registrationDue,
        Priority = TodoPriority.Normal,
        Tags = _registrationTags,
    };

    /// <summary>Creates the issue form the examples observe.</summary>
    /// <returns>An open issue reported by Maria and assigned to Priya.</returns>
    private static IssueDraft CreateCheckoutIssue() => new()
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

    /// <summary>Creates the transfer screen the examples observe.</summary>
    /// <returns>A transfer that sends the rent from the everyday account to the landlord.</returns>
    private static TransferForm CreateTransferForm() => new()
    {
        Amount = RentAmount,
        Reference = RentReference,
        SourceId = EverydayId,
        SourceName = EverydayName,
        SourceKind = AccountKind.Everyday,
        Currency = Currency,
        Balance = EverydayBalance,
        OverdraftLimit = EverydayOverdraft,
        AvailableBalance = EverydayAvailable,
        Payee = _landlord,
        ScheduledFor = _scheduledFor,
        IsRecurring = true,
        NotifyPayee = true,
        Memo = MemoText,
        Purpose = PurposeText,
        Fee = TransferFee,
    };
}

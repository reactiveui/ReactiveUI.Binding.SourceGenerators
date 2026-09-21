// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.ObservingWhenAny;

/// <summary>
/// Shows every <c>WhenAny</c> overload, from one observed property to twelve. <c>WhenAny</c> hands the selector one
/// <see cref="IObservedChange{TSender, TValue}"/> per property instead of the bare value. The change names the object
/// you observed and the current value. The selector runs when you subscribe and again after each change.
/// </summary>
public static class WhenAnyExamples
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

    /// <summary>The identifier of the payee.</summary>
    private const int PayeeId = 3;

    /// <summary>The name of the payee.</summary>
    private const string PayeeName = "Harbour Realty";

    /// <summary>The account that receives the money.</summary>
    private const string PayeeAccountNumber = "062-000 1234 5678";

    /// <summary>The most the customer allows in one transfer to the payee.</summary>
    private const decimal PayeeLimit = 1000M;

    /// <summary>The amount the customer starts with.</summary>
    private const decimal RentAmount = 250M;

    /// <summary>An amount above the payee's limit and above the available balance.</summary>
    private const decimal AboveAvailable = 9000M;

    /// <summary>An amount above the payee's limit and below the available balance.</summary>
    private const decimal AbovePayeeLimit = 1500M;

    /// <summary>The money left in the account after the rent is paid: 4700.50 available less 250.</summary>
    private const decimal RemainingAfterRent = 4450.50M;

    /// <summary>The money left in the account after an amount above the available balance: 4700.50 less 9000.</summary>
    private const decimal ShortfallAfterLargeTransfer = -4299.50M;

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

    /// <summary>The payee of the transfer.</summary>
    private static readonly Payee _landlord = new(PayeeId, PayeeName, PayeeAccountNumber, PayeeLimit);

    /// <summary>Observes the title of a to-do item and keeps each change as it arrives.</summary>
    public static void ObserveTodoTitleChange()
    {
        var item = CreateRegistrationTask();

        using var changes = item.WhenAny(x => x.Title, static change => change).Record();

        SampleCheck.Equal(item, changes.Latest.Sender);
        SampleCheck.Equal(RegistrationTitle, changes.Latest.Value);

        // A generated observation has no expression tree to report.
        SampleCheck.Equal(true, changes.Latest.Expression is null);

        item.Title = RenamedTitle;

        SampleCheck.Equal(AfterOneChange, changes.Count);
        SampleCheck.Equal(item, changes.Latest.Sender);
        SampleCheck.Equal(RenamedTitle, changes.Latest.Value);
    }

    /// <summary>Turns the title of a to-do item into its length, as a character counter does.</summary>
    public static void ProjectTodoTitleLength()
    {
        var item = CreateRegistrationTask();

        using var lengths = item.WhenAny(x => x.Title, static title => title.Value.Length).Record();

        item.Title = RenamedTitle;

        SampleCheck.SequenceEqual([RegistrationTitle.Length, RenamedTitle.Length], lengths.Values);
    }

    /// <summary>Turns the title and the done flag into one checklist line.</summary>
    public static void ProjectTodoChecklistLine()
    {
        var item = CreateRegistrationTask();

        using var lines = item
            .WhenAny(x => x.Title, x => x.IsDone, static (title, isDone) => (isDone.Value ? DoneMark : OpenMark) + title.Value)
            .Record();

        SampleCheck.SequenceEqual([OpenMark + RegistrationTitle], lines.Values);

        item.IsDone = true;

        SampleCheck.SequenceEqual([OpenMark + RegistrationTitle, DoneMark + RegistrationTitle], lines.Values);
    }

    /// <summary>Works out how urgent a to-do item is from three properties; a finished item is never urgent.</summary>
    public static void ProjectTodoEffectivePriority()
    {
        var item = CreateRegistrationTask();

        using var priorities = item
            .WhenAny(x => x.Title, x => x.IsDone, x => x.Priority, static (_, isDone, priority) => isDone.Value ? TodoPriority.Low : priority.Value)
            .Record();

        item.Priority = TodoPriority.High;
        item.IsDone = true;

        SampleCheck.SequenceEqual([TodoPriority.Normal, TodoPriority.High, TodoPriority.Low], priorities.Values);
    }

    /// <summary>Builds a headline with the deadline from four properties.</summary>
    public static void ProjectTodoHeadline()
    {
        var item = CreateRegistrationTask();

        using var headlines = item
            .WhenAny(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                static (title, _, isDone, due) =>
                    due.Value is { } day && !isDone.Value ? $"{title.Value} due {day.ToString(IsoDay, CultureInfo.InvariantCulture)}" : title.Value)
            .Record();

        item.DueDate = _movedDue;

        SampleCheck.SequenceEqual([$"{RegistrationTitle} due 2026-03-20", $"{RegistrationTitle} due 2026-03-27"], headlines.Values);
    }

    /// <summary>Decides from five properties whether a to-do item needs attention now.</summary>
    public static void ProjectTodoNeedsAttention()
    {
        var item = CreateRegistrationTask();

        using var flags = item
            .WhenAny(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority,
                static (_, _, isDone, due, priority) => !isDone.Value && due.Value is not null && priority.Value == TodoPriority.High)
            .Record();

        item.Priority = TodoPriority.High;

        SampleCheck.SequenceEqual([false, true], flags.Values);
    }

    /// <summary>Lists the labels after the title from six properties.</summary>
    public static void ProjectTodoTitleWithLabels()
    {
        var item = CreateRegistrationTask();

        using var lines = item
            .WhenAny(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority,
                x => x.Tags,
                static (title, _, _, _, _, tags) => $"{title.Value} #{string.Join(" #", tags.Value)}")
            .Record();

        item.Tags = _urgentTags;

        SampleCheck.SequenceEqual([$"{RegistrationTitle} #errands #car", $"{RegistrationTitle} #errands #car #urgent"], lines.Values);
    }

    /// <summary>Numbers a to-do item from seven properties, every editable property of the item.</summary>
    public static void ProjectTodoNumberedHeadline()
    {
        var item = CreateRegistrationTask();

        using var lines = item
            .WhenAny(
                x => x.Id,
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority,
                x => x.Tags,
                static (id, title, _, isDone, _, _, _) => $"{(isDone.Value ? DoneMark : OpenMark)}{id.Value}: {title.Value}")
            .Record();

        item.Title = RenamedTitle;
        item.IsDone = true;

        SampleCheck.SequenceEqual(
            [$"{OpenMark}{RegistrationId}: {RegistrationTitle}", $"{OpenMark}{RegistrationId}: {RenamedTitle}", $"{DoneMark}{RegistrationId}: {RenamedTitle}"],
            lines.Values);
    }

    /// <summary>Works out from eight properties of a transfer how much money the account keeps after it.</summary>
    public static void ProjectTransferRemainingBalance()
    {
        var draft = CreateTransferDraft();

        using var remaining = draft
            .WhenAny(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Name,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance,
                x => x.Payee!,
                static (amount, _, _, _, _, _, available, _) => available.Value - amount.Value)
            .Record();

        draft.Amount = AboveAvailable;

        SampleCheck.SequenceEqual([RemainingAfterRent, ShortfallAfterLargeTransfer], remaining.Values);
    }

    /// <summary>Checks from nine properties whether the account can cover the amount and the payee can receive it.</summary>
    public static void ProjectTransferIsPayable()
    {
        var draft = CreateTransferDraft();

        using var flags = draft
            .WhenAny(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Name,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance,
                x => x.Payee!,
                x => x.Source!.Id,
                static (amount, _, _, _, _, _, available, payee, _) => amount.Value <= available.Value && payee.Value.AccountNumber.Length > 0)
            .Record();

        draft.Amount = AboveAvailable;

        SampleCheck.SequenceEqual([true, false], flags.Values);
    }

    /// <summary>Checks from ten properties whether the amount fits the account and the payee's limit.</summary>
    public static void ProjectTransferWithinPayeeLimit()
    {
        var draft = CreateTransferDraft();

        using var flags = draft
            .WhenAny(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Name,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance,
                x => x.Payee!,
                x => x.Source!.Id,
                x => x.Source!.Kind,
                static (amount, _, _, _, _, _, available, payee, _, _) =>
                    amount.Value <= available.Value && (payee.Value.TransferLimit is not { } max || amount.Value <= max))
            .Record();

        draft.Amount = AbovePayeeLimit;

        SampleCheck.SequenceEqual([true, false], flags.Values);
    }

    /// <summary>Writes the statement line from eleven properties: the account, the payee and the reference. The payee is replaced as a whole.</summary>
    public static void ProjectTransferStatementLine()
    {
        var draft = CreateTransferDraft();

        using var lines = draft
            .WhenAny(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Name,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance,
                x => x.Payee!,
                x => x.Source!.Id,
                x => x.Source!.Kind,
                x => x.Source!,
                static (_, reference, _, _, _, _, _, payee, sourceId, _, _) => $"{sourceId.Value} -> {payee.Value.Name}: {reference.Value}")
            .Record();

        draft.Reference = RentReferenceRevised;

        SampleCheck.SequenceEqual([$"{EverydayId} -> {PayeeName}: {RentReference}", $"{EverydayId} -> {PayeeName}: {RentReferenceRevised}"], lines.Values);
    }

    /// <summary>Flags from twelve properties of the transfer screen, the most one call accepts, a transfer that needs a manual review.</summary>
    public static void ProjectTransferNeedsReview()
    {
        TransferViewModel screen = new(new InMemoryBankingBackend(ManualClock.StartOfWorkingDay()));
        var draft = screen.Draft;
        draft.Source = CreateEverydayAccount();
        draft.Payee = _landlord;
        draft.Amount = RentAmount;
        draft.Reference = RentReference;

        using var flags = screen
            .WhenAny(
                x => x.Draft.Amount,
                x => x.Draft.Reference,
                x => x.Draft.Source!.Name,
                x => x.Draft.Source!.Currency,
                x => x.Draft.Source!.Balance,
                x => x.Draft.Source!.OverdraftLimit,
                x => x.Draft.Source!.AvailableBalance,
                x => x.Draft.Payee!,
                x => x.Draft.Source!.Id,
                x => x.Draft.Source!.Kind,
                x => x.IsValid,
                x => x.ErrorMessage,
                static (amount, _, _, _, _, _, available, _, _, kind, _, _) => kind.Value == AccountKind.Savings || amount.Value > available.Value)
            .Record();

        draft.Source!.Kind = AccountKind.Savings;

        SampleCheck.SequenceEqual([false, true], flags.Values);
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
}

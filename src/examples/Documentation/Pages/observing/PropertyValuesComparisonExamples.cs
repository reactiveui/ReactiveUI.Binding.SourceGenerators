// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>
/// Compares and prints the <c>PropertyValues</c> emissions of <c>WhenChanged</c>. An emission is a read-only record
/// struct, so two emissions are equal when every member is equal, the hash code follows the members, and the text names
/// each member.
/// </summary>
public static class PropertyValuesComparisonExamples
{
    /// <summary>The amount the customer sends.</summary>
    private const decimal RentAmount = 250M;

    /// <summary>The balance of the account the money leaves.</summary>
    private const decimal EverydayBalance = 4200.50M;

    /// <summary>How far below zero the account may go.</summary>
    private const decimal EverydayOverdraft = 500M;

    /// <summary>The money the customer can spend: the balance plus the overdraft limit.</summary>
    private const decimal EverydayAvailable = 4700.50M;

    /// <summary>The fee the bank charges for the transfer.</summary>
    private const decimal TransferFee = 1.5M;

    /// <summary>The identifier of the payee.</summary>
    private const int PayeeId = 3;

    /// <summary>The most the customer allows in one transfer to the payee.</summary>
    private const decimal PayeeLimit = 1000M;

    /// <summary>The day the transfer is sent.</summary>
    private static readonly DateOnly _scheduledFor = new(2026, 3, 20);

    /// <summary>The payee of the transfer.</summary>
    private static readonly Payee _landlord = new(PayeeId, "Harbour Realty", "062-000 1234 5678", PayeeLimit);

    /// <summary>Prints an emission of two properties; the text names each member.</summary>
    /// <returns>A task that completes when the values are printed.</returns>
    public static async Task PrintTwoTransferValues()
    {
        var form = CreateTransferForm();

        var values = await form.WhenChanged(x => x.Amount, x => x.Reference).FirstAsync();

        var text = values.ToString();

        Console.WriteLine($"Emission: {text}.");

        // Output:
        // Emission: PropertyValues { Property1 = 250, Property2 = Rent March }.
    }

    /// <summary>Compares, hashes and prints an emission of three transfer properties.</summary>
    /// <returns>A task that completes when the values are compared.</returns>
    public static async Task CompareThreeTransferValues()
    {
        var form = CreateTransferForm();

        var values = await form
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId)
            .FirstAsync();
        var copy = values with { };
        var edited = values with { Property1 = default };

        Console.WriteLine(values == copy);
        Console.WriteLine(values != edited);
        Console.WriteLine(values.Equals((object)copy));
        Console.WriteLine(values.GetHashCode() == copy.GetHashCode());
        Console.WriteLine(values.ToString().Contains("Property3 = "));

        // Output:
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Compares, hashes and prints an emission of four transfer properties.</summary>
    /// <returns>A task that completes when the values are compared.</returns>
    public static async Task CompareFourTransferValues()
    {
        var form = CreateTransferForm();

        var values = await form
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName)
            .FirstAsync();
        var copy = values with { };
        var edited = values with { Property1 = default };

        Console.WriteLine(values == copy);
        Console.WriteLine(values != edited);
        Console.WriteLine(values.Equals((object)copy));
        Console.WriteLine(values.GetHashCode() == copy.GetHashCode());
        Console.WriteLine(values.ToString().Contains("Property4 = "));

        // Output:
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Compares, hashes and prints an emission of five transfer properties.</summary>
    /// <returns>A task that completes when the values are compared.</returns>
    public static async Task CompareFiveTransferValues()
    {
        var form = CreateTransferForm();

        var values = await form
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind)
            .FirstAsync();
        var copy = values with { };
        var edited = values with { Property1 = default };

        Console.WriteLine(values == copy);
        Console.WriteLine(values != edited);
        Console.WriteLine(values.Equals((object)copy));
        Console.WriteLine(values.GetHashCode() == copy.GetHashCode());
        Console.WriteLine(values.ToString().Contains("Property5 = "));

        // Output:
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Compares, hashes and prints an emission of six transfer properties.</summary>
    /// <returns>A task that completes when the values are compared.</returns>
    public static async Task CompareSixTransferValues()
    {
        var form = CreateTransferForm();

        var values = await form
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency)
            .FirstAsync();
        var copy = values with { };
        var edited = values with { Property1 = default };

        Console.WriteLine(values == copy);
        Console.WriteLine(values != edited);
        Console.WriteLine(values.Equals((object)copy));
        Console.WriteLine(values.GetHashCode() == copy.GetHashCode());
        Console.WriteLine(values.ToString().Contains("Property6 = "));

        // Output:
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Compares, hashes and prints an emission of seven transfer properties.</summary>
    /// <returns>A task that completes when the values are compared.</returns>
    public static async Task CompareSevenTransferValues()
    {
        var form = CreateTransferForm();

        var values = await form
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance)
            .FirstAsync();
        var copy = values with { };
        var edited = values with { Property1 = default };

        Console.WriteLine(values == copy);
        Console.WriteLine(values != edited);
        Console.WriteLine(values.Equals((object)copy));
        Console.WriteLine(values.GetHashCode() == copy.GetHashCode());
        Console.WriteLine(values.ToString().Contains("Property7 = "));

        // Output:
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Compares, hashes and prints an emission of eight transfer properties.</summary>
    /// <returns>A task that completes when the values are compared.</returns>
    public static async Task CompareEightTransferValues()
    {
        var form = CreateTransferForm();

        var values = await form
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit)
            .FirstAsync();
        var copy = values with { };
        var edited = values with { Property1 = default };

        Console.WriteLine(values == copy);
        Console.WriteLine(values != edited);
        Console.WriteLine(values.Equals((object)copy));
        Console.WriteLine(values.GetHashCode() == copy.GetHashCode());
        Console.WriteLine(values.ToString().Contains("Property8 = "));

        // Output:
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Compares, hashes and prints an emission of nine transfer properties.</summary>
    /// <returns>A task that completes when the values are compared.</returns>
    public static async Task CompareNineTransferValues()
    {
        var form = CreateTransferForm();

        var values = await form
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance)
            .FirstAsync();
        var copy = values with { };
        var edited = values with { Property1 = default };

        Console.WriteLine(values == copy);
        Console.WriteLine(values != edited);
        Console.WriteLine(values.Equals((object)copy));
        Console.WriteLine(values.GetHashCode() == copy.GetHashCode());
        Console.WriteLine(values.ToString().Contains("Property9 = "));

        // Output:
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Compares, hashes and prints an emission of ten transfer properties.</summary>
    /// <returns>A task that completes when the values are compared.</returns>
    public static async Task CompareTenTransferValues()
    {
        var form = CreateTransferForm();

        var values = await form
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.ScheduledFor)
            .FirstAsync();
        var copy = values with { };
        var edited = values with { Property1 = default };

        Console.WriteLine(values == copy);
        Console.WriteLine(values != edited);
        Console.WriteLine(values.Equals((object)copy));
        Console.WriteLine(values.GetHashCode() == copy.GetHashCode());
        Console.WriteLine(values.ToString().Contains("Property10 = "));

        // Output:
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Compares, hashes and prints an emission of eleven transfer properties.</summary>
    /// <returns>A task that completes when the values are compared.</returns>
    public static async Task CompareElevenTransferValues()
    {
        var form = CreateTransferForm();

        var values = await form
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.ScheduledFor,
                x => x.IsRecurring)
            .FirstAsync();
        var copy = values with { };
        var edited = values with { Property1 = default };

        Console.WriteLine(values == copy);
        Console.WriteLine(values != edited);
        Console.WriteLine(values.Equals((object)copy));
        Console.WriteLine(values.GetHashCode() == copy.GetHashCode());
        Console.WriteLine(values.ToString().Contains("Property11 = "));

        // Output:
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Compares, hashes and prints an emission of twelve transfer properties.</summary>
    /// <returns>A task that completes when the values are compared.</returns>
    public static async Task CompareTwelveTransferValues()
    {
        var form = CreateTransferForm();

        var values = await form
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee)
            .FirstAsync();
        var copy = values with { };
        var edited = values with { Property1 = default };

        Console.WriteLine(values == copy);
        Console.WriteLine(values != edited);
        Console.WriteLine(values.Equals((object)copy));
        Console.WriteLine(values.GetHashCode() == copy.GetHashCode());
        Console.WriteLine(values.ToString().Contains("Property12 = "));

        // Output:
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Compares, hashes and prints an emission of thirteen transfer properties.</summary>
    /// <returns>A task that completes when the values are compared.</returns>
    public static async Task CompareThirteenTransferValues()
    {
        var form = CreateTransferForm();

        var values = await form
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo)
            .FirstAsync();
        var copy = values with { };
        var edited = values with { Property1 = default };

        Console.WriteLine(values == copy);
        Console.WriteLine(values != edited);
        Console.WriteLine(values.Equals((object)copy));
        Console.WriteLine(values.GetHashCode() == copy.GetHashCode());
        Console.WriteLine(values.ToString().Contains("Property13 = "));

        // Output:
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Compares, hashes and prints an emission of fourteen transfer properties.</summary>
    /// <returns>A task that completes when the values are compared.</returns>
    public static async Task CompareFourteenTransferValues()
    {
        var form = CreateTransferForm();

        var values = await form
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo,
                x => x.Purpose)
            .FirstAsync();
        var copy = values with { };
        var edited = values with { Property1 = default };

        Console.WriteLine(values == copy);
        Console.WriteLine(values != edited);
        Console.WriteLine(values.Equals((object)copy));
        Console.WriteLine(values.GetHashCode() == copy.GetHashCode());
        Console.WriteLine(values.ToString().Contains("Property14 = "));

        // Output:
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Compares, hashes and prints an emission of fifteen transfer properties.</summary>
    /// <returns>A task that completes when the values are compared.</returns>
    public static async Task CompareFifteenTransferValues()
    {
        var form = CreateTransferForm();

        var values = await form
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo,
                x => x.Purpose,
                x => x.Fee)
            .FirstAsync();
        var copy = values with { };
        var edited = values with { Property1 = default };

        Console.WriteLine(values == copy);
        Console.WriteLine(values != edited);
        Console.WriteLine(values.Equals((object)copy));
        Console.WriteLine(values.GetHashCode() == copy.GetHashCode());
        Console.WriteLine(values.ToString().Contains("Property15 = "));

        // Output:
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Compares, hashes and prints an emission of sixteen transfer properties.</summary>
    /// <returns>A task that completes when the values are compared.</returns>
    public static async Task CompareSixteenTransferValues()
    {
        var form = CreateTransferForm();

        var values = await form
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo,
                x => x.Purpose,
                x => x.Fee,
                x => x.Payee!)
            .FirstAsync();
        var copy = values with { };
        var edited = values with { Property1 = default };

        Console.WriteLine(values == copy);
        Console.WriteLine(values != edited);
        Console.WriteLine(values.Equals((object)copy));
        Console.WriteLine(values.GetHashCode() == copy.GetHashCode());
        Console.WriteLine(values.ToString().Contains("Property16 = "));

        // Output:
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Creates the transfer form the examples observe.</summary>
    /// <returns>A form that sends the rent from the everyday account to the landlord.</returns>
    private static TransferForm CreateTransferForm() => new()
    {
        Amount = RentAmount,
        Reference = "Rent March",
        SourceId = "ACC-1001",
        SourceName = "Everyday",
        SourceKind = AccountKind.Everyday,
        Currency = "AUD",
        Balance = EverydayBalance,
        OverdraftLimit = EverydayOverdraft,
        AvailableBalance = EverydayAvailable,
        ScheduledFor = _scheduledFor,
        IsRecurring = true,
        NotifyPayee = true,
        Memo = "Paid on the first",
        Purpose = "Rent",
        Fee = TransferFee,
        Payee = _landlord,
    };
}

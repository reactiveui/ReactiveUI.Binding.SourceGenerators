// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>
/// Shows <c>WhenAnyDynamic</c>, which observes property chains that a program builds as expressions at run time. It takes
/// one to twelve chains and a selector that receives an observed change for each chain. It always reads the chains by
/// reflection, so use it only when the properties are not known when you write the code.
/// </summary>
public static class WhenAnyDynamicExamples
{
    /// <summary>The amount the customer starts with.</summary>
    private const decimal RentAmount = 250M;

    /// <summary>The amount the customer changes to.</summary>
    private const decimal RevisedAmount = 300M;

    /// <summary>The balance of the account the money leaves.</summary>
    private const decimal EverydayBalance = 4200.50M;

    /// <summary>How far below zero the account may go.</summary>
    private const decimal EverydayOverdraft = 500M;

    /// <summary>The money the customer can spend: the balance plus the overdraft limit.</summary>
    private const decimal EverydayAvailable = 4700.50M;

    /// <summary>The day the transfer is sent.</summary>
    private static readonly DateOnly _scheduledFor = new(2026, 3, 20);

    /// <summary>Registers the observation providers the run-time chain engine resolves by affinity.</summary>
    public static void RegisterObservationServices()
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices().BuildApp();
    }

    /// <summary>Observes one chain named by expressions and reports a value only when the chain changes it.</summary>
    public static void ObserveOneChain()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                static amount => $"{amount.Value}")
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250
        // 300
    }

    /// <summary>Observes one chain named by expressions and reports every notification, including a repeated value.</summary>
    public static void ObserveOneChainEveryTime()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                static amount => $"{amount.Value}",
                false)
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250
        // 300
    }

    /// <summary>Observes two chains named by expressions and reports a value only when the chain changes it.</summary>
    public static void ObserveTwoChains()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                static (amount, reference) => $"{amount.Value} {reference.Value}")
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes two chains named by expressions and reports every notification, including a repeated value.</summary>
    public static void ObserveTwoChainsEveryTime()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                static (amount, reference) => $"{amount.Value} {reference.Value}",
                false)
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes three chains named by expressions and reports a value only when the chain changes it.</summary>
    public static void ObserveThreeChains()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                static (amount, reference, _) => $"{amount.Value} {reference.Value}")
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes three chains named by expressions and reports every notification, including a repeated value.</summary>
    public static void ObserveThreeChainsEveryTime()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                static (amount, reference, _) => $"{amount.Value} {reference.Value}",
                false)
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes four chains named by expressions and reports a value only when the chain changes it.</summary>
    public static void ObserveFourChains()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                static (amount, reference, _, _) => $"{amount.Value} {reference.Value}")
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes four chains named by expressions and reports every notification, including a repeated value.</summary>
    public static void ObserveFourChainsEveryTime()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                static (amount, reference, _, _) => $"{amount.Value} {reference.Value}",
                false)
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes five chains named by expressions and reports a value only when the chain changes it.</summary>
    public static void ObserveFiveChains()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                static (amount, reference, _, _, _) => $"{amount.Value} {reference.Value}")
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes five chains named by expressions and reports every notification, including a repeated value.</summary>
    public static void ObserveFiveChainsEveryTime()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                static (amount, reference, _, _, _) => $"{amount.Value} {reference.Value}",
                false)
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes six chains named by expressions and reports a value only when the chain changes it.</summary>
    public static void ObserveSixChains()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                Expression.Property(root, nameof(TransferForm.Currency)),
                static (amount, reference, _, _, _, _) => $"{amount.Value} {reference.Value}")
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes six chains named by expressions and reports every notification, including a repeated value.</summary>
    public static void ObserveSixChainsEveryTime()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                Expression.Property(root, nameof(TransferForm.Currency)),
                static (amount, reference, _, _, _, _) => $"{amount.Value} {reference.Value}",
                false)
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes seven chains named by expressions and reports a value only when the chain changes it.</summary>
    public static void ObserveSevenChains()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                Expression.Property(root, nameof(TransferForm.Currency)),
                Expression.Property(root, nameof(TransferForm.Balance)),
                static (amount, reference, _, _, _, _, _) => $"{amount.Value} {reference.Value}")
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes seven chains named by expressions and reports every notification, including a repeated value.</summary>
    public static void ObserveSevenChainsEveryTime()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                Expression.Property(root, nameof(TransferForm.Currency)),
                Expression.Property(root, nameof(TransferForm.Balance)),
                static (amount, reference, _, _, _, _, _) => $"{amount.Value} {reference.Value}",
                false)
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes eight chains named by expressions and reports a value only when the chain changes it.</summary>
    public static void ObserveEightChains()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                Expression.Property(root, nameof(TransferForm.Currency)),
                Expression.Property(root, nameof(TransferForm.Balance)),
                Expression.Property(root, nameof(TransferForm.OverdraftLimit)),
                static (amount, reference, _, _, _, _, _, _) => $"{amount.Value} {reference.Value}")
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes eight chains named by expressions and reports every notification, including a repeated value.</summary>
    public static void ObserveEightChainsEveryTime()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                Expression.Property(root, nameof(TransferForm.Currency)),
                Expression.Property(root, nameof(TransferForm.Balance)),
                Expression.Property(root, nameof(TransferForm.OverdraftLimit)),
                static (amount, reference, _, _, _, _, _, _) => $"{amount.Value} {reference.Value}",
                false)
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes nine chains named by expressions and reports a value only when the chain changes it.</summary>
    public static void ObserveNineChains()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                Expression.Property(root, nameof(TransferForm.Currency)),
                Expression.Property(root, nameof(TransferForm.Balance)),
                Expression.Property(root, nameof(TransferForm.OverdraftLimit)),
                Expression.Property(root, nameof(TransferForm.AvailableBalance)),
                static (amount, reference, _, _, _, _, _, _, _) => $"{amount.Value} {reference.Value}")
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes nine chains named by expressions and reports every notification, including a repeated value.</summary>
    public static void ObserveNineChainsEveryTime()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                Expression.Property(root, nameof(TransferForm.Currency)),
                Expression.Property(root, nameof(TransferForm.Balance)),
                Expression.Property(root, nameof(TransferForm.OverdraftLimit)),
                Expression.Property(root, nameof(TransferForm.AvailableBalance)),
                static (amount, reference, _, _, _, _, _, _, _) => $"{amount.Value} {reference.Value}",
                false)
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes ten chains named by expressions and reports a value only when the chain changes it.</summary>
    public static void ObserveTenChains()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                Expression.Property(root, nameof(TransferForm.Currency)),
                Expression.Property(root, nameof(TransferForm.Balance)),
                Expression.Property(root, nameof(TransferForm.OverdraftLimit)),
                Expression.Property(root, nameof(TransferForm.AvailableBalance)),
                Expression.Property(root, nameof(TransferForm.ScheduledFor)),
                static (amount, reference, _, _, _, _, _, _, _, _) => $"{amount.Value} {reference.Value}")
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes ten chains named by expressions and reports every notification, including a repeated value.</summary>
    public static void ObserveTenChainsEveryTime()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                Expression.Property(root, nameof(TransferForm.Currency)),
                Expression.Property(root, nameof(TransferForm.Balance)),
                Expression.Property(root, nameof(TransferForm.OverdraftLimit)),
                Expression.Property(root, nameof(TransferForm.AvailableBalance)),
                Expression.Property(root, nameof(TransferForm.ScheduledFor)),
                static (amount, reference, _, _, _, _, _, _, _, _) => $"{amount.Value} {reference.Value}",
                false)
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes eleven chains named by expressions and reports a value only when the chain changes it.</summary>
    public static void ObserveElevenChains()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                Expression.Property(root, nameof(TransferForm.Currency)),
                Expression.Property(root, nameof(TransferForm.Balance)),
                Expression.Property(root, nameof(TransferForm.OverdraftLimit)),
                Expression.Property(root, nameof(TransferForm.AvailableBalance)),
                Expression.Property(root, nameof(TransferForm.ScheduledFor)),
                Expression.Property(root, nameof(TransferForm.IsRecurring)),
                static (amount, reference, _, _, _, _, _, _, _, _, _) => $"{amount.Value} {reference.Value}")
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes eleven chains named by expressions and reports every notification, including a repeated value.</summary>
    public static void ObserveElevenChainsEveryTime()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                Expression.Property(root, nameof(TransferForm.Currency)),
                Expression.Property(root, nameof(TransferForm.Balance)),
                Expression.Property(root, nameof(TransferForm.OverdraftLimit)),
                Expression.Property(root, nameof(TransferForm.AvailableBalance)),
                Expression.Property(root, nameof(TransferForm.ScheduledFor)),
                Expression.Property(root, nameof(TransferForm.IsRecurring)),
                static (amount, reference, _, _, _, _, _, _, _, _, _) => $"{amount.Value} {reference.Value}",
                false)
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes twelve chains named by expressions and reports a value only when the chain changes it.</summary>
    public static void ObserveTwelveChains()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                Expression.Property(root, nameof(TransferForm.Currency)),
                Expression.Property(root, nameof(TransferForm.Balance)),
                Expression.Property(root, nameof(TransferForm.OverdraftLimit)),
                Expression.Property(root, nameof(TransferForm.AvailableBalance)),
                Expression.Property(root, nameof(TransferForm.ScheduledFor)),
                Expression.Property(root, nameof(TransferForm.IsRecurring)),
                Expression.Property(root, nameof(TransferForm.NotifyPayee)),
                static (amount, reference, _, _, _, _, _, _, _, _, _, _) => $"{amount.Value} {reference.Value}")
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Observes twelve chains named by expressions and reports every notification, including a repeated value.</summary>
    public static void ObserveTwelveChainsEveryTime()
    {
        var form = CreateTransferForm();
        var root = Expression.Parameter(typeof(TransferForm), "x");

        using (form
            .WhenAnyDynamic(
                Expression.Property(root, nameof(TransferForm.Amount)),
                Expression.Property(root, nameof(TransferForm.Reference)),
                Expression.Property(root, nameof(TransferForm.SourceId)),
                Expression.Property(root, nameof(TransferForm.SourceName)),
                Expression.Property(root, nameof(TransferForm.SourceKind)),
                Expression.Property(root, nameof(TransferForm.Currency)),
                Expression.Property(root, nameof(TransferForm.Balance)),
                Expression.Property(root, nameof(TransferForm.OverdraftLimit)),
                Expression.Property(root, nameof(TransferForm.AvailableBalance)),
                Expression.Property(root, nameof(TransferForm.ScheduledFor)),
                Expression.Property(root, nameof(TransferForm.IsRecurring)),
                Expression.Property(root, nameof(TransferForm.NotifyPayee)),
                static (amount, reference, _, _, _, _, _, _, _, _, _, _) => $"{amount.Value} {reference.Value}",
                false)
            .Subscribe(Console.WriteLine))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250 Rent March
        // 300 Rent March
    }

    /// <summary>Creates the transfer form the examples observe.</summary>
    /// <returns>A form that sends the rent from the everyday account.</returns>
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
    };
}

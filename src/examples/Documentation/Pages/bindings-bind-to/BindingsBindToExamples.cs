// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.BindingsBindTo;

/// <summary>Shows how <c>BindTo</c> writes each value of an observable onto a property.</summary>
public static class BindingsBindToExamples
{
    /// <summary>The number of 4 MiB parts in the file the upload examples send.</summary>
    private const int UploadPartCount = 4;

    /// <summary>The size of the file the upload examples send: four 4 MiB parts.</summary>
    private const long UploadSizeBytes = 16_777_216;

    /// <summary>The percentage shown before any part of the upload has arrived.</summary>
    private const double EmptyPercent = 0D;

    /// <summary>The percentage shown when the upload is complete.</summary>
    private const double FullPercent = 100D;

    /// <summary>The number of unfinished items before one is finished.</summary>
    private const string ThreeRemainingText = "3";

    /// <summary>The number of unfinished items after one is finished.</summary>
    private const string TwoRemainingText = "2";

    /// <summary>The balance of both accounts, shown with a hint of two decimal places.</summary>
    private const string TotalBalanceTwoPlaces = "17680.75";

    /// <summary>The balance of both accounts, shown with the format hint <c>N0</c>.</summary>
    private const string TotalBalanceGrouped = "17,681";

    /// <summary>The balance of both accounts, shown by the currency converter without a hint.</summary>
    private const string TotalBalanceCurrency = "$17,680.75";

    /// <summary>The balance of both accounts, shown by the currency converter with the hint <c>N0</c>.</summary>
    private const string TotalBalanceCurrencyGrouped = "$17,681";

    /// <summary>The number of decimal places a hint asks for.</summary>
    private const int TwoDecimalPlaces = 2;

    /// <summary>The number format that groups digits and shows no decimals.</summary>
    private const string GroupedFormat = "N0";

    /// <summary>Fills a progress bar from the upload percentage; the values have the type of the property, so they are written as they are.</summary>
    public static void BindUploadPercentToProgressBar()
    {
        var storage = InMemoryObjectStorage.CreateSeeded(ManualClock.StartOfWorkingDay());
        var browser = OpenBucket(storage);
        StorageBrowserView view = new();

        using (browser.WhenChanged(x => x.UploadPercent).BindTo(view, v => v.UploadProgressBar.Value))
        {
            SampleCheck.Equal(EmptyPercent, view.UploadProgressBar.Value);

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

    /// <summary>Shows a count as text: the value type differs from the property type, so the registered converter for the pair runs. Disposing stops the writes.</summary>
    public static void BindRemainingCountToLabel()
    {
        var list = OpenTodoList();
        TodoView view = new();

        using (list.WhenChanged(x => x.RemainingCount).BindTo(view, v => v.RemainingLabel.Text))
        {
            SampleCheck.Equal(ThreeRemainingText, view.RemainingLabel.Text);

            list.SelectedItem = list.Items[0];
            list.CompleteCommand.Execute(null);
            SampleCheck.Equal(TwoRemainingText, view.RemainingLabel.Text);
        }

        list.SelectedItem = list.Items[1];
        list.CompleteCommand.Execute(null);
        SampleCheck.Equal(TwoRemainingText, view.RemainingLabel.Text);
    }

    /// <summary>Passes a conversion hint: the converter reads a whole number as the count of decimal places.</summary>
    public static void BindTotalBalanceWithDecimalPlacesHint()
    {
        var accounts = OpenAccounts();
        AccountsView view = new();

        using (accounts.WhenChanged(x => x.TotalBalance).BindTo(view, v => v.TotalBalanceLabel.Text, TwoDecimalPlaces))
        {
            SampleCheck.Equal(TotalBalanceTwoPlaces, view.TotalBalanceLabel.Text);
        }
    }

    /// <summary>Passes a conversion hint that is a number format. The argument is named because a text in that position would bind to the caller's expression parameter.</summary>
    public static void BindTotalBalanceWithFormatHint()
    {
        var accounts = OpenAccounts();
        AccountsView view = new();

        using (accounts.WhenChanged(x => x.TotalBalance).BindTo(view, v => v.TotalBalanceLabel.Text, conversionHint: GroupedFormat))
        {
            SampleCheck.Equal(TotalBalanceGrouped, view.TotalBalanceLabel.Text);
        }
    }

    /// <summary>Names the converter for this binding, which outranks every converter the library registers.</summary>
    public static void BindTotalBalanceWithConverterOverride()
    {
        var accounts = OpenAccounts();
        AccountsView view = new();

        using (accounts.WhenChanged(x => x.TotalBalance).BindTo(view, v => v.TotalBalanceLabel.Text, new CurrencyTextConverter()))
        {
            SampleCheck.Equal(TotalBalanceCurrency, view.TotalBalanceLabel.Text);
        }
    }

    /// <summary>Names the converter and gives it a hint.</summary>
    public static void BindTotalBalanceWithHintAndConverterOverride()
    {
        var accounts = OpenAccounts();
        AccountsView view = new();

        using (accounts.WhenChanged(x => x.TotalBalance).BindTo(view, v => v.TotalBalanceLabel.Text, GroupedFormat, new CurrencyTextConverter()))
        {
            SampleCheck.Equal(TotalBalanceCurrencyGrouped, view.TotalBalanceLabel.Text);
        }
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

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Infrastructure;
using Splat;

namespace ReactiveUI.Binding.Documentation.BindingsHooks;

/// <summary>Shows how an <see cref="IPropertyBindingHook"/> sees, and can refuse, each binding as it is created.</summary>
public static class BindingsHooksExamples
{
    /// <summary>The name of the everyday account.</summary>
    private const string EverydayName = "Everyday Account";

    /// <summary>A name the customer types for the everyday account.</summary>
    private const string RenamedEverydayName = "Household Account";

    /// <summary>What the recorder writes down for a one-way binding of the accounts screen.</summary>
    private const string OneWayRecord = "OneWay: AccountsViewModel -> AccountNameView";

    /// <summary>What the recorder writes down for a two-way binding of the accounts screen.</summary>
    private const string TwoWayRecord = "TwoWay: AccountsViewModel -> AccountNameView";

    /// <summary>The number of bindings the recorder sees when the four binding methods each create one.</summary>
    private const int FourBindings = 4;

    /// <summary>Creates a one-way binding with no hook registered: <see cref="BindingHooks.Any"/> is false, so the binding asks nobody.</summary>
    public static void BindWithoutHook()
    {
        var accounts = OpenAccounts();
        AccountNameView view = new();

        SampleCheck.Equal(false, BindingHooks.Any);

        using (accounts.BindOneWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
        {
            SampleCheck.Equal(EverydayName, view.NameTextBox.Text);
        }
    }

    /// <summary>Writes down every binding that is created: each of the four binding methods asks the hook once, with its direction.</summary>
    public static void RecordEveryBinding()
    {
        var accounts = OpenAccounts();
        AccountNameView view = new() { ViewModel = accounts };
        BindingRecorderHook recorder = new();

        using (BindingHookScope.Register(recorder))
        {
            SampleCheck.Equal(true, BindingHooks.Any);

            using (accounts.BindOneWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
            using (accounts.BindTwoWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
            using (view.OneWayBind(accounts, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
            using (view.Bind(accounts, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
            {
                SampleCheck.Equal(FourBindings, recorder.Recorded.Count);
            }
        }

        SampleCheck.SequenceEqual([OneWayRecord, TwoWayRecord, OneWayRecord, TwoWayRecord], recorder.Recorded);
    }

    /// <summary>Refuses two-way bindings on the accounts screen: the account is shown, and an edit in the view never reaches it.</summary>
    public static void VetoTwoWayBinding()
    {
        var accounts = OpenAccounts();
        AccountNameView view = new();
        ReadOnlyAccountHook readOnly = new();

        using (BindingHookScope.Register(readOnly))
        {
            using (accounts.BindTwoWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
            {
                // The refused binding is an empty disposable: nothing was written to the view either.
                SampleCheck.Equal(string.Empty, view.NameTextBox.Text);

                view.NameTextBox.Text = RenamedEverydayName;
                SampleCheck.Equal(EverydayName, accounts.SelectedAccount!.Name);
            }

            // One-way bindings pass the hook.
            using (accounts.BindOneWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
            {
                SampleCheck.Equal(EverydayName, view.NameTextBox.Text);
            }

            SampleCheck.Equal(1, readOnly.RefusalCount);
        }

        // With the hook gone the same two-way binding writes the edit to the account.
        using (accounts.BindTwoWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
        {
            view.NameTextBox.Text = RenamedEverydayName;
            SampleCheck.Equal(RenamedEverydayName, accounts.SelectedAccount!.Name);
        }
    }

    /// <summary>A refused view-first binding returns no binding, so the caller receives <see langword="null"/>.</summary>
    public static void VetoViewFirstBinding()
    {
        var accounts = OpenAccounts();
        AccountNameView view = new() { ViewModel = accounts };
        ReadOnlyAccountHook readOnly = new();

        using (BindingHookScope.Register(readOnly))
        {
            var refused = view.Bind(accounts, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text);
            using var allowed = view.OneWayBind(accounts, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text);

            SampleCheck.Equal(true, refused is null);
            SampleCheck.Equal(false, allowed is null);
            SampleCheck.Equal(EverydayName, view.NameTextBox.Text);
        }
    }

    /// <summary>Asks the registered hooks directly. One refusal is enough, and the hooks after it are not asked.</summary>
    public static void AskHooksDirectly()
    {
        var accounts = OpenAccounts();
        AccountNameView view = new();
        BindingRecorderHook firstRecorder = new();
        ReadOnlyAccountHook readOnly = new();
        BindingRecorderHook lastRecorder = new();

        using (BindingHookScope.Register(firstRecorder, readOnly, lastRecorder))
        {
            var oneWay = BindingHooks.ShouldBind(accounts, view, Observed(accounts), Observed(view), BindingDirection.OneWay);
            var twoWay = BindingHooks.ShouldBind(accounts, view, Observed(accounts), Observed(view), BindingDirection.TwoWay);

            SampleCheck.Equal(true, oneWay);
            SampleCheck.Equal(false, twoWay);
            SampleCheck.SequenceEqual([OneWayRecord, TwoWayRecord], firstRecorder.Recorded);
            SampleCheck.SequenceEqual([OneWayRecord], lastRecorder.Recorded);
        }
    }

    /// <summary>Makes a hook registered after the first binding visible: <see cref="BindingHooks"/> reads the registrations once, until <see cref="BindingHooks.Refresh"/>.</summary>
    public static void RefreshAfterLateRegistration()
    {
        var accounts = OpenAccounts();
        AccountNameView view = new();
        BindingRecorderHook recorder = new();

        SampleCheck.Equal(false, BindingHooks.Any);

        AppLocator.CurrentMutable.RegisterConstant<IPropertyBindingHook>(recorder);

        using (accounts.BindOneWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
        {
            SampleCheck.Equal(0, recorder.Recorded.Count);
        }

        BindingHooks.Refresh();

        SampleCheck.Equal(true, BindingHooks.Any);

        using (accounts.BindOneWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
        {
            SampleCheck.Equal(1, recorder.Recorded.Count);
        }

        AppLocator.CurrentMutable.UnregisterAll<IPropertyBindingHook>();
        BindingHooks.Refresh();

        SampleCheck.Equal(false, BindingHooks.Any);
    }

    /// <summary>Reads what a hook is handed: the objects being bound, and a reader for the current values on each side.</summary>
    /// <param name="root">The object whose values the reader returns.</param>
    /// <returns>A function that reads the object as one observed change.</returns>
    private static Func<IObservedChange<object, object>[]> Observed(object root) =>
        () => [new ObservedChange<object, object>(root, null, root)];

    /// <summary>Loads the accounts of the seeded bank and selects the everyday account.</summary>
    /// <returns>The accounts screen with the everyday account selected.</returns>
    private static AccountsViewModel OpenAccounts()
    {
        AccountsViewModel accounts = new(new InMemoryBankingBackend(ManualClock.StartOfWorkingDay()));
        accounts.LoadAccountsCommand.Execute(null);
        accounts.SelectedAccount = accounts.Accounts[0];
        return accounts;
    }
}

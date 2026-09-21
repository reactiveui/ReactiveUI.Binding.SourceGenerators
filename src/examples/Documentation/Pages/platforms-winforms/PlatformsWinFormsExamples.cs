// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.WinForms;
using ReactiveUI.Binding.WinForms.Builder;
using ReactiveUI.Primitives;
using Splat;
using Splat.Builder;

namespace ReactiveUI.Binding.Documentation.PlatformsWinForms;

/// <summary>Shows how ReactiveUI.Binding.WinForms binds real Windows Forms controls: the builder module, <c>{Name}Changed</c> event observation and the control view thread invoker.</summary>
public static class PlatformsWinFormsExamples
{
    /// <summary>The number of changes the filter box is given while it is observed.</summary>
    private const int TwoChanges = 2;

    /// <summary>The amount the customer types into the transfer form.</summary>
    private const string UtilityAmountText = "250.00";

    /// <summary>The amount the customer types into the transfer form, as a number.</summary>
    private const decimal UtilityAmount = 250M;

    /// <summary>The balance of the everyday account after the utility payment.</summary>
    private const decimal EverydayBalanceAfterPayment = 2200.75M;

    /// <summary>The text of an empty amount.</summary>
    private const string ZeroText = "0";

    /// <summary>The text the user types to narrow the to-do list.</summary>
    private const string CarFilter = "car";

    /// <summary>The title of the first seeded to-do item.</summary>
    private const string CarRegistrationTitle = "Renew car registration";

    /// <summary>The property whose change event the examples observe.</summary>
    private const string TextPropertyName = "Text";

    /// <summary>The text a posted callback records.</summary>
    private const string PostedText = "posted";

    /// <summary>The count of items left to do before any is finished.</summary>
    private const string RemainingText = "3";

    /// <summary>The size of the file the upload example sends, in bytes.</summary>
    private const long OffsitePhotoSize = 5_562_368;

    /// <summary>The size of one part of an upload, in bytes.</summary>
    private const long PartSize = 4_194_304;

    /// <summary>The value of a full progress bar.</summary>
    private const int FullPercent = 100;

    /// <summary>The whole-number percentage the progress bar shows after the first part of the upload.</summary>
    private const int FirstPartPercent = (int)((double)PartSize / OffsitePhotoSize * FullPercent);

    /// <summary>The name of the bucket the upload example sends to.</summary>
    private const string MediaBucketName = "acme-media";

    /// <summary>The file the upload example sends.</summary>
    private const string OffsitePhotoName = "team-offsite.jpg";

    /// <summary>The media type of the file the upload example sends.</summary>
    private const string JpegType = "image/jpeg";

    /// <summary>Builds the application with the WinForms module, which adds event-based observation and the control invoker.</summary>
    public static void BuildWinFormsApplication()
    {
        var app = RxBindingBuilder.CreateReactiveUIBindingBuilder()
            .WithCoreServices()
            .WithWinForms()
            .BuildApp();
        ViewThreadInvokers.Refresh();

        SampleCheck.Equal(true, ContainsType<WinFormsCreatesObservableForProperty>(app.Current!.GetServices<ICreatesObservableForProperty>()));
        SampleCheck.Equal(true, ContainsType<ControlViewThreadInvoker>(app.Current!.GetServices<IViewThreadInvoker>()));
    }

    /// <summary>Calls <c>WithWinForms</c> on a builder held as <see cref="IAppBuilder"/>; it returns the same builder.</summary>
    public static void ChainWithWinFormsThroughAppBuilder()
    {
        using ModernDependencyResolver resolver = new();
        IAppBuilder appBuilder = resolver.CreateReactiveUIBindingBuilder();

        var chained = appBuilder.WithWinForms();

        SampleCheck.Equal(true, ReferenceEquals(appBuilder, chained));
    }

    /// <summary>Applies the WinForms module to a resolver you own; it registers the observer and the invoker.</summary>
    public static void ConfigureWinFormsModuleOnResolver()
    {
        using ModernDependencyResolver resolver = new();
        WinFormsBindingModule module = new();

        module.Configure(resolver);

        SampleCheck.Equal(true, ContainsType<WinFormsCreatesObservableForProperty>(resolver.GetServices<ICreatesObservableForProperty>()));
        SampleCheck.Equal(true, ContainsType<ControlViewThreadInvoker>(resolver.GetServices<IViewThreadInvoker>()));
    }

    /// <summary>Asks the observer which properties it handles: a component property with a public <c>{Name}Changed</c> event, and only after the change.</summary>
    public static void DescribeChangedEventAffinity()
    {
        WinFormsCreatesObservableForProperty observer = new();

        SampleCheck.Equal(BindingAffinity.WinFormsEvent, observer.GetAffinityForObject(typeof(TextBox), TextPropertyName, false));
        SampleCheck.Equal(BindingAffinity.WinFormsEvent, observer.GetAffinityForObject(typeof(CheckBox), "Checked", false));
        SampleCheck.Equal(0, observer.GetAffinityForObject(typeof(TextBox), TextPropertyName, true));
        SampleCheck.Equal(0, observer.GetAffinityForObject(typeof(TextBox), "Missing", false));
        SampleCheck.Equal(0, observer.GetAffinityForObject(typeof(TodoItem), "Title", false));
    }

    /// <summary>Observes the filter box through the observer: it raises after each <c>TextChanged</c> and carries no value, so the subscriber reads the property.</summary>
    public static void ObserveTextBoxThroughObserver()
    {
        WinFormsCreatesObservableForProperty observer = new();
        using TodoForm view = new();
        var expression = Expression.Constant(view.FilterTextBox);
        List<string> texts = [];

        using (observer.GetNotificationForProperty(view.FilterTextBox, expression, TextPropertyName, false, false).Subscribe(change => texts.Add(((TextBox)change.Sender).Text)))
        {
            view.FilterTextBox.Text = CarFilter;
            view.FilterTextBox.Text = string.Empty;
        }

        view.FilterTextBox.Text = CarFilter;

        SampleCheck.SequenceEqual([CarFilter, string.Empty], texts);
        SampleCheck.Equal(TwoChanges, texts.Count);
    }

    /// <summary>Rejects an observation of a property the control raises no change event for.</summary>
    public static void RejectPropertyWithoutChangedEvent()
    {
        WinFormsCreatesObservableForProperty observer = new();
        using TodoForm view = new();
        var expression = Expression.Constant(view.FilterTextBox);
        var rejected = false;

        try
        {
            _ = observer.GetNotificationForProperty(view.FilterTextBox, expression, "Missing", false, false);
        }
        catch (ArgumentException)
        {
            rejected = true;
        }

        SampleCheck.Equal(true, rejected);
    }

    /// <summary>Observes the filter box with <c>WhenChanged</c>; the generator reads the <c>TextChanged</c> event at compile time.</summary>
    public static void ObserveFilterBoxWithWhenChanged()
    {
        using TodoForm view = new();
        List<string> texts = [];

        using (view.FilterTextBox.WhenChanged(x => x.Text).Subscribe(texts.Add))
        {
            view.FilterTextBox.Text = CarFilter;
        }

        SampleCheck.SequenceEqual([string.Empty, CarFilter], texts);
    }

    /// <summary>Fills in the transfer form: the amount box is bound both ways, the validation label follows the draft and the button follows the command.</summary>
    public static void BindTransferForm()
    {
        InMemoryBankingBackend backend = new(ManualClock.StartOfWorkingDay());
        TransferViewModel viewModel = new(backend);
        viewModel.LoadCommand.Execute(null);
        using var confirmation = viewModel.ConfirmTransfer.RegisterHandler(static context => context.SetOutput(true));
        using TransferForm view = new() { ViewModel = viewModel };
        viewModel.Draft.Source = viewModel.Accounts[0];
        viewModel.Draft.Payee = viewModel.Payees[0];

        using (viewModel.Draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, new DecimalToStringTypeConverter(), new StringToDecimalTypeConverter()))
        using (viewModel.BindOneWay(view, x => x.IsValid, v => v.ValidationLabel.Visible, static isValid => !isValid))
        using (viewModel.BindOneWay(view, x => x.ValidationSummary, v => v.ValidationLabel.Text))
        using (view.BindCommand(viewModel, x => x.TransferCommand, v => v.TransferButton))
        {
            SampleCheck.Equal(ZeroText, view.AmountTextBox.Text);
            SampleCheck.Equal(true, view.ValidationLabel.Visible);
            SampleCheck.Equal(viewModel.ValidationSummary, view.ValidationLabel.Text);
            SampleCheck.Equal(false, view.TransferButton.Enabled);

            view.AmountTextBox.Text = UtilityAmountText;

            SampleCheck.Equal(UtilityAmount, viewModel.Draft.Amount);
            SampleCheck.Equal(false, view.ValidationLabel.Visible);
            SampleCheck.Equal(string.Empty, view.ValidationLabel.Text);
            SampleCheck.Equal(true, view.TransferButton.Enabled);

            view.TransferButton.PerformClick();

            SampleCheck.Equal(true, viewModel.TransferCommand.Completion.IsCompletedSuccessfully);
            SampleCheck.Equal(EverydayBalanceAfterPayment, viewModel.LastReceipt!.NewBalance);
            SampleCheck.Equal(ZeroText, view.AmountTextBox.Text);
            SampleCheck.Equal(true, view.ValidationLabel.Visible);
        }
    }

    /// <summary>Binds the to-do screen: the filter box and the checkbox both ways, and the count of items left one way.</summary>
    public static void BindTodoFilterAndCheckBox()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        viewModel.LoadCommand.Execute(null);
        using TodoForm view = new() { ViewModel = viewModel };
        var allItems = viewModel.Items.Count;
        viewModel.SelectedItem = viewModel.Items[0];

        using (viewModel.BindTwoWay(view, x => x.FilterText, v => v.FilterTextBox.Text))
        using (viewModel.BindTwoWay(view, x => x.SelectedItem!.IsDone, v => v.DoneCheckBox.Checked))
        using (viewModel.BindOneWay(view, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(System.Globalization.CultureInfo.InvariantCulture)))
        {
            SampleCheck.Equal(RemainingText, view.RemainingLabel.Text);
            SampleCheck.Equal(false, view.DoneCheckBox.Checked);

            view.FilterTextBox.Text = CarFilter;

            SampleCheck.Equal(CarFilter, viewModel.FilterText);
            SampleCheck.Equal(1, viewModel.Items.Count);
            SampleCheck.Equal(true, viewModel.Items.Count < allItems);

            view.DoneCheckBox.Checked = true;

            SampleCheck.Equal(true, viewModel.SelectedItem!.IsDone);
            SampleCheck.Equal(CarRegistrationTitle, viewModel.SelectedItem.Title);
            SampleCheck.Equal("2", view.RemainingLabel.Text);
        }
    }

    /// <summary>Calls the three members of the control invoker on a control that has a handle: who it claims, who may write, and how to queue work on the owning thread.</summary>
    public static void CallInvokerMembers()
    {
        ControlViewThreadInvoker invoker = new();
        using UploadForm view = new();
        var progressBar = view.UploadProgressBar;
        UiThread.CreateHandle(progressBar);
        List<string> log = [];
        object unclaimed = new();
        var accessFromWorker = true;

        UiThread.RunOnWorker(() =>
        {
            accessFromWorker = invoker.CheckAccess(progressBar);
            invoker.Post(progressBar, static state => ((List<string>)state!).Add(PostedText), log);
        });

        SampleCheck.Equal(true, invoker.Claims(progressBar));
        SampleCheck.Equal(false, invoker.Claims(unclaimed));
        SampleCheck.Equal(true, invoker.CheckAccess(progressBar));
        SampleCheck.Equal(false, accessFromWorker);
        SampleCheck.Equal(0, log.Count);

        UiThread.RunQueued();

        SampleCheck.SequenceEqual([PostedText], log);
    }

    /// <summary>Posts to a control that has no handle yet: <c>InvokeRequired</c> is false, so the invoker runs the callback at once.</summary>
    public static void PostToControlWithoutHandleRunsInline()
    {
        ControlViewThreadInvoker invoker = new();
        using UploadForm view = new();
        List<string> log = [];
        var accessFromWorker = false;

        UiThread.RunOnWorker(() => accessFromWorker = invoker.CheckAccess(view.UploadProgressBar));
        invoker.Post(view.UploadProgressBar, static state => ((List<string>)state!).Add(PostedText), log);

        SampleCheck.Equal(true, accessFromWorker);
        SampleCheck.SequenceEqual([PostedText], log);
    }

    /// <summary>Rejects a missing callback.</summary>
    public static void RejectMissingCallback()
    {
        ControlViewThreadInvoker invoker = new();
        using UploadForm view = new();
        var rejected = false;

        try
        {
            invoker.Post(view.UploadProgressBar, null!, null);
        }
        catch (ArgumentNullException)
        {
            rejected = true;
        }

        SampleCheck.Equal(true, rejected);
    }

    /// <summary>Shows that Windows Forms refuses a write from a thread that does not own the control, once cross-thread checks are on. A binding has to avoid this.</summary>
    public static void RefuseWriteFromWorkerThread()
    {
        using UploadForm view = new();
        UiThread.CreateHandle(view.UploadProgressBar);
        var refused = false;
        Control.CheckForIllegalCrossThreadCalls = true;

        UiThread.RunOnWorker(() =>
        {
            try
            {
                view.UploadProgressBar.Value = FullPercent;
            }
            catch (InvalidOperationException)
            {
                refused = true;
            }
        });

        SampleCheck.Equal(true, refused);
    }

    /// <summary>Follows an upload whose progress arrives on a worker thread. No module is registered, so the generated binding carries its own WinForms invoker.</summary>
    public static void WriteProgressFromWorkerThreadWithoutModule()
    {
        using UploadForm view = new();

        SampleCheck.Equal(true, ViewThreadInvokers.ForTarget(view.UploadProgressBar) is null);

        RunUploadFromWorkerThread(view);
    }

    /// <summary>Follows the same upload after <see cref="BuildWinFormsApplication"/>: the registered control invoker claims the bar.</summary>
    public static void WriteProgressFromWorkerThreadThroughRegisteredInvoker()
    {
        using UploadForm view = new();

        SampleCheck.Equal(true, ViewThreadInvokers.ForTarget(view.UploadProgressBar) is ControlViewThreadInvoker);

        RunUploadFromWorkerThread(view);
    }

    /// <summary>Checks whether a sequence of services holds an instance of a type.</summary>
    /// <typeparam name="T">The type to look for.</typeparam>
    /// <param name="services">The services to search.</param>
    /// <returns><see langword="true"/> when one of the services is a <typeparamref name="T"/>.</returns>
    private static bool ContainsType<T>(IEnumerable<object> services)
    {
        foreach (var service in services)
        {
            if (service is T)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Uploads a photo while a worker thread releases the storage service; each progress report reaches the bar when the owning thread runs its message queue.</summary>
    /// <param name="view">The form whose bar follows the upload.</param>
    private static void RunUploadFromWorkerThread(UploadForm view)
    {
        var storage = InMemoryObjectStorage.CreateSeeded(ManualClock.StartOfWorkingDay());
        StorageBrowserViewModel viewModel = new(storage);
        viewModel.LoadBucketsCommand.Execute(null);
        viewModel.SelectedBucket = viewModel.Buckets[0];
        UploadRequest photo = new(OffsitePhotoName, OffsitePhotoSize, JpegType);
        UiThread.CreateHandle(view);

        using (viewModel.BindOneWay(view, x => x.UploadPercent, v => v.UploadProgressBar.Value, static percent => (int)percent))
        {
            storage.Gate.Hold();
            viewModel.UploadCommand.Execute(photo);

            UiThread.RunOnWorker(() =>
            {
                storage.Gate.ReleaseNext();
                storage.Gate.ReleaseNext();
            });

            SampleCheck.Equal(0, view.UploadProgressBar.Value);

            UiThread.RunQueued();

            SampleCheck.Equal(FirstPartPercent, view.UploadProgressBar.Value);

            UiThread.RunOnWorker(storage.Gate.ReleaseAll);
            UiThread.RunQueued();

            SampleCheck.Equal(true, viewModel.UploadCommand.Completion.IsCompletedSuccessfully);
            SampleCheck.Equal(FullPercent, view.UploadProgressBar.Value);
            SampleCheck.Equal(MediaBucketName, viewModel.SelectedBucket!.Name);
        }
    }
}

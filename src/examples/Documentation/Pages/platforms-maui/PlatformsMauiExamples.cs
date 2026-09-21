// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using Microsoft.Maui;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Maui;
using ReactiveUI.Binding.Maui.Builder;
using ReactiveUI.Primitives;
using Splat;
using Splat.Builder;

namespace ReactiveUI.Binding.Documentation.PlatformsMaui;

/// <summary>Shows how ReactiveUI.Binding.Maui binds real MAUI controls: the builder module, the Visibility converters and the dispatcher view thread invoker.</summary>
public static class PlatformsMauiExamples
{
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

    /// <summary>The text a posted callback records.</summary>
    private const string PostedText = "posted";

    /// <summary>The count of items left to do before any is finished.</summary>
    private const string RemainingText = "3";

    /// <summary>The size of the file the upload example sends, in bytes.</summary>
    private const long OffsitePhotoSize = 5_562_368;

    /// <summary>The value of a full progress bar, in percent.</summary>
    private const double FullPercent = 100;

    /// <summary>The number of decimal places the progress fraction is compared to.</summary>
    private const int ProgressDigits = 3;

    /// <summary>How much of the upload the service has received after its first part, as a fraction from 0 to 1 rounded to <see cref="ProgressDigits"/> places.</summary>
    private const double FirstPartFraction = 0.754;

    /// <summary>The name of the bucket the upload example sends to.</summary>
    private const string MediaBucketName = "acme-media";

    /// <summary>The file the upload example sends.</summary>
    private const string OffsitePhotoName = "team-offsite.jpg";

    /// <summary>The media type of the file the upload example sends.</summary>
    private const string JpegType = "image/jpeg";

    /// <summary>Builds the application with the MAUI module, which registers the Visibility converters with the resolver; importing them makes them available to bindings.</summary>
    public static void BuildMauiApplication()
    {
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var app = builder.WithCoreServices().WithMaui().BuildApp();
        ViewThreadInvokers.Refresh();

        SampleCheck.Equal(true, ContainsType<DispatcherViewThreadInvoker>(app.Current!.GetServices<IViewThreadInvoker>()));
        SampleCheck.Equal(true, ContainsType<BooleanToVisibilityTypeConverter>(app.Current!.GetServices<IBindingTypeConverter>()));
        SampleCheck.Equal(true, ContainsType<VisibilityToBooleanTypeConverter>(app.Current!.GetServices<IBindingTypeConverter>()));
        SampleCheck.Equal(true, builder.ConverterService.TypedConverters.TryGetConverter(typeof(bool), typeof(Visibility)) is null);

        builder.ConverterService.ImportFrom(app.Current!);

        SampleCheck.Equal(true, BindingConverters.Current.TypedConverters.TryGetConverter(typeof(bool), typeof(Visibility)) is BooleanToVisibilityTypeConverter);
        SampleCheck.Equal(true, BindingConverters.Current.TypedConverters.TryGetConverter(typeof(Visibility), typeof(bool)) is VisibilityToBooleanTypeConverter);
    }

    /// <summary>Calls <c>WithMaui</c> on a builder held as <see cref="IAppBuilder"/>; it returns the same builder.</summary>
    public static void ChainWithMauiThroughAppBuilder()
    {
        using ModernDependencyResolver resolver = new();
        IAppBuilder appBuilder = resolver.CreateReactiveUIBindingBuilder();

        var chained = appBuilder.WithMaui();

        SampleCheck.Equal(true, ReferenceEquals(appBuilder, chained));
    }

    /// <summary>Applies the MAUI module to a resolver you own; it registers the invoker and both converters.</summary>
    public static void ConfigureMauiModuleOnResolver()
    {
        using ModernDependencyResolver resolver = new();
        MauiBindingModule module = new();

        module.Configure(resolver);

        SampleCheck.Equal(true, ContainsType<DispatcherViewThreadInvoker>(resolver.GetServices<IViewThreadInvoker>()));
        SampleCheck.Equal(true, ContainsType<BooleanToVisibilityTypeConverter>(resolver.GetServices<IBindingTypeConverter>()));
        SampleCheck.Equal(true, ContainsType<VisibilityToBooleanTypeConverter>(resolver.GetServices<IBindingTypeConverter>()));
    }

    /// <summary>Converts the validation state to a <see cref="Visibility"/> with each hint.</summary>
    public static void ConvertBooleanToVisibility()
    {
        BooleanToVisibilityTypeConverter converter = new();

        SampleCheck.Equal(BindingAffinity.DefaultInternalTypeConverter, converter.GetAffinityForObjects());
        SampleCheck.Equal(Visibility.Visible, ConvertWith(converter, true, null));
        SampleCheck.Equal(Visibility.Collapsed, ConvertWith(converter, false, null));
        SampleCheck.Equal(Visibility.Collapsed, ConvertWith(converter, false, BooleanToVisibilityHints.None));
        SampleCheck.Equal(Visibility.Collapsed, ConvertWith(converter, true, BooleanToVisibilityHints.Inverse));
        SampleCheck.Equal(Visibility.Visible, ConvertWith(converter, false, BooleanToVisibilityHints.Inverse));
        SampleCheck.Equal(Visibility.Hidden, ConvertWith(converter, false, BooleanToVisibilityHints.UseHidden));
        SampleCheck.Equal(Visibility.Visible, ConvertWith(converter, true, BooleanToVisibilityHints.UseHidden));
        SampleCheck.Equal(Visibility.Hidden, ConvertWith(converter, true, BooleanToVisibilityHints.Inverse | BooleanToVisibilityHints.UseHidden));
        SampleCheck.Equal(Visibility.Collapsed, ConvertWith(converter, false, "not a hint"));
    }

    /// <summary>Converts a <see cref="Visibility"/> back to a boolean; only Visible is true, and the inverse hint flips the result.</summary>
    public static void ConvertVisibilityToBoolean()
    {
        VisibilityToBooleanTypeConverter converter = new();

        SampleCheck.Equal(BindingAffinity.DefaultInternalTypeConverter, converter.GetAffinityForObjects());
        SampleCheck.Equal(true, ConvertWith(converter, Visibility.Visible, null));
        SampleCheck.Equal(false, ConvertWith(converter, Visibility.Hidden, null));
        SampleCheck.Equal(false, ConvertWith(converter, Visibility.Collapsed, BooleanToVisibilityHints.None));
        SampleCheck.Equal(false, ConvertWith(converter, Visibility.Visible, BooleanToVisibilityHints.Inverse));
        SampleCheck.Equal(true, ConvertWith(converter, Visibility.Collapsed, BooleanToVisibilityHints.Inverse));
        SampleCheck.Equal(false, ConvertWith(converter, Visibility.Collapsed, "not a hint"));
    }

    /// <summary>Observes the filter entry with <c>WhenChanged</c>; a MAUI control is a <c>BindableObject</c>, which raises <c>PropertyChanged</c>.</summary>
    public static void ObserveFilterEntryWithWhenChanged()
    {
        TodoPage view = new();
        List<string?> texts = [];

        using (view.FilterEntry.WhenChanged(x => x.Text).Subscribe(texts.Add))
        {
            view.FilterEntry.Text = CarFilter;
        }

        SampleCheck.SequenceEqual([null, CarFilter], texts);
    }

    /// <summary>Fills in the transfer form: the amount entry is bound both ways, the validation label shows through the Visibility converter and the button follows the command.</summary>
    public static void BindTransferForm()
    {
        InMemoryBankingBackend backend = new(ManualClock.StartOfWorkingDay());
        TransferViewModel viewModel = new(backend);
        viewModel.LoadCommand.Execute(null);
        using var confirmation = viewModel.ConfirmTransfer.RegisterHandler(static context => context.SetOutput(true));
        TransferPage view = new() { ViewModel = viewModel };
        viewModel.Draft.Source = viewModel.Accounts[0];
        viewModel.Draft.Payee = viewModel.Payees[0];

        using (viewModel.Draft.BindTwoWay(view, x => x.Amount, v => v.AmountEntry.Text, new DecimalToStringTypeConverter(), new StringToDecimalTypeConverter()))
        using (view.OneWayBind(viewModel, x => x.IsValid, v => v.ValidationVisibility, new BooleanToVisibilityTypeConverter(), BooleanToVisibilityHints.Inverse))
        using (viewModel.BindOneWay(view, x => x.ValidationSummary, v => v.ValidationLabel.Text))
        using (view.BindCommand(viewModel, x => x.TransferCommand, v => v.TransferButton))
        {
            SampleCheck.Equal(ZeroText, view.AmountEntry.Text);
            SampleCheck.Equal(Visibility.Visible, view.ValidationVisibility);
            SampleCheck.Equal(true, view.ValidationLabel.IsVisible);
            SampleCheck.Equal(viewModel.ValidationSummary, view.ValidationLabel.Text);
            SampleCheck.Equal(false, view.TransferButton.IsEnabled);

            view.AmountEntry.Text = UtilityAmountText;

            SampleCheck.Equal(UtilityAmount, viewModel.Draft.Amount);
            SampleCheck.Equal(Visibility.Collapsed, view.ValidationVisibility);
            SampleCheck.Equal(false, view.ValidationLabel.IsVisible);
            SampleCheck.Equal(true, view.TransferButton.IsEnabled);

            ((IButtonController)view.TransferButton).SendClicked();

            SampleCheck.Equal(true, viewModel.TransferCommand.Completion.IsCompletedSuccessfully);
            SampleCheck.Equal(EverydayBalanceAfterPayment, viewModel.LastReceipt!.NewBalance);
            SampleCheck.Equal(ZeroText, view.AmountEntry.Text);
            SampleCheck.Equal(Visibility.Visible, view.ValidationVisibility);
        }
    }

    /// <summary>Binds the to-do screen: the filter entry and the checkbox both ways, and the count of items left one way.</summary>
    public static void BindTodoFilterAndCheckBox()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        viewModel.LoadCommand.Execute(null);
        TodoPage view = new() { ViewModel = viewModel };
        var allItems = viewModel.Items.Count;
        viewModel.SelectedItem = viewModel.Items[0];

        using (viewModel.BindTwoWay(view, x => x.FilterText, v => v.FilterEntry.Text))
        using (viewModel.BindTwoWay(view, x => x.SelectedItem!.IsDone, v => v.DoneCheckBox.IsChecked))
        using (viewModel.BindOneWay(view, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture)))
        {
            SampleCheck.Equal(RemainingText, view.RemainingLabel.Text);
            SampleCheck.Equal(false, view.DoneCheckBox.IsChecked);

            view.FilterEntry.Text = CarFilter;

            SampleCheck.Equal(CarFilter, viewModel.FilterText);
            SampleCheck.Equal(1, viewModel.Items.Count);
            SampleCheck.Equal(true, viewModel.Items.Count < allItems);

            view.DoneCheckBox.IsChecked = true;

            SampleCheck.Equal(true, viewModel.SelectedItem!.IsDone);
            SampleCheck.Equal(CarRegistrationTitle, viewModel.SelectedItem.Title);
            SampleCheck.Equal("2", view.RemainingLabel.Text);
        }
    }

    /// <summary>Binds a tick mark both ways through the two Visibility converters: the item shows the mark, and hiding the mark reopens the item.</summary>
    public static void BindDoneMarkWithBothConverters()
    {
        TodoItem item = new() { Title = CarRegistrationTitle };
        TodoPage view = new();

        using (item.BindTwoWay(view, x => x.IsDone, v => v.DoneMarkVisibility, new BooleanToVisibilityTypeConverter(), new VisibilityToBooleanTypeConverter()))
        {
            SampleCheck.Equal(Visibility.Collapsed, view.DoneMarkVisibility);
            SampleCheck.Equal(false, view.DoneMark.IsVisible);

            item.IsDone = true;

            SampleCheck.Equal(Visibility.Visible, view.DoneMarkVisibility);
            SampleCheck.Equal(true, view.DoneMark.IsVisible);

            view.DoneMarkVisibility = Visibility.Hidden;

            SampleCheck.Equal(false, item.IsDone);
        }
    }

    /// <summary>Calls the three members of the dispatcher invoker: who it claims, who may write, and how to queue work on the owning thread.</summary>
    public static void CallInvokerMembers()
    {
        using var dispatcher = QueueDispatcher.Install();
        DispatcherViewThreadInvoker invoker = new();
        UploadPage view = new();
        var progressBar = view.UploadProgressBar;
        List<string> log = [];
        object unclaimed = new();
        var accessFromWorker = true;

        RunOnWorker(() =>
        {
            accessFromWorker = invoker.CheckAccess(progressBar);
            invoker.Post(progressBar, static state => ((List<string>)state!).Add(PostedText), log);
        });

        SampleCheck.Equal(true, invoker.Claims(progressBar));
        SampleCheck.Equal(false, invoker.Claims(unclaimed));
        SampleCheck.Equal(true, invoker.CheckAccess(progressBar));
        SampleCheck.Equal(false, accessFromWorker);
        SampleCheck.Equal(0, log.Count);
        SampleCheck.Equal(1, dispatcher.QueuedCount);

        _ = dispatcher.RunQueued();

        SampleCheck.SequenceEqual([PostedText], log);
    }

    /// <summary>Uses the invoker where no dispatcher exists, as in a view's unit test: MAUI throws when asked for one, so the invoker lets any thread write and posts inline.</summary>
    public static void PostWithoutDispatcherRunsInline()
    {
        DispatcherViewThreadInvoker invoker = new();
        UploadPage view = new();
        List<string> log = [];
        var accessFromWorker = false;

        RunOnWorker(() => accessFromWorker = invoker.CheckAccess(view.UploadProgressBar));
        invoker.Post(view.UploadProgressBar, static state => ((List<string>)state!).Add(PostedText), log);

        SampleCheck.Equal(true, accessFromWorker);
        SampleCheck.SequenceEqual([PostedText], log);
    }

    /// <summary>Rejects a missing callback.</summary>
    public static void RejectMissingCallback()
    {
        DispatcherViewThreadInvoker invoker = new();
        UploadPage view = new();
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

    /// <summary>Follows an upload whose progress arrives on a worker thread. No module is registered, so the generated binding carries its own MAUI invoker.</summary>
    public static void WriteProgressFromWorkerThreadWithoutModule()
    {
        using var dispatcher = QueueDispatcher.Install();
        UploadPage view = new();

        SampleCheck.Equal(true, ViewThreadInvokers.ForTarget(view.UploadProgressBar) is null);

        RunUploadFromWorkerThread(view, dispatcher);
    }

    /// <summary>Follows the same upload after <see cref="BuildMauiApplication"/>: the registered dispatcher invoker claims the bar.</summary>
    public static void WriteProgressFromWorkerThreadThroughRegisteredInvoker()
    {
        using var dispatcher = QueueDispatcher.Install();
        UploadPage view = new();

        SampleCheck.Equal(true, ViewThreadInvokers.ForTarget(view.UploadProgressBar) is DispatcherViewThreadInvoker);

        RunUploadFromWorkerThread(view, dispatcher);
    }

    /// <summary>Runs work on a worker thread and waits for it to finish.</summary>
    /// <param name="work">The work to run.</param>
    private static void RunOnWorker(Action work)
    {
        Thread worker = new(work.Invoke);
        worker.Start();
        worker.Join();
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

    /// <summary>Converts a value with a typed converter and returns the result.</summary>
    /// <typeparam name="TFrom">The type converted from.</typeparam>
    /// <typeparam name="TTo">The type converted to.</typeparam>
    /// <param name="converter">The converter to call.</param>
    /// <param name="from">The value to convert.</param>
    /// <param name="hint">The conversion hint, or <see langword="null"/>.</param>
    /// <returns>The converted value.</returns>
    private static TTo? ConvertWith<TFrom, TTo>(BindingTypeConverter<TFrom, TTo> converter, TFrom from, object? hint)
    {
        var converted = converter.TryConvert(from, hint, out var result);

        SampleCheck.Equal(true, converted);
        return result;
    }

    /// <summary>Uploads a photo while a worker thread releases the storage service; each progress report reaches the bar when the owning thread runs its queue.</summary>
    /// <param name="view">The page whose bar follows the upload.</param>
    /// <param name="dispatcher">The dispatcher of the owning thread.</param>
    private static void RunUploadFromWorkerThread(UploadPage view, QueueDispatcher dispatcher)
    {
        var storage = InMemoryObjectStorage.CreateSeeded(ManualClock.StartOfWorkingDay());
        StorageBrowserViewModel viewModel = new(storage);
        viewModel.LoadBucketsCommand.Execute(null);
        viewModel.SelectedBucket = viewModel.Buckets[0];
        UploadRequest photo = new(OffsitePhotoName, OffsitePhotoSize, JpegType);

        using (viewModel.BindOneWay(view, x => x.UploadPercent, v => v.UploadProgressBar.Progress, static percent => percent / FullPercent))
        {
            storage.Gate.Hold();
            viewModel.UploadCommand.Execute(photo);

            RunOnWorker(() =>
            {
                storage.Gate.ReleaseNext();
                storage.Gate.ReleaseNext();
            });

            SampleCheck.Equal(0D, view.UploadProgressBar.Progress);
            SampleCheck.Equal(1, dispatcher.QueuedCount);

            _ = dispatcher.RunQueued();

            SampleCheck.Equal(FirstPartFraction, Math.Round(view.UploadProgressBar.Progress, ProgressDigits));

            RunOnWorker(storage.Gate.ReleaseAll);
            _ = dispatcher.RunQueued();

            SampleCheck.Equal(true, viewModel.UploadCommand.Completion.IsCompletedSuccessfully);
            SampleCheck.Equal(1D, view.UploadProgressBar.Progress);
            SampleCheck.Equal(MediaBucketName, viewModel.SelectedBucket!.Name);
        }
    }
}

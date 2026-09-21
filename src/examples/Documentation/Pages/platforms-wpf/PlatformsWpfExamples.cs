// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Wpf;
using ReactiveUI.Binding.Wpf.Builder;
using ReactiveUI.Primitives;
using Splat;
using Splat.Builder;

namespace ReactiveUI.Binding.Documentation.PlatformsWpf;

/// <summary>Shows how ReactiveUI.Binding.Wpf binds real WPF controls: the builder module, dependency-property observation, the Visibility converters and the dispatcher view thread invoker.</summary>
public static class PlatformsWpfExamples
{
    /// <summary>The number of dependency-property changes the box below is given.</summary>
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

    /// <summary>The property whose dependency property the examples observe.</summary>
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
    private const double FullPercent = 100;

    /// <summary>How much of the upload the service has received after its first part.</summary>
    private const double FirstPartPercent = (double)PartSize / OffsitePhotoSize * FullPercent;

    /// <summary>The name of the bucket the upload example sends to.</summary>
    private const string MediaBucketName = "acme-media";

    /// <summary>The file the upload example sends.</summary>
    private const string OffsitePhotoName = "team-offsite.jpg";

    /// <summary>The media type of the file the upload example sends.</summary>
    private const string JpegType = "image/jpeg";

    /// <summary>Builds the application with the WPF module, which adds no Visibility converter, and registers both converters.</summary>
    public static void BuildWpfApplication()
    {
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices().WithWpf();

        SampleCheck.Equal(true, builder.ConverterService.TypedConverters.TryGetConverter(typeof(bool), typeof(Visibility)) is null);
        SampleCheck.Equal(true, builder.ConverterService.TypedConverters.TryGetConverter(typeof(Visibility), typeof(bool)) is null);

        var app = builder
            .WithConverter(new BooleanToVisibilityTypeConverter())
            .WithConverter(new VisibilityToBooleanTypeConverter())
            .BuildApp();
        ViewThreadInvokers.Refresh();

        var observers = app.Current!.GetServices<ICreatesObservableForProperty>();
        var invokers = app.Current!.GetServices<IViewThreadInvoker>();

        SampleCheck.Equal(true, ContainsType<DependencyObjectObservableForProperty>(observers));
        SampleCheck.Equal(true, ContainsType<DispatcherViewThreadInvoker>(invokers));
        SampleCheck.Equal(true, BindingConverters.Current.TypedConverters.TryGetConverter(typeof(bool), typeof(Visibility)) is BooleanToVisibilityTypeConverter);
        SampleCheck.Equal(true, BindingConverters.Current.TypedConverters.TryGetConverter(typeof(Visibility), typeof(bool)) is VisibilityToBooleanTypeConverter);
    }

    /// <summary>Calls <c>WithWpf</c> on a builder held as <see cref="IAppBuilder"/>; it returns the same builder.</summary>
    public static void ChainWithWpfThroughAppBuilder()
    {
        using ModernDependencyResolver resolver = new();
        IAppBuilder appBuilder = resolver.CreateReactiveUIBindingBuilder();

        var chained = appBuilder.WithWpf();

        SampleCheck.Equal(true, ReferenceEquals(appBuilder, chained));
    }

    /// <summary>Applies the WPF module to a resolver you own; it registers the observer and the invoker.</summary>
    public static void ConfigureWpfModuleOnResolver()
    {
        using ModernDependencyResolver resolver = new();
        WpfBindingModule module = new();

        module.Configure(resolver);

        SampleCheck.Equal(true, ContainsType<DependencyObjectObservableForProperty>(resolver.GetServices<ICreatesObservableForProperty>()));
        SampleCheck.Equal(true, ContainsType<DispatcherViewThreadInvoker>(resolver.GetServices<IViewThreadInvoker>()));
    }

    /// <summary>Asks the provider which properties it observes: a <c>DependencyObject</c> that declares the dependency property, and nothing else.</summary>
    public static void DescribeDependencyPropertyAffinity()
    {
        DependencyObjectObservableForProperty provider = new();

        SampleCheck.Equal(BindingAffinity.WpfDependencyObject, provider.GetAffinityForObject(typeof(TextBox), TextPropertyName, false));
        SampleCheck.Equal(BindingAffinity.WpfDependencyObject, provider.GetAffinityForObject(typeof(TextBox), TextPropertyName, true));
        SampleCheck.Equal(0, provider.GetAffinityForObject(typeof(TextBox), "Missing", false));
        SampleCheck.Equal(0, provider.GetAffinityForObject(typeof(TodoItem), "Title", false));
    }

    /// <summary>Observes the filter box through the provider: it raises after each change and carries no value, so the observer reads the property.</summary>
    public static void ObserveTextBoxThroughProvider()
    {
        DependencyObjectObservableForProperty provider = new();
        TodoWindow view = new();
        var expression = System.Linq.Expressions.Expression.Constant(view.FilterTextBox);
        List<string> texts = [];

        using (provider.GetNotificationForProperty(view.FilterTextBox, expression, TextPropertyName, false, false).Subscribe(change => texts.Add(((TextBox)change.Sender).Text)))
        {
            view.FilterTextBox.Text = CarFilter;
            view.FilterTextBox.Text = string.Empty;
        }

        view.FilterTextBox.Text = CarFilter;

        SampleCheck.SequenceEqual([CarFilter, string.Empty], texts);
        SampleCheck.Equal(TwoChanges, texts.Count);
    }

    /// <summary>Rejects an observation of a property the control has no dependency property for.</summary>
    public static void RejectPropertyWithoutDependencyProperty()
    {
        DependencyObjectObservableForProperty provider = new();
        TodoWindow view = new();
        var expression = System.Linq.Expressions.Expression.Constant(view.FilterTextBox);
        var rejected = false;

        try
        {
            _ = provider.GetNotificationForProperty(view.FilterTextBox, expression, "Missing", false, false);
        }
        catch (ArgumentException)
        {
            rejected = true;
        }

        SampleCheck.Equal(true, rejected);
    }

    /// <summary>Observes the filter box with <c>WhenChanged</c>; the generator reads the dependency property at compile time.</summary>
    public static void ObserveFilterBoxWithWhenChanged()
    {
        TodoWindow view = new();
        List<string> texts = [];

        using (view.FilterTextBox.WhenChanged(x => x.Text).Subscribe(texts.Add))
        {
            view.FilterTextBox.Text = CarFilter;
        }

        SampleCheck.SequenceEqual([string.Empty, CarFilter], texts);
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

    /// <summary>Fills in the transfer form: the amount box is bound both ways, the validation label shows through the Visibility converter and the button follows the command.</summary>
    public static void BindTransferForm()
    {
        InMemoryBankingBackend backend = new(ManualClock.StartOfWorkingDay());
        TransferViewModel viewModel = new(backend);
        viewModel.LoadCommand.Execute(null);
        using var confirmation = viewModel.ConfirmTransfer.RegisterHandler(static context => context.SetOutput(true));
        TransferWindow view = new() { ViewModel = viewModel };
        viewModel.Draft.Source = viewModel.Accounts[0];
        viewModel.Draft.Payee = viewModel.Payees[0];

        using (viewModel.Draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, new DecimalToStringTypeConverter(), new StringToDecimalTypeConverter()))
        using (view.OneWayBind(viewModel, x => x.IsValid, v => v.ValidationLabel.Visibility, new BooleanToVisibilityTypeConverter(), BooleanToVisibilityHints.Inverse))
        using (view.OneWayBind(viewModel, x => x.ValidationSummary, v => v.ValidationLabel.Text))
        using (view.BindCommand(viewModel, x => x.TransferCommand, v => v.TransferButton))
        {
            SampleCheck.Equal(ZeroText, view.AmountTextBox.Text);
            SampleCheck.Equal(Visibility.Visible, view.ValidationLabel.Visibility);
            SampleCheck.Equal(viewModel.ValidationSummary, view.ValidationLabel.Text);
            SampleCheck.Equal(false, view.TransferButton.IsEnabled);

            view.AmountTextBox.Text = UtilityAmountText;

            SampleCheck.Equal(UtilityAmount, viewModel.Draft.Amount);
            SampleCheck.Equal(Visibility.Collapsed, view.ValidationLabel.Visibility);
            SampleCheck.Equal(string.Empty, view.ValidationLabel.Text);
            SampleCheck.Equal(true, view.TransferButton.IsEnabled);

            UiThread.Press(view.TransferButton);

            SampleCheck.Equal(true, viewModel.TransferCommand.Completion.IsCompletedSuccessfully);
            SampleCheck.Equal(EverydayBalanceAfterPayment, viewModel.LastReceipt!.NewBalance);
            SampleCheck.Equal(ZeroText, view.AmountTextBox.Text);
            SampleCheck.Equal(Visibility.Visible, view.ValidationLabel.Visibility);
        }
    }

    /// <summary>Binds the to-do screen: the filter box and the checkbox both ways, and the count of items left one way.</summary>
    public static void BindTodoFilterAndCheckBox()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        viewModel.LoadCommand.Execute(null);
        TodoWindow view = new() { ViewModel = viewModel };
        var allItems = viewModel.Items.Count;
        viewModel.SelectedItem = viewModel.Items[0];

        using (viewModel.BindTwoWay(view, x => x.FilterText, v => v.FilterTextBox.Text))
        using (viewModel.BindTwoWay(view, x => x.SelectedItem!.IsDone, v => v.DoneCheckBox.IsChecked, static done => done, static isChecked => isChecked == true))
        using (viewModel.BindOneWay(view, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture)))
        {
            SampleCheck.Equal(RemainingText, view.RemainingLabel.Text);
            SampleCheck.Equal(false, view.DoneCheckBox.IsChecked);

            view.FilterTextBox.Text = CarFilter;

            SampleCheck.Equal(CarFilter, viewModel.FilterText);
            SampleCheck.Equal(1, viewModel.Items.Count);
            SampleCheck.Equal(true, viewModel.Items.Count < allItems);

            view.DoneCheckBox.IsChecked = true;

            SampleCheck.Equal(true, viewModel.SelectedItem!.IsDone);
            SampleCheck.Equal(CarRegistrationTitle, viewModel.SelectedItem.Title);
            SampleCheck.Equal("2", view.RemainingLabel.Text);
        }
    }

    /// <summary>Binds a tick mark both ways through the converters the application registered: the item shows the mark, and hiding the mark reopens the item.</summary>
    public static void BindDoneMarkWithBothConverters()
    {
        TodoItem item = new() { Title = CarRegistrationTitle };
        TodoWindow view = new();

        using (item.BindTwoWay(view, x => x.IsDone, v => v.DoneMark.Visibility, new BooleanToVisibilityTypeConverter(), new VisibilityToBooleanTypeConverter()))
        {
            SampleCheck.Equal(Visibility.Collapsed, view.DoneMark.Visibility);

            item.IsDone = true;

            SampleCheck.Equal(Visibility.Visible, view.DoneMark.Visibility);

            view.DoneMark.Visibility = Visibility.Hidden;

            SampleCheck.Equal(false, item.IsDone);
        }
    }

    /// <summary>Shows that WPF refuses a write from a thread that does not own the control. A binding has to avoid this.</summary>
    public static void RefuseWriteFromWorkerThread()
    {
        UploadWindow view = new();
        var refused = false;

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
        SampleCheck.Equal(0D, view.UploadProgressBar.Value);
    }

    /// <summary>Calls the three members of the dispatcher invoker: who it claims, who may write, and how to queue work on the owning thread.</summary>
    public static void CallInvokerMembers()
    {
        DispatcherViewThreadInvoker invoker = new();
        UploadWindow view = new();
        var progressBar = view.UploadProgressBar;
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

        UiThread.RunQueued(progressBar.Dispatcher);

        SampleCheck.SequenceEqual([PostedText], log);
    }

    /// <summary>Posts to a frozen brush, which belongs to no thread: the invoker runs the callback at once.</summary>
    public static void PostToFrozenBrushRunsInline()
    {
        DispatcherViewThreadInvoker invoker = new();
        SolidColorBrush brush = new(Colors.Green);
        brush.Freeze();
        List<string> log = [];

        invoker.Post(brush, static state => ((List<string>)state!).Add(PostedText), log);

        SampleCheck.SequenceEqual([PostedText], log);
    }

    /// <summary>Rejects a missing callback.</summary>
    public static void RejectMissingCallback()
    {
        DispatcherViewThreadInvoker invoker = new();
        UploadWindow view = new();
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

    /// <summary>Follows an upload whose progress arrives on a worker thread. No module is registered, so the generated binding carries its own WPF invoker.</summary>
    public static void WriteProgressFromWorkerThreadWithoutModule()
    {
        UploadWindow view = new();

        SampleCheck.Equal(true, ViewThreadInvokers.ForTarget(view.UploadProgressBar) is null);

        RunUploadFromWorkerThread(view);
    }

    /// <summary>Follows the same upload after <see cref="BuildWpfApplication"/>: the registered dispatcher invoker claims the bar.</summary>
    public static void WriteProgressFromWorkerThreadThroughRegisteredInvoker()
    {
        UploadWindow view = new();

        SampleCheck.Equal(true, ViewThreadInvokers.ForTarget(view.UploadProgressBar) is DispatcherViewThreadInvoker);

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
    /// <param name="view">The window whose bar follows the upload.</param>
    private static void RunUploadFromWorkerThread(UploadWindow view)
    {
        var storage = InMemoryObjectStorage.CreateSeeded(ManualClock.StartOfWorkingDay());
        StorageBrowserViewModel viewModel = new(storage);
        viewModel.LoadBucketsCommand.Execute(null);
        viewModel.SelectedBucket = viewModel.Buckets[0];
        UploadRequest photo = new(OffsitePhotoName, OffsitePhotoSize, JpegType);

        using (viewModel.BindOneWay(view, x => x.UploadPercent, v => v.UploadProgressBar.Value))
        {
            storage.Gate.Hold();
            viewModel.UploadCommand.Execute(photo);

            UiThread.RunOnWorker(() =>
            {
                storage.Gate.ReleaseNext();
                storage.Gate.ReleaseNext();
            });

            SampleCheck.Equal(FirstPartPercent, viewModel.UploadPercent);
            SampleCheck.Equal(0D, view.UploadProgressBar.Value);

            UiThread.RunQueued(view.UploadProgressBar.Dispatcher);

            SampleCheck.Equal(FirstPartPercent, view.UploadProgressBar.Value);

            UiThread.RunOnWorker(storage.Gate.ReleaseAll);
            UiThread.RunQueued(view.UploadProgressBar.Dispatcher);

            SampleCheck.Equal(true, viewModel.UploadCommand.Completion.IsCompletedSuccessfully);
            SampleCheck.Equal(FullPercent, view.UploadProgressBar.Value);
            SampleCheck.Equal(MediaBucketName, viewModel.SelectedBucket!.Name);
        }
    }
}

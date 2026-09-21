// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.CommandBinding;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Controls;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Primitives.Advanced;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.MechanismsCommandBinders;

/// <summary>
/// Shows how a custom <see cref="ICreatesCommandBinding"/> is registered, how it outranks the generated command
/// binding, and how a stream of values drives a command.
/// </summary>
public static class MechanismsCommandBindersExamples
{
    /// <summary>The title of the item the add button creates.</summary>
    private const string PlumberTitle = "Call the plumber";

    /// <summary>The name of the uploaded screenshot.</summary>
    private const string ScreenshotName = "standup.png";

    /// <summary>The name of the second uploaded screenshot.</summary>
    private const string SecondScreenshotName = "retro.png";

    /// <summary>The name of the third uploaded screenshot.</summary>
    private const string ThirdScreenshotName = "planning.png";

    /// <summary>The media type of the uploaded screenshots.</summary>
    private const string PngType = "image/png";

    /// <summary>The size of an uploaded screenshot in bytes.</summary>
    private const long ScreenshotBytes = 350_000;

    /// <summary>The number of objects in the media bucket before an upload.</summary>
    private const int SeededMediaObjectCount = 5;

    /// <summary>The number of objects under <c>photos/2026/</c>.</summary>
    private const int PhotoObjectCount = 2;

    /// <summary>The folder the fixed-command example lists.</summary>
    private const string PhotoFolder = "photos/2026/";

    /// <summary>Registers the binder through the builder and asks the service which binder serves each control type.</summary>
    /// <param name="binder">The binder to register.</param>
    public static void RegisterBinderThroughBuilder(ClickCommandBinder binder)
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices().WithCommandBinder(binder).BuildApp();

        SampleCheck.Equal(true, ReferenceEquals(binder, CommandBinderService.GetBinder<ButtonControl>(false)));
        SampleCheck.Equal(true, ReferenceEquals(binder, CommandBinderService.GetBinder<ButtonControl>(true)));
        SampleCheck.Equal(true, CommandBinderService.GetBinder<TextBoxControl>(false) is null);
    }

    /// <summary>Compares every affinity score with the binder's bid; the binder outranks the scores below it and loses to the rest.</summary>
    public static void CompareBinderWithGeneratedBinding()
    {
        int[] belowBid =
        [
            BindingAffinity.Fallback,
            BindingAffinity.DefaultInternalTypeConverter,
            BindingAffinity.DefaultEvent,
            BindingAffinity.WpfDependencyObject,
            BindingAffinity.EventEnabledControl,
            BindingAffinity.Explicit,
            BindingAffinity.WinUiDependencyObject,
            BindingAffinity.WinFormsEvent,
        ];
        int[] atOrAboveBid = [BindingAffinity.ExactType, BindingAffinity.Kvo];

        foreach (var generated in belowBid)
        {
            SampleCheck.Equal(true, CommandBindingAffinityChecker.HasHigherAffinityPlugin<ButtonControl>(generated, false));
        }

        foreach (var generated in atOrAboveBid)
        {
            SampleCheck.Equal(false, CommandBindingAffinityChecker.HasHigherAffinityPlugin<ButtonControl>(generated, false));
        }

        SampleCheck.Equal(false, CommandBindingAffinityChecker.HasHigherAffinityPlugin<TextBoxControl>(BindingAffinity.Fallback, false));
    }

    /// <summary>Binds the add button; the binder enables it as the command allows and runs the command on each click.</summary>
    /// <param name="viewModel">The view model that owns the add command.</param>
    /// <param name="view">The view that owns the add button.</param>
    /// <param name="binder">The registered binder.</param>
    /// <returns>A task that completes when the added item is stored.</returns>
    public static async Task BindButtonWithRegisteredBinder(TodoListViewModel viewModel, TodoView view, ClickCommandBinder binder)
    {
        var itemCount = viewModel.Items.Count;

        using (view.BindCommand(viewModel, x => x.AddCommand, v => v.AddButton))
        {
            SampleCheck.Equal(1, binder.BindCount);
            SampleCheck.Equal(true, view.AddButton.Command is null);
            SampleCheck.Equal(false, view.AddButton.IsEnabled);

            viewModel.NewTitle = PlumberTitle;

            SampleCheck.Equal(true, view.AddButton.IsEnabled);

            view.AddButton.Press();
            await viewModel.AddCommand.Completion.ConfigureAwait(false);

            SampleCheck.Equal(itemCount + 1, viewModel.Items.Count);
            SampleCheck.Equal(PlumberTitle, viewModel.Items[^1].Title);
        }

        viewModel.NewTitle = PlumberTitle;
        view.AddButton.Press();

        SampleCheck.Equal(itemCount + 1, viewModel.Items.Count);
    }

    /// <summary>Binds the upload button with a stream of parameters; the binder passes the latest one to the command.</summary>
    /// <param name="browser">The browser whose selected bucket receives the upload.</param>
    /// <param name="view">The view that owns the upload button.</param>
    /// <param name="uploads">The stream of files the user picks.</param>
    /// <returns>A task that completes when the file is stored.</returns>
    public static async Task BindButtonWithParameterStream(StorageBrowserViewModel browser, StorageBrowserView view, Signal<UploadRequest> uploads)
    {
        using (view.BindCommand(browser, x => x.UploadCommand, v => v.UploadButton, uploads))
        {
            SampleCheck.Equal(false, view.UploadButton.IsEnabled);

            uploads.OnNext(new(ScreenshotName, ScreenshotBytes, PngType));

            SampleCheck.Equal(true, view.UploadButton.IsEnabled);

            view.UploadButton.Press();
            await browser.UploadCommand.Completion.ConfigureAwait(false);
        }

        SampleCheck.Equal(SeededMediaObjectCount + 1, browser.Objects.Count);
        SampleCheck.Equal(true, ContainsKey(browser.Objects, ScreenshotName));
        SampleCheck.Equal(string.Empty, browser.ErrorMessage);
    }

    /// <summary>Binds the refresh button to a named event; the binder receives the event name.</summary>
    /// <param name="browser">The browser whose command lists the objects.</param>
    /// <param name="view">The view that owns the refresh button.</param>
    /// <param name="binder">The registered binder.</param>
    /// <returns>A task that completes when the objects are listed.</returns>
    public static async Task BindButtonToNamedEvent(StorageBrowserViewModel browser, StorageBrowserView view, ClickCommandBinder binder)
    {
        browser.CurrentPrefix = PhotoFolder;

        using (view.BindCommand(browser, x => x.RefreshCommand, v => v.RefreshButton, nameof(ButtonControl.Click)))
        {
            SampleCheck.Equal(nameof(ButtonControl.Click), binder.LastEventName);
            SampleCheck.Equal(typeof(EventArgs), binder.LastEventArgsType);

            view.RefreshButton.Press();
            await browser.RefreshCommand.Completion.ConfigureAwait(false);
        }

        SampleCheck.Equal(PhotoObjectCount, browser.Objects.Count);
    }

    /// <summary>Calls the binder with explicit add and remove delegates, the overload that needs no event lookup.</summary>
    /// <param name="browser">The browser whose command reopens the link.</param>
    /// <param name="view">The view that owns the reconnect button.</param>
    /// <param name="storage">The storage service that owns the link.</param>
    /// <param name="binder">The registered binder.</param>
    /// <returns>A task that completes when the link is open again.</returns>
    public static async Task BindWithExplicitEventHandlers(
        StorageBrowserViewModel browser,
        StorageBrowserView view,
        InMemoryObjectStorage storage,
        ClickCommandBinder binder)
    {
        var button = view.ConnectButton;
        storage.Disconnect();

        using (binder.BindCommandToObject<ButtonControl, EventArgs>(
            browser.ConnectCommand,
            button,
            ImmutableEmptySignal<object>.Instance,
            handler => button.Click += handler.Invoke,
            handler => button.Click -= handler.Invoke))
        {
            SampleCheck.Equal(true, button.IsEnabled);

            button.Press();
            await browser.ConnectCommand.Completion.ConfigureAwait(false);
        }

        SampleCheck.Equal(ConnectionState.Connected, storage.Connection.State);
    }

    /// <summary>Executes a command with each value a stream produces, whether the stream is a property or an event source.</summary>
    /// <param name="browser">The browser whose command lists the objects.</param>
    /// <returns>A task that completes when the last listing has finished.</returns>
    public static async Task InvokeCommandForEachValue(StorageBrowserViewModel browser)
    {
        browser.CurrentPrefix = string.Empty;

        using (CommandInvoker.Invoke(browser.WhenChanged(x => x.CurrentPrefix), browser.RefreshCommand))
        {
            await browser.RefreshCommand.Completion.ConfigureAwait(false);

            SampleCheck.Equal(SeededMediaObjectCount + 1, browser.Objects.Count);

            browser.CurrentPrefix = PhotoFolder;
            await browser.RefreshCommand.Completion.ConfigureAwait(false);

            SampleCheck.Equal(PhotoObjectCount, browser.Objects.Count);
        }

        browser.CurrentPrefix = string.Empty;

        SampleCheck.Equal(PhotoObjectCount, browser.Objects.Count);
    }

    /// <summary>Executes whichever command the command stream produced last; a value that arrives while there is none is dropped.</summary>
    /// <param name="browser">The browser whose command uploads a file.</param>
    /// <param name="uploads">The stream of files the user picks.</param>
    /// <param name="commands">The stream of commands, which starts without one.</param>
    /// <returns>A task that completes when the file is stored.</returns>
    public static async Task InvokeLatestCommand(StorageBrowserViewModel browser, Signal<UploadRequest> uploads, Signal<ICommand?> commands)
    {
        browser.CurrentPrefix = string.Empty;
        browser.RefreshCommand.Execute(null);
        await browser.RefreshCommand.Completion.ConfigureAwait(false);

        var objectCount = browser.Objects.Count;

        using (CommandInvoker.Invoke(uploads, commands))
        {
            commands.OnNext(null);
            uploads.OnNext(new(SecondScreenshotName, ScreenshotBytes, PngType));

            SampleCheck.Equal(objectCount, browser.Objects.Count);

            commands.OnNext(browser.UploadCommand);
            uploads.OnNext(new(ThirdScreenshotName, ScreenshotBytes, PngType));
            await browser.UploadCommand.Completion.ConfigureAwait(false);
        }

        SampleCheck.Equal(objectCount + 1, browser.Objects.Count);
        SampleCheck.Equal(true, ContainsKey(browser.Objects, ThirdScreenshotName));
        SampleCheck.Equal(false, ContainsKey(browser.Objects, SecondScreenshotName));
    }

    /// <summary>Checks whether a list of stored objects holds a key.</summary>
    /// <param name="objects">The stored objects.</param>
    /// <param name="key">The key to look for.</param>
    /// <returns><see langword="true"/> when an object has the key.</returns>
    private static bool ContainsKey(IReadOnlyList<StorageObject> objects, string key)
    {
        foreach (var stored in objects)
        {
            if (stored.Key == key)
            {
                return true;
            }
        }

        return false;
    }
}

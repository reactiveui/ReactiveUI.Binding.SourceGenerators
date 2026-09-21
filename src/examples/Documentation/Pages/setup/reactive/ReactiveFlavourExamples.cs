// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Reactive.Concurrency;
using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Reactive.Builder;
using ReactiveUI.Binding.Reactive.Maui;
using ReactiveUI.Binding.Reactive.Maui.Builder;

namespace ReactiveUI.Binding.Documentation.Setup.ReactiveFlavour;

/// <summary>
/// The to-do bindings of the lean package, written against <c>ReactiveUI.Binding.Reactive</c>. That package takes
/// System.Reactive's <see cref="IScheduler"/> where the lean package takes a sequencer, and its types live under the
/// <c>ReactiveUI.Binding.Reactive</c> namespace.
/// </summary>
public static class ReactiveFlavourExamples
{
    /// <summary>The title an example gives the first item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>Shows the namespace the builder lives in.</summary>
    public static void ShowShiftedNamespace()
    {
        Console.WriteLine(typeof(ReactiveUIBindingBuilder).Namespace);

        // Output:
        // ReactiveUI.Binding.Reactive.Builder
    }

    /// <summary>Builds the application with the builder of the System.Reactive package, including its MAUI module.</summary>
    public static void BuildApplication()
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        var app = builder
            .WithCoreServices()
            .WithMaui()
            .BuildApp();

        Console.WriteLine(app.Current!.GetService<IViewThreadInvoker>() is DispatcherViewThreadInvoker);

        // Output:
        // True
    }

    /// <summary>Observes the title of an item with <c>WhenChanged</c>.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task ObserveTitleChange()
    {
        var viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());
        await viewModel.LoadAsync();
        var registration = viewModel.Items[0];

        using (registration.WhenChanged(x => x.Title).Subscribe(Console.WriteLine))
        {
            registration.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration
        // Renew car registration online
    }

    /// <summary>Binds the count of unfinished items to a label as text.</summary>
    /// <returns>A task that completes when an item is finished.</returns>
    public static async Task BindRemainingCountToLabel()
    {
        var viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());
        var view = new TodoView();
        await viewModel.LoadAsync();

        using (viewModel.BindOneWay(view, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture)))
        {
            Console.WriteLine(view.RemainingLabel.Text);

            viewModel.SelectedItem = viewModel.Items[0];
            await viewModel.CompleteAsync();

            Console.WriteLine(view.RemainingLabel.Text);
        }

        // Output:
        // 3
        // 2
    }

    /// <summary>Writes the label on a System.Reactive scheduler you supply, here the immediate scheduler that runs the write at once.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task BindRemainingCountOnImmediateScheduler()
    {
        var viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());
        var view = new TodoView();
        await viewModel.LoadAsync();

        using (viewModel.BindOneWay(view, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture), ImmediateScheduler.Instance))
        {
            Console.WriteLine(view.RemainingLabel.Text);
        }

        // Output:
        // 3
    }

    /// <summary>Writes the label on a thread of its own, through a System.Reactive scheduler that starts one.</summary>
    /// <returns>A task that completes when the label has been written.</returns>
    public static async Task BindRemainingCountOnNewThread()
    {
        var viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());
        var view = new TodoView();
        await viewModel.LoadAsync();

        var written = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        view.RemainingLabel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Label.Text))
            {
                _ = written.TrySetResult(Environment.CurrentManagedThreadId);
            }
        };

        var callerThread = Environment.CurrentManagedThreadId;

        using (viewModel.BindOneWay(view, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture), NewThreadScheduler.Default))
        {
            var writerThread = await written.Task;

            Console.WriteLine(view.RemainingLabel.Text);
            Console.WriteLine(writerThread != callerThread);
        }

        // Output:
        // 3
        // True
    }
}

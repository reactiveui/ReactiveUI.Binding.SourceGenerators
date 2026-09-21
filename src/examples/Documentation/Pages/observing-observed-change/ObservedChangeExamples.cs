// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Documentation;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.ObservingObservedChange;

/// <summary>Examples demonstrating <see cref="IObservedChange{TSender, TValue}"/> and related APIs.</summary>
public static class ObservedChangeExamples
{
    /// <summary>Title for review task.</summary>
    private const string ReviewPrTitle = "Review PR";

    /// <summary>Title for test writing task.</summary>
    private const string WriteTestsTitle = "Write tests";

    /// <summary>Title for production deployment task.</summary>
    private const string DeployProductionTitle = "Deploy to production";

    /// <summary>Title for code review task.</summary>
    private const string CodeReviewTitle = "Code review";

    /// <summary>Title for merging approved PRs task.</summary>
    private const string MergeApprovedTitle = "Merge approved PRs";

    /// <summary>Due date year.</summary>
    private const int DueYear = 2026;

    /// <summary>Due date month.</summary>
    private const int DueMonth = 12;

    /// <summary>Due date day.</summary>
    private const int DueDay = 25;

    /// <summary>Task ID 1.</summary>
    private const int TaskId1 = 1;

    /// <summary>Task ID 2.</summary>
    private const int TaskId2 = 2;

    /// <summary>Task ID 3.</summary>
    private const int TaskId3 = 3;

    /// <summary>Task ID 4.</summary>
    private const int TaskId4 = 4;

    /// <summary>Demonstrates the <see cref="IObservedChange{TSender, TValue}"/> interface and its properties.</summary>
    public static void DemonstrateIObservedChangeInterface()
    {
        var item = new TodoItem { Id = TaskId1, Title = ReviewPrTitle };
        Expression<Func<TodoItem, string>> expr = x => x.Title;

        // Create an observed change instance
        var change = new ObservedChange<TodoItem, string>(
            item,
            expr,
            item.Title);

        // Access the interface properties
        SampleCheck.Equal(item, change.Sender);
        SampleCheck.Equal(expr, change.Expression);
        SampleCheck.Equal(ReviewPrTitle, change.Value);
    }

    /// <summary>Demonstrates the <see cref="ObservedChange{TSender, TValue}"/> concrete type constructor and usage.</summary>
    public static void DemonstrateObservedChangeConstructor()
    {
        var item = new TodoItem { Id = TaskId2, Title = WriteTestsTitle };
        Expression<Func<TodoItem, string>>? expr = null;

        // Construct ObservedChange with explicit parameters
        var change = new ObservedChange<TodoItem, string>(item, expr, item.Title);

        SampleCheck.Equal(item, change.Sender);
        SampleCheck.Equal(WriteTestsTitle, change.Value);
        SampleCheck.Equal(null, change.Expression);
    }

    /// <summary>Demonstrates creating observed changes with various properties and expressions.</summary>
    public static void DemonstrateObservedChangedMixinsExtensionMethods()
    {
        var item = new TodoItem { Id = TaskId3, Title = DeployProductionTitle, Priority = TodoPriority.High };

        // Create observed changes for multiple properties
        var titleChange = new ObservedChange<TodoItem, string>(item, null, item.Title);
        var priorityChange = new ObservedChange<TodoItem, TodoPriority>(item, null, item.Priority);

        // Each change captures the current state
        SampleCheck.Equal(DeployProductionTitle, titleChange.Value);
        SampleCheck.Equal(TodoPriority.High, priorityChange.Value);
        SampleCheck.Equal(item, titleChange.Sender);
        SampleCheck.Equal(item, priorityChange.Sender);
    }

    /// <summary>Demonstrates creating and working with observed change objects directly.</summary>
    public static void DemonstrateReactiveNotifyPropertyChangedMixins()
    {
        var item = new TodoItem { Id = TaskId4, Title = CodeReviewTitle, DueDate = new DateOnly(DueYear, DueMonth, DueDay) };

        // Create an observed change directly
        var change = new ObservedChange<TodoItem, DateOnly?>(item, null, item.DueDate);

        // Access the value directly from the change object
        var dueDate = change.Value;
        SampleCheck.Equal(new DateOnly(DueYear, DueMonth, DueDay), dueDate);

        // Sender is always available
        SampleCheck.Equal(item, change.Sender);
    }

    /// <summary>Demonstrates extracting detailed information from an <see cref="IObservedChange{TSender, TValue}"/>.</summary>
    public static void DemonstrateExtractingInformationFromObservedChange()
    {
        var item = new TodoItem { Title = "Task A", Priority = TodoPriority.High };

        // Create and inspect an observed change
        var change = new ObservedChange<TodoItem, TodoPriority>(item, null, item.Priority);

        // All interface members are accessible
        SampleCheck.Equal(item, change.Sender);
        SampleCheck.Equal(TodoPriority.High, change.Value);

        // Expression can be null or populated
        SampleCheck.Equal(null, change.Expression);
    }

    /// <summary>Demonstrates using observed changes in a realistic Todo application scenario.</summary>
    public static void DemonstrateCommonScenariosWithTodoApp()
    {
        var store = InMemoryTodoStore.CreateSeeded();
        var viewModel = new TodoListViewModel(store);

        viewModel.LoadCommand.Execute(null);

        var item = viewModel.Items[0];

        // Create observed changes to document the state at different times
        var initialChange = new ObservedChange<TodoItem, bool>(item, null, item.IsDone);
        SampleCheck.Equal(false, initialChange.Value);

        item.IsDone = true;
        var completedChange = new ObservedChange<TodoItem, bool>(item, null, item.IsDone);
        SampleCheck.Equal(true, completedChange.Value);

        item.IsDone = false;
        var resetChange = new ObservedChange<TodoItem, bool>(item, null, item.IsDone);
        SampleCheck.Equal(false, resetChange.Value);

        // All three changes share the same sender
        SampleCheck.Equal(item, initialChange.Sender);
        SampleCheck.Equal(item, completedChange.Sender);
        SampleCheck.Equal(item, resetChange.Sender);
    }

    /// <summary>Demonstrates tracking property changes with multiple observed change snapshots.</summary>
    public static void DemonstrateCommonScenariosWithGitHub()
    {
        var item = new TodoItem { Title = MergeApprovedTitle };

        // Capture the initial state
        var before = new ObservedChange<TodoItem, string>(item, null, item.Title);
        SampleCheck.Equal(MergeApprovedTitle, before.Value);

        // Change the property
        item.Title = "Review test coverage";

        // Capture the updated state
        var after = new ObservedChange<TodoItem, string>(item, null, item.Title);
        SampleCheck.Equal("Review test coverage", after.Value);

        // Both changes refer to the same item
        SampleCheck.Equal(item, before.Sender);
        SampleCheck.Equal(item, after.Sender);
    }
}

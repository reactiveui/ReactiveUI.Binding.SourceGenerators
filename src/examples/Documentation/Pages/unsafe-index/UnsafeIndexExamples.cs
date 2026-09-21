// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Windows.Input;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.UnsafeIndex;

/// <summary>
/// Demonstrates the <c>Unsafe</c> twins of the binding methods. Each property path here is built while the app runs,
/// so the generator cannot read it and only the reflection-based overload can serve the call.
/// </summary>
public static class UnsafeIndexExamples
{
    /// <summary>The title of the first seeded item.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives an item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The last title an example gives an item.</summary>
    private const string FinalTitle = "Renew car registration by post";

    /// <summary>The last notes an example gives an item.</summary>
    private const string FinalNotes = "Bring nothing.";

    /// <summary>The notes an example gives an item.</summary>
    private const string SeededNotes = "Bring the insurance certificate.";

    /// <summary>The notes an example writes.</summary>
    private const string ReplacementNotes = "Bring the insurance certificate and a photo.";

    /// <summary>The title an example adds through a bound button.</summary>
    private const string NewItemTitle = "Water the plants";

    /// <summary>The text of a filter.</summary>
    private const string FilterQuery = "car";

    /// <summary>The name of the <see cref="TodoView.RemainingLabel"/> property.</summary>
    private const string RemainingLabelName = nameof(TodoView.RemainingLabel);

    /// <summary>The name of the <c>Text</c> property of a control.</summary>
    private const string TextName = nameof(TextBoxControl.Text);

    /// <summary>The name of the <see cref="TodoView.FilterTextBox"/> property.</summary>
    private const string FilterTextBoxName = nameof(TodoView.FilterTextBox);

    /// <summary>The name of the <see cref="TodoView.AddButton"/> property.</summary>
    private const string AddButtonName = nameof(TodoView.AddButton);

    /// <summary>The name of the <see cref="TransferViewModel.Draft"/> property.</summary>
    private const string DraftName = nameof(TransferViewModel.Draft);

    /// <summary>The name of the <see cref="TransferView.AmountTextBox"/> property.</summary>
    private const string AmountTextBoxName = nameof(TransferView.AmountTextBox);

    /// <summary>The amount of a transfer, as the view shows it.</summary>
    private const string AmountText = "125.50";

    /// <summary>The amount of a transfer.</summary>
    private const decimal AmountValue = 125.50M;

    /// <summary>The number of items after the seeded four and one added one.</summary>
    private const int ItemsAfterAdd = 5;

    /// <summary>The number of unfinished items after the first seeded item is finished.</summary>
    private const int RemainingAfterComplete = 2;

    /// <summary>The announcement an example emits.</summary>
    private const string SyncComplete = "Sync complete";

    /// <summary>The urgent announcement an example emits.</summary>
    private const string DiskFull = "Disk full";

    /// <summary>Registers the default converters and the button binder that the <c>Unsafe</c> methods look up while the app runs.</summary>
    public static void BuildApplication()
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder
            .WithCoreServices()
            .WithConverter(new TodoItemTitleConverter())
            .WithCommandBinder(new ButtonClickCommandBinder())
            .BuildApp();
    }

    /// <summary>Observes one property chosen by name.</summary>
    public static void ObserveTitleChosenAtRunTime()
    {
        var item = new TodoItem { Title = OriginalTitle };
        var titleColumn = RuntimePath<TodoItem, string>.Of(nameof(TodoItem.Title));
        List<string> titles = [];

        using (item.WhenChangedUnsafe(titleColumn).Subscribe(titles.Add))
        {
            item.Title = RenamedTitle;
        }

        SampleCheck.SequenceEqual([OriginalTitle, RenamedTitle], titles);
    }

    /// <summary>Observes two properties chosen by name; each change delivers both values.</summary>
    public static void ObserveTwoColumnsChosenAtRunTime()
    {
        var item = new TodoItem { Title = OriginalTitle };
        var titleColumn = RuntimePath<TodoItem, string>.Of(nameof(TodoItem.Title));
        var doneColumn = RuntimePath<TodoItem, bool>.Of(nameof(TodoItem.IsDone));
        List<PropertyValues<string, bool>> values = [];

        using (item.WhenChangedUnsafe(titleColumn, doneColumn).Subscribe(values.Add))
        {
            item.IsDone = true;
        }

        SampleCheck.SequenceEqual([new(OriginalTitle, false), new(OriginalTitle, true)], values);
    }

    /// <summary>Observes two properties chosen by name and combines them with a selector.</summary>
    public static void ObserveTwoColumnsWithSelector()
    {
        var item = new TodoItem { Title = OriginalTitle };
        var titleColumn = RuntimePath<TodoItem, string>.Of(nameof(TodoItem.Title));
        var doneColumn = RuntimePath<TodoItem, bool>.Of(nameof(TodoItem.IsDone));
        List<string> lines = [];

        using (item.WhenChangedUnsafe(titleColumn, doneColumn, static (title, done) => done ? $"[x] {title}" : $"[ ] {title}").Subscribe(lines.Add))
        {
            item.IsDone = true;
        }

        SampleCheck.SequenceEqual([$"[ ] {OriginalTitle}", $"[x] {OriginalTitle}"], lines);
    }

    /// <summary>Observes three properties chosen by name; each change delivers all three values.</summary>
    public static void ObserveThreeColumnsChosenAtRunTime()
    {
        var item = new TodoItem { Title = OriginalTitle, Notes = SeededNotes };
        var titleColumn = RuntimePath<TodoItem, string>.Of(nameof(TodoItem.Title));
        var notesColumn = RuntimePath<TodoItem, string>.Of(nameof(TodoItem.Notes));
        var doneColumn = RuntimePath<TodoItem, bool>.Of(nameof(TodoItem.IsDone));
        List<PropertyValues<string, string, bool>> values = [];

        using (item.WhenChangedUnsafe(titleColumn, notesColumn, doneColumn).Subscribe(values.Add))
        {
            item.IsDone = true;
        }

        SampleCheck.SequenceEqual([new(OriginalTitle, SeededNotes, false), new(OriginalTitle, SeededNotes, true)], values);
    }

    /// <summary>Observes three properties chosen by name and combines them with a selector.</summary>
    public static void ObserveThreeColumnsWithSelector()
    {
        var item = new TodoItem { Title = OriginalTitle, Notes = SeededNotes };
        var titleColumn = RuntimePath<TodoItem, string>.Of(nameof(TodoItem.Title));
        var notesColumn = RuntimePath<TodoItem, string>.Of(nameof(TodoItem.Notes));
        var doneColumn = RuntimePath<TodoItem, bool>.Of(nameof(TodoItem.IsDone));
        List<string> lines = [];

        using (item.WhenChangedUnsafe(titleColumn, notesColumn, doneColumn, static (title, notes, done) => $"{title} | {notes} | {done}").Subscribe(lines.Add))
        {
            item.Notes = ReplacementNotes;
        }

        SampleCheck.SequenceEqual([$"{OriginalTitle} | {SeededNotes} | False", $"{OriginalTitle} | {ReplacementNotes} | False"], lines);
    }

    /// <summary>Observes the value a property held before it changed.</summary>
    public static void ObserveTitleBeforeChange()
    {
        var item = new EditableTodoItem { Title = OriginalTitle };
        var titleColumn = RuntimePath<EditableTodoItem, string>.Of(nameof(EditableTodoItem.Title));
        List<string> titles = [];

        using (item.WhenChangingUnsafe(titleColumn).Subscribe(titles.Add))
        {
            item.Title = RenamedTitle;
            item.Title = FinalTitle;
        }

        SampleCheck.SequenceEqual([OriginalTitle, RenamedTitle], titles);
    }

    /// <summary>Observes two properties before they change.</summary>
    public static void ObserveTwoColumnsBeforeChange()
    {
        var item = new EditableTodoItem { Title = OriginalTitle, Notes = SeededNotes };
        var titleColumn = RuntimePath<EditableTodoItem, string>.Of(nameof(EditableTodoItem.Title));
        var notesColumn = RuntimePath<EditableTodoItem, string>.Of(nameof(EditableTodoItem.Notes));
        List<PropertyValues<string, string>> values = [];

        using (item.WhenChangingUnsafe(titleColumn, notesColumn).Subscribe(values.Add))
        {
            item.Notes = ReplacementNotes;
            item.Notes = FinalNotes;
        }

        SampleCheck.SequenceEqual([new(OriginalTitle, SeededNotes), new(OriginalTitle, ReplacementNotes)], values);
    }

    /// <summary>Observes two properties before they change and combines them with a selector.</summary>
    public static void ObserveTwoColumnsBeforeChangeWithSelector()
    {
        var item = new EditableTodoItem { Title = OriginalTitle, Notes = SeededNotes };
        var titleColumn = RuntimePath<EditableTodoItem, string>.Of(nameof(EditableTodoItem.Title));
        var notesColumn = RuntimePath<EditableTodoItem, string>.Of(nameof(EditableTodoItem.Notes));
        List<string> lines = [];

        using (item.WhenChangingUnsafe(titleColumn, notesColumn, static (title, notes) => $"{title}: {notes}").Subscribe(lines.Add))
        {
            item.Title = RenamedTitle;
            item.Title = FinalTitle;
        }

        SampleCheck.SequenceEqual([$"{OriginalTitle}: {SeededNotes}", $"{RenamedTitle}: {SeededNotes}"], lines);
    }

    /// <summary>Observes one property with <c>WhenAnyValueUnsafe</c>.</summary>
    public static void ObserveAnyValueChosenAtRunTime()
    {
        var item = new TodoItem { Title = OriginalTitle };
        var titleColumn = RuntimePath<TodoItem, string>.Of(nameof(TodoItem.Title));
        List<string> titles = [];

        using (item.WhenAnyValueUnsafe(titleColumn).Subscribe(titles.Add))
        {
            item.Title = RenamedTitle;
        }

        SampleCheck.SequenceEqual([OriginalTitle, RenamedTitle], titles);
    }

    /// <summary>Observes one property with <c>WhenAnyValueUnsafe</c> and maps each value with a selector.</summary>
    public static void ObserveAnyValueWithSelector()
    {
        var item = new TodoItem { Title = OriginalTitle };
        var titleColumn = RuntimePath<TodoItem, string>.Of(nameof(TodoItem.Title));
        List<int> lengths = [];

        using (item.WhenAnyValueUnsafe(titleColumn, static title => title.Length).Subscribe(lengths.Add))
        {
            item.Title = RenamedTitle;
        }

        SampleCheck.SequenceEqual([OriginalTitle.Length, RenamedTitle.Length], lengths);
    }

    /// <summary>Observes two properties with <c>WhenAnyValueUnsafe</c>.</summary>
    public static void ObserveTwoAnyValuesChosenAtRunTime()
    {
        var item = new TodoItem { Title = OriginalTitle };
        var titleColumn = RuntimePath<TodoItem, string>.Of(nameof(TodoItem.Title));
        var priorityColumn = RuntimePath<TodoItem, TodoPriority>.Of(nameof(TodoItem.Priority));
        List<PropertyValues<string, TodoPriority>> values = [];

        using (item.WhenAnyValueUnsafe(titleColumn, priorityColumn).Subscribe(values.Add))
        {
            item.Priority = TodoPriority.High;
        }

        SampleCheck.SequenceEqual([new(OriginalTitle, TodoPriority.Normal), new(OriginalTitle, TodoPriority.High)], values);
    }

    /// <summary>Observes two properties with <c>WhenAnyValueUnsafe</c> and combines them with a selector.</summary>
    public static void ObserveTwoAnyValuesWithSelector()
    {
        var item = new TodoItem { Title = OriginalTitle };
        var titleColumn = RuntimePath<TodoItem, string>.Of(nameof(TodoItem.Title));
        var priorityColumn = RuntimePath<TodoItem, TodoPriority>.Of(nameof(TodoItem.Priority));
        List<string> lines = [];

        using (item.WhenAnyValueUnsafe(titleColumn, priorityColumn, static (title, priority) => $"{title} [{priority}]").Subscribe(lines.Add))
        {
            item.Priority = TodoPriority.High;
        }

        SampleCheck.SequenceEqual([$"{OriginalTitle} [Normal]", $"{OriginalTitle} [High]"], lines);
    }

    /// <summary>Observes one property with <c>WhenAnyUnsafe</c>; the selector receives the change, not only its value.</summary>
    public static void ObserveAnyChosenAtRunTime()
    {
        var item = new TodoItem { Title = OriginalTitle };
        var titleColumn = RuntimePath<TodoItem, string>.Of(nameof(TodoItem.Title));
        List<string> lines = [];

        using (item.WhenAnyUnsafe(titleColumn, static change => $"{change.Sender.Id}: {change.Value}").Subscribe(lines.Add))
        {
            item.Title = RenamedTitle;
        }

        SampleCheck.SequenceEqual([$"0: {OriginalTitle}", $"0: {RenamedTitle}"], lines);
    }

    /// <summary>Observes two properties with <c>WhenAnyUnsafe</c>.</summary>
    public static void ObserveTwoAnyChosenAtRunTime()
    {
        var item = new TodoItem { Title = OriginalTitle };
        var titleColumn = RuntimePath<TodoItem, string>.Of(nameof(TodoItem.Title));
        var doneColumn = RuntimePath<TodoItem, bool>.Of(nameof(TodoItem.IsDone));
        List<string> lines = [];

        using (item.WhenAnyUnsafe(titleColumn, doneColumn, static (title, done) => $"{title.Value} {done.Value}").Subscribe(lines.Add))
        {
            item.IsDone = true;
        }

        SampleCheck.SequenceEqual([$"{OriginalTitle} False", $"{OriginalTitle} True"], lines);
    }

    /// <summary>Follows the announcement stream a property holds, and switches when the property is replaced.</summary>
    public static void ObserveAnnouncementStreamChosenAtRunTime()
    {
        TodoAnnouncer announcer = new();
        var latestColumn = RuntimePath<TodoAnnouncer, IObservable<string>?>.Of(nameof(TodoAnnouncer.Latest));
        List<string> messages = [];

        using (announcer.WhenAnyObservableUnsafe(latestColumn).Subscribe(messages.Add))
        {
            announcer.Latest = Signal.Return(SyncComplete);
        }

        SampleCheck.SequenceEqual([SyncComplete], messages);
    }

    /// <summary>Merges the announcement streams of two properties.</summary>
    public static void MergeAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latestColumn = RuntimePath<TodoAnnouncer, IObservable<string>?>.Of(nameof(TodoAnnouncer.Latest));
        var urgentColumn = RuntimePath<TodoAnnouncer, IObservable<string>?>.Of(nameof(TodoAnnouncer.Urgent));
        List<string> messages = [];

        using (announcer.WhenAnyObservableUnsafe(latestColumn, urgentColumn).Subscribe(messages.Add))
        {
            announcer.Latest = Signal.Return(SyncComplete);
            announcer.Urgent = Signal.Return(DiskFull);
        }

        SampleCheck.SequenceEqual([SyncComplete, DiskFull], messages);
    }

    /// <summary>Combines the latest announcement of two properties with a selector.</summary>
    public static void CombineAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latestColumn = RuntimePath<TodoAnnouncer, IObservable<string>?>.Of(nameof(TodoAnnouncer.Latest));
        var urgentColumn = RuntimePath<TodoAnnouncer, IObservable<string>?>.Of(nameof(TodoAnnouncer.Urgent));
        List<string> lines = [];

        using (announcer.WhenAnyObservableUnsafe(latestColumn, urgentColumn, static (latest, urgent) => $"{latest} / {urgent}").Subscribe(lines.Add))
        {
            announcer.Latest = Signal.Return(SyncComplete);
            announcer.Urgent = Signal.Return(DiskFull);
        }

        SampleCheck.SequenceEqual([$"{SyncComplete} / {DiskFull}"], lines);
    }

    /// <summary>Writes a source property to a target property, with both paths built at run time.</summary>
    public static void BindOneWayBetweenPathsChosenAtRunTime()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();
        var source = RuntimePath<TodoListViewModel, string>.Of(nameof(TodoListViewModel.FilterText));
        var target = RuntimePath<TodoView, string>.Of(FilterTextBoxName, TextName);

        using (viewModel.BindOneWayUnsafe(view, source, target))
        {
            viewModel.FilterText = FilterQuery;
        }

        SampleCheck.Equal(FilterQuery, view.FilterTextBox.Text);
    }

    /// <summary>Writes a source property to a target property of another type through a conversion function.</summary>
    public static void BindOneWayWithConversionChosenAtRunTime()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();
        var source = RuntimePath<TodoListViewModel, int>.Of(nameof(TodoListViewModel.RemainingCount));
        var target = RuntimePath<TodoView, string>.Of(RemainingLabelName, TextName);

        using (viewModel.BindOneWayUnsafe(view, source, target, static count => count.ToString(CultureInfo.InvariantCulture)))
        {
            SampleCheck.Equal("3", view.RemainingLabel.Text);
        }
    }

    /// <summary>Carries a property both ways between two paths built at run time.</summary>
    public static void BindTwoWayBetweenPathsChosenAtRunTime()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();
        var source = RuntimePath<TodoListViewModel, string>.Of(nameof(TodoListViewModel.FilterText));
        var target = RuntimePath<TodoView, string>.Of(FilterTextBoxName, TextName);

        using (viewModel.BindTwoWayUnsafe(view, source, target))
        {
            view.FilterTextBox.Text = FilterQuery;

            SampleCheck.Equal(FilterQuery, viewModel.FilterText);
        }
    }

    /// <summary>Carries an amount both ways as text, with a conversion function for each direction.</summary>
    public static void BindTwoWayWithConversionsChosenAtRunTime()
    {
        TransferViewModel viewModel = new(CreateBackend());
        TransferView view = new();
        var source = RuntimePath<TransferViewModel, decimal>.Of(DraftName, nameof(TransferDraft.Amount));
        var target = RuntimePath<TransferView, string>.Of(AmountTextBoxName, TextName);

        using (viewModel.BindTwoWayUnsafe(
            view,
            source,
            target,
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture)))
        {
            view.AmountTextBox.Text = AmountText;

            SampleCheck.Equal(AmountValue, viewModel.Draft.Amount);
        }
    }

    /// <summary>Binds a view model property to a view property, written view first.</summary>
    public static void OneWayBindChosenAtRunTime()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };
        var source = RuntimePath<TodoListViewModel, string>.Of(nameof(TodoListViewModel.FilterText));
        var target = RuntimePath<TodoView, string>.Of(FilterTextBoxName, TextName);

        using (view.OneWayBindUnsafe(viewModel, source, target))
        {
            viewModel.FilterText = FilterQuery;
        }

        SampleCheck.Equal(FilterQuery, view.FilterTextBox.Text);
    }

    /// <summary>Binds a view model property to a view property, written view first, through a selector.</summary>
    public static void OneWayBindWithSelectorChosenAtRunTime()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };
        var source = RuntimePath<TodoListViewModel, int>.Of(nameof(TodoListViewModel.RemainingCount));
        var target = RuntimePath<TodoView, string>.Of(RemainingLabelName, TextName);

        using (view.OneWayBindUnsafe(viewModel, source, target, static count => $"{count} left"))
        {
            SampleCheck.Equal("3 left", view.RemainingLabel.Text);
        }
    }

    /// <summary>Carries a property both ways, written view first.</summary>
    public static void BindChosenAtRunTime()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };
        var source = RuntimePath<TodoListViewModel, string>.Of(nameof(TodoListViewModel.FilterText));
        var target = RuntimePath<TodoView, string>.Of(FilterTextBoxName, TextName);

        using (view.BindUnsafe(viewModel, source, target))
        {
            view.FilterTextBox.Text = FilterQuery;

            SampleCheck.Equal(FilterQuery, viewModel.FilterText);
        }
    }

    /// <summary>Carries an amount both ways as text, written view first, with a conversion function for each direction.</summary>
    public static void BindWithConversionsChosenAtRunTime()
    {
        TransferViewModel viewModel = new(CreateBackend());
        TransferView view = new() { ViewModel = viewModel };
        var source = RuntimePath<TransferViewModel, decimal>.Of(DraftName, nameof(TransferDraft.Amount));
        var target = RuntimePath<TransferView, string>.Of(AmountTextBoxName, TextName);

        using (view.BindUnsafe(
            viewModel,
            source,
            target,
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture)))
        {
            view.AmountTextBox.Text = AmountText;

            SampleCheck.Equal(AmountValue, viewModel.Draft.Amount);
        }
    }

    /// <summary>Writes each selected item of a stream to a text property, through the converter registered for the two types.</summary>
    public static void BindToTargetChosenAtRunTime()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();
        var source = RuntimePath<TodoListViewModel, TodoItem?>.Of(nameof(TodoListViewModel.SelectedItem));
        var target = RuntimePath<TodoView, string?>.Of(nameof(TodoView.SelectedTitleTextBox), TextName);

        using (viewModel.WhenChangedUnsafe(source).BindToUnsafe(view, target))
        {
            viewModel.SelectedItem = viewModel.Items[0];

            SampleCheck.Equal(OriginalTitle, view.SelectedTitleTextBox.Text);
        }
    }

    /// <summary>Writes each selected item of a stream to a text property, through a converter you name.</summary>
    public static void BindToWithConverterOverride()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();
        var source = RuntimePath<TodoListViewModel, TodoItem?>.Of(nameof(TodoListViewModel.SelectedItem));
        var target = RuntimePath<TodoView, string?>.Of(nameof(TodoView.SelectedTitleTextBox), TextName);

        using (viewModel.WhenChangedUnsafe(source).BindToUnsafe(view, target, new TodoItemTitleConverter()))
        {
            viewModel.SelectedItem = viewModel.Items[0];

            SampleCheck.Equal(OriginalTitle, view.SelectedTitleTextBox.Text);
        }
    }

    /// <summary>Writes each selected item of a stream to a text property, passing a conversion hint and a converter you name.</summary>
    public static void BindToWithHintAndConverterOverride()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();
        var source = RuntimePath<TodoListViewModel, TodoItem?>.Of(nameof(TodoListViewModel.SelectedItem));
        var target = RuntimePath<TodoView, string?>.Of(nameof(TodoView.SelectedTitleTextBox), TextName);

        using (viewModel.WhenChangedUnsafe(source).BindToUnsafe(view, target, TodoItemTitleConverter.UpperCaseHint, new TodoItemTitleConverter()))
        {
            viewModel.SelectedItem = viewModel.Items[0];

            SampleCheck.Equal(OriginalTitle.ToUpperInvariant(), view.SelectedTitleTextBox.Text);
        }
    }

    /// <summary>Writes each selected item of a stream to a text property, passing a conversion hint to the registered converter.</summary>
    public static void BindToWithConversionHint()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();
        var source = RuntimePath<TodoListViewModel, TodoItem?>.Of(nameof(TodoListViewModel.SelectedItem));
        var target = RuntimePath<TodoView, string?>.Of(nameof(TodoView.SelectedTitleTextBox), TextName);

        using (viewModel.WhenChangedUnsafe(source).BindToUnsafe(view, target, TodoItemTitleConverter.UpperCaseHint))
        {
            viewModel.SelectedItem = viewModel.Items[0];

            SampleCheck.Equal(OriginalTitle.ToUpperInvariant(), view.SelectedTitleTextBox.Text);
        }
    }

    /// <summary>Runs a command, chosen by name, for each selected item of a stream.</summary>
    /// <returns>A task that completes when the command has run.</returns>
    public static async Task InvokeCommandChosenAtRunTime()
    {
        var viewModel = CreateLoadedViewModel();
        var source = RuntimePath<TodoListViewModel, TodoItem?>.Of(nameof(TodoListViewModel.SelectedItem));
        var command = RuntimePath<TodoListViewModel, ICommand?>.Of(nameof(TodoListViewModel.CompleteCommand));

        using (viewModel.WhenChangedUnsafe(source).InvokeCommandUnsafe(viewModel, command))
        {
            viewModel.SelectedItem = viewModel.Items[0];
            await viewModel.CompleteCommand.Completion;
        }

        SampleCheck.Equal(RemainingAfterComplete, viewModel.RemainingCount);
    }

    /// <summary>Binds a command to a button chosen by name.</summary>
    /// <returns>A task that completes when the command has run.</returns>
    public static async Task BindCommandToButtonChosenAtRunTime()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };
        var command = RuntimePath<TodoListViewModel, AsyncDelegateCommand?>.Of(nameof(TodoListViewModel.AddCommand));
        var button = RuntimePath<TodoView, ButtonControl>.Of(AddButtonName);

        using (view.BindCommandUnsafe(viewModel, command, button, null))
        {
            viewModel.NewTitle = NewItemTitle;
            view.AddButton.Press();
            await viewModel.AddCommand.Completion;
        }

        SampleCheck.Equal(ItemsAfterAdd, viewModel.Items.Count);
    }

    /// <summary>Binds a command to a named button event, passing a stream of parameters.</summary>
    /// <returns>A task that completes when the command has run.</returns>
    public static async Task BindCommandWithParameterStream()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };
        var command = RuntimePath<TodoListViewModel, AsyncDelegateCommand?>.Of(nameof(TodoListViewModel.AddCommand));
        var button = RuntimePath<TodoView, ButtonControl>.Of(AddButtonName);

        using (view.BindCommandUnsafe(viewModel, command, button, Signal.Return<string?>(NewItemTitle), nameof(ButtonControl.Click)))
        {
            viewModel.NewTitle = NewItemTitle;
            view.AddButton.Press();
            await viewModel.AddCommand.Completion;
        }

        SampleCheck.Equal(ItemsAfterAdd, viewModel.Items.Count);
    }

    /// <summary>Binds a command to a button, taking the parameter from a view model property.</summary>
    /// <returns>A task that completes when the command has run.</returns>
    public static async Task BindCommandWithParameterProperty()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };
        var command = RuntimePath<TodoListViewModel, AsyncDelegateCommand?>.Of(nameof(TodoListViewModel.AddCommand));
        var button = RuntimePath<TodoView, ButtonControl>.Of(AddButtonName);
        var parameter = RuntimePath<TodoListViewModel, string?>.Of(nameof(TodoListViewModel.NewTitle));

        using (view.BindCommandUnsafe(viewModel, command, button, parameter, null))
        {
            viewModel.NewTitle = NewItemTitle;
            view.AddButton.Press();
            await viewModel.AddCommand.Completion;
        }

        SampleCheck.Equal(ItemsAfterAdd, viewModel.Items.Count);
    }

    /// <summary>Registers an asynchronous handler for an interaction chosen by name.</summary>
    /// <returns>A task that completes when the interaction has been answered.</returns>
    public static async Task BindInteractionWithTaskHandler()
    {
        TransferViewModel viewModel = new(CreateBackend());
        TransferView view = new() { ViewModel = viewModel };
        var interaction = RuntimePath<TransferViewModel, IInteraction<TransferDraft, bool>>.Of(nameof(TransferViewModel.ConfirmTransfer));

        using (view.BindInteractionUnsafe(
            viewModel,
            interaction,
            static context =>
            {
                context.SetOutput(true);
                return Task.CompletedTask;
            }))
        {
            var confirmed = await viewModel.ConfirmTransfer.Handle(viewModel.Draft);

            SampleCheck.Equal(true, confirmed);
        }
    }

    /// <summary>Registers an observable handler for an interaction chosen by name.</summary>
    /// <returns>A task that completes when the interaction has been answered.</returns>
    public static async Task BindInteractionWithObservableHandler()
    {
        TransferViewModel viewModel = new(CreateBackend());
        TransferView view = new() { ViewModel = viewModel };
        var interaction = RuntimePath<TransferViewModel, IInteraction<TransferDraft, bool>>.Of(nameof(TransferViewModel.ConfirmTransfer));

        using (view.BindInteractionUnsafe(
            viewModel,
            interaction,
            static context =>
            {
                context.SetOutput(true);
                return Signal.Return(true);
            }))
        {
            var confirmed = await viewModel.ConfirmTransfer.Handle(viewModel.Draft);

            SampleCheck.Equal(true, confirmed);
        }
    }

    /// <summary>Creates a view model that has loaded the seeded items.</summary>
    /// <returns>The loaded view model.</returns>
    private static TodoListViewModel CreateLoadedViewModel()
    {
        var viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());

        viewModel.LoadCommand.Execute(null);
        return viewModel;
    }

    /// <summary>Creates a bank whose accounts are ready to read.</summary>
    /// <returns>The banking backend.</returns>
    private static InMemoryBankingBackend CreateBackend() => new(ManualClock.StartOfWorkingDay());
}

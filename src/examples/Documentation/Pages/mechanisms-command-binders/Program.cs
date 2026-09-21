// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.MechanismsCommandBinders;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives.Signals;

var todoStore = InMemoryTodoStore.CreateSeeded();

TodoListViewModel todoViewModel = new(todoStore);

todoViewModel.LoadCommand.Execute(null);

TodoView todoView = new() { ViewModel = todoViewModel };

var storage = InMemoryObjectStorage.CreateSeeded(ManualClock.StartOfWorkingDay());

StorageBrowserViewModel browser = new(storage);

StorageBrowserView browserView = new() { ViewModel = browser };

browser.LoadBucketsCommand.Execute(null);

await browser.LoadBucketsCommand.Completion;

browser.SelectedBucket = browser.Buckets[0];

ClickCommandBinder binder = new();

MechanismsCommandBindersExamples.RegisterBinderThroughBuilder(binder);

MechanismsCommandBindersExamples.CompareBinderWithGeneratedBinding();

await MechanismsCommandBindersExamples.BindButtonWithRegisteredBinder(todoViewModel, todoView, binder);

using Signal<UploadRequest> uploads = new();

await MechanismsCommandBindersExamples.BindButtonWithParameterStream(browser, browserView, uploads);

await MechanismsCommandBindersExamples.BindButtonToNamedEvent(browser, browserView, binder);

await MechanismsCommandBindersExamples.BindWithExplicitEventHandlers(browser, browserView, storage, binder);

await MechanismsCommandBindersExamples.InvokeCommandForEachValue(browser);

using Signal<UploadRequest> queuedUploads = new();

using Signal<ICommand?> commands = new();

await MechanismsCommandBindersExamples.InvokeLatestCommand(browser, queuedUploads, commands);

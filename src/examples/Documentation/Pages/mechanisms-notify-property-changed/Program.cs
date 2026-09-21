// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.MechanismsNotifyPropertyChanged;
using ReactiveUI.Binding.Documentation.Todo;

const string MediaBucket = "acme-media";

const string PhotoFolder = "photos/2026/";

var todoStore = InMemoryTodoStore.CreateSeeded();

TodoListViewModel viewModel = new(todoStore);

viewModel.LoadCommand.Execute(null);

var item = viewModel.Items[0];

var storage = InMemoryObjectStorage.CreateSeeded(ManualClock.StartOfWorkingDay());

var objects = await storage.ListObjectsAsync(MediaBucket, PhotoFolder);

StorageBrowserViewModel browser = new(storage);

MechanismsNotifyPropertyChangedExamples.RegisterCoreObservationServices();

MechanismsNotifyPropertyChangedExamples.ObserveNotifyingClassThroughWhenChanged(item);

MechanismsNotifyPropertyChangedExamples.ObserveNotifyingClassWithInpcProvider(item);

MechanismsNotifyPropertyChangedExamples.ObservePlainClassWithInpcProvider(objects[0]);

MechanismsNotifyPropertyChangedExamples.ObservePlainClassWithPocoProvider(objects[0]);

MechanismsNotifyPropertyChangedExamples.ObservePathThroughPlainClass(browser, objects[0], objects[1]);

MechanismsNotifyPropertyChangedExamples.ObservePlainClassThroughWhenChanged(browser, objects[0], objects[1]);

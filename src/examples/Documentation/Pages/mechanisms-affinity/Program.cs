// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.MechanismsAffinity;
using ReactiveUI.Binding.Documentation.Todo;

const string MediaBucket = "acme-media";

const string BannerFolder = "photos/2026/";

var todoStore = InMemoryTodoStore.CreateSeeded();

TodoListViewModel viewModel = new(todoStore);

viewModel.LoadCommand.Execute(null);

var item = viewModel.Items[0];

var storage = InMemoryObjectStorage.CreateSeeded(ManualClock.StartOfWorkingDay());

var objects = await storage.ListObjectsAsync(MediaBucket, BannerFolder);

var banner = objects[0];

TodoPropertyObservableForProperty tied = new(nameof(TodoItem.Notes), BindingAffinity.Explicit);

TodoPropertyObservableForProperty outranking = new(nameof(TodoItem.Title), BindingAffinity.Explicit + 1);

_ = MechanismsAffinityExamples.RegisterProviders(tied, outranking);

MechanismsAffinityExamples.ObserveNotifyingClass(item);

MechanismsAffinityExamples.ObservePlainClass(banner);

MechanismsAffinityExamples.PickWinningMechanism(outranking);

MechanismsAffinityExamples.CompareEveryAffinityScore(tied);

MechanismsAffinityExamples.ObserveWithTiedAndOutrankingProviders(item, tied, outranking);

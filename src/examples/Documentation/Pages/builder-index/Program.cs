// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.BuilderIndex;
using ReactiveUI.Binding.Documentation.Todo;

BuilderIndexExamples.CreateBuilder();

BuilderIndexExamples.CreateBuilderForResolver();

BuilderIndexExamples.EnsureInitializedNeedsBuildApp();

TodoItemObservableForProperty provider = new();

ButtonCommandBinder binder = new();

var app = BuilderIndexExamples.BuildTodoApplication(provider, binder);

BuilderIndexExamples.EnsureInitializedAfterBuildApp(app);

var store = BuilderIndexExamples.ResolveModuleService(app);

TodoListViewModel viewModel = new(store);

viewModel.LoadCommand.Execute(null);

TodoView view = new() { ViewModel = viewModel };

BuilderIndexExamples.ObserveThroughRegisteredProvider(viewModel.Items[0], provider);

BuilderIndexExamples.ConvertTagsWithRegisteredConverter(viewModel, view);

BuilderIndexExamples.ConvertPriorityWithRegisteredFallbackConverter();

BuilderIndexExamples.SetIndexedTagWithRegisteredSetMethodConverter();

BuilderIndexExamples.AttachCommandWithRegisteredBinder(viewModel, view, binder);

BuilderIndexExamples.ResolveMappedView(viewModel);

BuilderIndexExamples.RegisterCoreObservationModule();

BuilderIndexExamples.RegisterConvertersThroughAppBuilder();

BuilderIndexExamples.ConfigureThroughAppBuilder();

BuilderIndexExamples.BuildThroughAppBuilder();

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.StringAndEquality;
using ReactiveUI.Binding.Documentation.Todo;

var store = InMemoryTodoStore.CreateSeeded();

TodoListViewModel viewModel = new(store);

viewModel.LoadCommand.Execute(null);

TodoView view = new();

view.ViewModel = viewModel;

StringAndEqualityExamples.DemonstrateStringConverter();

StringAndEqualityExamples.DemonstrateEqualityConverter(viewModel);

StringAndEqualityExamples.DemonstrateBooleanConverters();

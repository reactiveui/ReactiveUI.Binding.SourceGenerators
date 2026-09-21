// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Setup.ReactiveFlavour;

ReactiveFlavourExamples.ShowShiftedNamespace();

ReactiveFlavourExamples.BuildApplication();

await ReactiveFlavourExamples.ObserveTitleChange();

await ReactiveFlavourExamples.BindRemainingCountToLabel();

await ReactiveFlavourExamples.BindRemainingCountOnImmediateScheduler();

await ReactiveFlavourExamples.BindRemainingCountOnNewThread();

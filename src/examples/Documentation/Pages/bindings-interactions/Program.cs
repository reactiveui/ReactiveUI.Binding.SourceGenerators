// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.BindingsInteractions;
using ReactiveUI.Binding.Documentation.GitHub;

await BindingsInteractionsExamples.ConfirmCloseWithTaskHandler();

await BindingsInteractionsExamples.ConfirmDropWithObservableHandler();

await BindingsInteractionsExamples.ConfirmCloseThroughInterfaceProperty();

await BindingsInteractionsExamples.ApproveLargeTransferInTwoSteps();

await BindingsInteractionsExamples.CancelTransferAtApprovalStep();

await BindingsInteractionsExamples.RegisterHandlersInReverseOrder();

await BindingsInteractionsExamples.AuditApprovalWithDerivedInteraction();

await BindingsInteractionsExamples.HandleThroughInterface(new Interaction<Issue, bool>());

await BindingsInteractionsExamples.InspectInteractionContext();

await BindingsInteractionsExamples.CloseIssueWithoutHandler();

await BindingsInteractionsExamples.DisposeBindingRemovesHandler();

BindingsInteractionsExamples.BindInteractionWithoutViewModel();

BindingsInteractionsExamples.ConstructUnhandledInteractionException();

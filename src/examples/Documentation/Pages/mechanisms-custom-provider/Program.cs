// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.MechanismsCustomProvider;

var clock = ManualClock.StartOfWorkingDay();

var storage = InMemoryObjectStorage.CreateSeeded(clock);

StorageBrowserViewModel browser = new(storage);

StorageConnectionObservableForProperty provider = new();

_ = MechanismsCustomProviderExamples.RegisterProviderThroughBuilder(provider);

await MechanismsCustomProviderExamples.ObserveConnectionThroughProvider(browser, storage, provider);

MechanismsCustomProviderExamples.ObserveConnectionWithWhenChanged(browser, storage, provider);

MechanismsCustomProviderExamples.CallProviderThroughMixins(storage.Connection, provider);

MechanismsCustomProviderExamples.FindProviderThatOutranksGeneratedMechanism(provider);

MechanismsCustomProviderExamples.ObserveThroughPluginPropertyObservable(storage.Connection, provider);

MechanismsCustomProviderExamples.ChooseBetweenGeneratedObservationAndProvider(storage.Connection);

MechanismsCustomProviderExamples.ObserveThroughExpressionChain(browser);

MechanismsCustomProviderExamples.ObserveThroughObservableForPropertySink(storage.Connection, provider);

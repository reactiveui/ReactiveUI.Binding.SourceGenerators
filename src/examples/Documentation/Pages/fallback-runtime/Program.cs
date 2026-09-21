// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.FallbackRuntime;

FallbackRuntimeExamples.BuildApplication();

FallbackObservationExamples.ObserveOnePropertyAfterChange();

FallbackObservationExamples.ObserveTwoPropertiesAfterChange();

FallbackObservationExamples.ObserveThreePropertiesAfterChange();

FallbackObservationExamples.ObserveOnePropertyBeforeChange();

FallbackObservationExamples.ObserveTwoPropertiesBeforeChange();

FallbackObservationExamples.ObserveThreePropertiesBeforeChange();

FallbackObservationExamples.ObserveOneAnyValue();

FallbackObservationExamples.ObserveTwoAnyValues();

FallbackObservationExamples.ObserveThreeAnyValues();

FallbackRuntimeExamples.BindOneWayBetweenProperties();

FallbackRuntimeExamples.BindOneWayOnSequencer();

FallbackRuntimeExamples.BindOneWayWithConversion();

FallbackRuntimeExamples.BindTwoWayBetweenProperties();

FallbackRuntimeExamples.BindTwoWayWithConverterPair();

FallbackRuntimeExamples.OneWayBindToView();

FallbackRuntimeExamples.OneWayBindToViewWithConversion();

FallbackRuntimeExamples.BindViewAndViewModel();

FallbackRuntimeExamples.BindViewAndViewModelWithConverterPair();

FallbackRuntimeExamples.BindViewAndViewModelOnSignal();

FallbackRuntimeExamples.BindViewAndViewModelWithConverterPairOnSignal();

FallbackRuntimeExamples.BindStreamToProperty();

FallbackRuntimeExamples.BindStreamToPropertyWithHintAndConverter();

FallbackRuntimeExamples.BindStreamToNullTarget();

FallbackRuntimeExamples.ConvertWithRegisteredConverter();

FallbackRuntimeExamples.ConvertWithConversionHint();

FallbackRuntimeExamples.ConvertWithConverterOverride();

FallbackRuntimeExamples.ConvertWithoutConverterFails();

FallbackRuntimeExamples.CreateConverterPairWithFactory();

FallbackRuntimeExamples.CreateConverterPairWithConstructor();

await FallbackRuntimeExamples.BindCommandToButton();

await FallbackRuntimeExamples.BindCommandToButtonEvent();

FallbackRuntimeExamples.BindCommandWithoutViewModel();

await FallbackRuntimeExamples.InvokeCommandForStream();

await FallbackRuntimeExamples.BindInteractionToHandler();

ExpressionEngineExamples.NameThePathOfAnExpression();

ExpressionEngineExamples.SplitAnExpressionIntoLinks();

ExpressionEngineExamples.FindTheMemberBehindAConversion();

ExpressionEngineExamples.FindTheParentOfAMember();

ExpressionEngineExamples.ReadTheArgumentsOfAnIndexer();

ExpressionEngineExamples.RewriteAnExpressionIntoChainShape();

ExpressionEngineExamples.ReadAPropertyWithAFetcher();

ExpressionEngineExamples.ReadAPropertyWithAFetcherOrThrow();

ExpressionEngineExamples.WriteAPropertyWithASetter();

ExpressionEngineExamples.WriteAPropertyWithASetterOrThrow();

ExpressionEngineExamples.ReadTheValueAtTheEndOfAPath();

ExpressionEngineExamples.ReadThroughANullLink();

ExpressionEngineExamples.ReadEveryValueAlongAPath();

ExpressionEngineExamples.WriteTheValueAtTheEndOfAPath();

ExpressionEngineExamples.WriteTheValueAtTheEndOfAPathWithoutThrowing();

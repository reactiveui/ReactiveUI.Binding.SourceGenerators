// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.BindingsTwoWay;

// Number text does not depend on the machine's regional settings.
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

BindingsTwoWayExamples.BuildApplication();

BindingsTwoWayExamples.BindTodoFilterToTextBox();

BindingsTwoWayExamples.BindTransferAmountWithConverters();

BindingsTwoWayExamples.BindTokenOnSequencer();

BindingsTwoWayExamples.BindReferenceBurstOnSequencer();

BindingsTwoWayExamples.BindTransferAmountWithConvertersOnSequencer();

BindingsTwoWayExamples.BindTransferAmountWithConverterObjects();

BindingsTwoWayExamples.BindTransferAmountWithConverterObjectsAndHint();

BindingsTwoWayExamples.BindTransferAmountWithConverterObjectsOnSequencer();

BindingsTwoWayExamples.BindTransferAmountUnsafeWithConverterObjects();

BindingsTwoWayExamples.BindTodoFilterInView();

BindingsTwoWayExamples.BindTodoFilterWithStaticLambdas();

BindingsTwoWayExamples.BindTransferAmountInView();

BindingsTwoWayExamples.BindScoreInViewOnSequencer();

BindingsTwoWayExamples.BindScoreWithConverterObjects();

BindingsTwoWayExamples.BindScoreWithConverterObjectsAndHint();

BindingsTwoWayExamples.BindScoreWithConverterObjectsOnSequencer();

BindingsTwoWayExamples.BindScoreUnsafeWithConverterObjects();

BindingsTwoWayExamples.BindTodoFilterWithNullScheduler();

BindingsTwoWayExamples.BindTransferAmountWithConvertersAndNullScheduler();

BindingsTwoWayExamples.BindScoreInViewWithNullScheduler();

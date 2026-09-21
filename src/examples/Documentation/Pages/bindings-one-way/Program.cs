// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.BindingsOneWay;

// Number and currency text does not depend on the machine's regional settings.
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

BindingsOneWayExamples.BuildApplication();

BindingsOneWayExamples.BindSelectedIssueTitleToLabel();

BindingsOneWayExamples.BindSelectedIssueAssigneeToLabel();

BindingsOneWayExamples.BindSelectedIssueStateToLabel();

BindingsOneWayExamples.BindRemainingCountToLabel();

BindingsOneWayExamples.BindBalanceToLabelWithCurrencyFormat();

BindingsOneWayExamples.OneWayBindUploadPercentToProgressBar();

BindingsOneWayExamples.OneWayBindAvailableBalanceWithSelector();

BindingsOneWayExamples.BindOneWayOnSequencer();

BindingsOneWayExamples.BindOneWayConvertedOnSequencer();

BindingsOneWayExamples.BindOneWayWithConverterObject();

BindingsOneWayExamples.BindOneWayWithConverterObjectAndHint();

BindingsOneWayExamples.BindOneWayWithConverterObjectOnSequencer();

BindingsOneWayExamples.OneWayBindPriorityToBadgeColour();

BindingsOneWayExamples.OneWayBindPriorityToBadgeColourWithHint();

BindingsOneWayExamples.OneWayBindPriorityToBadgeColourOnSequencer();

BindingsOneWayExamples.OneWayBindWithStaticLambdas();

BindingsOneWayExamples.BindOneWayUnsafeWithConverterObject();

BindingsOneWayExamples.OneWayBindWithSelectorOnSequencer();

BindingsOneWayExamples.OneWayBindUnsafeWithConverterObject();

BindingsOneWayExamples.BindOneWayWithNullScheduler();

BindingsOneWayExamples.BindOneWayConvertedWithNullScheduler();

BindingsOneWayExamples.OneWayBindWithSelectorAndNullScheduler();

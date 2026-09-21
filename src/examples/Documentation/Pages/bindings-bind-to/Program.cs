// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.BindingsBindTo;

// Number text does not depend on the machine's regional settings.
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

BindingsBindToExamples.BindUploadPercentToProgressBar();

BindingsBindToExamples.BindRemainingCountToLabel();

BindingsBindToExamples.BindTotalBalanceWithDecimalPlacesHint();

BindingsBindToExamples.BindTotalBalanceWithFormatHint();

BindingsBindToExamples.BindTotalBalanceWithConverterOverride();

BindingsBindToExamples.BindTotalBalanceWithHintAndConverterOverride();

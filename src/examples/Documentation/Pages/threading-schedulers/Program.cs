// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.ThreadingSchedulers;

// The converters format numbers with the current culture, so the run does not depend on the machine's settings.
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

ThreadingSchedulersExamples.SetMainThread();

ThreadingSchedulersExamples.WriteOnOwningThreadSkipsMainThread();

ThreadingSchedulersExamples.WriteToUnclaimedControlSkipsMainThread();

ThreadingSchedulersExamples.UseSynchronizationContext();

ThreadingSchedulersExamples.ObserveOnViewThread();

ThreadingSchedulersExamples.ObserveOnViewThreadWithFallback();

ThreadingSchedulersExamples.BindOneWayOnSequencer();

ThreadingSchedulersExamples.BindOneWayWithConversionOnSequencer();

ThreadingSchedulersExamples.BindTwoWayOnSequencer();

ThreadingSchedulersExamples.BindTwoWayBurstOnSequencer();

ThreadingSchedulersExamples.BindTwoWayWithConversionOnSequencer();

ThreadingSchedulersExamples.BindViewOnSequencer();

ThreadingSchedulersExamples.OneWayBindViewOnSequencer();

ThreadingSchedulersExamples.BuildApplication();

ThreadingSchedulersExamples.BindOneWayUnsafeOnSequencer();

ThreadingSchedulersExamples.BindOneWayUnsafeWithConversionOnSequencer();

ThreadingSchedulersExamples.BindOneWayUnsafeWithConverterOnSequencer();

ThreadingSchedulersExamples.BindTwoWayUnsafeOnSequencer();

ThreadingSchedulersExamples.BindTwoWayUnsafeWithConversionOnSequencer();

ThreadingSchedulersExamples.BindTwoWayUnsafeWithConvertersOnSequencer();

ThreadingSchedulersExamples.BindViewUnsafeOnSequencer();

ThreadingSchedulersExamples.BindViewUnsafeWithConvertersOnSequencer();

ThreadingSchedulersExamples.OneWayBindViewUnsafeOnSequencer();

ThreadingSchedulersExamples.OneWayBindViewUnsafeWithConverterOnSequencer();

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.BindingsErrors;

var logger = BindingsErrorsExamples.BuildApplication();

BindingsErrorsExamples.FaultWithoutInnerExceptionIsLogged(logger);

BindingsErrorsExamples.FaultWithInnerExceptionIsRethrown(logger);

BindingsErrorsExamples.SetterThatThrowsReachesTheCaller();

BindingsErrorsExamples.SubscribeAppliesTheFaultContract(logger);

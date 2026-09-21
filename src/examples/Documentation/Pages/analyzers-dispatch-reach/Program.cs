// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.AnalyzersDispatchReach;

await AnalyzersDispatchReachExamples.ReportCallOutsideRootNamespace();

await AnalyzersDispatchReachExamples.AcceptCallUnderRootNamespace();

await AnalyzersDispatchReachExamples.AcceptCallOutsideRootNamespaceOnCSharp10();

await AnalyzersDispatchReachExamples.AcceptCallOutsideRootNamespaceWithInterceptors();

await AnalyzersDispatchReachExamples.ReportCompilerOlderThanRoslyn48();

await AnalyzersDispatchReachExamples.AcceptCompilerRoslyn48();

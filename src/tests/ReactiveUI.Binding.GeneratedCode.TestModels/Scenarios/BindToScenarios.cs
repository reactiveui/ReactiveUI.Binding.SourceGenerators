// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

/// <summary>
/// Scenario methods for BindTo that the source generator processes at compile time.
/// </summary>
public static class BindToScenarios
{
    /// <summary>
    /// Binds a string observable stream to a same-typed string property (direct assignment).
    /// </summary>
    /// <param name="source">The source observable stream.</param>
    /// <param name="target">The target view.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable StringToString(IObservable<string> source, BigView target)
        => source.BindTo(target, x => x.ViewProp1);
}

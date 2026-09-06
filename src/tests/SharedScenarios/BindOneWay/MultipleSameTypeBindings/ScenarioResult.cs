// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.BindOneWay.MultipleSameTypeBindings;

/// <summary>What this scenario hands back to the test that runs it.</summary>
/// <param name="FirstBinding">The scenario's FirstBinding result.</param>
/// <param name="LastBinding">The scenario's LastBinding result.</param>
public sealed record ScenarioResult(
    IDisposable FirstBinding,
    IDisposable LastBinding);

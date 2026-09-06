// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.BindOneWay.MultipleBindings;

/// <summary>What this scenario hands back to the test that runs it.</summary>
/// <param name="NameBinding">The scenario's NameBinding result.</param>
/// <param name="AgeBinding">The scenario's AgeBinding result.</param>
public sealed record ScenarioResult(
    IDisposable NameBinding,
    IDisposable AgeBinding);

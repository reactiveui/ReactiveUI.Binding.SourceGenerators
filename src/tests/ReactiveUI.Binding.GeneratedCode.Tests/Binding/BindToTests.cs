// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>
/// Runtime execution tests for generated BindTo bindings.
/// </summary>
public class BindToTests
{
    /// <summary>
    /// Verifies that BindTo applies the observable's current and subsequent values to the target property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_String_PushesValuesToTarget()
    {
        var subject = new BehaviorSubject<string>("initial");
        var target = new BigView();

        using var binding = BindToScenarios.StringToString(subject, target);

        await Assert.That(target.ViewProp1).IsEqualTo("initial");

        subject.OnNext("updated");

        await Assert.That(target.ViewProp1).IsEqualTo("updated");
    }

    /// <summary>
    /// Verifies that disposing a BindTo binding stops further updates to the target property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_Disposed_StopsUpdating()
    {
        var subject = new BehaviorSubject<string>("initial");
        var target = new BigView();

        var binding = BindToScenarios.StringToString(subject, target);
        binding.Dispose();

        subject.OnNext("after-dispose");

        await Assert.That(target.ViewProp1).IsEqualTo("initial");
    }
}

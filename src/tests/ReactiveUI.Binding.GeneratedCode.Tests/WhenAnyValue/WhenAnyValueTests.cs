// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenAnyValue;

/// <summary>
/// Tests that the source-generator-generated WhenAnyValue code works correctly at runtime.
/// </summary>
public class WhenAnyValueTests
{
    /// <summary>
    /// Verifies that single-property WhenAnyValue emits initial value and subsequent changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsInitialAndChanges()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "A" };
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.SingleProperty(fixture)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("A");

        fixture.Value1 = "B";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values).Contains("B");
    }

    /// <summary>
    /// Verifies that two-property WhenAnyValue emits tuples.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_EmitsTuples()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "A", Value2 = "B" };
        var values = new List<(string property1, string property2)>();

        using var sub = WhenAnyValueScenarios.TwoProperties(fixture)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].property1).IsEqualTo("A");
        await Assert.That(values[0].property2).IsEqualTo("B");
    }

    /// <summary>
    /// Verifies that three-property WhenAnyValue emits tuples.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ThreeProperties_EmitsTuples()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "A", Value2 = "B", Value3 = "C" };
        var values = new List<(string property1, string property2, string property3)>();

        using var sub = WhenAnyValueScenarios.ThreeProperties(fixture)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].property1).IsEqualTo("A");
        await Assert.That(values[0].property2).IsEqualTo("B");
        await Assert.That(values[0].property3).IsEqualTo("C");
    }

    /// <summary>
    /// Verifies that WhenAnyValue with a selector projects values correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSelector_ProjectsValues()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "Hello", Value2 = "World" };
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.WithSelector_TwoProperties(fixture)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Hello_World");
    }

    /// <summary>
    /// Verifies that WhenAnyValue emits all sequential changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SequentialChanges_EmitsAll()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "A" };
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.SingleProperty(fixture)
            .Subscribe(values.Add);

        fixture.Value1 = "B";
        fixture.Value1 = "C";
        fixture.Value1 = "D";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(4);
        await Assert.That(values[0]).IsEqualTo("A");
        await Assert.That(values[1]).IsEqualTo("B");
        await Assert.That(values[2]).IsEqualTo("C");
        await Assert.That(values[3]).IsEqualTo("D");
    }
}

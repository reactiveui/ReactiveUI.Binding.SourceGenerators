// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenChanged;

/// <summary>
/// Tests that the source-generator-generated WhenChanged code works correctly at runtime.
/// </summary>
public class WhenChangedTests
{
    /// <summary>
    /// The initial name value used by single-property tests.
    /// </summary>
    private const string InitialName = "Initial";

    /// <summary>
    /// The minimum number of emissions expected after a single change.
    /// </summary>
    private const int MinEmissionsAfterChange = 2;

    /// <summary>
    /// The minimum number of emissions expected after two changes.
    /// </summary>
    private const int MinEmissionsAfterTwoChanges = 3;

    /// <summary>
    /// The minimum number of emissions expected after three sequential changes.
    /// </summary>
    private const int MinEmissionsAfterThreeChanges = 4;

    /// <summary>
    /// The index of the second emission.
    /// </summary>
    private const int SecondEmissionIndex = 1;

    /// <summary>
    /// The index of the third emission.
    /// </summary>
    private const int ThirdEmissionIndex = 2;

    /// <summary>
    /// The index of the fourth emission.
    /// </summary>
    private const int FourthEmissionIndex = 3;

    /// <summary>
    /// The two-property test value for the integer property.
    /// </summary>
    private const int TwoPropIntValue = 42;

    /// <summary>
    /// The updated value for the integer property in two-property tests.
    /// </summary>
    private const int UpdatedIntValue = 2;

    /// <summary>
    /// The three-property test value for the integer property.
    /// </summary>
    private const int ThreePropIntValue = 10;

    /// <summary>
    /// The four-property test value for the integer property.
    /// </summary>
    private const int FourPropIntValue = 20;

    /// <summary>
    /// The three-property test value for the double property.
    /// </summary>
    private const double ThreePropDoubleValue = 3.14;

    /// <summary>
    /// The four-property test value for the double property.
    /// </summary>
    private const double FourPropDoubleValue = 2.71;

    /// <summary>
    /// Verifies that a single-property WhenChanged emits the initial value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsInitialValue()
    {
        var vm = new TestViewModel { Name = InitialName };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(InitialName);
    }

    /// <summary>
    /// Verifies that a single-property WhenChanged emits when the property changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsOnChange()
    {
        var vm = new TestViewModel { Name = InitialName };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        vm.Name = "Changed";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(MinEmissionsAfterChange);
        await Assert.That(values).Contains("Changed");
    }

    /// <summary>
    /// Verifies that a single-property WhenChanged emits all sequential changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsSequentialChanges()
    {
        var vm = new TestViewModel { Name = "A" };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        vm.Name = "B";
        vm.Name = "C";
        vm.Name = "D";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(MinEmissionsAfterThreeChanges);
        await Assert.That(values[0]).IsEqualTo("A");
        await Assert.That(values[SecondEmissionIndex]).IsEqualTo("B");
        await Assert.That(values[ThirdEmissionIndex]).IsEqualTo("C");
        await Assert.That(values[FourthEmissionIndex]).IsEqualTo("D");
    }

    /// <summary>
    /// Verifies that a two-property WhenChanged emits the initial tuple.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_EmitsInitialTuple()
    {
        var vm = new BigViewModel { Prop1 = "Hello", Prop2 = TwoPropIntValue };
        var values = new List<(string property1, int property2)>();

        using var sub = WhenChangedScenarios.TwoProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].property1).IsEqualTo("Hello");
        await Assert.That(values[0].property2).IsEqualTo(TwoPropIntValue);
    }

    /// <summary>
    /// Verifies that a two-property WhenChanged emits when either property changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_EmitsOnEitherChange()
    {
        var vm = new BigViewModel { Prop1 = "A", Prop2 = 1 };
        var values = new List<(string property1, int property2)>();

        using var sub = WhenChangedScenarios.TwoProperties(vm)
            .Subscribe(values.Add);

        vm.Prop1 = "B";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(MinEmissionsAfterChange);
        await Assert.That(values[^1].property1).IsEqualTo("B");
        await Assert.That(values[^1].property2).IsEqualTo(1);

        vm.Prop2 = UpdatedIntValue;

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(MinEmissionsAfterTwoChanges);
        await Assert.That(values[^1].property1).IsEqualTo("B");
        await Assert.That(values[^1].property2).IsEqualTo(UpdatedIntValue);
    }

    /// <summary>
    /// Verifies that three-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ThreeProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "X", Prop2 = ThreePropIntValue, Prop3 = ThreePropDoubleValue };
        var values = new List<(string property1, int property2, double property3)>();

        using var sub = WhenChangedScenarios.ThreeProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].property1).IsEqualTo("X");
        await Assert.That(values[0].property2).IsEqualTo(ThreePropIntValue);
        await Assert.That(values[0].property3).IsEqualTo(ThreePropDoubleValue);
    }

    /// <summary>
    /// Verifies that four-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FourProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "Y", Prop2 = FourPropIntValue, Prop3 = FourPropDoubleValue, Prop4 = true };
        var values = new List<(string property1, int property2, double property3, bool property4)>();

        using var sub = WhenChangedScenarios.FourProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].property1).IsEqualTo("Y");
        await Assert.That(values[0].property2).IsEqualTo(FourPropIntValue);
        await Assert.That(values[0].property3).IsEqualTo(FourPropDoubleValue);
        await Assert.That(values[0].property4).IsTrue();
    }

    /// <summary>
    /// Verifies that WhenChanged with a selector combines property values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSelector_CombinesValues()
    {
        var vm = new BigViewModel { Prop1 = "Hello", Prop2 = TwoPropIntValue };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.WithSelector_TwoProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Hello_42");
    }

    /// <summary>
    /// Verifies that deep chain WhenChanged emits the nested property value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_EmitsNestedPropertyValue()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Seattle");
    }

    /// <summary>
    /// Verifies that deep chain WhenChanged emits when the nested property changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_EmitsOnNestedPropertyChange()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        vm.Address.City = "Portland";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(MinEmissionsAfterChange);
        await Assert.That(values).Contains("Portland");
    }

    /// <summary>
    /// Verifies that disposing the subscription stops listening for changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Disposal_StopsListening()
    {
        var vm = new TestViewModel { Name = InitialName };
        var values = new List<string>();

        var sub = WhenChangedScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        sub.Dispose();

        vm.Name = "AfterDisposal";

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(InitialName);
    }
}

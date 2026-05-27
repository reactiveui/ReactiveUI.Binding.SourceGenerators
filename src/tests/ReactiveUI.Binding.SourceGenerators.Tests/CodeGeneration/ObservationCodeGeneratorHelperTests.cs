// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>
/// Tests for <see cref="ObservationCodeGenerator"/> helper methods.
/// </summary>
public partial class ObservationCodeGeneratorHelperTests
{
    /// <summary>
    /// Verifies GetSelectorType returns correct Func type for single-property invocation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetSelectorType_SingleProperty_ReturnsFuncType()
    {
        var inv = ModelFactory.CreateInvocationInfo(
            returnTypeFullName: "global::System.String",
            hasSelector: true);

        var result = ObservationCodeGenerator.GetSelectorType(inv);

        await Assert.That(result).IsEqualTo("global::System.Func<global::System.String, global::System.String>");
    }

    /// <summary>
    /// Verifies GetSelectorType returns correct Func type for multi-property invocation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetSelectorType_MultiProperty_ReturnsFuncType()
    {
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment("Name", "global::System.String")
            ]),
            new([
                ModelFactory.CreatePropertyPathSegment("Age", "global::System.Int32")
            ])
        ]);

        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: "global::System.String",
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(["x => x.Name", "x => x.Age"]));

        var result = ObservationCodeGenerator.GetSelectorType(inv);

        await Assert.That(result)
            .IsEqualTo("global::System.Func<global::System.String, global::System.Int32, global::System.String>");
    }

    /// <summary>
    /// Verifies GroupByTypeSignature groups invocations with the same source and return types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_SameSignature_GroupedTogether()
    {
        var inv1 = ModelFactory.CreateInvocationInfo(callerLineNumber: 10);
        var inv2 = ModelFactory.CreateInvocationInfo(callerLineNumber: 20);
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = ObservationCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(1);
        await Assert.That(groups[0].Invocations.Length).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies GroupByTypeSignature separates invocations with different return types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_DifferentReturnTypes_SeparateGroups()
    {
        var inv1 = ModelFactory.CreateInvocationInfo(returnTypeFullName: "global::System.String");
        var inv2 = ModelFactory.CreateInvocationInfo(returnTypeFullName: "global::System.Int32");
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = ObservationCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies GenerateMultiPropertyObservation with deep chain and IReactiveObject before-change.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task
        GenerateMultiPropertyObservation_ReactiveObjectBeforeChange_WithDeepChain_GeneratesPropertyChangingVariables()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment("Name", "global::System.String")
            ]),
            new([
                ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
                ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address")
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: "global::System.String",
            isBeforeChange: true,
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(["x => x.Name", "x => x.Address.City"]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChanging");
        await Assert.That(result).Contains("CombineLatest");
    }

    /// <summary>
    /// Verifies IsINPC returns true when classInfo implements IReactiveObject.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPC_ReactiveObject_ReturnsTrue()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);
        await Assert.That(ObservationCodeGenerator.IsINPC(classInfo)).IsTrue();
    }

    /// <summary>
    /// Verifies IsINPC returns true when classInfo implements INPC directly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPC_DirectINPC_ReturnsTrue()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        await Assert.That(ObservationCodeGenerator.IsINPC(classInfo)).IsTrue();
    }

    /// <summary>
    /// Verifies IsINPC returns false when classInfo has no INPC support.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPC_NoINPC_ReturnsFalse()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo();
        await Assert.That(ObservationCodeGenerator.IsINPC(classInfo)).IsFalse();
    }

    /// <summary>
    /// Verifies IsINPC returns false when classInfo is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPC_NullClassInfo_ReturnsFalse() =>
        await Assert.That(ObservationCodeGenerator.IsINPC(null)).IsFalse();

    /// <summary>
    /// Verifies IsINPChanging returns true when classInfo implements IReactiveObject.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPChanging_ReactiveObject_ReturnsTrue()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);
        await Assert.That(ObservationCodeGenerator.IsINPChanging(classInfo)).IsTrue();
    }

    /// <summary>
    /// Verifies IsINPChanging returns true when classInfo implements INPChanging directly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPChanging_DirectINPChanging_ReturnsTrue()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);
        await Assert.That(ObservationCodeGenerator.IsINPChanging(classInfo)).IsTrue();
    }

    /// <summary>
    /// Verifies IsINPChanging returns false when classInfo has no INPChanging support.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPChanging_NoINPChanging_ReturnsFalse()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo();
        await Assert.That(ObservationCodeGenerator.IsINPChanging(classInfo)).IsFalse();
    }

    /// <summary>
    /// Verifies IsINPChanging returns false when classInfo is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsINPChanging_NullClassInfo_ReturnsFalse() =>
        await Assert.That(ObservationCodeGenerator.IsINPChanging(null)).IsFalse();

    /// <summary>
    /// Verifies GetTypeCastName returns the fully qualified name when classInfo is provided.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetTypeCastName_WithClassInfo_ReturnsFullyQualifiedName()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        var result = ObservationCodeGenerator.GetTypeCastName(classInfo);
        await Assert.That(result).IsEqualTo(classInfo.FullyQualifiedName);
    }

    /// <summary>
    /// Verifies GetTypeCastName returns "object" when classInfo is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetTypeCastName_NullClassInfo_ReturnsObject()
    {
        var result = ObservationCodeGenerator.GetTypeCastName(null);
        await Assert.That(result).IsEqualTo("object");
    }
}

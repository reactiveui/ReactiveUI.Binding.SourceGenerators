// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>
/// Tests for <see cref="ObservationCodeGenerator"/> — deep-chain and inline observation.
/// </summary>
public partial class ObservationCodeGeneratorHelperTests
{
    /// <summary>
    /// Verifies GenerateDeepChainObservation generates Select/Switch pattern for two-level chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainObservation_TwoLevelChain_GeneratesSelectSwitch()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
                ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address")
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            expressionTexts: new EquatableArray<string>(["x => x.Address.City"]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateDeepChainObservation(sb, inv, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("__obs0");
        await Assert.That(result).Contains("__obs1");
        await Assert.That(result).Contains("RxBindingExtensions.Select(");
        await Assert.That(result).Contains("RxBindingExtensions.Switch(");
        await Assert.That(result).Contains("RxBindingExtensions.DistinctUntilChanged(");
    }

    /// <summary>
    /// Verifies GenerateDeepChainObservation for before-change does not add DistinctUntilChanged.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainObservation_BeforeChange_NoDistinctUntilChanged()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
                ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address")
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            isBeforeChange: true,
            expressionTexts: new EquatableArray<string>(["x => x.Address.City"]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateDeepChainObservation(sb, inv, classInfo, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChanging");
        await Assert.That(result).DoesNotContain("DistinctUntilChanged");
    }

    /// <summary>
    /// Verifies GenerateDeepChainVariable generates variable declarations for deep chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainVariable_TwoLevelChain_GeneratesVariableDeclarations()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
            ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address")
        ]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, false, "__propObs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("var __propObs0_s0");
        await Assert.That(result).Contains("var __propObs0_s1");
        await Assert.That(result).Contains("var __propObs0");
        await Assert.That(result).Contains("RxBindingExtensions.Switch(");
        await Assert.That(result).Contains("RxBindingExtensions.DistinctUntilChanged(");
    }

    /// <summary>
    /// Verifies GenerateDeepChainVariable with isBeforeChange=true generates PropertyChanging code and no DistinctUntilChanged.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainVariable_BeforeChange_GeneratesPropertyChangingCode()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
            ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address")
        ]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, true, "__propObs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChanging");
        await Assert.That(result).DoesNotContain("DistinctUntilChanged");
    }

    /// <summary>
    /// Verifies EmitInlineObservation with single property INPC generates PropertyObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_SingleProperty_INPC_GeneratesPropertyObservable()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Name")]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            "source",
            path,
            "global::System.String",
            classInfo,
            "sourceObs");

        var result = sb.ToString();
        await Assert.That(result).Contains("var sourceObs");
        await Assert.That(result).Contains("PropertyObservable");
    }

    /// <summary>
    /// Verifies EmitInlineObservation with single property and no INPC generates ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_SingleProperty_NoINPC_GeneratesReturnObservable()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Name")]);
        var classInfo = ModelFactory.CreateClassBindingInfo();

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            "source",
            path,
            "global::System.String",
            classInfo,
            "sourceObs");

        var result = sb.ToString();
        await Assert.That(result).Contains("var sourceObs");
        await Assert.That(result).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies EmitInlineObservation with deep chain generates Select/Switch pattern.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_DeepChain_GeneratesSelectSwitchPattern()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
            ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address")
        ]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            "source",
            path,
            "global::System.String",
            classInfo,
            "sourceObs");

        var result = sb.ToString();
        await Assert.That(result).Contains("RxBindingExtensions.Switch(");
        await Assert.That(result).Contains("RxBindingExtensions.DistinctUntilChanged(");
        await Assert.That(result).Contains("var sourceObs");
    }

    /// <summary>
    /// Verifies GenerateDeepChainVariable with null classInfo generates after-change code with ReturnObservable fallback.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainVariable_NullClassInfo_GeneratesAfterChangeCode()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
            ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address")
        ]);

        ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, null, false, "__propObs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("var __propObs0_s0");
        await Assert.That(result).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies GenerateDeepChainVariable with IReactiveObject after-change.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainVariable_ReactiveObjectAfterChange_GeneratesPropertyObservable()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
            ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address")
        ]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, false, "__propObs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains("DistinctUntilChanged");
    }

    /// <summary>
    /// Verifies GenerateDeepChainObservation with null classInfo generates after-change code with ReturnObservable fallback.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainObservation_NullClassInfo_GeneratesCode()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
                ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address")
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            expressionTexts: new EquatableArray<string>(["x => x.Address.City"]));

        ObservationCodeGenerator.GenerateDeepChainObservation(sb, inv, null, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("ReturnObservable");
        await Assert.That(result).Contains("Switch");
    }

    /// <summary>
    /// Verifies GenerateDeepChainObservation with IReactiveObject after-change.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainObservation_ReactiveObjectAfterChange_GeneratesPropertyObservable()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
                ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address")
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            expressionTexts: new EquatableArray<string>(["x => x.Address.City"]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateDeepChainObservation(sb, inv, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains("DistinctUntilChanged");
    }

    /// <summary>
    /// Verifies EmitInlineObservation with null classInfo generates PropertyObservable (uses null-safe path).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_NullClassInfo_SingleProperty_GeneratesPropertyObservable()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Name")]);

        ObservationCodeGenerator.EmitInlineObservation(sb, "source", path, "global::System.String", null, "sourceObs");

        var result = sb.ToString();
        await Assert.That(result).Contains("var sourceObs");
    }

    /// <summary>
    /// Verifies EmitInlineObservation with IReactiveObject generates PropertyObservable for single property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_ReactiveObject_SingleProperty_GeneratesPropertyObservable()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Name")]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            "source",
            path,
            "global::System.String",
            classInfo,
            "sourceObs");

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyObservable");
    }

    /// <summary>
    /// Verifies EmitInlineObservation with IReactiveObject and deep chain generates Switch pattern.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_ReactiveObject_DeepChain_GeneratesSwitchPattern()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
            ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address")
        ]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            "source",
            path,
            "global::System.String",
            classInfo,
            "sourceObs");

        var result = sb.ToString();
        await Assert.That(result).Contains("RxBindingExtensions.Switch(");
        await Assert.That(result).Contains("PropertyObservable");
    }

    /// <summary>
    /// Verifies EmitInlineObservation with deep chain and null classInfo generates ReturnObservable fallback.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInlineObservation_DeepChain_NullClassInfo_GeneratesReturnObservable()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>([
            ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
            ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address")
        ]);

        ObservationCodeGenerator.EmitInlineObservation(sb, "source", path, "global::System.String", null, "sourceObs");

        var result = sb.ToString();
        await Assert.That(result).Contains("ReturnObservable");
        await Assert.That(result).Contains("__sourceObs_s0");
        await Assert.That(result).Contains("__sourceObs_s1");
        await Assert.That(result).Contains("Switch");
        await Assert.That(result).Contains("DistinctUntilChanged");
    }
}

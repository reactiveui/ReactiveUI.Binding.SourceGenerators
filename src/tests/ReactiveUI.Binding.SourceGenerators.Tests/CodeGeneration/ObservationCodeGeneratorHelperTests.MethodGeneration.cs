// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>
/// Tests for <see cref="ObservationCodeGenerator"/> — overload, method, runtime-fallback and affinity generation.
/// </summary>
public partial class ObservationCodeGeneratorHelperTests
{
    /// <summary>
    /// Verifies GenerateConcreteOverload with CallerArgExpr mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExpr_GeneratesExpressionDispatch()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo();
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, true, "WhenChanged");

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerArgumentExpression");
        await Assert.That(result).Contains("__WhenChanged_");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload with CallerFilePath mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerFilePath_GeneratesFilePathDispatch()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo();
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, false, "WhenChanged");

        var result = sb.ToString();
        await Assert.That(result).Contains("callerLineNumber");
        await Assert.That(result).Contains("callerFilePath.EndsWith");
    }

    /// <summary>
    /// Verifies GenerateRuntimeFallback for single property generates throw (no runtime reflection fallback).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateRuntimeFallback_SingleProperty_GeneratesThrow()
    {
        var sb = new StringBuilder();

        ObservationCodeGenerator.GenerateRuntimeFallback(sb, "WhenChanged", 1, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("throw new global::System.InvalidOperationException");
        await Assert.That(result).Contains("WhenChanged");
    }

    /// <summary>
    /// Verifies GenerateRuntimeFallback for multi-property generates throw (no runtime reflection fallback).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateRuntimeFallback_MultiPropertyWithSelector_GeneratesThrow()
    {
        var sb = new StringBuilder();

        ObservationCodeGenerator.GenerateRuntimeFallback(sb, "WhenChanged", 2, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("throw new global::System.InvalidOperationException");
        await Assert.That(result).Contains("WhenChanged");
    }

    /// <summary>
    /// Verifies GenerateRuntimeFallback for multi-property without selector generates throw.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateRuntimeFallback_MultiPropertyNoSelector_GeneratesThrow()
    {
        var sb = new StringBuilder();

        ObservationCodeGenerator.GenerateRuntimeFallback(sb, "WhenChanged", 2, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("throw new global::System.InvalidOperationException");
    }

    /// <summary>
    /// Verifies GenerateRuntimeFallback for WhenChanging includes correct method prefix in error message.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateRuntimeFallback_WhenChanging_IncludesMethodPrefixInErrorMessage()
    {
        var sb = new StringBuilder();

        ObservationCodeGenerator.GenerateRuntimeFallback(sb, "WhenChanging", 1, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("throw new global::System.InvalidOperationException");
        await Assert.That(result).Contains("WhenChanging");
    }

    /// <summary>
    /// Verifies GenerateObservationMethod with a deep chain and selector generates Switch and Select wrapping.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateObservationMethod_DeepChainWithSelector_GeneratesSwitchPattern()
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
            returnTypeFullName: "global::System.Int32",
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(["x => x.Address.City"]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateObservationMethod(sb, inv, classInfo, "DEADBEEF", false, "WhenChanged");

        var result = sb.ToString();
        await Assert.That(result).Contains("RxBindingExtensions.Switch(");
        await Assert.That(result).Contains("__WhenChanged_DEADBEEF");
    }

    /// <summary>
    /// Verifies GenerateObservationMethod with a single property and selector generates Select wrapping.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateObservationMethod_SinglePropertyWithSelector_GeneratesSelectWrap()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo(
            returnTypeFullName: "global::System.Int32",
            hasSelector: true);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateObservationMethod(sb, inv, classInfo, "CAFEBABE", false, "WhenChanged");

        var result = sb.ToString();
        await Assert.That(result).Contains("RxBindingExtensions.Select(");
        await Assert.That(result).Contains("selector");
    }

    /// <summary>
    /// Verifies GenerateObservationMethod with null classInfo generates code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateObservationMethod_NullClassInfo_GeneratesCode()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo();

        ObservationCodeGenerator.GenerateObservationMethod(sb, inv, null, "DEADBEEF", false, "WhenChanged");

        var result = sb.ToString();
        await Assert.That(result).Contains("__WhenChanged_DEADBEEF");
    }

    /// <summary>
    /// Verifies GenerateObservationMethod single property without selector generates direct return.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateObservationMethod_SinglePropertyNoSelector_GeneratesDirectReturn()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo(hasSelector: false);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateObservationMethod(sb, inv, classInfo, "ABC123", false, "WhenChanged");

        var result = sb.ToString();
        await Assert.That(result).Contains("__WhenChanged_ABC123");
        await Assert.That(result).Contains("PropertyObservable");
    }

    /// <summary>
    /// Verifies Generate with empty invocations returns null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_EmptyInvocations_ReturnsNull()
    {
        var result = ObservationCodeGenerator.Generate(
            [],
            [],
            new LanguageFeatures(true, true, true),
            "WhenChanged");

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies Generate with default invocations returns null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_DefaultInvocations_ReturnsNull()
    {
        var result = ObservationCodeGenerator.Generate(
            default,
            [],
            new LanguageFeatures(true, true, true),
            "WhenChanged");

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies Generate with valid invocations returns non-null source containing method prefix.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_WithInvocations_ReturnsNonNullSource()
    {
        var inv = ModelFactory.CreateInvocationInfo();
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        var result = ObservationCodeGenerator.Generate(
            [inv],
            [classInfo],
            new LanguageFeatures(true, true, true),
            "WhenChanged");

        await Assert.That(result).IsNotNull();
        await Assert.That(result!).Contains("__WhenChanged_");
    }

    /// <summary>
    /// Verifies Generate with invocations but no matching class info skips affinity check
    /// and does not emit ObservationAffinityChecker (covers null branches for groupClassInfo/groupPlugin).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_NoMatchingClassInfo_SkipsAffinityCheck()
    {
        var inv = ModelFactory.CreateInvocationInfo();

        var result = ObservationCodeGenerator.Generate(
            [inv],
            [],
            new LanguageFeatures(true, true, true),
            "WhenChanged");

        await Assert.That(result).IsNotNull();
        await Assert.That(result!).DoesNotContain("ObservationAffinityChecker");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload with multiple invocations in a group generates else if branching.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_MultipleInvocationsInGroup_GeneratesElseIf()
    {
        var sb = new StringBuilder();
        var inv1 = ModelFactory.CreateInvocationInfo(callerLineNumber: 10, expressionTexts: new EquatableArray<string>([
            "x => x.Name"
        ]));
        var inv2 = ModelFactory.CreateInvocationInfo(callerLineNumber: 20, expressionTexts: new EquatableArray<string>([
            "x => x.Age"
        ]));
        var group = new ObservationCodeGenerator.TypeGroup(inv1, [inv1, inv2]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, true, "WhenChanged");

        var result = sb.ToString();
        await Assert.That(result).Contains("if (");
        await Assert.That(result).Contains("else if (");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload with selector generates selector parameter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_WithSelector_GeneratesSelectorParameter()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo(returnTypeFullName: "global::System.Int32", hasSelector: true);
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, true, "WhenChanged");

        var result = sb.ToString();
        await Assert.That(result).Contains("Func<");
        await Assert.That(result).Contains("selector");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload with CallerArgExpr and multi-property generates multiple expression checks with AND.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExpr_MultiProperty_GeneratesMultipleExpressionChecks()
    {
        var sb = new StringBuilder();
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
            returnTypeFullName: "(global::System.String, global::System.Int32)",
            hasSelector: false,
            expressionTexts: new EquatableArray<string>(["x => x.Name", "x => x.Age"]));
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, true, "WhenChanged");

        var result = sb.ToString();
        await Assert.That(result).Contains("property1Expression");
        await Assert.That(result).Contains("property2Expression");
        await Assert.That(result).Contains("&&");
    }

    /// <summary>
    /// Verifies GenerateObservationMethod generates method signature with correct prefix and suffix.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateObservationMethod_SingleProperty_GeneratesMethodSignature()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo();
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateObservationMethod(
            sb,
            inv,
            classInfo,
            "ABCDEF0123456789",
            false,
            "WhenChanged");

        var result = sb.ToString();
        await Assert.That(result).Contains("__WhenChanged_ABCDEF0123456789");
        await Assert.That(result).Contains("private static");
        await Assert.That(result).Contains("global::System.IObservable");
    }

    /// <summary>
    /// Verifies EmitAffinityCheck emits HasHigherAffinityPlugin check with correct type and affinity.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitAffinityCheck_SingleProperty_EmitsCorrectCheck()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo();

        ObservationCodeGenerator.EmitAffinityCheck(sb, inv, "WhenChanged", 1, false, 5);

        var result = sb.ToString();
        await Assert.That(result).Contains("ObservationAffinityChecker.HasHigherAffinityPlugin");
        await Assert.That(result).Contains("typeof(global::TestApp.MyViewModel)");
        await Assert.That(result).Contains(", 5, false)");
    }

    /// <summary>
    /// Verifies EmitAffinityCheck emits beforeChanged=true for WhenChanging.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitAffinityCheck_WhenChanging_EmitsBeforeChangedTrue()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo(isBeforeChange: true);

        ObservationCodeGenerator.EmitAffinityCheck(sb, inv, "WhenChanging", 1, false, 10);

        var result = sb.ToString();
        await Assert.That(result).Contains(", 10, true)");
    }

    /// <summary>
    /// Verifies EmitAffinityFallbackReturn emits direct RuntimeObservationFallback call for single property without selector.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitAffinityFallbackReturn_SinglePropertyNoSelector_EmitsDirectFallback()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo();

        ObservationCodeGenerator.EmitAffinityFallbackReturn(sb, inv, "WhenChanged", 1, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("RuntimeObservationFallback.WhenChanged(objectToMonitor, property1)");
    }

    /// <summary>
    /// Verifies EmitAffinityFallbackReturn emits SelectObservable wrapper for single property with selector.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitAffinityFallbackReturn_SinglePropertyWithSelector_EmitsSelectObservable()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo(returnTypeFullName: "global::System.Int32", hasSelector: true);

        ObservationCodeGenerator.EmitAffinityFallbackReturn(sb, inv, "WhenChanged", 1, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("SelectObservable<global::System.String, global::System.Int32>");
        await Assert.That(result).Contains("RuntimeObservationFallback.WhenChanged(objectToMonitor, property1)");
        await Assert.That(result).Contains("selector);");
    }

    /// <summary>
    /// Verifies EmitAffinityFallbackReturn emits multi-property fallback for two properties without selector.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitAffinityFallbackReturn_TwoPropertiesNoSelector_EmitsMultiPropertyFallback()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>(
        [
            new([ModelFactory.CreatePropertyPathSegment("Name", "global::System.String")]),
            new([ModelFactory.CreatePropertyPathSegment("Age", "int")])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            expressionTexts: new EquatableArray<string>(["x => x.Name", "x => x.Age"]));

        ObservationCodeGenerator.EmitAffinityFallbackReturn(sb, inv, "WhenChanged", 2, false);

        var result = sb.ToString();
        await Assert.That(result)
            .Contains("RuntimeObservationFallback.WhenChanged(objectToMonitor, property1, property2)");
    }

    /// <summary>
    /// Verifies EmitAffinityFallbackReturn emits tuple decomposition for multi-property with selector.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitAffinityFallbackReturn_TwoPropertiesWithSelector_EmitsTupleDecomposition()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>(
        [
            new([ModelFactory.CreatePropertyPathSegment("Name", "global::System.String")]),
            new([ModelFactory.CreatePropertyPathSegment("Age", "int")])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: "global::System.String",
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(["x => x.Name", "x => x.Age"]));

        ObservationCodeGenerator.EmitAffinityFallbackReturn(sb, inv, "WhenChanged", 2, true);

        var result = sb.ToString();
        await Assert.That(result)
            .Contains("SelectObservable<global::System.ValueTuple<global::System.String, int>, global::System.String>");
        await Assert.That(result).Contains("__t => selector(__t.Item1, __t.Item2)");
    }

    /// <summary>
    /// Verifies EmitAffinityFallbackReturn emits WhenChanging fallback for before-change observation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitAffinityFallbackReturn_WhenChanging_EmitsCorrectMethodName()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo(isBeforeChange: true);

        ObservationCodeGenerator.EmitAffinityFallbackReturn(sb, inv, "WhenChanging", 1, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("RuntimeObservationFallback.WhenChanging(objectToMonitor, property1)");
    }

    /// <summary>
    /// Verifies EmitAffinityFallbackReturn emits WhenAnyValue fallback correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitAffinityFallbackReturn_WhenAnyValue_EmitsCorrectMethodName()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo();

        ObservationCodeGenerator.EmitAffinityFallbackReturn(sb, inv, "WhenAnyValue", 1, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("RuntimeObservationFallback.WhenAnyValue(objectToMonitor, property1)");
    }

    /// <summary>
    /// Verifies EmitAffinityFallbackReturn emits three-property tuple decomposition with selector.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitAffinityFallbackReturn_ThreePropertiesWithSelector_EmitsThreeItemDecomposition()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>(
        [
            new([ModelFactory.CreatePropertyPathSegment("Name", "global::System.String")]),
            new([ModelFactory.CreatePropertyPathSegment("Age", "int")]),
            new([ModelFactory.CreatePropertyPathSegment("City", "global::System.String")])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: "global::System.String",
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(["x => x.Name", "x => x.Age", "x => x.City"]));

        ObservationCodeGenerator.EmitAffinityFallbackReturn(sb, inv, "WhenChanged", 3, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("__t => selector(__t.Item1, __t.Item2, __t.Item3)");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload emits affinity check when generatedAffinity is provided.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_WithAffinity_EmitsAffinityCheck()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo();
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, true, "WhenChanged", 5);

        var result = sb.ToString();
        await Assert.That(result).Contains("ObservationAffinityChecker.HasHigherAffinityPlugin");
        await Assert.That(result).Contains(", 5, false)");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload does not emit affinity check when generatedAffinity is -1.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_WithoutAffinity_NoAffinityCheck()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo();
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, true, "WhenChanged", -1);

        var result = sb.ToString();
        await Assert.That(result).DoesNotContain("ObservationAffinityChecker");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload skips affinity check for more than 3 properties.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_FourProperties_SkipsAffinityCheck()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>(
        [
            new([ModelFactory.CreatePropertyPathSegment("P1", "global::System.String")]),
            new([ModelFactory.CreatePropertyPathSegment("P2", "global::System.String")]),
            new([ModelFactory.CreatePropertyPathSegment("P3", "global::System.String")]),
            new([ModelFactory.CreatePropertyPathSegment("P4", "global::System.String")])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            expressionTexts: new EquatableArray<string>(["x => x.P1", "x => x.P2", "x => x.P3", "x => x.P4"]));
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, true, "WhenChanged", 5);

        var result = sb.ToString();
        await Assert.That(result).DoesNotContain("ObservationAffinityChecker");
    }
}

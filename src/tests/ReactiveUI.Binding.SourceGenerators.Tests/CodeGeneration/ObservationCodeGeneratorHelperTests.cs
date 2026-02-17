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
public class ObservationCodeGeneratorHelperTests
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
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>(new[]
        {
            new EquatableArray<PropertyPathSegment>(new[] { ModelFactory.CreatePropertyPathSegment("Name", "global::System.String") }),
            new EquatableArray<PropertyPathSegment>(new[] { ModelFactory.CreatePropertyPathSegment("Age", "global::System.Int32") }),
        });

        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: "global::System.String",
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(new[] { "x => x.Name", "x => x.Age" }));

        var result = ObservationCodeGenerator.GetSelectorType(inv);

        await Assert.That(result).IsEqualTo("global::System.Func<global::System.String, global::System.Int32, global::System.String>");
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
    /// Verifies GenerateSinglePropertyObservation generates INPC after-change code for ReactiveObject
    /// (ReactiveObject implements INPC, so the INPC path is used).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateSinglePropertyObservation_ReactiveObjectAfterChange_GeneratesPropertyChangedHandler()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo();
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateSinglePropertyObservation(
            sb, inv, classInfo, "obj.Name", "Name", isBeforeChange: false);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains("\"Name\"");
        await Assert.That(result).Contains("INotifyPropertyChanged");
    }

    /// <summary>
    /// Verifies GenerateSinglePropertyObservation generates INPChanging before-change code for ReactiveObject
    /// (ReactiveObject implements INPChanging, so the INPChanging path is used).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateSinglePropertyObservation_ReactiveObjectBeforeChange_GeneratesPropertyChangingHandler()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo(isBeforeChange: true);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateSinglePropertyObservation(
            sb, inv, classInfo, "obj.Name", "Name", isBeforeChange: true);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChangingObservable");
        await Assert.That(result).Contains("\"Name\"");
    }

    /// <summary>
    /// Verifies GenerateSinglePropertyObservation generates INPC after-change code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateSinglePropertyObservation_INPCAfterChange_GeneratesPropertyChangedHandler()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo();
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateSinglePropertyObservation(
            sb, inv, classInfo, "obj.Name", "Name", isBeforeChange: false);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains("INotifyPropertyChanged");
    }

    /// <summary>
    /// Verifies GenerateSinglePropertyObservation generates INPChanging before-change code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateSinglePropertyObservation_INPChangingBeforeChange_GeneratesPropertyChangingHandler()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo(isBeforeChange: true);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateSinglePropertyObservation(
            sb, inv, classInfo, "obj.Name", "Name", isBeforeChange: true);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChangingObservable");
        await Assert.That(result).Contains("INotifyPropertyChanging");
    }

    /// <summary>
    /// Verifies GenerateSinglePropertyObservation generates Observable.Return fallback.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateSinglePropertyObservation_NoInterface_GeneratesObservableReturn()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo();
        var classInfo = ModelFactory.CreateClassBindingInfo();

        ObservationCodeGenerator.GenerateSinglePropertyObservation(
            sb, inv, classInfo, "obj.Name", "Name", isBeforeChange: false);

        var result = sb.ToString();
        await Assert.That(result).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies GenerateShallowPathObservation for single segment delegates to single property logic.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowPathObservation_SingleSegment_GeneratesInlineObservation()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            new[] { ModelFactory.CreatePropertyPathSegment("Name") });
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, classInfo, isBeforeChange: false);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains("\"Name\"");
    }

    /// <summary>
    /// Verifies GenerateMultiPropertyObservation produces CombineLatest with multiple paths.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_MultiplePaths_GeneratesCombineLatest()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>(new[]
        {
            new EquatableArray<PropertyPathSegment>(new[] { ModelFactory.CreatePropertyPathSegment("Name", "global::System.String") }),
            new EquatableArray<PropertyPathSegment>(new[] { ModelFactory.CreatePropertyPathSegment("Age", "global::System.Int32") }),
        });

        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: "global::System.String",
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(new[] { "x => x.Name", "x => x.Age" }));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, isBeforeChange: false);

        var result = sb.ToString();
        await Assert.That(result).Contains("CombineLatest");
        await Assert.That(result).Contains("selector");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload with CallerArgExpr mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExpr_GeneratesExpressionDispatch()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo();
        var group = new ObservationCodeGenerator.TypeGroup(inv, new[] { inv });

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, supportsCallerArgExpr: true, "WhenChanged");

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
        var group = new ObservationCodeGenerator.TypeGroup(inv, new[] { inv });

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, supportsCallerArgExpr: false, "WhenChanged");

        var result = sb.ToString();
        await Assert.That(result).Contains("callerLineNumber");
        await Assert.That(result).Contains("callerFilePath.EndsWith");
    }

    /// <summary>
    /// Verifies GenerateShallowObservableVariable generates INPC variable for after-change.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_INPCAfterChange_GeneratesVariableDeclaration()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            new[] { ModelFactory.CreatePropertyPathSegment("Name") });
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, isBeforeChange: false, "__propObs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("var __propObs0");
        await Assert.That(result).Contains("PropertyObservable");
    }

    /// <summary>
    /// Verifies GenerateShallowObservableVariable generates INPChanging variable for before-change.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_INPChangingBeforeChange_GeneratesVariableDeclaration()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            new[] { ModelFactory.CreatePropertyPathSegment("Name") });
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, isBeforeChange: true, "__propObs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("var __propObs0");
        await Assert.That(result).Contains("PropertyChanging");
        await Assert.That(result).Contains("INotifyPropertyChanging");
    }

    /// <summary>
    /// Verifies GenerateShallowObservableVariable generates Observable.Return fallback.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_NoInterface_GeneratesObservableReturn()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            new[] { ModelFactory.CreatePropertyPathSegment("Name") });
        var classInfo = ModelFactory.CreateClassBindingInfo();

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, isBeforeChange: false, "__propObs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("var __propObs0");
        await Assert.That(result).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies GenerateShallowPathObservation for before-change produces PropertyChanging code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowPathObservation_BeforeChange_GeneratesPropertyChangingCode()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            new[] { ModelFactory.CreatePropertyPathSegment("Name") });
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, classInfo, isBeforeChange: true);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChanging");
        await Assert.That(result).Contains("INotifyPropertyChanging");
    }

    /// <summary>
    /// Verifies GenerateShallowPathObservation with no interface generates Observable.Return.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowPathObservation_NoInterface_GeneratesObservableReturn()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            new[] { ModelFactory.CreatePropertyPathSegment("Name") });
        var classInfo = ModelFactory.CreateClassBindingInfo();

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, classInfo, isBeforeChange: false);

        var result = sb.ToString();
        await Assert.That(result).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies GenerateDeepChainObservation generates Select/Switch pattern for two-level chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateDeepChainObservation_TwoLevelChain_GeneratesSelectSwitch()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>(new[]
        {
            new EquatableArray<PropertyPathSegment>(new[]
            {
                ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
                ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address"),
            }),
        });
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            expressionTexts: new EquatableArray<string>(new[] { "x => x.Address.City" }));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateDeepChainObservation(sb, inv, classInfo, isBeforeChange: false);

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
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>(new[]
        {
            new EquatableArray<PropertyPathSegment>(new[]
            {
                ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
                ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address"),
            }),
        });
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            isBeforeChange: true,
            expressionTexts: new EquatableArray<string>(new[] { "x => x.Address.City" }));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateDeepChainObservation(sb, inv, classInfo, isBeforeChange: true);

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
        var path = new EquatableArray<PropertyPathSegment>(new[]
        {
            ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
            ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address"),
        });
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, isBeforeChange: false, "__propObs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("var __propObs0_s0");
        await Assert.That(result).Contains("var __propObs0_s1");
        await Assert.That(result).Contains("var __propObs0");
        await Assert.That(result).Contains("RxBindingExtensions.Switch(");
        await Assert.That(result).Contains("RxBindingExtensions.DistinctUntilChanged(");
    }

    /// <summary>
    /// Verifies GenerateRuntimeFallback for single property generates throw (no runtime reflection fallback).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateRuntimeFallback_SingleProperty_GeneratesThrow()
    {
        var sb = new StringBuilder();

        ObservationCodeGenerator.GenerateRuntimeFallback(sb, "WhenChanged", propCount: 1, hasSelector: false);

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

        ObservationCodeGenerator.GenerateRuntimeFallback(sb, "WhenChanged", propCount: 2, hasSelector: true);

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

        ObservationCodeGenerator.GenerateRuntimeFallback(sb, "WhenChanged", propCount: 2, hasSelector: false);

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

        ObservationCodeGenerator.GenerateRuntimeFallback(sb, "WhenChanging", propCount: 1, hasSelector: false);

        var result = sb.ToString();
        await Assert.That(result).Contains("throw new global::System.InvalidOperationException");
        await Assert.That(result).Contains("WhenChanging");
    }

    /// <summary>
    /// Verifies GenerateMultiPropertyObservation with no selector generates tuple lambda.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_NoSelector_GeneratesTupleResult()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>(new[]
        {
            new EquatableArray<PropertyPathSegment>(new[] { ModelFactory.CreatePropertyPathSegment("Name", "global::System.String") }),
            new EquatableArray<PropertyPathSegment>(new[] { ModelFactory.CreatePropertyPathSegment("Age", "global::System.Int32") }),
        });
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: "(global::System.String property1, global::System.Int32 property2)",
            hasSelector: false,
            expressionTexts: new EquatableArray<string>(new[] { "x => x.Name", "x => x.Age" }));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, isBeforeChange: false);

        var result = sb.ToString();
        await Assert.That(result).Contains("CombineLatest");
        await Assert.That(result).Contains("property1: p1");
        await Assert.That(result).Contains("property2: p2");
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

        ObservationCodeGenerator.GenerateObservationMethod(sb, inv, classInfo, "ABCDEF0123456789", isBeforeChange: false, "WhenChanged");

        var result = sb.ToString();
        await Assert.That(result).Contains("__WhenChanged_ABCDEF0123456789");
        await Assert.That(result).Contains("private static");
        await Assert.That(result).Contains("global::System.IObservable");
    }
}

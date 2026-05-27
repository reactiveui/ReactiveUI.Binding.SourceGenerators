// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>
/// Tests for <see cref="ObservationCodeGenerator"/> — single-property and shallow-path observation.
/// </summary>
public partial class ObservationCodeGeneratorHelperTests
{
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
            sb,
            inv,
            classInfo,
            "obj.Name",
            "Name",
            false);

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
            sb,
            inv,
            classInfo,
            "obj.Name",
            "Name",
            true);

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
            sb,
            inv,
            classInfo,
            "obj.Name",
            "Name",
            false);

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
            sb,
            inv,
            classInfo,
            "obj.Name",
            "Name",
            true);

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
            sb,
            inv,
            classInfo,
            "obj.Name",
            "Name",
            false);

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
            [ModelFactory.CreatePropertyPathSegment("Name")]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains("\"Name\"");
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
            [ModelFactory.CreatePropertyPathSegment("Name")]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, false, "__propObs0");

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
            [ModelFactory.CreatePropertyPathSegment("Name")]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, true, "__propObs0");

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
            [ModelFactory.CreatePropertyPathSegment("Name")]);
        var classInfo = ModelFactory.CreateClassBindingInfo();

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, false, "__propObs0");

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
            [ModelFactory.CreatePropertyPathSegment("Name")]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, classInfo, true);

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
            [ModelFactory.CreatePropertyPathSegment("Name")]);
        var classInfo = ModelFactory.CreateClassBindingInfo();

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies GenerateShallowPathObservation with null classInfo generates Observable.Return fallback.
    /// Covers the <c>classInfo?.ImplementsIReactiveObject ?? false</c> null-propagation branch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowPathObservation_NullClassInfo_GeneratesObservableReturn()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Name")]);

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, null, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies GenerateShallowPathObservation with IReactiveObject after-change generates PropertyObservable.
    /// Covers the short-circuit OR path where ImplementsIReactiveObject is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowPathObservation_ReactiveObjectAfterChange_GeneratesPropertyObservable()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Name")]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyObservable");
    }

    /// <summary>
    /// Verifies GenerateShallowPathObservation with IReactiveObject before-change generates PropertyChangingObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowPathObservation_ReactiveObjectBeforeChange_GeneratesPropertyChangingObservable()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Name")]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateShallowPathObservation(sb, path, classInfo, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChangingObservable");
    }

    /// <summary>
    /// Verifies GenerateShallowObservableVariable with null classInfo generates ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_NullClassInfo_GeneratesReturnObservable()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Name")]);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, null, false, "__obs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies GenerateShallowObservableVariable with IReactiveObject after-change generates PropertyObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_ReactiveObjectAfterChange_GeneratesPropertyObservable()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Name")]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, false, "__obs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyObservable");
    }

    /// <summary>
    /// Verifies GenerateShallowObservableVariable with IReactiveObject before-change generates PropertyChangingObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateShallowObservableVariable_ReactiveObjectBeforeChange_GeneratesPropertyChangingObservable()
    {
        var sb = new StringBuilder();
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Name")]);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, true, "__obs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChangingObservable");
    }
}

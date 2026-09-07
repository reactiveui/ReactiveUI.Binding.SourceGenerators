// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="BindingEmitterHelpers"/>, shared by the four property-binding emitters.</summary>
public class BindingEmitterHelpersTests
{
    /// <summary>The fully qualified name of a source property type.</summary>
    private const string IntTypeName = "global::System.Int32";

    /// <summary>The fully qualified name of a target property type.</summary>
    private const string StringTypeName = "global::System.String";

    /// <summary>The fully qualified name of the view model a call site binds from.</summary>
    private const string ViewModelTypeName = "global::TestApp.MyViewModel";

    /// <summary>A type the view model derives from, which a view may expose it as.</summary>
    private const string ViewModelBaseTypeName = "global::TestApp.ViewModelBase";

    /// <summary>The name a view exposes its view model under.</summary>
    private const string ViewModelPropertyName = "ViewModel";

    /// <summary>A view exposing its view model as the concrete type is observed through it, unnarrowed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewModelObservation_ViewModelTypedExactly_ObservesTheViewWithoutNarrowing()
    {
        var observation = ResolveWithViewModelProperty(ViewModelTypeName);

        await Assert.That(observation.RootVariable).IsEqualTo("view");
        await Assert.That(observation.Path[0].ReadCastTypeFullName).IsNull();
    }

    /// <summary>
    /// A view exposing its view model as a base is still holding the view model the call site named, so the
    /// binding follows that property and the read narrows to the named type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewModelObservation_ViewModelTypedAsABase_ObservesTheViewAndNarrowsTheRead()
    {
        var observation = ResolveWithViewModelProperty(ViewModelBaseTypeName);

        await Assert.That(observation.RootVariable).IsEqualTo("view");
        await Assert.That(observation.Path[0].ReadCastTypeFullName).IsEqualTo(ViewModelTypeName);
    }

    /// <summary>
    /// The weakly typed declaration the non-generic view interface requires names no view model, so it is not
    /// followed - the call site handed the view model over directly, and the view may never have been given one.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewModelObservation_ViewModelTypedAsObject_ObservesTheViewModelItWasHanded()
    {
        var observation = ResolveWithViewModelProperty("object");

        await Assert.That(observation.RootVariable).IsEqualTo("viewModel");
    }

    /// <summary>A view exposing no view model at all leaves the binding on the one it was handed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewModelObservation_ViewDeclaresNoViewModel_ObservesTheViewModelItWasHanded()
    {
        var view = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        var observation = BindingEmitterHelpers.ResolveViewModelObservation(
            ModelFactory.CreateBindingInvocationInfo(),
            ModelFactory.CreateClassBindingInfo(implementsINPC: true),
            view);

        await Assert.That(observation.RootVariable).IsEqualTo("viewModel");
    }

    /// <summary>A supplied converter settles the conversion, so the registry is not asked for one.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RequiresRegistryConversion_ConverterSupplied_ReturnsFalse()
    {
        var group = Group(hasConversion: true, sourceType: IntTypeName, targetType: StringTypeName);

        await Assert.That(BindingEmitterHelpers.RequiresRegistryConversion(group)).IsFalse();
    }

    /// <summary>Two sides of the same type need no conversion at all.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RequiresRegistryConversion_SameTypeBothSides_ReturnsFalse()
    {
        var group = Group(hasConversion: false, sourceType: StringTypeName, targetType: StringTypeName);

        await Assert.That(BindingEmitterHelpers.RequiresRegistryConversion(group)).IsFalse();
    }

    /// <summary>Differing sides with no supplied converter are what the registry exists to serve.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RequiresRegistryConversion_DifferingTypesNoConverter_ReturnsTrue()
    {
        var group = Group(hasConversion: false, sourceType: IntTypeName, targetType: StringTypeName);

        await Assert.That(BindingEmitterHelpers.RequiresRegistryConversion(group)).IsTrue();
    }

    /// <summary>Resolves the observation for a view declaring its view model as the given type.</summary>
    /// <param name="declaredType">The type the view declares its view model property as.</param>
    /// <returns>The resolved observation.</returns>
    private static BindingEmitterHelpers.ViewModelObservation ResolveWithViewModelProperty(string declaredType)
    {
        var view = ModelFactory.CreateClassBindingInfo(
            implementsINPC: true,
            properties: new EquatableArray<ObservablePropertyInfo>(
                [ModelFactory.CreateObservablePropertyInfo(ViewModelPropertyName, declaredType)]));

        return BindingEmitterHelpers.ResolveViewModelObservation(
            ModelFactory.CreateBindingInvocationInfo(),
            ModelFactory.CreateClassBindingInfo(implementsINPC: true),
            view);
    }

    /// <summary>Builds a group fixing both property types and whether a converter was supplied.</summary>
    /// <param name="hasConversion">Whether the call site supplied a converter.</param>
    /// <param name="sourceType">The fully qualified source property type.</param>
    /// <param name="targetType">The fully qualified target property type.</param>
    /// <returns>The binding type group.</returns>
    private static BindingTypeGroup Group(bool hasConversion, string sourceType, string targetType) =>
        new(
            "global::TestApp.MyViewModel",
            "global::TestApp.MyView",
            sourceType,
            targetType,
            hasConversion,
            false,
            [ModelFactory.CreateBindingInvocationInfo()]);
}

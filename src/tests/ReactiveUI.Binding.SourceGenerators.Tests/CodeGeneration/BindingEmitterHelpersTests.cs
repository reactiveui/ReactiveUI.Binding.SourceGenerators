// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="BindingEmitterHelpers"/>, shared by the four property-binding emitters.</summary>
public class BindingEmitterHelpersTests
{
    /// <summary>The fully qualified name of a source property type.</summary>
    private const string IntTypeName = "global::System.Int32";

    /// <summary>The fully qualified name of a target property type.</summary>
    private const string StringTypeName = "global::System.String";

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

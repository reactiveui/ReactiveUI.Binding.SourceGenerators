// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

using NSubstitute;

using ReactiveUI.Binding.SourceGenerators.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>
/// Unit tests for <see cref="ExtractorValidation"/> helper methods.
/// Tests the guard-clause branches extracted from extractor classes.
/// </summary>
public class ExtractorValidationTests
{
    /// <summary>
    /// Verifies that the stub extension class name is recognized.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_StubClassName_ReturnsTrue()
    {
        var result = ExtractorValidation.IsRecognizedExtensionClass("ReactiveUIBindingExtensions");
        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that the scheduler extension class name is recognized.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_SchedulerClassName_ReturnsTrue()
    {
        var result = ExtractorValidation.IsRecognizedExtensionClass("ReactiveSchedulerExtensions");
        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that the generated extension class name is recognized.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_GeneratedClassName_ReturnsTrue()
    {
        var result = ExtractorValidation.IsRecognizedExtensionClass("__ReactiveUIGeneratedBindings");
        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that an unrecognized class name is rejected.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_UnknownClassName_ReturnsFalse()
    {
        var result = ExtractorValidation.IsRecognizedExtensionClass("CustomExtensions");
        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that null is rejected as unrecognized.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_Null_ReturnsFalse()
    {
        var result = ExtractorValidation.IsRecognizedExtensionClass(null);
        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that an empty string is rejected as unrecognized.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_Empty_ReturnsFalse()
    {
        var result = ExtractorValidation.IsRecognizedExtensionClass(string.Empty);
        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that argument count at the minimum is accepted.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasMinimumArguments_ExactMinimum_ReturnsTrue()
    {
        var result = ExtractorValidation.HasMinimumArguments(3, 3);
        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that argument count above the minimum is accepted.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasMinimumArguments_AboveMinimum_ReturnsTrue()
    {
        var result = ExtractorValidation.HasMinimumArguments(5, 3);
        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that argument count below the minimum is rejected.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasMinimumArguments_BelowMinimum_ReturnsFalse()
    {
        var result = ExtractorValidation.HasMinimumArguments(2, 3);
        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that zero arguments is rejected when minimum is required.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasMinimumArguments_Zero_ReturnsFalse()
    {
        var result = ExtractorValidation.HasMinimumArguments(0, 3);
        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that a populated immutable array is accepted.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasItems_PopulatedArray_ReturnsTrue()
    {
        var items = ImmutableArray.Create("a", "b");
        var result = ExtractorValidation.HasItems(items);
        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that a single-item array is accepted.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasItems_SingleItem_ReturnsTrue()
    {
        var items = ImmutableArray.Create("a");
        var result = ExtractorValidation.HasItems(items);
        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that an empty immutable array is rejected.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasItems_EmptyArray_ReturnsFalse()
    {
        var items = ImmutableArray<string>.Empty;
        var result = ExtractorValidation.HasItems(items);
        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that a default (uninitialized) immutable array is rejected.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasItems_DefaultArray_ReturnsFalse()
    {
        var items = default(ImmutableArray<string>);
        var result = ExtractorValidation.HasItems(items);
        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that ExtractMethodSymbol returns null for a default SymbolInfo.
    /// Exercises the "symbol is not IMethodSymbol" guard extracted from all extractors.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractMethodSymbol_DefaultSymbolInfo_ReturnsNull()
    {
        var result = ExtractorValidation.ExtractMethodSymbol(default);
        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that GetTypeDisplayName returns null for a null type symbol.
    /// Exercises the type info null guard extracted from all extractors.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetTypeDisplayName_NullType_ReturnsNull()
    {
        var result = ExtractorValidation.GetTypeDisplayName(null);
        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that ResolveEventArgsType returns the fallback for a null delegate type.
    /// Exercises the event delegate null guard extracted from EventHelpers.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveEventArgsType_NullDelegateType_ReturnsFallback()
    {
        var result = ExtractorValidation.ResolveEventArgsType(null);
        await Assert.That(result).IsEqualTo("global::System.EventArgs");
    }

    /// <summary>
    /// Verifies that FindSelectorReturnType returns null when the parameters array is empty.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindSelectorReturnType_EmptyParameters_ReturnsNull()
    {
        var result = ExtractorValidation.FindSelectorReturnType(
            ImmutableArray<IParameterSymbol>.Empty, "selector");

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that FindSelectorReturnType returns null when no parameter matches the name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindSelectorReturnType_NoMatchingParameter_ReturnsNull()
    {
        var typeArg = Substitute.For<ITypeSymbol>();
        typeArg.ToDisplayString(Arg.Any<SymbolDisplayFormat>()).Returns("string");

        var funcType = Substitute.For<INamedTypeSymbol>();
        funcType.TypeArguments.Returns(ImmutableArray.Create(typeArg));

        var param = Substitute.For<IParameterSymbol>();
        param.Name.Returns("otherParam");
        param.Type.Returns(funcType);

        var parameters = ImmutableArray.Create(param);

        var result = ExtractorValidation.FindSelectorReturnType(parameters, "selector");

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that FindSelectorReturnType returns the return type when a matching parameter is found.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindSelectorReturnType_MatchingParameter_ReturnsType()
    {
        var typeArg = Substitute.For<ITypeSymbol>();
        typeArg.ToDisplayString(Arg.Any<SymbolDisplayFormat>()).Returns("string");

        var funcType = Substitute.For<INamedTypeSymbol>();
        funcType.TypeArguments.Returns(ImmutableArray.Create(typeArg));

        var param = Substitute.For<IParameterSymbol>();
        param.Name.Returns("selector");
        param.Type.Returns(funcType);

        var parameters = ImmutableArray.Create(param);

        var result = ExtractorValidation.FindSelectorReturnType(parameters, "selector");

        await Assert.That(result).IsEqualTo("string");
    }

    /// <summary>
    /// Verifies that FindSelectorReturnType matches any of multiple parameter names.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindSelectorReturnType_MultipleNames_MatchesSecondName()
    {
        var typeArg = Substitute.For<ITypeSymbol>();
        typeArg.ToDisplayString(Arg.Any<SymbolDisplayFormat>()).Returns("int");

        var funcType = Substitute.For<INamedTypeSymbol>();
        funcType.TypeArguments.Returns(ImmutableArray.Create(typeArg));

        var param = Substitute.For<IParameterSymbol>();
        param.Name.Returns("conversionFunc");
        param.Type.Returns(funcType);

        var parameters = ImmutableArray.Create(param);

        var result = ExtractorValidation.FindSelectorReturnType(parameters, "selector", "conversionFunc");

        await Assert.That(result).IsEqualTo("int");
    }

    /// <summary>
    /// Verifies that FindSelectorReturnType skips parameters with non-generic types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindSelectorReturnType_NonGenericType_ReturnsNull()
    {
        var nonGenericType = Substitute.For<INamedTypeSymbol>();
        nonGenericType.TypeArguments.Returns(ImmutableArray<ITypeSymbol>.Empty);

        var param = Substitute.For<IParameterSymbol>();
        param.Name.Returns("selector");
        param.Type.Returns(nonGenericType);

        var parameters = ImmutableArray.Create(param);

        var result = ExtractorValidation.FindSelectorReturnType(parameters, "selector");

        await Assert.That(result).IsNull();
    }
}

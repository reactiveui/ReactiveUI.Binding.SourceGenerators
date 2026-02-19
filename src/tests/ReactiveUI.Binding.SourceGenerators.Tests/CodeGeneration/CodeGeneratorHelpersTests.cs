// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using System.Text;

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>
/// Tests for <see cref="CodeGeneratorHelpers"/> methods.
/// </summary>
public class CodeGeneratorHelpersTests
{
    /// <summary>
    /// Verifies BuildPropertyAccessChain with a single segment produces "root.Property".
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertyAccessChain_SingleSegment_ReturnsDottedPath()
    {
        var path = new EquatableArray<PropertyPathSegment>(
            new[] { ModelFactory.CreatePropertyPathSegment("Name") });

        var result = CodeGeneratorHelpers.BuildPropertyAccessChain("obj", path);

        await Assert.That(result).IsEqualTo("obj.Name");
    }

    /// <summary>
    /// Verifies BuildPropertyAccessChain with multiple segments produces "root.A.B".
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertyAccessChain_MultiSegment_ReturnsDottedPath()
    {
        var path = new EquatableArray<PropertyPathSegment>(new[]
        {
            ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
            ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address"),
        });

        var result = CodeGeneratorHelpers.BuildPropertyAccessChain("obj", path);

        await Assert.That(result).IsEqualTo("obj.Address.City");
    }

    /// <summary>
    /// Verifies BuildPropertyAccessChain with an empty path returns just the root.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertyAccessChain_EmptyPath_ReturnsRoot()
    {
        var path = new EquatableArray<PropertyPathSegment>(Array.Empty<PropertyPathSegment>());

        var result = CodeGeneratorHelpers.BuildPropertyAccessChain("obj", path);

        await Assert.That(result).IsEqualTo("obj");
    }

    /// <summary>
    /// Verifies BuildPropertyPathString produces dotted property names for a single segment.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertyPathString_SingleSegment_ReturnsPropertyName()
    {
        var path = new EquatableArray<PropertyPathSegment>(
            new[] { ModelFactory.CreatePropertyPathSegment("Name") });

        var result = CodeGeneratorHelpers.BuildPropertyPathString(path);

        await Assert.That(result).IsEqualTo("Name");
    }

    /// <summary>
    /// Verifies BuildPropertyPathString produces dotted names for multiple segments.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertyPathString_MultiSegment_ReturnsDottedPath()
    {
        var path = new EquatableArray<PropertyPathSegment>(new[]
        {
            ModelFactory.CreatePropertyPathSegment("Address"),
            ModelFactory.CreatePropertyPathSegment("City"),
        });

        var result = CodeGeneratorHelpers.BuildPropertyPathString(path);

        await Assert.That(result).IsEqualTo("Address.City");
    }

    /// <summary>
    /// Verifies BuildPropertyPathString returns empty string for empty path.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertyPathString_EmptyPath_ReturnsEmptyString()
    {
        var path = new EquatableArray<PropertyPathSegment>(Array.Empty<PropertyPathSegment>());

        var result = CodeGeneratorHelpers.BuildPropertyPathString(path);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>
    /// Verifies ComputePathSuffix returns the last two path segments.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputePathSuffix_NormalPath_ReturnsLastTwoSegments()
    {
        var result = CodeGeneratorHelpers.ComputePathSuffix("/src/ViewModels/MyViewModel.cs");

        await Assert.That(result).IsEqualTo("ViewModels/MyViewModel.cs");
    }

    /// <summary>
    /// Verifies ComputePathSuffix returns just the filename for a single-segment path.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputePathSuffix_SingleSegment_ReturnsFilename()
    {
        var result = CodeGeneratorHelpers.ComputePathSuffix("MyViewModel.cs");

        await Assert.That(result).IsEqualTo("MyViewModel.cs");
    }

    /// <summary>
    /// Verifies ComputePathSuffix returns empty for empty input.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputePathSuffix_EmptyString_ReturnsEmpty()
    {
        var result = CodeGeneratorHelpers.ComputePathSuffix(string.Empty);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>
    /// Verifies ComputePathSuffix normalizes backslashes to forward slashes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputePathSuffix_Backslashes_NormalizesToForwardSlashes()
    {
        var result = CodeGeneratorHelpers.ComputePathSuffix(@"C:\src\ViewModels\MyViewModel.cs");

        await Assert.That(result).IsEqualTo("ViewModels/MyViewModel.cs");
    }

    /// <summary>
    /// Verifies EscapeString escapes double quotes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EscapeString_WithQuotes_EscapesQuotes()
    {
        var result = CodeGeneratorHelpers.EscapeString("say \"hello\"");

        await Assert.That(result).IsEqualTo("say \\\"hello\\\"");
    }

    /// <summary>
    /// Verifies EscapeString escapes backslashes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EscapeString_WithBackslashes_EscapesBackslashes()
    {
        var result = CodeGeneratorHelpers.EscapeString(@"path\to\file");

        await Assert.That(result).IsEqualTo("path\\\\to\\\\file");
    }

    /// <summary>
    /// Verifies EscapeString leaves clean strings unchanged.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EscapeString_CleanString_ReturnsUnchanged()
    {
        var result = CodeGeneratorHelpers.EscapeString("x => x.Name");

        await Assert.That(result).IsEqualTo("x => x.Name");
    }

    /// <summary>
    /// Verifies FindClassInfo returns matching class info when found.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindClassInfo_Found_ReturnsMatchingInfo()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(fullyQualifiedName: "global::TestApp.MyViewModel");
        var allClasses = ImmutableArray.Create(classInfo);

        var result = CodeGeneratorHelpers.FindClassInfo(allClasses, "global::TestApp.MyViewModel");

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.FullyQualifiedName).IsEqualTo("global::TestApp.MyViewModel");
    }

    /// <summary>
    /// Verifies FindClassInfo returns null when class info is not found.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindClassInfo_NotFound_ReturnsNull()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(fullyQualifiedName: "global::TestApp.OtherType");
        var allClasses = ImmutableArray.Create(classInfo);

        var result = CodeGeneratorHelpers.FindClassInfo(allClasses, "global::TestApp.MyViewModel");

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies FindClassInfo returns null for empty array.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindClassInfo_EmptyArray_ReturnsNull()
    {
        var allClasses = ImmutableArray<ClassBindingInfo>.Empty;

        var result = CodeGeneratorHelpers.FindClassInfo(allClasses, "global::TestApp.MyViewModel");

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies AppendExtensionClassHeader produces expected structure.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendExtensionClassHeader_ProducesExpectedStructure()
    {
        var sb = new StringBuilder();

        CodeGeneratorHelpers.AppendExtensionClassHeader(sb);

        var result = sb.ToString();
        await Assert.That(result).Contains("// <auto-generated/>");
        await Assert.That(result).Contains("#pragma warning disable");
        await Assert.That(result).Contains("namespace ReactiveUI.Binding");
        await Assert.That(result).Contains("__ReactiveUIGeneratedBindings");
    }

    /// <summary>
    /// Verifies AppendExtensionClassFooter produces closing braces.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendExtensionClassFooter_ProducesClosingBraces()
    {
        var sb = new StringBuilder();

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);

        var result = sb.ToString();
        await Assert.That(result).Contains("}");
    }

    /// <summary>
    /// Verifies BuildPropertyAccessLambda delegates to BuildPropertyAccessChain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertyAccessLambda_ReturnsParameterDotPath()
    {
        var path = new EquatableArray<PropertyPathSegment>(new[]
        {
            ModelFactory.CreatePropertyPathSegment("Address"),
            ModelFactory.CreatePropertyPathSegment("City"),
        });

        var result = CodeGeneratorHelpers.BuildPropertyAccessLambda("x", path);

        await Assert.That(result).IsEqualTo("x.Address.City");
    }

    /// <summary>
    /// Verifies BuildPropertySetterChain delegates to BuildPropertyAccessChain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildPropertySetterChain_ReturnsRootDotPath()
    {
        var path = new EquatableArray<PropertyPathSegment>(
            new[] { ModelFactory.CreatePropertyPathSegment("Text") });

        var result = CodeGeneratorHelpers.BuildPropertySetterChain("target", path);

        await Assert.That(result).IsEqualTo("target.Text");
    }

    /// <summary>
    /// Verifies StableStringHash is deterministic across multiple calls.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StableStringHash_SameInput_ReturnsSameHash()
    {
        int hash1 = CodeGeneratorHelpers.StableStringHash("global::TestApp.MyViewModel");
        int hash2 = CodeGeneratorHelpers.StableStringHash("global::TestApp.MyViewModel");

        await Assert.That(hash1).IsEqualTo(hash2);
    }

    /// <summary>
    /// Verifies StableStringHash returns different hashes for different strings.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StableStringHash_DifferentInput_ReturnsDifferentHash()
    {
        int hash1 = CodeGeneratorHelpers.StableStringHash("TypeA");
        int hash2 = CodeGeneratorHelpers.StableStringHash("TypeB");

        await Assert.That(hash1).IsNotEqualTo(hash2);
    }

    /// <summary>
    /// Verifies StableStringHash returns 0 for null input.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StableStringHash_NullInput_ReturnsZero()
    {
        int result = CodeGeneratorHelpers.StableStringHash(null!);

        await Assert.That(result).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies ComputeStableMethodSuffix produces a 16-character hex string.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeStableMethodSuffix_ReturnsHexString()
    {
        var result = CodeGeneratorHelpers.ComputeStableMethodSuffix(
            "global::TestApp.MyViewModel", "/src/Test.cs", 42, "x => x.Name");

        await Assert.That(result.Length).IsEqualTo(16);
        await Assert.That(result.All(c => "0123456789ABCDEF".Contains(c))).IsTrue();
    }

    /// <summary>
    /// Verifies ComputeStableMethodSuffix is deterministic.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeStableMethodSuffix_SameInput_ReturnsSameResult()
    {
        var result1 = CodeGeneratorHelpers.ComputeStableMethodSuffix(
            "global::TestApp.MyViewModel", "/src/Test.cs", 42, "x => x.Name");
        var result2 = CodeGeneratorHelpers.ComputeStableMethodSuffix(
            "global::TestApp.MyViewModel", "/src/Test.cs", 42, "x => x.Name");

        await Assert.That(result1).IsEqualTo(result2);
    }

    /// <summary>
    /// Verifies ComputeStableMethodSuffix produces different results for different discriminators.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeStableMethodSuffix_DifferentDiscriminator_ReturnsDifferentResult()
    {
        var result1 = CodeGeneratorHelpers.ComputeStableMethodSuffix(
            "global::TestApp.MyViewModel", "/src/Test.cs", 42, "x => x.Name");
        var result2 = CodeGeneratorHelpers.ComputeStableMethodSuffix(
            "global::TestApp.MyViewModel", "/src/Test.cs", 42, "x => x.Age");

        await Assert.That(result1).IsNotEqualTo(result2);
    }

    /// <summary>
    /// Verifies ComputeStableMethodSuffix produces different results for different line numbers.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeStableMethodSuffix_DifferentLineNumber_ReturnsDifferentResult()
    {
        var result1 = CodeGeneratorHelpers.ComputeStableMethodSuffix(
            "global::TestApp.MyViewModel", "/src/Test.cs", 10);
        var result2 = CodeGeneratorHelpers.ComputeStableMethodSuffix(
            "global::TestApp.MyViewModel", "/src/Test.cs", 20);

        await Assert.That(result1).IsNotEqualTo(result2);
    }

    /// <summary>
    /// Verifies ComputePathSuffix returns the full path when only one slash is present
    /// (secondLastSlash is less than zero). Covers line 119.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputePathSuffix_OneSlash_ReturnsFullPath()
    {
        var result = CodeGeneratorHelpers.ComputePathSuffix("folder/MyFile.cs");

        await Assert.That(result).IsEqualTo("folder/MyFile.cs");
    }

    /// <summary>
    /// Verifies NormalizeLambdaText strips the "static " prefix from lambda text.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NormalizeLambdaText_StaticPrefix_StripsPrefix()
    {
        var result = CodeGeneratorHelpers.NormalizeLambdaText("static x => x.Name");

        await Assert.That(result).IsEqualTo("x => x.Name");
    }

    /// <summary>
    /// Verifies NormalizeLambdaText returns the input unchanged when no "static " prefix is present.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NormalizeLambdaText_NoPrefix_ReturnsUnchanged()
    {
        var result = CodeGeneratorHelpers.NormalizeLambdaText("x => x.Name");

        await Assert.That(result).IsEqualTo("x => x.Name");
    }

    /// <summary>
    /// Verifies NormalizeLambdaText does not strip "static" when it is not followed by a space
    /// or when the string is too short to contain the prefix.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NormalizeLambdaText_ShortString_ReturnsUnchanged()
    {
        var result = CodeGeneratorHelpers.NormalizeLambdaText("stat");

        await Assert.That(result).IsEqualTo("stat");
    }

    /// <summary>
    /// Verifies EscapeString handles strings with both quotes and backslashes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EscapeString_MixedSpecialChars_EscapesAll()
    {
        var result = CodeGeneratorHelpers.EscapeString("a\\\"b");

        await Assert.That(result).IsEqualTo("a\\\\\\\"b");
    }
}

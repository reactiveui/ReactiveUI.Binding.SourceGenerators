// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Fallback;

/// <summary>Covers the conversion a generated binding routes a value through before it writes.</summary>
/// <remarks>
/// A refused conversion is the case that matters: the generated binding skips the assignment for that
/// emission rather than writing a default, so the failure has to be reported rather than absorbed.
/// </remarks>
public class RuntimeBindingConverterTests
{
    /// <summary>The value handed to the conversion.</summary>
    private const string SourceValue = "41";

    /// <summary>The value a successful conversion produces.</summary>
    private const int ConvertedValue = 41;

    /// <summary>An explicit converter takes precedence over the registry.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvert_WithAConverterOverride_ProducesTheConvertedValue()
    {
        var converter = new StubBindingTypeConverter(
            typeof(string),
            typeof(int),
            static (_, _) => new(true, ConvertedValue));

        var converted = RuntimeBindingConverter.TryConvert<string, int>(
            SourceValue,
            null,
            converter,
            out var result);

        await Assert.That(converted).IsTrue();
        await Assert.That(result).IsEqualTo(ConvertedValue);
    }

    /// <summary>An explicit converter that refuses the value fails the conversion.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvert_WhenTheConverterOverrideRefuses_Fails()
    {
        var converter = new StubBindingTypeConverter(
            typeof(string),
            typeof(int),
            static (_, _) => new(false, null));

        var converted = RuntimeBindingConverter.TryConvert<string, int>(
            SourceValue,
            null,
            converter,
            out var result);

        await Assert.That(converted).IsFalse();
        await Assert.That(result).IsEqualTo(0);
    }

    /// <summary>An explicit converter producing a value of another type fails the conversion.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvert_WhenTheConverterOverrideProducesAnotherType_Fails()
    {
        var converter = new StubBindingTypeConverter(
            typeof(string),
            typeof(int),
            static (_, _) => new(true, SourceValue));

        var converted = RuntimeBindingConverter.TryConvert<string, int>(
            SourceValue,
            null,
            converter,
            out var result);

        await Assert.That(converted).IsFalse();
        await Assert.That(result).IsEqualTo(0);
    }

    /// <summary>The registry resolves the conversion when no explicit converter is supplied.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvert_WithoutAConverterOverride_ResolvesFromTheRegistry()
    {
        var converted = RuntimeBindingConverter.TryConvert<string, int>(
            SourceValue,
            null,
            null,
            out var result);

        await Assert.That(converted).IsTrue();
        await Assert.That(result).IsEqualTo(ConvertedValue);
    }

    /// <summary>A type pair the registry cannot bridge fails the conversion.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvert_WhenNoConverterReachesTheTypePair_Fails()
    {
        var converted = RuntimeBindingConverter.TryConvert<TestViewModel, int>(
            new(),
            null,
            null,
            out var result);

        await Assert.That(converted).IsFalse();
        await Assert.That(result).IsEqualTo(0);
    }
}

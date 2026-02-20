// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>
/// Unit tests for <see cref="BindingTypeConverterDispatch"/>.
/// </summary>
public class BindingTypeConverterDispatchTests
{
    /// <summary>
    /// Verifies that TryConvert returns true for an exact type match.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvert_ExactTypeMatch_ReturnsTrue()
    {
        var converter = new StubBindingTypeConverter(
            typeof(string),
            typeof(int),
            (from, hint) => (true, int.Parse((string)from!)));

        var success = BindingTypeConverterDispatch.TryConvert(converter, "42", typeof(int), null, out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(42);
    }

    /// <summary>
    /// Verifies that TryConvert returns false when the source type does not match.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvert_SourceTypeMismatch_ReturnsFalse()
    {
        var converter = new StubBindingTypeConverter(
            typeof(string),
            typeof(int),
            (from, hint) => (true, 42));

        var success = BindingTypeConverterDispatch.TryConvert(converter, true, typeof(int), null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that TryConvert returns false when the target type does not match.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvert_TargetTypeMismatch_ReturnsFalse()
    {
        var converter = new StubBindingTypeConverter(
            typeof(string),
            typeof(int),
            (from, hint) => (true, 42));

        var success = BindingTypeConverterDispatch.TryConvert(converter, "42", typeof(bool), null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that TryConvert handles nullable target types correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvert_NullableTargetType_ReturnsTrue()
    {
        var converter = new StubBindingTypeConverter(
            typeof(double),
            typeof(double?),
            (from, hint) => (true, (double?)from));

        var success = BindingTypeConverterDispatch.TryConvert(converter, 3.14, typeof(double?), null, out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(3.14);
    }

    /// <summary>
    /// Verifies that TryConvert allows boxed non-nullable to nullable converter.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvert_BoxedNonNullableToNullableConverter_ReturnsTrue()
    {
        var converter = new StubBindingTypeConverter(
            typeof(int?),
            typeof(string),
            (from, hint) => (true, from?.ToString() ?? "null"));

        var success = BindingTypeConverterDispatch.TryConvert(converter, 42, typeof(string), null, out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo("42");
    }

    /// <summary>
    /// Verifies that TryConvert returns false for null input with non-nullable FromType.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvert_NullInputWithNonNullableFromType_ReturnsFalse()
    {
        var converter = new StubBindingTypeConverter(
            typeof(int),
            typeof(string),
            (from, hint) => (true, "42"));

        var success = BindingTypeConverterDispatch.TryConvert(converter, null, typeof(string), null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that TryConvert passes the conversion hint correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvert_PassesConversionHint()
    {
        var hintValue = "some-hint";
        var converter = new StubBindingTypeConverter(
            typeof(string),
            typeof(string),
            (from, hint) => (true, hint));

        var success = BindingTypeConverterDispatch.TryConvert(converter, "input", typeof(string), hintValue, out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(hintValue);
    }

    /// <summary>
    /// Verifies that TryConvertFallback succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvertFallback_Success_ReturnsTrue()
    {
        var converter = new StubFallbackConverter(
            (fromType, from, toType, hint) => (true, "fallback-result"));

        var success = BindingTypeConverterDispatch.TryConvertFallback(converter, typeof(int), 42, typeof(string), null, out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo("fallback-result");
    }

    /// <summary>
    /// Verifies that TryConvertFallback returns false when the result is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvertFallback_NullResult_ReturnsFalse()
    {
        var converter = new StubFallbackConverter(
            (fromType, from, toType, hint) => (true, null));

        var success = BindingTypeConverterDispatch.TryConvertFallback(converter, typeof(int), 42, typeof(string), null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that TryConvertFallback returns false when the converter fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvertFallback_ConverterFails_ReturnsFalse()
    {
        var converter = new StubFallbackConverter(
            (fromType, from, toType, hint) => (false, null));

        var success = BindingTypeConverterDispatch.TryConvertFallback(converter, typeof(int), 42, typeof(string), null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that TryConvertAny routes to TryConvert for IBindingTypeConverter.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvertAny_RoutesToTypedConverter()
    {
        var converter = new StubBindingTypeConverter(
            typeof(string),
            typeof(int),
            (from, hint) => (true, 42));

        var success = BindingTypeConverterDispatch.TryConvertAny(converter, typeof(string), "input", typeof(int), null, out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(42);
    }

    /// <summary>
    /// Verifies that TryConvertAny routes to TryConvertFallback for IBindingFallbackConverter.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvertAny_RoutesToFallbackConverter()
    {
        var converter = new StubFallbackConverter(
            (fromType, from, toType, hint) => (true, "fallback"));

        var success = BindingTypeConverterDispatch.TryConvertAny(converter, typeof(int), 42, typeof(string), null, out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo("fallback");
    }

    /// <summary>
    /// Verifies that TryConvertAny returns false for null input with fallback converter.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvertAny_NullInputWithFallback_ReturnsFalse()
    {
        var converter = new StubFallbackConverter(
            (fromType, from, toType, hint) => (true, "fallback"));

        var success = BindingTypeConverterDispatch.TryConvertAny(converter, typeof(int), null, typeof(string), null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that TryConvertAny returns false for unknown converter type.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvertAny_UnknownConverterType_ReturnsFalse()
    {
        var success = BindingTypeConverterDispatch.TryConvertAny(new object(), typeof(int), 42, typeof(string), null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that TryConvertAny returns false for null converter.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvertAny_NullConverter_ReturnsFalse()
    {
        var success = BindingTypeConverterDispatch.TryConvertAny(null, typeof(int), 42, typeof(string), null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that TryConvert allows null input when the converter's FromType is a nullable value type.
    /// Covers BindingTypeConverterDispatch.cs line 45 (Nullable.GetUnderlyingType is NOT null branch).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvert_NullInputWithNullableFromType_ProceedsToConvert()
    {
        // Converter from int? to string; FromType is typeof(int?) which is a nullable value type
        var converter = new StubBindingTypeConverter(
            typeof(int?),
            typeof(string),
            (from, hint) => (true, from?.ToString() ?? "null"));

        // Pass null as `from` value. Since FromType (int?) is a nullable value type,
        // Nullable.GetUnderlyingType returns typeof(int), so the null check allows it through.
        var success = BindingTypeConverterDispatch.TryConvert(converter, null, typeof(string), null, out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo("null");
    }

    /// <summary>
    /// Verifies that TryConvert with a real NullableIntegerToStringTypeConverter handles null input.
    /// The converter's FromType is int? (nullable), so null should be allowed through the dispatch.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TryConvert_NullableIntegerToString_NullInput_Succeeds()
    {
        var converter = new NullableIntegerToStringTypeConverter();

        var success = BindingTypeConverterDispatch.TryConvert(converter, null, typeof(string), null, out var result);

        await Assert.That(success).IsTrue();

        // NullableIntegerToStringTypeConverter.TryConvert(null, ...) sets result = null, returns true
        // Then TryConvertTyped sets result = typedResult which is null
        // Then the dispatch checks if the result is null for non-nullable TTo - string is ref type, so it's OK
        await Assert.That(result).IsNull();
    }
}

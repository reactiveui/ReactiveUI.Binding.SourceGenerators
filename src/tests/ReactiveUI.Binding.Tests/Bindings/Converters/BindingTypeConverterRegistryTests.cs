// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding;

namespace ReactiveUI.Binding.Tests.Bindings.Converters;

/// <summary>
/// Tests for the internal <see cref="BindingTypeConverterRegistry.CloneRegistryShallow"/> method.
/// </summary>
public class BindingTypeConverterRegistryTests
{
    /// <summary>
    /// Verifies that CloneRegistryShallow returns an empty dictionary for empty input.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CloneRegistryShallow_EmptyDictionary_ReturnsEmpty()
    {
        var source = new Dictionary<(Type fromType, Type toType), List<IBindingTypeConverter>>();

        var clone = BindingTypeConverterRegistry.CloneRegistryShallow(source);

        await Assert.That(clone.Count).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that CloneRegistryShallow returns a shallow copy with the same entries.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CloneRegistryShallow_WithEntries_ReturnsShallowCopy()
    {
        var converter = new TestConverter(typeof(string), typeof(int));
        var list = new List<IBindingTypeConverter> { converter };
        var source = new Dictionary<(Type fromType, Type toType), List<IBindingTypeConverter>>
        {
            [(typeof(string), typeof(int))] = list,
        };

        var clone = BindingTypeConverterRegistry.CloneRegistryShallow(source);

        await Assert.That(clone.Count).IsEqualTo(1);
        await Assert.That(clone.ContainsKey((typeof(string), typeof(int)))).IsTrue();

        var clonedList = clone[(typeof(string), typeof(int))];
        await Assert.That(clonedList.Count).IsEqualTo(1);
        await Assert.That(ReferenceEquals(clonedList, list)).IsTrue();
    }

    /// <summary>
    /// Verifies that modifying the clone does not affect the original dictionary.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CloneRegistryShallow_ModifyingClone_DoesNotAffectOriginal()
    {
        var converter = new TestConverter(typeof(string), typeof(int));
        var list = new List<IBindingTypeConverter> { converter };
        var source = new Dictionary<(Type fromType, Type toType), List<IBindingTypeConverter>>
        {
            [(typeof(string), typeof(int))] = list,
        };

        var clone = BindingTypeConverterRegistry.CloneRegistryShallow(source);

        // Remove entry from clone
        clone.Remove((typeof(string), typeof(int)));

        // Original should still have the entry
        await Assert.That(source.Count).IsEqualTo(1);
        await Assert.That(clone.Count).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that CloneRegistryShallow throws ArgumentNullException for null input.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CloneRegistryShallow_NullInput_ThrowsArgumentNullException()
    {
        await Assert.That(() => BindingTypeConverterRegistry.CloneRegistryShallow(null!))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Minimal test implementation of <see cref="IBindingTypeConverter"/>.
    /// </summary>
    private sealed class TestConverter : IBindingTypeConverter
    {
        public TestConverter(Type fromType, Type toType)
        {
            FromType = fromType;
            ToType = toType;
        }

        public Type FromType { get; }

        public Type ToType { get; }

        public int GetAffinityForObjects() => 2;

        public bool TryConvertTyped(object? from, object? conversionHint, out object? result)
        {
            result = null;
            return false;
        }
    }
}

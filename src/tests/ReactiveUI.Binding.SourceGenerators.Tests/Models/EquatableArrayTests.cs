// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Models;

/// <summary>
/// Tests for <see cref="EquatableArray{T}"/> methods.
/// </summary>
public class EquatableArrayTests
{
    /// <summary>
    /// Verifies ComputeHashCode returns 0 for null array.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeHashCode_NullArray_ReturnsZero()
    {
        int result = EquatableArray<PropertyPathSegment>.ComputeHashCode(null);

        await Assert.That(result).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies ComputeHashCode returns consistent hash for empty array.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeHashCode_EmptyArray_ReturnsConsistentHash()
    {
        int hash1 = EquatableArray<PropertyPathSegment>.ComputeHashCode(Array.Empty<PropertyPathSegment>());
        int hash2 = EquatableArray<PropertyPathSegment>.ComputeHashCode(Array.Empty<PropertyPathSegment>());

        await Assert.That(hash1).IsEqualTo(hash2);
        await Assert.That(hash1).IsEqualTo(17); // Hash seed with no elements
    }

    /// <summary>
    /// Verifies ComputeHashCode returns same hash for same content in different arrays.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeHashCode_SameContent_ReturnsSameHash()
    {
        var seg = new PropertyPathSegment("Name", "global::System.String", "global::TestApp.VM");
        var arr1 = new[] { seg };
        var arr2 = new[] { new PropertyPathSegment("Name", "global::System.String", "global::TestApp.VM") };

        int hash1 = EquatableArray<PropertyPathSegment>.ComputeHashCode(arr1);
        int hash2 = EquatableArray<PropertyPathSegment>.ComputeHashCode(arr2);

        await Assert.That(hash1).IsEqualTo(hash2);
    }

    /// <summary>
    /// Verifies ComputeHashCode returns different hash for different content.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeHashCode_DifferentContent_ReturnsDifferentHash()
    {
        var arr1 = new[] { new PropertyPathSegment("Name", "global::System.String", "global::TestApp.VM") };
        var arr2 = new[] { new PropertyPathSegment("Age", "global::System.Int32", "global::TestApp.VM") };

        int hash1 = EquatableArray<PropertyPathSegment>.ComputeHashCode(arr1);
        int hash2 = EquatableArray<PropertyPathSegment>.ComputeHashCode(arr2);

        await Assert.That(hash1).IsNotEqualTo(hash2);
    }

    /// <summary>
    /// Verifies EquatableArray constructor caches hash code matching ComputeHashCode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_CachesHashCode_MatchesComputeHashCode()
    {
        var seg = new PropertyPathSegment("Name", "global::System.String", "global::TestApp.VM");
        var arr = new[] { seg };

        var equatable = new EquatableArray<PropertyPathSegment>(arr);
        int expected = EquatableArray<PropertyPathSegment>.ComputeHashCode(arr);

        await Assert.That(equatable.GetHashCode()).IsEqualTo(expected);
    }

    /// <summary>
    /// Verifies default-constructed EquatableArray has hash code 0.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultConstructed_HashCode_ReturnsZero()
    {
        var defaultArray = default(EquatableArray<PropertyPathSegment>);

        await Assert.That(defaultArray.GetHashCode()).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies equal arrays are equal via Equals method.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Equals_SameContent_ReturnsTrue()
    {
        var arr1 = new EquatableArray<PropertyPathSegment>(
            new[] { new PropertyPathSegment("Name", "global::System.String", "global::TestApp.VM") });
        var arr2 = new EquatableArray<PropertyPathSegment>(
            new[] { new PropertyPathSegment("Name", "global::System.String", "global::TestApp.VM") });

        await Assert.That(arr1.Equals(arr2)).IsTrue();
    }

    /// <summary>
    /// Verifies different arrays are not equal via Equals method.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Equals_DifferentContent_ReturnsFalse()
    {
        var arr1 = new EquatableArray<PropertyPathSegment>(
            new[] { new PropertyPathSegment("Name", "global::System.String", "global::TestApp.VM") });
        var arr2 = new EquatableArray<PropertyPathSegment>(
            new[] { new PropertyPathSegment("Age", "global::System.Int32", "global::TestApp.VM") });

        await Assert.That(arr1.Equals(arr2)).IsFalse();
    }

    /// <summary>
    /// Verifies default arrays are equal to each other.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Equals_BothDefault_ReturnsTrue()
    {
        var arr1 = default(EquatableArray<PropertyPathSegment>);
        var arr2 = default(EquatableArray<PropertyPathSegment>);

        await Assert.That(arr1.Equals(arr2)).IsTrue();
    }

    /// <summary>
    /// Verifies Length returns the number of elements.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Length_MultipleElements_ReturnsCount()
    {
        var arr = new EquatableArray<PropertyPathSegment>(new[]
        {
            new PropertyPathSegment("A", "global::System.String", "global::T"),
            new PropertyPathSegment("B", "global::System.Int32", "global::T"),
        });

        await Assert.That(arr.Length).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies Length returns 0 for default-constructed array.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Length_DefaultConstructed_ReturnsZero()
    {
        var arr = default(EquatableArray<PropertyPathSegment>);

        await Assert.That(arr.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies operator== returns true for equal arrays.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OperatorEquals_SameContent_ReturnsTrue()
    {
        var arr1 = new EquatableArray<PropertyPathSegment>(
            new[] { new PropertyPathSegment("Name", "global::System.String", "global::T") });
        var arr2 = new EquatableArray<PropertyPathSegment>(
            new[] { new PropertyPathSegment("Name", "global::System.String", "global::T") });

        await Assert.That(arr1 == arr2).IsTrue();
    }

    /// <summary>
    /// Verifies operator!= returns true for different arrays.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OperatorNotEquals_DifferentContent_ReturnsTrue()
    {
        var arr1 = new EquatableArray<PropertyPathSegment>(
            new[] { new PropertyPathSegment("Name", "global::System.String", "global::T") });
        var arr2 = new EquatableArray<PropertyPathSegment>(
            new[] { new PropertyPathSegment("Age", "global::System.Int32", "global::T") });

        await Assert.That(arr1 != arr2).IsTrue();
    }
}

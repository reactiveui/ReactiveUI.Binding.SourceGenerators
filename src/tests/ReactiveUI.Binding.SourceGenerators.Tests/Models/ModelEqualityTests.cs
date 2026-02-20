// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Models;

/// <summary>
/// Tests for auto-generated record equality members on internal model types:
/// <see cref="ObservableTypeInfo"/>, <see cref="ObservablePropertyInfo"/>,
/// <see cref="InvocationInfo"/>, <see cref="BindingInvocationInfo"/>,
/// <see cref="PropertyPathSegment"/>, and <see cref="ObservationCodeGenerator.TypeGroup"/>.
/// </summary>
public class ModelEqualityTests
{
    // ───────────────────────────────────────────────────────────────────────────
    // ObservableTypeInfo
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that two ObservableTypeInfo instances with identical values are equal.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableTypeInfo_Equals_SameValues_ReturnsTrue()
    {
        var properties = new EquatableArray<ObservablePropertyInfo>(new[]
        {
            ModelFactory.CreateObservablePropertyInfo("Name"),
        });

        var a = new ObservableTypeInfo("global::TestApp.MyViewModel", "MyViewModel", "INPC", 21, true, properties);
        var b = new ObservableTypeInfo("global::TestApp.MyViewModel", "MyViewModel", "INPC", 21, true, properties);

        await Assert.That(a.Equals(b)).IsTrue();
    }

    /// <summary>
    /// Verifies that two ObservableTypeInfo instances with different values are not equal.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableTypeInfo_Equals_DifferentValues_ReturnsFalse()
    {
        var properties = new EquatableArray<ObservablePropertyInfo>(new[]
        {
            ModelFactory.CreateObservablePropertyInfo("Name"),
        });

        var a = new ObservableTypeInfo("global::TestApp.MyViewModel", "MyViewModel", "INPC", 21, true, properties);
        var b = new ObservableTypeInfo("global::TestApp.OtherType", "OtherType", "INPC", 21, true, properties);

        await Assert.That(a.Equals(b)).IsFalse();
    }

    /// <summary>
    /// Verifies that ObservableTypeInfo.Equals returns false when compared to null object.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableTypeInfo_Equals_ObjectNull_ReturnsFalse()
    {
        var properties = new EquatableArray<ObservablePropertyInfo>(Array.Empty<ObservablePropertyInfo>());
        var a = new ObservableTypeInfo("global::TestApp.MyViewModel", "MyViewModel", "INPC", 21, true, properties);

        await Assert.That(a.Equals((object?)null)).IsFalse();
    }

    /// <summary>
    /// Verifies that ObservableTypeInfo.Equals returns false when compared to a different type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableTypeInfo_Equals_ObjectWrongType_ReturnsFalse()
    {
        var properties = new EquatableArray<ObservablePropertyInfo>(Array.Empty<ObservablePropertyInfo>());
        var a = new ObservableTypeInfo("global::TestApp.MyViewModel", "MyViewModel", "INPC", 21, true, properties);

        await Assert.That(a.Equals("string")).IsFalse();
    }

    /// <summary>
    /// Verifies that two ObservableTypeInfo instances with same values produce the same hash code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableTypeInfo_GetHashCode_SameValues_AreEqual()
    {
        var properties = new EquatableArray<ObservablePropertyInfo>(new[]
        {
            ModelFactory.CreateObservablePropertyInfo("Name"),
        });

        var a = new ObservableTypeInfo("global::TestApp.MyViewModel", "MyViewModel", "INPC", 21, true, properties);
        var b = new ObservableTypeInfo("global::TestApp.MyViewModel", "MyViewModel", "INPC", 21, true, properties);

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    /// <summary>
    /// Verifies that operator== returns true for ObservableTypeInfo instances with same values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableTypeInfo_OperatorEquals_SameValues_ReturnsTrue()
    {
        var properties = new EquatableArray<ObservablePropertyInfo>(new[]
        {
            ModelFactory.CreateObservablePropertyInfo("Name"),
        });

        var a = new ObservableTypeInfo("global::TestApp.MyViewModel", "MyViewModel", "INPC", 21, true, properties);
        var b = new ObservableTypeInfo("global::TestApp.MyViewModel", "MyViewModel", "INPC", 21, true, properties);

        await Assert.That(a == b).IsTrue();
    }

    /// <summary>
    /// Verifies that operator!= returns true for ObservableTypeInfo instances with different values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableTypeInfo_OperatorNotEquals_DifferentValues_ReturnsTrue()
    {
        var properties = new EquatableArray<ObservablePropertyInfo>(Array.Empty<ObservablePropertyInfo>());

        var a = new ObservableTypeInfo("global::TestApp.MyViewModel", "MyViewModel", "INPC", 21, true, properties);
        var b = new ObservableTypeInfo("global::TestApp.MyViewModel", "MyViewModel", "ReactiveObject", 24, true, properties);

        await Assert.That(a != b).IsTrue();
    }

    /// <summary>
    /// Verifies that ObservableTypeInfo.ToString contains the type name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableTypeInfo_ToString_ContainsTypeName()
    {
        var properties = new EquatableArray<ObservablePropertyInfo>(Array.Empty<ObservablePropertyInfo>());
        var a = new ObservableTypeInfo("global::TestApp.MyViewModel", "MyViewModel", "INPC", 21, true, properties);

        await Assert.That(a.ToString()).Contains("ObservableTypeInfo");
    }

    // ───────────────────────────────────────────────────────────────────────────
    // ObservablePropertyInfo
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that two ObservablePropertyInfo instances with identical values are equal.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_Equals_SameValues_ReturnsTrue()
    {
        var a = new ObservablePropertyInfo("Name", "global::System.String", true, false, false);
        var b = new ObservablePropertyInfo("Name", "global::System.String", true, false, false);

        await Assert.That(a.Equals(b)).IsTrue();
    }

    /// <summary>
    /// Verifies that two ObservablePropertyInfo instances with different values are not equal.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_Equals_DifferentValues_ReturnsFalse()
    {
        var a = new ObservablePropertyInfo("Name", "global::System.String", true, false, false);
        var b = new ObservablePropertyInfo("Age", "global::System.Int32", true, false, false);

        await Assert.That(a.Equals(b)).IsFalse();
    }

    /// <summary>
    /// Verifies that ObservablePropertyInfo.Equals returns false when compared to null object.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_Equals_ObjectNull_ReturnsFalse()
    {
        var a = new ObservablePropertyInfo("Name", "global::System.String", true, false, false);

        await Assert.That(a.Equals((object?)null)).IsFalse();
    }

    /// <summary>
    /// Verifies that ObservablePropertyInfo.Equals returns false when compared to a different type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_Equals_ObjectWrongType_ReturnsFalse()
    {
        var a = new ObservablePropertyInfo("Name", "global::System.String", true, false, false);

        await Assert.That(a.Equals("string")).IsFalse();
    }

    /// <summary>
    /// Verifies that two ObservablePropertyInfo instances with same values produce the same hash code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_GetHashCode_SameValues_AreEqual()
    {
        var a = new ObservablePropertyInfo("Name", "global::System.String", true, false, false);
        var b = new ObservablePropertyInfo("Name", "global::System.String", true, false, false);

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    /// <summary>
    /// Verifies that operator== returns true for ObservablePropertyInfo instances with same values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_OperatorEquals_SameValues_ReturnsTrue()
    {
        var a = new ObservablePropertyInfo("Name", "global::System.String", true, false, false);
        var b = new ObservablePropertyInfo("Name", "global::System.String", true, false, false);

        await Assert.That(a == b).IsTrue();
    }

    /// <summary>
    /// Verifies that operator!= returns true for ObservablePropertyInfo instances with different values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_OperatorNotEquals_DifferentValues_ReturnsTrue()
    {
        var a = new ObservablePropertyInfo("Name", "global::System.String", true, false, false);
        var b = new ObservablePropertyInfo("Name", "global::System.String", false, false, false);

        await Assert.That(a != b).IsTrue();
    }

    /// <summary>
    /// Verifies that ObservablePropertyInfo.ToString contains the type name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservablePropertyInfo_ToString_ContainsTypeName()
    {
        var a = new ObservablePropertyInfo("Name", "global::System.String", true, false, false);

        await Assert.That(a.ToString()).Contains("ObservablePropertyInfo");
    }

    // ───────────────────────────────────────────────────────────────────────────
    // InvocationInfo
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that two InvocationInfo instances with identical values are equal.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_Equals_SameValues_ReturnsTrue()
    {
        var a = ModelFactory.CreateInvocationInfo();
        var b = ModelFactory.CreateInvocationInfo();

        await Assert.That(a.Equals(b)).IsTrue();
    }

    /// <summary>
    /// Verifies that two InvocationInfo instances with different values are not equal.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_Equals_DifferentValues_ReturnsFalse()
    {
        var a = ModelFactory.CreateInvocationInfo(callerLineNumber: 42);
        var b = ModelFactory.CreateInvocationInfo(callerLineNumber: 99);

        await Assert.That(a.Equals(b)).IsFalse();
    }

    /// <summary>
    /// Verifies that InvocationInfo.Equals returns false when compared to null object.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_Equals_ObjectNull_ReturnsFalse()
    {
        var a = ModelFactory.CreateInvocationInfo();

        await Assert.That(a.Equals((object?)null)).IsFalse();
    }

    /// <summary>
    /// Verifies that InvocationInfo.Equals returns false when compared to a different type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_Equals_ObjectWrongType_ReturnsFalse()
    {
        var a = ModelFactory.CreateInvocationInfo();

        await Assert.That(a.Equals("string")).IsFalse();
    }

    /// <summary>
    /// Verifies that two InvocationInfo instances with same values produce the same hash code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_GetHashCode_SameValues_AreEqual()
    {
        var a = ModelFactory.CreateInvocationInfo();
        var b = ModelFactory.CreateInvocationInfo();

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    /// <summary>
    /// Verifies that operator== returns true for InvocationInfo instances with same values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_OperatorEquals_SameValues_ReturnsTrue()
    {
        var a = ModelFactory.CreateInvocationInfo();
        var b = ModelFactory.CreateInvocationInfo();

        await Assert.That(a == b).IsTrue();
    }

    /// <summary>
    /// Verifies that operator!= returns true for InvocationInfo instances with different values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_OperatorNotEquals_DifferentValues_ReturnsTrue()
    {
        var a = ModelFactory.CreateInvocationInfo(methodName: "WhenChanged");
        var b = ModelFactory.CreateInvocationInfo(methodName: "WhenChanging");

        await Assert.That(a != b).IsTrue();
    }

    /// <summary>
    /// Verifies that InvocationInfo.ToString contains the type name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvocationInfo_ToString_ContainsTypeName()
    {
        var a = ModelFactory.CreateInvocationInfo();

        await Assert.That(a.ToString()).Contains("InvocationInfo");
    }

    // ───────────────────────────────────────────────────────────────────────────
    // BindingInvocationInfo
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that two BindingInvocationInfo instances with identical values are equal.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_Equals_SameValues_ReturnsTrue()
    {
        var a = ModelFactory.CreateBindingInvocationInfo();
        var b = ModelFactory.CreateBindingInvocationInfo();

        await Assert.That(a.Equals(b)).IsTrue();
    }

    /// <summary>
    /// Verifies that two BindingInvocationInfo instances with different values are not equal.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_Equals_DifferentValues_ReturnsFalse()
    {
        var a = ModelFactory.CreateBindingInvocationInfo(isTwoWay: false);
        var b = ModelFactory.CreateBindingInvocationInfo(isTwoWay: true);

        await Assert.That(a.Equals(b)).IsFalse();
    }

    /// <summary>
    /// Verifies that BindingInvocationInfo.Equals returns false when compared to null object.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_Equals_ObjectNull_ReturnsFalse()
    {
        var a = ModelFactory.CreateBindingInvocationInfo();

        await Assert.That(a.Equals((object?)null)).IsFalse();
    }

    /// <summary>
    /// Verifies that BindingInvocationInfo.Equals returns false when compared to a different type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_Equals_ObjectWrongType_ReturnsFalse()
    {
        var a = ModelFactory.CreateBindingInvocationInfo();

        await Assert.That(a.Equals("string")).IsFalse();
    }

    /// <summary>
    /// Verifies that two BindingInvocationInfo instances with same values produce the same hash code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_GetHashCode_SameValues_AreEqual()
    {
        var a = ModelFactory.CreateBindingInvocationInfo();
        var b = ModelFactory.CreateBindingInvocationInfo();

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    /// <summary>
    /// Verifies that operator== returns true for BindingInvocationInfo instances with same values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_OperatorEquals_SameValues_ReturnsTrue()
    {
        var a = ModelFactory.CreateBindingInvocationInfo();
        var b = ModelFactory.CreateBindingInvocationInfo();

        await Assert.That(a == b).IsTrue();
    }

    /// <summary>
    /// Verifies that operator!= returns true for BindingInvocationInfo instances with different values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_OperatorNotEquals_DifferentValues_ReturnsTrue()
    {
        var a = ModelFactory.CreateBindingInvocationInfo(methodName: "BindOneWay");
        var b = ModelFactory.CreateBindingInvocationInfo(methodName: "BindTwoWay");

        await Assert.That(a != b).IsTrue();
    }

    /// <summary>
    /// Verifies that BindingInvocationInfo.ToString contains the type name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingInvocationInfo_ToString_ContainsTypeName()
    {
        var a = ModelFactory.CreateBindingInvocationInfo();

        await Assert.That(a.ToString()).Contains("BindingInvocationInfo");
    }

    // ───────────────────────────────────────────────────────────────────────────
    // PropertyPathSegment
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that two PropertyPathSegment instances with identical values are equal.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_Equals_SameValues_ReturnsTrue()
    {
        var a = new PropertyPathSegment("Name", "global::System.String", "global::TestApp.MyViewModel");
        var b = new PropertyPathSegment("Name", "global::System.String", "global::TestApp.MyViewModel");

        await Assert.That(a.Equals(b)).IsTrue();
    }

    /// <summary>
    /// Verifies that two PropertyPathSegment instances with different values are not equal.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_Equals_DifferentValues_ReturnsFalse()
    {
        var a = new PropertyPathSegment("Name", "global::System.String", "global::TestApp.MyViewModel");
        var b = new PropertyPathSegment("Age", "global::System.Int32", "global::TestApp.MyViewModel");

        await Assert.That(a.Equals(b)).IsFalse();
    }

    /// <summary>
    /// Verifies that PropertyPathSegment.Equals returns false when compared to null object.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_Equals_ObjectNull_ReturnsFalse()
    {
        var a = new PropertyPathSegment("Name", "global::System.String", "global::TestApp.MyViewModel");

        await Assert.That(a.Equals((object?)null)).IsFalse();
    }

    /// <summary>
    /// Verifies that PropertyPathSegment.Equals returns false when compared to a different type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_Equals_ObjectWrongType_ReturnsFalse()
    {
        var a = new PropertyPathSegment("Name", "global::System.String", "global::TestApp.MyViewModel");

        await Assert.That(a.Equals("string")).IsFalse();
    }

    /// <summary>
    /// Verifies that two PropertyPathSegment instances with same values produce the same hash code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_GetHashCode_SameValues_AreEqual()
    {
        var a = new PropertyPathSegment("Name", "global::System.String", "global::TestApp.MyViewModel");
        var b = new PropertyPathSegment("Name", "global::System.String", "global::TestApp.MyViewModel");

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    /// <summary>
    /// Verifies that operator== returns true for PropertyPathSegment instances with same values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_OperatorEquals_SameValues_ReturnsTrue()
    {
        var a = new PropertyPathSegment("Name", "global::System.String", "global::TestApp.MyViewModel");
        var b = new PropertyPathSegment("Name", "global::System.String", "global::TestApp.MyViewModel");

        await Assert.That(a == b).IsTrue();
    }

    /// <summary>
    /// Verifies that operator!= returns true for PropertyPathSegment instances with different values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_OperatorNotEquals_DifferentValues_ReturnsTrue()
    {
        var a = new PropertyPathSegment("Name", "global::System.String", "global::TestApp.MyViewModel");
        var b = new PropertyPathSegment("Name", "global::System.String", "global::TestApp.OtherType");

        await Assert.That(a != b).IsTrue();
    }

    /// <summary>
    /// Verifies that PropertyPathSegment.ToString contains the type name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyPathSegment_ToString_ContainsTypeName()
    {
        var a = new PropertyPathSegment("Name", "global::System.String", "global::TestApp.MyViewModel");

        await Assert.That(a.ToString()).Contains("PropertyPathSegment");
    }

    // ───────────────────────────────────────────────────────────────────────────
    // ObservationCodeGenerator.TypeGroup
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that two TypeGroup instances with identical values are equal.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_Equals_SameValues_ReturnsTrue()
    {
        var inv = ModelFactory.CreateInvocationInfo();
        var invocations = new[] { inv };

        var a = new ObservationCodeGenerator.TypeGroup(inv, invocations);
        var b = new ObservationCodeGenerator.TypeGroup(inv, invocations);

        await Assert.That(a.Equals(b)).IsTrue();
    }

    /// <summary>
    /// Verifies that two TypeGroup instances with different First values are not equal.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_Equals_DifferentValues_ReturnsFalse()
    {
        var invA = ModelFactory.CreateInvocationInfo(callerLineNumber: 10);
        var invB = ModelFactory.CreateInvocationInfo(callerLineNumber: 20);

        var a = new ObservationCodeGenerator.TypeGroup(invA, new[] { invA });
        var b = new ObservationCodeGenerator.TypeGroup(invB, new[] { invB });

        await Assert.That(a.Equals(b)).IsFalse();
    }

    /// <summary>
    /// Verifies that TypeGroup.Equals returns false when compared to null object.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_Equals_ObjectNull_ReturnsFalse()
    {
        var inv = ModelFactory.CreateInvocationInfo();
        var a = new ObservationCodeGenerator.TypeGroup(inv, new[] { inv });

        await Assert.That(a.Equals((object?)null)).IsFalse();
    }

    /// <summary>
    /// Verifies that TypeGroup.Equals returns false when compared to a different type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_Equals_ObjectWrongType_ReturnsFalse()
    {
        var inv = ModelFactory.CreateInvocationInfo();
        var a = new ObservationCodeGenerator.TypeGroup(inv, new[] { inv });

        await Assert.That(a.Equals("string")).IsFalse();
    }

    /// <summary>
    /// Verifies that two TypeGroup instances with same values produce the same hash code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_GetHashCode_SameValues_AreEqual()
    {
        var inv = ModelFactory.CreateInvocationInfo();
        var invocations = new[] { inv };

        var a = new ObservationCodeGenerator.TypeGroup(inv, invocations);
        var b = new ObservationCodeGenerator.TypeGroup(inv, invocations);

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    /// <summary>
    /// Verifies that operator== returns true for TypeGroup instances with same values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_OperatorEquals_SameValues_ReturnsTrue()
    {
        var inv = ModelFactory.CreateInvocationInfo();
        var invocations = new[] { inv };

        var a = new ObservationCodeGenerator.TypeGroup(inv, invocations);
        var b = new ObservationCodeGenerator.TypeGroup(inv, invocations);

        await Assert.That(a == b).IsTrue();
    }

    /// <summary>
    /// Verifies that operator!= returns true for TypeGroup instances with different values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_OperatorNotEquals_DifferentValues_ReturnsTrue()
    {
        var invA = ModelFactory.CreateInvocationInfo(callerLineNumber: 10);
        var invB = ModelFactory.CreateInvocationInfo(callerLineNumber: 20);

        var a = new ObservationCodeGenerator.TypeGroup(invA, new[] { invA });
        var b = new ObservationCodeGenerator.TypeGroup(invB, new[] { invB });

        await Assert.That(a != b).IsTrue();
    }

    /// <summary>
    /// Verifies that TypeGroup.ToString contains the type name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_ToString_ContainsTypeName()
    {
        var inv = ModelFactory.CreateInvocationInfo();
        var a = new ObservationCodeGenerator.TypeGroup(inv, new[] { inv });

        await Assert.That(a.ToString()).Contains("TypeGroup");
    }

    /// <summary>
    /// Verifies that TypeGroup.SourceTypeFullName delegates to First.SourceTypeFullName.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_SourceTypeFullName_DelegatesToFirst()
    {
        var inv = ModelFactory.CreateInvocationInfo(sourceTypeFullName: "global::TestApp.SpecialViewModel");
        var a = new ObservationCodeGenerator.TypeGroup(inv, new[] { inv });

        await Assert.That(a.SourceTypeFullName).IsEqualTo("global::TestApp.SpecialViewModel");
    }

    /// <summary>
    /// Verifies that TypeGroup.ReturnTypeFullName delegates to First.ReturnTypeFullName.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeGroup_ReturnTypeFullName_DelegatesToFirst()
    {
        var inv = ModelFactory.CreateInvocationInfo(returnTypeFullName: "global::System.Int32");
        var a = new ObservationCodeGenerator.TypeGroup(inv, new[] { inv });

        await Assert.That(a.ReturnTypeFullName).IsEqualTo("global::System.Int32");
    }
}

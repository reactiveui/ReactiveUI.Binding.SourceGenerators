// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// Polyfill implementation adapted from SimonCropp/Polyfill
// https://github.com/SimonCropp/Polyfill
#if !NET
namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies the types of members that are dynamically accessed.
/// This enumeration has a <see cref="FlagsAttribute"/> attribute that allows a
/// bitwise combination of its member values.
/// </summary>
[Flags]
[SuppressMessage("Roslynator", "RCS1157:Composite enum value contains undefined flag", Justification = "This is a polyfill class matching .NET")]
[SuppressMessage("Major Code Smell", "S4070:Non-flags enums should not be marked with \"FlagsAttribute\"", Justification = "This is a polyfill class matching .NET")]
internal enum DynamicallyAccessedMemberTypes
{
    /// <summary>
    /// Specifies no members.
    /// </summary>
    None = 0,

    /// <summary>
    /// Specifies the default, parameterless public constructor.
    /// </summary>
    PublicParameterlessConstructor = 0x_0001,

    /// <summary>
    /// Specifies all public constructors.
    /// </summary>
    PublicConstructors = 0x_0002 | PublicParameterlessConstructor,

    /// <summary>
    /// Specifies all non-public constructors.
    /// </summary>
    NonPublicConstructors = 0x_0004,

    /// <summary>
    /// Specifies all public methods.
    /// </summary>
    PublicMethods = 0x_0008,

    /// <summary>
    /// Specifies all non-public methods.
    /// </summary>
    NonPublicMethods = 0x_0010,

    /// <summary>
    /// Specifies all public fields.
    /// </summary>
    PublicFields = 0x_0020,

    /// <summary>
    /// Specifies all non-public fields.
    /// </summary>
    NonPublicFields = 0x_0040,

    /// <summary>
    /// Specifies all public nested types.
    /// </summary>
    PublicNestedTypes = 0x_0080,

    /// <summary>
    /// Specifies all non-public nested types.
    /// </summary>
    NonPublicNestedTypes = 0x_0100,

    /// <summary>
    /// Specifies all public properties.
    /// </summary>
    PublicProperties = 0x_0200,

    /// <summary>
    /// Specifies all non-public properties.
    /// </summary>
    NonPublicProperties = 0x_0400,

    /// <summary>
    /// Specifies all public events.
    /// </summary>
    PublicEvents = 0x_0800,

    /// <summary>
    /// Specifies all non-public events.
    /// </summary>
    NonPublicEvents = 0x_1000,

    /// <summary>
    /// Specifies all interfaces implemented by the type.
    /// </summary>
    Interfaces = 0x_2000,

    /// <summary>
    /// Specifies all members.
    /// </summary>
    All = ~None,
}

#else
[assembly: TypeForwardedTo(typeof(DynamicallyAccessedMemberTypes))]
#endif

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// Polyfill implementation adapted from SimonCropp/Polyfill
// https://github.com/SimonCropp/Polyfill
#if !NET5_0_OR_GREATER

namespace System.Runtime.CompilerServices;

/// <summary>
/// Used to indicate to the compiler that a method should be called in its containing
/// module's initializer.
/// </summary>
/// <remarks>
/// When one or more valid methods with this attribute are found in a compilation, the compiler
/// will emit a module initializer that calls each of the attributed methods.
/// On .NET Framework this polyfill enables compilation only; the runtime does not invoke module initializers.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class ModuleInitializerAttribute : Attribute
{
}

#endif

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>The values of 6 observed properties, as one emission.</summary>
/// <typeparam name="T1">The type of the first observed property.</typeparam>
/// <typeparam name="T2">The type of the second observed property.</typeparam>
/// <typeparam name="T3">The type of the third observed property.</typeparam>
/// <typeparam name="T4">The type of the fourth observed property.</typeparam>
/// <typeparam name="T5">The type of the fifth observed property.</typeparam>
/// <typeparam name="T6">The type of the sixth observed property.</typeparam>
/// <param name="Property1">The value of the first observed property.</param>
/// <param name="Property2">The value of the second observed property.</param>
/// <param name="Property3">The value of the third observed property.</param>
/// <param name="Property4">The value of the fourth observed property.</param>
/// <param name="Property5">The value of the fifth observed property.</param>
/// <param name="Property6">The value of the sixth observed property.</param>
/// <remarks>
/// Multi-property observation emits every property's current value together, so the emission is one value
/// rather than a group of them. Being positional, it still deconstructs, so a subscriber can name the
/// properties it cares about without naming the type.
/// </remarks>
[DebuggerDisplay("PropertyValues: {Property1}, {Property2}, +4 more")]
public readonly record struct PropertyValues<T1, T2, T3, T4, T5, T6>(
    T1 Property1,
    T2 Property2,
    T3 Property3,
    T4 Property4,
    T5 Property5,
    T6 Property6);

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>The values of 14 observed properties, as one emission.</summary>
/// <typeparam name="T1">The type of the first observed property.</typeparam>
/// <typeparam name="T2">The type of the second observed property.</typeparam>
/// <typeparam name="T3">The type of the third observed property.</typeparam>
/// <typeparam name="T4">The type of the fourth observed property.</typeparam>
/// <typeparam name="T5">The type of the fifth observed property.</typeparam>
/// <typeparam name="T6">The type of the sixth observed property.</typeparam>
/// <typeparam name="T7">The type of the seventh observed property.</typeparam>
/// <typeparam name="T8">The type of the eighth observed property.</typeparam>
/// <typeparam name="T9">The type of the ninth observed property.</typeparam>
/// <typeparam name="T10">The type of the tenth observed property.</typeparam>
/// <typeparam name="T11">The type of the eleventh observed property.</typeparam>
/// <typeparam name="T12">The type of the twelfth observed property.</typeparam>
/// <typeparam name="T13">The type of the thirteenth observed property.</typeparam>
/// <typeparam name="T14">The type of the fourteenth observed property.</typeparam>
/// <param name="Property1">The value of the first observed property.</param>
/// <param name="Property2">The value of the second observed property.</param>
/// <param name="Property3">The value of the third observed property.</param>
/// <param name="Property4">The value of the fourth observed property.</param>
/// <param name="Property5">The value of the fifth observed property.</param>
/// <param name="Property6">The value of the sixth observed property.</param>
/// <param name="Property7">The value of the seventh observed property.</param>
/// <param name="Property8">The value of the eighth observed property.</param>
/// <param name="Property9">The value of the ninth observed property.</param>
/// <param name="Property10">The value of the tenth observed property.</param>
/// <param name="Property11">The value of the eleventh observed property.</param>
/// <param name="Property12">The value of the twelfth observed property.</param>
/// <param name="Property13">The value of the thirteenth observed property.</param>
/// <param name="Property14">The value of the fourteenth observed property.</param>
/// <remarks>
/// Multi-property observation emits every property's current value together, so the emission is one value
/// rather than a group of them. Being positional, it still deconstructs, so a subscriber can name the
/// properties it cares about without naming the type.
/// </remarks>
[DebuggerDisplay("PropertyValues: {Property1}, {Property2}, +12 more")]
public readonly record struct PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
    T1 Property1,
    T2 Property2,
    T3 Property3,
    T4 Property4,
    T5 Property5,
    T6 Property6,
    T7 Property7,
    T8 Property8,
    T9 Property9,
    T10 Property10,
    T11 Property11,
    T12 Property12,
    T13 Property13,
    T14 Property14);

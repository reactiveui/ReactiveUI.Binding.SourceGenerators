// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding;

/// <summary>
/// Extension methods for ReactiveUI property observation and binding.
/// These generic stubs serve as fallbacks that throw when no concrete generated overload is found.
/// The source generator emits concrete typed overloads that C# overload resolution prefers
/// over these generic stubs due to having fewer optional parameters (or matching type specificity).
/// Supports 1-16 property selectors for observation APIs (WhenChanged, WhenChanging, WhenAnyValue).
/// </summary>
public static partial class ReactiveUIBindingExtensions
{
    private const string NoGeneratedBindingMessage =
        "No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.";
}

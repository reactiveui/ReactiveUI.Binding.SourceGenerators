// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding.Helpers;

/// <summary>
/// Indicates to CA1062 that a parameter has been validated as not null by the method.
/// When applied to a parameter of a null-check helper method, CA1062 recognizes
/// that callers have validated the argument after the call returns.
/// </summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class ValidatedNotNullAttribute : Attribute;

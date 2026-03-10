// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Indicates that this View should be constructed once and then reused
/// every time its ViewModel's View is resolved. The source generator will
/// emit a cached singleton pattern instead of creating a new instance per resolution.
/// </summary>
/// <remarks>
/// This is not supported on Views that may be reused multiple
/// times in the Visual Tree.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class SingleInstanceViewAttribute : Attribute;

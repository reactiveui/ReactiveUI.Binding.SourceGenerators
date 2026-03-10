// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Apply this attribute to a view class to exclude it from automatic registration
/// when scanning for <see cref="IViewFor{T}"/> implementations during source generation
/// or runtime assembly scanning.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ExcludeFromViewRegistrationAttribute : Attribute;

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>The two types a BindCommand call site joins.</summary>
/// <param name="ViewTypeFullName">The fully qualified view type.</param>
/// <param name="ViewModelTypeFullName">The fully qualified view model type.</param>
[DebuggerDisplay("BindCommandSides: {ViewTypeFullName,nq} <- {ViewModelTypeFullName,nq}")]
internal readonly record struct BindCommandSides(string ViewTypeFullName, string ViewModelTypeFullName);

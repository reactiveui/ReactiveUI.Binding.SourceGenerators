// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>An assembly loaded for a test, and the context holding it.</summary>
/// <param name="Assembly">The loaded assembly.</param>
/// <param name="Context">The collectible context; dispose it to unload.</param>
[DebuggerDisplay("LoadedAssembly: {Assembly.GetName().Name,nq}")]
public readonly record struct LoadedAssembly(Assembly Assembly, CollectibleAssemblyLoadContext Context);

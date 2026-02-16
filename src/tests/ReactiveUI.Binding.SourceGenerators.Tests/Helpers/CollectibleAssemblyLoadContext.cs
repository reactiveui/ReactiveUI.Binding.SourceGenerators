// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.Loader;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>
/// A collectible assembly load context for loading in-memory compiled assemblies.
/// Falls back to the default context for framework and referenced assemblies.
/// </summary>
public sealed class CollectibleAssemblyLoadContext : AssemblyLoadContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CollectibleAssemblyLoadContext"/> class.
    /// </summary>
    public CollectibleAssemblyLoadContext()
        : base(isCollectible: true)
    {
    }

    /// <inheritdoc/>
    protected override Assembly? Load(AssemblyName assemblyName) =>
        Default.LoadFromAssemblyName(assemblyName);
}

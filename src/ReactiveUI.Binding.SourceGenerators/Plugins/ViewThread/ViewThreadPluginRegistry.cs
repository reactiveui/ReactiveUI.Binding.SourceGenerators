// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

/// <summary>Matches a binding target's type to the invoker a generated binding carries for it.</summary>
internal static class ViewThreadPluginRegistry
{
    /// <summary>The plugins, one per UI platform.</summary>
    private static readonly IViewThreadPlugin[] Plugins =
    [
        new WpfViewThreadPlugin(),
        new WinFormsViewThreadPlugin(),
        new MauiViewThreadPlugin(),
    ];

    /// <summary>The platform types each compilation resolves, in plugin order.</summary>
    private static readonly ConditionalWeakTable<Compilation, OwnerTypes> OwnerCache = new();

    /// <summary>Finds the invoker a generated binding carries for a target type.</summary>
    /// <param name="type">The target's type, or null when the model could not name it.</param>
    /// <param name="compilation">The compilation the type belongs to.</param>
    /// <returns>The invoker class name, or null when the type belongs to no supported platform.</returns>
    internal static string? InvokerFor(ITypeSymbol? type, Compilation compilation)
    {
        var owners = GetOwners(compilation);
        if (type is null || !owners.AnyResolved)
        {
            return null;
        }

        for (var current = type; current is not null; current = current.BaseType)
        {
            for (var i = 0; i < Plugins.Length; i++)
            {
                if (SymbolEqualityComparer.Default.Equals(current, owners.Types[i]))
                {
                    return Plugins[i].InvokerTypeName;
                }
            }
        }

        return null;
    }

    /// <summary>Lists the invokers whose platform type resolves in a compilation.</summary>
    /// <param name="compilation">The compilation to inspect.</param>
    /// <returns>The invoker class names, in plugin order.</returns>
    internal static EquatableArray<string> InvokersIn(Compilation compilation)
    {
        var owners = GetOwners(compilation);
        var names = new List<string>(Plugins.Length);

        for (var i = 0; i < Plugins.Length; i++)
        {
            if (owners.Types[i] is not null)
            {
                names.Add(Plugins[i].InvokerTypeName);
            }
        }

        return new([.. names]);
    }

    /// <summary>Emits the declarations of the named invokers, in plugin order.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="invokerTypeNames">The invoker class names to declare.</param>
    /// <param name="supportsNullable">Whether the consumer compiles with nullable reference types.</param>
    internal static void EmitInvokers(StringBuilder sb, EquatableArray<string> invokerTypeNames, bool supportsNullable)
    {
        var nullableSuffix = supportsNullable ? "?" : string.Empty;

        for (var i = 0; i < Plugins.Length; i++)
        {
            if (Contains(invokerTypeNames, Plugins[i].InvokerTypeName))
            {
                Plugins[i].EmitInvoker(sb, nullableSuffix);
            }
        }
    }

    /// <summary>Determines whether a list of names holds one name.</summary>
    /// <param name="names">The names to search.</param>
    /// <param name="name">The name to find.</param>
    /// <returns><see langword="true"/> when the name is in the list; otherwise <see langword="false"/>.</returns>
    private static bool Contains(EquatableArray<string> names, string name)
    {
        for (var i = 0; i < names.Length; i++)
        {
            if (string.Equals(names[i], name, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Resolves the platform types a compilation references, once per compilation.</summary>
    /// <param name="compilation">The compilation to inspect.</param>
    /// <returns>The resolved types, in plugin order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static OwnerTypes GetOwners(Compilation compilation) =>
        OwnerCache.GetValue(compilation, static c => new(c));

    /// <summary>The platform types one compilation resolves, in plugin order.</summary>
    private sealed class OwnerTypes
    {
        /// <summary>Initializes a new instance of the <see cref="OwnerTypes"/> class.</summary>
        /// <param name="compilation">The compilation to resolve the types in.</param>
        public OwnerTypes(Compilation compilation)
        {
            Types = new INamedTypeSymbol?[Plugins.Length];
            for (var i = 0; i < Plugins.Length; i++)
            {
                Types[i] = compilation.GetTypeByMetadataName(Plugins[i].OwnerMetadataName);
                AnyResolved |= Types[i] is not null;
            }
        }

        /// <summary>Gets the resolved types; an entry is null when the compilation does not reference that platform.</summary>
        public INamedTypeSymbol?[] Types { get; }

        /// <summary>Gets a value indicating whether the compilation references any supported platform.</summary>
        public bool AnyResolved { get; }
    }
}

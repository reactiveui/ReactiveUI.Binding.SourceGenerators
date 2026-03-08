// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>
/// Provides shared validation helpers extracted from the extractor classes.
/// These are internal methods that enable direct unit testing of guard-clause branches.
/// </summary>
internal static class ExtractorValidation
{
    /// <summary>
    /// Checks whether a containing type name matches one of the recognized
    /// extension class names used by this generator (stub, scheduler, or generated).
    /// </summary>
    /// <param name="containingTypeName">The containing type name to check.</param>
    /// <returns><see langword="true"/> if the name is recognized; otherwise <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsRecognizedExtensionClass(string? containingTypeName) =>
        containingTypeName == Constants.StubExtensionClassName
        || containingTypeName == Constants.SchedulerExtensionClassName
        || containingTypeName == Constants.GeneratedExtensionClassName;

    /// <summary>
    /// Checks whether an invocation has at least the required number of arguments.
    /// </summary>
    /// <param name="argumentCount">The actual argument count.</param>
    /// <param name="minimumRequired">The minimum required argument count.</param>
    /// <returns><see langword="true"/> if there are enough arguments; otherwise <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasMinimumArguments(int argumentCount, int minimumRequired) =>
        argumentCount >= minimumRequired;

    /// <summary>
    /// Checks whether an immutable array of items is non-empty and should be processed.
    /// Used by RegistrationGenerator to guard against empty type detection results.
    /// </summary>
    /// <typeparam name="T">The type of items in the array.</typeparam>
    /// <param name="items">The immutable array to check.</param>
    /// <returns><see langword="true"/> if the array has items; otherwise <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasItems<T>(ImmutableArray<T> items) =>
        !items.IsDefaultOrEmpty;

    /// <summary>
    /// Extracts an <see cref="IMethodSymbol"/> from a <see cref="SymbolInfo"/>,
    /// returning null if the resolved symbol is not a method.
    /// </summary>
    /// <param name="symbolInfo">The symbol info from GetSymbolInfo.</param>
    /// <returns>The method symbol, or null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IMethodSymbol? ExtractMethodSymbol(SymbolInfo symbolInfo) =>
        symbolInfo.Symbol as IMethodSymbol;

    /// <summary>
    /// Gets the fully qualified display name of a type symbol,
    /// returning null if the symbol is null.
    /// </summary>
    /// <param name="type">The type symbol, which may be null.</param>
    /// <returns>The fully qualified type name, or null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? GetTypeDisplayName(ITypeSymbol? type) =>
        type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    /// <summary>
    /// Searches method parameters for a selector or conversion function parameter
    /// and returns the fully qualified return type (the last type argument of the Func).
    /// </summary>
    /// <param name="parameters">The method parameters to search.</param>
    /// <param name="parameterNames">The parameter names to match (e.g. "selector", "conversionFunc").</param>
    /// <returns>The fully qualified return type, or null if no matching parameter was found.</returns>
    internal static string? FindSelectorReturnType(ImmutableArray<IParameterSymbol> parameters, params string[] parameterNames)
    {
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (parameter.Type is INamedTypeSymbol { TypeArguments.Length: > 0 } funcType)
            {
                for (var n = 0; n < parameterNames.Length; n++)
                {
                    if (parameter.Name == parameterNames[n])
                    {
                        return funcType.TypeArguments[funcType.TypeArguments.Length - 1]
                            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Resolves the event delegate type to its event args type.
    /// Returns "global::System.EventArgs" as a fallback when the delegate type
    /// is not a named type or has a non-standard parameter count.
    /// </summary>
    /// <param name="delegateType">The event's delegate type symbol.</param>
    /// <returns>The fully qualified event args type name.</returns>
    internal static string ResolveEventArgsType(ITypeSymbol? delegateType)
    {
        if (delegateType is not INamedTypeSymbol namedDelegateType)
        {
            return "global::System.EventArgs";
        }

        var invokeMethod = namedDelegateType.DelegateInvokeMethod;
        if (invokeMethod is { Parameters.Length: 2 })
        {
            return invokeMethod.Parameters[1].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        return "global::System.EventArgs";
    }
}

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
    /// <summary>The number of parameters on a standard .NET event handler delegate (sender, event args).</summary>
    private const int StandardEventHandlerParameterCount = 2;

    /// <summary>
    /// Checks whether a containing type name matches one of the recognized
    /// extension class names used by this generator (stub, scheduler, or generated).
    /// </summary>
    /// <param name="containingTypeName">The containing type name to check.</param>
    /// <returns><see langword="true"/> if the name is recognized; otherwise <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsRecognizedExtensionClass(string? containingTypeName) =>
        containingTypeName is Constants.StubExtensionClassName
            or Constants.SchedulerExtensionClassName
            or Constants.GeneratedExtensionClassName;

    /// <summary>
    /// Checks whether the type declaring an invoked method is one of the recognized extension classes
    /// used by this generator (stub, scheduler, or generated).
    /// </summary>
    /// <param name="containingType">The type containing the invoked method.</param>
    /// <returns><see langword="true"/> if the declaring class is recognized; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// An extension block declares its members in a synthesized grouping type rather than in the static class
    /// itself, so the class carrying the recognized name is one level further out. Which of the two a call site
    /// resolves to is decided by the compiler that hosts the generator rather than by anything in the consumer's
    /// project, and the two disagree across compiler versions, so both shapes are accepted. Rejecting the
    /// grouping type drops every call site of an API declared that way while leaving the rest generating.
    /// </remarks>
    internal static bool IsRecognizedExtensionClass(INamedTypeSymbol? containingType) =>
        containingType is not null
        && (IsRecognizedExtensionClass(containingType.Name)
            || (IsExtensionGroupingType(containingType)
                && IsRecognizedExtensionClass(containingType.ContainingType?.Name)));

    /// <summary>Checks whether an invocation has at least the required number of arguments.</summary>
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

    /// <summary>Extracts an <see cref="IMethodSymbol"/> from a <see cref="SymbolInfo"/>, returning null if the resolved symbol is not a method.</summary>
    /// <param name="symbolInfo">The symbol info from GetSymbolInfo.</param>
    /// <returns>The method symbol, or null.</returns>
    /// <remarks>
    /// Only a symbol the model actually resolved is used. Candidates are deliberately not consulted: a call the
    /// model could not narrow may have candidates of differing shape, and generating from one of those means
    /// reading a call's arguments against the wrong parameter list.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IMethodSymbol? ExtractMethodSymbol(SymbolInfo symbolInfo) =>
        symbolInfo.Symbol as IMethodSymbol;

    /// <summary>Gets the fully qualified display name of a type symbol, returning null if the symbol is null.</summary>
    /// <param name="type">The type symbol, which may be null.</param>
    /// <returns>The fully qualified type name, or null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? GetTypeDisplayName(ITypeSymbol? type) =>
        type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    /// <summary>Names a type only when a generated overload could declare a parameter of it.</summary>
    /// <param name="type">The type symbol, which may be null.</param>
    /// <returns>The fully qualified type name, or <see langword="null"/> when no overload could name it.</returns>
    /// <remarks>
    /// <para>
    /// A call made through a type parameter binds to whatever closes it, which the call site does not name.
    /// Writing the parameter's own name into an overload puts an identifier no consumer declared into their
    /// build, so the whole compilation fails over generated code they cannot edit - including every unrelated
    /// call site in the project. Declining the call site leaves it on the runtime stub instead.
    /// </para>
    /// <para>
    /// A static type fails the same way and reaches here by a different route: a call written through the
    /// stub's declaring class - <c>ReactiveUIBindingExtensions.WhenChanged(vm, x => x.Name)</c> - puts that
    /// class where the observed object goes, and no member can declare a parameter of it or close a generic
    /// over it. Such a call resolves against that class's own members, so neither a generated overload nor an
    /// interceptor matching the call written on an instance is a candidate for it either way.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? GetDeclarableTypeDisplayName(ITypeSymbol? type) =>
        type is INamedTypeSymbol { IsStatic: false } named ? GetTypeDisplayName(named) : null;

    /// <summary>
    /// Searches method parameters for a selector or conversion function parameter
    /// and returns the fully qualified return type (the last type argument of the Func).
    /// </summary>
    /// <param name="parameters">The method parameters to search.</param>
    /// <param name="parameterNames">The parameter names to match (e.g. "selector", "conversionFunc").</param>
    /// <returns>The fully qualified return type, or null if no matching parameter was found.</returns>
    internal static string? FindSelectorReturnType(
        ImmutableArray<IParameterSymbol> parameters,
        params string[] parameterNames)
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
        return invokeMethod is { Parameters.Length: StandardEventHandlerParameterCount }
            ? invokeMethod.Parameters[1].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            : "global::System.EventArgs";
    }

    /// <summary>Names one of the type arguments the compiler settled on for a resolved call.</summary>
    /// <param name="methodSymbol">The method the call resolved to.</param>
    /// <param name="index">Which of its type arguments to name.</param>
    /// <returns>The fully qualified name, or <see langword="null"/> when nothing declarable is there.</returns>
    /// <remarks>
    /// The route to a type when the property path is not one: a selector built at run time still resolves to a
    /// method whose type arguments the compiler inferred, and those are the types the generated member has to
    /// declare. An index outside the list answers null rather than throwing, so a caller reading a shape this
    /// package does not serve declines the call site instead of failing the whole generation pass.
    /// </remarks>
    internal static string? TypeArgumentDisplayName(IMethodSymbol methodSymbol, int index) =>
        index < 0 || index >= methodSymbol.TypeArguments.Length
            ? null
            : GetDeclarableTypeDisplayName(methodSymbol.TypeArguments[index]);

    /// <summary>Names the type a selector produces, read from the selector rather than from its body.</summary>
    /// <param name="semanticModel">The semantic model for the call site.</param>
    /// <param name="selector">The argument the selector was passed as.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The fully qualified produced type, or <see langword="null"/> when nothing declarable is there.</returns>
    /// <remarks>
    /// A selector held in a variable, or built at run time, has no body a path can be read from - but it still
    /// has a type, and that type is <c>Expression&lt;Func&lt;TInput, TProduced&gt;&gt;</c>. Taking the produced
    /// type from there is what lets a call site the compiler could not read still be served, because the
    /// generated member has to declare exactly the types the call already has.
    /// </remarks>
    internal static string? SelectorProducedTypeDisplayName(
        SemanticModel semanticModel,
        Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax selector,
        CancellationToken ct)
    {
        // Expression<Func<TInput, TProduced>>: unwrap the expression, then take what the delegate returns.
        return semanticModel.GetTypeInfo(selector, ct).ConvertedType
                is INamedTypeSymbol { TypeArguments.Length: 1 } expression
            && expression.TypeArguments[0] is INamedTypeSymbol { TypeArguments.Length: > 1 } func
            ? GetTypeDisplayName(func.TypeArguments[func.TypeArguments.Length - 1])
            : null;
    }

    /// <summary>Checks whether a type is the synthesized grouping type that holds an extension block's members.</summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is an extension grouping type; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// The grouping type is unnamed when the declaration is read from source and is named <c>&lt;&gt;E__N</c>
    /// when it is read from metadata. Neither spelling is a legal C# identifier, so no type a consumer can
    /// declare is mistaken for one.
    /// </remarks>
    private static bool IsExtensionGroupingType(INamedTypeSymbol type) =>
        type.Name.Length == 0 || type.Name[0] == '<';
}

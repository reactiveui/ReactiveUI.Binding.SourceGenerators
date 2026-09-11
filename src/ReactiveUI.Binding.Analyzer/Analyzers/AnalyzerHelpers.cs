// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>Shared helper methods for analyzers. No LINQ, manual loops.</summary>
internal static class AnalyzerHelpers
{
    /// <summary>Checks if a method symbol belongs to our generated extension class.</summary>
    /// <param name="methodSymbol">The method symbol to check.</param>
    /// <returns>true if the method is from our generated extension class.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsBindingExtensionMethod(IMethodSymbol methodSymbol) =>
        methodSymbol.ContainingType?.Name is SourceGenerators.Constants.GeneratedExtensionClassName
            or SourceGenerators.Constants.StubExtensionClassName;

    /// <summary>Checks if an expression is an inline lambda (not a variable reference or method call).</summary>
    /// <param name="expression">The expression to check.</param>
    /// <returns>true if the expression is an inline lambda.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsInlineLambda(ExpressionSyntax expression) =>
        expression is SimpleLambdaExpressionSyntax or ParenthesizedLambdaExpressionSyntax;

    /// <summary>Checks if a type supports before-change notifications based on its notification mechanism.</summary>
    /// <param name="typeSymbol">The type to check.</param>
    /// <param name="compilation">The current compilation for type resolution.</param>
    /// <param name="mechanism">Output: the name of the detected mechanism.</param>
    /// <returns>true if the type supports before-change notifications.</returns>
    internal static bool HasBeforeChangeSupport(
        INamedTypeSymbol typeSymbol,
        Compilation compilation,
        out string mechanism)
    {
        // IReactiveObject supports before-change via GetChangingObservable()
        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.IReactiveObjectMetadataName, byInterface: true))
        {
            mechanism = "IReactiveObject";
            return true;
        }

        // INotifyPropertyChanging supports before-change
        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.INotifyPropertyChangingMetadataName, byInterface: true))
        {
            mechanism = "INotifyPropertyChanging";
            return true;
        }

        // Platform types that DON'T support before-change
        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.WpfDependencyObjectMetadataName, byInterface: false))
        {
            mechanism = "WPF DependencyObject";
            return false;
        }

        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.WinUIDependencyObjectMetadataName, byInterface: false))
        {
            mechanism = "WinUI DependencyObject";
            return false;
        }

        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.WinFormsComponentMetadataName, byInterface: false))
        {
            mechanism = "WinForms Component";
            return false;
        }

        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.AndroidViewMetadataName, byInterface: false))
        {
            mechanism = "Android View";
            return false;
        }

        // KVO (NSObject) supports before-change
        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.NSObjectMetadataName, byInterface: false))
        {
            mechanism = "KVO";
            return true;
        }

        // INPC without INotifyPropertyChanging
        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.INotifyPropertyChangedMetadataName, byInterface: true))
        {
            mechanism = "INotifyPropertyChanged (without INotifyPropertyChanging)";
            return false;
        }

        mechanism = "unknown";
        return false;
    }

    /// <summary>
    /// Extracts the first type argument from a method symbol as an <see cref="INamedTypeSymbol"/>.
    /// Returns null if the method has no type arguments or the first argument is not a named type.
    /// </summary>
    /// <param name="methodSymbol">The method symbol to extract from.</param>
    /// <returns>The first type argument as <see cref="INamedTypeSymbol"/>, or null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static INamedTypeSymbol? ExtractFirstTypeArgument(IMethodSymbol methodSymbol) => ExtractTypeArgument(methodSymbol, 0);

    /// <summary>
    /// Extracts one of a method's type arguments as an <see cref="INamedTypeSymbol"/>. Returns null when the
    /// method has fewer arguments than that, or when the one asked for is not a named type.
    /// </summary>
    /// <param name="methodSymbol">The method symbol to extract from.</param>
    /// <param name="index">Which type argument to read.</param>
    /// <returns>The type argument as <see cref="INamedTypeSymbol"/>, or null.</returns>
    /// <remarks>
    /// Which argument names the observed object differs by API: most name it first, while the ones taking a
    /// stream the caller already built name that stream's value type there instead.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static INamedTypeSymbol? ExtractTypeArgument(IMethodSymbol methodSymbol, int index) =>
        methodSymbol.TypeArguments.Length <= index ? null : methodSymbol.TypeArguments[index] as INamedTypeSymbol;

    /// <summary>
    /// Determines whether a method's first type argument lacks any observable notification mechanism.
    /// Returns <c>false</c> if the method has no type arguments (non-generic dispatch overload).
    /// </summary>
    /// <param name="methodSymbol">The method symbol.</param>
    /// <param name="compilation">The current compilation.</param>
    /// <param name="sourceType">The resolved source type, if the check matched.</param>
    /// <returns><c>true</c> if the type has no observable mechanism.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool LacksObservableMechanism(
        IMethodSymbol methodSymbol,
        Compilation compilation,
        out INamedTypeSymbol? sourceType) =>
        LacksObservableMechanism(methodSymbol, compilation, 0, out sourceType);

    /// <summary>
    /// Determines whether the type argument naming this API's observed object lacks any observable notification
    /// mechanism. Returns <c>false</c> when the method has no such argument (a non-generic dispatch overload).
    /// </summary>
    /// <param name="methodSymbol">The method symbol.</param>
    /// <param name="compilation">The current compilation.</param>
    /// <param name="typeArgumentIndex">Which type argument names the observed object.</param>
    /// <param name="sourceType">The resolved source type, if the check matched.</param>
    /// <returns><c>true</c> if the type has no observable mechanism.</returns>
    internal static bool LacksObservableMechanism(
        IMethodSymbol methodSymbol,
        Compilation compilation,
        int typeArgumentIndex,
        out INamedTypeSymbol? sourceType)
    {
        sourceType = ExtractTypeArgument(methodSymbol, typeArgumentIndex);
        return sourceType is not null && !HasObservableMechanism(sourceType, compilation);
    }

    /// <summary>
    /// Determines whether a method's first type argument lacks before-change notification support.
    /// Returns <c>false</c> if the method has no type arguments (non-generic dispatch overload).
    /// </summary>
    /// <param name="methodSymbol">The method symbol.</param>
    /// <param name="compilation">The current compilation.</param>
    /// <param name="receiverType">The resolved receiver type, if the check matched.</param>
    /// <param name="mechanism">The detected notification mechanism name.</param>
    /// <returns><c>true</c> if the type does NOT support before-change notifications.</returns>
    internal static bool LacksBeforeChangeSupport(
        IMethodSymbol methodSymbol,
        Compilation compilation,
        out INamedTypeSymbol? receiverType,
        out string mechanism)
    {
        mechanism = string.Empty;
        receiverType = ExtractFirstTypeArgument(methodSymbol);
        return receiverType is not null && !HasBeforeChangeSupport(receiverType, compilation, out mechanism);
    }

    /// <summary>
    /// Determines whether a binding method's source type implements
    /// <c>INotifyDataErrorInfo</c>, requiring the runtime validation engine.
    /// Returns <c>false</c> if the method has no type arguments or the
    /// <c>INotifyDataErrorInfo</c> type cannot be resolved in the compilation.
    /// </summary>
    /// <param name="methodSymbol">The binding method symbol.</param>
    /// <param name="compilation">The current compilation.</param>
    /// <param name="sourceType">The resolved source type, if the check passed.</param>
    /// <returns><c>true</c> if the source type implements <c>INotifyDataErrorInfo</c>.</returns>
    internal static bool ImplementsDataErrorInfo(
        IMethodSymbol methodSymbol,
        Compilation compilation,
        out INamedTypeSymbol? sourceType)
    {
        sourceType = ExtractFirstTypeArgument(methodSymbol);
        if (sourceType is null)
        {
            return false;
        }

        var dataErrorInfo =
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.INotifyDataErrorInfoMetadataName);
        return dataErrorInfo is not null && ImplementsInterface(sourceType, dataErrorInfo);
    }

    /// <summary>
    /// Determines whether a type notifies about property changes at all, through any of the mechanisms this
    /// library observes: INotifyPropertyChanged, IReactiveObject, a WPF or WinUI dependency object, an Apple
    /// NSObject, a WinForms component, or an Android view.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="compilation">The current compilation for type resolution.</param>
    /// <returns><c>true</c> if the type supports property observation; otherwise, <c>false</c>.</returns>
    internal static bool HasObservableMechanism(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        INamedTypeSymbol?[] interfaces =
        [
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.INotifyPropertyChangedMetadataName),
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.IReactiveObjectMetadataName),
        ];

        for (var i = 0; i < interfaces.Length; i++)
        {
            if (interfaces[i] is { } observable && ImplementsInterface(typeSymbol, observable))
            {
                return true;
            }
        }

        INamedTypeSymbol?[] baseTypes =
        [
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.WpfDependencyObjectMetadataName),
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.WinUIDependencyObjectMetadataName),
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.NSObjectMetadataName),
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.WinFormsComponentMetadataName),
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.AndroidViewMetadataName),
        ];

        for (var i = 0; i < baseTypes.Length; i++)
        {
            if (baseTypes[i] is { } observable && InheritsFrom(typeSymbol, observable))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a type implements a specific interface.</summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="interfaceSymbol">The interface symbol to look for.</param>
    /// <returns><c>true</c> if the type implements the specified interface; otherwise, <c>false</c>.</returns>
    internal static bool ImplementsInterface(INamedTypeSymbol typeSymbol, INamedTypeSymbol interfaceSymbol)
    {
        var allInterfaces = typeSymbol.AllInterfaces;
        for (var i = 0; i < allInterfaces.Length; i++)
        {
            if (SymbolEqualityComparer.Default.Equals(allInterfaces[i], interfaceSymbol))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a type inherits from a specific base type.</summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="baseTypeSymbol">The base type symbol to look for in the inheritance hierarchy.</param>
    /// <returns><c>true</c> if the type inherits from the specified base type; otherwise, <c>false</c>.</returns>
    internal static bool InheritsFrom(INamedTypeSymbol typeSymbol, INamedTypeSymbol baseTypeSymbol)
    {
        var current = typeSymbol.BaseType;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseTypeSymbol))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Resolves a well-known type by metadata name and tests whether <paramref name="typeSymbol"/>
    /// matches it, either by interface implementation or base-type inheritance.
    /// </summary>
    /// <param name="typeSymbol">The type to test.</param>
    /// <param name="compilation">The current compilation for type resolution.</param>
    /// <param name="metadataName">The metadata name of the well-known type to resolve.</param>
    /// <param name="byInterface">
    /// <c>true</c> to test interface implementation; <c>false</c> to test base-type inheritance.
    /// </param>
    /// <returns><c>true</c> if the resolved type exists and matches; otherwise, <c>false</c>.</returns>
    private static bool Matches(
        INamedTypeSymbol typeSymbol,
        Compilation compilation,
        string metadataName,
        bool byInterface)
    {
        var target = compilation.GetTypeByMetadataName(metadataName);
        if (target is null)
        {
            return false;
        }

        return byInterface
            ? ImplementsInterface(typeSymbol, target)
            : InheritsFrom(typeSymbol, target);
    }
}

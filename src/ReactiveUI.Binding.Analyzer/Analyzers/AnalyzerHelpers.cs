// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>
/// Shared helper methods for analyzers. No LINQ, manual loops.
/// </summary>
internal static class AnalyzerHelpers
{
    /// <summary>
    /// Checks if a method symbol belongs to our generated extension class.
    /// </summary>
    /// <param name="methodSymbol">The method symbol to check.</param>
    /// <returns>true if the method is from our generated extension class.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsBindingExtensionMethod(IMethodSymbol methodSymbol)
    {
        var containingType = methodSymbol.ContainingType;
        if (containingType == null)
        {
            return false;
        }

        var name = containingType.Name;
        return name == SourceGenerators.Constants.GeneratedExtensionClassName
            || name == SourceGenerators.Constants.StubExtensionClassName;
    }

    /// <summary>
    /// Checks if an expression is an inline lambda (not a variable reference or method call).
    /// </summary>
    /// <param name="expression">The expression to check.</param>
    /// <returns>true if the expression is an inline lambda.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsInlineLambda(ExpressionSyntax expression) => expression is SimpleLambdaExpressionSyntax or ParenthesizedLambdaExpressionSyntax;

    /// <summary>
    /// Checks if a type supports before-change notifications based on its notification mechanism.
    /// </summary>
    /// <param name="typeSymbol">The type to check.</param>
    /// <param name="compilation">The current compilation for type resolution.</param>
    /// <param name="mechanism">Output: the name of the detected mechanism.</param>
    /// <returns>true if the type supports before-change notifications.</returns>
    internal static bool HasBeforeChangeSupport(INamedTypeSymbol typeSymbol, Compilation compilation, out string mechanism)
    {
        mechanism = string.Empty;

        // IReactiveObject supports before-change via GetChangingObservable()
        var iro = compilation.GetTypeByMetadataName(SourceGenerators.Constants.IReactiveObjectMetadataName);
        if (iro != null && ImplementsInterface(typeSymbol, iro))
        {
            mechanism = "IReactiveObject";
            return true;
        }

        // INotifyPropertyChanging supports before-change
        var inpChanging = compilation.GetTypeByMetadataName(SourceGenerators.Constants.INotifyPropertyChangingMetadataName);
        if (inpChanging != null && ImplementsInterface(typeSymbol, inpChanging))
        {
            mechanism = "INotifyPropertyChanging";
            return true;
        }

        // Check for platform types that DON'T support before-change
        var wpfDO = compilation.GetTypeByMetadataName(SourceGenerators.Constants.WpfDependencyObjectMetadataName);
        if (wpfDO != null && InheritsFrom(typeSymbol, wpfDO))
        {
            mechanism = "WPF DependencyObject";
            return false;
        }

        var winuiDO = compilation.GetTypeByMetadataName(SourceGenerators.Constants.WinUIDependencyObjectMetadataName);
        if (winuiDO != null && InheritsFrom(typeSymbol, winuiDO))
        {
            mechanism = "WinUI DependencyObject";
            return false;
        }

        var winformsComp = compilation.GetTypeByMetadataName(SourceGenerators.Constants.WinFormsComponentMetadataName);
        if (winformsComp != null && InheritsFrom(typeSymbol, winformsComp))
        {
            mechanism = "WinForms Component";
            return false;
        }

        var androidView = compilation.GetTypeByMetadataName(SourceGenerators.Constants.AndroidViewMetadataName);
        if (androidView != null && InheritsFrom(typeSymbol, androidView))
        {
            mechanism = "Android View";
            return false;
        }

        // KVO (NSObject) supports before-change
        var nsObject = compilation.GetTypeByMetadataName(SourceGenerators.Constants.NSObjectMetadataName);
        if (nsObject != null && InheritsFrom(typeSymbol, nsObject))
        {
            mechanism = "KVO";
            return true;
        }

        // INPC without INotifyPropertyChanging
        var inpc = compilation.GetTypeByMetadataName(SourceGenerators.Constants.INotifyPropertyChangedMetadataName);
        if (inpc != null && ImplementsInterface(typeSymbol, inpc))
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
    internal static INamedTypeSymbol? ExtractFirstTypeArgument(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.TypeArguments.Length == 0)
        {
            return null;
        }

        return methodSymbol.TypeArguments[0] as INamedTypeSymbol;
    }

    /// <summary>
    /// Determines whether a method's first type argument lacks any observable notification mechanism.
    /// Returns <c>false</c> if the method has no type arguments (non-generic dispatch overload).
    /// </summary>
    /// <param name="methodSymbol">The method symbol.</param>
    /// <param name="compilation">The current compilation.</param>
    /// <param name="sourceType">The resolved source type, if the check matched.</param>
    /// <returns><c>true</c> if the type has no observable mechanism.</returns>
    internal static bool LacksObservableMechanism(
        IMethodSymbol methodSymbol,
        Compilation compilation,
        out INamedTypeSymbol? sourceType)
    {
        sourceType = ExtractFirstTypeArgument(methodSymbol);
        if (sourceType == null)
        {
            return false;
        }

        return !TypeAnalyzer.HasObservableMechanism(sourceType, compilation);
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
        if (receiverType == null)
        {
            return false;
        }

        return !HasBeforeChangeSupport(receiverType, compilation, out mechanism);
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
        if (sourceType == null)
        {
            return false;
        }

        var dataErrorInfo = compilation.GetTypeByMetadataName(SourceGenerators.Constants.INotifyDataErrorInfoMetadataName);
        if (dataErrorInfo == null)
        {
            return false;
        }

        return ImplementsInterface(sourceType, dataErrorInfo);
    }

    /// <summary>
    /// Determines whether a type implements a specific interface.
    /// </summary>
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

    /// <summary>
    /// Determines whether a type inherits from a specific base type.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="baseTypeSymbol">The base type symbol to look for in the inheritance hierarchy.</param>
    /// <returns><c>true</c> if the type inherits from the specified base type; otherwise, <c>false</c>.</returns>
    internal static bool InheritsFrom(INamedTypeSymbol typeSymbol, INamedTypeSymbol baseTypeSymbol)
    {
        var current = typeSymbol.BaseType;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseTypeSymbol))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }
}

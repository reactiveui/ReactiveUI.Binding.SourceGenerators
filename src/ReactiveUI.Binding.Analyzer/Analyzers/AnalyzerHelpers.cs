// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
    internal static bool IsBindingExtensionMethod(IMethodSymbol methodSymbol)
    {
        return methodSymbol.ContainingType?.Name == SourceGenerators.Constants.GeneratedExtensionClassName;
    }

    /// <summary>
    /// Checks if an expression is an inline lambda (not a variable reference or method call).
    /// </summary>
    /// <param name="expression">The expression to check.</param>
    /// <returns>true if the expression is an inline lambda.</returns>
    internal static bool IsInlineLambda(ExpressionSyntax expression)
    {
        return expression is SimpleLambdaExpressionSyntax or ParenthesizedLambdaExpressionSyntax;
    }

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

    internal static bool ImplementsInterface(INamedTypeSymbol typeSymbol, INamedTypeSymbol interfaceSymbol)
    {
        var allInterfaces = typeSymbol.AllInterfaces;
        for (int i = 0; i < allInterfaces.Length; i++)
        {
            if (SymbolEqualityComparer.Default.Equals(allInterfaces[i], interfaceSymbol))
            {
                return true;
            }
        }

        return false;
    }

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

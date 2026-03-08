// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>
/// Pipeline A transform: extracts ClassBindingInfo from class declarations.
/// </summary>
internal static class TypeDetectionExtractor
{
    /// <summary>
    /// Pipeline A transform: extracts ClassBindingInfo from a class declaration with a base list.
    /// Sets boolean flags by walking AllInterfaces + base type chain.
    /// </summary>
    /// <param name="context">The generator syntax context containing the semantic model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A ClassBindingInfo POCO, or null if the node is not relevant.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static ClassBindingInfo? ExtractClassBindingInfo(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;

        var semanticModel = context.SemanticModel;
        var typeSymbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(classDecl, ct)!;

        var wellKnown = SymbolHelpers.GetWellKnownSymbols(semanticModel.Compilation);

        var implementsIReactiveObject = false;
        var implementsINPC = false;
        var implementsINPChanging = false;

        // Walk AllInterfaces (includes inherited interfaces)
        var allInterfaces = typeSymbol.AllInterfaces;
        for (var i = 0; i < allInterfaces.Length; i++)
        {
            ct.ThrowIfCancellationRequested();
            var iface = allInterfaces[i];

            if (wellKnown.IReactiveObject != null && SymbolEqualityComparer.Default.Equals(iface, wellKnown.IReactiveObject))
            {
                implementsIReactiveObject = true;
            }

            if (wellKnown.INPC != null && SymbolEqualityComparer.Default.Equals(iface, wellKnown.INPC))
            {
                implementsINPC = true;
            }

            if (wellKnown.INPChanging != null && SymbolEqualityComparer.Default.Equals(iface, wellKnown.INPChanging))
            {
                implementsINPChanging = true;
            }
        }

        // Walk base type chain for platform detection
        var inheritsWpfDependencyObject = false;
        var inheritsWinUIDependencyObject = false;
        var inheritsNSObject = false;
        var inheritsWinFormsComponent = false;
        var inheritsAndroidView = false;

        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            ct.ThrowIfCancellationRequested();

            if (wellKnown.WpfDependencyObject != null && SymbolEqualityComparer.Default.Equals(baseType, wellKnown.WpfDependencyObject))
            {
                inheritsWpfDependencyObject = true;
            }

            if (wellKnown.WinUIDependencyObject != null && SymbolEqualityComparer.Default.Equals(baseType, wellKnown.WinUIDependencyObject))
            {
                inheritsWinUIDependencyObject = true;
            }

            if (wellKnown.NSObject != null && SymbolEqualityComparer.Default.Equals(baseType, wellKnown.NSObject))
            {
                inheritsNSObject = true;
            }

            if (wellKnown.WinFormsComponent != null && SymbolEqualityComparer.Default.Equals(baseType, wellKnown.WinFormsComponent))
            {
                inheritsWinFormsComponent = true;
            }

            if (wellKnown.AndroidView != null && SymbolEqualityComparer.Default.Equals(baseType, wellKnown.AndroidView))
            {
                inheritsAndroidView = true;
            }

            baseType = baseType.BaseType;
        }

        // Extract properties
        var properties = ExtractProperties(typeSymbol, ct);

        return new ClassBindingInfo(
            FullyQualifiedName: typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            MetadataName: typeSymbol.MetadataName,
            ImplementsIReactiveObject: implementsIReactiveObject,
            ImplementsINPC: implementsINPC,
            ImplementsINPChanging: implementsINPChanging,
            InheritsWpfDependencyObject: inheritsWpfDependencyObject,
            InheritsWinUIDependencyObject: inheritsWinUIDependencyObject,
            InheritsNSObject: inheritsNSObject,
            InheritsWinFormsComponent: inheritsWinFormsComponent,
            InheritsAndroidView: inheritsAndroidView,
            Properties: properties);
    }

    /// <summary>
    /// Extracts the properties from a named type symbol.
    /// </summary>
    /// <param name="typeSymbol">The type symbol.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An array of observable property info.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static EquatableArray<ObservablePropertyInfo> ExtractProperties(INamedTypeSymbol typeSymbol, CancellationToken ct)
    {
        var properties = new List<ObservablePropertyInfo>(16);
        var members = typeSymbol.GetMembers();

        for (var i = 0; i < members.Length; i++)
        {
            ct.ThrowIfCancellationRequested();
            if (members[i] is not IPropertySymbol property)
            {
                continue;
            }

            if (property.IsStatic || property.IsWriteOnly)
            {
                continue;
            }

            var hasPublicGetter = property.GetMethod!.DeclaredAccessibility == Accessibility.Public;
            var isIndexer = property.IsIndexer;

            // Check if it's a DependencyProperty (heuristic: companion static field ending in "Property")
            var isDependencyProperty = false;
            for (var j = 0; j < members.Length; j++)
            {
                if (members[j] is IFieldSymbol { IsStatic: true } field
                    && field.Name == property.Name + "Property")
                {
                    isDependencyProperty = true;
                    break;
                }
            }

            properties.Add(new ObservablePropertyInfo(
                PropertyName: property.Name,
                PropertyTypeFullName: property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                HasPublicGetter: hasPublicGetter,
                IsIndexer: isIndexer,
                IsDependencyProperty: isDependencyProperty));
        }

        return new EquatableArray<ObservablePropertyInfo>(properties.ToArray());
    }
}

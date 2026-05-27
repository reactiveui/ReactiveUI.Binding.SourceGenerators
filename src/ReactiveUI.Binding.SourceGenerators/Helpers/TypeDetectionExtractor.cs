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

        // Walk AllInterfaces (includes inherited interfaces)
        DetectImplementedInterfaces(
            typeSymbol,
            wellKnown,
            ct,
            out var implementsIReactiveObject,
            out var implementsINPC,
            out var implementsINPChanging);

        // Walk base type chain for platform detection
        var platform = DetectPlatformBaseTypes(typeSymbol, wellKnown, ct);

        // Extract properties
        var properties = ExtractProperties(typeSymbol, ct);

        return new(
            typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            typeSymbol.MetadataName,
            implementsIReactiveObject,
            implementsINPC,
            implementsINPChanging,
            platform.InheritsWpfDependencyObject,
            platform.InheritsWinUIDependencyObject,
            platform.InheritsNSObject,
            platform.InheritsWinFormsComponent,
            platform.InheritsAndroidView,
            properties);
    }

    /// <summary>
    /// Extracts the properties from a named type symbol.
    /// </summary>
    /// <param name="typeSymbol">The type symbol.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An array of observable property info.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static EquatableArray<ObservablePropertyInfo> ExtractProperties(
        INamedTypeSymbol typeSymbol,
        CancellationToken ct)
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

            properties.Add(new(
                property.Name,
                property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                hasPublicGetter,
                isIndexer,
                isDependencyProperty));
        }

        return new([.. properties]);
    }

    /// <summary>
    /// Walks <c>AllInterfaces</c> to detect notification interfaces.
    /// </summary>
    /// <param name="typeSymbol">The type to inspect.</param>
    /// <param name="wellKnown">The cached well-known symbols.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="implementsIReactiveObject">Set to true if the type implements IReactiveObject.</param>
    /// <param name="implementsINPC">Set to true if the type implements INotifyPropertyChanged.</param>
    /// <param name="implementsINPChanging">Set to true if the type implements INotifyPropertyChanging.</param>
    private static void DetectImplementedInterfaces(
        INamedTypeSymbol typeSymbol,
        SymbolHelpers.WellKnownSymbolsBox wellKnown,
        CancellationToken ct,
        out bool implementsIReactiveObject,
        out bool implementsINPC,
        out bool implementsINPChanging)
    {
        implementsIReactiveObject = false;
        implementsINPC = false;
        implementsINPChanging = false;

        var allInterfaces = typeSymbol.AllInterfaces;
        for (var i = 0; i < allInterfaces.Length; i++)
        {
            ct.ThrowIfCancellationRequested();
            var iface = allInterfaces[i];

            if (wellKnown.IReactiveObject != null &&
                SymbolEqualityComparer.Default.Equals(iface, wellKnown.IReactiveObject))
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
    }

    /// <summary>
    /// Walks the base type chain to detect platform-specific base types (WPF/WinUI/KVO/WinForms/Android).
    /// </summary>
    /// <param name="typeSymbol">The type to inspect.</param>
    /// <param name="wellKnown">The cached well-known symbols.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The detected platform base-type flags.</returns>
    private static PlatformBaseTypeFlags DetectPlatformBaseTypes(
        INamedTypeSymbol typeSymbol,
        SymbolHelpers.WellKnownSymbolsBox wellKnown,
        CancellationToken ct)
    {
        var wpf = false;
        var winui = false;
        var ns = false;
        var winforms = false;
        var android = false;

        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            ct.ThrowIfCancellationRequested();

            wpf = wpf || Matches(baseType, wellKnown.WpfDependencyObject);
            winui = winui || Matches(baseType, wellKnown.WinUIDependencyObject);
            ns = ns || Matches(baseType, wellKnown.NSObject);
            winforms = winforms || Matches(baseType, wellKnown.WinFormsComponent);
            android = android || Matches(baseType, wellKnown.AndroidView);

            baseType = baseType.BaseType;
        }

        return new(wpf, winui, ns, winforms, android);
    }

    /// <summary>
    /// Determines whether <paramref name="symbol"/> equals the (possibly null) candidate symbol.
    /// </summary>
    /// <param name="symbol">The symbol to compare.</param>
    /// <param name="candidate">The candidate symbol; null is treated as no match.</param>
    /// <returns><c>true</c> if the candidate is non-null and equal; otherwise, <c>false</c>.</returns>
    private static bool Matches(INamedTypeSymbol symbol, INamedTypeSymbol? candidate) =>
        candidate != null && SymbolEqualityComparer.Default.Equals(symbol, candidate);

    /// <summary>
    /// Platform-specific base-type detection flags for a class.
    /// </summary>
    /// <param name="InheritsWpfDependencyObject">Whether the type inherits a WPF DependencyObject.</param>
    /// <param name="InheritsWinUIDependencyObject">Whether the type inherits a WinUI DependencyObject.</param>
    /// <param name="InheritsNSObject">Whether the type inherits an Apple NSObject.</param>
    /// <param name="InheritsWinFormsComponent">Whether the type inherits a WinForms Component.</param>
    /// <param name="InheritsAndroidView">Whether the type inherits an Android View.</param>
    private readonly record struct PlatformBaseTypeFlags(
        bool InheritsWpfDependencyObject,
        bool InheritsWinUIDependencyObject,
        bool InheritsNSObject,
        bool InheritsWinFormsComponent,
        bool InheritsAndroidView);
}

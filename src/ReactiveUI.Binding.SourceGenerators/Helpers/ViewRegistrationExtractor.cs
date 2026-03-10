// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>
/// Extracts <see cref="ViewRegistrationInfo"/> from class declarations implementing <c>IViewFor&lt;T&gt;</c>.
/// </summary>
internal static class ViewRegistrationExtractor
{
    /// <summary>
    /// Extracts a <see cref="ViewRegistrationInfo"/> from a class that implements <c>IViewFor&lt;T&gt;</c>.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="ViewRegistrationInfo"/> if the class implements <c>IViewFor&lt;T&gt;</c>; otherwise, <see langword="null"/>.</returns>
    internal static ViewRegistrationInfo? ExtractFromIViewForImplementation(
        GeneratorSyntaxContext context,
        CancellationToken ct)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var typeSymbol = semanticModel.GetDeclaredSymbol(classDecl, ct) as INamedTypeSymbol;

        if (typeSymbol is null || typeSymbol.IsAbstract)
        {
            return null;
        }

        // Skip types marked with [ExcludeFromViewRegistration]
        if (HasAttribute(typeSymbol, Constants.ExcludeFromViewRegistrationAttributeMetadataName, semanticModel.Compilation))
        {
            return null;
        }

        var iViewForGeneric = semanticModel.Compilation.GetTypeByMetadataName(
            Constants.IViewForGenericMetadataName);

        // Walk AllInterfaces to find IViewFor<T>
        var allInterfaces = typeSymbol.AllInterfaces;
        for (var i = 0; i < allInterfaces.Length; i++)
        {
            ct.ThrowIfCancellationRequested();
            var iface = allInterfaces[i];

            if (iViewForGeneric is object
                && iface.IsGenericType
                && SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, iViewForGeneric)
                && iface.TypeArguments.Length == 1)
            {
                var viewModelType = iface.TypeArguments[0];
                var viewModelFqn = viewModelType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var viewFqn = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var hasParameterlessCtor = HasAccessibleParameterlessConstructor(typeSymbol);
                var contract = ExtractViewContract(typeSymbol, semanticModel.Compilation);
                var isSingleInstance = HasAttribute(typeSymbol, Constants.SingleInstanceViewAttributeMetadataName, semanticModel.Compilation);

                return new ViewRegistrationInfo(viewModelFqn, viewFqn, hasParameterlessCtor, contract, isSingleInstance);
            }
        }

        return null;
    }

    /// <summary>
    /// Checks whether the type has the specified attribute applied.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="attributeMetadataName">The metadata name of the attribute.</param>
    /// <param name="compilation">The compilation for symbol resolution.</param>
    /// <returns><see langword="true"/> if the attribute is present; otherwise, <see langword="false"/>.</returns>
    private static bool HasAttribute(INamedTypeSymbol type, string attributeMetadataName, Compilation compilation)
    {
        // Attribute types live in the same assembly as IViewFor<T>; if IViewFor<T> resolved, these will too.
        var attributeSymbol = InvalidOperationExceptionHelper.EnsureNotNull(
            compilation.GetTypeByMetadataName(attributeMetadataName),
            attributeMetadataName);

        var attributes = type.GetAttributes();
        for (var i = 0; i < attributes.Length; i++)
        {
            if (SymbolEqualityComparer.Default.Equals(attributes[i].AttributeClass, attributeSymbol))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Extracts the contract string from <c>[ViewContract("...")]</c> if present.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="compilation">The compilation for symbol resolution.</param>
    /// <returns>The contract string, or <see langword="null"/> if not present.</returns>
    private static string? ExtractViewContract(INamedTypeSymbol type, Compilation compilation)
    {
        // ViewContractAttribute lives in the same assembly as IViewFor<T>; always resolvable here.
        var attributeSymbol = InvalidOperationExceptionHelper.EnsureNotNull(
            compilation.GetTypeByMetadataName(Constants.ViewContractAttributeMetadataName),
            Constants.ViewContractAttributeMetadataName);

        var attributes = type.GetAttributes();
        for (var i = 0; i < attributes.Length; i++)
        {
            var attr = attributes[i];
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeSymbol)
                && attr.ConstructorArguments.Length == 1
                && attr.ConstructorArguments[0].Value is string contract)
            {
                return contract;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks whether the type has an accessible parameterless constructor (public or internal).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if a parameterless constructor is accessible; otherwise, <see langword="false"/>.</returns>
    private static bool HasAccessibleParameterlessConstructor(INamedTypeSymbol type)
    {
        var constructors = type.InstanceConstructors;
        for (var i = 0; i < constructors.Length; i++)
        {
            var ctor = constructors[i];
            if (ctor.Parameters.Length == 0
                && (ctor.DeclaredAccessibility == Accessibility.Public
                    || ctor.DeclaredAccessibility == Accessibility.Internal))
            {
                return true;
            }
        }

        return false;
    }
}

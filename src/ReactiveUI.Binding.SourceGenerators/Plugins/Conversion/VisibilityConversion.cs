// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Validates and emits the enum mapping shared by visibility adapters.</summary>
internal static class VisibilityConversion
{
    /// <summary>Matches the native visibility converter's score.</summary>
    private const int Affinity = 2;

    /// <summary>Builds a mapping only for the exact framework enum and its required members.</summary>
    /// <param name="source">The input type.</param>
    /// <param name="target">The output type.</param>
    /// <param name="metadataName">The platform's visibility enum.</param>
    /// <param name="hidden">The enum member representing false.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="supportsHidden">Whether the platform supports the hidden flag.</param>
    /// <param name="hintNamespace">The platform's visibility hint namespace.</param>
    /// <returns>The typed visibility mapping, or null when ineligible.</returns>
    internal static ConversionInfo? Select(
        ITypeSymbol source,
        ITypeSymbol target,
        string metadataName,
        string hidden,
        Compilation compilation,
        bool supportsHidden,
        string hintNamespace)
    {
        var forward = source.SpecialType == SpecialType.System_Boolean;
        var enumType = forward ? target : source;
        return (!forward && target.SpecialType != SpecialType.System_Boolean)
            || enumType.TypeKind != TypeKind.Enum
            || !NativeTypeIdentity.Matches(enumType, metadataName)
            || enumType.GetMembers("Visible").IsEmpty
            || enumType.GetMembers(hidden).IsEmpty
            ? null
            : Create(enumType, forward, hidden, compilation, supportsHidden, hintNamespace);
    }

    /// <summary>Applies the platform's inversion and hidden-value rules.</summary>
    /// <param name="enumType">The verified framework enum.</param>
    /// <param name="forward">Whether the input is boolean.</param>
    /// <param name="hidden">The ordinary false enum member.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="supportsHidden">Whether Hidden is supported.</param>
    /// <param name="hintNamespace">The platform's hint namespace.</param>
    /// <returns>The typed conversion.</returns>
    private static ConversionInfo Create(ITypeSymbol enumType, bool forward, string hidden, Compilation compilation, bool supportsHidden, string hintNamespace)
    {
        var typeName = enumType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var inverse = VisibilityHintExpressions.Flag(compilation, hintNamespace, "Inverse");
        var visible = forward ? "__value" : $"__value == {typeName}.Visible";
        if (inverse != "false")
        {
            visible = $"({visible}) != ({inverse})";
        }

        var notVisible = $"{typeName}.{hidden}";
        if (supportsHidden && !enumType.GetMembers("Hidden").IsEmpty)
        {
            var useHidden = VisibilityHintExpressions.Flag(compilation, hintNamespace, "UseHidden");
            notVisible = $"({useHidden}) ? {typeName}.Hidden : {notVisible}";
        }

        return new(forward ? $"({visible}) ? {typeName}.Visible : ({notVisible})" : visible, "true", Affinity);
    }
}

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Reads supported visibility flags through the hint enum present in the consumer.</summary>
internal static class VisibilityHintExpressions
{
    /// <summary>Builds an enum-specific flag test for either runtime flavour.</summary>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="hintNamespace">The platform's hint namespace, or empty when hints do not apply.</param>
    /// <param name="member">The flag member to test.</param>
    /// <returns>A typed flag test, or false when the hint contract is absent.</returns>
    internal static string Flag(Compilation compilation, string hintNamespace, string member)
    {
        if (hintNamespace.Length == 0)
        {
            return "false";
        }

        var first = ForType(compilation, $"{hintNamespace}.BooleanToVisibilityHint", member, "0");
        var reactiveNamespace = hintNamespace.Replace("ReactiveUI", "ReactiveUI.Reactive");
        var second = ForType(compilation, $"{reactiveNamespace}.BooleanToVisibilityHint", member, "1");
        if (first is null)
        {
            return second ?? "false";
        }

        return second is null ? first : $"({first}) || ({second})";
    }

    /// <summary>Tests a flag only when the exact enum and member are accessible.</summary>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="metadataName">The hint enum metadata name.</param>
    /// <param name="member">The flag member.</param>
    /// <param name="suffix">The discriminator for the emitted pattern variable.</param>
    /// <returns>The flag expression, or null.</returns>
    private static string? ForType(Compilation compilation, string metadataName, string member, string suffix)
    {
        var type = compilation.GetTypeByMetadataName(metadataName);
        if (type is not { TypeKind: TypeKind.Enum } || type.GetMembers(member).IsEmpty
            || !compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
        {
            return null;
        }

        var typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var variable = $"__{member}Hint{suffix}";
        return $"__hint is {typeName} {variable} && ({variable} & {typeName}.{member}) != 0";
    }
}

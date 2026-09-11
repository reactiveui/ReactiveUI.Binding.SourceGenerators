// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>
/// Extracts <see cref="BindToInvocationInfo"/> from <c>BindTo</c> invocations. The source is an
/// observable stream (the receiver), so only the target property path is extracted.
/// </summary>
internal static class BindToExtractor
{
    /// <summary>The minimum number of arguments a BindTo invocation must have (target, property).</summary>
    private const int MinimumBindToArgumentCount = 2;

    /// <summary>Pipeline B transform: extracts <see cref="BindToInvocationInfo"/> from a <c>BindTo</c> invocation.</summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="BindToInvocationInfo"/> POCO, or null if the invocation is not analyzable.</returns>
    internal static BindToInvocationInfo? ExtractBindToInvocation(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

        var semanticModel = context.SemanticModel;
        var methodSymbol = ExtractorValidation.ExtractMethodSymbol(semanticModel.GetSymbolInfo(invocation, ct));
        if (methodSymbol is null)
        {
            return null;
        }

        if (!ExtractorValidation.IsRecognizedExtensionClass(methodSymbol.ContainingType))
        {
            return null;
        }

        // Need at least 2 arguments: target, property.
        var args = invocation.ArgumentList.Arguments;
        if (!ExtractorValidation.HasMinimumArguments(args.Count, MinimumBindToArgumentCount))
        {
            return null;
        }

        // The source value type is the T in the receiver's IObservable<T>.
        var receiverType = semanticModel.GetTypeInfo(memberAccess.Expression, ct).Type;
        var sourceValueType = GetObservableValueType(receiverType);
        if (sourceValueType is null)
        {
            return null;
        }

        var targetPropertyArg = args[1].Expression;
        var targetPropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(targetPropertyArg, semanticModel, ct);
        var targetTypeName =
            ExtractorValidation.GetDeclarableTypeDisplayName(semanticModel.GetTypeInfo(args[0].Expression, ct).Type);

        // A target the model cannot name leaves nothing to declare a member against, generated or otherwise.
        if (targetTypeName is null)
        {
            return null;
        }

        var reflectionOnly = targetPropertyPath is null || targetPropertyPath.Length == 0;
        var written = WrittenProperty(methodSymbol, targetPropertyPath, reflectionOnly);
        if (written is null)
        {
            return null;
        }

        var sourceValueTypeFullName = sourceValueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        EquatableArray<PropertyPathSegment> targetPath = reflectionOnly ? default : new(targetPropertyPath!);

        DetectConversionParameters(methodSymbol, out var hasConversionHint, out var hasConverterOverride);

        var filePath = invocation.SyntaxTree.FilePath;
        var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        var targetExpressionText =
            CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(targetPropertyArg.ToString());

        return new(
            filePath,
            lineNumber,
            sourceValueTypeFullName,
            targetTypeName,
            targetPath,
            written.Value.TypeFullName,
            written.Value.IsReferenceType,
            hasConversionHint,
            hasConverterOverride,
            targetExpressionText,
            InterceptableLocationReader.Read(semanticModel, invocation, ct),
            reflectionOnly);
    }

    /// <summary>
    /// Returns the <c>T</c> of <c>System.IObservable&lt;T&gt;</c> for the given receiver type,
    /// checking the type itself and all implemented interfaces.
    /// </summary>
    /// <param name="receiver">The receiver type symbol.</param>
    /// <returns>The observable value type, or null if the receiver is not an observable.</returns>
    internal static ITypeSymbol? GetObservableValueType(ITypeSymbol? receiver)
    {
        if (receiver is INamedTypeSymbol direct && IsFrameworkObservable(direct))
        {
            return direct.TypeArguments[0];
        }

        // A null receiver simply has no interfaces to walk, so it falls through to the same result as one
        // that implements nothing; a separate guard for it could never be taken from the only caller.
        foreach (var iface in receiver?.AllInterfaces ?? System.Collections.Immutable.ImmutableArray<INamedTypeSymbol>.Empty)
        {
            if (IsFrameworkObservable(iface))
            {
                return iface.TypeArguments[0];
            }
        }

        return null;
    }

    /// <summary>Names the type of the property a call writes, however the call names it.</summary>
    /// <param name="methodSymbol">The method the call resolved to.</param>
    /// <param name="targetPropertyPath">The path read from the selector, where one could be read.</param>
    /// <param name="reflectionOnly">Whether the selector resolved to no path at compile time.</param>
    /// <returns>The written type, or <see langword="null"/> when nothing declarable is there.</returns>
    /// <remarks>
    /// A selector the compiler could read names the type at the end of the path. One it could not still resolved
    /// to a method, whose last type argument is that same property's type, so the call is served rather than
    /// dropped for want of a name the path would have supplied.
    /// </remarks>
    private static WrittenPropertyType? WrittenProperty(
        IMethodSymbol methodSymbol,
        PropertyPathSegment[]? targetPropertyPath,
        bool reflectionOnly)
    {
        if (!reflectionOnly)
        {
            var leaf = targetPropertyPath![targetPropertyPath.Length - 1];
            return new(leaf.PropertyTypeFullName, leaf.IsReferenceType);
        }

        var written = ExtractorValidation.DeclarableTypeArgument(methodSymbol, methodSymbol.TypeArguments.Length - 1);
        return written is null
            ? null
            : new WrittenPropertyType(
                written.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                written.IsReferenceType);
    }

    /// <summary>Determines whether a type is the framework's own <c>System.IObservable&lt;T&gt;</c>.</summary>
    /// <param name="type">The type to judge.</param>
    /// <returns><see langword="true"/> when it is that interface rather than one of the same name.</returns>
    /// <remarks>
    /// Asked of the receiver and of each interface it implements, so the shape and the namespace are described
    /// once. Both a lookalike declared elsewhere and one of the same name taking a different number of type
    /// arguments answer no. A named type always belongs to a namespace, the global one at worst, so there is
    /// none to account for; the shapes that belong to no namespace - an array, a pointer, a function pointer -
    /// are not named types and never arrive here.
    /// </remarks>
    private static bool IsFrameworkObservable(INamedTypeSymbol type) =>
        type is { Name: "IObservable", TypeArguments.Length: 1 }
        && type.ContainingNamespace.ToDisplayString() == "System";

    /// <summary>
    /// Scans the method parameters to detect the presence of a <c>conversionHint</c> parameter
    /// and an <c>IBindingTypeConverter</c>-typed <c>converterOverride</c> parameter.
    /// </summary>
    /// <param name="methodSymbol">The resolved method symbol.</param>
    /// <param name="hasConversionHint">Set to true if a <c>conversionHint</c> parameter exists.</param>
    /// <param name="hasConverterOverride">Set to true if an <c>IBindingTypeConverter</c> converter override exists.</param>
    private static void DetectConversionParameters(
        IMethodSymbol methodSymbol,
        out bool hasConversionHint,
        out bool hasConverterOverride)
    {
        hasConversionHint = false;
        hasConverterOverride = false;

        foreach (var parameter in methodSymbol.Parameters)
        {
            if (parameter.Name == "conversionHint")
            {
                hasConversionHint = true;
            }
            else if (parameter.Name == "converterOverride"
                     && parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                         .EndsWith("IBindingTypeConverter", StringComparison.Ordinal))
            {
                hasConverterOverride = true;
            }
        }
    }

    /// <summary>The type a <c>BindTo</c> call writes to.</summary>
    /// <param name="TypeFullName">The fully qualified property type.</param>
    /// <param name="IsReferenceType">Whether that type is a reference type.</param>
    private readonly record struct WrittenPropertyType(string TypeFullName, bool IsReferenceType);
}

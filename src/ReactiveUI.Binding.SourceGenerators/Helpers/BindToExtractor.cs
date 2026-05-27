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
    /// <summary>
    /// The minimum number of arguments a BindTo invocation must have (target, property).
    /// </summary>
    private const int MinimumBindToArgumentCount = 2;

    /// <summary>
    /// Pipeline B transform: extracts <see cref="BindToInvocationInfo"/> from a <c>BindTo</c> invocation.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="BindToInvocationInfo"/> POCO, or null if the invocation is not analyzable.</returns>
    internal static BindToInvocationInfo? ExtractBindToInvocation(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

        var semanticModel = context.SemanticModel;
        var methodSymbol = ExtractorValidation.ExtractMethodSymbol(semanticModel.GetSymbolInfo(invocation, ct));
        if (methodSymbol == null)
        {
            return null;
        }

        if (!ExtractorValidation.IsRecognizedExtensionClass(methodSymbol.ContainingType.Name))
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
        if (sourceValueType == null)
        {
            return null;
        }

        var targetPropertyArg = args[1].Expression;
        var targetPropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(targetPropertyArg, semanticModel, ct);
        if (targetPropertyPath == null || targetPropertyPath.Length == 0)
        {
            return null;
        }

        var targetTypeName =
            ExtractorValidation.GetTypeDisplayName(semanticModel.GetTypeInfo(args[0].Expression, ct).Type);
        if (targetTypeName == null)
        {
            return null;
        }

        var sourceValueTypeFullName = sourceValueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var targetPropertyTypeFullName = targetPropertyPath[targetPropertyPath.Length - 1].PropertyTypeFullName;

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
            new(targetPropertyPath),
            targetPropertyTypeFullName,
            hasConversionHint,
            hasConverterOverride,
            targetExpressionText);
    }

    /// <summary>
    /// Returns the <c>T</c> of <c>System.IObservable&lt;T&gt;</c> for the given receiver type,
    /// checking the type itself and all implemented interfaces.
    /// </summary>
    /// <param name="receiver">The receiver type symbol.</param>
    /// <returns>The observable value type, or null if the receiver is not an observable.</returns>
    private static ITypeSymbol? GetObservableValueType(ITypeSymbol? receiver)
    {
        if (receiver is INamedTypeSymbol { Name: "IObservable", TypeArguments.Length: 1 } direct
            && direct.ContainingNamespace?.ToDisplayString() == "System")
        {
            return direct.TypeArguments[0];
        }

        if (receiver is null)
        {
            return null;
        }

        foreach (var iface in receiver.AllInterfaces)
        {
            if (iface is { Name: "IObservable", TypeArguments.Length: 1 }
                && iface.ContainingNamespace?.ToDisplayString() == "System")
            {
                return iface.TypeArguments[0];
            }
        }

        return null;
    }

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
}

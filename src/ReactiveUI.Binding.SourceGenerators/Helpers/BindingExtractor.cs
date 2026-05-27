// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>
/// Extracts BindingInvocationInfo from BindOneWay, BindTwoWay, OneWayBind, and Bind invocations.
/// </summary>
internal static class BindingExtractor
{
    /// <summary>
    /// The minimum number of arguments a binding invocation must have
    /// (source/view, target/view model, source property, target property).
    /// </summary>
    private const int MinimumBindingArgumentCount = 3;

    /// <summary>
    /// Pipeline B transform: extracts BindingInvocationInfo from a BindOneWay/BindTwoWay/OneWayBind/Bind invocation.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A BindingInvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static BindingInvocationInfo? ExtractBindInvocation(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

        var semanticModel = context.SemanticModel;
        var methodSymbol = ExtractorValidation.ExtractMethodSymbol(semanticModel.GetSymbolInfo(invocation, ct));
        if (methodSymbol == null)
        {
            return null;
        }

        // Verify this is our stub or generated method
        if (!ExtractorValidation.IsRecognizedExtensionClass(methodSymbol.ContainingType.Name))
        {
            return null;
        }

        var methodName = memberAccess.Name.Identifier.Text;
        var isTwoWay = methodName is Constants.BindTwoWayMethodName or Constants.BindMethodName;

        // Need at least 3 arguments: source/view (this), target/viewModel, sourceProp/vmProp, targetProp/viewProp
        var args = invocation.ArgumentList.Arguments;
        InvalidOperationExceptionHelper.EnsureMinimumArguments(args.Count, MinimumBindingArgumentCount);

        // Extract property paths
        var sourcePropertyArg = args[1].Expression;
        var targetPropertyArg = args[2].Expression;

        var sourcePropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(sourcePropertyArg, semanticModel, ct);
        var targetPropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(targetPropertyArg, semanticModel, ct);

        if (sourcePropertyPath == null || targetPropertyPath == null)
        {
            return null;
        }

        // Get types
        var receiverTypeName = InvalidOperationExceptionHelper.EnsureNotNull(
            ExtractorValidation.GetTypeDisplayName(semanticModel.GetTypeInfo(memberAccess.Expression, ct).Type),
            "receiver type display name");

        var firstArgTypeName = InvalidOperationExceptionHelper.EnsureNotNull(
            ExtractorValidation.GetTypeDisplayName(semanticModel.GetTypeInfo(args[0].Expression, ct).Type),
            "first argument type display name");

        var sourcePropertyTypeFullName = sourcePropertyPath[sourcePropertyPath.Length - 1].PropertyTypeFullName;
        var targetPropertyTypeFullName = targetPropertyPath[targetPropertyPath.Length - 1].PropertyTypeFullName;

        DetectBindingParameters(
            methodSymbol,
            out var hasConversion,
            out var hasScheduler,
            out var hasConverterOverride);

        var filePath = invocation.SyntaxTree.FilePath;
        var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        var sourceExpressionText =
            CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(sourcePropertyArg.ToString());
        var targetExpressionText =
            CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(targetPropertyArg.ToString());

        var isViewFirst = methodName is Constants.OneWayBindMethodName or Constants.BindMethodName;
        var sourceTypeFullName = isViewFirst ? firstArgTypeName : receiverTypeName;
        var targetTypeFullName = isViewFirst ? receiverTypeName : firstArgTypeName;

        return new(
            filePath,
            lineNumber,
            sourceTypeFullName,
            new(sourcePropertyPath),
            targetTypeFullName,
            new(targetPropertyPath),
            sourcePropertyTypeFullName,
            targetPropertyTypeFullName,
            hasConversion,
            hasScheduler,
            isTwoWay,
            methodName,
            sourceExpressionText,
            targetExpressionText,
            hasConverterOverride);
    }

    /// <summary>
    /// Scans the method parameters to detect conversion, scheduler, and converter-override
    /// capabilities of the binding overload.
    /// </summary>
    /// <param name="methodSymbol">The resolved method symbol.</param>
    /// <param name="hasConversion">Set to true if a conversion/selector parameter exists.</param>
    /// <param name="hasScheduler">Set to true if a <c>scheduler</c> parameter exists.</param>
    /// <param name="hasConverterOverride">Set to true if an <c>IBindingTypeConverter</c> converter override exists.</param>
    private static void DetectBindingParameters(
        IMethodSymbol methodSymbol,
        out bool hasConversion,
        out bool hasScheduler,
        out bool hasConverterOverride)
    {
        hasConversion = false;
        hasScheduler = false;
        hasConverterOverride = false;

        foreach (var parameter in methodSymbol.Parameters)
        {
            if (parameter.Name is "conversionFunc" or "sourceToTargetConv" or "selector" or "vmToViewConverter")
            {
                hasConversion = true;
            }

            if (SymbolHelpers.DetectHasConverterOverride(parameter))
            {
                hasConverterOverride = true;
            }

            if (parameter.Name == "scheduler")
            {
                hasScheduler = true;
            }
        }
    }
}

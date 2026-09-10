// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>
/// Extracts <see cref="InvokeCommandInvocationInfo"/> from <c>InvokeCommand</c> invocations. The values come
/// from the receiver, so the only path extracted is the one reaching the command.
/// </summary>
internal static class InvokeCommandExtractor
{
    /// <summary>The minimum number of arguments this overload carries (target, command property).</summary>
    private const int MinimumInvokeCommandArgumentCount = 2;

    /// <summary>Pipeline B transform: extracts <see cref="InvokeCommandInvocationInfo"/> from an <c>InvokeCommand</c> invocation.</summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An <see cref="InvokeCommandInvocationInfo"/> POCO, or null if the invocation is not analyzable.</returns>
    /// <remarks>
    /// The overload taking the command itself has nothing to resolve and nothing to observe, so it carries no
    /// selector and is declined here: it falls short of the argument count, and its only argument is not a
    /// lambda a path could be read from.
    /// </remarks>
    internal static InvokeCommandInvocationInfo? ExtractInvokeCommandInvocation(
        GeneratorSyntaxContext context,
        CancellationToken ct)
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

        var args = invocation.ArgumentList.Arguments;
        if (!ExtractorValidation.HasMinimumArguments(args.Count, MinimumInvokeCommandArgumentCount))
        {
            return null;
        }

        // The values offered as the command parameter are the T of the receiver's IObservable<T>.
        var sourceValueType = BindToExtractor.GetObservableValueType(
            semanticModel.GetTypeInfo(memberAccess.Expression, ct).Type);
        if (sourceValueType is null)
        {
            return null;
        }

        var commandArg = args[1].Expression;
        var commandPropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(commandArg, semanticModel, ct);
        var targetTypeName =
            ExtractorValidation.GetDeclarableTypeDisplayName(semanticModel.GetTypeInfo(args[0].Expression, ct).Type);

        // One guard for both: a target the model could not name is as unusable as a path it could not read.
        return commandPropertyPath is null || commandPropertyPath.Length == 0 || targetTypeName is null
            ? null
            : new InvokeCommandInvocationInfo(
                invocation.SyntaxTree.FilePath,
                invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                sourceValueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                targetTypeName,
                new(commandPropertyPath),
                CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(commandArg.ToString()),
                InterceptableLocationReader.Read(semanticModel, invocation, ct));
    }
}

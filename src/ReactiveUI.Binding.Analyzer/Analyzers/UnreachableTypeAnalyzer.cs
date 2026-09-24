// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ReactiveUI.Binding.Helpers;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>
/// Reports binding calls that name a type generated code cannot reach (RXUIBIND015): a private or protected nested
/// type, or a generic closed over one, in the call's signature or along an observed path.
/// </summary>
/// <remarks>
/// Generated overloads and interceptors are declared in a class of their own, so the generator declines such a call
/// and it stays on the runtime stub, which throws when it runs. The checks mirror the generator's: the closed
/// signature of the resolved method, then each link of every selector lambda.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnreachableTypeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostics this analyzer reports.</summary>
    private static readonly ImmutableArray<DiagnosticDescriptor> ReportedDiagnostics =
        new[] { DiagnosticWarnings.UnreachableType }.ToImmutableArray();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ReportedDiagnostics;

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        ArgumentExceptionHelper.ThrowIfNull(context);
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(static operationContext => AnalyzeInvocation(in operationContext), OperationKind.Invocation);
    }

    /// <summary>Reports a binding call that names a type generated code cannot reach.</summary>
    /// <param name="context">The operation analysis context.</param>
    internal static void AnalyzeInvocation(in OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;

        if (!AnalyzerHelpers.IsBindingExtensionMethod(method) || AnalyzerHelpers.IsUnsafeBindingMethod(method))
        {
            return;
        }

        var unreachable = FindUnreachableInSignature(method, context.Compilation)
            ?? FindUnreachableInPaths(invocation.Arguments, context);
        if (unreachable is null)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticWarnings.UnreachableType,
            invocation.Syntax.GetLocation(),
            unreachable.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
    }

    /// <summary>Finds the first type in a resolved method's closed signature that generated code cannot name.</summary>
    /// <param name="method">The resolved binding method.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The unreachable type, or null when every type is reachable.</returns>
    internal static ITypeSymbol? FindUnreachableInSignature(IMethodSymbol method, Compilation compilation)
    {
        var typeArguments = method.TypeArguments;
        for (var i = 0; i < typeArguments.Length; i++)
        {
            if (!IsReachable(typeArguments[i], compilation))
            {
                return typeArguments[i];
            }
        }

        var parameters = method.Parameters;
        for (var i = 0; i < parameters.Length; i++)
        {
            if (!IsReachable(parameters[i].Type, compilation))
            {
                return parameters[i].Type;
            }
        }

        return method.ReturnsVoid || IsReachable(method.ReturnType, compilation) ? null : method.ReturnType;
    }

    /// <summary>Finds the first link of a selector lambda whose owner or value type generated code cannot name.</summary>
    /// <param name="arguments">The invocation arguments.</param>
    /// <param name="context">The operation analysis context.</param>
    /// <returns>The unreachable type, or null when every link is reachable.</returns>
    internal static ITypeSymbol? FindUnreachableInPaths(ImmutableArray<IArgumentOperation> arguments, in OperationAnalysisContext context)
    {
        var semanticModel = context.Operation.SemanticModel!;
        for (var i = 0; i < arguments.Length; i++)
        {
            if (arguments[i].Value.Syntax is not LambdaExpressionSyntax { ExpressionBody: { } body })
            {
                continue;
            }

            var current = AnalyzerHelpers.SkipNullForgivingAndParentheses(body);
            while (current is MemberAccessExpressionSyntax memberAccess)
            {
                var owner = semanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;
                if (owner is not null && !IsReachable(owner, context.Compilation))
                {
                    return owner;
                }

                if (semanticModel.GetSymbolInfo(memberAccess, context.CancellationToken).Symbol is IPropertySymbol property
                    && !IsReachable(property.Type, context.Compilation))
                {
                    return property.Type;
                }

                current = AnalyzerHelpers.SkipNullForgivingAndParentheses(memberAccess.Expression);
            }
        }

        return null;
    }

    /// <summary>Determines whether generated code in the consumer's assembly can name a type.</summary>
    /// <param name="type">The type.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns><see langword="true"/> when the type is accessible from outside every type that declares it.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsReachable(ITypeSymbol type, Compilation compilation) =>
        compilation.IsSymbolAccessibleWithin(type, compilation.Assembly);
}

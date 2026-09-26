// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ReactiveUI.Binding.Helpers;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>
/// Reports RXUIBIND021 on a binding call that has no generated binding, so the runtime method it names throws when it
/// runs.
/// </summary>
/// <remarks>
/// <para>
/// Analyzers read the compilation with every generator's output in it, so a call that the generator bound either resolves
/// to a generated overload or is claimed by a generated interceptor. A call that still resolves to the runtime stub, and
/// that no interceptor claims, got nothing. That covers every cause at once, including a member another source generator
/// adds, which the generator cannot see.
/// </para>
/// <para>
/// Interceptors exist only on Roslyn 4.13 and newer, so the analyzer built against an older Roslyn has no interceptor to
/// ask about. The generator that loads beside it cannot write one either, so the stub is always the whole answer there.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NoGeneratedBindingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostics this analyzer reports.</summary>
    private static readonly ImmutableArray<DiagnosticDescriptor> ReportedDiagnostics =
        new[] { DiagnosticWarnings.NoGeneratedBinding }.ToImmutableArray();

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

    /// <summary>Reports a call to a runtime stub that no generated binding replaces.</summary>
    /// <param name="context">The operation analysis context.</param>
    internal static void AnalyzeInvocation(in OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;
        if (!AnalyzerHelpers.GeneratedApiNames.Contains(method.Name)
            || !IsRuntimeStub(method.ContainingType)
            || !Throws(method)
            || IsIntercepted(invocation, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticWarnings.NoGeneratedBinding,
            invocation.Syntax.GetLocation(),
            method.Name));
    }

    /// <summary>Determines whether a method is declared by a runtime stub class, whose binding methods throw.</summary>
    /// <param name="containingType">The type declaring the method.</param>
    /// <returns><see langword="true"/> for the stub classes, directly or through an extension block's grouping type.</returns>
    /// <remarks>
    /// An extension block declares its members in a grouping type nested in the static class. Its name is empty when read
    /// from source and starts with <c>&lt;</c> when read from metadata.
    /// </remarks>
    internal static bool IsRuntimeStub(INamedTypeSymbol containingType) =>
        IsStubName(containingType.Name)
        || ((containingType.Name.Length == 0 || containingType.Name[0] == '<') && IsStubName(containingType.ContainingType!.Name));

    /// <summary>Determines whether a stub class's method throws, rather than doing the work itself.</summary>
    /// <param name="method">The method.</param>
    /// <returns><see langword="true"/> for <c>ToProperty</c> and for every method that takes an expression tree.</returns>
    /// <remarks>
    /// Every method that takes an expression tree throws, and so does every <c>ToProperty</c> overload, which names its
    /// property by a delegate or a string. <c>InvokeCommand</c> with a command object runs the command itself.
    /// </remarks>
    private static bool Throws(IMethodSymbol method)
    {
        if (method.Name == Constants.ToPropertyMethodName)
        {
            return true;
        }

        foreach (var parameter in method.Parameters)
        {
            if (parameter.Type is INamedTypeSymbol { Name: "Expression", Arity: 1 })
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a type name is one of the runtime stub classes.</summary>
    /// <param name="name">The type name.</param>
    /// <returns><see langword="true"/> for the stub classes.</returns>
    private static bool IsStubName(string name) =>
        name is Constants.StubExtensionClassName or Constants.SchedulerExtensionClassName;

    /// <summary>Determines whether a generated interceptor claims the call.</summary>
    /// <param name="invocation">The call.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when an interceptor replaces the call.</returns>
    private static bool IsIntercepted(IInvocationOperation invocation, CancellationToken cancellationToken)
    {
#if ROSLYN_4_13
        // A binding method is only ever called through an invocation expression, and an operation handed to an
        // analyzer always carries the model it was bound with.
        return Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetInterceptorMethod(
            invocation.SemanticModel!,
            (Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax)invocation.Syntax,
            cancellationToken) is not null;
#else

        // The baseline compiler has no interceptors, so every call that resolves to the stub runs it.
        _ = invocation;
        _ = cancellationToken;
        return false;
#endif
    }
}

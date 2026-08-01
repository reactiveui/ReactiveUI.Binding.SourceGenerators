// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ReactiveUI.Binding.Helpers;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>
/// Reports binding calls that the generated dispatch cannot reach, so the fallback to runtime observation is a
/// build warning rather than something noticed in a profiler.
/// </summary>
/// <remarks>
/// Compile-time dispatch is chosen by extension-method lookup, which only reaches an overload declared in a
/// namespace enclosing the call site. Before C# 10 there is no global using to widen that, and an assembly
/// exposing its internals cannot leave the overloads in the shared namespace without risking an ambiguous call
/// against the assembly it exposes them to. That combination - old language version, exposed internals, a file
/// outside the root namespace - is the one case where a call silently gets the runtime path instead.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DispatchReachAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The analyzer config key a build exposes the root namespace under.</summary>
    private const string RootNamespaceKey = "build_property.RootNamespace";

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticWarnings.DispatchOutOfReach);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        ArgumentExceptionHelper.ThrowIfNull(context);
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // The three conditions are properties of the whole compilation, so settle them once and register
        // nothing at all when they do not hold - which is the overwhelmingly common case.
        context.RegisterCompilationStartAction(static startContext =>
        {
            if (!AppliesTo(startContext.Compilation, startContext.Options, out var rootNamespace))
            {
                return;
            }

            startContext.RegisterOperationAction(
                operationContext => AnalyzeInvocation(in operationContext, rootNamespace),
                OperationKind.Invocation);
        });
    }

    /// <summary>Reports a binding call in a file the generated dispatch will not be found from.</summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="rootNamespace">The root namespace the dispatch is emitted into.</param>
    internal static void AnalyzeInvocation(in OperationAnalysisContext context, string rootNamespace)
    {
        var invocation = (IInvocationOperation)context.Operation;
        if (!AnalyzerHelpers.IsBindingExtensionMethod(invocation.TargetMethod))
        {
            return;
        }

        // Read the language version from the tree rather than the compilation: it is a parse option, so a
        // compilation can hold trees that differ, and the reach of a generated overload follows the file.
        if (invocation.Syntax.SyntaxTree.Options is not CSharpParseOptions { LanguageVersion: < LanguageVersion.CSharp10 })
        {
            return;
        }

        var containingNamespace = context.ContainingSymbol.ContainingNamespace;
        var callSiteNamespace = containingNamespace?.IsGlobalNamespace != false
            ? string.Empty
            : containingNamespace.ToDisplayString();

        if (IsWithin(callSiteNamespace, rootNamespace))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticWarnings.DispatchOutOfReach,
            invocation.Syntax.GetLocation(),
            rootNamespace,
            callSiteNamespace.Length == 0 ? "<global namespace>" : callSiteNamespace));
    }

    /// <summary>Determines whether a namespace is the root namespace or nested inside it.</summary>
    /// <param name="callSiteNamespace">The namespace the call site is declared in.</param>
    /// <param name="rootNamespace">The root namespace.</param>
    /// <returns><see langword="true"/> when the call site can reach the root namespace by lookup.</returns>
    private static bool IsWithin(string callSiteNamespace, string rootNamespace) =>
        string.Equals(callSiteNamespace, rootNamespace, StringComparison.Ordinal)
        || (callSiteNamespace.Length > rootNamespace.Length
            && callSiteNamespace[rootNamespace.Length] == '.'
            && callSiteNamespace.StartsWith(rootNamespace, StringComparison.Ordinal));

    /// <summary>Determines whether this compilation is the one where dispatch can fall out of reach.</summary>
    /// <param name="compilation">The compilation being analyzed.</param>
    /// <param name="options">The analyzer options, which carry the root namespace.</param>
    /// <param name="rootNamespace">The root namespace, when one applies.</param>
    /// <returns><see langword="true"/> when every condition for the gap holds.</returns>
    private static bool AppliesTo(Compilation compilation, AnalyzerOptions options, out string rootNamespace)
    {
        rootNamespace = string.Empty;

        if (!GrantsInternalsVisibleTo(compilation))
        {
            return false;
        }

        if (!options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(RootNamespaceKey, out var value)
            || string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        rootNamespace = value;
        return true;
    }

    /// <summary>Determines whether the compilation exposes its internals to another assembly.</summary>
    /// <param name="compilation">The compilation to inspect.</param>
    /// <returns><see langword="true"/> when the assembly grants <c>InternalsVisibleTo</c> to anyone.</returns>
    private static bool GrantsInternalsVisibleTo(Compilation compilation)
    {
        foreach (var attribute in compilation.Assembly.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass is not null
                && string.Equals(
                    attributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    Constants.InternalsVisibleToAttributeFullName,
                    StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}

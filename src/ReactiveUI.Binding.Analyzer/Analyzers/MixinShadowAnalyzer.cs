// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ReactiveUI.Binding.Helpers;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>
/// Reports a binding call that reached ReactiveUI's own mixin instead of a generated overload, so losing
/// compile-time binding is a build warning rather than something found in a profiler.
/// </summary>
/// <remarks>
/// Which method the call reaches is decided by extension-method lookup. A file that imports ReactiveUI and
/// not this package binds to ReactiveUI's mixin, and every extractor recognises a call by its declaring type,
/// so that call is simply not one of ours: no dispatch is generated and none of the other diagnostics have
/// anything to say about it. Two ordinary things put a file in that state - importing ReactiveUI for
/// <c>ReactiveCommand</c> alongside this package, and an editor's import cleanup removing the binding import
/// once calls resolve to the generated overloads that made it look unused.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MixinShadowAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The namespace ReactiveUI declares its own observation and binding mixins in.</summary>
    private const string ReactiveUiNamespace = "ReactiveUI";

    /// <summary>The stub class the lean runtime package declares, used to detect that it is referenced.</summary>
    private const string StubMetadataName =
        $"{Constants.SharedGeneratedNamespace}.{Constants.StubExtensionClassName}";

    /// <summary>The same stub class in the System.Reactive flavour of the runtime package.</summary>
    private const string ReactiveStubMetadataName =
        $"{Constants.ReactiveRuntimeNamespace}.{Constants.StubExtensionClassName}";

    /// <summary>The API names this package generates bindings for.</summary>
    private static readonly ImmutableHashSet<string> GeneratedApiNames = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        Constants.WhenChangedMethodName,
        Constants.WhenChangingMethodName,
        Constants.WhenAnyMethodName,
        Constants.WhenAnyValueMethodName,
        Constants.WhenAnyObservableMethodName,
        Constants.BindOneWayMethodName,
        Constants.BindTwoWayMethodName,
        Constants.OneWayBindMethodName,
        Constants.BindMethodName,
        Constants.BindToMethodName,
        Constants.BindCommandMethodName,
        Constants.BindInteractionMethodName);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticWarnings.MixinShadowsGeneratedBinding);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        ArgumentExceptionHelper.ThrowIfNull(context);
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Only a compilation that references this package can have lost a binding to ReactiveUI's mixin, and
        // that is a property of the whole compilation, so settle it once rather than per call site.
        context.RegisterCompilationStartAction(static startContext =>
        {
            if (!ReferencesBindingPackage(startContext.Compilation))
            {
                return;
            }

            startContext.RegisterOperationAction(
                static operationContext => AnalyzeInvocation(in operationContext),
                OperationKind.Invocation);
        });
    }

    /// <summary>Reports a call that ReactiveUI's mixin answered in place of a generated overload.</summary>
    /// <param name="context">The operation analysis context.</param>
    internal static void AnalyzeInvocation(in OperationAnalysisContext context)
    {
        var method = ((IInvocationOperation)context.Operation).TargetMethod;

        if (!GeneratedApiNames.Contains(method.Name) || AnalyzerHelpers.IsBindingExtensionMethod(method))
        {
            return;
        }

        // An extension declared in an extension block belongs to a synthesized type nested in the class the
        // consumer wrote, so the namespace is only correct once the walk reaches the outermost type.
        var declaringType = OutermostContainingType(method.ContainingType);
        if (!IsReactiveUiNamespace(declaringType.ContainingNamespace))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticWarnings.MixinShadowsGeneratedBinding,
            context.Operation.Syntax.GetLocation(),
            method.Name,
            declaringType.Name));
    }

    /// <summary>Walks out of any nested or synthesized type to the type the consumer wrote.</summary>
    /// <param name="type">The type the invoked method belongs to.</param>
    /// <returns>The outermost containing type.</returns>
    private static INamedTypeSymbol OutermostContainingType(INamedTypeSymbol type)
    {
        while (type.ContainingType is not null)
        {
            type = type.ContainingType;
        }

        return type;
    }

    /// <summary>Determines whether a namespace is ReactiveUI's own, rather than one nested under it.</summary>
    /// <param name="namespaceSymbol">The namespace the declaring type sits in.</param>
    /// <returns><see langword="true"/> for exactly <c>ReactiveUI</c>.</returns>
    /// <remarks>
    /// Nested namespaces are excluded deliberately: this package declares its surface under
    /// <c>ReactiveUI.Binding</c>, which is the case this diagnostic exists to tell apart.
    /// </remarks>
    private static bool IsReactiveUiNamespace(INamespaceSymbol namespaceSymbol) =>
        !namespaceSymbol.IsGlobalNamespace
        && string.Equals(namespaceSymbol.Name, ReactiveUiNamespace, StringComparison.Ordinal)
        && namespaceSymbol.ContainingNamespace.IsGlobalNamespace;

    /// <summary>Determines whether the compilation references this package at all.</summary>
    /// <param name="compilation">The compilation being analyzed.</param>
    /// <returns><see langword="true"/> when either runtime flavour's stub class is in reach.</returns>
    private static bool ReferencesBindingPackage(Compilation compilation) =>
        compilation.GetTypeByMetadataName(StubMetadataName) is not null
        || compilation.GetTypeByMetadataName(ReactiveStubMetadataName) is not null;
}

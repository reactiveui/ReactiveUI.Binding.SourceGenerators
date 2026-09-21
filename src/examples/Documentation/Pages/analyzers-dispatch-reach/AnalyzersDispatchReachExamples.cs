// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.Analyzer.Analyzers;

namespace ReactiveUI.Binding.Documentation.AnalyzersDispatchReach;

/// <summary>
/// Shows the two build-time diagnostics about reaching the generated code: RXUIBIND009, a warning for a call in a
/// file the generated dispatch cannot be found from, and RXUIBIND100, an error for a compiler that is too old to run
/// the generator.
/// </summary>
public static class AnalyzersDispatchReachExamples
{
    /// <summary>The id of the diagnostic for a call the generated dispatch cannot reach.</summary>
    private const string DispatchOutOfReachId = "RXUIBIND009";

    /// <summary>The root namespace of the to-do project.</summary>
    private const string TodoRootNamespace = "TodoApp";

    /// <summary>The compiler version the package needs at least.</summary>
    private const string OldestSupportedCompiler = "roslyn4.8";

    /// <summary>A compiler older than the package supports.</summary>
    private const string TooOldCompiler = "roslyn4.7";

    /// <summary>The namespace the generator puts interceptors in.</summary>
    private const string InterceptorNamespace = "ReactiveUI.Binding.Generated.Interceptors";

    /// <summary>The name of the compiler feature that lists the namespaces allowed to intercept calls.</summary>
    private const string InterceptorsNamespacesFeature = "InterceptorsNamespaces";

    /// <summary>A report on the remaining to-do items, in a namespace outside the root namespace of a project that exposes its internals.</summary>
    private const string ReportOutsideRootSource = """
        using System;
        using System.Runtime.CompilerServices;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Todo;

        [assembly: InternalsVisibleTo("TodoApp.Tests")]

        namespace TodoReports
        {
            public static class RemainingReport
            {
                public static IObservable<int> Watch(TodoListViewModel viewModel)
                {
                    return viewModel.WhenChanged(x => x.RemainingCount);
                }
            }
        }
        """;

    /// <summary>The same report, in a namespace under the root namespace.</summary>
    private const string ReportUnderRootSource = """
        using System;
        using System.Runtime.CompilerServices;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Todo;

        [assembly: InternalsVisibleTo("TodoApp.Tests")]

        namespace TodoApp.Reports
        {
            public static class RemainingReport
            {
                public static IObservable<int> Watch(TodoListViewModel viewModel)
                {
                    return viewModel.WhenChanged(x => x.RemainingCount);
                }
            }
        }
        """;

    /// <summary>The expected message of the RXUIBIND009 warning.</summary>
    private const string OutOfReachMessage =
        "This binding throws rather than reaching its generated code. The assembly exposes its internals, so on C# 9 and below "
        + "the generated dispatch has to live in the root namespace 'TodoApp' to stay unambiguous, and this file's namespace "
        + "'TodoReports' is not under it. Move the file under the root namespace, raise the language version to 10 or later, "
        + "or name the Unsafe overload to resolve the expression at run time.";

    /// <summary>The expected message of the RXUIBIND100 error for a build with Roslyn 4.7.</summary>
    private const string CompilerTooOldMessage =
        "ReactiveUI.Binding's source generator requires Roslyn 4.8 or later (Visual Studio 2022 17.8+, or .NET SDK 8.0.100+), "
        + "but this project is building with Roslyn 4.7. Upgrade the build tools to get compile-time bindings.";

    /// <summary>Reports a call in a file outside the root namespace, RXUIBIND009, for a project on C# 7.3 that exposes its internals.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReportCallOutsideRootNamespace()
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.CSharp7_3);

        var diagnostics = await SourceAnalysis.AnalyzeAsync(ReportOutsideRootSource, new DispatchReachAnalyzer(), parseOptions, TodoRootNamespace);
        var diagnostic = SourceAnalysis.Single(diagnostics);

        SampleCheck.Equal(DispatchOutOfReachId, diagnostic.Id);
        SampleCheck.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        SampleCheck.Equal("viewModel.WhenChanged(x => x.RemainingCount)", SourceAnalysis.FlaggedTextOf(diagnostic));
        SampleCheck.Equal(OutOfReachMessage, SourceAnalysis.MessageOf(diagnostic));
    }

    /// <summary>Accepts the same call once the file is under the root namespace, which is the first fix for RXUIBIND009.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task AcceptCallUnderRootNamespace()
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.CSharp7_3);

        var diagnostics = await SourceAnalysis.AnalyzeAsync(ReportUnderRootSource, new DispatchReachAnalyzer(), parseOptions, TodoRootNamespace);

        SampleCheck.Equal(0, diagnostics.Length);
    }

    /// <summary>Accepts the call in the outside file once the project compiles as C# 10, which is the second fix for RXUIBIND009.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task AcceptCallOutsideRootNamespaceOnCSharp10()
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.CSharp10);

        var diagnostics = await SourceAnalysis.AnalyzeAsync(ReportOutsideRootSource, new DispatchReachAnalyzer(), parseOptions, TodoRootNamespace);

        SampleCheck.Equal(0, diagnostics.Length);
    }

    /// <summary>
    /// Accepts the call in the outside file when the build lets the generator intercept calls, which is the third fix for
    /// RXUIBIND009. The package's build files list the interceptor namespace for a compiler that supports interception.
    /// </summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task AcceptCallOutsideRootNamespaceWithInterceptors()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp7_3)
            .WithFeatures([new KeyValuePair<string, string>(InterceptorsNamespacesFeature, InterceptorNamespace)]);

        var diagnostics = await SourceAnalysis.AnalyzeAsync(ReportOutsideRootSource, new DispatchReachAnalyzer(), parseOptions, TodoRootNamespace);

        SampleCheck.Equal(0, diagnostics.Length);
    }

    /// <summary>Reports a build whose compiler is older than Roslyn 4.8 with the RXUIBIND100 error.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReportCompilerOlderThanRoslyn48()
    {
        var message = await BuildProbe.RunCompilerCheckAsync(TooOldCompiler);

        SampleCheck.Equal(CompilerTooOldMessage, message);
    }

    /// <summary>Accepts a build whose compiler is Roslyn 4.8, the oldest one the package supports.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task AcceptCompilerRoslyn48()
    {
        var message = await BuildProbe.RunCompilerCheckAsync(OldestSupportedCompiler);

        SampleCheck.Equal(string.Empty, message);
    }
}

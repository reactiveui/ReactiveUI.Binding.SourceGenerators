// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReactiveUI.Binding.Documentation.AnalyzersNotifications;

/// <summary>
/// Compiles a piece of source text and runs the analyzers the package ships over it, which is how a build reports
/// the diagnostics. The source can use every type in the running program, including the Common example apps.
/// </summary>
public static class SourceAnalysis
{
    /// <summary>The compiler references: every assembly the running program can load.</summary>
    private static readonly ImmutableArray<MetadataReference> References = LoadReferences();

    /// <summary>Compiles source text and runs an analyzer over it.</summary>
    /// <param name="source">The C# source to compile.</param>
    /// <param name="analyzer">The analyzer to run.</param>
    /// <returns>The diagnostics the analyzer reports, in source order.</returns>
    /// <exception cref="InvalidOperationException">The source does not compile.</exception>
    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source, DiagnosticAnalyzer analyzer)
    {
        var compilation = CSharpCompilation.Create(
            "ExampleApp",
            [CSharpSyntaxTree.ParseText(source)],
            References,
            new(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        foreach (var problem in compilation.GetDiagnostics())
        {
            if (problem.Severity == DiagnosticSeverity.Error)
            {
                throw new InvalidOperationException($"The example source does not compile: {problem}");
            }
        }

        var reported = await compilation.WithAnalyzers([analyzer]).GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
        return reported.Sort(static (left, right) => left.Location.SourceSpan.Start.CompareTo(right.Location.SourceSpan.Start));
    }

    /// <summary>Returns the one diagnostic in a result.</summary>
    /// <param name="diagnostics">The diagnostics an analysis reported.</param>
    /// <returns>The only diagnostic.</returns>
    /// <exception cref="InvalidOperationException">The analysis did not report exactly one diagnostic.</exception>
    public static Diagnostic Single(ImmutableArray<Diagnostic> diagnostics)
    {
        if (diagnostics.Length != 1)
        {
            throw new InvalidOperationException($"Expected one diagnostic, received {diagnostics.Length}.");
        }

        return diagnostics[0];
    }

    /// <summary>Reads the message a build prints for a diagnostic.</summary>
    /// <param name="diagnostic">The diagnostic.</param>
    /// <returns>The message text.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string MessageOf(Diagnostic diagnostic) => diagnostic.GetMessage(CultureInfo.InvariantCulture);

    /// <summary>Reads the source text a diagnostic points at.</summary>
    /// <param name="diagnostic">The diagnostic.</param>
    /// <returns>The text the diagnostic underlines.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FlaggedTextOf(Diagnostic diagnostic) =>
        diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan);

    /// <summary>Loads a reference to every assembly the running program can load.</summary>
    /// <returns>The references.</returns>
    private static ImmutableArray<MetadataReference> LoadReferences()
    {
        var platformAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        var builder = ImmutableArray.CreateBuilder<MetadataReference>();

        foreach (var path in platformAssemblies.Split(Path.PathSeparator))
        {
            builder.Add(MetadataReference.CreateFromFile(path));
        }

        return builder.ToImmutable();
    }
}

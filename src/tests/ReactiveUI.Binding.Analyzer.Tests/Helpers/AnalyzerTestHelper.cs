// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReactiveUI.Binding.Analyzer.Tests.Helpers;

/// <summary>
/// Helper for testing Roslyn analyzers.
/// </summary>
public static class AnalyzerTestHelper
{
    /// <summary>
    /// Runs an analyzer on the provided source code and returns diagnostics.
    /// </summary>
    /// <typeparam name="TAnalyzer">The type of analyzer to run.</typeparam>
    /// <param name="source">The source code to analyze.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the diagnostics.</returns>
    public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync<TAnalyzer>(string source)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var compilation = CreateCompilation(source);
        var analyzer = new TAnalyzer();

        var compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

        var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        // Filter to only analyzer diagnostics (exclude compiler errors)
        return diagnostics
            .Where(d => analyzer.SupportedDiagnostics.Any(sd => sd.Id == d.Id))
            .ToImmutableArray();
    }

    /// <summary>
    /// Creates a CSharpCompilation from source code with necessary references.
    /// </summary>
    internal static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var addedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var references = new List<MetadataReference>();

        void AddReference(Assembly assembly)
        {
            if (!assembly.IsDynamic && addedPaths.Add(assembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        // Add core framework references via typeof to ensure assemblies are loaded
        AddReference(typeof(object).Assembly);
        AddReference(typeof(Enumerable).Assembly);
        AddReference(typeof(System.Attribute).Assembly);
        AddReference(typeof(Expression<>).Assembly);
        AddReference(typeof(System.ComponentModel.INotifyPropertyChanged).Assembly);
        AddReference(typeof(System.ComponentModel.INotifyPropertyChanging).Assembly);
        AddReference(typeof(System.ComponentModel.INotifyDataErrorInfo).Assembly);

        // Add ReactiveUI reference
        AddReference(typeof(ReactiveUI.IReactiveObject).Assembly);

        // Add runtime assemblies by name for any that typeof didn't cover
        var assemblyNames = new[]
        {
            "System.Runtime",
            "System.ComponentModel.Primitives",
            "System.ObjectModel",
            "System.Collections",
        };

        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var name in assemblyNames)
        {
            var asm = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == name);
            if (asm != null)
            {
                AddReference(asm);
            }
        }

        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));
    }
}

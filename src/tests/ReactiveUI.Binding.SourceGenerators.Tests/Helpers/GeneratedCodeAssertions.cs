// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>
/// TUnit assertion extension methods for <see cref="GeneratorTestResult"/>.
/// </summary>
internal static class GeneratedCodeAssertions
{
    /// <summary>
    /// Asserts that the output compilation has no errors.
    /// </summary>
    /// <param name="result">The generator test result.</param>
    /// <returns>A task representing the asynchronous assertion.</returns>
    public static async Task CompilationSucceeds(this GeneratorTestResult result)
    {
        if (result.CompilationErrors.Length > 0)
        {
            var errorMessages = string.Join(
                Environment.NewLine,
                result.CompilationErrors.Select(d => $"  {d.Id}: {d.GetMessage()} at {d.Location}"));

            await Assert.That(result.CompilationErrors.Length).IsEqualTo(0)
                .Because($"Compilation should succeed but had {result.CompilationErrors.Length} error(s):{Environment.NewLine}{errorMessages}");
        }
    }

    /// <summary>
    /// Asserts that a generated source file with the specified hint name exists.
    /// </summary>
    /// <param name="result">The generator test result.</param>
    /// <param name="hintName">The hint name of the generated file.</param>
    /// <returns>A task representing the asynchronous assertion.</returns>
    public static async Task HasGeneratedSource(this GeneratorTestResult result, string hintName)
    {
        await Assert.That(result.GeneratedSources.ContainsKey(hintName)).IsTrue()
            .Because($"Expected generated source '{hintName}' but found: [{string.Join(", ", result.GeneratedSources.Keys)}]");
    }

    /// <summary>
    /// Asserts that a generated source file contains the specified text.
    /// </summary>
    /// <param name="result">The generator test result.</param>
    /// <param name="hintName">The hint name of the generated file.</param>
    /// <param name="text">The text expected in the generated source.</param>
    /// <returns>A task representing the asynchronous assertion.</returns>
    public static async Task GeneratedSourceContains(this GeneratorTestResult result, string hintName, string text)
    {
        await result.HasGeneratedSource(hintName);
        var source = result.GeneratedSources[hintName];
        await Assert.That(source).Contains(text);
    }

    /// <summary>
    /// Asserts that no generated source file with the specified hint name exists.
    /// </summary>
    /// <param name="result">The generator test result.</param>
    /// <param name="hintName">The hint name that should NOT be present.</param>
    /// <returns>A task representing the asynchronous assertion.</returns>
    public static async Task DoesNotHaveGeneratedSource(this GeneratorTestResult result, string hintName)
    {
        await Assert.That(result.GeneratedSources.ContainsKey(hintName)).IsFalse()
            .Because($"Expected no generated source '{hintName}' but found it among: [{string.Join(", ", result.GeneratedSources.Keys)}]");
    }

    /// <summary>
    /// Asserts that the generator produced no diagnostics.
    /// </summary>
    /// <param name="result">The generator test result.</param>
    /// <returns>A task representing the asynchronous assertion.</returns>
    public static async Task HasNoGeneratorDiagnostics(this GeneratorTestResult result)
    {
        if (result.GeneratorDiagnostics.Length > 0)
        {
            var diagnosticMessages = string.Join(
                Environment.NewLine,
                result.GeneratorDiagnostics.Select(d => $"  {d.Id}: {d.GetMessage()}"));

            await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0)
                .Because($"Generator should produce no diagnostics but had:{Environment.NewLine}{diagnosticMessages}");
        }
    }
}

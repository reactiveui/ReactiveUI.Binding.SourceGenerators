// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;

using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Snapshot tests for BindInteraction invocation generation.
/// </summary>
public class BindInteractionGeneratorTests
{
    /// <summary>
    /// Verifies BindInteraction with a task-based handler.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TaskHandler()
    {
        var source = SharedSourceReader.ReadScenario("BindInteraction/TaskHandler");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindInteractionGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindInteraction with an observable-based handler.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableHandler()
    {
        var source = SharedSourceReader.ReadScenario("BindInteraction/ObservableHandler");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindInteractionGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindInteraction with a deep property path (Child.Confirm).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepPropertyPath()
    {
        var source = SharedSourceReader.ReadScenario("BindInteraction/DeepPropertyPath");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindInteractionGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindInteraction with a ViewModel that does not implement INPC.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NonINPCViewModel()
    {
        var source = SharedSourceReader.ReadScenario("BindInteraction/NonINPCViewModel");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindInteractionGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindInteraction generates CallerFilePath-based dispatch when targeting pre-C# 10.
    /// CompilationSucceeds is omitted because the CallerFilePath stub signature is ambiguous
    /// with the runtime extension method in this test harness (both assemblies are referenced).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallerFilePathFallback()
    {
        var source = SharedSourceReader.ReadScenario("BindInteraction/TaskHandler");
        var result = await TestHelper.TestPassWithResult(
            source, typeof(BindInteractionGeneratorTests), LanguageVersion.CSharp7_3);
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindInteraction generates CallerFilePath-based dispatch for ObservableHandler.
    /// Covers the CallerFilePath overload branch with observable handler type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallerFilePathFallback_ObservableHandler()
    {
        var source = SharedSourceReader.ReadScenario("BindInteraction/ObservableHandler");
        var result = await TestHelper.TestPassWithResult(
            source, typeof(BindInteractionGeneratorTests), LanguageVersion.CSharp7_3);
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindInteraction generates CallerFilePath-based dispatch for DeepPropertyPath.
    /// Covers the deep path observation branch in the CallerFilePath codepath.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallerFilePathFallback_DeepPropertyPath()
    {
        var source = SharedSourceReader.ReadScenario("BindInteraction/DeepPropertyPath");
        var result = await TestHelper.TestPassWithResult(
            source, typeof(BindInteractionGeneratorTests), LanguageVersion.CSharp7_3);
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindInteraction generates CallerFilePath-based dispatch for NonINPCViewModel.
    /// Covers the non-INPC return-observable branch in the CallerFilePath codepath.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallerFilePathFallback_NonINPCViewModel()
    {
        var source = SharedSourceReader.ReadScenario("BindInteraction/NonINPCViewModel");
        var result = await TestHelper.TestPassWithResult(
            source, typeof(BindInteractionGeneratorTests), LanguageVersion.CSharp7_3);
        await result.HasNoGeneratorDiagnostics();
    }
}

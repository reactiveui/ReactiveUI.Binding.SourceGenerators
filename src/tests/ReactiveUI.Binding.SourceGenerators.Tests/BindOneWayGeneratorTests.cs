// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;

using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Snapshot tests for BindOneWay (one-way binding) invocation generation.
/// </summary>
public class BindOneWayGeneratorTests
{
    /// <summary>
    /// Verifies BindOneWay with same-type string property binding.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_StringToString()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/SinglePropertyStringToString");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindOneWayGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindOneWay with int property binding.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_IntToInt()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/SinglePropertyIntToInt");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindOneWayGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindOneWay with multiple bindings on the same source/target pair.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleBindings()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/MultipleBindings");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindOneWayGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindOneWay on a ReactiveObject-based ViewModel.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveObject_Source()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/ReactiveObjectSource");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindOneWayGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindOneWay with a conversion function from int to string.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_WithConverter()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/SinglePropertyWithConverter");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindOneWayGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindOneWay with a scheduler parameter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_WithScheduler()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/SinglePropertyWithScheduler");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindOneWayGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that BindOneWay generates CallerFilePath dispatch when targeting pre-C# 10.
    /// CompilationSucceeds is omitted because the CallerFilePath stub signature is ambiguous
    /// with the runtime extension method in this test harness (both assemblies are referenced).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_StringToString_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/SinglePropertyStringToString");
        var result = await TestHelper.TestPassWithResult(
            source, typeof(BindOneWayGeneratorTests), LanguageVersion.CSharp9);
        await result.HasNoGeneratorDiagnostics();
    }
}

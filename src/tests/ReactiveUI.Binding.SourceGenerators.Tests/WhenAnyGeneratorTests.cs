// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;

using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Snapshot tests for WhenAny (IObservedChange-wrapping) invocation generation.
/// </summary>
public class WhenAnyGeneratorTests
{
    /// <summary>
    /// Verifies WhenAny with a single property on an INPC class.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_INPC()
    {
        var source = SharedSourceReader.ReadScenario("WhenAny/SinglePropertyINPC");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenAnyGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenAny with two properties and a selector combining IObservedChange values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_TwoProperties()
    {
        var source = SharedSourceReader.ReadScenario("WhenAny/MultiPropertyTwoProperties");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenAnyGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenAny generates CallerFilePath dispatch when targeting pre-C# 10.
    /// CompilationSucceeds is omitted because the CallerFilePath stub signature is ambiguous
    /// with the runtime extension method in this test harness (both assemblies are referenced).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_INPC_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("WhenAny/SinglePropertyINPC");
        var result = await TestHelper.TestPassWithResult(
            source, typeof(WhenAnyGeneratorTests), LanguageVersion.CSharp9);
        await result.HasNoGeneratorDiagnostics();
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Snapshot tests for Bind (view-first two-way binding) invocation generation.
/// </summary>
public class BindGeneratorTests
{
    /// <summary>
    /// Verifies Bind with same-type string property binding.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_StringToString()
    {
        var source = SharedSourceReader.ReadScenario("Bind/SinglePropertyStringToString");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies Bind with multiple bindings on the same view/vm pair.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleBindings()
    {
        var source = SharedSourceReader.ReadScenario("Bind/MultipleBindings");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }
}

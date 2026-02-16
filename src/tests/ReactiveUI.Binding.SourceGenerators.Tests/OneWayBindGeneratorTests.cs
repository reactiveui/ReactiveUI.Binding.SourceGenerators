// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Snapshot tests for OneWayBind (view-first one-way binding) invocation generation.
/// </summary>
public class OneWayBindGeneratorTests
{
    /// <summary>
    /// Verifies OneWayBind with same-type string property binding.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_StringToString()
    {
        var source = SharedSourceReader.ReadScenario("OneWayBind/SinglePropertyStringToString");
        var result = await TestHelper.TestPassWithResult(source, typeof(OneWayBindGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies OneWayBind with multiple bindings on the same view/vm pair.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleBindings()
    {
        var source = SharedSourceReader.ReadScenario("OneWayBind/MultipleBindings");
        var result = await TestHelper.TestPassWithResult(source, typeof(OneWayBindGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }
}

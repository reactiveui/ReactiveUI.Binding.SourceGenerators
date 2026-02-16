// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Snapshot tests for WhenAnyObservable (Switch/Merge/CombineLatest) invocation generation.
/// </summary>
public class WhenAnyObservableGeneratorTests
{
    /// <summary>
    /// Verifies WhenAnyObservable with a single observable property (Switch pattern).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleObservable()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyObservable/SingleObservable");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenAnyObservableGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenAnyObservable with two same-type observable properties (Merge pattern).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoObservables_Merge()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyObservable/TwoObservablesMerge");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenAnyObservableGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }
}

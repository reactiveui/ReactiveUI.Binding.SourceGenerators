// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Snapshot tests for BindTo (observable-to-property binding) invocation generation.
/// </summary>
public class BindToGeneratorTests
{
    /// <summary>
    /// Verifies BindTo with a same-typed string observable and string property (direct assignment).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SameTypeString()
    {
        var source = SharedSourceReader.ReadScenario("BindTo/SameTypeString");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindToGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindTo coerces differing source/target types via the converter registry.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DifferingTypes()
    {
        var source = SharedSourceReader.ReadScenario("BindTo/DifferingTypes");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindToGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindTo with an explicit IBindingTypeConverter override.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithConverterOverride()
    {
        var source = SharedSourceReader.ReadScenario("BindTo/WithConverterOverride");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindToGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindTo with a conversion hint forwarded to the resolved converter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithConversionHint()
    {
        var source = SharedSourceReader.ReadScenario("BindTo/WithConversionHint");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindToGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that BindTo generates CallerFilePath dispatch when targeting pre-C# 10.
    /// CompilationSucceeds is omitted because the CallerFilePath stub signature is ambiguous
    /// with the runtime extension method in this test harness (both assemblies are referenced).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SameTypeString_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("BindTo/SameTypeString");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(BindToGeneratorTests),
            LanguageVersion.CSharp7_3);
        await result.HasNoGeneratorDiagnostics();
    }
}

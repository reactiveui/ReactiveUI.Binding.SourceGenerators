// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers how the generator reads the consumer's language and framework capabilities, which decide the shape of
/// every emitted overload. Both inputs come from the compilation rather than from anything under our control, so
/// each is read defensively and answered with the conservative value when it is absent.
/// </summary>
public class LanguageFeatureDetectionTests
{
    /// <summary>A minimal consumer, enough to build a compilation from.</summary>
    private const string MinimalSource = """
                                         namespace Probe
                                         {
                                             public class Marker
                                             {
                                             }
                                         }
                                         """;

    /// <summary>C# parse options carry the version the consumer asked for.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadLanguageVersion_CSharpParseOptions_ReturnsTheRequestedVersion()
    {
        var version = BindingGenerator.ReadLanguageVersion(new CSharpParseOptions(LanguageVersion.CSharp10));

        await Assert.That(version).IsEqualTo(LanguageVersion.CSharp10);
    }

    /// <summary>
    /// Options that are not C#'s carry no version to read, and the compiler default is the answer that keeps the
    /// pass running rather than failing every file in the compilation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadLanguageVersion_NonCSharpParseOptions_ReturnsTheCompilerDefault() =>
        await Assert.That(BindingGenerator.ReadLanguageVersion(null)).IsEqualTo(LanguageVersion.Default);

    /// <summary>A consumer on a framework that declares the attribute can have generated code apply it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasAccessibleExpressionAttribute_FrameworkDeclaresIt_ReturnsTrue()
    {
        var compilation = TestHelper.CreateCompilation(MinimalSource, LanguageVersion.CSharp10);

        await Assert.That(BindingGenerator.HasAccessibleExpressionAttribute(compilation)).IsTrue();
    }

    /// <summary>
    /// A compilation whose references do not declare the attribute cannot apply it, so the expression parameters
    /// are left off the generated overload and dispatch falls back to the file and line.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasAccessibleExpressionAttribute_NothingDeclaresIt_ReturnsFalse()
    {
        var compilation = CSharpCompilation.Create("Probe");

        await Assert.That(BindingGenerator.HasAccessibleExpressionAttribute(compilation)).IsFalse();
    }

    /// <summary>A runtime stub without expression-text parameters keeps the shorter interceptor signature.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StubHasExpressionParameters_AttributeAvailableButStubOmittedThem_ReturnsFalse()
    {
        const string source = """
            namespace Probe
            {
                public static class Stub
                {
                    public static void WhenAnyValue<TSender, TValue>(
                        TSender sender,
                        System.Linq.Expressions.Expression<System.Func<TSender, TValue>> property1,
                        string callerFilePath = "",
                        int callerLineNumber = 0) { }
                }
            }
            """;
        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var stub = compilation.GetTypeByMetadataName("Probe.Stub");

        await Assert.That(BindingGenerator.HasAccessibleExpressionAttribute(compilation)).IsTrue();
        await Assert.That(BindingGenerator.StubHasExpressionParameters(stub)).IsFalse();
    }

    /// <summary>A runtime stub with expression-text parameters exposes the longer interceptor signature.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StubHasExpressionParameters_RuntimeStubDeclaresThem_ReturnsTrue()
    {
        const string source = """
            namespace Probe
            {
                public static class Stub
                {
                    public static void WhenAnyValue<TSender, TValue>(
                        TSender sender,
                        System.Linq.Expressions.Expression<System.Func<TSender, TValue>> property1,
                        string property1Expression = "",
                        string callerFilePath = "",
                        int callerLineNumber = 0) { }
                }
            }
            """;
        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var stub = compilation.GetTypeByMetadataName("Probe.Stub");

        await Assert.That(BindingGenerator.StubHasExpressionParameters(stub)).IsTrue();
    }

    /// <summary>An interceptor matches a runtime stub built without caller-expression parameters.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NetFrameworkStyleStub_InterceptorMatchesShortSignature()
    {
        const string source = """
            using System;
            using System.ComponentModel;
            using System.Linq.Expressions;
            using System.Runtime.CompilerServices;
            using ReactiveUI.Binding;

            namespace ReactiveUI.Binding
            {
                public static class ReactiveUIBindingExtensions
                {
                    public static IObservable<TValue> WhenAnyValue<TSender, TValue>(
                        this TSender sender,
                        Expression<Func<TSender, TValue>> property1,
                        [CallerFilePath] string callerFilePath = "",
                        [CallerLineNumber] int callerLineNumber = 0)
                        where TSender : class => throw new NotImplementedException();
                }
            }

            public sealed class Model : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler PropertyChanged;
                public string Name { get; set; } = "initial";
            }

            public static class Usage
            {
                public static IObservable<string> Observe(Model model) => model.WhenAnyValue(x => x.Name, "", 0);
            }
            """;
        var parseOptions = TestHelper.InterceptingParseOptionsFor(LanguageVersion.CSharp10);
        var compilation = TestHelper.CreateCompilation(source, parseOptions, false, "TestAssembly", []);
        var result = TestHelper.RunGenerator(compilation, parseOptions, "Probe", true);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(
            "WhenAnyValueDispatch.g.cs",
            InterceptableLocationReader.IsSupported ? "__Intercept_WhenAnyValue_" : "Concrete typed overload for WhenAnyValue");
    }
}

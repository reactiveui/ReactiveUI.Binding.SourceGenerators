// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Tests the tier that claims a binding call site outright instead of offering an overload that has to win
/// extension-method lookup.
/// </summary>
/// <remarks>
/// Which tier a build gets is settled by the compiler hosting the generator and by whether the project lists
/// the generated namespace, so every test here states the outcome for both and the suite runs against both
/// generator builds. That is what keeps the assertions honest on a compiler that cannot describe a call site
/// at all, where the overloads are still the only thing that can be emitted.
/// </remarks>
public class InterceptedCallSiteTests
{
    /// <summary>The attribute text an interceptor carries.</summary>
    private const string InterceptsAttribute = "InterceptsLocation(";

    /// <summary>The declaration text a dispatch overload carries.</summary>
    private const string OverloadDeclaration = "public static global::System.IObservable<";

    /// <summary>The dispatch file these tests read.</summary>
    private const string DispatchFileName = "WhenChangedDispatch.g.cs";

    /// <summary>The root namespace the scenarios build under.</summary>
    private const string RootNamespace = "TestApp";

    /// <summary>The type the scenario exposes its binding through.</summary>
    private const string UsageTypeName = $"{RootNamespace}.Usage";

    /// <summary>A binding whose value can be read back out of the emitted assembly.</summary>
    private const string Scenario = """
                                    using System;
                                    using System.ComponentModel;
                                    using ReactiveUI.Binding;

                                    namespace TestApp
                                    {
                                        public class MyViewModel : INotifyPropertyChanged
                                        {
                                            private string _name = "start";

                                            public event PropertyChangedEventHandler PropertyChanged;

                                            public string Name
                                            {
                                                get { return _name; }
                                                set
                                                {
                                                    _name = value;
                                                    var handler = PropertyChanged;
                                                    if (handler != null)
                                                    {
                                                        handler(this, new PropertyChangedEventArgs("Name"));
                                                    }
                                                }
                                            }
                                        }

                                        public static class Usage
                                        {
                                            public static string Observe()
                                            {
                                                var viewModel = new MyViewModel();
                                                string seen = null;
                                                var subscription = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(
                                                    viewModel.WhenChanged(x => x.Name),
                                                    delegate(string value) { seen = value; });
                                                viewModel.Name = "changed";
                                                subscription.Dispose();
                                                return seen;
                                            }
                                        }
                                    }
                                    """;

    /// <summary>The same binding written in a namespace no root namespace encloses.</summary>
    private const string OutOfReachScenario = """
                                              using System;
                                              using System.ComponentModel;
                                              using ReactiveUI.Binding;

                                              namespace Elsewhere
                                              {
                                                  public class MyViewModel : INotifyPropertyChanged
                                                  {
                                                      public event PropertyChangedEventHandler PropertyChanged;

                                                      public string Name { get; set; }
                                                  }

                                                  public class Usage
                                                  {
                                                      public void Bind()
                                                      {
                                                          var viewModel = new MyViewModel();
                                                          var observable = viewModel.WhenChanged(x => x.Name);
                                                          GC.KeepAlive(observable);
                                                      }
                                                  }
                                              }
                                              """;

    /// <summary>A build listing the generated namespace gets the tier its compiler can honour.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OptedInBuild_EmitsInterceptorsWhereTheCompilerCanDescribeACallSite()
    {
        var dispatch = GenerateDispatch(Scenario, LanguageVersion.CSharp10, optIn: true);

        await Assert.That(dispatch.Contains(InterceptsAttribute, StringComparison.Ordinal))
            .IsEqualTo(InterceptableLocationReader.IsSupported);
        await Assert.That(dispatch.Contains(OverloadDeclaration, StringComparison.Ordinal))
            .IsEqualTo(!InterceptableLocationReader.IsSupported);
    }

    /// <summary>The opt-in is what turns the tier on; without it the overloads are emitted either way.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildWithoutTheOptIn_EmitsTheDispatchOverload()
    {
        var dispatch = GenerateDispatch(Scenario, LanguageVersion.CSharp10, optIn: false);

        await Assert.That(dispatch).DoesNotContain(InterceptsAttribute);
        await Assert.That(dispatch).Contains(OverloadDeclaration);
    }

    /// <summary>
    /// A project below C# 10 is served the same way. Interception is refused on a language version, not chosen
    /// by one, which is what puts a compile-time binding in reach of a consumer the overloads cannot serve.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OptedInBuild_BelowCSharp10_StillClaimsTheCallSite()
    {
        var dispatch = GenerateDispatch(Scenario, LanguageVersion.CSharp7_3, optIn: true);

        await Assert.That(dispatch.Contains(InterceptsAttribute, StringComparison.Ordinal))
            .IsEqualTo(InterceptableLocationReader.IsSupported);
        await Assert.That(dispatch.Contains(OverloadDeclaration, StringComparison.Ordinal))
            .IsEqualTo(!InterceptableLocationReader.IsSupported);
    }

    /// <summary>
    /// A file declared outside the root namespace is out of the overloads' reach, and is claimed anyway. This is
    /// the case the tier exists for: nothing about an interceptor goes through extension-method lookup.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OptedInBuild_ClaimsACallSiteOutsideTheRootNamespace()
    {
        var dispatch = GenerateDispatch(OutOfReachScenario, LanguageVersion.CSharp7_3, optIn: true);

        await Assert.That(dispatch.Contains(InterceptsAttribute, StringComparison.Ordinal))
            .IsEqualTo(InterceptableLocationReader.IsSupported);
    }

    /// <summary>
    /// The emitted assembly runs the binding. Emission is where the compiler checks an interceptor against the
    /// call it replaces, so a signature that does not match fails here rather than being noticed downstream.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OptedInBuild_RunsTheBindingItClaimed()
    {
        var result = Generate(Scenario, LanguageVersion.CSharp10, optIn: true, RootNamespace);

        await Assert.That(result.CompilationErrors).IsEmpty();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        var observe = assembly.GetType(UsageTypeName)?.GetMethod(
            "Observe",
            BindingFlags.Public | BindingFlags.Static);

        await Assert.That(observe).IsNotNull();
        await Assert.That(observe!.Invoke(null, null)).IsEqualTo("changed");

        context.Unload();
    }

    /// <summary>The same binding runs when the overloads are what the build got.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildWithoutTheOptIn_RunsTheBindingItDispatched()
    {
        var result = Generate(Scenario, LanguageVersion.CSharp10, optIn: false, RootNamespace);

        await Assert.That(result.CompilationErrors).IsEmpty();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        var observe = assembly.GetType(UsageTypeName)?.GetMethod(
            "Observe",
            BindingFlags.Public | BindingFlags.Static);

        await Assert.That(observe).IsNotNull();
        await Assert.That(observe!.Invoke(null, null)).IsEqualTo("changed");

        context.Unload();
    }

    /// <summary>The generated namespace is what the opt-in has to name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OptedInBuild_GeneratesIntoTheInterceptedNamespace()
    {
        var dispatch = GenerateDispatch(Scenario, LanguageVersion.CSharp10, optIn: true);

        await Assert.That(dispatch.Contains($"namespace {Constants.InterceptorNamespace}", StringComparison.Ordinal))
            .IsEqualTo(InterceptableLocationReader.IsSupported);
    }

    /// <summary>Runs the generator and returns the dispatch file it produced.</summary>
    /// <param name="source">The consumer source.</param>
    /// <param name="languageVersion">The language version the consumer builds at.</param>
    /// <param name="optIn">Whether the build lists the generated namespace for interception.</param>
    /// <param name="rootNamespace">The root namespace the build exposes.</param>
    /// <returns>The generated dispatch file.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GenerateDispatch(
        string source,
        LanguageVersion languageVersion,
        bool optIn,
        string? rootNamespace = RootNamespace) =>
        Generate(source, languageVersion, optIn, rootNamespace).GeneratedSources[DispatchFileName];

    /// <summary>Runs the generator over a compilation parsed the way the build in question parses.</summary>
    /// <param name="source">The consumer source.</param>
    /// <param name="languageVersion">The language version the consumer builds at.</param>
    /// <param name="optIn">Whether the build lists the generated namespace for interception.</param>
    /// <param name="rootNamespace">The root namespace the build exposes.</param>
    /// <returns>The generator result.</returns>
    private static GeneratorTestResult Generate(
        string source,
        LanguageVersion languageVersion,
        bool optIn,
        string? rootNamespace)
    {
        var parseOptions = optIn
            ? TestHelper.InterceptingParseOptionsFor(languageVersion)
            : TestHelper.ParseOptionsFor(languageVersion);

        var compilation = TestHelper.CreateCompilation(source, parseOptions, false, "TestAssembly", []);

        return TestHelper.RunGenerator(compilation, parseOptions, rootNamespace, true);
    }
}

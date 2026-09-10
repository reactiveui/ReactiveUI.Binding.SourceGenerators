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

    /// <summary>One call site per generated API, so every emitter's tier is settled by the same build.</summary>
    private const string EveryApiScenario = """
                                            using System;
                                            using System.ComponentModel;
                                            using System.Threading.Tasks;
                                            using System.Windows.Input;
                                            using ReactiveUI.Binding;

                                            namespace TestApp
                                            {
                                                public class Person : INotifyPropertyChanged, INotifyPropertyChanging
                                                {
                                                    public event PropertyChangedEventHandler PropertyChanged;

                                                    public event PropertyChangingEventHandler PropertyChanging;

                                                    public string Name { get; set; }

                                                    public int Age { get; set; }

                                                    public ICommand Save { get; set; }

                                                    public IInteraction<string, bool> Confirm { get; set; }

                                                    public IObservable<int> Ticks { get; set; }

                                                    public IObservable<int> Pulses { get; set; }
                                                }

                                                public class Button
                                                {
                                                    public event EventHandler Click;

                                                    public string Text { get; set; }
                                                }

                                                public class PersonView : INotifyPropertyChanged, IViewFor<Person>
                                                {
                                                    public event PropertyChangedEventHandler PropertyChanged;

                                                    public Person ViewModel { get; set; }

                                                    object IViewFor.ViewModel
                                                    {
                                                        get { return ViewModel; }
                                                        set { ViewModel = (Person)value; }
                                                    }

                                                    public Button SaveButton { get; set; }

                                                    public string Display { get; set; }

                                                    public string Summary { get; set; }
                                                }

                                                public class Usage
                                                {
                                                    public void Bind(Person person, PersonView view, IObservable<string> names)
                                                    {
                                                        GC.KeepAlive(person.WhenChanged(x => x.Name));
                                                        GC.KeepAlive(person.WhenChanging(x => x.Name));
                                                        GC.KeepAlive(person.WhenAnyValue(x => x.Age));
                                                        GC.KeepAlive(person.WhenAny(x => x.Name, c => c.Value));
                                                        GC.KeepAlive(person.WhenAnyObservable(x => x.Ticks));
                                                        GC.KeepAlive(person.WhenAnyObservable(x => x.Ticks, x => x.Pulses, (a, b) => a + b));

                                                        person.BindOneWay(view, x => x.Name, v => v.Display);
                                                        person.BindTwoWay(view, x => x.Name, v => v.Display);
                                                        view.OneWayBind(person, x => x.Name, v => v.Summary);
                                                        view.Bind(person, x => x.Name, v => v.Display);
                                                        names.BindTo(view, v => v.Display);
                                                        names.InvokeCommand(person, x => x.Save);
                                                        view.BindCommand(person, x => x.Save, v => v.SaveButton);
                                                        view.BindCommand(person, x => x.Save, v => v.SaveButton, names);
                                                        view.BindInteraction(person, x => x.Confirm, Handle);
                                                    }

                                                    private static Task Handle(IInteractionContext<string, bool> context)
                                                    {
                                                        context.SetOutput(true);
                                                        return Task.CompletedTask;
                                                    }
                                                }
                                            }
                                            """;

    /// <summary>The dispatch file each generated binding API emits.</summary>
    /// <remarks>
    /// Named rather than matched on a suffix, because the view locator emits one too and it claims no call
    /// site. An API that stopped emitting its file would drop silently out of a pattern.
    /// </remarks>
    private static readonly string[] ApiDispatchFiles =
    [
        "WhenChangedDispatch.g.cs",
        "WhenChangingDispatch.g.cs",
        "WhenAnyValueDispatch.g.cs",
        "WhenAnyDispatch.g.cs",
        "WhenAnyObservableDispatch.g.cs",
        "BindOneWayDispatch.g.cs",
        "BindTwoWayDispatch.g.cs",
        "OneWayBindDispatch.g.cs",
        "BindDispatch.g.cs",
        "BindToDispatch.g.cs",
        "BindCommandDispatch.g.cs",
        "BindInteractionDispatch.g.cs",
        "InvokeCommandDispatch.g.cs",
    ];

    /// <summary>Every generated API is claimed the same way, so each emitter's tier follows one decision.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OptedInBuild_ClaimsEveryGeneratedApi()
    {
        var result = Generate(EveryApiScenario, LanguageVersion.CSharp10, optIn: true, RootNamespace);

        await Assert.That(result.CompilationErrors).IsEmpty();

        foreach (var file in ApiDispatchFiles)
        {
            await Assert.That(result.GeneratedSources.ContainsKey(file)).IsTrue();

            await Assert.That(result.GeneratedSources[file].Contains(InterceptsAttribute, StringComparison.Ordinal))
                .IsEqualTo(InterceptableLocationReader.IsSupported);
        }
    }

    /// <summary>Emission checks an interceptor against the call it replaces, so every API is emitted here.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OptedInBuild_EmitsWithEveryApiClaimed()
    {
        var result = Generate(EveryApiScenario, LanguageVersion.CSharp10, optIn: true, RootNamespace);

        var (_, context) = TestHelper.EmitAndLoad(result);
        context.Unload();

        await Assert.That(result.CompilationErrors).IsEmpty();
    }

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

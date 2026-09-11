// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for BindTo (observable-to-property binding) invocation generation.</summary>
public class BindToGeneratorTests
{
    /// <summary>The <c>BindToDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindToDispatchgcsName = "BindToDispatch.g.cs";

    /// <summary>The attribute a generated member carries when only the runtime engine can serve it.</summary>
    private const string RequiresUnreferencedCode = "RequiresUnreferencedCode";

    /// <summary>The runtime engine a call site the compiler could not read is handed to.</summary>
    private const string RuntimeBindingFallback = "RuntimeBindingFallback.BindTo";

    /// <summary>Verifies BindTo with a same-typed string observable and string property (direct assignment).</summary>
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

    /// <summary>Verifies BindTo coerces differing source/target types via the converter registry.</summary>
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

    /// <summary>Verifies BindTo with an explicit IBindingTypeConverter override.</summary>
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

    /// <summary>Verifies BindTo with a conversion hint forwarded to the resolved converter.</summary>
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
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>A BindTo call on a custom extension class is not one of ours and is skipped.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_CustomExtension_GeneratesNoDispatch()
    {
        const string source = """
                              using System;

                              namespace TestApp
                              {
                                  public class MyView
                                  {
                                      public string Caption { get; set; } = "";
                                  }

                                  public static class CustomExtensions
                                  {
                                      public static IDisposable BindTo<TValue, TTarget, TProp>(
                                          this IObservable<TValue> source,
                                          TTarget target,
                                          System.Linq.Expressions.Expression<Func<TTarget, TProp>> property) => null!;
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(IObservable<string> source, MyView view)
                                      {
                                          source.BindTo(view, x => x.Caption);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.DoesNotHaveGeneratedSource(BindToDispatchgcsName);
    }

    /// <summary>A BindTo call whose receiver does not resolve to any symbol is skipped.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_UnresolvedReceiver_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyView
                                  {
                                      public string Caption { get; set; } = "";
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyView view)
                                      {
                                          undefinedStream.BindTo(view, x => x.Caption);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.DoesNotHaveGeneratedSource(BindToDispatchgcsName);
    }

    /// <summary>
    /// A BindTo declared on a class sharing the stub's name but taking too few arguments is skipped
    /// rather than read past the end of its argument list.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_TooFewArguments_GeneratesNoDispatch()
    {
        const string source = """
                              using System;

                              namespace TestApp
                              {
                                  public static class ReactiveUIBindingExtensions
                                  {
                                      public static IDisposable BindTo(this IObservable<string> source) => null!;
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(IObservable<string> source)
                                      {
                                          source.BindTo();
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.DoesNotHaveGeneratedSource(BindToDispatchgcsName);
    }

    /// <summary>
    /// A BindTo whose receiver is a type name rather than a value has no receiver type at all, and is
    /// skipped rather than dereferenced.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_ReceiverWithoutAType_GeneratesNoDispatch()
    {
        const string source = """
                              using System;

                              namespace TestApp
                              {
                                  public class MyView
                                  {
                                      public string Caption { get; set; } = "";
                                  }

                                  public static class ReactiveUIBindingExtensions
                                  {
                                      public static IDisposable BindTo(object target, object property) => null!;
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyView view)
                                      {
                                          ReactiveUIBindingExtensions.BindTo(view, view);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.DoesNotHaveGeneratedSource(BindToDispatchgcsName);
    }

    /// <summary>A BindTo whose receiver implements no observable interface is skipped.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_NonObservableReceiver_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class MyView : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public string Caption { get; set; } = "";
                                  }

                                  public static class ReactiveUIBindingExtensions
                                  {
                                      public static IDisposable BindTo(this MyView source, object target, object property) => null!;
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyView view)
                                      {
                                          view.BindTo(view, view);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.DoesNotHaveGeneratedSource(BindToDispatchgcsName);
    }

    /// <summary>
    /// A receiver that reaches IObservable through an implemented interface rather than being one is
    /// still observed, and binds against that interface's value type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_ObservableThroughAnInterface_GeneratesDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyView : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public string Caption { get; set; } = "";
                                  }

                                  public class StringStream : IObservable<string>
                                  {
                                      public IDisposable Subscribe(IObserver<string> observer) => null!;
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(StringStream source, MyView view)
                                      {
                                          source.BindTo(view, x => x.Caption);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.HasGeneratedSource(BindToDispatchgcsName);
    }

    /// <summary>A target selector held in a variable names no path to read, so the runtime engine serves it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TargetPropertyFromAVariable_GeneratesAnAnnotatedRuntimeDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Linq.Expressions;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyView : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public string Caption { get; set; } = "";
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable Execute(IObservable<string> source, MyView view)
                                      {
                                          Expression<Func<MyView, string>> property = x => x.Caption;
                                          return source.BindTo(view, property);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();

        var dispatch = result.GeneratedSources[BindToDispatchgcsName];
        await Assert.That(dispatch).Contains(RequiresUnreferencedCode);
        await Assert.That(dispatch).Contains(RuntimeBindingFallback);
    }

    /// <summary>A target selector whose body is no property path is served by the runtime engine, and says so.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TargetPropertyWithoutAPropertyPath_GeneratesAnAnnotatedRuntimeDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyView : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public string Caption { get; set; } = "";

                                      public string Resolve() => Caption;
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable Execute(IObservable<string> source, MyView view)
                                      {
                                          return source.BindTo(view, x => x.Resolve());
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();

        var dispatch = result.GeneratedSources[BindToDispatchgcsName];
        await Assert.That(dispatch).Contains(RequiresUnreferencedCode);
        await Assert.That(dispatch).Contains(RuntimeBindingFallback);
    }

    /// <summary>A target selector producing a type no member can declare leaves the call to the stub.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TargetPropertyProducingAnUndeclarableType_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyView : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public string[] Resolve() => new string[0];
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable Execute(IObservable<string[]> source, MyView view)
                                      {
                                          return source.BindTo(view, x => x.Resolve());
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindToDispatchgcsName);
    }
}

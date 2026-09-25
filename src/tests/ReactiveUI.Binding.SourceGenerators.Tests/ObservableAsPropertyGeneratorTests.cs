// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for the bodies and helper fields written for <c>[ObservableAsProperty]</c> partial properties.</summary>
public class ObservableAsPropertyGeneratorTests
{
    /// <summary>A partial property gets a helper field and a body, and ToProperty assigns the helper.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PartialProperty_WithToProperty()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public partial class PersonViewModel : INotifyPropertyChanged
                                  {
                                      public PersonViewModel(IObservable<string> names, IObservable<int?> ages)
                                      {
                                          _fullNameHelper = names.ToProperty(this, x => x.FullName);
                                          _ageHelper = ages.ToProperty(this, x => x.Age);
                                      }

                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      [ObservableAsProperty]
                                      public partial string FullName { get; }

                                      [ObservableAsProperty]
                                      internal partial int? Age { get; }
                                  }
                              }
                              """;

        var result = await TestHelper.TestPassWithResult(source, typeof(ObservableAsPropertyGeneratorTests), LanguageVersion.CSharp13);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>A generic type nested in a partial type repeats both declarations around the members.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NestedGenericType()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public partial class Outer
                                  {
                                      public partial class Holder<T> : INotifyPropertyChanged
                                      {
                                          public Holder(IObservable<T> values) => _currentHelper = values.ToProperty(this, x => x.Current);

                                          public event PropertyChangedEventHandler? PropertyChanged;

                                          [ObservableAsProperty]
                                          public partial T Current { get; }
                                      }
                                  }
                              }
                              """;

        var result = await TestHelper.TestPassWithResult(source, typeof(ObservableAsPropertyGeneratorTests), LanguageVersion.CSharp13);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Two types of the same name in different namespaces get separate files (ReactiveUI.SourceGenerators #131).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SameNameDifferentNamespaces()
    {
        const string source = """
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace First
                              {
                                  public partial class Widget : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      [ObservableAsProperty]
                                      public partial int Count { get; }
                                  }
                              }

                              namespace Second
                              {
                                  public partial class Widget : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      [ObservableAsProperty]
                                      public partial string? Label { get; }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp13);
        await result.HasGeneratedSource("First.Widget.ObservableAsProperties.g.cs");
        await result.HasGeneratedSource("Second.Widget.ObservableAsProperties.g.cs");
        await result.CompilationSucceeds();
    }

    /// <summary>A property three types deep is wrapped in every containing declaration (ReactiveUI.SourceGenerators #133).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ThreeLevelsNested()
    {
        const string source = """
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public partial class A
                                  {
                                      public partial class B
                                      {
                                          public partial class C : INotifyPropertyChanged
                                          {
                                              public event PropertyChangedEventHandler? PropertyChanged;

                                              [ObservableAsProperty]
                                              public partial int Depth { get; }
                                          }
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp13);
        await result.HasGeneratedSource("TestApp.A.B.C.ObservableAsProperties.g.cs");
        await result.CompilationSucceeds();
    }

    /// <summary>
    /// A type with more than one type parameter names its file without the space its display name puts between
    /// them, so the hint name stays a plain file name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoTypeParameters_NamesTheFileWithoutSpaces()
    {
        const string source = """
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public partial class Pair<TKey, TValue> : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      [ObservableAsProperty]
                                      public partial int Count { get; }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp13);
        await result.HasGeneratedSource("TestApp.Pair[TKey,TValue].ObservableAsProperties.g.cs");
        await result.CompilationSucceeds();
    }

    /// <summary>A property that is not a partial get-only declaration is left alone.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NonPartialOrSettable_GeneratesNothing()
    {
        const string source = """
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public partial class PersonViewModel
                                  {
                                      [ObservableAsProperty]
                                      public string Name { get; } = "";

                                      [ObservableAsProperty]
                                      public string Title { get; set; } = "";
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp13);
        await result.DoesNotHaveGeneratedSource("TestApp.PersonViewModel.ObservableAsProperties.g.cs");
    }

    /// <summary>A partial property in a type nested inside a type that is not partial cannot be implemented.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NonPartialContainer_GeneratesNothing()
    {
        const string source = """
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class Outer
                                  {
                                      public partial class PersonViewModel
                                      {
                                          [ObservableAsProperty]
                                          public partial string Name { get; }
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp13);
        await result.DoesNotHaveGeneratedSource("TestApp.Outer.PersonViewModel.ObservableAsProperties.g.cs");
    }
}

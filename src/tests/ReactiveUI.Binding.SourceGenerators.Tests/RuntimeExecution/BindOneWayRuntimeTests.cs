// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Tests that verify the generator produces correct BindOneWay dispatch code
/// and that it compiles and contains expected structure.
/// </summary>
public class BindOneWayRuntimeTests
{
    /// <summary>
    /// Verifies that BindOneWay generates dispatch and registration files.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringBinding_GeneratesDispatchAndRegistration()
    {
        const string source = """
            using System;
            using System.ComponentModel;
            using ReactiveUI.Binding;

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string Name { get; set; } = "";
                }

                public class MyView : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string NameText { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new MyViewModel();
                        var view = new MyView();
                        var binding = vm.BindOneWay(view, x => x.Name, x => x.NameText);
                    }
                }
            }
            """;

        var result = TestHelper.RunGenerator(source);

        await result.HasNoGeneratorDiagnostics();
        await result.HasGeneratedSource("BindOneWayDispatch.g.cs");
        await result.HasGeneratedSource("GeneratedBinderRegistration.g.cs");
        await result.GeneratedSourceContains("BindOneWayDispatch.g.cs", "NameText");
        await result.GeneratedSourceContains("BindOneWayDispatch.g.cs", "Name");
    }

    /// <summary>
    /// Verifies that WhenChanging generates dispatch code with PropertyChanging subscription.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanging_GeneratesPropertyChangingSubscription()
    {
        const string source = """
            using System;
            using System.ComponentModel;
            using ReactiveUI.Binding;

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged, INotifyPropertyChanging
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public event PropertyChangingEventHandler? PropertyChanging;
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new MyViewModel();
                        var obs = vm.WhenChanging(x => x.Name);
                    }
                }
            }
            """;

        var result = TestHelper.RunGenerator(source);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        await result.HasGeneratedSource("WhenChangingDispatch.g.cs");
        await result.GeneratedSourceContains("WhenChangingDispatch.g.cs", "PropertyChanging");
    }

    /// <summary>
    /// Verifies that WhenAnyValue generates dispatch code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_GeneratesDispatch()
    {
        const string source = """
            using System;
            using System.ComponentModel;
            using ReactiveUI.Binding;

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string Name { get; set; } = "";
                    public int Age { get; set; }
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new MyViewModel();
                        var obs = vm.WhenAnyValue(x => x.Name, x => x.Age);
                    }
                }
            }
            """;

        var result = TestHelper.RunGenerator(source);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        await result.HasGeneratedSource("WhenAnyValueDispatch.g.cs");
        await result.GeneratedSourceContains("WhenAnyValueDispatch.g.cs", "CombineLatest");
    }

    /// <summary>
    /// Verifies that BindTwoWay generates dispatch code for two-way binding.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_GeneratesDispatch()
    {
        const string source = """
            using System;
            using System.ComponentModel;
            using ReactiveUI.Binding;

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string Name { get; set; } = "";
                }

                public class MyView : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string NameText { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new MyViewModel();
                        var view = new MyView();
                        var binding = vm.BindTwoWay(view, x => x.Name, x => x.NameText);
                    }
                }
            }
            """;

        var result = TestHelper.RunGenerator(source);

        await result.HasNoGeneratorDiagnostics();
        await result.HasGeneratedSource("BindTwoWayDispatch.g.cs");
        await result.GeneratedSourceContains("BindTwoWayDispatch.g.cs", "Name");
        await result.GeneratedSourceContains("BindTwoWayDispatch.g.cs", "NameText");
    }
}

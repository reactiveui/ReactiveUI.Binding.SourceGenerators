// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;

using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Snapshot tests for WhenChanged (after-change) invocation generation.
/// </summary>
public class WhenChangedGeneratorTests
{
    /// <summary>
    /// Verifies that a WhenChanged invocation on an INPC class generates PropertyChanged observation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_INPC()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/SinglePropertyINPC");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that the generator produces correct output when no invocations are present.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NoInvocations()
    {
        const string source = """
            using System;
            using System.ComponentModel;

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string Name { get; set; } = "";
                }
            }
            """;

        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanged on a ReactiveObject-based class.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_ReactiveObject()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/SinglePropertyReactiveObject");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanged with two properties returns a tuple.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_TwoProperties()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/MultiPropertyTwoProperties");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanged with three properties returns a tuple.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_ThreeProperties()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/MultiPropertyThreeProperties");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanged with a selector function.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_WithSelector()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/MultiPropertyWithSelector");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanged with a deep property chain like x => x.Child.Name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepPropertyChain()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/DeepPropertyChain");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanged with a nullable property type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableProperty()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/NullableProperty");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanged with multiple ViewModels in the same compilation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleViewModels()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/MultipleViewModels");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanged with an integer property type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IntProperty()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/IntProperty");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanged with a 4-level deep property chain (ObjChain1 -> ObjChain2 -> ObjChain3 -> Model).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FourLevelDeepChain()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/FourLevelDeepChain");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanged with multiple invocations on the same ViewModel.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleInvocations_SameViewModel()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/MultipleInvocationsSameViewModel");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanged with a mix of deep chain and shallow properties in multi-property observation.
    /// Tests that Address.City uses Select/Switch deep chain pattern while Name uses shallow observation,
    /// both combined via CombineLatest with a selector function.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_WithDeepChains()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/MultiPropertyWithDeepChains");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanged with the null-forgiving operator (x => x.Child!.Name).
    /// The generator must unwrap the PostfixUnaryExpressionSyntax (SuppressNullableWarning)
    /// to extract the correct property path.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullForgivingDeepChain()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/NullForgivingDeepChain");
        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanged on a WPF DependencyObject generates the correct observation code.
    /// Uses a stub DependencyObject to simulate WPF without requiring the WPF SDK.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfDependencyObject_Property()
    {
        const string source = """
            using System;
            using System.ComponentModel;

            using ReactiveUI.Binding;

            namespace System.Windows
            {
                public class DependencyObject {}
            }

            namespace TestApp
            {
                public class MyWpfControl : System.Windows.DependencyObject, INotifyPropertyChanged
                {
                    private string _text = string.Empty;
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string Text
                    {
                        get => _text;
                        set
                        {
                            if (_text != value)
                            {
                                _text = value;
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                            }
                        }
                    }
                }

                public static class Scenario
                {
                    public static IObservable<string> Execute(MyWpfControl control)
                        => control.WhenChanged(x => x.Text);
                }
            }
            """;

        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanged on a WinForms Component generates the correct observation code.
    /// Uses a stub Component to simulate WinForms without requiring the WinForms SDK.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsComponent_Property()
    {
        const string source = """
            using System;
            using System.ComponentModel;

            using ReactiveUI.Binding;

            namespace TestApp
            {
                public class MyWinFormsControl : System.ComponentModel.Component, INotifyPropertyChanged
                {
                    private string _text = string.Empty;
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string Text
                    {
                        get => _text;
                        set
                        {
                            if (_text != value)
                            {
                                _text = value;
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                            }
                        }
                    }
                }

                public static class Scenario
                {
                    public static IObservable<string> Execute(MyWinFormsControl control)
                        => control.WhenChanged(x => x.Text);
                }
            }
            """;

        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanged on an Android View generates the correct observation code.
    /// Uses a stub View to simulate Android without requiring the Android SDK.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidView_Property()
    {
        const string source = """
            using System;
            using System.ComponentModel;

            using ReactiveUI.Binding;

            namespace Android.Views
            {
                public class View {}
            }

            namespace TestApp
            {
                public class MyAndroidView : Android.Views.View, INotifyPropertyChanged
                {
                    private string _text = string.Empty;
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string Text
                    {
                        get => _text;
                        set
                        {
                            if (_text != value)
                            {
                                _text = value;
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                            }
                        }
                    }
                }

                public static class Scenario
                {
                    public static IObservable<string> Execute(MyAndroidView view)
                        => view.WhenChanged(x => x.Text);
                }
            }
            """;

        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanged on an NSObject (KVO) generates the correct observation code.
    /// Uses a stub NSObject to simulate Apple platforms without requiring the iOS/macOS SDK.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVO_NSObject_Property()
    {
        const string source = """
            using System;
            using System.ComponentModel;

            using ReactiveUI.Binding;

            namespace Foundation
            {
                public class NSObject {}
            }

            namespace TestApp
            {
                public class MyAppleView : Foundation.NSObject, INotifyPropertyChanged
                {
                    private string _text = string.Empty;
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string Text
                    {
                        get => _text;
                        set
                        {
                            if (_text != value)
                            {
                                _text = value;
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                            }
                        }
                    }
                }

                public static class Scenario
                {
                    public static IObservable<string> Execute(MyAppleView view)
                        => view.WhenChanged(x => x.Text);
                }
            }
            """;

        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanged on a WinUI DependencyObject generates the correct observation code.
    /// Uses a stub DependencyObject to simulate WinUI without requiring the WinUI SDK.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIDependencyObject_Property()
    {
        const string source = """
            using System;
            using System.ComponentModel;

            using ReactiveUI.Binding;

            namespace Microsoft.UI.Xaml
            {
                public class DependencyObject {}
            }

            namespace TestApp
            {
                public class MyWinUIControl : Microsoft.UI.Xaml.DependencyObject, INotifyPropertyChanged
                {
                    private string _text = string.Empty;
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string Text
                    {
                        get => _text;
                        set
                        {
                            if (_text != value)
                            {
                                _text = value;
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                            }
                        }
                    }
                }

                public static class Scenario
                {
                    public static IObservable<string> Execute(MyWinUIControl control)
                        => control.WhenChanged(x => x.Text);
                }
            }
            """;

        var result = await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests));
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanged generates CallerFilePath dispatch when targeting pre-C# 10.
    /// CompilationSucceeds is omitted because the CallerFilePath stub signature is ambiguous
    /// with the runtime extension method in this test harness (both assemblies are referenced).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_INPC_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/SinglePropertyINPC");
        var result = await TestHelper.TestPassWithResult(
            source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp9);
        await result.HasNoGeneratorDiagnostics();
    }
}

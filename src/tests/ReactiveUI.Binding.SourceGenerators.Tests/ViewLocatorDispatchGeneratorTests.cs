// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Snapshot tests for the ViewLocatorDispatchGenerator.
/// Verifies that the generator correctly detects IViewFor&lt;T&gt; implementations
/// and generates AOT-safe view dispatch code.
/// </summary>
public class ViewLocatorDispatchGeneratorTests
{
    /// <summary>
    /// Verifies that a single IViewFor&lt;T&gt; implementation generates correct dispatch code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task SingleViewForImplementation()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class LoginViewModel : INotifyPropertyChanged
                {
                    public string UserName { get; set; }
                    public event PropertyChangedEventHandler PropertyChanged;
                }

                public class LoginView : ReactiveUI.Binding.IViewFor<LoginViewModel>
                {
                    public LoginViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (LoginViewModel)value;
                    }
                }
            }
            """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>
    /// Verifies that multiple IViewFor&lt;T&gt; implementations generate multiple dispatch branches.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task MultipleViewForImplementations()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class LoginViewModel : INotifyPropertyChanged
                {
                    public string UserName { get; set; }
                    public event PropertyChangedEventHandler PropertyChanged;
                }

                public class MainViewModel : INotifyPropertyChanged
                {
                    public string Title { get; set; }
                    public event PropertyChangedEventHandler PropertyChanged;
                }

                public class LoginView : ReactiveUI.Binding.IViewFor<LoginViewModel>
                {
                    public LoginViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (LoginViewModel)value;
                    }
                }

                public class MainView : ReactiveUI.Binding.IViewFor<MainViewModel>
                {
                    public MainViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (MainViewModel)value;
                    }
                }
            }
            """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>
    /// Verifies that a view without a parameterless constructor generates service-locator-only dispatch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task ViewWithoutParameterlessConstructor()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class SettingsViewModel : INotifyPropertyChanged
                {
                    public string Theme { get; set; }
                    public event PropertyChangedEventHandler PropertyChanged;
                }

                public class SettingsView : ReactiveUI.Binding.IViewFor<SettingsViewModel>
                {
                    private readonly string _config;

                    public SettingsView(string config)
                    {
                        _config = config;
                    }

                    public SettingsViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (SettingsViewModel)value;
                    }
                }
            }
            """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>
    /// Verifies that abstract classes implementing IViewFor&lt;T&gt; are excluded from dispatch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task AbstractViewIsExcluded()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class BaseViewModel : INotifyPropertyChanged
                {
                    public string Name { get; set; }
                    public event PropertyChangedEventHandler PropertyChanged;
                }

                public abstract class BaseView : ReactiveUI.Binding.IViewFor<BaseViewModel>
                {
                    public BaseViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (BaseViewModel)value;
                    }
                }
            }
            """;

        // Abstract views should not produce ViewDispatch.g.cs
        var result = TestHelper.RunGenerator(source);
        return Verify(result.Driver)
            .UseTypeName("VDG")
            .UseMethodName("AbstractExcl");
    }

    /// <summary>
    /// Verifies that a view with a private constructor generates service-locator-only dispatch (no direct construction).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task ViewWithPrivateConstructor()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class PrivateCtorViewModel : INotifyPropertyChanged
                {
                    public string Data { get; set; }
                    public event PropertyChangedEventHandler PropertyChanged;
                }

                public class PrivateCtorView : ReactiveUI.Binding.IViewFor<PrivateCtorViewModel>
                {
                    private PrivateCtorView() { }

                    public PrivateCtorViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (PrivateCtorViewModel)value;
                    }
                }
            }
            """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>
    /// Verifies that a class not implementing IViewFor produces no ViewDispatch output.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task NonViewForClass_NoDispatch()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class PlainViewModel : INotifyPropertyChanged
                {
                    public string Name { get; set; }
                    public event PropertyChangedEventHandler PropertyChanged;
                }
            }
            """;

        var result = TestHelper.RunGenerator(source);
        return Verify(result.Driver)
            .UseTypeName("VDG")
            .UseMethodName("NoViewFor");
    }

    /// <summary>
    /// Verifies that duplicate IViewFor&lt;T&gt; implementations for the same view model are deduplicated.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task DuplicateViewModelsAreDeduplicated()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class SharedViewModel : INotifyPropertyChanged
                {
                    public string Data { get; set; }
                    public event PropertyChangedEventHandler PropertyChanged;
                }

                public class DesktopView : ReactiveUI.Binding.IViewFor<SharedViewModel>
                {
                    public SharedViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (SharedViewModel)value;
                    }
                }

                public class MobileView : ReactiveUI.Binding.IViewFor<SharedViewModel>
                {
                    public SharedViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (SharedViewModel)value;
                    }
                }
            }
            """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }
}

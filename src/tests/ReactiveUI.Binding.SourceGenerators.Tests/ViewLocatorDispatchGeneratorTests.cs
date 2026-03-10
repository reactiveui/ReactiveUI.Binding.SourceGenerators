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

    /// <summary>
    /// Verifies that a view marked with [ExcludeFromViewRegistration] is not included in dispatch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task ExcludedViewIsSkipped()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class ExcludedViewModel : INotifyPropertyChanged
                {
                    public string Name { get; set; }
                    public event PropertyChangedEventHandler PropertyChanged;
                }

                [ReactiveUI.Binding.ExcludeFromViewRegistration]
                public class ExcludedView : ReactiveUI.Binding.IViewFor<ExcludedViewModel>
                {
                    public ExcludedViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (ExcludedViewModel)value;
                    }
                }
            }
            """;

        // Excluded views should not produce ViewDispatch.g.cs
        var result = TestHelper.RunGenerator(source);
        return Verify(result.Driver)
            .UseTypeName("VDG")
            .UseMethodName("ExclAttr");
    }

    /// <summary>
    /// Verifies that a view marked with [SingleInstanceView] generates singleton dispatch code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task SingleInstanceViewGeneratesSingletonCache()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class SingletonViewModel : INotifyPropertyChanged
                {
                    public string Name { get; set; }
                    public event PropertyChangedEventHandler PropertyChanged;
                }

                [ReactiveUI.Binding.SingleInstanceView]
                public class SingletonView : ReactiveUI.Binding.IViewFor<SingletonViewModel>
                {
                    public SingletonViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (SingletonViewModel)value;
                    }
                }
            }
            """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>
    /// Verifies that a view marked with [ViewContract] generates contract-aware dispatch code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task ViewContractGeneratesContractDispatch()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class ContractViewModel : INotifyPropertyChanged
                {
                    public string Name { get; set; }
                    public event PropertyChangedEventHandler PropertyChanged;
                }

                [ReactiveUI.Binding.ViewContract("compact")]
                public class CompactView : ReactiveUI.Binding.IViewFor<ContractViewModel>
                {
                    public ContractViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (ContractViewModel)value;
                    }
                }
            }
            """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>
    /// Verifies that when a ViewModel has both a default view and a contract view,
    /// the contract-specific check is emitted first so it is not shadowed by the default branch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task DefaultAndContractViewsDispatchCorrectly()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class DashboardViewModel : INotifyPropertyChanged
                {
                    public string Title { get; set; }
                    public event PropertyChangedEventHandler PropertyChanged;
                }

                public class DashboardView : ReactiveUI.Binding.IViewFor<DashboardViewModel>
                {
                    public DashboardViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (DashboardViewModel)value;
                    }
                }

                [ReactiveUI.Binding.ViewContract("compact")]
                public class CompactDashboardView : ReactiveUI.Binding.IViewFor<DashboardViewModel>
                {
                    public DashboardViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (DashboardViewModel)value;
                    }
                }
            }
            """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>
    /// Verifies that when a ViewModel has multiple contract views but no default view,
    /// the dispatch block contains only contract checks with no default fallback return.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task MultipleContractViewsWithoutDefault()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class ThemeViewModel : INotifyPropertyChanged
                {
                    public string Name { get; set; }
                    public event PropertyChangedEventHandler PropertyChanged;
                }

                [ReactiveUI.Binding.ViewContract("light")]
                public class LightThemeView : ReactiveUI.Binding.IViewFor<ThemeViewModel>
                {
                    public ThemeViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (ThemeViewModel)value;
                    }
                }

                [ReactiveUI.Binding.ViewContract("dark")]
                public class DarkThemeView : ReactiveUI.Binding.IViewFor<ThemeViewModel>
                {
                    public ThemeViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (ThemeViewModel)value;
                    }
                }
            }
            """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>
    /// Verifies that [SingleInstanceView] on a view without parameterless constructor
    /// generates service-locator-only dispatch (no singleton cache field).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task SingleInstanceViewWithoutParameterlessCtor()
    {
        const string source = """
            using System.ComponentModel;

            namespace TestApp
            {
                public class NoCtorSingletonViewModel : INotifyPropertyChanged
                {
                    public string Name { get; set; }
                    public event PropertyChangedEventHandler PropertyChanged;
                }

                [ReactiveUI.Binding.SingleInstanceView]
                public class NoCtorSingletonView : ReactiveUI.Binding.IViewFor<NoCtorSingletonViewModel>
                {
                    private readonly string _cfg;
                    public NoCtorSingletonView(string cfg) { _cfg = cfg; }

                    public NoCtorSingletonViewModel ViewModel { get; set; }
                    object ReactiveUI.Binding.IViewFor.ViewModel
                    {
                        get => ViewModel;
                        set => ViewModel = (NoCtorSingletonViewModel)value;
                    }
                }
            }
            """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }
}

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Covers matching a binding target's type to the runtime invoker a generated binding routes through.</summary>
public class ViewThreadPluginRegistryTests
{
    /// <summary>The WPF runtime invoker.</summary>
    private const string WpfInvoker = "global::ReactiveUI.Binding.Wpf.DispatcherViewThreadInvoker";

    /// <summary>The metadata name of a WPF view.</summary>
    private const string WpfView = "TestApp.WpfView";

    /// <summary>The metadata name of a type from no platform.</summary>
    private const string PlainViewModel = "TestApp.PlainViewModel";

    /// <summary>
    /// A compilation that declares one type from each platform, plus a type from none, with each platform's runtime
    /// invoker. The WinForms invoker is the System.Reactive flavour's, so either flavour's invoker counts.
    /// </summary>
    private const string PlatformSource = """
                                          namespace System.Windows.Threading
                                          {
                                              public class DispatcherObject { }
                                          }

                                          namespace System.Windows.Forms
                                          {
                                              public class Control { }
                                          }

                                          namespace Microsoft.Maui.Controls
                                          {
                                              public class BindableObject { }
                                          }

                                          namespace ReactiveUI.Binding.Wpf
                                          {
                                              public sealed class DispatcherViewThreadInvoker { }
                                          }

                                          namespace ReactiveUI.Binding.Reactive.WinForms
                                          {
                                              public sealed class ControlViewThreadInvoker { }
                                          }

                                          namespace ReactiveUI.Binding.Maui
                                          {
                                              public sealed class DispatcherViewThreadInvoker { }
                                          }

                                          namespace TestApp
                                          {
                                              public class WpfView : System.Windows.Threading.DispatcherObject { }

                                              public class DerivedWpfView : WpfView { }

                                              public class WinFormsView : System.Windows.Forms.Control { }

                                              public class MauiView : Microsoft.Maui.Controls.BindableObject { }

                                              public class PlainViewModel { }
                                          }
                                          """;

    /// <summary>A compilation that references WPF but not the runtime package carrying its invoker.</summary>
    private const string PlatformWithoutRuntimeSource = """
                                                        namespace System.Windows.Threading
                                                        {
                                                            public class DispatcherObject { }
                                                        }

                                                        namespace TestApp
                                                        {
                                                            public class WpfView : System.Windows.Threading.DispatcherObject { }
                                                        }
                                                        """;

    /// <summary>A compilation that references no platform.</summary>
    private const string PlainSource = """
                                       namespace TestApp
                                       {
                                           public class PlainViewModel { }
                                       }
                                       """;

    /// <summary>
    /// A type from a platform, directly or through a base class, names that platform's runtime invoker in the flavour
    /// the compilation references.
    /// </summary>
    /// <param name="metadataName">The type to look up.</param>
    /// <param name="expected">The runtime invoker it should route through.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(WpfView, WpfInvoker)]
    [Arguments("TestApp.DerivedWpfView", WpfInvoker)]
    [Arguments("TestApp.WinFormsView", "global::ReactiveUI.Binding.Reactive.WinForms.ControlViewThreadInvoker")]
    [Arguments("TestApp.MauiView", "global::ReactiveUI.Binding.Maui.DispatcherViewThreadInvoker")]
    public async Task InvokerFor_WithATypeFromAPlatform_NamesItsInvoker(string metadataName, string expected)
    {
        var compilation = TestHelper.CreateCompilation(PlatformSource);

        await Assert.That(ViewThreadPluginRegistry.InvokerFor(compilation.GetTypeByMetadataName(metadataName), compilation))
            .IsEqualTo(expected);
    }

    /// <summary>A type from no platform carries no invoker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokerFor_WithATypeFromNoPlatform_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation(PlatformSource);

        await Assert.That(ViewThreadPluginRegistry.InvokerFor(compilation.GetTypeByMetadataName(PlainViewModel), compilation))
            .IsNull();
    }

    /// <summary>With no type there is nothing to match.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokerFor_WithNoType_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation(PlatformSource);

        await Assert.That(ViewThreadPluginRegistry.InvokerFor(null, compilation)).IsNull();
    }

    /// <summary>In a compilation that references no platform, no type carries an invoker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokerFor_WhenTheCompilationReferencesNoPlatform_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation(PlainSource);

        await Assert.That(ViewThreadPluginRegistry.InvokerFor(compilation.GetTypeByMetadataName(PlainViewModel), compilation))
            .IsNull();
    }

    /// <summary>A platform type whose runtime invoker is out of reach names the package that ships it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MissingPackageFor_WhenTheRuntimeInvokerIsMissing_NamesThePackage()
    {
        var compilation = TestHelper.CreateCompilation(PlatformWithoutRuntimeSource);

        await Assert.That(ViewThreadPluginRegistry.MissingPackageFor(compilation.GetTypeByMetadataName(WpfView), compilation))
            .IsEqualTo("ReactiveUI.Binding.Wpf");
    }

    /// <summary>Nothing is missing for a platform whose invoker resolves, for a type from no platform, or for no type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MissingPackageFor_WhenNothingIsMissing_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation(PlatformSource);

        await Assert.That(ViewThreadPluginRegistry.MissingPackageFor(compilation.GetTypeByMetadataName(WpfView), compilation)).IsNull();
        await Assert.That(ViewThreadPluginRegistry.MissingPackageFor(compilation.GetTypeByMetadataName(PlainViewModel), compilation)).IsNull();
        await Assert.That(ViewThreadPluginRegistry.MissingPackageFor(null, compilation)).IsNull();
    }

    /// <summary>A platform type whose runtime invoker is out of reach carries no invoker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokerFor_WhenTheRuntimeInvokerIsMissing_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation(PlatformWithoutRuntimeSource);

        await Assert.That(ViewThreadPluginRegistry.InvokerFor(compilation.GetTypeByMetadataName(WpfView), compilation))
            .IsNull();
    }
}

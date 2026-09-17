// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Covers matching a binding target's type to the invoker a generated binding carries.</summary>
public class ViewThreadPluginRegistryTests
{
    /// <summary>The WPF invoker class name.</summary>
    private const string WpfInvoker = "__WpfViewThreadInvoker";

    /// <summary>The MAUI invoker class name.</summary>
    private const string MauiInvoker = "__MauiViewThreadInvoker";

    /// <summary>A compilation that declares one type from each platform, plus a type from none.</summary>
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

                                          namespace TestApp
                                          {
                                              public class WpfView : System.Windows.Threading.DispatcherObject { }

                                              public class DerivedWpfView : WpfView { }

                                              public class WinFormsView : System.Windows.Forms.Control { }

                                              public class MauiView : Microsoft.Maui.Controls.BindableObject { }

                                              public class PlainViewModel { }
                                          }
                                          """;

    /// <summary>A compilation that references no platform.</summary>
    private const string PlainSource = """
                                       namespace TestApp
                                       {
                                           public class PlainViewModel { }
                                       }
                                       """;

    /// <summary>A type from a platform, directly or through a base class, names that platform's invoker.</summary>
    /// <param name="metadataName">The type to look up.</param>
    /// <param name="expected">The invoker class name it should carry.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("TestApp.WpfView", WpfInvoker)]
    [Arguments("TestApp.DerivedWpfView", WpfInvoker)]
    [Arguments("TestApp.WinFormsView", "__WinFormsViewThreadInvoker")]
    [Arguments("TestApp.MauiView", MauiInvoker)]
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

        await Assert.That(ViewThreadPluginRegistry.InvokerFor(compilation.GetTypeByMetadataName("TestApp.PlainViewModel"), compilation))
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

        await Assert.That(ViewThreadPluginRegistry.InvokerFor(compilation.GetTypeByMetadataName("TestApp.PlainViewModel"), compilation))
            .IsNull();
    }

    /// <summary>Each platform the compilation references gets an invoker, in plugin order.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokersIn_ListsEachPlatformTheCompilationReferences()
    {
        var compilation = TestHelper.CreateCompilation(PlatformSource);

        await Assert.That(string.Join(",", ViewThreadPluginRegistry.InvokersIn(compilation)))
            .IsEqualTo("__WpfViewThreadInvoker,__WinFormsViewThreadInvoker,__MauiViewThreadInvoker");
    }

    /// <summary>A compilation that references no platform gets no invokers.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokersIn_WhenTheCompilationReferencesNoPlatform_ListsNothing()
    {
        var compilation = TestHelper.CreateCompilation(PlainSource);

        await Assert.That(ViewThreadPluginRegistry.InvokersIn(compilation).Length).IsEqualTo(0);
    }

    /// <summary>Only the named invokers are declared.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInvokers_DeclaresOnlyTheNamedInvokers()
    {
        var sb = new StringBuilder();

        ViewThreadPluginRegistry.EmitInvokers(sb, new([MauiInvoker]), supportsNullable: true);

        var emitted = sb.ToString();
        await Assert.That(emitted).Contains($"class {MauiInvoker}");
        await Assert.That(emitted).DoesNotContain($"class {WpfInvoker}");
    }

    /// <summary>With nullable reference types the callback and its state are annotated.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInvokers_WithNullableReferenceTypes_AnnotatesTheState()
    {
        var sb = new StringBuilder();

        ViewThreadPluginRegistry.EmitInvokers(sb, new([WpfInvoker]), supportsNullable: true);

        await Assert.That(sb.ToString()).Contains("global::System.Action<object?> callback, object? state");
    }

    /// <summary>Below C# 8 nothing is annotated.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitInvokers_BelowCSharp8_AnnotatesNothing()
    {
        var sb = new StringBuilder();

        ViewThreadPluginRegistry.EmitInvokers(sb, new([WpfInvoker, MauiInvoker]), supportsNullable: false);

        await Assert.That(sb.ToString()).DoesNotContain("?");
    }
}

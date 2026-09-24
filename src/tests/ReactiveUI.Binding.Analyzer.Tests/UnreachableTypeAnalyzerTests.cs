// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="UnreachableTypeAnalyzer"/>, which reports RXUIBIND015.</summary>
public class UnreachableTypeAnalyzerTests
{
    /// <summary>The diagnostic reported for a type generated code cannot reach.</summary>
    private const string UnreachableId = "RXUIBIND015";

    /// <summary>The runtime stub the analyzer recognizes, with a safe and an Unsafe entry point.</summary>
    private const string Preamble = """
                                    using System;
                                    using System.Collections.Generic;
                                    using System.ComponentModel;
                                    using System.Linq.Expressions;
                                    using ReactiveUI.Binding;

                                    namespace ReactiveUI.Binding
                                    {
                                        public static class ReactiveUIBindingExtensions
                                        {
                                            public static IObservable<TReturn> WhenChanged<TObj, TReturn>(
                                                this TObj objectToMonitor,
                                                Expression<Func<TObj, TReturn>> property)
                                                where TObj : class
                                                => throw new InvalidOperationException();

                                            public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
                                                this TSender sender,
                                                Expression property,
                                                Func<object, TRet> selector)
                                                => throw new InvalidOperationException();

                                            public static IObservable<TReturn> WhenChangedUnsafe<TObj, TReturn>(
                                                this TObj objectToMonitor,
                                                Expression<Func<TObj, TReturn>> property)
                                                where TObj : class
                                                => throw new InvalidOperationException();
                                        }
                                    }

                                    """;

    /// <summary>A private nested receiver is out of reach, so the call is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PrivateNestedReceiver_Reported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public static class Outer
                                             {
                                                 public static void Run() => new Vm().WhenChanged(x => x.Name);

                                                 private sealed class Vm : INotifyPropertyChanged
                                                 {
                                                     public event PropertyChangedEventHandler PropertyChanged;
                                                     public string Name { get; set; }
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].GetMessage()).Contains("Outer.Vm");
    }

    /// <summary>A protected nested value type reached through the path is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ProtectedNestedValueType_Reported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public class Host : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler PropertyChanged;

                                                 private Inner Child { get; set; }

                                                 public void Run() => this.WhenChanged(x => x.Child);

                                                 protected sealed class Inner
                                                 {
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
    }

    /// <summary>A protected internal nested type is reachable from the assembly, so nothing is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ProtectedInternalNestedValueType_NotReported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public class Host : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler PropertyChanged;

                                                 internal Inner Child { get; set; }

                                                 public void Run() => this.WhenChanged(x => x.Child);

                                                 protected internal sealed class Inner
                                                 {
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>A generic closed over a private nested type is out of reach.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenericClosedOverPrivateType_Reported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public class Host : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler PropertyChanged;

                                                 private List<Item> Items { get; set; }

                                                 public void Run() => this.WhenChanged(x => x.Items);

                                                 private sealed class Item
                                                 {
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
    }

    /// <summary>A path whose intermediate link has a private owner type is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PrivateIntermediateLink_Reported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public class Host : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler PropertyChanged;

                                                 private Inner Child { get; set; }

                                                 public void Run() => this.WhenChanged(x => x.Child.Name);

                                                 private sealed class Inner
                                                 {
                                                     public string Name { get; set; }
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].GetMessage()).Contains("Host.Inner");
    }

    /// <summary>An internal nested type is reachable, so nothing is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InternalNestedReceiver_NotReported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public static class Outer
                                             {
                                                 public static void Run() => new Vm().WhenChanged(x => x.Name);

                                                 internal sealed class Vm : INotifyPropertyChanged
                                                 {
                                                     public event PropertyChangedEventHandler PropertyChanged;
                                                     public string Name { get; set; }
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>An Unsafe call resolves its types at run time, so a private type is not reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UnsafeCall_NotReported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public static class Outer
                                             {
                                                 public static void Run() => new Vm().WhenChangedUnsafe(x => x.Name);

                                                 private sealed class Vm : INotifyPropertyChanged
                                                 {
                                                     public event PropertyChangedEventHandler PropertyChanged;
                                                     public string Name { get; set; }
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>A runtime-only API such as WhenAnyDynamic generates nothing either way, so it is not reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RuntimeOnlyApi_NotReported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public static class Outer
                                             {
                                                 public static void Run(Expression path) => new Vm().WhenAnyDynamic(path, static x => x);

                                                 private sealed class Vm
                                                 {
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>A call on a method that is not a binding entry point is ignored.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UnrelatedCall_NotReported()
    {
        const string Source = Preamble + """
                                         namespace TestApp
                                         {
                                             public static class Outer
                                             {
                                                 public static void Run() => Use(new Vm());

                                                 private static void Use<T>(T value)
                                                 {
                                                 }

                                                 private sealed class Vm
                                                 {
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await GetDiagnosticsAsync(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Runs the analyzer and keeps its own diagnostic.</summary>
    /// <param name="source">The source to analyze.</param>
    /// <returns>The RXUIBIND015 diagnostics.</returns>
    private static async Task<Microsoft.CodeAnalysis.Diagnostic[]> GetDiagnosticsAsync(string source)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<UnreachableTypeAnalyzer>(source);
        return [.. diagnostics.Where(static d => d.Id == UnreachableId)];
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>
/// Tests for <see cref="TypeAnalyzer"/>.
/// </summary>
public class TypeAnalyzerTests
{
    /// <summary>
    /// Common using directives and the generated stub class that the analyzer recognizes.
    /// </summary>
    private const string Preamble = """
        using System;
        using System.ComponentModel;
        using System.Linq.Expressions;

        namespace ReactiveUI.Binding
        {
            public static class __ReactiveUIGeneratedBindings
            {
                public static object WhenChanged<TObj, TReturn>(
                    this TObj obj,
                    Expression<Func<TObj, TReturn>> property)
                    where TObj : class
                    => throw new NotImplementedException();
            }
        }
        """;

    /// <summary>
    /// Verifies RXUIBIND002 is reported when the source type does not implement any observable mechanism.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_NoObservableMechanism_ReportsDiagnostic()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class PlainObject
                {
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var obj = new PlainObject();
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(obj, x => x.Name);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Id).IsEqualTo("RXUIBIND002");
    }

    /// <summary>
    /// Verifies RXUIBIND002 is NOT reported when the source type implements INotifyPropertyChanged.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_INPC_NoDiagnostic()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new MyViewModel();
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND002 is NOT reported for empty source code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_EmptySource_NoDiagnostics()
    {
        const string source = """
            namespace TestApp
            {
                public class EmptyClass { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }
}

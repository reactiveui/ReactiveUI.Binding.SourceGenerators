// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>
/// Tests for <see cref="BindingInvocationAnalyzer"/> — RXUIBIND006 (unsupported path segment).
/// </summary>
public partial class BindingInvocationAnalyzerTests
{
    /// <summary>
    /// Verifies RXUIBIND006 is reported when a property path contains an indexer.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_Indexer_InPropertyPath_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public System.Collections.Generic.List<string> Items { get; set; } = new();
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Items[0]);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported when a property path contains a field access.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_FieldAccess_InPropertyPath_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string _name = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x._name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported when a property path contains a method call.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_MethodCall_InPropertyPath_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string GetName() => "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.GetName());
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is NOT reported for simple property chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_SimplePropertyChain_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class Address : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string City { get; set; } = "";
                                             }

                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public Address Address { get; set; } = new();
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Address.City);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is NOT reported when a block-body lambda is used,
    /// because the analyzer only walks expression-body lambdas for unsupported path segments.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_BlockBodyLambda_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string _name = "";

                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => { return x._name; });
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported when a property path contains a method call
    /// in the middle of a chain (e.g., x =&gt; x.GetValue().Name).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_MethodCallInChain_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class Inner : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string Name { get; set; } = "";
                                             }

                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public Inner GetValue() => new Inner();
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.GetValue().Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported when a property path contains an indexer
    /// in the middle of a chain (e.g., x =&gt; x.Items[0].Name).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_IndexerInChain_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class Inner : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string Name { get; set; } = "";
                                             }

                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public System.Collections.Generic.List<Inner> Items { get; set; } = new();
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Items[0].Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported for BindOneWay when source property path
    /// contains a method call.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_BindOneWay_MethodCallInPath_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string GetName() => "";
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindOneWay(vm, view, x => x.GetName(), x => x.NameText);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported for BindTwoWay when target property path
    /// contains a field access.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_BindTwoWay_FieldInTargetPath_ReportsDiagnostic()
    {
        const string Source = Preamble + """

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
                                                 public string _nameText = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     var view = new MyView();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTwoWay(vm, view, x => x.Name, x => x._nameText);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported for a field access in the middle of a property chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_FieldInChain_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class Inner : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string Name { get; set; } = "";
                                             }

                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public Inner _inner = new();
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x._inner.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported for WhenChanging with a method call in the property path.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_WhenChanging_MethodCallInPath_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged, INotifyPropertyChanging
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public event PropertyChangingEventHandler? PropertyChanging;
                                                 public string GetName() => "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.GetName());
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported for WhenChanging with a field access in the property path.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_WhenChanging_FieldAccess_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged, INotifyPropertyChanging
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public event PropertyChangingEventHandler? PropertyChanging;
                                                 public string _name = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x._name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported for an indexer access in a BindOneWay source lambda.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_BindOneWay_IndexerInPath_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public System.Collections.Generic.List<string> Items { get; set; } = new();
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindOneWay(vm, view, x => x.Items[0], x => x.NameText);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that a parenthesized lambda with block body does not report RXUIBIND006
    /// for unsupported path segments.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_ParenthesizedBlockBodyLambda_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string _name = "";

                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, (MyViewModel x) => { return x._name; });
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that a const field access in a property path does NOT report RXUIBIND006.
    /// The WalkForUnsupportedSegments method specifically excludes const fields from the
    /// field check (only non-const fields are flagged).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_ConstFieldAccess_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public const string DefaultName = "test";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.DefaultName);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();

        // Const field is excluded from the field check — no diagnostic
        await Assert.That(unsupportedDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that when WhenChanged is invoked with an identity lambda (just the parameter),
    /// the WalkForUnsupportedSegments exits cleanly at the terminal IdentifierNameSyntax.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_IdentityLambda_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported when a BindOneWay target property path contains a field
    /// inside the target lambda. Both source and target lambdas are checked for unsupported segments.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_BindOneWay_FieldInTargetPath_ReportsDiagnostic()
    {
        const string Source = Preamble + """

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
                                                 public string _nameText = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     var view = new MyView();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindOneWay(vm, view, x => x.Name, x => x._nameText);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND006 reports a field access through a parenthesized lambda expression.
    /// This exercises the ParenthesizedLambdaExpressionSyntax branch in CheckUnsupportedPathSegment.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_ParenthesizedLambda_FieldAccess_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string _name = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, (MyViewModel x) => x._name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported for BindTo when the target property path contains a method call.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_BindTo_MethodCallInPath_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyView : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string GetName() => "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     IObservable<string> source = null!;
                                                     var view = new MyView();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTo(source, view, x => x.GetName());
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }
}

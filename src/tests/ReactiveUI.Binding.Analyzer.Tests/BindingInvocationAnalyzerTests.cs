// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>
/// Tests for <see cref="BindingInvocationAnalyzer"/>.
/// </summary>
public class BindingInvocationAnalyzerTests
{
    /// <summary>
    /// Common using directives and the generated stub class that the analyzer recognizes.
    /// The analyzer checks for methods on a class named __ReactiveUIGeneratedBindings.
    /// </summary>
    private const string Preamble = """
        using System;
        using System.Collections;
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

                public static object WhenChanging<TObj, TReturn>(
                    this TObj obj,
                    Expression<Func<TObj, TReturn>> property)
                    where TObj : class
                    => throw new NotImplementedException();

                public static object BindOneWay<TSource, TTarget, TValue>(
                    this TSource source,
                    TTarget target,
                    Expression<Func<TSource, TValue>> sourceProperty,
                    Expression<Func<TTarget, TValue>> targetProperty)
                    where TSource : class
                    where TTarget : class
                    => throw new NotImplementedException();

                public static object BindTwoWay<TSource, TTarget, TValue>(
                    this TSource source,
                    TTarget target,
                    Expression<Func<TSource, TValue>> sourceProperty,
                    Expression<Func<TTarget, TValue>> targetProperty)
                    where TSource : class
                    where TTarget : class
                    => throw new NotImplementedException();
            }
        }
        """;

    /// <summary>
    /// Placeholder test to verify the analyzer test infrastructure works.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmptySource_NoDiagnostics()
    {
        const string source = """
            namespace TestApp
            {
                public class EmptyClass { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND001 is reported when a non-inline lambda (variable reference) is passed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND001_NonInlineLambda_Variable_ReportsDiagnostic()
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
                        Expression<Func<MyViewModel, string>> expr = x => x.Name;
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, expr);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Id).IsEqualTo("RXUIBIND001");
    }

    /// <summary>
    /// Verifies RXUIBIND001 is NOT reported when an inline lambda is passed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND001_InlineLambda_NoDiagnostic()
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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND001 is reported for WhenChanging with non-inline lambda.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND001_WhenChanging_NonInlineLambda_ReportsDiagnostic()
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
                        Expression<Func<MyViewModel, string>> expr = x => x.Name;
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, expr);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);

        // Filter to only RXUIBIND001 (WhenChanging on INPC-only also fires RXUIBIND004)
        var nonInlineDiags = diagnostics.Where(d => d.Id == "RXUIBIND001").ToArray();
        await Assert.That(nonInlineDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND001 is reported for BindOneWay with non-inline source lambda.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND001_BindOneWay_NonInlineLambda_ReportsDiagnostic()
    {
        var source = Preamble + """

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
                        Expression<Func<MyViewModel, string>> expr = x => x.Name;
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindOneWay(vm, view, expr, x => x.NameText);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var nonInlineDiags = diagnostics.Where(d => d.Id == "RXUIBIND001").ToArray();
        await Assert.That(nonInlineDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND003 is reported when accessing a private property in a lambda.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_PrivateProperty_ReportsDiagnostic()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    private string Secret { get; set; } = "";

                    public void Test()
                    {
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, x => x.Secret);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var privateDiags = diagnostics.Where(d => d.Id == "RXUIBIND003").ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND003 is reported when accessing a protected property in a lambda.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_ProtectedProperty_ReportsDiagnostic()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    protected string InternalName { get; set; } = "";
                }

                public class Usage : MyViewModel
                {
                    public void Test()
                    {
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, x => x.InternalName);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var privateDiags = diagnostics.Where(d => d.Id == "RXUIBIND003").ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND003 is NOT reported for public property access.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_PublicProperty_NoDiagnostic()
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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var privateMemberDiags = diagnostics.Where(d => d.Id == "RXUIBIND003").ToArray();
        await Assert.That(privateMemberDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND004 is reported when WhenChanging is called on a type
    /// that only implements INotifyPropertyChanged (no INotifyPropertyChanging).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_INPCOnly_ReportsDiagnostic()
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
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var beforeChangeDiags = diagnostics.Where(d => d.Id == "RXUIBIND004").ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND004 is NOT reported when WhenChanging is called on a type
    /// that implements INotifyPropertyChanging.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_INPChanging_NoDiagnostic()
    {
        var source = Preamble + """

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
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var beforeChangeDiags = diagnostics.Where(d => d.Id == "RXUIBIND004").ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND004 is NOT reported for WhenChanged (only applies to WhenChanging).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_WhenChanged_NoDiagnostic()
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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var beforeChangeDiags = diagnostics.Where(d => d.Id == "RXUIBIND004").ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND005 is reported when BindOneWay source type implements INotifyDataErrorInfo.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_BindOneWay_WithDataErrorInfo_ReportsDiagnostic()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
                    public bool HasErrors => false;
                    public IEnumerable GetErrors(string? propertyName) => Array.Empty<string>();
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
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindOneWay(vm, view, x => x.Name, x => x.NameText);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var validationDiags = diagnostics.Where(d => d.Id == "RXUIBIND005").ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND005 is reported for BindTwoWay when source implements INotifyDataErrorInfo.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_BindTwoWay_WithDataErrorInfo_ReportsDiagnostic()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
                    public bool HasErrors => false;
                    public IEnumerable GetErrors(string? propertyName) => Array.Empty<string>();
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
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTwoWay(vm, view, x => x.Name, x => x.NameText);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var validationDiags = diagnostics.Where(d => d.Id == "RXUIBIND005").ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND005 is NOT reported when source type does not implement INotifyDataErrorInfo.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_NoDataErrorInfo_NoDiagnostic()
    {
        var source = Preamble + """

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
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindOneWay(vm, view, x => x.Name, x => x.NameText);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var validationDiags = diagnostics.Where(d => d.Id == "RXUIBIND005").ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND005 is NOT reported for WhenChanged (only applies to binding methods).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_WhenChanged_NoDiagnostic()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
                    public bool HasErrors => false;
                    public IEnumerable GetErrors(string? propertyName) => Array.Empty<string>();
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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var validationDiags = diagnostics.Where(d => d.Id == "RXUIBIND005").ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that non-binding methods do not trigger any diagnostics.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NonBindingMethod_NoDiagnostics()
    {
        const string source = """
            using System;
            using System.ComponentModel;

            namespace TestApp
            {
                public static class SomeExtensions
                {
                    public static void DoSomething<T>(this T obj) { }
                }

                public class MyClass : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string Name { get; set; } = "";

                    public void Test()
                    {
                        this.DoSomething();
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported when a property path contains an indexer.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_Indexer_InPropertyPath_ReportsDiagnostic()
    {
        var source = Preamble + """

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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
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
        var source = Preamble + """

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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
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
        var source = Preamble + """

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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
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
        var source = Preamble + """

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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that RXUIBIND001 is reported for non-inline lambda but RXUIBIND003 is not
    /// (since the analyzer only walks inline lambda bodies for private member access).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleDiagnostics_NonInlineLambda_NoPrivateMemberCheck()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    private string Secret { get; set; } = "";

                    public void Test()
                    {
                        Expression<Func<MyViewModel, string>> expr = x => x.Secret;
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, expr);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);

        // RXUIBIND001 for non-inline lambda
        var nonInlineDiags = diagnostics.Where(d => d.Id == "RXUIBIND001").ToArray();
        await Assert.That(nonInlineDiags.Length).IsEqualTo(1);

        // RXUIBIND003 should NOT be reported because the lambda is not inline
        var privateDiags = diagnostics.Where(d => d.Id == "RXUIBIND003").ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(0);
    }
}

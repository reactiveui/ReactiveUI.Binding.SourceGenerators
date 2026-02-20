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

    /// <summary>
    /// Verifies RXUIBIND004 is NOT reported when WhenChanging is called on a type
    /// that implements IReactiveObject (supports before-change notifications).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_IReactiveObject_NoDiagnostic()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class ReactiveViewModel : ReactiveUI.IReactiveObject
                {
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new ReactiveViewModel();
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
    /// Verifies RXUIBIND004 is reported when WhenChanging is called on a WPF DependencyObject type
    /// (WPF does not support before-change notifications).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_WpfDependencyObject_ReportsDiagnostic()
    {
        var source = Preamble + """

            namespace System.Windows
            {
                public class DependencyObject { }
            }

            namespace TestApp
            {
                public class WpfControl : System.Windows.DependencyObject
                {
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new WpfControl();
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
    /// Verifies RXUIBIND004 is reported when WhenChanging is called on a WinUI DependencyObject type
    /// (WinUI does not support before-change notifications).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_WinUIDependencyObject_ReportsDiagnostic()
    {
        var source = Preamble + """

            namespace Microsoft.UI.Xaml
            {
                public class DependencyObject { }
            }

            namespace TestApp
            {
                public class WinUIControl : Microsoft.UI.Xaml.DependencyObject
                {
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new WinUIControl();
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
    /// Verifies RXUIBIND004 is reported when WhenChanging is called on a WinForms Component type
    /// (WinForms does not support before-change notifications).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_WinFormsComponent_ReportsDiagnostic()
    {
        var source = Preamble + """

            namespace System.ComponentModel
            {
                public class Component { }
            }

            namespace TestApp
            {
                public class WinFormsControl : System.ComponentModel.Component
                {
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new WinFormsControl();
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
    /// Verifies RXUIBIND004 is reported when WhenChanging is called on an Android View type
    /// (Android does not support before-change notifications).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_AndroidView_ReportsDiagnostic()
    {
        var source = Preamble + """

            namespace Android.Views
            {
                public class View { }
            }

            namespace TestApp
            {
                public class AndroidControl : Android.Views.View
                {
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new AndroidControl();
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
    /// Verifies RXUIBIND004 is NOT reported when WhenChanging is called on an Apple NSObject type
    /// (KVO supports before-change notifications).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_KVO_NSObject_NoDiagnostic()
    {
        var source = Preamble + """

            namespace Foundation
            {
                public class NSObject { }
            }

            namespace TestApp
            {
                public class AppleView : Foundation.NSObject
                {
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new AppleView();
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
    /// Verifies RXUIBIND004 is reported when WhenChanging is called on a type with no
    /// recognized notification mechanism (unknown type falls through to no before-change support).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_UnknownType_ReportsDiagnostic()
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
                        var vm = new PlainObject();
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
    /// Verifies RXUIBIND004 is reported when WhenChanging is called on a type that inherits
    /// from a platform type through a deep inheritance chain (e.g., two levels deep from
    /// WPF DependencyObject). Tests the InheritsFrom base-type walk at line 132.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_InheritsFrom_DeepChain_ReportsDiagnostic()
    {
        var source = Preamble + """

            namespace System.Windows
            {
                public class DependencyObject { }
            }

            namespace TestApp
            {
                public class WpfControlBase : System.Windows.DependencyObject
                {
                    public string BaseName { get; set; } = "";
                }

                public class WpfDerivedControl : WpfControlBase
                {
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new WpfDerivedControl();
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
    /// Verifies RXUIBIND003 is NOT reported when a block-body lambda is used,
    /// because the analyzer only walks expression-body lambdas for private member access.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_BlockBodyLambda_NoDiagnostic()
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
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, x => { return x.Secret; });
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var privateDiags = diagnostics.Where(d => d.Id == "RXUIBIND003").ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is NOT reported when a block-body lambda is used,
    /// because the analyzer only walks expression-body lambdas for unsupported path segments.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_BlockBodyLambda_NoDiagnostic()
    {
        var source = Preamble + """

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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
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
        var source = Preamble + """

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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
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
        var source = Preamble + """

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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
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
        var source = Preamble + """

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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND003 is reported when accessing a private property in a WhenChanging lambda.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_WhenChanging_PrivateProperty_ReportsDiagnostic()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged, INotifyPropertyChanging
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public event PropertyChangingEventHandler? PropertyChanging;
                    private string Secret { get; set; } = "";

                    public void Test()
                    {
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(this, x => x.Secret);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var privateDiags = diagnostics.Where(d => d.Id == "RXUIBIND003").ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND003 is reported when accessing a private property in a BindOneWay source lambda.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_BindOneWay_PrivateProperty_ReportsDiagnostic()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    private string Secret { get; set; } = "";

                    public MyView View { get; set; } = new();

                    public void Test()
                    {
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindOneWay(this, View, x => x.Secret, x => x.NameText);
                    }
                }

                public class MyView : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string NameText { get; set; } = "";
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var privateDiags = diagnostics.Where(d => d.Id == "RXUIBIND003").ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported for a field access in the middle of a property chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_FieldInChain_ReportsDiagnostic()
    {
        var source = Preamble + """

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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND001 is reported for BindTwoWay with non-inline target lambda.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND001_BindTwoWay_NonInlineLambda_ReportsDiagnostic()
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
                        Expression<Func<MyView, string>> targetExpr = x => x.NameText;
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTwoWay(vm, view, x => x.Name, targetExpr);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var nonInlineDiags = diagnostics.Where(d => d.Id == "RXUIBIND001").ToArray();
        await Assert.That(nonInlineDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported for WhenChanging with a method call in the property path.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_WhenChanging_MethodCallInPath_ReportsDiagnostic()
    {
        var source = Preamble + """

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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND003 with block-body lambda does not fire for BindTwoWay.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_BindTwoWay_BlockBodyLambda_NoDiagnostic()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    private string Secret { get; set; } = "";

                    public MyView View { get; set; } = new();

                    public void Test()
                    {
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTwoWay(this, View, x => { return x.Secret; }, x => x.NameText);
                    }
                }

                public class MyView : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public string NameText { get; set; } = "";
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var privateDiags = diagnostics.Where(d => d.Id == "RXUIBIND003").ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND006 is reported for WhenChanging with a field access in the property path.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_WhenChanging_FieldAccess_ReportsDiagnostic()
    {
        var source = Preamble + """

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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
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
        var source = Preamble + """

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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that a parenthesized lambda with block body does not report RXUIBIND003
    /// for private members.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_ParenthesizedBlockBodyLambda_NoDiagnostic()
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
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, (MyViewModel x) => { return x.Secret; });
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var privateDiags = diagnostics.Where(d => d.Id == "RXUIBIND003").ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that a parenthesized lambda with block body does not report RXUIBIND006
    /// for unsupported path segments.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_ParenthesizedBlockBodyLambda_NoDiagnostic()
    {
        var source = Preamble + """

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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that a regular (non-extension) method call does not trigger any diagnostics.
    /// This exercises the IsBindingExtensionMethod false branch where the method is not
    /// from __ReactiveUIGeneratedBindings.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegularMethodCall_NotExtensionMethod_NoDiagnostics()
    {
        const string source = """
            using System;

            namespace TestApp
            {
                public class MyClass
                {
                    public string Name { get; set; } = "";

                    public void Test()
                    {
                        Console.WriteLine(Name);
                        Name.ToString();
                        var len = Name.Length;
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that a block-body lambda (simple lambda with block body) exercises the
    /// body == null branch in CheckNonInlineLambda. A block lambda is still an inline lambda
    /// so RXUIBIND001 should NOT be reported, but the expression body extraction returns null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BlockBodyLambda_IsInlineLambda_NoDiagnosticForRXUIBIND001()
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
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => { return x.Name; });
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var nonInlineDiags = diagnostics.Where(d => d.Id == "RXUIBIND001").ToArray();
        await Assert.That(nonInlineDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that a block-body parenthesized lambda is still considered inline
    /// and does NOT trigger RXUIBIND001.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ParenthesizedBlockBodyLambda_IsInlineLambda_NoDiagnosticForRXUIBIND001()
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
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, (MyViewModel x) => { return x.Name; });
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var nonInlineDiags = diagnostics.Where(d => d.Id == "RXUIBIND001").ToArray();
        await Assert.That(nonInlineDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that RXUIBIND003 is reported when accessing an internal property in a lambda,
    /// which is considered protected-level access (non-public). This validates the compound
    /// condition branch for Accessibility.Internal in the private member check.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_InternalProperty_NoDiagnostic()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    internal string InternalName { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new MyViewModel();
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.InternalName);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var privateDiags = diagnostics.Where(d => d.Id == "RXUIBIND003").ToArray();

        // Internal is not Private or Protected, so no diagnostic
        await Assert.That(privateDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that no diagnostics are reported when source code contains no method invocations
    /// at all. This confirms the analyzer handles the case where there are zero invocation
    /// operations to analyze.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NoMethodInvocations_NoDiagnostics()
    {
        const string source = """
            namespace TestApp
            {
                public class SimpleClass
                {
                    public string Name { get; set; } = "";
                    public int Age;
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that RXUIBIND003 deep chain walking terminates correctly when reaching
    /// the lambda parameter (IdentifierNameSyntax). This tests the while loop exit condition
    /// where current is no longer a MemberAccessExpressionSyntax.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_DeepChain_PublicProperty_NoDiagnostic()
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
        var privateDiags = diagnostics.Where(d => d.Id == "RXUIBIND003").ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that RXUIBIND003 is reported for a private property in a deep property chain.
    /// This ensures the while loop walks the entire chain and detects private members
    /// at intermediate positions.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_DeepChain_PrivateIntermediateProperty_ReportsDiagnostic()
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
                    private Address PrivateAddress { get; set; } = new();

                    public void Test()
                    {
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, x => x.PrivateAddress.City);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var privateDiags = diagnostics.Where(d => d.Id == "RXUIBIND003").ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that RXUIBIND005 is NOT reported for WhenChanging (only applies to BindOneWay/BindTwoWay).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_WhenChanging_NoDiagnostic()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged, INotifyPropertyChanging, INotifyDataErrorInfo
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public event PropertyChangingEventHandler? PropertyChanging;
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
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var validationDiags = diagnostics.Where(d => d.Id == "RXUIBIND005").ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies no diagnostics when a non-binding extension method is called that happens
    /// to take a lambda argument. This exercises the IsBindingExtensionMethod false branch
    /// with a custom extension method that takes an expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CustomExtensionMethodWithLambda_NoDiagnostics()
    {
        const string source = """
            using System;
            using System.Linq.Expressions;
            using System.ComponentModel;

            namespace TestApp
            {
                public static class CustomExtensions
                {
                    public static object CustomMethod<TObj, TReturn>(
                        this TObj obj,
                        Expression<Func<TObj, TReturn>> property)
                        where TObj : class
                        => throw new NotImplementedException();
                }

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
                        vm.CustomMethod(x => x.Name);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
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
        var source = Preamble + """

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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
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
        var source = Preamble + """

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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND001 and RXUIBIND006 behavior with non-inline lambdas in BindTwoWay.
    /// The RXUIBIND001 is reported for non-inline expressions, but RXUIBIND006 is NOT checked
    /// because the non-lambda expression is not a LambdaExpressionSyntax.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND001_BindTwoWay_BothNonInline_ReportsMultipleDiagnostics()
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
                        Expression<Func<MyViewModel, string>> srcExpr = x => x.Name;
                        Expression<Func<MyView, string>> tgtExpr = x => x.NameText;
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTwoWay(vm, view, srcExpr, tgtExpr);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var nonInlineDiags = diagnostics.Where(d => d.Id == "RXUIBIND001").ToArray();

        // Both source and target Expression<Func<>> args are non-inline
        await Assert.That(nonInlineDiags.Length).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies that when BindOneWay is called with both inline lambdas, all path checks
    /// run against both lambdas. This exercises the full iteration loop in CheckPrivateMember
    /// and CheckUnsupportedPathSegment for multiple Expression arguments.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_BothInlineLambdas_AllChecksRun_NoDiagnostics()
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
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that RXUIBIND005 is reported for BindTwoWay when the source type implements
    /// INotifyDataErrorInfo through a base class. Tests the AllInterfaces walk for inherited
    /// interface detection.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_BindTwoWay_InheritedDataErrorInfo_ReportsDiagnostic()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class BaseViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
                    public bool HasErrors => false;
                    public IEnumerable GetErrors(string? propertyName) => Array.Empty<string>();
                }

                public class DerivedViewModel : BaseViewModel
                {
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
                        var vm = new DerivedViewModel();
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
    /// Verifies RXUIBIND006 is reported when a BindOneWay target property path contains a field
    /// inside the target lambda. Both source and target lambdas are checked for unsupported segments.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_BindOneWay_FieldInTargetPath_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that a parenthesized lambda with expression body is treated identically
    /// to a simple lambda for all analyzer checks.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ParenthesizedExpressionLambda_PublicProperty_NoDiagnostics()
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
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, (MyViewModel x) => x.Name);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that RXUIBIND003 is reported for a private property accessed through
    /// a parenthesized lambda expression. This exercises the ParenthesizedLambdaExpressionSyntax
    /// branch in CheckPrivateMember.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_ParenthesizedLambda_PrivateProperty_ReportsDiagnostic()
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
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, (MyViewModel x) => x.Secret);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var privateDiags = diagnostics.Where(d => d.Id == "RXUIBIND003").ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND006 reports a field access through a parenthesized lambda expression.
    /// This exercises the ParenthesizedLambdaExpressionSyntax branch in CheckUnsupportedPathSegment.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND006_ParenthesizedLambda_FieldAccess_ReportsDiagnostic()
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
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, (MyViewModel x) => x._name);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND005 is NOT reported for BindTwoWay when the source type does NOT
    /// implement INotifyDataErrorInfo. This exercises the AllInterfaces loop fallthrough
    /// path in CheckValidationSupport where no interface matches.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_BindTwoWay_NoDataErrorInfo_NoDiagnostic()
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
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTwoWay(vm, view, x => x.Name, x => x.NameText);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var validationDiags = diagnostics.Where(d => d.Id == "RXUIBIND005").ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that RXUIBIND004 is reported with the correct mechanism message
    /// when WhenChanging is called on an INPC-only type (no INotifyPropertyChanging).
    /// This validates the diagnostic message format argument contains the mechanism name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_INPCOnly_DiagnosticMessageContainsMechanism()
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
        var message = beforeChangeDiags[0].GetMessage();
        await Assert.That(message).Contains("INotifyPropertyChanged");
    }

    /// <summary>
    /// Verifies that RXUIBIND004 reports the correct mechanism message for WPF DependencyObject.
    /// This validates the "WPF DependencyObject" mechanism string in the diagnostic message.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_WpfDependencyObject_DiagnosticMessageContainsMechanism()
    {
        var source = Preamble + """

            namespace System.Windows
            {
                public class DependencyObject { }
            }

            namespace TestApp
            {
                public class WpfControl : System.Windows.DependencyObject
                {
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new WpfControl();
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var beforeChangeDiags = diagnostics.Where(d => d.Id == "RXUIBIND004").ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
        var message = beforeChangeDiags[0].GetMessage();
        await Assert.That(message).Contains("WPF DependencyObject");
    }

    /// <summary>
    /// Verifies that RXUIBIND004 reports the correct mechanism message for WinUI DependencyObject.
    /// This validates the "WinUI DependencyObject" mechanism string in the diagnostic message.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_WinUIDependencyObject_DiagnosticMessageContainsMechanism()
    {
        var source = Preamble + """

            namespace Microsoft.UI.Xaml
            {
                public class DependencyObject { }
            }

            namespace TestApp
            {
                public class WinUIControl : Microsoft.UI.Xaml.DependencyObject
                {
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new WinUIControl();
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var beforeChangeDiags = diagnostics.Where(d => d.Id == "RXUIBIND004").ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
        var message = beforeChangeDiags[0].GetMessage();
        await Assert.That(message).Contains("WinUI DependencyObject");
    }

    /// <summary>
    /// Verifies that RXUIBIND004 reports the correct mechanism message for WinForms Component.
    /// This validates the "WinForms Component" mechanism string in the diagnostic message.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_WinFormsComponent_DiagnosticMessageContainsMechanism()
    {
        var source = Preamble + """

            namespace System.ComponentModel
            {
                public class Component { }
            }

            namespace TestApp
            {
                public class WinFormsControl : System.ComponentModel.Component
                {
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new WinFormsControl();
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var beforeChangeDiags = diagnostics.Where(d => d.Id == "RXUIBIND004").ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
        var message = beforeChangeDiags[0].GetMessage();
        await Assert.That(message).Contains("WinForms Component");
    }

    /// <summary>
    /// Verifies that RXUIBIND004 reports the correct mechanism message for Android View.
    /// This validates the "Android View" mechanism string in the diagnostic message.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_AndroidView_DiagnosticMessageContainsMechanism()
    {
        var source = Preamble + """

            namespace Android.Views
            {
                public class View { }
            }

            namespace TestApp
            {
                public class AndroidControl : Android.Views.View
                {
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new AndroidControl();
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var beforeChangeDiags = diagnostics.Where(d => d.Id == "RXUIBIND004").ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
        var message = beforeChangeDiags[0].GetMessage();
        await Assert.That(message).Contains("Android View");
    }

    /// <summary>
    /// Verifies that RXUIBIND004 reports "unknown" mechanism when WhenChanging is called on
    /// a plain type with no recognized notification mechanism. This exercises the final
    /// fallthrough path in HasBeforeChangeSupport where mechanism is set to "unknown".
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_UnknownType_DiagnosticMessageContainsUnknown()
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
                        var vm = new PlainObject();
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var beforeChangeDiags = diagnostics.Where(d => d.Id == "RXUIBIND004").ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
        var message = beforeChangeDiags[0].GetMessage();
        await Assert.That(message).Contains("unknown");
    }

    /// <summary>
    /// Verifies RXUIBIND005 diagnostic message includes the source type name when
    /// BindOneWay is called on a type implementing INotifyDataErrorInfo.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_BindOneWay_DiagnosticMessageContainsTypeName()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class ValidatingViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
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
                        var vm = new ValidatingViewModel();
                        var view = new MyView();
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindOneWay(vm, view, x => x.Name, x => x.NameText);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var validationDiags = diagnostics.Where(d => d.Id == "RXUIBIND005").ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(1);
        var message = validationDiags[0].GetMessage();
        await Assert.That(message).Contains("ValidatingViewModel");
    }

    /// <summary>
    /// Verifies that RXUIBIND003 and RXUIBIND006 are both reported when a private field
    /// is accessed in the lambda body. The field is both private (RXUIBIND003) and a field
    /// rather than a property (RXUIBIND006). The analyzer should report RXUIBIND003 first
    /// because it is checked first and breaks out of the while loop.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_And_RXUIBIND006_PrivateField_ReportsBothDiagnostics()
    {
        var source = Preamble + """

            namespace TestApp
            {
                public class MyViewModel : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;
                    private string _secret = "";

                    public void Test()
                    {
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, x => x._secret);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);

        // RXUIBIND003 for private member access
        var privateDiags = diagnostics.Where(d => d.Id == "RXUIBIND003").ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);

        // RXUIBIND006 for field access (separate check)
        var unsupportedDiags = diagnostics.Where(d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that RXUIBIND004 is NOT reported when WhenChanging is called on a type
    /// that inherits from NSObject through an intermediate class. This tests the InheritsFrom
    /// walk for the KVO/NSObject path which supports before-change notifications.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_KVO_NSObject_DeepInheritance_NoDiagnostic()
    {
        var source = Preamble + """

            namespace Foundation
            {
                public class NSObject { }
            }

            namespace TestApp
            {
                public class AppleBase : Foundation.NSObject { }

                public class AppleDerived : AppleBase
                {
                    public string Name { get; set; } = "";
                }

                public class Usage
                {
                    public void Test()
                    {
                        var vm = new AppleDerived();
                        ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        var beforeChangeDiags = diagnostics.Where(d => d.Id == "RXUIBIND004").ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(0);
    }
}

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.Analyzer.Analyzers;

namespace ReactiveUI.Binding.Documentation.AnalyzersBindingShapes;

/// <summary>
/// Shows the diagnostics that report a binding whose shape the generator does not serve: RXUIBIND005 for a source
/// that validates through INotifyDataErrorInfo, RXUIBIND007 for a BindCommand control with no event to bind,
/// RXUIBIND008 for a BindInteraction property that is not an interaction, and RXUIBIND011 for a call that reached
/// ReactiveUI's own mixin.
/// </summary>
public static class AnalyzersBindingShapesExamples
{
    /// <summary>The id of the diagnostic for a source that validates through INotifyDataErrorInfo.</summary>
    private const string ValidationNotGeneratedId = "RXUIBIND005";

    /// <summary>The id of the diagnostic for a BindCommand control with no default event.</summary>
    private const string NoBindableEventId = "RXUIBIND007";

    /// <summary>The id of the diagnostic for a BindInteraction property that is not an interaction.</summary>
    private const string InvalidInteractionTypeId = "RXUIBIND008";

    /// <summary>The id of the diagnostic for a call that reached ReactiveUI's own mixin.</summary>
    private const string MixinShadowsGeneratedBindingId = "RXUIBIND011";

    /// <summary>Binds the amount box of the transfer screen to a view model that validates through INotifyDataErrorInfo.</summary>
    private const string DataErrorInfoSource = """
        using System;
        using System.Collections;
        using System.ComponentModel;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Banking;
        using ReactiveUI.Binding.Documentation.Infrastructure;

        namespace BankingApp;

        public sealed class AmountEntryViewModel : ObservableObject, INotifyDataErrorInfo
        {
            private string _amountText = string.Empty;

            public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

            public string AmountText
            {
                get => _amountText;
                set
                {
                    _amountText = value;
                    RaisePropertyChanged();
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(AmountText)));
                }
            }

            public bool HasErrors => string.IsNullOrWhiteSpace(AmountText);

            public IEnumerable GetErrors(string? propertyName) => HasErrors ? new[] { "Enter an amount." } : Array.Empty<string>();
        }

        public static class AmountBinder
        {
            public static IDisposable Bind(AmountEntryViewModel viewModel, TransferView view)
            {
                return viewModel.BindTwoWay(view, x => x.AmountText, v => v.AmountTextBox.Text);
            }
        }
        """;

    /// <summary>Binds the validation label of the transfer screen to a view model that exposes its errors as a property.</summary>
    private const string ValidationPropertySource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Banking;

        namespace BankingApp;

        public static class ValidationBinder
        {
            public static IDisposable Bind(TransferViewModel viewModel, TransferView view)
            {
                return viewModel.BindOneWay(view, x => x.ValidationSummary, v => v.ValidationLabel.Text);
            }
        }
        """;

    /// <summary>Binds the add command of the to-do screen to the title box, which raises no event.</summary>
    private const string TextBoxCommandSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Todo;

        namespace TodoApp;

        public static class AddCommandBinder
        {
            public static IDisposable Bind(TodoView view, TodoListViewModel viewModel)
            {
                return view.BindCommand(viewModel, x => x.AddCommand, v => v.NewTitleTextBox);
            }
        }
        """;

    /// <summary>Binds the add command of the to-do screen to the add button, which raises Click.</summary>
    private const string ButtonCommandSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Todo;

        namespace TodoApp;

        public static class AddCommandBinder
        {
            public static IDisposable Bind(TodoView view, TodoListViewModel viewModel)
            {
                return view.BindCommand(viewModel, x => x.AddCommand, v => v.AddButton);
            }
        }
        """;

    /// <summary>
    /// Binds a handler to the error message of the issue board. The source declares its own BindInteraction that
    /// accepts any property type, because the runtime signature stops the compiler before the analyzer sees a
    /// property that is not an interaction.
    /// </summary>
    private const string NotAnInteractionSource = """
        using System;
        using System.Linq.Expressions;
        using System.Threading.Tasks;
        using ReactiveUI.Binding.Documentation.GitHub;
        using TolerantBinding;

        namespace TolerantBinding
        {
            public static class ReactiveUIBindingExtensions
            {
                public static IDisposable BindInteraction<TViewModel, TView, TProperty>(
                    this TView view,
                    TViewModel viewModel,
                    Expression<Func<TViewModel, TProperty>> propertyName,
                    Func<object, Task> handler)
                    where TViewModel : class
                    where TView : class => throw new NotImplementedException();
            }
        }

        namespace GitHubApp
        {
            public static class ConfirmCloseBinder
            {
                public static IDisposable Bind(IssueBoardView view, IssueBoardViewModel viewModel)
                {
                    return view.BindInteraction(viewModel, x => x.ErrorMessage, context => Task.CompletedTask);
                }
            }
        }
        """;

    /// <summary>The same binding to the confirm-close question, which is an interaction.</summary>
    private const string InteractionPropertySource = """
        using System;
        using System.Linq.Expressions;
        using System.Threading.Tasks;
        using ReactiveUI.Binding.Documentation.GitHub;
        using TolerantBinding;

        namespace TolerantBinding
        {
            public static class ReactiveUIBindingExtensions
            {
                public static IDisposable BindInteraction<TViewModel, TView, TProperty>(
                    this TView view,
                    TViewModel viewModel,
                    Expression<Func<TViewModel, TProperty>> propertyName,
                    Func<object, Task> handler)
                    where TViewModel : class
                    where TView : class => throw new NotImplementedException();
            }
        }

        namespace GitHubApp
        {
            public static class ConfirmCloseBinder
            {
                public static IDisposable Bind(IssueBoardView view, IssueBoardViewModel viewModel)
                {
                    return view.BindInteraction(viewModel, x => x.ConfirmClose, context => Task.CompletedTask);
                }
            }
        }
        """;

    /// <summary>Watches the remaining count of the to-do list in a file that imports ReactiveUI and not this package.</summary>
    private const string MixinSource = """
        using System;
        using System.Linq.Expressions;
        using ReactiveUI;
        using ReactiveUI.Binding.Documentation.Todo;

        namespace ReactiveUI
        {
            public static class WhenAnyMixin
            {
                public static IObservable<TRet> WhenAnyValue<TSender, TRet>(this TSender sender, Expression<Func<TSender, TRet>> property1)
                    where TSender : class => throw new NotImplementedException();
            }
        }

        namespace TodoApp
        {
            public static class RemainingWatcher
            {
                public static IObservable<int> Watch(TodoListViewModel viewModel)
                {
                    return viewModel.WhenAnyValue(x => x.RemainingCount);
                }
            }
        }
        """;

    /// <summary>Watches the remaining count of the to-do list in a file that imports this package.</summary>
    private const string BindingImportSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Todo;

        namespace TodoApp;

        public static class RemainingWatcher
        {
            public static IObservable<int> Watch(TodoListViewModel viewModel)
            {
                return viewModel.WhenAnyValue(x => x.RemainingCount);
            }
        }
        """;

    /// <summary>Reports a bound source that implements INotifyDataErrorInfo, RXUIBIND005, and accepts a source that exposes its errors as a property.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReportSourceWithDataErrorInfo()
    {
        var diagnostics = await SourceAnalysis.AnalyzeAsync(DataErrorInfoSource, new BindingInvocationAnalyzer());
        var diagnostic = SourceAnalysis.Single(diagnostics);

        SampleCheck.Equal(ValidationNotGeneratedId, diagnostic.Id);
        SampleCheck.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
        SampleCheck.Equal("viewModel.BindTwoWay(view, x => x.AmountText, v => v.AmountTextBox.Text)", SourceAnalysis.FlaggedTextOf(diagnostic));
        SampleCheck.Equal(
            "Source type 'AmountEntryViewModel' implements INotifyDataErrorInfo; validation state propagation is not generated "
            + "and requires runtime engine or manual ErrorsChanged subscription",
            SourceAnalysis.MessageOf(diagnostic));

        var fixedDiagnostics = await SourceAnalysis.AnalyzeAsync(ValidationPropertySource, new BindingInvocationAnalyzer());

        SampleCheck.Equal(0, fixedDiagnostics.Length);
    }

    /// <summary>Reports a BindCommand control with no event to bind, RXUIBIND007, and accepts a button.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReportCommandControlWithNoEvent()
    {
        var diagnostics = await SourceAnalysis.AnalyzeAsync(TextBoxCommandSource, new BindingInvocationAnalyzer());
        var diagnostic = SourceAnalysis.Single(diagnostics);

        SampleCheck.Equal(NoBindableEventId, diagnostic.Id);
        SampleCheck.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        SampleCheck.Equal("v.NewTitleTextBox", SourceAnalysis.FlaggedTextOf(diagnostic));
        SampleCheck.Equal(
            "Control type 'TextBoxControl' has no default bindable event (Click, TouchUpInside, Pressed) and no 'toEvent' was specified",
            SourceAnalysis.MessageOf(diagnostic));

        var fixedDiagnostics = await SourceAnalysis.AnalyzeAsync(ButtonCommandSource, new BindingInvocationAnalyzer());

        SampleCheck.Equal(0, fixedDiagnostics.Length);
    }

    /// <summary>
    /// Reports a BindInteraction property whose type is not an interaction, RXUIBIND008, and accepts a property that is
    /// one. The runtime BindInteraction rejects such a property at compile time, so the source declares a BindInteraction
    /// that takes any property type; the analyzer recognises it by its class name.
    /// </summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReportInteractionPropertyThatIsNotAnInteraction()
    {
        var diagnostics = await SourceAnalysis.AnalyzeAsync(NotAnInteractionSource, new BindingInvocationAnalyzer());
        var diagnostic = SourceAnalysis.Single(diagnostics);

        SampleCheck.Equal(InvalidInteractionTypeId, diagnostic.Id);
        SampleCheck.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        SampleCheck.Equal("x.ErrorMessage", SourceAnalysis.FlaggedTextOf(diagnostic));
        SampleCheck.Equal("Property 'string' does not implement IInteraction<TInput, TOutput>", SourceAnalysis.MessageOf(diagnostic));

        var fixedDiagnostics = await SourceAnalysis.AnalyzeAsync(InteractionPropertySource, new BindingInvocationAnalyzer());

        SampleCheck.Equal(0, fixedDiagnostics.Length);
    }

    /// <summary>Reports a call that reached ReactiveUI's mixin, RXUIBIND011, and accepts a file that imports this package.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReportCallThatReachedReactiveUiMixin()
    {
        var diagnostics = await SourceAnalysis.AnalyzeAsync(MixinSource, new MixinShadowAnalyzer());
        var diagnostic = SourceAnalysis.Single(diagnostics);

        SampleCheck.Equal(MixinShadowsGeneratedBindingId, diagnostic.Id);
        SampleCheck.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        SampleCheck.Equal("viewModel.WhenAnyValue(x => x.RemainingCount)", SourceAnalysis.FlaggedTextOf(diagnostic));
        SampleCheck.Equal(
            "'WhenAnyValue' resolved to ReactiveUI's 'WhenAnyMixin', so this call generates nothing and takes the runtime expression engine",
            SourceAnalysis.MessageOf(diagnostic));

        var fixedDiagnostics = await SourceAnalysis.AnalyzeAsync(BindingImportSource, new MixinShadowAnalyzer());

        SampleCheck.Equal(0, fixedDiagnostics.Length);
    }
}

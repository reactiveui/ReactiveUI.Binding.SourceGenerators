// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.Analyzer.Analyzers;

namespace ReactiveUI.Binding.Documentation.AnalyzersInlineLambdas;

/// <summary>
/// Shows the diagnostics that report a property path the generator cannot read: RXUIBIND001 for a path that is not
/// written inline, RXUIBIND003 for a private or protected member, and RXUIBIND006 for an indexer, a field or a
/// method call.
/// </summary>
public static class AnalyzersInlineLambdasExamples
{
    /// <summary>The id of the diagnostic for a path that is not an inline lambda.</summary>
    private const string NonInlineLambdaId = "RXUIBIND001";

    /// <summary>The id of the diagnostic for a private or protected member in a path.</summary>
    private const string PrivateMemberId = "RXUIBIND003";

    /// <summary>The id of the diagnostic for an unsupported path segment.</summary>
    private const string UnsupportedPathSegmentId = "RXUIBIND006";

    /// <summary>The end of the message for a path segment the generator cannot read.</summary>
    private const string UnsupportedSegmentTail =
        "which is not a simple property access. Indexers, fields, and method calls are not generated, "
        + "so the call throws unless it names the Unsafe overload.";

    /// <summary>Watches the title of a to-do item through a path held in a variable.</summary>
    private const string VariablePathSource = """
        using System;
        using System.Linq.Expressions;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Todo;

        namespace TodoApp;

        public static class TitleWatcher
        {
            public static IObservable<string> Watch(TodoItem item)
            {
                Expression<Func<TodoItem, string>> titlePath = x => x.Title;
                return item.WhenChanged(titlePath);
            }
        }
        """;

    /// <summary>Watches the title of a to-do item with the path written in the call.</summary>
    private const string InlinePathSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Todo;

        namespace TodoApp;

        public static class TitleWatcher
        {
            public static IObservable<string> Watch(TodoItem item)
            {
                return item.WhenChanged(x => x.Title);
            }
        }
        """;

    /// <summary>Watches the title of a to-do item through a path held in a variable, on the Unsafe overload.</summary>
    private const string UnsafePathSource = """
        using System;
        using System.Linq.Expressions;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Todo;

        namespace TodoApp;

        public static class TitleWatcher
        {
            public static IObservable<string> Watch(TodoItem item)
            {
                Expression<Func<TodoItem, string>> titlePath = x => x.Title;
                return item.WhenChangedUnsafe(titlePath);
            }
        }
        """;

    /// <summary>Watches a private property of a quick-add view model from inside the view model.</summary>
    private const string PrivateMemberSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Infrastructure;

        namespace TodoApp;

        public sealed class QuickAddViewModel : ObservableObject
        {
            private string _draft = string.Empty;

            private string Draft
            {
                get => _draft;
                set
                {
                    _draft = value;
                    RaisePropertyChanged();
                }
            }

            public IObservable<string> ObserveDraft() => this.WhenChanged(x => x.Draft);
        }
        """;

    /// <summary>Watches a public property of a quick-add view model from inside the view model.</summary>
    private const string PublicMemberSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Infrastructure;

        namespace TodoApp;

        public sealed class QuickAddViewModel : ObservableObject
        {
            private string _draft = string.Empty;

            public string Draft
            {
                get => _draft;
                set
                {
                    _draft = value;
                    RaisePropertyChanged();
                }
            }

            public IObservable<string> ObserveDraft() => this.WhenChanged(x => x.Draft);
        }
        """;

    /// <summary>Watches the title of the first item in the to-do list.</summary>
    private const string IndexerSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Todo;

        namespace TodoApp;

        public static class FirstItemWatcher
        {
            public static IObservable<string> Watch(TodoListViewModel viewModel)
            {
                return viewModel.WhenChanged(x => x.Items[0].Title);
            }
        }
        """;

    /// <summary>Watches the title of the selected to-do item.</summary>
    private const string SelectedItemSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Todo;

        namespace TodoApp;

        public static class SelectedItemWatcher
        {
            public static IObservable<string> Watch(TodoListViewModel viewModel)
            {
                return viewModel.WhenChanged(x => x.SelectedItem!.Title);
            }
        }
        """;

    /// <summary>Watches the title of the selected issue, shown in capitals.</summary>
    private const string MethodCallSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.GitHub;

        namespace GitHubApp;

        public static class IssueHeadlineWatcher
        {
            public static IObservable<string> Watch(IssueBoardViewModel viewModel)
            {
                return viewModel.WhenChanged(x => x.SelectedIssue!.Title.ToUpperInvariant());
            }
        }
        """;

    /// <summary>Watches the title of the selected issue and leaves the capitals to the caller.</summary>
    private const string IssueTitleSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.GitHub;

        namespace GitHubApp;

        public static class IssueHeadlineWatcher
        {
            public static IObservable<string> Watch(IssueBoardViewModel viewModel)
            {
                return viewModel.WhenChanged(x => x.SelectedIssue!.Title);
            }
        }
        """;

    /// <summary>Watches a search box whose text is a public field.</summary>
    private const string FieldSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Infrastructure;

        namespace TodoApp;

        public sealed class TodoSearchViewModel : ObservableObject
        {
            public string Query = string.Empty;

            public IObservable<string> ObserveQuery() => this.WhenChanged(x => x.Query);
        }
        """;

    /// <summary>Watches a search box whose text is a property.</summary>
    private const string PropertySource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Infrastructure;

        namespace TodoApp;

        public sealed class TodoSearchViewModel : ObservableObject
        {
            private string _query = string.Empty;

            public string Query
            {
                get => _query;
                set
                {
                    _query = value;
                    RaisePropertyChanged();
                }
            }

            public IObservable<string> ObserveQuery() => this.WhenChanged(x => x.Query);
        }
        """;

    /// <summary>Reports a path held in a variable, RXUIBIND001, because the generator reads only a lambda written in the call.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReportPathHeldInVariable()
    {
        var diagnostics = await SourceAnalysis.AnalyzeAsync(VariablePathSource, new BindingInvocationAnalyzer());
        var diagnostic = SourceAnalysis.Single(diagnostics);

        SampleCheck.Equal(NonInlineLambdaId, diagnostic.Id);
        SampleCheck.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
        SampleCheck.Equal("titlePath", SourceAnalysis.FlaggedTextOf(diagnostic));
        SampleCheck.Equal(
            "Expression argument must be an inline lambda expression for compile-time optimization. "
            + "A variable or method reference is not generated, so the call throws unless it names the Unsafe overload.",
            SourceAnalysis.MessageOf(diagnostic));
    }

    /// <summary>Accepts a path written in the call, which is the fix for RXUIBIND001.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task AcceptInlineLambda()
    {
        var diagnostics = await SourceAnalysis.AnalyzeAsync(InlinePathSource, new BindingInvocationAnalyzer());

        SampleCheck.Equal(0, diagnostics.Length);
    }

    /// <summary>Accepts a path held in a variable when the call names the Unsafe overload, which reads the path at run time.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task AcceptUnsafeOverloadForVariablePath()
    {
        var diagnostics = await SourceAnalysis.AnalyzeAsync(UnsafePathSource, new BindingInvocationAnalyzer());

        SampleCheck.Equal(0, diagnostics.Length);
    }

    /// <summary>Reports a private property in a path, RXUIBIND003, and accepts the same property once it is public.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReportPrivateMemberInPath()
    {
        var diagnostics = await SourceAnalysis.AnalyzeAsync(PrivateMemberSource, new BindingInvocationAnalyzer());
        var diagnostic = SourceAnalysis.Single(diagnostics);

        SampleCheck.Equal(PrivateMemberId, diagnostic.Id);
        SampleCheck.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        SampleCheck.Equal("Draft", SourceAnalysis.FlaggedTextOf(diagnostic));
        SampleCheck.Equal(
            "Expression accesses private or protected member 'Draft' which cannot be observed by a generated extension method",
            SourceAnalysis.MessageOf(diagnostic));

        var fixedDiagnostics = await SourceAnalysis.AnalyzeAsync(PublicMemberSource, new BindingInvocationAnalyzer());

        SampleCheck.Equal(0, fixedDiagnostics.Length);
    }

    /// <summary>Reports an indexer in a path, RXUIBIND006, and accepts a path made of properties.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReportIndexerInPath()
    {
        var diagnostics = await SourceAnalysis.AnalyzeAsync(IndexerSource, new BindingInvocationAnalyzer());
        var diagnostic = SourceAnalysis.Single(diagnostics);

        SampleCheck.Equal(UnsupportedPathSegmentId, diagnostic.Id);
        SampleCheck.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        SampleCheck.Equal("x.Items[0]", SourceAnalysis.FlaggedTextOf(diagnostic));
        SampleCheck.Equal($"Expression contains 'x.Items[0]' {UnsupportedSegmentTail}", SourceAnalysis.MessageOf(diagnostic));

        var fixedDiagnostics = await SourceAnalysis.AnalyzeAsync(SelectedItemSource, new BindingInvocationAnalyzer());

        SampleCheck.Equal(0, fixedDiagnostics.Length);
    }

    /// <summary>Reports a method call in a path, RXUIBIND006, and accepts a path that ends at the property.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReportMethodCallInPath()
    {
        var diagnostics = await SourceAnalysis.AnalyzeAsync(MethodCallSource, new BindingInvocationAnalyzer());
        var diagnostic = SourceAnalysis.Single(diagnostics);

        SampleCheck.Equal(UnsupportedPathSegmentId, diagnostic.Id);
        SampleCheck.Equal("x.SelectedIssue!.Title.ToUpperInvariant()", SourceAnalysis.FlaggedTextOf(diagnostic));
        SampleCheck.Equal(
            $"Expression contains 'x.SelectedIssue!.Title.ToUpperInvariant()' {UnsupportedSegmentTail}",
            SourceAnalysis.MessageOf(diagnostic));

        var fixedDiagnostics = await SourceAnalysis.AnalyzeAsync(IssueTitleSource, new BindingInvocationAnalyzer());

        SampleCheck.Equal(0, fixedDiagnostics.Length);
    }

    /// <summary>Reports a field in a path, RXUIBIND006, and accepts the same value as a property.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReportFieldInPath()
    {
        var diagnostics = await SourceAnalysis.AnalyzeAsync(FieldSource, new BindingInvocationAnalyzer());
        var diagnostic = SourceAnalysis.Single(diagnostics);

        SampleCheck.Equal(UnsupportedPathSegmentId, diagnostic.Id);
        SampleCheck.Equal("Query", SourceAnalysis.FlaggedTextOf(diagnostic));
        SampleCheck.Equal($"Expression contains 'Query' {UnsupportedSegmentTail}", SourceAnalysis.MessageOf(diagnostic));

        var fixedDiagnostics = await SourceAnalysis.AnalyzeAsync(PropertySource, new BindingInvocationAnalyzer());

        SampleCheck.Equal(0, fixedDiagnostics.Length);
    }
}

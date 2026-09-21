// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.Analyzer.Analyzers;

namespace ReactiveUI.Binding.Documentation.AnalyzersNotifications;

/// <summary>
/// Shows the diagnostics that report an observation that would never see a change: RXUIBIND002 for a type that
/// raises no notification, RXUIBIND004 for a before-change observation on a type that only reports after-change
/// notifications, and RXUIBIND010 for a path that passes through a type that raises no notification.
/// </summary>
public static class AnalyzersNotificationsExamples
{
    /// <summary>The id of the diagnostic for a type with no notification mechanism.</summary>
    private const string NoObservablePropertiesId = "RXUIBIND002";

    /// <summary>The id of the diagnostic for a before-change observation the type cannot serve.</summary>
    private const string NoBeforeChangeSupportId = "RXUIBIND004";

    /// <summary>The id of the diagnostic for a path through a type that raises no notification.</summary>
    private const string SilentPathLinkId = "RXUIBIND010";

    /// <summary>Watches the size of a stored object, which is a plain class that raises no notification.</summary>
    private const string PlainClassSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.CloudStorage;

        namespace StorageApp;

        public static class ObjectSizeWatcher
        {
            public static IObservable<long> Watch(StorageObject stored)
            {
                return stored.WhenChanged(x => x.Size);
            }
        }
        """;

    /// <summary>Watches the object the user picked, which the view model reports through its own notification.</summary>
    private const string NotifyingOwnerSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.CloudStorage;

        namespace StorageApp;

        public static class SelectedObjectWatcher
        {
            public static IObservable<StorageObject?> Watch(StorageBrowserViewModel viewModel)
            {
                return viewModel.WhenChanged(x => x.SelectedObject);
            }
        }
        """;

    /// <summary>Watches a to-do title before it changes, on a type that reports only after-change notifications.</summary>
    private const string BeforeChangeOnAfterChangeTypeSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Todo;

        namespace TodoApp;

        public static class TitleEditWatcher
        {
            public static IObservable<string> WatchBeforeEdit(TodoItem item)
            {
                return item.WhenChanging(x => x.Title);
            }
        }
        """;

    /// <summary>Watches a to-do title after it changes.</summary>
    private const string AfterChangeSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Todo;

        namespace TodoApp;

        public static class TitleEditWatcher
        {
            public static IObservable<string> WatchAfterEdit(TodoItem item)
            {
                return item.WhenChanged(x => x.Title);
            }
        }
        """;

    /// <summary>Watches a to-do title before it changes, on a type that reports before-change notifications.</summary>
    private const string BeforeChangeTypeSource = """
        using System;
        using System.ComponentModel;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.Infrastructure;

        namespace TodoApp;

        public sealed class EditableTodo : ObservableObject, INotifyPropertyChanging
        {
            private string _title = string.Empty;

            public event PropertyChangingEventHandler? PropertyChanging;

            public string Title
            {
                get => _title;
                set
                {
                    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Title)));
                    _title = value;
                    RaisePropertyChanged();
                }
            }
        }

        public static class TitleEditWatcher
        {
            public static IObservable<string> WatchBeforeEdit(EditableTodo item)
            {
                return item.WhenChanging(x => x.Title);
            }
        }
        """;

    /// <summary>Watches the state of the link through the connection object, which raises no property notification.</summary>
    private const string SilentLinkSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.CloudStorage;

        namespace StorageApp;

        public static class ConnectionWatcher
        {
            public static IObservable<ConnectionState> Watch(StorageBrowserViewModel viewModel)
            {
                return viewModel.WhenChanged(x => x.Connection.State);
            }
        }
        """;

    /// <summary>Watches the state of the link through the view model property that mirrors it.</summary>
    private const string MirroredStateSource = """
        using System;
        using ReactiveUI.Binding;
        using ReactiveUI.Binding.Documentation.CloudStorage;

        namespace StorageApp;

        public static class ConnectionWatcher
        {
            public static IObservable<ConnectionState> Watch(StorageBrowserViewModel viewModel)
            {
                return viewModel.WhenChanged(x => x.ConnectionStatus);
            }
        }
        """;

    /// <summary>Reports a type that raises no notification, RXUIBIND002, and accepts an owner that does.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReportTypeWithNoNotifications()
    {
        var diagnostics = await SourceAnalysis.AnalyzeAsync(PlainClassSource, new TypeAnalyzer());
        var diagnostic = SourceAnalysis.Single(diagnostics);

        SampleCheck.Equal(NoObservablePropertiesId, diagnostic.Id);
        SampleCheck.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        SampleCheck.Equal("stored.WhenChanged(x => x.Size)", SourceAnalysis.FlaggedTextOf(diagnostic));
        SampleCheck.Equal(
            "Type 'StorageObject' has no observable properties and does not implement any observable notification mechanism",
            SourceAnalysis.MessageOf(diagnostic));

        var fixedDiagnostics = await SourceAnalysis.AnalyzeAsync(NotifyingOwnerSource, new TypeAnalyzer());

        SampleCheck.Equal(0, fixedDiagnostics.Length);
    }

    /// <summary>Reports a before-change observation on a type that has no before-change notification, RXUIBIND004.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReportBeforeChangeOnAfterChangeType()
    {
        var diagnostics = await SourceAnalysis.AnalyzeAsync(BeforeChangeOnAfterChangeTypeSource, new BindingInvocationAnalyzer());
        var diagnostic = SourceAnalysis.Single(diagnostics);

        SampleCheck.Equal(NoBeforeChangeSupportId, diagnostic.Id);
        SampleCheck.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        SampleCheck.Equal("item.WhenChanging(x => x.Title)", SourceAnalysis.FlaggedTextOf(diagnostic));
        SampleCheck.Equal(
            "Type 'TodoItem' does not support before-change notifications via INotifyPropertyChanged (without INotifyPropertyChanging); "
            + "WhenChanging reads the value once and then stays silent",
            SourceAnalysis.MessageOf(diagnostic));
    }

    /// <summary>Accepts the two fixes for RXUIBIND004: observe after the change, or observe a type that reports before the change.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task AcceptBeforeChangeOnType()
    {
        var afterChange = await SourceAnalysis.AnalyzeAsync(AfterChangeSource, new BindingInvocationAnalyzer());

        SampleCheck.Equal(0, afterChange.Length);

        var beforeChange = await SourceAnalysis.AnalyzeAsync(BeforeChangeTypeSource, new BindingInvocationAnalyzer());

        SampleCheck.Equal(0, beforeChange.Length);
    }

    /// <summary>Reports a path through a type that raises no notification, RXUIBIND010, and accepts a path that avoids it.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReportPathThroughSilentType()
    {
        var diagnostics = await SourceAnalysis.AnalyzeAsync(SilentLinkSource, new BindingInvocationAnalyzer());
        var diagnostic = SourceAnalysis.Single(diagnostics);

        SampleCheck.Equal(SilentPathLinkId, diagnostic.Id);
        SampleCheck.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        SampleCheck.Equal("x.Connection", SourceAnalysis.FlaggedTextOf(diagnostic));
        SampleCheck.Equal(
            "Type 'StorageConnection' raises no notification, so 'x.Connection' is read once and the observation stops following the path there",
            SourceAnalysis.MessageOf(diagnostic));

        var fixedDiagnostics = await SourceAnalysis.AnalyzeAsync(MirroredStateSource, new BindingInvocationAnalyzer());

        SampleCheck.Equal(0, fixedDiagnostics.Length);
    }
}

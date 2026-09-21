// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Controls;
using ReactiveUI.Binding.Documentation.Todo;
using Splat;

namespace ReactiveUI.Binding.Documentation.ThreadingInvokers;

/// <summary>Shows how a binding finds the thread that owns a control: <c>IViewThreadInvoker</c> and <c>ViewThreadInvokers</c>.</summary>
public static class ThreadingInvokersExamples
{
    /// <summary>The title of the item before the sync.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title the user types on the UI thread.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The first title a background sync delivers.</summary>
    private const string FirstSyncedTitle = "Renew car registration (draft)";

    /// <summary>The second title a background sync delivers.</summary>
    private const string SecondSyncedTitle = "Renew car registration (reviewed)";

    /// <summary>The last title a background sync delivers.</summary>
    private const string LastSyncedTitle = "Renew car registration (final)";

    /// <summary>The text a posted callback records.</summary>
    private const string PostedText = "posted";

    /// <summary>The number of writes a label receives when it starts with a title and then shows the latest one.</summary>
    private const int InitialAndLatestWrites = 2;

    /// <summary>Runs work on another thread and waits for it to finish.</summary>
    /// <param name="work">The work to run.</param>
    public static void RunOnWorkerThread(Action work)
    {
        Thread worker = new(work.Invoke);
        worker.Start();
        worker.Join();
    }

    /// <summary>Registers the invoker for the label type. The binding engine asks each registered invoker whether it claims the control a binding writes to.</summary>
    public static void RegisterInvoker()
    {
        UiDispatcher dispatcher = new();
        DispatcherLabelControl label = new(dispatcher);
        LabelControl unclaimed = new();
        UiDispatcherViewThreadInvoker invoker = new();

        AppLocator.CurrentMutable.RegisterConstant<IViewThreadInvoker>(invoker);
        ViewThreadInvokers.Refresh();

        SampleCheck.Equal(invoker, ViewThreadInvokers.ForTarget(label));
        SampleCheck.Equal(true, ViewThreadInvokers.ForTarget(unclaimed) is null);
        SampleCheck.Equal(true, ViewThreadInvokers.ForTarget(null) is null);
    }

    /// <summary>Calls the three members of the invoker directly: who it claims, who may write, and how to queue work.</summary>
    public static void CallInvokerMembers()
    {
        UiDispatcher dispatcher = new();
        DispatcherLabelControl label = new(dispatcher);
        LabelControl unclaimed = new();
        UiDispatcherViewThreadInvoker invoker = new();
        List<string> log = [];
        var accessFromWorker = true;

        RunOnWorkerThread(() => accessFromWorker = invoker.CheckAccess(label));
        invoker.Post(label, static state => ((List<string>)state!).Add(PostedText), log);

        SampleCheck.Equal(true, invoker.Claims(label));
        SampleCheck.Equal(false, invoker.Claims(unclaimed));
        SampleCheck.Equal(true, invoker.CheckAccess(label));
        SampleCheck.Equal(false, accessFromWorker);
        SampleCheck.Equal(0, log.Count);

        _ = dispatcher.RunPending();

        SampleCheck.SequenceEqual([PostedText], log);
    }

    /// <summary>Shows that a control refuses a write from another thread, which is what the invoker keeps a binding from doing.</summary>
    public static void RefuseWriteFromWorkerThread()
    {
        UiDispatcher dispatcher = new();
        DispatcherLabelControl label = new(dispatcher);
        var refused = false;

        RunOnWorkerThread(() =>
        {
            try
            {
                label.Text = RenamedTitle;
            }
            catch (InvalidOperationException)
            {
                refused = true;
            }
        });

        SampleCheck.Equal(true, refused);
        SampleCheck.Equal(string.Empty, label.Text);
    }

    /// <summary>Writes a bound value on the owning thread: the write runs inline and nothing waits.</summary>
    public static void WriteOnOwningThreadRunsInline()
    {
        UiDispatcher dispatcher = new();
        DispatcherLabelControl titleLabel = new(dispatcher);
        TodoItem item = new() { Title = OriginalTitle };

        using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text))
        {
            SampleCheck.Equal(OriginalTitle, titleLabel.Text);

            item.Title = RenamedTitle;

            SampleCheck.Equal(RenamedTitle, titleLabel.Text);
            SampleCheck.Equal(0, dispatcher.PendingCount);
        }
    }

    /// <summary>Writes a bound value from another thread: the write waits until the owning thread runs its queue.</summary>
    public static void WriteFromWorkerThreadWaitsForOwningThread()
    {
        UiDispatcher dispatcher = new();
        DispatcherLabelControl titleLabel = new(dispatcher);
        TodoItem item = new() { Title = OriginalTitle };

        using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text))
        {
            RunOnWorkerThread(() => item.Title = FirstSyncedTitle);

            SampleCheck.Equal(OriginalTitle, titleLabel.Text);
            SampleCheck.Equal(1, dispatcher.PendingCount);

            var ran = dispatcher.RunPending();

            SampleCheck.Equal(1, ran);
            SampleCheck.Equal(FirstSyncedTitle, titleLabel.Text);
        }
    }

    /// <summary>Changes a bound value three times before the owning thread runs: only the latest value is written.</summary>
    public static void LatestValueWins()
    {
        UiDispatcher dispatcher = new();
        DispatcherLabelControl titleLabel = new(dispatcher);
        TodoItem item = new() { Title = OriginalTitle };

        using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text))
        {
            RunOnWorkerThread(() =>
            {
                item.Title = FirstSyncedTitle;
                item.Title = SecondSyncedTitle;
                item.Title = LastSyncedTitle;
            });

            SampleCheck.Equal(1, dispatcher.PendingCount);

            _ = dispatcher.RunPending();

            SampleCheck.Equal(LastSyncedTitle, titleLabel.Text);
            SampleCheck.Equal(InitialAndLatestWrites, titleLabel.WriteCount);
        }
    }
}

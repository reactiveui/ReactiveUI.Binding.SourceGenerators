// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Covers where a binding delivers its write to the view. A view model raises its notifications from whatever
/// thread did the work, and the UI frameworks only allow a view to be touched from the thread that owns it, so
/// the binding has to move the write rather than leaving each consumer to do it.
/// </summary>
public class ViewWriteSchedulingRuntimeTests
{
    /// <summary>What the scenario returns when the write went through the established sequencer.</summary>
    private const string Scheduled = "scheduled";

    /// <summary>A binding made while a sequencer stands in for the view's thread.</summary>
    private const string SchedulingSource = """
                                            using System;
                                            using System.ComponentModel;
                                            using ReactiveUI.Binding;
                                            using ReactiveUI.Primitives.Concurrency;

                                            namespace TestApp
                                            {
                                                public class RecordingSequencer : ISequencer
                                                {
                                                    public bool Used { get; private set; }

                                                    public DateTimeOffset Now
                                                    {
                                                        get { return ImmediateSequencer.Instance.Now; }
                                                    }

                                                    public long Timestamp
                                                    {
                                                        get { return ImmediateSequencer.Instance.Timestamp; }
                                                    }

                                                    public void Schedule(IWorkItem item)
                                                    {
                                                        Used = true;
                                                        ImmediateSequencer.Instance.Schedule(item);
                                                    }

                                                    public void Schedule(IWorkItem item, long dueTimestamp)
                                                    {
                                                        Used = true;
                                                        ImmediateSequencer.Instance.Schedule(item, dueTimestamp);
                                                    }
                                                }

                                                public class MyViewModel : INotifyPropertyChanged
                                                {
                                                    private string _name = "";

                                                    public event PropertyChangedEventHandler PropertyChanged;

                                                    public string Name
                                                    {
                                                        get { return _name; }
                                                        set
                                                        {
                                                            _name = value;
                                                            var handler = PropertyChanged;
                                                            if (handler != null)
                                                            {
                                                                handler(this, new PropertyChangedEventArgs("Name"));
                                                            }
                                                        }
                                                    }
                                                }

                                                public class MyView : INotifyPropertyChanged
                                                {
                                                    public event PropertyChangedEventHandler PropertyChanged;

                                                    public string DisplayName { get; set; } = "";
                                                }

                                                public static class Usage
                                                {
                                                    public static string Run()
                                                    {
                                                        var sequencer = new RecordingSequencer();
                                                        BindingSchedulers.MainThread = sequencer;

                                                        try
                                                        {
                                                            var viewModel = new MyViewModel();
                                                            var view = new MyView();

                                                            var binding = viewModel.BindOneWay(view, x => x.Name, x => x.DisplayName);

                                                            viewModel.Name = "changed";

                                                            return sequencer.Used ? "scheduled" : "inline";
                                                        }
                                                        finally
                                                        {
                                                            BindingSchedulers.MainThread = null;
                                                        }
                                                    }
                                                }
                                            }
                                            """;

    /// <summary>
    /// A write to the view goes through the established sequencer. Delivering it on the notifying thread is
    /// what throws on the UI frameworks, and an application that had this done for it will not have added the
    /// marshalling itself.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_WhenAViewThreadIsEstablished_DeliversTheWriteThroughIt()
    {
        var result = TestHelper.RunGenerator(SchedulingSource, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        var run = assembly.GetType("TestApp.Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;

        var outcome = (string?)run.Invoke(null, null);

        context.Unload();

        await Assert.That(outcome).IsEqualTo(Scheduled);
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Covers a view whose view model is replaced after the binding is made. Setting the view model after
/// construction and swapping it later is the ordinary view lifecycle - a router does it on every navigation -
/// so a binding that holds the instance it was handed stops tracking the view without saying so.
/// </summary>
public class ViewModelReplacementRuntimeTests
{
    /// <summary>The value the second view model publishes after it replaces the first.</summary>
    private const string ValueFromReplacement = "second";

    /// <summary>A view that notifies when its view model changes, bound once and then given a new one.</summary>
    private const string ViewModelReplacementSource = """
                                                      using System;
                                                      using System.ComponentModel;
                                                      using ReactiveUI.Binding;

                                                      namespace TestApp
                                                      {
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

                                                          public class MyView : INotifyPropertyChanged, IViewFor<MyViewModel>
                                                          {
                                                              private MyViewModel _viewModel;

                                                              public event PropertyChangedEventHandler PropertyChanged;

                                                              public string Text { get; set; } = "";

                                                              public MyViewModel ViewModel
                                                              {
                                                                  get { return _viewModel; }
                                                                  set
                                                                  {
                                                                      _viewModel = value;
                                                                      var handler = PropertyChanged;
                                                                      if (handler != null)
                                                                      {
                                                                          handler(this, new PropertyChangedEventArgs("ViewModel"));
                                                                      }
                                                                  }
                                                              }

                                                              object IViewFor.ViewModel
                                                              {
                                                                  get { return ViewModel; }
                                                                  set { ViewModel = (MyViewModel)value; }
                                                              }
                                                          }

                                                          public static class Usage
                                                          {
                                                              public static string Run()
                                                              {
                                                                  var view = new MyView();
                                                                  view.ViewModel = new MyViewModel { Name = "first" };

                                                                  var binding = view.OneWayBind(view.ViewModel, vm => vm.Name, v => v.Text);

                                                                  view.ViewModel = new MyViewModel { Name = "ignored" };
                                                                  view.ViewModel.Name = "second";

                                                                  return view.Text;
                                                              }
                                                          }
                                                      }
                                                      """;

    /// <summary>
    /// A binding follows the view's current view model. Holding the instance the call was handed leaves the
    /// replaced view model driving the view, with nothing raised to say the binding went stale.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task OneWayBind_AfterTheViewModelIsReplaced_TracksTheNewViewModel() =>
        AssertTracksReplacement(ViewModelReplacementSource);

    /// <summary>The two-way binding follows the view's current view model in the view-model-to-view direction.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task Bind_AfterTheViewModelIsReplaced_TracksTheNewViewModel() =>
        AssertTracksReplacement(
            ViewModelReplacementSource.Replace(
                "view.OneWayBind(view.ViewModel, vm => vm.Name, v => v.Text)",
                "view.Bind(view.ViewModel, vm => vm.Name, v => v.Text)",
                StringComparison.Ordinal));

    /// <summary>Runs a replacement scenario and asserts the view ends up showing the replacement's value.</summary>
    /// <param name="source">The scenario source to generate, compile and run.</param>
    /// <returns>A task representing the asynchronous assertion.</returns>
    private static async Task AssertTracksReplacement(string source)
    {
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        var run = assembly.GetType("TestApp.Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;

        var outcome = (string?)run.Invoke(null, null);

        context.Unload();

        await Assert.That(outcome).IsEqualTo(ValueFromReplacement);
    }
}

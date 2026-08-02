// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>
/// Builds compilation sources for the Apple key-value-observing path. The KVO observation code the
/// generator emits instantiates helper classes that are themselves generated, so a scenario only proves
/// anything if it compiles - which needs enough of <c>Foundation</c> present for those helpers to bind.
/// A stub stands in for the real framework so the scenarios run on every target, not just Apple ones.
/// </summary>
internal static class ApplePlatformSource
{
    /// <summary>The generated file that declares the observation helper classes.</summary>
    internal const string HelperHintName = "ObservationHelpers.g.cs";

    /// <summary>
    /// The members of <c>Foundation</c> the generated KVO helpers bind against: the observer callback they
    /// override and the add/remove observer pair they subscribe through.
    /// </summary>
    private const string FoundationStub = """
                                          namespace Foundation
                                          {
                                              public class NSString
                                              {
                                                  private readonly string _value;
                                                  public NSString(string value) { _value = value; }
                                                  public static explicit operator NSString(string value) => new NSString(value);
                                              }
                                              public class NSDictionary {}
                                              public enum NSKeyValueObservingOptions { New = 1, Old = 2 }
                                              public class NSObject
                                              {
                                                  public virtual void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context) {}
                                                  public void AddObserver(NSObject observer, NSString keyPath, NSKeyValueObservingOptions options, IntPtr context) {}
                                                  public void RemoveObserver(NSObject observer, NSString keyPath) {}
                                              }
                                          }
                                          """;

    /// <summary>Builds a source that only declares an <c>NSObject</c>-derived view, with no binding call.</summary>
    /// <returns>The compilation source.</returns>
    internal static string TypeDetectionScenario() => $$"""
        using System;

        {{FoundationStub}}

        namespace TestApp
        {
            public class MyAppleView : Foundation.NSObject
            {
                public string Text { get; set; }
            }
        }
        """;

    /// <summary>Builds a source with an <c>NSObject</c>-derived view and a plain view model, bound by the given call.</summary>
    /// <param name="bindingCall">
    /// The binding expression, with <c>vm</c> and <c>view</c> in scope - for example
    /// <c>view.BindOneWay(vm, x =&gt; x.Text, x =&gt; x.Name)</c>.
    /// </param>
    /// <returns>The compilation source.</returns>
    internal static string BindingScenario(string bindingCall) => $$"""
        using System;
        using System.ComponentModel;

        using ReactiveUI.Binding;

        {{FoundationStub}}

        namespace TestApp
        {
            public class MyAppleView : Foundation.NSObject
            {
                public string Text { get; set; }
            }

            public class MyViewModel : INotifyPropertyChanged
            {
                private string _name = string.Empty;
                public event PropertyChangedEventHandler PropertyChanged;
                public string Name
                {
                    get => _name;
                    set
                    {
                        if (_name != value)
                        {
                            _name = value;
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                        }
                    }
                }
            }

            public static class Scenario
            {
                public static IDisposable Execute(MyViewModel vm, MyAppleView view)
                    => {{bindingCall}};
            }
        }
        """;
}

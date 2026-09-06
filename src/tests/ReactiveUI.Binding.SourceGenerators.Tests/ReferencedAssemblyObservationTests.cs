// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers observation of a source type the consumer references rather than declares. A referenced type reaches
/// the generator only as a symbol, never as a class declaration, so detection that reads declarations alone
/// finds nothing for it and the call site silently loses its notification mechanism.
/// </summary>
public class ReferencedAssemblyObservationTests
{
    /// <summary>The dispatch file the WhenChanged call site is generated into.</summary>
    private const string DispatchFileName = "WhenChangedDispatch.g.cs";

    /// <summary>The observable the generator emits when it knows the type raises PropertyChanged.</summary>
    private const string SubscribingObservable = "PropertyObservable";

    /// <summary>The observable the generator emits when it has no notification mechanism for the type.</summary>
    private const string SingleValueObservable = "ImmediateReturnSignal";

    /// <summary>A view model in another assembly, notifying exactly as one in the consumer's own would.</summary>
    private const string ReferencedAssemblySource = """
                                                    using System.ComponentModel;

                                                    namespace ExternalLib
                                                    {
                                                        public class ExternalViewModel : INotifyPropertyChanged
                                                        {
                                                            private string _name;

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
                                                    }
                                                    """;

    /// <summary>A consumer that observes the referenced view model and declares no view model of its own.</summary>
    private const string ConsumerSource = """
                                          using System;
                                          using ReactiveUI.Binding;

                                          namespace Consumer
                                          {
                                              public static class Usage
                                              {
                                                  public static IObservable<string> Observe(ExternalLib.ExternalViewModel viewModel)
                                                  {
                                                      return viewModel.WhenChanged(x => x.Name);
                                                  }
                                              }
                                          }
                                          """;

    /// <summary>A referenced INotifyPropertyChanged type is observed through its PropertyChanged event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnAReferencedType_SubscribesToPropertyChanged()
    {
        var result = RunAgainstReferencedViewModel();

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchFileName, SubscribingObservable);
    }

    /// <summary>
    /// A referenced type does not degrade to a single value. That degradation is silent at the call site, and
    /// the single-value observable completes, which terminates every operator downstream of it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnAReferencedType_DoesNotDegradeToASingleValue()
    {
        var result = RunAgainstReferencedViewModel();

        await result.GeneratedSourceDoesNotContain(DispatchFileName, SingleValueObservable);
    }

    /// <summary>Runs the generator over a consumer whose view model lives in a referenced assembly.</summary>
    /// <returns>The generator result for the referenced-view-model scenario.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static GeneratorTestResult RunAgainstReferencedViewModel() =>
        TestHelper.RunGenerator(
            ConsumerSource,
            LanguageVersion.CSharp10,
            null,
            false,
            [TestHelper.CompileToReference(ReferencedAssemblySource, "ExternalLib", LanguageVersion.CSharp10)]);
}

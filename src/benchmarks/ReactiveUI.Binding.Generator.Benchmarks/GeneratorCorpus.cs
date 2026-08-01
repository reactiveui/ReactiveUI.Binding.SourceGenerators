// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

namespace ReactiveUI.Binding.Generator.Benchmarks;

/// <summary>
/// Builds consumer source for the generator to chew on: view-model and view pairs with a spread of call sites
/// across the observation and binding APIs.
/// </summary>
/// <remarks>
/// A corpus rather than one call site, because the emitter's cost is per invocation and per group; a single
/// call site measures mostly driver overhead and would hide whatever the emitter itself does.
/// </remarks>
internal static class GeneratorCorpus
{
    /// <summary>Roughly how many characters one view-model and view pair contributes.</summary>
    private const int PairSourceCapacity = 2_048;

    /// <summary>Builds a compilation unit containing the given number of view-model and view pairs.</summary>
    /// <param name="pairCount">How many view-model and view pairs to emit.</param>
    /// <returns>The source text.</returns>
    internal static string Build(int pairCount)
    {
        var sb = new StringBuilder(pairCount * PairSourceCapacity);

        _ = sb.AppendLine("using System;")
            .AppendLine("using System.ComponentModel;")
            .AppendLine("using System.Windows.Input;")
            .AppendLine("using ReactiveUI.Binding;")
            .AppendLine()
            .AppendLine("namespace Corpus")
            .AppendLine("{");

        for (var i = 0; i < pairCount; i++)
        {
            AppendPair(sb, i);
        }

        return sb.AppendLine("}").ToString();
    }

    /// <summary>Appends one view-model, view, and usage class.</summary>
    /// <param name="sb">The builder to append to.</param>
    /// <param name="index">The index that makes the emitted names unique.</param>
    private static void AppendPair(StringBuilder sb, int index)
    {
        AppendTypes(sb, index);
        AppendUsage(sb, index);
    }

    /// <summary>Appends the view model, child, button, and view for one pair.</summary>
    /// <param name="sb">The builder to append to.</param>
    /// <param name="index">The index that makes the emitted names unique.</param>
    private static void AppendTypes(StringBuilder sb, int index) =>
        sb.Append($$"""
                        public class Child{{index}} : INotifyPropertyChanged
                        {
                            public event PropertyChangedEventHandler PropertyChanged;

                            public string Nested { get; set; }
                        }

                        public class MyViewModel{{index}} : INotifyPropertyChanged
                        {
                            public event PropertyChangedEventHandler PropertyChanged;

                            public string Name { get; set; }

                            public int Count { get; set; }

                            public bool Flag { get; set; }

                            public Child{{index}} Child { get; set; }

                            public ICommand Save { get; set; }
                        }

                        public class MyButton{{index}}
                        {
                            public event EventHandler Click;
                        }

                        public class MyView{{index}} : IViewFor<MyViewModel{{index}}>
                        {
                            public MyViewModel{{index}} ViewModel { get; set; }

                            object IViewFor.ViewModel { get => ViewModel; set => ViewModel = (MyViewModel{{index}})value; }

                            public string NameText { get; set; }

                            public string CountText { get; set; }

                            public bool FlagValue { get; set; }

                            public MyButton{{index}} SaveButton { get; set; }
                        }

                """);

    /// <summary>Appends the call sites for one pair, spread across the observation and binding APIs.</summary>
    /// <param name="sb">The builder to append to.</param>
    /// <param name="index">The index that makes the emitted names unique.</param>
    private static void AppendUsage(StringBuilder sb, int index) =>
        sb.Append($$"""
                        public static class Usage{{index}}
                        {
                            public static IObservable<string> ObserveName(MyViewModel{{index}} vm) => vm.WhenChanged(x => x.Name);

                            public static IObservable<string> ObserveNested(MyViewModel{{index}} vm) => vm.WhenChanged(x => x.Child.Nested);

                            public static IObservable<(string, int)> ObserveBoth(MyViewModel{{index}} vm) => vm.WhenChanged(x => x.Name, x => x.Count);

                            public static IObservable<int> ObserveChanging(MyViewModel{{index}} vm) => vm.WhenChanging(x => x.Count);

                            public static IObservable<string> AnyValue(MyViewModel{{index}} vm) => vm.WhenAnyValue(x => x.Name);

                            public static IDisposable BindName(MyViewModel{{index}} vm, MyView{{index}} view) => vm.BindOneWay(view, x => x.Name, x => x.NameText);

                            public static IDisposable BindFlag(MyViewModel{{index}} vm, MyView{{index}} view) => vm.BindTwoWay(view, x => x.Flag, x => x.FlagValue);

                            public static IDisposable OneWay(MyViewModel{{index}} vm, MyView{{index}} view) => view.OneWayBind(vm, x => x.Name, x => x.NameText);

                            public static IReactiveBinding<MyView{{index}}, string> TwoWay(MyViewModel{{index}} vm, MyView{{index}} view) => view.Bind(vm, x => x.Name, x => x.NameText);

                            public static IDisposable Command(MyViewModel{{index}} vm, MyView{{index}} view) => view.BindCommand(vm, x => x.Save, x => x.SaveButton);

                            public static IDisposable ToTarget(MyViewModel{{index}} vm, MyView{{index}} view) => vm.WhenChanged(x => x.Name).BindTo(view, x => x.NameText);
                        }


                """);
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
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
    /// <summary>Opens the body of a corpus type.</summary>
    private const string TypeBodyOpen = "        {";

    /// <summary>Closes the body of a corpus type.</summary>
    private const string TypeBodyClose = "        }";

    /// <summary>Names the view model parameter and opens the view parameter of a corpus call site.</summary>
    private const string ViewModelAndViewParameters = " vm, MyView";

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendTypes(StringBuilder sb, int index) =>
        sb.Append("        public class Child").Append(index).AppendLine(" : INotifyPropertyChanged").AppendLine(TypeBodyOpen)
            .AppendLine("            public event PropertyChangedEventHandler PropertyChanged;").AppendLine()
            .AppendLine("            public string Nested { get; set; }").AppendLine(TypeBodyClose).AppendLine()
            .Append("        public class MyViewModel").Append(index).AppendLine(" : INotifyPropertyChanged").AppendLine(TypeBodyOpen)
            .AppendLine("            public event PropertyChangedEventHandler PropertyChanged;").AppendLine()
            .AppendLine("            public string Name { get; set; }").AppendLine().AppendLine("            public int Count { get; set; }")
            .AppendLine().AppendLine("            public bool Flag { get; set; }").AppendLine().Append("            public Child").Append(index)
            .AppendLine(" Child { get; set; }").AppendLine().AppendLine("            public ICommand Save { get; set; }").AppendLine(TypeBodyClose)
            .AppendLine().Append("        public class MyButton").Append(index).AppendLine().AppendLine(TypeBodyOpen)
            .AppendLine("            public event EventHandler Click;").AppendLine(TypeBodyClose).AppendLine().Append("        public class MyView")
            .Append(index).Append(" : IViewFor<MyViewModel").Append(index).AppendLine(">").AppendLine(TypeBodyOpen)
            .Append("            public MyViewModel").Append(index).AppendLine(" ViewModel { get; set; }").AppendLine()
            .Append("            object IViewFor.ViewModel { get => ViewModel; set => ViewModel = (MyViewModel").Append(index).AppendLine(")value; }")
            .AppendLine().AppendLine("            public string NameText { get; set; }").AppendLine()
            .AppendLine("            public string CountText { get; set; }").AppendLine()
            .AppendLine("            public bool FlagValue { get; set; }").AppendLine().Append("            public MyButton").Append(index)
            .AppendLine(" SaveButton { get; set; }").AppendLine(TypeBodyClose);

    /// <summary>Appends the call sites for one pair, spread across the observation and binding APIs.</summary>
    /// <param name="sb">The builder to append to.</param>
    /// <param name="index">The index that makes the emitted names unique.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendUsage(StringBuilder sb, int index) =>
        sb.Append("        public static class Usage").Append(index).AppendLine().AppendLine(TypeBodyOpen)
            .Append("            public static IObservable<string> ObserveName(MyViewModel").Append(index)
            .AppendLine(" vm) => vm.WhenChanged(x => x.Name);").AppendLine()
            .Append("            public static IObservable<string> ObserveNested(MyViewModel").Append(index)
            .AppendLine(" vm) => vm.WhenChanged(x => x.Child.Nested);").AppendLine()
            .Append("            public static IObservable<(string, int)> ObserveBoth(MyViewModel").Append(index)
            .AppendLine(" vm) => vm.WhenChanged(x => x.Name, x => x.Count);").AppendLine()
            .Append("            public static IObservable<int> ObserveChanging(MyViewModel").Append(index)
            .AppendLine(" vm) => vm.WhenChanging(x => x.Count);").AppendLine()
            .Append("            public static IObservable<string> AnyValue(MyViewModel").Append(index)
            .AppendLine(" vm) => vm.WhenAnyValue(x => x.Name);").AppendLine().Append("            public static IDisposable BindName(MyViewModel")
            .Append(index).Append(ViewModelAndViewParameters).Append(index).AppendLine(" view) => vm.BindOneWay(view, x => x.Name, x => x.NameText);").AppendLine()
            .Append("            public static IDisposable BindFlag(MyViewModel").Append(index).Append(ViewModelAndViewParameters).Append(index)
            .AppendLine(" view) => vm.BindTwoWay(view, x => x.Flag, x => x.FlagValue);").AppendLine()
            .Append("            public static IDisposable OneWay(MyViewModel").Append(index).Append(ViewModelAndViewParameters).Append(index)
            .AppendLine(" view) => view.OneWayBind(vm, x => x.Name, x => x.NameText);").AppendLine()
            .Append("            public static IReactiveBinding<MyView").Append(index).Append(", string> TwoWay(MyViewModel").Append(index)
            .Append(ViewModelAndViewParameters).Append(index).AppendLine(" view) => view.Bind(vm, x => x.Name, x => x.NameText);").AppendLine()
            .Append("            public static IDisposable Command(MyViewModel").Append(index).Append(ViewModelAndViewParameters).Append(index)
            .AppendLine(" view) => view.BindCommand(vm, x => x.Save, x => x.SaveButton);").AppendLine()
            .Append("            public static IDisposable ToTarget(MyViewModel").Append(index).Append(ViewModelAndViewParameters).Append(index)
            .AppendLine(" view) => vm.WhenChanged(x => x.Name).BindTo(view, x => x.NameText);").AppendLine(TypeBodyClose).AppendLine();
}

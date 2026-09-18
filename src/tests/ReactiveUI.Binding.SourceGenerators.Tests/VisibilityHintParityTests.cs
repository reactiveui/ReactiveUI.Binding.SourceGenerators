// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Executes the framework-specific visibility hint contracts in generated bindings.</summary>
public class VisibilityHintParityTests
{
    /// <summary>Inverse and hidden flags follow the selected framework's converters in both directions.</summary>
    /// <param name="framework">The framework enum namespace.</param>
    /// <param name="hintNamespace">The namespace declaring the hint enum.</param>
    /// <param name="supportsHidden">Whether this framework uses Hidden for a false value.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("System.Windows", "ReactiveUI", true)]
    [Arguments("Microsoft.Maui", "ReactiveUI", true)]
    [Arguments("Microsoft.UI.Xaml", "ReactiveUI", false)]
    [Arguments("Windows.UI.Xaml", "ReactiveUI.Uno", false)]
    [Arguments("System.Windows", "ReactiveUI.Reactive", true)]
    public async Task BindTo_UsesNativeVisibilityHints(string framework, string hintNamespace, bool supportsHidden)
    {
        var result = TestHelper.RunGenerator(Scenario(framework, hintNamespace, supportsHidden), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.GeneratedSourceDoesNotContain("BindToDispatch.g.cs", "RuntimeBindingConverter");
        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;
            await Assert.That((bool)run.Invoke(null, null)!).IsTrue();
        }
        finally
        {
            context.Unload();
        }
    }

    /// <summary>Builds a consumer covering every combination of the native visibility flags.</summary>
    /// <param name="framework">The framework enum namespace.</param>
    /// <param name="hintNamespace">The hint enum namespace.</param>
    /// <param name="supportsHidden">Whether Hidden is meaningful on this framework.</param>
    /// <returns>The executable consumer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Scenario(string framework, string hintNamespace, bool supportsHidden) => $$"""
        using System;
        using ReactiveUI.Binding;
        using Visibility = {{framework}}.Visibility;
        using Hint = {{hintNamespace}}.BooleanToVisibilityHint;
        namespace {{framework}} { public enum Visibility { Visible, Collapsed, Hidden } }
        namespace {{hintNamespace}} { [Flags] public enum BooleanToVisibilityHint { None = 0, Inverse = 2, UseHidden = 4 } }
        public class Target<T> { public T Value { get; set; } }
        public static class Usage
        {
            public static bool Run()
            {
                foreach (var hint in new[] { Hint.None, Hint.Inverse, Hint.UseHidden, Hint.Inverse | Hint.UseHidden })
                {
                    foreach (var value in new[] { true, false })
                    {
                        var visible = (hint & Hint.Inverse) != 0 ? !value : value;
                        var hidden = {{(supportsHidden ? "true" : "false")}} && (hint & Hint.UseHidden) != 0;
                        var expected = visible ? Visibility.Visible : hidden ? Visibility.Hidden : Visibility.Collapsed;
                        var target = new Target<Visibility>();
                        IObservable<bool> source = new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<bool>(value);
                        using (source.BindTo(target, x => x.Value, conversionHint: hint))
                            if (target.Value != expected) return false;
                    }
                    foreach (var visibility in new[] { Visibility.Visible, Visibility.Collapsed, Visibility.Hidden })
                    {
                        var expected = (visibility == Visibility.Visible) != ((hint & Hint.Inverse) != 0);
                        var target = new Target<bool>();
                        IObservable<Visibility> source = new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<Visibility>(visibility);
                        using (source.BindTo(target, x => x.Value, conversionHint: hint))
                            if (target.Value != expected) return false;
                    }
                }
                return true;
            }
        }
        """;
}

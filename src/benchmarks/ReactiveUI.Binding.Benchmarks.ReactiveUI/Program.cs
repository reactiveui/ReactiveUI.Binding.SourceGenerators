// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;

namespace ReactiveUI.Binding.Benchmarks.ReactiveUI;

/// <summary>Entry point for the ReactiveUI comparison benchmarks.</summary>
internal static class Program
{
    /// <summary>Runs the benchmarks selected by <paramref name="args"/>.</summary>
    /// <param name="args">The command line arguments passed to the benchmark switcher.</param>
    internal static void Main(string[] args) =>
        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}

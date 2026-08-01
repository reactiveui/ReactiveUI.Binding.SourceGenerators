// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;

namespace ReactiveUI.Binding.Generator.Benchmarks;

/// <summary>Entry point that hands the command line to the BenchmarkDotNet switcher.</summary>
internal static class Program
{
    /// <summary>Runs the benchmark selected by the command line.</summary>
    /// <param name="args">The command-line arguments passed to the switcher.</param>
    internal static void Main(string[] args) =>
        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}

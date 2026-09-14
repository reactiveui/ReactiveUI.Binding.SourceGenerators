// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using BenchmarkDotNet.Running;

namespace ReactiveUI.Binding.Benchmarks.Configs;

/// <summary>Runs the benchmarks on .NET with EventPipe.</summary>
public static class BenchmarkHost
{
    /// <summary>Runs the benchmarks the command line selects.</summary>
    /// <param name="assembly">The assembly holding the benchmarks.</param>
    /// <param name="args">The command line arguments passed to the benchmark switcher.</param>
    public static void Run(Assembly assembly, string[] args) =>
        _ = BenchmarkSwitcher.FromAssembly(assembly).Run(args, new BenchmarkConfig());
}

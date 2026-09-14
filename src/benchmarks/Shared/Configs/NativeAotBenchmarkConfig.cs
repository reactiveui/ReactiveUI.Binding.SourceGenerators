// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace ReactiveUI.Binding.Benchmarks.Configs;

/// <summary>Adds the NativeAOT runtimes to <see cref="BenchmarkConfig"/>, for benchmarks whose code publishes ahead of time.</summary>
public class NativeAotBenchmarkConfig : BenchmarkConfig
{
    /// <summary>Initializes a new instance of the <see cref="NativeAotBenchmarkConfig"/> class.</summary>
    public NativeAotBenchmarkConfig()
    {
        _ = AddJob(new Job(nameof(RuntimeMoniker.NativeAot10_0)).WithRuntime(NativeAotRuntime.Net10_0));
        _ = AddJob(new Job(nameof(RuntimeMoniker.NativeAot11_0)).WithRuntime(NativeAotRuntime.Net11_0));
    }
}

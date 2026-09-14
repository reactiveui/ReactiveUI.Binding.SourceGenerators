// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;

namespace ReactiveUI.Binding.Benchmarks.Configs;

/// <summary>Runs a benchmark on .NET 8, 10 and 11.</summary>
public class BenchmarkConfig : ProfilerConfig
{
    /// <summary>Initializes a new instance of the <see cref="BenchmarkConfig"/> class.</summary>
    public BenchmarkConfig()
    {
        _ = AddJob(new Job().WithRuntime(CoreRuntime.Core80));
        _ = AddJob(new Job().WithRuntime(CoreRuntime.Core10_0));
        _ = AddJob(new Job().WithRuntime(CoreRuntime.Core11_0));
        _ = AddExporter(MarkdownExporter.GitHub);
    }
}

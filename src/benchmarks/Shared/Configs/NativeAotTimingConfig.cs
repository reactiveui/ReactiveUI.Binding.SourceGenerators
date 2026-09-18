// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Filters;

namespace ReactiveUI.Binding.Benchmarks.Configs;

/// <summary>Runs NativeAOT timing jobs separately from EventPipe profiling.</summary>
/// <remarks>
/// EventPipe cannot attach to a NativeAOT process built with the default settings, and a run keeps one set of
/// diagnosers, so these jobs run apart from <see cref="BenchmarkConfig"/>. The filter keeps only the NativeAOT jobs
/// a benchmark class adds through <see cref="NativeAotBenchmarkConfig"/>.
/// </remarks>
public class NativeAotTimingConfig : ManualConfig
{
    /// <summary>Initializes a new instance of the <see cref="NativeAotTimingConfig"/> class.</summary>
    public NativeAotTimingConfig()
    {
        Add(DefaultConfig.Instance);
        _ = AddExporter(MarkdownExporter.GitHub);
        _ = AddFilter(new SimpleFilter(static benchmark => NativeAotBenchmarkConfig.IsNativeAotJob(benchmark.Job)));
        _ = WithOption(ConfigOptions.DontOverwriteResults, true);
    }
}

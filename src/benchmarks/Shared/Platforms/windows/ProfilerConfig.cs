// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;

namespace ReactiveUI.Binding.Benchmarks.Configs;

/// <summary>Traces every benchmark with ETW, which records CPU samples and GC events for .NET Framework and .NET alike.</summary>
public class ProfilerConfig : ManualConfig
{
    /// <summary>Initializes a new instance of the <see cref="ProfilerConfig"/> class.</summary>
    public ProfilerConfig() => _ = AddDiagnoser(new EtwProfiler(new EtwProfilerConfig()));
}

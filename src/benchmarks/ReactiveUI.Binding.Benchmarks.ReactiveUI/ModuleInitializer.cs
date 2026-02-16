// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

using ReactiveUI.Builder;

using Splat;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>
/// Module initializer that configures ReactiveUI before any benchmarks run.
/// </summary>
internal static class ModuleInitializer
{
    /// <summary>
    /// Initializes ReactiveUI using the builder pattern.
    /// </summary>
    [ModuleInitializer]
    internal static void Initialize()
    {
        ModeDetector.OverrideModeDetector(new BenchmarkModeDetector());
        RxAppBuilder.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();
    }

    /// <summary>
    /// Mode detector for benchmark context.
    /// </summary>
    private sealed class BenchmarkModeDetector : IModeDetector
    {
        public bool? InUnitTestRunner() => true;
    }
}

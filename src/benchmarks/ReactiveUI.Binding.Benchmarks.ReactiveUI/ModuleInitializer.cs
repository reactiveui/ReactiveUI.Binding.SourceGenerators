// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Builder;

using Splat;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>
/// One-shot initializer that configures ReactiveUI before any benchmarks run.
/// Call <see cref="EnsureInitialized"/> from each benchmark class's static constructor.
/// </summary>
internal static class ModuleInitializer
{
    private static int _initialized;

    /// <summary>
    /// Ensures ReactiveUI is initialized exactly once, regardless of how many
    /// benchmark classes call this method.
    /// </summary>
    internal static void EnsureInitialized()
    {
        if (Interlocked.Exchange(ref _initialized, 1) != 0)
        {
            return;
        }

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

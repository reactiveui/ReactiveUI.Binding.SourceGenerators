// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Mixins;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Builds the runtime services once per process, for the benchmarks that resolve through them.</summary>
/// <remarks>
/// The generated path needs none of this: it names every type and member it touches. Only the reflection
/// fallback resolves through the registered services, so only the benchmarks measuring it initialise them.
/// </remarks>
internal static class BindingInitializer
{
    /// <summary>Guards the one-shot build.</summary>
    private static int _initialized;

    /// <summary>Builds the core services, at most once however many benchmark classes ask.</summary>
    internal static void EnsureInitialized()
    {
        if (Interlocked.Exchange(ref _initialized, 1) != 0)
        {
            return;
        }

        _ = RxBindingBuilder.CreateReactiveUIBindingBuilder()
            .WithCoreServices()
            .BuildApp();
    }
}

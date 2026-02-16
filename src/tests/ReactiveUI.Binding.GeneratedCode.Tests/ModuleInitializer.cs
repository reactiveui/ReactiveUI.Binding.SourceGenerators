// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

using ReactiveUI.Binding.Builder;

[assembly: NotInParallel]

namespace ReactiveUI.Binding.GeneratedCode.Tests;

/// <summary>
/// Module initializer for the generated code test project.
/// </summary>
public static class ModuleInitializer
{
    /// <summary>
    /// Initializes the RxBinding builder for generated code tests.
    /// </summary>
    [ModuleInitializer]
    public static void Initialize()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        builder.WithCoreServices();
        builder.BuildApp();
    }
}

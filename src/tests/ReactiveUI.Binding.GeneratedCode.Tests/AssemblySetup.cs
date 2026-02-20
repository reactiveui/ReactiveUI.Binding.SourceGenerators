// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;

[assembly: NotInParallel]

namespace ReactiveUI.Binding.GeneratedCode.Tests;

/// <summary>
/// Assembly-level setup for the generated code test project.
/// </summary>
public static class AssemblySetup
{
    /// <summary>
    /// Initializes the RxBinding builder for generated code tests.
    /// </summary>
    [Before(Assembly)]
    public static void Initialize()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        builder.WithCoreServices();
        builder.BuildApp();
    }
}

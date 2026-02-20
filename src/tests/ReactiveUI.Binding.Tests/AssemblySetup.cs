// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;

[assembly: NotInParallel]

namespace ReactiveUI.Binding.Tests;

/// <summary>
/// Assembly-level setup for the test assembly.
/// </summary>
public static class AssemblySetup
{
    /// <summary>
    /// Initializes the test assembly by configuring the service locator.
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

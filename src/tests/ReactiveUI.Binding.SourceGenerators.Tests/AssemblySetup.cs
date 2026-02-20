// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

[assembly: NotInParallel]

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Assembly-level setup for source generator tests.
/// </summary>
public static class AssemblySetup
{
    /// <summary>
    /// Initializes the source generators.
    /// </summary>
    [Before(Assembly)]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
        VerifierSettings.UseSplitModeForUniqueDirectory();
    }
}

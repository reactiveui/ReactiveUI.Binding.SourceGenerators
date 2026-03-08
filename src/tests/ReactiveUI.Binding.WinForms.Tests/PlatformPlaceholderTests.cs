// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.WinForms.Tests;

/// <summary>
/// Placeholder test to ensure the test assembly has at least one runnable test.
/// </summary>
public class PlatformPlaceholderTests
{
    /// <summary>
    /// Verifies the WinForms test assembly is loaded and discoverable by the test runner.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsTestAssembly_IsDiscoverable()
    {
        await Assert.That(typeof(PlatformPlaceholderTests).Assembly).IsNotNull();
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

using TUnit.Core;
using TUnit.Core.Enums;

namespace ReactiveUI.Binding.WinForms.Tests;

/// <summary>
/// Placeholder test for non-Windows platforms where WinForms tests are excluded.
/// </summary>
[ExcludeOn(OS.Windows)]
public class PlatformPlaceholderTests
{
    /// <summary>
    /// Ensures the test assembly has at least one runnable test on non-Windows platforms.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsTests_SkippedOnNonWindows()
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        await Assert.That(isWindows).IsFalse();
    }
}

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Checks that registration choices are independent of preceding output formatting.</summary>
public class ChainRegistrationEmitterTests
{
    /// <summary>Windows and Unix line endings produce the same registration declaration and references.</summary>
    /// <param name="isBeforeChange">Whether the registration observes before-change notifications.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task AppendChoiceOpen_WithDifferentLineEndings_EmitsTheSameChoice(bool isBeforeChange)
    {
        const int generatedAffinity = 5;
        var unixOutput = new StringBuilder("// Prefix\n\n");
        var windowsOutput = new StringBuilder("// Prefix\r\n\r\n");
        var unixStart = unixOutput.Length;
        var windowsStart = windowsOutput.Length;
        var segment = ModelFactory.CreatePropertyPathSegment();

        ChainRegistrationEmitter.AppendChoiceOpen(unixOutput, "__p1", segment, generatedAffinity, isBeforeChange);
        ChainRegistrationEmitter.AppendChoiceOpen(windowsOutput, "__p1", segment, generatedAffinity, isBeforeChange);

        await Assert.That(windowsOutput.ToString(windowsStart, windowsOutput.Length - windowsStart))
            .IsEqualTo(unixOutput.ToString(unixStart, unixOutput.Length - unixStart));
    }
}

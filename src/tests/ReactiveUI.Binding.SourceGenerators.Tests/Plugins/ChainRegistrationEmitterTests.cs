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

    /// <summary>
    /// The arguments hand the registration everything it needs fixed at compile time, in the order the observable's
    /// constructor takes them, and end on the timing so each caller can finish the call its own way.
    /// </summary>
    /// <param name="isBeforeChange">Whether the registration observes before-change notifications.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task AppendPluginObservableArguments_WritesTheLinkInConstructorOrder(bool isBeforeChange)
    {
        const string indent = "  ";
        var segment = ModelFactory.CreatePropertyPathSegment();
        var sb = new StringBuilder();

        var returned = ChainRegistrationEmitter.AppendPluginObservableArguments(
            sb,
            indent,
            "__registration",
            "__source",
            segment,
            "global::System.Int32",
            isBeforeChange);

        string[] expected =
        [
            $"{indent}__registration,",
            $"{indent}__source,",
            $"{indent}((global::System.Linq.Expressions.Expression<global::System.Func<{segment.DeclaringTypeFullName}, global::System.Int32>>)(__e => __e.{segment.PropertyName})).Body,",
            $"{indent}\"{segment.PropertyName}\",",
            $"{indent}(object __o) => (({segment.DeclaringTypeFullName})__o).{segment.PropertyName},",
            $"{indent}{(isBeforeChange ? "true" : "false")}",
        ];

        await Assert.That(returned).IsSameReferenceAs(sb);
        await Assert.That(sb.ToString()).IsEqualTo(string.Join(Environment.NewLine, expected));
    }
}

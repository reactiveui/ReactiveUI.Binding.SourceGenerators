// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="InterceptorEmitter"/>.</summary>
public class InterceptorEmitterTests
{
    /// <summary>
    /// One generated method claims every call site that reaches its body, so it carries one attribute per call site,
    /// in call-site order, ahead of its declaration.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendClaimingMethodOpen_WritesOneAttributePerCallSiteThenOpensTheMethod()
    {
        const string declarationOpen = "        internal static global::System.IDisposable __Intercept_Probe_";
        List<InterceptorLocation> callSites = [new(1, "first"), new(1, "second")];
        var expected = new StringBuilder();
        InterceptorEmitter.AppendAttribute(expected, callSites[0], InterceptorEmitter.MemberIndent);
        InterceptorEmitter.AppendAttribute(expected, callSites[1], InterceptorEmitter.MemberIndent);
        _ = expected.Append(declarationOpen).Append("ABC").AppendLine("(");
        var actual = new StringBuilder();

        InterceptorEmitter.AppendClaimingMethodOpen(actual, callSites, static location => location, declarationOpen, "ABC");

        await Assert.That(actual.ToString()).IsEqualTo(expected.ToString());
        await Assert.That(actual.ToString().IndexOf("\"first\"", StringComparison.Ordinal))
            .IsLessThan(actual.ToString().IndexOf("\"second\"", StringComparison.Ordinal));
    }
}

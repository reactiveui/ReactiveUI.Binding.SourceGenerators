// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Checks native assembly membership and exported selectors for KVO eligibility.</summary>
public class KvoEligibilityParityTests
{
    /// <summary>Native declarations use Cocoa naming while consumer CLR properties fall through.</summary>
    /// <param name="name">The selected property.</param>
    /// <param name="expected">The native key, or null for an ordinary consumer property.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("Title", "title")]
    [Arguments("Enabled", "isEnabled")]
    [Arguments("IsHidden", "isHidden")]
    [Arguments("Count", "nativeCount")]
    [Arguments("Managed", null)]
    public async Task InspectProperty_RespectsNativeDeclaration(string name, string? expected)
    {
        var framework = TestHelper.CompileToReference(
            """
            namespace Foundation
            {
                public class NSObject {}
                public class ExportAttribute : System.Attribute { public ExportAttribute(string name) {} }
                public class Native : NSObject
                {
                    public string Title { get; set; }
                    public bool Enabled { get; set; }
                    public bool IsHidden { get; set; }
                    public int Count { [Export("nativeCount")] get; set; }
                }
            }
            """,
            "AppleFramework",
            LanguageVersion.CSharp10);
        var compilation = TestHelper.CreateCompilation(
            "public class Consumer : Foundation.Native { public int Managed { get; set; } }",
            LanguageVersion.CSharp10,
            false,
            "ConsumerAssembly",
            [framework]);
        var owner = compilation.GetTypeByMetadataName("Consumer")!;
        var property = (IPropertySymbol)PlatformSymbols.FindMember(owner, name)!;
        var selected = new KVOObservationPlugin().InspectProperty(owner, property);
        await Assert.That(selected?.KvoKeyPath).IsEqualTo(expected);
    }
}

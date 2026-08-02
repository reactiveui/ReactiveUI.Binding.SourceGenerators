// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Generators;
using ReactiveUI.Binding.SourceGenerators.Plugins;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Generators;

/// <summary>Tests for how observation helper declarations are selected and emitted.</summary>
public class ObservationHelperGeneratorTests
{
    /// <summary>An observation kind no plugin answers to.</summary>
    private const string UnknownKind = "NotAnObservationKind";

    /// <summary>Verifies that no detected types yield no helper kinds.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SelectHelperKinds_NoTypes_YieldsNothing()
    {
        var kinds = ObservationHelperGenerator.SelectHelperKinds([]);

        await Assert.That(kinds.Length).IsEqualTo(0);
    }

    /// <summary>Verifies that a kind whose plugin declares no helpers yields nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SelectHelperKinds_KindWithoutHelpers_YieldsNothing()
    {
        var kinds = ObservationHelperGenerator.SelectHelperKinds(["INPC"]);

        await Assert.That(kinds.Length).IsEqualTo(0);
    }

    /// <summary>Verifies that an unrecognised kind is ignored rather than selected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SelectHelperKinds_UnknownKind_YieldsNothing()
    {
        var kinds = ObservationHelperGenerator.SelectHelperKinds([UnknownKind]);

        await Assert.That(kinds.Length).IsEqualTo(0);
    }

    /// <summary>Verifies that repeats of one kind collapse, so another type of a seen kind changes nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SelectHelperKinds_RepeatedKind_CollapsesToOne()
    {
        var kinds = ObservationHelperGenerator.SelectHelperKinds(["KVO", "KVO", "KVO"]);

        await Assert.That(kinds.Length).IsEqualTo(1);
        await Assert.That(kinds[0]).IsEqualTo("KVO");
    }

    /// <summary>
    /// Verifies every kind the selection can yield resolves back to a plugin. This is the invariant that lets
    /// emission treat an unresolvable kind as impossible; a new plugin whose kind did not round-trip would
    /// silently declare nothing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SelectHelperKinds_EveryYieldedKind_ResolvesToAPlugin()
    {
        var everyKind = ImmutableArray.CreateBuilder<string>();
        for (var i = 0; i < ObservationPluginRegistry.Count; i++)
        {
            everyKind.Add(ObservationPluginRegistry.GetPlugin(i).ObservationKind);
        }

        var kinds = ObservationHelperGenerator.SelectHelperKinds(everyKind.ToImmutable());

        await Assert.That(kinds.Length).IsGreaterThan(0);
        for (var i = 0; i < kinds.Length; i++)
        {
            await Assert.That(ObservationPluginRegistry.GetPluginByKind(kinds[i])).IsNotNull();
        }
    }

    /// <summary>Verifies that a selected kind's declarations are appended.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendHelperDeclarations_KnownKind_AppendsItsDeclarations()
    {
        var sb = new StringBuilder();

        ObservationHelperGenerator.AppendHelperDeclarations(sb, new(["KVO"]));

        await Assert.That(sb.ToString()).Contains("__KVOObservable");
    }

    /// <summary>
    /// Verifies that a kind no plugin answers to contributes nothing and does not throw. A generator that threw
    /// on an unexpected kind would fail the consumer's build rather than merely generate less.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendHelperDeclarations_UnknownKind_AppendsNothing()
    {
        var sb = new StringBuilder();

        ObservationHelperGenerator.AppendHelperDeclarations(sb, new([UnknownKind]));

        await Assert.That(sb.Length).IsEqualTo(0);
    }
}

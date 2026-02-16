// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>
/// Tests for <see cref="ObservationCodeGenerator"/> helper methods exercised via WhenChanging scenarios.
/// </summary>
public class WhenChangingCodeGeneratorHelperTests
{
    /// <summary>
    /// Verifies GroupByTypeSignature groups invocations with the same source and return types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_SameTypes_GroupedTogether()
    {
        var inv1 = ModelFactory.CreateInvocationInfo(callerLineNumber: 10, isBeforeChange: true, methodName: "WhenChanging");
        var inv2 = ModelFactory.CreateInvocationInfo(callerLineNumber: 20, isBeforeChange: true, methodName: "WhenChanging");
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = ObservationCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(1);
        await Assert.That(groups[0].Invocations.Length).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies GroupByTypeSignature separates invocations with different source types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_DifferentSourceTypes_SeparateGroups()
    {
        var inv1 = ModelFactory.CreateInvocationInfo(sourceTypeFullName: "global::TestApp.ViewModelA", isBeforeChange: true);
        var inv2 = ModelFactory.CreateInvocationInfo(sourceTypeFullName: "global::TestApp.ViewModelB", isBeforeChange: true);
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = ObservationCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload dispatches to CallerArgExpr when supported.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExprSupported_GeneratesCallerArgExprOverload()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo(isBeforeChange: true, methodName: "WhenChanging");
        var group = new ObservationCodeGenerator.TypeGroup(inv, new[] { inv });

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, supportsCallerArgExpr: true, "WhenChanging");

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerArgumentExpression");
        await Assert.That(result).Contains("__WhenChanging_");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload dispatches to CallerFilePath when not supported.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExprNotSupported_GeneratesCallerFilePathOverload()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo(isBeforeChange: true, methodName: "WhenChanging");
        var group = new ObservationCodeGenerator.TypeGroup(inv, new[] { inv });

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, supportsCallerArgExpr: false, "WhenChanging");

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerFilePath");
        await Assert.That(result).Contains("callerLineNumber");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload with CallerArgExpr generates propertyExpression dispatch for WhenChanging.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExpr_GeneratesPropertyExpressionDispatch()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo(isBeforeChange: true, methodName: "WhenChanging");
        var group = new ObservationCodeGenerator.TypeGroup(inv, new[] { inv });

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, supportsCallerArgExpr: true, "WhenChanging");

        var result = sb.ToString();
        await Assert.That(result).Contains("Expression ==");
        await Assert.That(result).Contains("WhenChanging");
        await Assert.That(result).Contains("__WhenChanging_");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload with CallerFilePath generates file path + line number dispatch for WhenChanging.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerFilePath_GeneratesFilePathDispatch()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateInvocationInfo(
            callerFilePath: "/src/ViewModels/MyViewModel.cs",
            callerLineNumber: 42,
            isBeforeChange: true,
            methodName: "WhenChanging");
        var group = new ObservationCodeGenerator.TypeGroup(inv, new[] { inv });

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, supportsCallerArgExpr: false, "WhenChanging");

        var result = sb.ToString();
        await Assert.That(result).Contains("callerLineNumber == 42");
        await Assert.That(result).Contains("callerFilePath.EndsWith");
        await Assert.That(result).Contains("__WhenChanging_");
    }
}

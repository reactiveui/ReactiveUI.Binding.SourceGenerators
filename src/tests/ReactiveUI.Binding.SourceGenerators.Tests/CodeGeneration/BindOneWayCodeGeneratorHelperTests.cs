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
/// Tests for <see cref="BindOneWayCodeGenerator"/> helper methods.
/// </summary>
public class BindOneWayCodeGeneratorHelperTests
{
    /// <summary>
    /// Verifies GroupByTypeSignature groups invocations with the same type signature.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_SameSignature_GroupedTogether()
    {
        var inv1 = ModelFactory.CreateBindingInvocationInfo(callerLineNumber: 10);
        var inv2 = ModelFactory.CreateBindingInvocationInfo(callerLineNumber: 20);
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindOneWayCodeGenerator.GroupByTypeSignature(invocations);

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
        var inv1 = ModelFactory.CreateBindingInvocationInfo(sourceTypeFullName: "global::TestApp.ViewModelA");
        var inv2 = ModelFactory.CreateBindingInvocationInfo(sourceTypeFullName: "global::TestApp.ViewModelB");
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindOneWayCodeGenerator.GroupByTypeSignature(invocations);

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
        var inv = ModelFactory.CreateBindingInvocationInfo();
        var group = new BindOneWayCodeGenerator.BindingTypeGroup(
            "global::TestApp.MyViewModel",
            "global::TestApp.MyView",
            "global::System.String",
            "global::System.String",
            false,
            false,
            new[] { inv });

        BindOneWayCodeGenerator.GenerateConcreteOverload(sb, group, supportsCallerArgExpr: true);

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerArgumentExpression");
        await Assert.That(result).Contains("__BindOneWay_");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload dispatches to CallerFilePath when not supported.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExprNotSupported_GeneratesCallerFilePathOverload()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo();
        var group = new BindOneWayCodeGenerator.BindingTypeGroup(
            "global::TestApp.MyViewModel",
            "global::TestApp.MyView",
            "global::System.String",
            "global::System.String",
            false,
            false,
            new[] { inv });

        BindOneWayCodeGenerator.GenerateConcreteOverload(sb, group, supportsCallerArgExpr: false);

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerFilePath");
        await Assert.That(result).Contains("callerLineNumber");
    }

    /// <summary>
    /// Verifies GenerateCallerArgExprOverload generates dual expression dispatch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateCallerArgExprOverload_SingleInvocation_GeneratesDualExpressionDispatch()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo(
            sourceExpressionText: "x => x.Name",
            targetExpressionText: "x => x.Text");
        var group = new BindOneWayCodeGenerator.BindingTypeGroup(
            "global::TestApp.MyViewModel",
            "global::TestApp.MyView",
            "global::System.String",
            "global::System.String",
            false,
            false,
            new[] { inv });

        BindOneWayCodeGenerator.GenerateCallerArgExprOverload(sb, group);

        var result = sb.ToString();
        await Assert.That(result).Contains("sourcePropertyExpression == ");
        await Assert.That(result).Contains("targetPropertyExpression == ");
        await Assert.That(result).Contains("__BindOneWay_");
    }

    /// <summary>
    /// Verifies GenerateCallerFilePathOverload generates file path dispatch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateCallerFilePathOverload_SingleInvocation_GeneratesFilePathDispatch()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo(
            callerFilePath: "/src/Views/MyView.cs",
            callerLineNumber: 50);
        var group = new BindOneWayCodeGenerator.BindingTypeGroup(
            "global::TestApp.MyViewModel",
            "global::TestApp.MyView",
            "global::System.String",
            "global::System.String",
            false,
            false,
            new[] { inv });

        BindOneWayCodeGenerator.GenerateCallerFilePathOverload(sb, group);

        var result = sb.ToString();
        await Assert.That(result).Contains("callerLineNumber == 50");
        await Assert.That(result).Contains("callerFilePath.EndsWith");
        await Assert.That(result).Contains("__BindOneWay_");
    }

    /// <summary>
    /// Verifies GenerateBindOneWayMethod generates PropertyObservable + Subscribe pattern.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindOneWayMethod_StandardInvocation_GeneratesPropertyObservableSubscribe()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo();
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindOneWayCodeGenerator.GenerateBindOneWayMethod(sb, inv, classInfo, suffix: "TEST00000000TEST");

        var result = sb.ToString();
        await Assert.That(result).Contains("__BindOneWay_TEST00000000TEST");
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains("INotifyPropertyChanged");
        await Assert.That(result).Contains("Subscribe");
        await Assert.That(result).Contains("target.Text = value");
    }

    /// <summary>
    /// Verifies FormatExtraArgs returns empty when no conversion or scheduler.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_NoConversionNoScheduler_ReturnsEmpty()
    {
        var group = new BindOneWayCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.String",
            "global::System.String",
            false,
            false,
            Array.Empty<BindingInvocationInfo>());

        var result = BindOneWayCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>
    /// Verifies FormatExtraArgs includes conversion function when HasConversion is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_WithConversion_IncludesConversionFunc()
    {
        var group = new BindOneWayCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.String",
            "global::System.String",
            true,
            false,
            Array.Empty<BindingInvocationInfo>());

        var result = BindOneWayCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains("conversionFunc");
    }

    /// <summary>
    /// Verifies FormatExtraMethodParams returns empty when no extra params.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_NoConversionNoScheduler_ReturnsEmpty()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(
            hasConversion: false,
            hasScheduler: false);

        var result = BindOneWayCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>
    /// Verifies FormatExtraMethodParams includes Func type when HasConversion is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_WithConversion_IncludesFuncParam()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(hasConversion: true);

        var result = BindOneWayCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).Contains("global::System.Func<");
        await Assert.That(result).Contains("conversionFunc");
    }

    /// <summary>
    /// Verifies FormatExtraMethodParams includes IScheduler when HasScheduler is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_WithScheduler_IncludesSchedulerParam()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(hasScheduler: true);

        var result = BindOneWayCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).Contains("IScheduler");
        await Assert.That(result).Contains("scheduler");
    }

    /// <summary>
    /// Verifies GenerateBindOneWayMethod with conversion includes .Select chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindOneWayMethod_WithConversion_IncludesSelectChain()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo(hasConversion: true);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindOneWayCodeGenerator.GenerateBindOneWayMethod(sb, inv, classInfo, suffix: "TEST00000000TEST");

        var result = sb.ToString();
        await Assert.That(result).Contains("RxBindingExtensions.Select");
        await Assert.That(result).Contains("conversionFunc");
    }

    /// <summary>
    /// Verifies GenerateBindOneWayMethod with scheduler includes .ObserveOn chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindOneWayMethod_WithScheduler_IncludesObserveOn()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo(hasScheduler: true);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindOneWayCodeGenerator.GenerateBindOneWayMethod(sb, inv, classInfo, suffix: "TEST00000000TEST");

        var result = sb.ToString();
        await Assert.That(result).Contains("ObserveOn");
        await Assert.That(result).Contains("scheduler");
    }
}

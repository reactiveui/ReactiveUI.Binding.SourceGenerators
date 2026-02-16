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
/// Tests for <see cref="BindTwoWayCodeGenerator"/> helper methods.
/// </summary>
public class BindTwoWayCodeGeneratorHelperTests
{
    /// <summary>
    /// Verifies GroupByTypeSignature groups invocations with the same type signature.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_SameSignature_GroupedTogether()
    {
        var inv1 = ModelFactory.CreateBindingInvocationInfo(callerLineNumber: 10, isTwoWay: true, methodName: "BindTwoWay");
        var inv2 = ModelFactory.CreateBindingInvocationInfo(callerLineNumber: 20, isTwoWay: true, methodName: "BindTwoWay");
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindTwoWayCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(1);
        await Assert.That(groups[0].Invocations.Length).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies GroupByTypeSignature separates invocations with different target types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_DifferentTargetTypes_SeparateGroups()
    {
        var inv1 = ModelFactory.CreateBindingInvocationInfo(targetTypeFullName: "global::TestApp.ViewA", isTwoWay: true);
        var inv2 = ModelFactory.CreateBindingInvocationInfo(targetTypeFullName: "global::TestApp.ViewB", isTwoWay: true);
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindTwoWayCodeGenerator.GroupByTypeSignature(invocations);

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
        var inv = ModelFactory.CreateBindingInvocationInfo(isTwoWay: true, methodName: "BindTwoWay");
        var group = new BindTwoWayCodeGenerator.BindingTypeGroup(
            "global::TestApp.MyViewModel",
            "global::TestApp.MyView",
            "global::System.String",
            "global::System.String",
            false,
            false,
            new[] { inv });

        BindTwoWayCodeGenerator.GenerateConcreteOverload(sb, group, supportsCallerArgExpr: true);

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerArgumentExpression");
        await Assert.That(result).Contains("__BindTwoWay_");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload dispatches to CallerFilePath when not supported.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExprNotSupported_GeneratesCallerFilePathOverload()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo(isTwoWay: true, methodName: "BindTwoWay");
        var group = new BindTwoWayCodeGenerator.BindingTypeGroup(
            "global::TestApp.MyViewModel",
            "global::TestApp.MyView",
            "global::System.String",
            "global::System.String",
            false,
            false,
            new[] { inv });

        BindTwoWayCodeGenerator.GenerateConcreteOverload(sb, group, supportsCallerArgExpr: false);

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
            isTwoWay: true,
            methodName: "BindTwoWay",
            sourceExpressionText: "x => x.Name",
            targetExpressionText: "x => x.Text");
        var group = new BindTwoWayCodeGenerator.BindingTypeGroup(
            "global::TestApp.MyViewModel",
            "global::TestApp.MyView",
            "global::System.String",
            "global::System.String",
            false,
            false,
            new[] { inv });

        BindTwoWayCodeGenerator.GenerateCallerArgExprOverload(sb, group);

        var result = sb.ToString();
        await Assert.That(result).Contains("sourcePropertyExpression == ");
        await Assert.That(result).Contains("targetPropertyExpression == ");
        await Assert.That(result).Contains("__BindTwoWay_");
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
            callerLineNumber: 55,
            isTwoWay: true,
            methodName: "BindTwoWay");
        var group = new BindTwoWayCodeGenerator.BindingTypeGroup(
            "global::TestApp.MyViewModel",
            "global::TestApp.MyView",
            "global::System.String",
            "global::System.String",
            false,
            false,
            new[] { inv });

        BindTwoWayCodeGenerator.GenerateCallerFilePathOverload(sb, group);

        var result = sb.ToString();
        await Assert.That(result).Contains("callerLineNumber == 55");
        await Assert.That(result).Contains("callerFilePath.EndsWith");
        await Assert.That(result).Contains("__BindTwoWay_");
    }

    /// <summary>
    /// Verifies GenerateBindTwoWayMethod generates PropertyObservable + CompositeDisposable pattern.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindTwoWayMethod_StandardInvocation_GeneratesCompositeDisposable()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo(isTwoWay: true, methodName: "BindTwoWay");
        var sourceClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        var targetClassInfo = ModelFactory.CreateClassBindingInfo(
            fullyQualifiedName: "global::TestApp.MyView",
            metadataName: "MyView",
            implementsINPC: true);

        BindTwoWayCodeGenerator.GenerateBindTwoWayMethod(sb, inv, sourceClassInfo, targetClassInfo, suffix: "TEST00000000TEST");

        var result = sb.ToString();
        await Assert.That(result).Contains("__BindTwoWay_TEST00000000TEST");
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains("INotifyPropertyChanged");
        await Assert.That(result).Contains("CompositeDisposable");
        await Assert.That(result).Contains("target.Text = value");
        await Assert.That(result).Contains("source.Name = value");
    }

    /// <summary>
    /// Verifies FormatExtraArgs returns empty when no conversion or scheduler.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_NoConversionNoScheduler_ReturnsEmpty()
    {
        var group = new BindTwoWayCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.String",
            "global::System.String",
            false,
            false,
            Array.Empty<BindingInvocationInfo>());

        var result = BindTwoWayCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>
    /// Verifies FormatExtraArgs includes two-way converter args when HasConversion is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_WithConversion_IncludesTwoWayConverterArgs()
    {
        var group = new BindTwoWayCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.String",
            "global::System.String",
            true,
            false,
            Array.Empty<BindingInvocationInfo>());

        var result = BindTwoWayCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains("sourceToTargetConv");
        await Assert.That(result).Contains("targetToSourceConv");
    }

    /// <summary>
    /// Verifies FormatExtraArgs includes scheduler when HasScheduler is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_WithScheduler_IncludesSchedulerArg()
    {
        var group = new BindTwoWayCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.String",
            "global::System.String",
            false,
            true,
            Array.Empty<BindingInvocationInfo>());

        var result = BindTwoWayCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains("scheduler");
    }

    /// <summary>
    /// Verifies FormatExtraMethodParams returns empty when no conversion or scheduler.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_NoConversionNoScheduler_ReturnsEmpty()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(
            hasConversion: false, hasScheduler: false, isTwoWay: true, methodName: "BindTwoWay");

        var result = BindTwoWayCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>
    /// Verifies FormatExtraMethodParams includes two-way Func params when HasConversion is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_WithConversion_IncludesTwoWayFuncParams()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(
            hasConversion: true, isTwoWay: true, methodName: "BindTwoWay");

        var result = BindTwoWayCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).Contains("sourceToTargetConv");
        await Assert.That(result).Contains("targetToSourceConv");
        await Assert.That(result).Contains("global::System.Func<");
    }

    /// <summary>
    /// Verifies GenerateBindTwoWayMethod with conversion includes .Select chains.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindTwoWayMethod_WithConversion_IncludesSelectChains()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo(
            hasConversion: true, isTwoWay: true, methodName: "BindTwoWay");
        var sourceClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        var targetClassInfo = ModelFactory.CreateClassBindingInfo(
            fullyQualifiedName: "global::TestApp.MyView",
            metadataName: "MyView",
            implementsINPC: true);

        BindTwoWayCodeGenerator.GenerateBindTwoWayMethod(sb, inv, sourceClassInfo, targetClassInfo, suffix: "TEST00000000TEST");

        var result = sb.ToString();
        await Assert.That(result).Contains("ObservableExtensions.Select");
        await Assert.That(result).Contains("sourceToTargetConv");
        await Assert.That(result).Contains("targetToSourceConv");
    }

    /// <summary>
    /// Verifies GenerateBindTwoWayMethod with scheduler includes .ObserveOn chains.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindTwoWayMethod_WithScheduler_IncludesObserveOnChains()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo(
            hasScheduler: true, isTwoWay: true, methodName: "BindTwoWay");
        var sourceClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        var targetClassInfo = ModelFactory.CreateClassBindingInfo(
            fullyQualifiedName: "global::TestApp.MyView",
            metadataName: "MyView",
            implementsINPC: true);

        BindTwoWayCodeGenerator.GenerateBindTwoWayMethod(sb, inv, sourceClassInfo, targetClassInfo, suffix: "TEST00000000TEST");

        var result = sb.ToString();
        await Assert.That(result).Contains("ObserveOn");
        await Assert.That(result).Contains("scheduler");
    }

    /// <summary>
    /// Verifies GenerateBindTwoWayMethod generates Skip(1) on target observable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindTwoWayMethod_AlwaysGeneratesSkipOne()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo(isTwoWay: true, methodName: "BindTwoWay");
        var sourceClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        var targetClassInfo = ModelFactory.CreateClassBindingInfo(
            fullyQualifiedName: "global::TestApp.MyView",
            metadataName: "MyView",
            implementsINPC: true);

        BindTwoWayCodeGenerator.GenerateBindTwoWayMethod(sb, inv, sourceClassInfo, targetClassInfo, suffix: "TEST00000000TEST");

        var result = sb.ToString();
        await Assert.That(result).Contains("Skip");
    }

    /// <summary>
    /// Verifies AppendExtraParameters appends conversion parameters with correct types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendExtraParameters_WithConversion_AppendsConverterParamsWithTypes()
    {
        var sb = new StringBuilder();
        var group = new BindTwoWayCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.Int32",
            "global::System.String",
            true,
            false,
            Array.Empty<BindingInvocationInfo>());

        BindTwoWayCodeGenerator.AppendExtraParameters(sb, group);

        var result = sb.ToString();
        await Assert.That(result).Contains("global::System.Func<global::System.Int32, global::System.String>");
        await Assert.That(result).Contains("global::System.Func<global::System.String, global::System.Int32>");
    }
}

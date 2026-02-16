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
/// Tests for <see cref="BindCodeGenerator"/> helper methods.
/// </summary>
public class BindCodeGeneratorHelperTests
{
    /// <summary>
    /// Verifies GroupByTypeSignature groups invocations with the same type signature.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_SameSignature_GroupedTogether()
    {
        var inv1 = ModelFactory.CreateBindingInvocationInfo(callerLineNumber: 10, methodName: "Bind");
        var inv2 = ModelFactory.CreateBindingInvocationInfo(callerLineNumber: 20, methodName: "Bind");
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindCodeGenerator.GroupByTypeSignature(invocations);

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
        var inv1 = ModelFactory.CreateBindingInvocationInfo(sourceTypeFullName: "global::TestApp.ViewModelA", methodName: "Bind");
        var inv2 = ModelFactory.CreateBindingInvocationInfo(sourceTypeFullName: "global::TestApp.ViewModelB", methodName: "Bind");
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies GroupByTypeSignature separates invocations with HasConversion difference.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_DifferentHasConversion_SeparateGroups()
    {
        var inv1 = ModelFactory.CreateBindingInvocationInfo(hasConversion: false, methodName: "Bind");
        var inv2 = ModelFactory.CreateBindingInvocationInfo(hasConversion: true, methodName: "Bind");
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies FormatExtraArgs returns empty string when no conversion or scheduler.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_NoConversionNoScheduler_ReturnsEmpty()
    {
        var group = new BindCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.String",
            "global::System.String",
            false,
            false,
            Array.Empty<BindingInvocationInfo>());

        var result = BindCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>
    /// Verifies FormatExtraArgs includes converter args when HasConversion is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_WithConversion_IncludesConverterArgs()
    {
        var group = new BindCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.String",
            "global::System.String",
            true,
            false,
            Array.Empty<BindingInvocationInfo>());

        var result = BindCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains("vmToViewConverter");
        await Assert.That(result).Contains("viewToVmConverter");
    }

    /// <summary>
    /// Verifies FormatExtraArgs includes scheduler arg when HasScheduler is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_WithScheduler_IncludesSchedulerArg()
    {
        var group = new BindCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.String",
            "global::System.String",
            false,
            true,
            Array.Empty<BindingInvocationInfo>());

        var result = BindCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains("scheduler");
    }

    /// <summary>
    /// Verifies FormatExtraArgs includes both converter and scheduler when both are true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_WithConversionAndScheduler_IncludesBoth()
    {
        var group = new BindCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.String",
            "global::System.String",
            true,
            true,
            Array.Empty<BindingInvocationInfo>());

        var result = BindCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains("vmToViewConverter");
        await Assert.That(result).Contains("scheduler");
    }

    /// <summary>
    /// Verifies FormatReturnType produces expected IReactiveBinding type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatReturnType_ReturnsIReactiveBindingType()
    {
        var group = new BindCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.String",
            "global::System.String",
            false,
            false,
            Array.Empty<BindingInvocationInfo>());

        var result = BindCodeGenerator.FormatReturnType(group);

        await Assert.That(result).Contains("IReactiveBinding");
        await Assert.That(result).Contains("global::TestApp.View");
    }

    /// <summary>
    /// Verifies FormatMethodReturnType produces expected IReactiveBinding type for invocation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatMethodReturnType_ReturnsIReactiveBindingType()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(methodName: "Bind");

        var result = BindCodeGenerator.FormatMethodReturnType(inv);

        await Assert.That(result).Contains("IReactiveBinding");
        await Assert.That(result).Contains("global::TestApp.MyView");
    }

    /// <summary>
    /// Verifies FormatExtraMethodParams returns empty when no conversion or scheduler.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_NoConversionNoScheduler_ReturnsEmpty()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(hasConversion: false, hasScheduler: false, methodName: "Bind");

        var result = BindCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>
    /// Verifies FormatExtraMethodParams includes Func types when HasConversion is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_WithConversion_IncludesFuncParams()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(hasConversion: true, methodName: "Bind");

        var result = BindCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).Contains("global::System.Func<");
        await Assert.That(result).Contains("vmToViewConverter");
        await Assert.That(result).Contains("viewToVmConverter");
    }

    /// <summary>
    /// Verifies FormatExtraMethodParams includes IScheduler when HasScheduler is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_WithScheduler_IncludesSchedulerParam()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(hasScheduler: true, methodName: "Bind");

        var result = BindCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).Contains("IScheduler");
        await Assert.That(result).Contains("scheduler");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload dispatches to CallerArgExpr when supported.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExpr_GeneratesExpressionDispatch()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo(methodName: "Bind");
        var group = new BindCodeGenerator.BindingTypeGroup(
            "global::TestApp.MyViewModel",
            "global::TestApp.MyView",
            "global::System.String",
            "global::System.String",
            false,
            false,
            new[] { inv });

        BindCodeGenerator.GenerateConcreteOverload(sb, group, supportsCallerArgExpr: true);

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerArgumentExpression");
        await Assert.That(result).Contains("__Bind_");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload dispatches to CallerFilePath when not supported.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerFilePath_GeneratesFilePathDispatch()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo(methodName: "Bind");
        var group = new BindCodeGenerator.BindingTypeGroup(
            "global::TestApp.MyViewModel",
            "global::TestApp.MyView",
            "global::System.String",
            "global::System.String",
            false,
            false,
            new[] { inv });

        BindCodeGenerator.GenerateConcreteOverload(sb, group, supportsCallerArgExpr: false);

        var result = sb.ToString();
        await Assert.That(result).Contains("callerFilePath.EndsWith");
        await Assert.That(result).Contains("callerLineNumber");
    }

    /// <summary>
    /// Verifies GenerateBindMethod generates two-way binding with inline PropertyObservable and view-first parameter ordering.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindMethod_StandardInvocation_GeneratesTwoWayBinding()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo(methodName: "Bind");
        var sourceClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        var targetClassInfo = ModelFactory.CreateClassBindingInfo(
            fullyQualifiedName: "global::TestApp.MyView",
            metadataName: "MyView",
            implementsINPC: true);

        BindCodeGenerator.GenerateBindMethod(sb, inv, sourceClassInfo, targetClassInfo, suffix: "TEST00000000TEST");

        var result = sb.ToString();
        await Assert.That(result).Contains("__Bind_TEST00000000TEST");
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains("INotifyPropertyChanged");
        await Assert.That(result).Contains("ReactiveBinding");
        await Assert.That(result).Contains("view.Text = value");
        await Assert.That(result).Contains("viewModel.Name = value");
        await Assert.That(result).Contains("Skip");
    }

    /// <summary>
    /// Verifies AppendExtraParameters appends conversion parameters.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendExtraParameters_WithConversion_AppendsConverterParams()
    {
        var sb = new StringBuilder();
        var group = new BindCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.String",
            "global::System.Int32",
            true,
            false,
            Array.Empty<BindingInvocationInfo>());

        BindCodeGenerator.AppendExtraParameters(sb, group);

        var result = sb.ToString();
        await Assert.That(result).Contains("vmToViewConverter");
        await Assert.That(result).Contains("viewToVmConverter");
        await Assert.That(result).Contains("global::System.Func<global::System.String, global::System.Int32>");
    }

    /// <summary>
    /// Verifies AppendExtraParameters appends scheduler parameter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendExtraParameters_WithScheduler_AppendsSchedulerParam()
    {
        var sb = new StringBuilder();
        var group = new BindCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.String",
            "global::System.String",
            false,
            true,
            Array.Empty<BindingInvocationInfo>());

        BindCodeGenerator.AppendExtraParameters(sb, group);

        var result = sb.ToString();
        await Assert.That(result).Contains("IScheduler");
    }
}

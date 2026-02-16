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
/// Tests for <see cref="OneWayBindCodeGenerator"/> helper methods.
/// </summary>
public class OneWayBindCodeGeneratorHelperTests
{
    /// <summary>
    /// Verifies GroupByTypeSignature groups invocations with the same type signature.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_SameSignature_GroupedTogether()
    {
        var inv1 = ModelFactory.CreateBindingInvocationInfo(callerLineNumber: 10, methodName: "OneWayBind");
        var inv2 = ModelFactory.CreateBindingInvocationInfo(callerLineNumber: 20, methodName: "OneWayBind");
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = OneWayBindCodeGenerator.GroupByTypeSignature(invocations);

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
        var inv1 = ModelFactory.CreateBindingInvocationInfo(targetTypeFullName: "global::TestApp.ViewA", methodName: "OneWayBind");
        var inv2 = ModelFactory.CreateBindingInvocationInfo(targetTypeFullName: "global::TestApp.ViewB", methodName: "OneWayBind");
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = OneWayBindCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies FormatExtraArgs returns empty when no conversion or scheduler.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_NoConversionNoScheduler_ReturnsEmpty()
    {
        var group = new OneWayBindCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.String",
            "global::System.String",
            false,
            false,
            Array.Empty<BindingInvocationInfo>());

        var result = OneWayBindCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>
    /// Verifies FormatExtraArgs includes selector when HasConversion is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_WithConversion_IncludesSelectorArg()
    {
        var group = new OneWayBindCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.String",
            "global::System.String",
            true,
            false,
            Array.Empty<BindingInvocationInfo>());

        var result = OneWayBindCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains("selector");
    }

    /// <summary>
    /// Verifies FormatExtraArgs includes scheduler when HasScheduler is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_WithScheduler_IncludesSchedulerArg()
    {
        var group = new OneWayBindCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.String",
            "global::System.String",
            false,
            true,
            Array.Empty<BindingInvocationInfo>());

        var result = OneWayBindCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains("scheduler");
    }

    /// <summary>
    /// Verifies FormatReturnType without conversion uses source property type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatReturnType_NoConversion_UsesSourcePropertyType()
    {
        var group = new OneWayBindCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.Int32",
            "global::System.String",
            false,
            false,
            Array.Empty<BindingInvocationInfo>());

        var result = OneWayBindCodeGenerator.FormatReturnType(group);

        await Assert.That(result).Contains("IReactiveBinding");
        await Assert.That(result).Contains("global::System.Int32");
    }

    /// <summary>
    /// Verifies FormatReturnType with conversion uses target property type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatReturnType_WithConversion_UsesTargetPropertyType()
    {
        var group = new OneWayBindCodeGenerator.BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            "global::System.Int32",
            "global::System.String",
            true,
            false,
            Array.Empty<BindingInvocationInfo>());

        var result = OneWayBindCodeGenerator.FormatReturnType(group);

        await Assert.That(result).Contains("IReactiveBinding");
        await Assert.That(result).Contains("global::System.String");
    }

    /// <summary>
    /// Verifies FormatMethodReturnType uses target property type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatMethodReturnType_ReturnsTargetPropertyType()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(
            targetPropertyTypeFullName: "global::System.Int32",
            methodName: "OneWayBind");

        var result = OneWayBindCodeGenerator.FormatMethodReturnType(inv);

        await Assert.That(result).Contains("IReactiveBinding");
        await Assert.That(result).Contains("global::System.Int32");
    }

    /// <summary>
    /// Verifies FormatExtraMethodParams returns empty when no conversion or scheduler.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_NoConversionNoScheduler_ReturnsEmpty()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(
            hasConversion: false,
            hasScheduler: false,
            methodName: "OneWayBind");

        var result = OneWayBindCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>
    /// Verifies FormatExtraMethodParams includes Func selector when HasConversion is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_WithConversion_IncludesFuncSelector()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(hasConversion: true, methodName: "OneWayBind");

        var result = OneWayBindCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).Contains("global::System.Func<");
        await Assert.That(result).Contains("selector");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload with CallerArgExpr generates expression dispatch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExpr_GeneratesExpressionDispatch()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo(methodName: "OneWayBind");
        var group = new OneWayBindCodeGenerator.BindingTypeGroup(
            "global::TestApp.MyViewModel",
            "global::TestApp.MyView",
            "global::System.String",
            "global::System.String",
            false,
            false,
            new[] { inv });

        OneWayBindCodeGenerator.GenerateConcreteOverload(sb, group, supportsCallerArgExpr: true);

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerArgumentExpression");
        await Assert.That(result).Contains("__OneWayBind_");
    }

    /// <summary>
    /// Verifies GenerateOneWayBindMethod generates PropertyObservable + Subscribe + ReactiveBinding.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateOneWayBindMethod_StandardInvocation_GeneratesReactiveBinding()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindingInvocationInfo(methodName: "OneWayBind");
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        OneWayBindCodeGenerator.GenerateOneWayBindMethod(sb, inv, classInfo, suffix: "TEST00000000TEST");

        var result = sb.ToString();
        await Assert.That(result).Contains("__OneWayBind_TEST00000000TEST");
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains("INotifyPropertyChanged");
        await Assert.That(result).Contains("ReactiveBinding");
        await Assert.That(result).Contains("BindingDirection.OneWay");
        await Assert.That(result).Contains("view.Text = value");
    }
}

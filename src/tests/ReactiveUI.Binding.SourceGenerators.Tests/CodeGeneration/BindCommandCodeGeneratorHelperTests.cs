// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>
/// Tests for <see cref="BindCommandCodeGenerator"/> helper methods and command binding plugins.
/// </summary>
public class BindCommandCodeGeneratorHelperTests
{
    /// <summary>
    /// Verifies CommandPropertyBindingPlugin.CanHandle returns true when HasCommandProperty is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_CanHandle_WithCommandProperty_ReturnsTrue()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(hasCommandProperty: true);

        var plugin = new CommandPropertyBindingPlugin();
        var result = plugin.CanHandle(inv);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies CommandPropertyBindingPlugin.CanHandle returns false when HasCommandProperty is false.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_CanHandle_WithoutCommandProperty_ReturnsFalse()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(hasCommandProperty: false);

        var plugin = new CommandPropertyBindingPlugin();
        var result = plugin.CanHandle(inv);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies CommandPropertyBindingPlugin emits Command+CommandParameter+observable parameter code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_EmitBinding_ObservableParam_EmitsVolatilePattern()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: "global::System.String",
            hasCommandProperty: true,
            hasCommandParameterProperty: true);

        var plugin = new CommandPropertyBindingPlugin();
        plugin.EmitBinding(sb, inv, "view.SaveButton", false);

        var result = sb.ToString();
        await Assert.That(result).Contains("Volatile.Write(ref __latestParam, p)");
        await Assert.That(result).Contains("view.SaveButton.Command = cmd");
        await Assert.That(result).Contains("view.SaveButton.CommandParameter = param");
        await Assert.That(result).Contains("CompositeDisposable2");
    }

    /// <summary>
    /// Verifies CommandPropertyBindingPlugin emits Command+CommandParameter+expression parameter code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_EmitBinding_ExpressionParam_EmitsDirectAccess()
    {
        var paramPath = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Param", "global::System.String", "global::TestApp.MyViewModel")]);
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasExpressionParameter: true,
            parameterTypeFullName: "global::System.String",
            parameterPropertyPath: paramPath,
            hasCommandProperty: true,
            hasCommandParameterProperty: true);

        var plugin = new CommandPropertyBindingPlugin();
        plugin.EmitBinding(sb, inv, "view.SaveButton", false);

        var result = sb.ToString();
        await Assert.That(result).Contains("view.SaveButton.Command = cmd");
        await Assert.That(result).Contains("view.SaveButton.CommandParameter = viewModel.Param");
        await Assert.That(result).DoesNotContain("Volatile");
    }

    /// <summary>
    /// Verifies CommandPropertyBindingPlugin emits Command-only code when no parameter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_EmitBinding_NoParam_EmitsCommandOnly()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasCommandProperty: true,
            hasCommandParameterProperty: false);

        var plugin = new CommandPropertyBindingPlugin();
        plugin.EmitBinding(sb, inv, "view.SaveButton", false);

        var result = sb.ToString();
        await Assert.That(result).Contains("view.SaveButton.Command = cmd");
        await Assert.That(result).DoesNotContain("CommandParameter");
        await Assert.That(result).DoesNotContain("Volatile");
    }

    /// <summary>
    /// Verifies EventEnabledBindingPlugin.CanHandle returns true when event and Enabled property exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_CanHandle_WithEventAndEnabled_ReturnsTrue()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: "Click",
            hasEnabledProperty: true);

        var plugin = new EventEnabledBindingPlugin();
        var result = plugin.CanHandle(inv);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies EventEnabledBindingPlugin.CanHandle returns false when no event is resolved.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_CanHandle_WithoutEvent_ReturnsFalse()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: null,
            hasEnabledProperty: true);

        var plugin = new EventEnabledBindingPlugin();
        var result = plugin.CanHandle(inv);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies EventEnabledBindingPlugin.CanHandle returns false when no Enabled property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_CanHandle_WithoutEnabled_ReturnsFalse()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: "Click",
            hasEnabledProperty: false);

        var plugin = new EventEnabledBindingPlugin();
        var result = plugin.CanHandle(inv);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies EventEnabledBindingPlugin emits event+Enabled with observable parameter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_EmitBinding_ObservableParam_EmitsCanExecuteSync()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: "global::System.String",
            resolvedEventName: "Click",
            hasEnabledProperty: true);

        var plugin = new EventEnabledBindingPlugin();
        plugin.EmitBinding(sb, inv, "view.SaveButton", false);

        var result = sb.ToString();
        await Assert.That(result).Contains("view.SaveButton.Enabled = false");
        await Assert.That(result).Contains("view.SaveButton.Enabled = cmd.CanExecute(param)");
        await Assert.That(result).Contains("cmd.CanExecuteChanged += __canExecHandler");
        await Assert.That(result).Contains("view.SaveButton.Click += __Handler");
        await Assert.That(result).Contains("Volatile.Read(ref __latestParam)");
    }

    /// <summary>
    /// Verifies EventEnabledBindingPlugin emits event+Enabled with expression parameter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_EmitBinding_ExpressionParam_EmitsDirectPropertyAccess()
    {
        var paramPath = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Param", "global::System.String", "global::TestApp.MyViewModel")]);
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasExpressionParameter: true,
            parameterTypeFullName: "global::System.String",
            parameterPropertyPath: paramPath,
            resolvedEventName: "Click",
            hasEnabledProperty: true);

        var plugin = new EventEnabledBindingPlugin();
        plugin.EmitBinding(sb, inv, "view.SaveButton", false);

        var result = sb.ToString();
        await Assert.That(result).Contains("view.SaveButton.Enabled = cmd.CanExecute(viewModel.Param)");
        await Assert.That(result).Contains("view.SaveButton.Click += __Handler");
        await Assert.That(result).DoesNotContain("Volatile");
    }

    /// <summary>
    /// Verifies EventEnabledBindingPlugin emits event+Enabled with no parameter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_EmitBinding_NoParam_EmitsNullParam()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: "Click",
            hasEnabledProperty: true);

        var plugin = new EventEnabledBindingPlugin();
        plugin.EmitBinding(sb, inv, "view.SaveButton", false);

        var result = sb.ToString();
        await Assert.That(result).Contains("view.SaveButton.Enabled = cmd.CanExecute(null)");
        await Assert.That(result).Contains("cmd.Execute(null)");
        await Assert.That(result).Contains("view.SaveButton.Click += __Handler");
    }

    /// <summary>
    /// Verifies EventEnabledBindingPlugin uses fallback EventArgs type when null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_EmitBinding_NullEventArgsType_UsesFallback()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: "Click",
            resolvedEventArgsTypeFullName: null,
            hasEnabledProperty: true);

        var plugin = new EventEnabledBindingPlugin();
        plugin.EmitBinding(sb, inv, "view.SaveButton", false);

        var result = sb.ToString();
        await Assert.That(result).Contains("global::System.EventArgs");
    }

    /// <summary>
    /// Verifies DefaultEventBindingPlugin.CanHandle returns true when event is resolved.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultEventPlugin_CanHandle_WithEvent_ReturnsTrue()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(resolvedEventName: "Click");

        var plugin = new DefaultEventBindingPlugin();
        var result = plugin.CanHandle(inv);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies DefaultEventBindingPlugin.CanHandle returns false when no event.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultEventPlugin_CanHandle_WithoutEvent_ReturnsFalse()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(resolvedEventName: null);

        var plugin = new DefaultEventBindingPlugin();
        var result = plugin.CanHandle(inv);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies DefaultEventBindingPlugin.EmitBinding uses fallback EventArgs when ResolvedEventArgsTypeFullName is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultEventPlugin_EmitBinding_NullEventArgsType_UsesFallback()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: "Click",
            resolvedEventArgsTypeFullName: null);

        var plugin = new DefaultEventBindingPlugin();
        plugin.EmitBinding(sb, inv, "view.SaveButton", false);

        var result = sb.ToString();
        await Assert.That(result).Contains("global::System.EventArgs");
    }

    /// <summary>
    /// Verifies DefaultEventBindingPlugin.EmitBinding uses specific EventArgs when provided.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultEventPlugin_EmitBinding_WithEventArgsType_UsesSpecificType()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: "Click",
            resolvedEventArgsTypeFullName: "global::System.Windows.RoutedEventArgs");

        var plugin = new DefaultEventBindingPlugin();
        plugin.EmitBinding(sb, inv, "view.SaveButton", false);

        var result = sb.ToString();
        await Assert.That(result).Contains("global::System.Windows.RoutedEventArgs");
    }

    /// <summary>
    /// Verifies GroupByTypeSignature groups BindCommand invocations with the same type signature.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_SameSignature_GroupedTogether()
    {
        var inv1 = ModelFactory.CreateBindCommandInvocationInfo(callerLineNumber: 10);
        var inv2 = ModelFactory.CreateBindCommandInvocationInfo(callerLineNumber: 20);
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindCommandCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(1);
        await Assert.That(groups[0].Invocations.Length).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies GroupByTypeSignature separates invocations with different view types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_DifferentViewTypes_SeparateGroups()
    {
        var inv1 = ModelFactory.CreateBindCommandInvocationInfo(viewTypeFullName: "global::TestApp.ViewA");
        var inv2 = ModelFactory.CreateBindCommandInvocationInfo(viewTypeFullName: "global::TestApp.ViewB");
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindCommandCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload generates CallerArgumentExpression dispatch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExpr_GeneratesExpressionDispatch()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo();
        var group = new BindCommandCodeGenerator.BindCommandTypeGroup(
            inv.ViewTypeFullName,
            inv.ViewModelTypeFullName,
            inv.CommandTypeFullName,
            inv.ControlTypeFullName,
            false,
            false,
            null,
            [inv]);

        BindCommandCodeGenerator.GenerateConcreteOverload(sb, group, true, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerArgumentExpression");
        await Assert.That(result).Contains("__BindCommand_");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload generates CallerFilePath dispatch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerFilePath_GeneratesFilePathDispatch()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo();
        var group = new BindCommandCodeGenerator.BindCommandTypeGroup(
            inv.ViewTypeFullName,
            inv.ViewModelTypeFullName,
            inv.CommandTypeFullName,
            inv.ControlTypeFullName,
            false,
            false,
            null,
            [inv]);

        BindCommandCodeGenerator.GenerateConcreteOverload(sb, group, false, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("callerFilePath.EndsWith");
        await Assert.That(result).Contains("callerLineNumber");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload with observable parameter includes withParameter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateCallerArgExprOverload_WithObservableParam_IncludesWithParameter()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: "global::System.String");
        var group = new BindCommandCodeGenerator.BindCommandTypeGroup(
            inv.ViewTypeFullName,
            inv.ViewModelTypeFullName,
            inv.CommandTypeFullName,
            inv.ControlTypeFullName,
            true,
            false,
            "global::System.String",
            [inv]);

        BindCommandCodeGenerator.GenerateCallerArgExprOverload(sb, group, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("IObservable<global::System.String> withParameter");
        await Assert.That(result).Contains(", withParameter)");
    }

    /// <summary>
    /// Verifies GenerateConcreteOverload with expression parameter includes withParameter expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateCallerArgExprOverload_WithExpressionParam_IncludesWithParameterExpr()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasExpressionParameter: true,
            parameterTypeFullName: "global::System.String");
        var group = new BindCommandCodeGenerator.BindCommandTypeGroup(
            inv.ViewTypeFullName,
            inv.ViewModelTypeFullName,
            inv.CommandTypeFullName,
            inv.ControlTypeFullName,
            false,
            true,
            "global::System.String",
            [inv]);

        BindCommandCodeGenerator.GenerateCallerArgExprOverload(sb, group, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("Expression<global::System.Func<");
        await Assert.That(result).Contains("withParameterExpression");
    }

    /// <summary>
    /// Verifies CallerFilePath overload with observable parameter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateCallerFilePathOverload_WithObservableParam_IncludesWithParameter()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: "global::System.String");
        var group = new BindCommandCodeGenerator.BindCommandTypeGroup(
            inv.ViewTypeFullName,
            inv.ViewModelTypeFullName,
            inv.CommandTypeFullName,
            inv.ControlTypeFullName,
            true,
            false,
            "global::System.String",
            [inv]);

        BindCommandCodeGenerator.GenerateCallerFilePathOverload(sb, group, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("IObservable<global::System.String> withParameter");
    }

    /// <summary>
    /// Verifies CallerFilePath overload with expression parameter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateCallerFilePathOverload_WithExpressionParam_IncludesWithParameter()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasExpressionParameter: true,
            parameterTypeFullName: "global::System.String");
        var group = new BindCommandCodeGenerator.BindCommandTypeGroup(
            inv.ViewTypeFullName,
            inv.ViewModelTypeFullName,
            inv.CommandTypeFullName,
            inv.ControlTypeFullName,
            false,
            true,
            "global::System.String",
            [inv]);

        BindCommandCodeGenerator.GenerateCallerFilePathOverload(sb, group, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("Expression<global::System.Func<");
    }

    /// <summary>
    /// Verifies GenerateBindCommandMethod with CommandProperty plugin path.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindCommandMethod_CommandPropertyPlugin_EmitsCommandBinding()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasCommandProperty: true,
            hasCommandParameterProperty: false);
        var vmClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindCommandCodeGenerator.GenerateBindCommandMethod(sb, inv, vmClassInfo, "TESTSUFFIX", false);

        var result = sb.ToString();
        await Assert.That(result).Contains("__BindCommand_TESTSUFFIX");
        await Assert.That(result).Contains(".Command = cmd");
    }

    /// <summary>
    /// Verifies GenerateBindCommandMethod with EventEnabled plugin path.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindCommandMethod_EventEnabledPlugin_EmitsEnabledSync()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: "Click",
            hasCommandProperty: false,
            hasEnabledProperty: true);
        var vmClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindCommandCodeGenerator.GenerateBindCommandMethod(sb, inv, vmClassInfo, "TESTSUFFIX", false);

        var result = sb.ToString();
        await Assert.That(result).Contains("__BindCommand_TESTSUFFIX");
        await Assert.That(result).Contains(".Enabled =");
        await Assert.That(result).Contains("CanExecuteChanged");
        await Assert.That(result).Contains("HasHigherAffinityPlugin");
    }

    /// <summary>
    /// Verifies GenerateBindCommandMethod with no plugin match falls through to custom binder + throw.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindCommandMethod_NoPlugin_EmitsThrow()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: null,
            hasCommandProperty: false,
            hasEnabledProperty: false);
        var vmClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindCommandCodeGenerator.GenerateBindCommandMethod(sb, inv, vmClassInfo, "TESTSUFFIX", false);

        var result = sb.ToString();
        await Assert.That(result).Contains("No bindable event found");
        await Assert.That(result).Contains("HasHigherAffinityPlugin");
    }

    /// <summary>
    /// Verifies EmitCommandAffinityCheck with observable parameter emits Select wrapper.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitCommandAffinityCheck_ObservableParam_EmitsSelectWrapper()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: "global::System.String");

        BindCommandCodeGenerator.EmitCommandAffinityCheck(sb, inv, "view.SaveButton", 5, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("SelectObservable<global::System.String, object>");
        await Assert.That(result).Contains("HasHigherAffinityPlugin<global::TestApp.MyButton>(5, true)");
        await Assert.That(result).Contains("GetBinder<global::TestApp.MyButton>(true)");
    }

    /// <summary>
    /// Verifies EmitCommandAffinityCheck with expression parameter emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitCommandAffinityCheck_ExpressionParam_EmitsReturnObservable()
    {
        var paramPath = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Param", "global::System.String", "global::TestApp.MyViewModel")]);
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasExpressionParameter: true,
            parameterTypeFullName: "global::System.String",
            parameterPropertyPath: paramPath);

        BindCommandCodeGenerator.EmitCommandAffinityCheck(sb, inv, "view.SaveButton", 3, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("ReturnObservable<object>(viewModel.Param)");
        await Assert.That(result).Contains("HasHigherAffinityPlugin<global::TestApp.MyButton>(3, true)");
    }

    /// <summary>
    /// Verifies EmitCommandAffinityCheck with no parameter emits EmptyObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitCommandAffinityCheck_NoParam_EmitsEmptyObservable()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo();

        BindCommandCodeGenerator.EmitCommandAffinityCheck(sb, inv, "view.SaveButton", -1, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("EmptyObservable<object>.Instance");
        await Assert.That(result).Contains("HasHigherAffinityPlugin<global::TestApp.MyButton>(-1, false)");
        await Assert.That(result).Contains("GetBinder<global::TestApp.MyButton>(false)");
    }

    /// <summary>
    /// Verifies BuildParameterObservableExpression returns SelectObservable for observable parameter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildParameterObservableExpression_ObservableParam_ReturnsSelectObservable()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: "global::System.String");

        var result = BindCommandCodeGenerator.BuildParameterObservableExpression(inv);

        await Assert.That(result).Contains("SelectObservable<global::System.String, object>");
    }

    /// <summary>
    /// Verifies BuildParameterObservableExpression returns ReturnObservable for expression parameter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildParameterObservableExpression_ExpressionParam_ReturnsReturnObservable()
    {
        var paramPath = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Param", "global::System.String", "global::TestApp.MyViewModel")]);
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasExpressionParameter: true,
            parameterTypeFullName: "global::System.String",
            parameterPropertyPath: paramPath);

        var result = BindCommandCodeGenerator.BuildParameterObservableExpression(inv);

        await Assert.That(result).Contains("ReturnObservable<object>(viewModel.Param)");
    }

    /// <summary>
    /// Verifies BuildParameterObservableExpression returns EmptyObservable when no parameter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildParameterObservableExpression_NoParam_ReturnsEmptyObservable()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo();

        var result = BindCommandCodeGenerator.BuildParameterObservableExpression(inv);

        await Assert.That(result).Contains("EmptyObservable<object>.Instance");
    }

    /// <summary>
    /// Verifies Generate returns null when invocations are empty.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_EmptyInvocations_ReturnsNull()
    {
        var result = BindCommandCodeGenerator.Generate(
            [],
            [],
            new LanguageFeatures(true, true, true));

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies Generate returns null when invocations are default.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_DefaultInvocations_ReturnsNull()
    {
        var result = BindCommandCodeGenerator.Generate(
            default,
            [],
            new LanguageFeatures(true, true, true));

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies GenerateBindCommandMethod with observable parameter generates correct method params.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindCommandMethod_WithObservableParam_IncludesObservableParam()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: "global::System.String",
            resolvedEventName: "Click");
        var vmClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindCommandCodeGenerator.GenerateBindCommandMethod(sb, inv, vmClassInfo, "TESTSUFFIX", false);

        var result = sb.ToString();
        await Assert.That(result).Contains("IObservable<global::System.String> withParameter");
    }

    /// <summary>
    /// Verifies GenerateBindCommandMethod with an expression parameter reads the parameter value from the
    /// view model at call time. The <c>Expression&lt;Func&lt;...&gt;&gt;</c> parameter itself lives on the
    /// public overload (covered by <see cref="GenerateCallerArgExprOverload_WithExpressionParam_IncludesWithParameterExpr"/>);
    /// the worker consumes the compile-time-extracted property path via a <c>ReturnObservable</c>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindCommandMethod_WithExpressionParam_ReadsParameterFromViewModel()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasExpressionParameter: true,
            parameterTypeFullName: "global::System.String",
            parameterPropertyPath: new EquatableArray<PropertyPathSegment>(
                [ModelFactory.CreatePropertyPathSegment("Param", "global::System.String", "global::TestApp.MyViewModel")]),
            resolvedEventName: "Click");
        var vmClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindCommandCodeGenerator.GenerateBindCommandMethod(sb, inv, vmClassInfo, "TESTSUFFIX", false);

        var result = sb.ToString();
        await Assert.That(result).Contains("ReturnObservable<object>");
        await Assert.That(result).Contains("viewModel.Param");
    }

    // ───────────────────────────────────────────────────────────────────────────
    // Nullability annotations (C# 8+ targets emit nullable-aware syntax; C# 7.3 does not)
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies the DefaultEvent plugin emits a nullable handler <c>sender</c> parameter (<c>object?</c>)
    /// under nullable support, so the generated local function matches the <c>EventHandler</c> delegate.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultEventPlugin_EmitBinding_SupportsNullable_EmitsNullableSender()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(resolvedEventName: "Click");

        new DefaultEventBindingPlugin().EmitBinding(sb, inv, "view.SaveButton", true);

        var result = sb.ToString();
        await Assert.That(result).Contains("void __Handler(object? sender,");
        await Assert.That(result).DoesNotContain("void __Handler(object sender,");
    }

    /// <summary>
    /// Verifies the DefaultEvent plugin emits a plain <c>object sender</c> (no nullable annotation) when the
    /// target does not support nullable reference types (C# 7.3, where <c>object?</c> is a compile error).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultEventPlugin_EmitBinding_NoNullable_EmitsPlainSender()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(resolvedEventName: "Click");

        new DefaultEventBindingPlugin().EmitBinding(sb, inv, "view.SaveButton", false);

        var result = sb.ToString();
        await Assert.That(result).Contains("void __Handler(object sender,");
        await Assert.That(result).DoesNotContain("object? sender");
    }

    /// <summary>
    /// Verifies the EventEnabled plugin also emits a nullable handler <c>sender</c> under nullable support.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_EmitBinding_SupportsNullable_EmitsNullableSender()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: "Click",
            hasEnabledProperty: true);

        new EventEnabledBindingPlugin().EmitBinding(sb, inv, "view.SaveButton", true);

        await Assert.That(sb.ToString()).Contains("void __Handler(object? sender,");
    }

    /// <summary>
    /// Verifies a reference-typed observable command parameter declares <c>__latestParam</c> as nullable
    /// under nullable support, so its <c>= default</c> initializer (which is null for reference types) is
    /// null-clean.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_EmitBinding_SupportsNullable_ReferenceParam_EmitsNullableLatestParam()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: "global::System.String",
            parameterIsReferenceType: true,
            hasCommandProperty: true,
            hasCommandParameterProperty: true);

        new CommandPropertyBindingPlugin().EmitBinding(sb, inv, "view.SaveButton", true);

        await Assert.That(sb.ToString()).Contains("global::System.String? __latestParam = default;");
    }

    /// <summary>
    /// Verifies a value-typed observable command parameter keeps a non-nullable <c>__latestParam</c> even
    /// under nullable support — a value-type <c>default</c> is not null, and annotating it would change the
    /// declared type's semantics.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_EmitBinding_SupportsNullable_ValueParam_KeepsNonNullableLatestParam()
    {
        var sb = new StringBuilder();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: "global::System.Int32",
            parameterIsReferenceType: false,
            hasCommandProperty: true,
            hasCommandParameterProperty: true);

        new CommandPropertyBindingPlugin().EmitBinding(sb, inv, "view.SaveButton", true);

        var result = sb.ToString();
        await Assert.That(result).Contains("global::System.Int32 __latestParam = default;");
        await Assert.That(result).DoesNotContain("global::System.Int32? __latestParam");
    }

    /// <summary>
    /// Verifies the concrete BindCommand overload emits a nullable optional <c>toEvent</c> parameter
    /// (<c>string?</c>) under nullable support.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_SupportsNullable_EmitsNullableToEvent()
    {
        var sb = new StringBuilder();
        var group = BindCommandCodeGenerator.GroupByTypeSignature(
            [ModelFactory.CreateBindCommandInvocationInfo()])[0];

        BindCommandCodeGenerator.GenerateConcreteOverload(sb, group, true, true);

        await Assert.That(sb.ToString()).Contains("string? toEvent = null");
    }

    /// <summary>
    /// Verifies the concrete BindCommand overload emits a non-nullable <c>toEvent</c> parameter on C# 7.3.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_NoNullable_EmitsPlainToEvent()
    {
        var sb = new StringBuilder();
        var group = BindCommandCodeGenerator.GroupByTypeSignature(
            [ModelFactory.CreateBindCommandInvocationInfo()])[0];

        BindCommandCodeGenerator.GenerateConcreteOverload(sb, group, true, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("string toEvent = null");
        await Assert.That(result).DoesNotContain("string? toEvent");
    }

    /// <summary>
    /// Verifies the full BindCommand generation emits a <c>#nullable enable</c> directive when the consumer
    /// compilation supports nullable reference types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_SupportsNullable_EmitsNullableEnableDirective()
    {
        var result = BindCommandCodeGenerator.Generate(
            [ModelFactory.CreateBindCommandInvocationInfo()],
            [],
            new LanguageFeatures(true, true, true));

        await Assert.That(result).IsNotNull();
        await Assert.That(result!).Contains("#nullable enable");
    }

    /// <summary>
    /// Verifies the full BindCommand generation omits the <c>#nullable enable</c> directive on C# 7.3.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_NoNullable_OmitsNullableEnableDirective()
    {
        var result = BindCommandCodeGenerator.Generate(
            [ModelFactory.CreateBindCommandInvocationInfo()],
            [],
            new LanguageFeatures(true, false, true));

        await Assert.That(result).IsNotNull();
        await Assert.That(result!).DoesNotContain("#nullable enable");
    }
}

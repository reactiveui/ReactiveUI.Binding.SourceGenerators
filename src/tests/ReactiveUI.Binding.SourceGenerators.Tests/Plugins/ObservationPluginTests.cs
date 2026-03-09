// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>
/// Unit tests for all observation plugins, covering emit methods not exercised by snapshot tests.
/// </summary>
public class ObservationPluginTests
{
    // ========== WpfObservationPlugin ==========

    /// <summary>
    /// Verifies WPF plugin shallow observation emits EventObservable with DependencyPropertyDescriptor.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitShallowObservation_AfterChange_EmitsEventObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyControl", isBeforeChange: false, includeStartWith: true);

        var result = sb.ToString();
        await Assert.That(result).Contains("EventObservable");
        await Assert.That(result).Contains("DependencyPropertyDescriptor");
        await Assert.That(result).Contains("TextProperty");
    }

    /// <summary>
    /// Verifies WPF plugin shallow observation before-change emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitShallowObservation_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyControl", isBeforeChange: true, includeStartWith: true);

        await Assert.That(sb.ToString()).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies WPF plugin shallow observation variable emits EventObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitShallowObservationVariable_AfterChange_EmitsEventObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservationVariable(sb, "obj", segment, "global::TestApp.MyControl", isBeforeChange: false, "__obs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("var __obs0");
        await Assert.That(result).Contains("EventObservable");
    }

    /// <summary>
    /// Verifies WPF plugin shallow observation variable before-change emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitShallowObservationVariable_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservationVariable(sb, "obj", segment, "global::TestApp.MyControl", isBeforeChange: true, "__obs0");

        await Assert.That(sb.ToString()).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies WPF plugin deep chain root segment after-change emits EventObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitDeepChainRootSegment_AfterChange_EmitsEventObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, "global::TestApp.MyControl", isBeforeChange: false, "__obs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("EventObservable");
        await Assert.That(result).Contains("var __obs0");
    }

    /// <summary>
    /// Verifies WPF plugin deep chain root segment before-change emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitDeepChainRootSegment_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, "global::TestApp.MyControl", isBeforeChange: true, "__obs0");

        await Assert.That(sb.ToString()).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies WPF plugin deep chain inner segment after-change emits EventObservable with Switch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitDeepChainInnerSegment_AfterChange_EmitsEventObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("City", "string", "global::TestApp.Address");

        plugin.EmitDeepChainInnerSegment(sb, "__obs0", "__obs1", "__p1", segment, isBeforeChange: false);

        var result = sb.ToString();
        await Assert.That(result).Contains("EventObservable");
        await Assert.That(result).Contains("Switch");
    }

    /// <summary>
    /// Verifies WPF plugin deep chain inner segment before-change emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitDeepChainInnerSegment_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("City", "string", "global::TestApp.Address");

        plugin.EmitDeepChainInnerSegment(sb, "__obs0", "__obs1", "__p1", segment, isBeforeChange: true);

        await Assert.That(sb.ToString()).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies WPF plugin inline observation variable emits EventObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitInlineObservationVariable_EmitsEventObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitInlineObservationVariable(sb, "source", segment, "global::TestApp.MyControl", "sourceObs");

        var result = sb.ToString();
        await Assert.That(result).Contains("EventObservable");
        await Assert.That(result).Contains("var sourceObs");
    }

    /// <summary>
    /// Verifies WPF plugin EmitHelperClasses is a no-op.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitHelperClasses_IsNoOp()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();

        plugin.EmitHelperClasses(sb);

        await Assert.That(sb.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies WPF plugin properties.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_Properties_AreCorrect()
    {
        var plugin = new WpfObservationPlugin();

        await Assert.That(plugin.Affinity).IsEqualTo(4);
        await Assert.That(plugin.ObservationKind).IsEqualTo("WpfDP");
        await Assert.That(plugin.SupportsBeforeChanged).IsFalse();
        await Assert.That(plugin.RequiresHelperClasses).IsFalse();
    }

    /// <summary>
    /// Verifies WPF plugin matches WPF DependencyObject types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_IsAMatch_WpfDependencyObject()
    {
        var plugin = new WpfObservationPlugin();
        var classInfo = ModelFactory.CreateClassBindingInfo(inheritsWpfDependencyObject: true);

        await Assert.That(plugin.IsAMatch(classInfo)).IsTrue();
    }

    // ========== WinFormsObservationPlugin ==========

    /// <summary>
    /// Verifies WinForms plugin shallow observation variable after-change emits EventObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitShallowObservationVariable_AfterChange_EmitsEventObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservationVariable(sb, "obj", segment, "global::TestApp.MyTextBox", isBeforeChange: false, "__obs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("EventObservable");
        await Assert.That(result).Contains("TextChanged");
    }

    /// <summary>
    /// Verifies WinForms plugin shallow observation variable before-change emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitShallowObservationVariable_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservationVariable(sb, "obj", segment, "global::TestApp.MyTextBox", isBeforeChange: true, "__obs0");

        await Assert.That(sb.ToString()).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies WinForms plugin shallow observation before-change emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitShallowObservation_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyTextBox", isBeforeChange: true, includeStartWith: true);

        await Assert.That(sb.ToString()).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies WinForms plugin deep chain root segment after-change emits EventObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitDeepChainRootSegment_AfterChange_EmitsEventObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, "global::TestApp.MyTextBox", isBeforeChange: false, "__obs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("EventObservable");
        await Assert.That(result).Contains("TextChanged");
    }

    /// <summary>
    /// Verifies WinForms plugin deep chain root segment before-change emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitDeepChainRootSegment_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, "global::TestApp.MyTextBox", isBeforeChange: true, "__obs0");

        await Assert.That(sb.ToString()).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies WinForms plugin deep chain inner segment after-change emits EventObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitDeepChainInnerSegment_AfterChange_EmitsEventObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string", "global::TestApp.Inner");

        plugin.EmitDeepChainInnerSegment(sb, "__obs0", "__obs1", "__p1", segment, isBeforeChange: false);

        var result = sb.ToString();
        await Assert.That(result).Contains("EventObservable");
        await Assert.That(result).Contains("Switch");
    }

    /// <summary>
    /// Verifies WinForms plugin deep chain inner segment before-change emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitDeepChainInnerSegment_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string", "global::TestApp.Inner");

        plugin.EmitDeepChainInnerSegment(sb, "__obs0", "__obs1", "__p1", segment, isBeforeChange: true);

        await Assert.That(sb.ToString()).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies WinForms plugin inline observation variable emits EventObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitInlineObservationVariable_EmitsEventObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitInlineObservationVariable(sb, "source", segment, "global::TestApp.MyTextBox", "sourceObs");

        var result = sb.ToString();
        await Assert.That(result).Contains("EventObservable");
        await Assert.That(result).Contains("TextChanged");
    }

    /// <summary>
    /// Verifies WinForms plugin EmitHelperClasses is a no-op.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitHelperClasses_IsNoOp()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();

        plugin.EmitHelperClasses(sb);

        await Assert.That(sb.Length).IsEqualTo(0);
    }

    // ========== WinUIObservationPlugin ==========

    /// <summary>
    /// Verifies WinUI plugin shallow observation before-change emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitShallowObservation_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyControl", isBeforeChange: true, includeStartWith: true);

        await Assert.That(sb.ToString()).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies WinUI plugin shallow observation variable after-change emits WinUIDPObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitShallowObservationVariable_AfterChange_EmitsWinUIDPObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservationVariable(sb, "obj", segment, "global::TestApp.MyControl", isBeforeChange: false, "__obs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("__WinUIDPObservable");
        await Assert.That(result).Contains("var __obs0");
    }

    /// <summary>
    /// Verifies WinUI plugin shallow observation variable before-change emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitShallowObservationVariable_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservationVariable(sb, "obj", segment, "global::TestApp.MyControl", isBeforeChange: true, "__obs0");

        await Assert.That(sb.ToString()).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies WinUI plugin deep chain root segment after-change emits WinUIDPObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitDeepChainRootSegment_AfterChange_EmitsWinUIDPObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, "global::TestApp.MyControl", isBeforeChange: false, "__obs0");

        await Assert.That(sb.ToString()).Contains("__WinUIDPObservable");
    }

    /// <summary>
    /// Verifies WinUI plugin deep chain root segment before-change emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitDeepChainRootSegment_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, "global::TestApp.MyControl", isBeforeChange: true, "__obs0");

        await Assert.That(sb.ToString()).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies WinUI plugin deep chain inner segment after-change emits WinUIDPObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitDeepChainInnerSegment_AfterChange_EmitsWinUIDPObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string", "global::TestApp.Inner");

        plugin.EmitDeepChainInnerSegment(sb, "__obs0", "__obs1", "__p1", segment, isBeforeChange: false);

        var result = sb.ToString();
        await Assert.That(result).Contains("__WinUIDPObservable");
        await Assert.That(result).Contains("Switch");
    }

    /// <summary>
    /// Verifies WinUI plugin deep chain inner segment before-change emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitDeepChainInnerSegment_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string", "global::TestApp.Inner");

        plugin.EmitDeepChainInnerSegment(sb, "__obs0", "__obs1", "__p1", segment, isBeforeChange: true);

        await Assert.That(sb.ToString()).Contains("ReturnObservable");
    }

    /// <summary>
    /// Verifies WinUI plugin inline observation variable emits WinUIDPObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitInlineObservationVariable_EmitsWinUIDPObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitInlineObservationVariable(sb, "source", segment, "global::TestApp.MyControl", "sourceObs");

        var result = sb.ToString();
        await Assert.That(result).Contains("__WinUIDPObservable");
        await Assert.That(result).Contains("var sourceObs");
    }

    /// <summary>
    /// Verifies WinUI plugin emits helper classes with __WinUIDPObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitHelperClasses_EmitsWinUIDPObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();

        plugin.EmitHelperClasses(sb);

        var result = sb.ToString();
        await Assert.That(result).Contains("__WinUIDPObservable");
        await Assert.That(result).Contains("RegisterPropertyChangedCallback");
    }

    // ========== KVOObservationPlugin ==========

    /// <summary>
    /// Verifies KVO plugin shallow observation variable after-change emits KVOObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitShallowObservationVariable_AfterChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservationVariable(sb, "obj", segment, "global::TestApp.MyView", isBeforeChange: false, "__obs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("__KVOObservable");
        await Assert.That(result).Contains("\"text\"");
    }

    /// <summary>
    /// Verifies KVO plugin shallow observation variable before-change emits KVOObservable with beforeChange true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitShallowObservationVariable_BeforeChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservationVariable(sb, "obj", segment, "global::TestApp.MyView", isBeforeChange: true, "__obs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("__KVOObservable");
        await Assert.That(result).Contains("true)");
    }

    /// <summary>
    /// Verifies KVO plugin deep chain root segment emits KVOObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitDeepChainRootSegment_AfterChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, "global::TestApp.MyView", isBeforeChange: false, "__obs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("__KVOObservable");
        await Assert.That(result).Contains("\"text\"");
    }

    /// <summary>
    /// Verifies KVO plugin deep chain root segment before-change emits KVOObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitDeepChainRootSegment_BeforeChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, "global::TestApp.MyView", isBeforeChange: true, "__obs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("__KVOObservable");
        await Assert.That(result).Contains("true)");
    }

    /// <summary>
    /// Verifies KVO plugin deep chain inner segment emits KVOObservable with Switch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitDeepChainInnerSegment_AfterChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("City", "string", "global::TestApp.Address");

        plugin.EmitDeepChainInnerSegment(sb, "__obs0", "__obs1", "__p1", segment, isBeforeChange: false);

        var result = sb.ToString();
        await Assert.That(result).Contains("__KVOObservable");
        await Assert.That(result).Contains("Switch");
    }

    /// <summary>
    /// Verifies KVO plugin deep chain inner segment before-change emits KVOObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitDeepChainInnerSegment_BeforeChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("City", "string", "global::TestApp.Address");

        plugin.EmitDeepChainInnerSegment(sb, "__obs0", "__obs1", "__p1", segment, isBeforeChange: true);

        var result = sb.ToString();
        await Assert.That(result).Contains("__KVOObservable");
        await Assert.That(result).Contains("true)");
    }

    /// <summary>
    /// Verifies KVO plugin inline observation variable emits KVOObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitInlineObservationVariable_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitInlineObservationVariable(sb, "source", segment, "global::TestApp.MyView", "sourceObs");

        var result = sb.ToString();
        await Assert.That(result).Contains("__KVOObservable");
        await Assert.That(result).Contains("var sourceObs");
    }

    /// <summary>
    /// Verifies KVO key path for boolean property uses "is" prefix.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_BooleanProperty_UsesIsPrefix()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Enabled", "bool");

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyView", isBeforeChange: false, includeStartWith: true);

        await Assert.That(sb.ToString()).Contains("\"isEnabled\"");
    }

    /// <summary>
    /// Verifies KVO key path for empty property name returns empty string.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmptyPropertyName_ReturnsEmpty()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment(string.Empty, "string");

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyView", isBeforeChange: false, includeStartWith: true);

        await Assert.That(sb.ToString()).Contains("\"\"");
    }

    /// <summary>
    /// Verifies KVO plugin emits helper classes with __KVOObserver and __KVOObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitHelperClasses_EmitsKVOClasses()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();

        plugin.EmitHelperClasses(sb);

        var result = sb.ToString();
        await Assert.That(result).Contains("__KVOObserver");
        await Assert.That(result).Contains("__KVOObservable");
        await Assert.That(result).Contains("AddObserver");
    }

    /// <summary>
    /// Verifies KVO plugin shallow observation emits both before/after change variants correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitShallowObservation_BeforeChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyView", isBeforeChange: true, includeStartWith: true);

        var result = sb.ToString();
        await Assert.That(result).Contains("__KVOObservable");
        await Assert.That(result).Contains("true, true");
    }

    // ========== AndroidObservationPlugin ==========

    /// <summary>
    /// Verifies Android plugin shallow observation variable emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitShallowObservationVariable_EmitsReturnObservable()
    {
        var plugin = new AndroidObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservationVariable(sb, "obj", segment, "global::TestApp.MyAndroidView", isBeforeChange: false, "__obs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("ReturnObservable");
        await Assert.That(result).Contains("var __obs0");
    }

    /// <summary>
    /// Verifies Android plugin deep chain root segment emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitDeepChainRootSegment_EmitsReturnObservable()
    {
        var plugin = new AndroidObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, "global::TestApp.MyAndroidView", isBeforeChange: false, "__obs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("ReturnObservable");
        await Assert.That(result).Contains("var __obs0");
    }

    /// <summary>
    /// Verifies Android plugin deep chain inner segment emits ReturnObservable with Switch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitDeepChainInnerSegment_EmitsReturnObservable()
    {
        var plugin = new AndroidObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("City", "string", "global::TestApp.Address");

        plugin.EmitDeepChainInnerSegment(sb, "__obs0", "__obs1", "__p1", segment, isBeforeChange: false);

        var result = sb.ToString();
        await Assert.That(result).Contains("ReturnObservable");
        await Assert.That(result).Contains("Switch");
    }

    /// <summary>
    /// Verifies Android plugin inline observation variable emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitInlineObservationVariable_EmitsReturnObservable()
    {
        var plugin = new AndroidObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitInlineObservationVariable(sb, "source", segment, "global::TestApp.MyAndroidView", "sourceObs");

        var result = sb.ToString();
        await Assert.That(result).Contains("ReturnObservable");
        await Assert.That(result).Contains("var sourceObs");
    }

    /// <summary>
    /// Verifies Android plugin EmitHelperClasses is a no-op.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitHelperClasses_IsNoOp()
    {
        var plugin = new AndroidObservationPlugin();
        var sb = new StringBuilder();

        plugin.EmitHelperClasses(sb);

        await Assert.That(sb.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies Android plugin properties.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_Properties_AreCorrect()
    {
        var plugin = new AndroidObservationPlugin();

        await Assert.That(plugin.Affinity).IsEqualTo(5);
        await Assert.That(plugin.ObservationKind).IsEqualTo("Android");
        await Assert.That(plugin.SupportsBeforeChanged).IsFalse();
        await Assert.That(plugin.RequiresHelperClasses).IsFalse();
    }

    // ========== Shallow observation with includeStartWith=false ==========

    /// <summary>
    /// Verifies INPC plugin shallow observation with includeStartWith=false emits "false".
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task INPCPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new INPCObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Name", "string");

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyViewModel", isBeforeChange: false, includeStartWith: false);

        await Assert.That(sb.ToString()).Contains("false)");
    }

    /// <summary>
    /// Verifies ReactiveObject plugin shallow observation with includeStartWith=false emits "false".
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveObjectPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new ReactiveObjectObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Name", "string");

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyViewModel", isBeforeChange: false, includeStartWith: false);

        await Assert.That(sb.ToString()).Contains("false)");
    }

    /// <summary>
    /// Verifies WPF plugin shallow observation with includeStartWith=false emits "false".
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyControl", isBeforeChange: false, includeStartWith: false);

        await Assert.That(sb.ToString()).Contains("false)");
    }

    /// <summary>
    /// Verifies WinForms plugin shallow observation with includeStartWith=false emits "false".
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyTextBox", isBeforeChange: false, includeStartWith: false);

        await Assert.That(sb.ToString()).Contains("false)");
    }

    /// <summary>
    /// Verifies WinUI plugin shallow observation with includeStartWith=false emits "false".
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyControl", isBeforeChange: false, includeStartWith: false);

        await Assert.That(sb.ToString()).Contains("false)");
    }

    /// <summary>
    /// Verifies KVO plugin shallow observation with includeStartWith=false emits "false".
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyView", isBeforeChange: false, includeStartWith: false);

        await Assert.That(sb.ToString()).Contains("false, false)");
    }

    /// <summary>
    /// Verifies Android plugin shallow observation emits ReturnObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitShallowObservation_EmitsReturnObservable()
    {
        var plugin = new AndroidObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", "string");

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyAndroidView", isBeforeChange: false, includeStartWith: true);

        await Assert.That(sb.ToString()).Contains("ReturnObservable");
    }

    // ========== INPCObservationPlugin ==========

    /// <summary>
    /// Verifies INPC plugin EmitHelperClasses is a no-op.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task INPCPlugin_EmitHelperClasses_IsNoOp()
    {
        var plugin = new INPCObservationPlugin();
        var sb = new StringBuilder();

        plugin.EmitHelperClasses(sb);

        await Assert.That(sb.Length).IsEqualTo(0);
    }

    // ========== ReactiveObjectObservationPlugin ==========

    /// <summary>
    /// Verifies ReactiveObject plugin EmitHelperClasses is a no-op.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveObjectPlugin_EmitHelperClasses_IsNoOp()
    {
        var plugin = new ReactiveObjectObservationPlugin();
        var sb = new StringBuilder();

        plugin.EmitHelperClasses(sb);

        await Assert.That(sb.Length).IsEqualTo(0);
    }

    // ========== ObservationPluginRegistry ==========

    /// <summary>
    /// Verifies GetPlugin returns the correct plugin by index.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Registry_GetPlugin_ReturnsCorrectPlugin()
    {
        var plugin0 = ObservationPluginRegistry.GetPlugin(0);

        await Assert.That(plugin0.Affinity).IsEqualTo(15); // KVO has highest affinity
    }

    /// <summary>
    /// Verifies GetPluginByKind returns null for unknown kind.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Registry_GetPluginByKind_UnknownKind_ReturnsNull()
    {
        var plugin = ObservationPluginRegistry.GetPluginByKind("NonExistent");

        await Assert.That(plugin).IsNull();
    }

    /// <summary>
    /// Verifies Count returns the correct number of plugins.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Registry_Count_Returns7()
    {
        await Assert.That(ObservationPluginRegistry.Count).IsEqualTo(7);
    }
}

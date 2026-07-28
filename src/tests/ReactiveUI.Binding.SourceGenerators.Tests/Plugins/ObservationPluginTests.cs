// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Unit tests for all observation plugins, covering emit methods not exercised by snapshot tests.</summary>
public class ObservationPluginTests
{
    /// <summary>The <c>EventObservable</c> name these tests generate against.</summary>
    private const string EventObservableName = "EventObservable";

    /// <summary>The <c>false)</c> fragment these tests expect in the generated source.</summary>
    private const string FalseFragment = "false)";

    /// <summary>The fully qualified name of the <c>Address</c> type used by these tests.</summary>
    private const string AddressTypeName = "global::TestApp.Address";

    /// <summary>The fully qualified name of the <c>Inner</c> type used by these tests.</summary>
    private const string InnerTypeName = "global::TestApp.Inner";

    /// <summary>The fully qualified name of the <c>MyAndroidView</c> type used by these tests.</summary>
    private const string MyAndroidViewTypeName = "global::TestApp.MyAndroidView";

    /// <summary>The fully qualified name of the <c>MyControl</c> type used by these tests.</summary>
    private const string MyControlTypeName = "global::TestApp.MyControl";

    /// <summary>The fully qualified name of the <c>MyTextBox</c> type used by these tests.</summary>
    private const string MyTextBoxTypeName = "global::TestApp.MyTextBox";

    /// <summary>The fully qualified name of the <c>MyView</c> type used by these tests.</summary>
    private const string MyViewTypeName = "global::TestApp.MyView";

    /// <summary>The <c>__KVOObservable</c> local the generated code is expected to emit.</summary>
    private const string KVOObservableLocal = "__KVOObservable";

    /// <summary>The <c>__obs0</c> local the generated code is expected to emit.</summary>
    private const string Obs0Local = "__obs0";

    /// <summary>The <c>__obs1</c> local the generated code is expected to emit.</summary>
    private const string Obs1Local = "__obs1";

    /// <summary>The <c>ReturnObservable</c> name these tests generate against.</summary>
    private const string ReturnObservableName = "ReturnObservable";

    /// <summary>The <c>source</c> name these tests generate against.</summary>
    private const string SourceName = "source";

    /// <summary>The <c>sourceObs</c> name these tests generate against.</summary>
    private const string SourceObsName = "sourceObs";

    /// <summary>The <c>string</c> name these tests generate against.</summary>
    private const string StringName = "string";

    /// <summary>The <c>Switch</c> name these tests generate against.</summary>
    private const string SwitchName = "Switch";

    /// <summary>The <c>TextChanged</c> name these tests generate against.</summary>
    private const string TextChangedName = "TextChanged";

    /// <summary>The <c>true)</c> fragment these tests expect in the generated source.</summary>
    private const string TrueFragment = "true)";

    /// <summary>The <c>var __obs0</c> local the generated code is expected to emit.</summary>
    private const string Obs0Declaration = "var __obs0";

    /// <summary>The <c>var sourceObs</c> local the generated code is expected to emit.</summary>
    private const string SourceObsDeclaration = "var sourceObs";

    /// <summary>The <c>__WinUIDPObservable</c> local the generated code is expected to emit.</summary>
    private const string WinUIDPObservableLocal = "__WinUIDPObservable";

    // ========== WpfObservationPlugin ==========
    /// <summary>Verifies WPF plugin shallow observation emits EventObservable with DependencyPropertyDescriptor.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitShallowObservation_AfterChange_EmitsEventObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyControlTypeName, false, true);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).Contains("DependencyPropertyDescriptor");
        await Assert.That(result).Contains("TextProperty");
    }

    /// <summary>Verifies WPF plugin shallow observation before-change emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitShallowObservation_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyControlTypeName, true, true);

        await Assert.That(sb.ToString()).Contains(ReturnObservableName);
    }

    /// <summary>Verifies WPF plugin shallow observation variable emits EventObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitShallowObservationVariable_AfterChange_EmitsEventObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyControlTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(Obs0Declaration);
        await Assert.That(result).Contains(EventObservableName);
    }

    /// <summary>Verifies WPF plugin shallow observation variable before-change emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitShallowObservationVariable_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyControlTypeName, true, Obs0Local);

        await Assert.That(sb.ToString()).Contains(ReturnObservableName);
    }

    /// <summary>Verifies WPF plugin deep chain root segment after-change emits EventObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitDeepChainRootSegment_AfterChange_EmitsEventObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyControlTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).Contains(Obs0Declaration);
    }

    /// <summary>Verifies WPF plugin deep chain root segment before-change emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitDeepChainRootSegment_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyControlTypeName, true, Obs0Local);

        await Assert.That(sb.ToString()).Contains(ReturnObservableName);
    }

    /// <summary>Verifies WPF plugin deep chain inner segment after-change emits EventObservable with Switch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitDeepChainInnerSegment_AfterChange_EmitsEventObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("City", StringName, AddressTypeName);

        plugin.EmitDeepChainInnerSegment(sb, Obs0Local, Obs1Local, "__p1", segment, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).Contains(SwitchName);
    }

    /// <summary>Verifies WPF plugin deep chain inner segment before-change emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitDeepChainInnerSegment_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("City", StringName, AddressTypeName);

        plugin.EmitDeepChainInnerSegment(sb, Obs0Local, Obs1Local, "__p1", segment, true);

        await Assert.That(sb.ToString()).Contains(ReturnObservableName);
    }

    /// <summary>Verifies WPF plugin inline observation variable emits EventObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitInlineObservationVariable_EmitsEventObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitInlineObservationVariable(sb, SourceName, segment, MyControlTypeName, SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).Contains(SourceObsDeclaration);
    }

    /// <summary>Verifies WPF plugin EmitHelperClasses is a no-op.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitHelperClasses_IsNoOp()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();

        plugin.EmitHelperClasses(sb);

        await Assert.That(sb.Length).IsEqualTo(0);
    }

    /// <summary>Verifies WPF plugin properties.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_Properties_AreCorrect()
    {
        const int ExpectedPluginAffinity = 4;
        var plugin = new WpfObservationPlugin();

        await Assert.That(plugin.Affinity).IsEqualTo(ExpectedPluginAffinity);
        await Assert.That(plugin.ObservationKind).IsEqualTo("WpfDP");
        await Assert.That(plugin.SupportsBeforeChanged).IsFalse();
        await Assert.That(plugin.RequiresHelperClasses).IsFalse();
    }

    /// <summary>Verifies WPF plugin matches WPF DependencyObject types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_IsAMatch_WpfDependencyObject()
    {
        var plugin = new WpfObservationPlugin();
        var classInfo = ModelFactory.CreateClassBindingInfo(inheritsWpfDependencyObject: true);

        await Assert.That(plugin.IsAMatch(classInfo)).IsTrue();
    }

    // ========== WinFormsObservationPlugin ==========
    /// <summary>Verifies WinForms plugin shallow observation variable after-change emits EventObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitShallowObservationVariable_AfterChange_EmitsEventObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyTextBoxTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).Contains(TextChangedName);
    }

    /// <summary>Verifies WinForms plugin shallow observation variable before-change emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitShallowObservationVariable_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyTextBoxTypeName, true, Obs0Local);

        await Assert.That(sb.ToString()).Contains(ReturnObservableName);
    }

    /// <summary>Verifies WinForms plugin shallow observation before-change emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitShallowObservation_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyTextBoxTypeName, true, true);

        await Assert.That(sb.ToString()).Contains(ReturnObservableName);
    }

    /// <summary>Verifies WinForms plugin deep chain root segment after-change emits EventObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitDeepChainRootSegment_AfterChange_EmitsEventObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyTextBoxTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).Contains(TextChangedName);
    }

    /// <summary>Verifies WinForms plugin deep chain root segment before-change emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitDeepChainRootSegment_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyTextBoxTypeName, true, Obs0Local);

        await Assert.That(sb.ToString()).Contains(ReturnObservableName);
    }

    /// <summary>Verifies WinForms plugin deep chain inner segment after-change emits EventObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitDeepChainInnerSegment_AfterChange_EmitsEventObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName, InnerTypeName);

        plugin.EmitDeepChainInnerSegment(sb, Obs0Local, Obs1Local, "__p1", segment, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).Contains(SwitchName);
    }

    /// <summary>Verifies WinForms plugin deep chain inner segment before-change emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitDeepChainInnerSegment_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName, InnerTypeName);

        plugin.EmitDeepChainInnerSegment(sb, Obs0Local, Obs1Local, "__p1", segment, true);

        await Assert.That(sb.ToString()).Contains(ReturnObservableName);
    }

    /// <summary>Verifies WinForms plugin inline observation variable emits EventObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitInlineObservationVariable_EmitsEventObservable()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitInlineObservationVariable(sb, SourceName, segment, MyTextBoxTypeName, SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).Contains(TextChangedName);
    }

    /// <summary>Verifies WinForms plugin EmitHelperClasses is a no-op.</summary>
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
    /// <summary>Verifies WinUI plugin shallow observation before-change emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitShallowObservation_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyControlTypeName, true, true);

        await Assert.That(sb.ToString()).Contains(ReturnObservableName);
    }

    /// <summary>Verifies WinUI plugin shallow observation variable after-change emits WinUIDPObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitShallowObservationVariable_AfterChange_EmitsWinUIDPObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyControlTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(WinUIDPObservableLocal);
        await Assert.That(result).Contains(Obs0Declaration);
    }

    /// <summary>Verifies WinUI plugin shallow observation variable before-change emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitShallowObservationVariable_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyControlTypeName, true, Obs0Local);

        await Assert.That(sb.ToString()).Contains(ReturnObservableName);
    }

    /// <summary>Verifies WinUI plugin deep chain root segment after-change emits WinUIDPObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitDeepChainRootSegment_AfterChange_EmitsWinUIDPObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyControlTypeName, false, Obs0Local);

        await Assert.That(sb.ToString()).Contains(WinUIDPObservableLocal);
    }

    /// <summary>Verifies WinUI plugin deep chain root segment before-change emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitDeepChainRootSegment_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyControlTypeName, true, Obs0Local);

        await Assert.That(sb.ToString()).Contains(ReturnObservableName);
    }

    /// <summary>Verifies WinUI plugin deep chain inner segment after-change emits WinUIDPObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitDeepChainInnerSegment_AfterChange_EmitsWinUIDPObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName, InnerTypeName);

        plugin.EmitDeepChainInnerSegment(sb, Obs0Local, Obs1Local, "__p1", segment, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(WinUIDPObservableLocal);
        await Assert.That(result).Contains(SwitchName);
    }

    /// <summary>Verifies WinUI plugin deep chain inner segment before-change emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitDeepChainInnerSegment_BeforeChange_EmitsReturnObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName, InnerTypeName);

        plugin.EmitDeepChainInnerSegment(sb, Obs0Local, Obs1Local, "__p1", segment, true);

        await Assert.That(sb.ToString()).Contains(ReturnObservableName);
    }

    /// <summary>Verifies WinUI plugin inline observation variable emits WinUIDPObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitInlineObservationVariable_EmitsWinUIDPObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitInlineObservationVariable(sb, SourceName, segment, MyControlTypeName, SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(WinUIDPObservableLocal);
        await Assert.That(result).Contains(SourceObsDeclaration);
    }

    /// <summary>Verifies WinUI plugin emits helper classes with __WinUIDPObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitHelperClasses_EmitsWinUIDPObservable()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();

        plugin.EmitHelperClasses(sb);

        var result = sb.ToString();
        await Assert.That(result).Contains(WinUIDPObservableLocal);
        await Assert.That(result).Contains("RegisterPropertyChangedCallback");
    }

    // ========== KVOObservationPlugin ==========
    /// <summary>Verifies KVO plugin shallow observation variable after-change emits KVOObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitShallowObservationVariable_AfterChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyViewTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains("\"text\"");
    }

    /// <summary>Verifies KVO plugin shallow observation variable before-change emits KVOObservable with beforeChange true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitShallowObservationVariable_BeforeChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyViewTypeName, true, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains(TrueFragment);
    }

    /// <summary>Verifies KVO plugin deep chain root segment emits KVOObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitDeepChainRootSegment_AfterChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyViewTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains("\"text\"");
    }

    /// <summary>Verifies KVO plugin deep chain root segment before-change emits KVOObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitDeepChainRootSegment_BeforeChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyViewTypeName, true, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains(TrueFragment);
    }

    /// <summary>Verifies KVO plugin deep chain inner segment emits KVOObservable with Switch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitDeepChainInnerSegment_AfterChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("City", StringName, AddressTypeName);

        plugin.EmitDeepChainInnerSegment(sb, Obs0Local, Obs1Local, "__p1", segment, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains(SwitchName);
    }

    /// <summary>Verifies KVO plugin deep chain inner segment before-change emits KVOObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitDeepChainInnerSegment_BeforeChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("City", StringName, AddressTypeName);

        plugin.EmitDeepChainInnerSegment(sb, Obs0Local, Obs1Local, "__p1", segment, true);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains(TrueFragment);
    }

    /// <summary>Verifies KVO plugin inline observation variable emits KVOObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitInlineObservationVariable_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitInlineObservationVariable(sb, SourceName, segment, MyViewTypeName, SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains(SourceObsDeclaration);
    }

    /// <summary>Verifies KVO key path for boolean property uses "is" prefix.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_BooleanProperty_UsesIsPrefix()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Enabled", "bool");

        plugin.EmitShallowObservation(sb, "obj", segment, MyViewTypeName, false, true);

        await Assert.That(sb.ToString()).Contains("\"isEnabled\"");
    }

    /// <summary>Verifies KVO key path for boolean property already starting with "Is" does not double-prefix.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_BooleanPropertyAlreadyStartingWithIs_DoesNotDoublePrefix()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("IsEnabled", "bool");

        plugin.EmitShallowObservation(sb, "obj", segment, MyViewTypeName, false, true);

        await Assert.That(sb.ToString()).Contains("\"isEnabled\"");
    }

    /// <summary>Verifies KVO key path for empty property name returns empty string.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmptyPropertyName_ReturnsEmpty()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment(string.Empty, StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyViewTypeName, false, true);

        await Assert.That(sb.ToString()).Contains("\"\"");
    }

    /// <summary>Verifies KVO plugin emits helper classes with __KVOObserver and __KVOObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitHelperClasses_EmitsKVOClasses()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();

        plugin.EmitHelperClasses(sb);

        var result = sb.ToString();
        await Assert.That(result).Contains("__KVOObserver");
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains("AddObserver");
    }

    /// <summary>Verifies KVO plugin shallow observation emits both before/after change variants correctly.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitShallowObservation_BeforeChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyViewTypeName, true, true);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains("true, true");
    }

    // ========== AndroidObservationPlugin ==========
    /// <summary>Verifies Android plugin shallow observation variable emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitShallowObservationVariable_EmitsReturnObservable()
    {
        var plugin = new AndroidObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyAndroidViewTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(ReturnObservableName);
        await Assert.That(result).Contains(Obs0Declaration);
    }

    /// <summary>Verifies Android plugin deep chain root segment emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitDeepChainRootSegment_EmitsReturnObservable()
    {
        var plugin = new AndroidObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyAndroidViewTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(ReturnObservableName);
        await Assert.That(result).Contains(Obs0Declaration);
    }

    /// <summary>Verifies Android plugin deep chain inner segment emits ReturnObservable with Switch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitDeepChainInnerSegment_EmitsReturnObservable()
    {
        var plugin = new AndroidObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("City", StringName, AddressTypeName);

        plugin.EmitDeepChainInnerSegment(sb, Obs0Local, Obs1Local, "__p1", segment, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(ReturnObservableName);
        await Assert.That(result).Contains(SwitchName);
    }

    /// <summary>Verifies Android plugin inline observation variable emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitInlineObservationVariable_EmitsReturnObservable()
    {
        var plugin = new AndroidObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitInlineObservationVariable(sb, SourceName, segment, MyAndroidViewTypeName, SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(ReturnObservableName);
        await Assert.That(result).Contains(SourceObsDeclaration);
    }

    /// <summary>Verifies Android plugin EmitHelperClasses is a no-op.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitHelperClasses_IsNoOp()
    {
        var plugin = new AndroidObservationPlugin();
        var sb = new StringBuilder();

        plugin.EmitHelperClasses(sb);

        await Assert.That(sb.Length).IsEqualTo(0);
    }

    /// <summary>Verifies Android plugin properties.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_Properties_AreCorrect()
    {
        const int ExpectedPluginAffinity = 5;
        var plugin = new AndroidObservationPlugin();

        await Assert.That(plugin.Affinity).IsEqualTo(ExpectedPluginAffinity);
        await Assert.That(plugin.ObservationKind).IsEqualTo("Android");
        await Assert.That(plugin.SupportsBeforeChanged).IsFalse();
        await Assert.That(plugin.RequiresHelperClasses).IsFalse();
    }

    // ========== Shallow observation with includeStartWith=false ==========
    /// <summary>Verifies INPC plugin shallow observation with includeStartWith=false emits "false".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task INPCPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new INPCObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Name", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyViewModel", false, false);

        await Assert.That(sb.ToString()).Contains(FalseFragment);
    }

    /// <summary>Verifies ReactiveObject plugin shallow observation with includeStartWith=false emits "false".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveObjectPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new ReactiveObjectObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Name", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyViewModel", false, false);

        await Assert.That(sb.ToString()).Contains(FalseFragment);
    }

    /// <summary>Verifies WPF plugin shallow observation with includeStartWith=false emits "false".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyControlTypeName, false, false);

        await Assert.That(sb.ToString()).Contains(FalseFragment);
    }

    /// <summary>Verifies WinForms plugin shallow observation with includeStartWith=false emits "false".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new WinFormsObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyTextBoxTypeName, false, false);

        await Assert.That(sb.ToString()).Contains(FalseFragment);
    }

    /// <summary>Verifies WinUI plugin shallow observation with includeStartWith=false emits "false".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new WinUIObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyControlTypeName, false, false);

        await Assert.That(sb.ToString()).Contains(FalseFragment);
    }

    /// <summary>Verifies KVO plugin shallow observation with includeStartWith=false emits "false".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyViewTypeName, false, false);

        await Assert.That(sb.ToString()).Contains("false, false)");
    }

    /// <summary>Verifies Android plugin shallow observation emits ReturnObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitShallowObservation_EmitsReturnObservable()
    {
        var plugin = new AndroidObservationPlugin();
        var sb = new StringBuilder();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyAndroidViewTypeName, false, true);

        await Assert.That(sb.ToString()).Contains(ReturnObservableName);
    }

    // ========== INPCObservationPlugin ==========
    /// <summary>Verifies INPC plugin EmitHelperClasses is a no-op.</summary>
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
    /// <summary>Verifies ReactiveObject plugin EmitHelperClasses is a no-op.</summary>
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
    /// <summary>Verifies GetPlugin returns the correct plugin by index.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Registry_GetPlugin_ReturnsCorrectPlugin()
    {
        const int ExpectedPlugin0Affinity = 15;
        var plugin0 = ObservationPluginRegistry.GetPlugin(0);

        await Assert.That(plugin0.Affinity).IsEqualTo(ExpectedPlugin0Affinity); // KVO has highest affinity
    }

    /// <summary>Verifies GetPluginByKind returns null for unknown kind.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Registry_GetPluginByKind_UnknownKind_ReturnsNull()
    {
        var plugin = ObservationPluginRegistry.GetPluginByKind("NonExistent");

        await Assert.That(plugin).IsNull();
    }

    /// <summary>Verifies Count returns the correct number of plugins.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Registry_Count_Returns7()
    {
        const int ExpectedPluginCount = 7;
        await Assert.That(ObservationPluginRegistry.Count).IsEqualTo(ExpectedPluginCount);
    }
}

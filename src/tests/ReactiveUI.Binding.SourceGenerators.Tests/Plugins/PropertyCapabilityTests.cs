// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>
/// Covers which properties each mechanism reaches. A type advertises a mechanism; a property participates in it
/// or does not, and the mechanisms differ in what participation means - a companion field, a companion event, or
/// nothing at all beyond the type implementing an interface.
/// </summary>
public class PropertyCapabilityTests
{
    /// <summary>The property the capability questions are asked about.</summary>
    private const string PropertyName = "Caption";

    /// <summary>Every widget property is reachable through the Android mechanism, which declares nothing per property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_ReachesAnyPropertyOfAMatchedType()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(inheritsAndroidView: true);

        await Assert.That(new AndroidObservationPlugin().CanObserveProperty(classInfo, PropertyName)).IsTrue();
    }

    /// <summary>The notification interfaces name the property in the event, so every property is reachable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NotifyPlugin_ReachesAnyPropertyOfAMatchedType()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        await Assert.That(new INPCObservationPlugin().CanObserveProperty(classInfo, PropertyName)).IsTrue();
    }

    /// <summary>Key-value observing reaches any property of a matched type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KvoPlugin_ReachesAnyPropertyOfAMatchedType()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(inheritsNSObject: true);

        await Assert.That(new KVOObservationPlugin().CanObserveProperty(classInfo, PropertyName)).IsTrue();
    }

    /// <summary>A property the type does not declare is unknown, and an unknown property stays observable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DependencyPropertyPlugins_TreatAnUndeclaredPropertyAsObservable()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(inheritsWpfDependencyObject: true);

        await Assert.That(new WpfObservationPlugin().CanObserveProperty(classInfo, "NotDeclaredHere")).IsTrue();
        await Assert.That(new WinUIObservationPlugin().CanObserveProperty(classInfo, "NotDeclaredHere")).IsTrue();
    }

    /// <summary>A declared property with no companion change event is out of the component mechanism's reach.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComponentPlugin_DoesNotReachAPropertyWithoutAChangeEvent()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(
            inheritsWinFormsComponent: true,
            properties: new EquatableArray<ObservablePropertyInfo>(
                [ModelFactory.CreateObservablePropertyInfo(PropertyName)]));

        await Assert.That(new WinFormsObservationPlugin().CanObserveProperty(classInfo, PropertyName)).IsFalse();
    }

    /// <summary>A declared property with a companion change event is within the component mechanism's reach.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComponentPlugin_ReachesAPropertyWithAChangeEvent()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(
            inheritsWinFormsComponent: true,
            properties: new EquatableArray<ObservablePropertyInfo>(
                [ModelFactory.CreateObservablePropertyInfo(PropertyName, hasChangeEvent: true)]));

        await Assert.That(new WinFormsObservationPlugin().CanObserveProperty(classInfo, PropertyName)).IsTrue();
    }
}

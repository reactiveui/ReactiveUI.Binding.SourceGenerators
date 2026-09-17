// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// Observation plugin for types implementing <c>IReactiveObject</c>, which claims them ahead of the
/// plain INPC plugin. <c>IReactiveObject</c> implements both notification interfaces, so the emitted
/// observation is the same.
/// </summary>
internal sealed class ReactiveObjectObservationPlugin : NotifyPropertyObservationPlugin
{
    /// <inheritdoc/>
    public override int Affinity => BindingAffinity.ExactType;

    /// <inheritdoc/>
    public override string ObservationKind => "ReactiveObject";

    /// <inheritdoc/>
    public override bool IsAMatch(ClassBindingInfo classInfo) => classInfo.ImplementsIReactiveObject;
}

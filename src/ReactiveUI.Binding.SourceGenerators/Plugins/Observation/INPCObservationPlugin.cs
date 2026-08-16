// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// Observation plugin for types implementing <see cref="System.ComponentModel.INotifyPropertyChanged"/>.
/// Supports both after-change and before-change (if the type also implements INotifyPropertyChanging).
/// </summary>
internal sealed class INPCObservationPlugin : NotifyPropertyObservationPlugin
{
    /// <inheritdoc/>
    public override int Affinity => BindingAffinity.Explicit;

    /// <inheritdoc/>
    public override string ObservationKind => "INPC";

    /// <inheritdoc/>
    public override bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.ImplementsINPC && !classInfo.ImplementsIReactiveObject;
}

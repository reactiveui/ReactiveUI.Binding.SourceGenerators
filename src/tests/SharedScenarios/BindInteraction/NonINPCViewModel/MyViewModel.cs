// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding;

namespace SharedScenarios.BindInteraction.NonINPCViewModel
{
    /// <summary>
    /// ViewModel that does NOT implement INotifyPropertyChanged.
    /// The generator should use ReturnObservable fallback for property observation.
    /// </summary>
    public class MyViewModel
    {
        /// <summary>
        /// Gets the confirmation interaction.
        /// </summary>
        public Interaction<string, bool> Confirm { get; } = new Interaction<string, bool>();
    }
}

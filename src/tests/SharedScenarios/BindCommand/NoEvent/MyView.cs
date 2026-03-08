// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.BindCommand.NoEvent
{
    /// <summary>
    /// View containing a plain control with no events.
    /// </summary>
#pragma warning disable CS0067 // Event is never used
    public class MyView : IViewFor, INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public object? ViewModel { get; set; }

        /// <summary>
        /// Gets the plain control (no Click event).
        /// </summary>
        public PlainControl Label { get; } = new PlainControl();
    }
#pragma warning restore CS0067
}

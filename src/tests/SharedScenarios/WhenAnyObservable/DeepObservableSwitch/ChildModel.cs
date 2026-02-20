// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyObservable.DeepObservableSwitch
{
    /// <summary>
    /// Child model with an observable property.
    /// </summary>
    public class ChildModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for <see cref="MyCommand"/>.
        /// </summary>
        private IObservable<string>? _myCommand;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the command observable.
        /// </summary>
        public IObservable<string>? MyCommand
        {
            get => _myCommand;
            set
            {
                if (_myCommand != value)
                {
                    _myCommand = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MyCommand)));
                }
            }
        }
    }
}

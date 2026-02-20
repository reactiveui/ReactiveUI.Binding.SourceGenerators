// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyObservable.DeepObservableSwitch
{
    /// <summary>
    /// Parent ViewModel containing a child model with an observable property.
    /// </summary>
    public class ParentViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for <see cref="Child"/>.
        /// </summary>
        private ChildModel _child = new();

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the child model.
        /// </summary>
        public ChildModel Child
        {
            get => _child;
            set
            {
                if (_child != value)
                {
                    _child = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Child)));
                }
            }
        }
    }
}

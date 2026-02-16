// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Reactive.Concurrency;

using ReactiveUI.Binding;

namespace SharedScenarios.BindOneWay.SinglePropertyWithScheduler
{
    /// <summary>
    /// Source ViewModel with a string property.
    /// </summary>
    public class MyViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for <see cref="Name"/>.
        /// </summary>
        private string _name = string.Empty;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }
    }
}

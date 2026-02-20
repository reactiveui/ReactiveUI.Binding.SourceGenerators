// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanging.MultiPropertyWithDeepChains
{
    /// <summary>
    /// ViewModel with a shallow property and a deep property chain, supporting before-change notifications.
    /// </summary>
    public class MyViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <summary>
        /// The backing field for <see cref="Name"/>.
        /// </summary>
        private string _name = string.Empty;

        /// <summary>
        /// The backing field for <see cref="Address"/>.
        /// </summary>
        private AddressModel _address = new();

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging;

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
                    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Name)));
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        public AddressModel Address
        {
            get => _address;
            set
            {
                if (_address != value)
                {
                    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Address)));
                    _address = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Address)));
                }
            }
        }
    }
}

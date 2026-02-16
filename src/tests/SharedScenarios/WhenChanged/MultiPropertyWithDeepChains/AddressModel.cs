// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanged.MultiPropertyWithDeepChains
{
    /// <summary>
    /// Address model with a city property.
    /// </summary>
    public class AddressModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for <see cref="City"/>.
        /// </summary>
        private string _city = string.Empty;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        public string City
        {
            get => _city;
            set
            {
                if (_city != value)
                {
                    _city = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(City)));
                }
            }
        }
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanging.MultiPropertyWithDeepChains
{
    /// <summary>
    /// Address model with a city property and before-change notifications.
    /// </summary>
    public class AddressModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <summary>
        /// The backing field for <see cref="City"/>.
        /// </summary>
        private string _city = string.Empty;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging;

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
                    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(City)));
                    _city = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(City)));
                }
            }
        }
    }
}

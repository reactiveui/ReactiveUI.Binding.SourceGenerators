// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.BindOneWay.SinglePropertyIntToInt
{
    /// <summary>
    /// Target View with an integer property.
    /// </summary>
    public class MyView : INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for <see cref="DisplayCount"/>.
        /// </summary>
        private int _displayCount;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the display count.
        /// </summary>
        public int DisplayCount
        {
            get => _displayCount;
            set
            {
                if (_displayCount != value)
                {
                    _displayCount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayCount)));
                }
            }
        }
    }
}

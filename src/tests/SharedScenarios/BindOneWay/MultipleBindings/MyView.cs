// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.BindOneWay.MultipleBindings
{
    /// <summary>
    /// Target View with multiple properties.
    /// </summary>
    public class MyView : INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for <see cref="NameText"/>.
        /// </summary>
        private string _nameText = string.Empty;

        /// <summary>
        /// The backing field for <see cref="AgeDisplay"/>.
        /// </summary>
        private int _ageDisplay;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the name text.
        /// </summary>
        public string NameText
        {
            get => _nameText;
            set
            {
                if (_nameText != value)
                {
                    _nameText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameText)));
                }
            }
        }

        /// <summary>
        /// Gets or sets the age display.
        /// </summary>
        public int AgeDisplay
        {
            get => _ageDisplay;
            set
            {
                if (_ageDisplay != value)
                {
                    _ageDisplay = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AgeDisplay)));
                }
            }
        }
    }
}

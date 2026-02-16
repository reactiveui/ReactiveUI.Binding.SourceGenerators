// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.OneWayBind.MultipleBindings
{
    /// <summary>
    /// Target View with multiple properties.
    /// </summary>
    public class MyView : IViewFor, INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for <see cref="NameText"/>.
        /// </summary>
        private string _nameText = string.Empty;

        /// <summary>
        /// The backing field for <see cref="AgeText"/>.
        /// </summary>
        private int _ageText;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public object? ViewModel { get; set; }

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
        /// Gets or sets the age text.
        /// </summary>
        public int AgeText
        {
            get => _ageText;
            set
            {
                if (_ageText != value)
                {
                    _ageText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AgeText)));
                }
            }
        }
    }
}

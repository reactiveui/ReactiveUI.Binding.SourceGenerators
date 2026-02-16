// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanged.MultipleViewModels
{
    /// <summary>
    /// Second ViewModel with a count property.
    /// </summary>
    public class ViewModel2 : INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for <see cref="Count"/>.
        /// </summary>
        private int _count;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        public int Count
        {
            get => _count;
            set
            {
                if (_count != value)
                {
                    _count = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                }
            }
        }
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanged.FourLevelDeepChain
{
    /// <summary>
    /// Top level in the deep chain.
    /// </summary>
    public class Level1 : INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for <see cref="Model"/>.
        /// </summary>
        private Level2 _model = new();

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the level 2 model.
        /// </summary>
        public Level2 Model
        {
            get => _model;
            set
            {
                if (_model != value)
                {
                    _model = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Model)));
                }
            }
        }
    }
}

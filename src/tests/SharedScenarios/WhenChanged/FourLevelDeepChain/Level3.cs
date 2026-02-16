// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanged.FourLevelDeepChain
{
    /// <summary>
    /// Third level in the deep chain.
    /// </summary>
    public class Level3 : INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for <see cref="Model"/>.
        /// </summary>
        private Model _model = new();

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public Model Model
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

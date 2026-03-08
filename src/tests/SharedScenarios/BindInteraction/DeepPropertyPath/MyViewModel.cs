// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.BindInteraction.DeepPropertyPath
{
    /// <summary>
    /// ViewModel with a nested child that holds the interaction.
    /// </summary>
    public class MyViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for <see cref="Child"/>.
        /// </summary>
        private ChildViewModel? _child;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the child view model.
        /// </summary>
        public ChildViewModel? Child
        {
            get => _child;
            set
            {
                if (_child != value)
                {
                    _child = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Child)));
                }
            }
        }
    }
}

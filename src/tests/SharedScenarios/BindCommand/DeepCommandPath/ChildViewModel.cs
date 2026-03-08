// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Windows.Input;

namespace SharedScenarios.BindCommand.DeepCommandPath
{
    /// <summary>
    /// Child ViewModel that owns the command.
    /// </summary>
    public class ChildViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for <see cref="SaveCommand"/>.
        /// </summary>
        private ICommand? _saveCommand;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the save command.
        /// </summary>
        public ICommand? SaveCommand
        {
            get => _saveCommand;
            set
            {
                if (_saveCommand != value)
                {
                    _saveCommand = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SaveCommand)));
                }
            }
        }
    }
}

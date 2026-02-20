// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyObservable.MultipleInvocationsSameType
{
    /// <summary>
    /// ViewModel with two observable string properties for testing multiple same-type WhenAnyObservable invocations.
    /// </summary>
    public class MyViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for <see cref="Command1"/>.
        /// </summary>
        private IObservable<string>? _command1;

        /// <summary>
        /// The backing field for <see cref="Command2"/>.
        /// </summary>
        private IObservable<string>? _command2;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the first command observable.
        /// </summary>
        public IObservable<string>? Command1
        {
            get => _command1;
            set
            {
                if (_command1 != value)
                {
                    _command1 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Command1)));
                }
            }
        }

        /// <summary>
        /// Gets or sets the second command observable.
        /// </summary>
        public IObservable<string>? Command2
        {
            get => _command2;
            set
            {
                if (_command2 != value)
                {
                    _command2 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Command2)));
                }
            }
        }
    }
}

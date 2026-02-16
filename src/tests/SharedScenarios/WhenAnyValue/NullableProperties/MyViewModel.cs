// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyValue.NullableProperties
{
    /// <summary>
    /// ViewModel with nullable properties.
    /// </summary>
    public class MyViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for <see cref="NullableName"/>.
        /// </summary>
        private string? _nullableName;

        /// <summary>
        /// The backing field for <see cref="NullableAge"/>.
        /// </summary>
        private int? _nullableAge;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the nullable name.
        /// </summary>
        public string? NullableName
        {
            get => _nullableName;
            set
            {
                if (_nullableName != value)
                {
                    _nullableName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NullableName)));
                }
            }
        }

        /// <summary>
        /// Gets or sets the nullable age.
        /// </summary>
        public int? NullableAge
        {
            get => _nullableAge;
            set
            {
                if (_nullableAge != value)
                {
                    _nullableAge = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NullableAge)));
                }
            }
        }
    }
}

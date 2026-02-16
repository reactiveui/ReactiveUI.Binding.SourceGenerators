// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyValue.MultiPropertyFiveProperties
{
    /// <summary>
    /// ViewModel with five observable properties.
    /// </summary>
    public class MyViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for <see cref="Prop1"/>.
        /// </summary>
        private string _prop1 = string.Empty;

        /// <summary>
        /// The backing field for <see cref="Prop2"/>.
        /// </summary>
        private int _prop2;

        /// <summary>
        /// The backing field for <see cref="Prop3"/>.
        /// </summary>
        private double _prop3;

        /// <summary>
        /// The backing field for <see cref="Prop4"/>.
        /// </summary>
        private bool _prop4;

        /// <summary>
        /// The backing field for <see cref="Prop5"/>.
        /// </summary>
        private string _prop5 = string.Empty;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets property 1.
        /// </summary>
        public string Prop1
        {
            get => _prop1;
            set
            {
                if (_prop1 != value)
                {
                    _prop1 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop1)));
                }
            }
        }

        /// <summary>
        /// Gets or sets property 2.
        /// </summary>
        public int Prop2
        {
            get => _prop2;
            set
            {
                if (_prop2 != value)
                {
                    _prop2 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop2)));
                }
            }
        }

        /// <summary>
        /// Gets or sets property 3.
        /// </summary>
        public double Prop3
        {
            get => _prop3;
            set
            {
                if (_prop3 != value)
                {
                    _prop3 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop3)));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether property 4 is set.
        /// </summary>
        public bool Prop4
        {
            get => _prop4;
            set
            {
                if (_prop4 != value)
                {
                    _prop4 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop4)));
                }
            }
        }

        /// <summary>
        /// Gets or sets property 5.
        /// </summary>
        public string Prop5
        {
            get => _prop5;
            set
            {
                if (_prop5 != value)
                {
                    _prop5 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop5)));
                }
            }
        }
    }
}

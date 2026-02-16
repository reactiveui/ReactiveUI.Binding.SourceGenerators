// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// An address model for testing deep property chains.
/// Implements both INotifyPropertyChanged and INotifyPropertyChanging.
/// </summary>
public class Address : INotifyPropertyChanged, INotifyPropertyChanging
{
    private string _street = string.Empty;
    private string _city = string.Empty;
    private string _zipCode = string.Empty;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Gets or sets the street.
    /// </summary>
    public string Street
    {
        get => _street;
        set
        {
            if (_street != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Street)));
                _street = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Street)));
            }
        }
    }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string City
    {
        get => _city;
        set
        {
            if (_city != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(City)));
                _city = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(City)));
            }
        }
    }

    /// <summary>
    /// Gets or sets the zip code.
    /// </summary>
    public string ZipCode
    {
        get => _zipCode;
        set
        {
            if (_zipCode != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(ZipCode)));
                _zipCode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZipCode)));
            }
        }
    }
}

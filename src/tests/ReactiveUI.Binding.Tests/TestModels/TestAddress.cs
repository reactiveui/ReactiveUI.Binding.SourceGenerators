// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A nested test model for testing expression chains.
/// </summary>
public class TestAddress : INotifyPropertyChanged
{
    private string _city = string.Empty;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string City
    {
        get => _city;
        set
        {
            if (_city == value)
            {
                return;
            }

            _city = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(City)));
        }
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A simple view model with Name and Age properties for testing WhenChanged/WhenChanging.
/// Implements both INotifyPropertyChanged and INotifyPropertyChanging.
/// </summary>
public class TestViewModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    private string _name = string.Empty;
    private int _age;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Name)));
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
    }

    /// <summary>
    /// Gets or sets the age.
    /// </summary>
    public int Age
    {
        get => _age;
        set
        {
            if (_age != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Age)));
                _age = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Age)));
            }
        }
    }
}

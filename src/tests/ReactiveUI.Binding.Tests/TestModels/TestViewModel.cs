// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A test view model implementing INotifyPropertyChanged and INotifyPropertyChanging.
/// </summary>
public class TestViewModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    private string _name = string.Empty;
    private int _age;
    private TestAddress? _address;

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
            if (_name == value)
            {
                return;
            }

            OnPropertyChanging();
            _name = value;
            OnPropertyChanged();
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
            if (_age == value)
            {
                return;
            }

            OnPropertyChanging();
            _age = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the address.
    /// </summary>
    public TestAddress? Address
    {
        get => _address;
        set
        {
            if (ReferenceEquals(_address, value))
            {
                return;
            }

            OnPropertyChanging();
            _address = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Raises the PropertyChanging event.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    protected void OnPropertyChanging([CallerMemberName] string? propertyName = null) =>
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

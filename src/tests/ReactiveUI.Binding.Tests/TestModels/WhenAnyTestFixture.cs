// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A test fixture with 12 value properties for multi-property WhenAnyValue testing.
/// </summary>
public class WhenAnyTestFixture : INotifyPropertyChanged
{
    private string _value1 = string.Empty;
    private string _value2 = string.Empty;
    private string _value3 = string.Empty;
    private string _value4 = string.Empty;
    private string _value5 = string.Empty;
    private string _value6 = string.Empty;
    private string _value7 = string.Empty;
    private string _value8 = string.Empty;
    private string _value9 = string.Empty;
    private string _value10 = string.Empty;
    private string _value11 = string.Empty;
    private string _value12 = string.Empty;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets value 1.</summary>
    public string Value1
    {
        get => _value1;
        set
        {
            if (_value1 != value)
            {
                _value1 = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Gets or sets value 2.</summary>
    public string Value2
    {
        get => _value2;
        set
        {
            if (_value2 != value)
            {
                _value2 = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Gets or sets value 3.</summary>
    public string Value3
    {
        get => _value3;
        set
        {
            if (_value3 != value)
            {
                _value3 = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Gets or sets value 4.</summary>
    public string Value4
    {
        get => _value4;
        set
        {
            if (_value4 != value)
            {
                _value4 = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Gets or sets value 5.</summary>
    public string Value5
    {
        get => _value5;
        set
        {
            if (_value5 != value)
            {
                _value5 = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Gets or sets value 6.</summary>
    public string Value6
    {
        get => _value6;
        set
        {
            if (_value6 != value)
            {
                _value6 = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Gets or sets value 7.</summary>
    public string Value7
    {
        get => _value7;
        set
        {
            if (_value7 != value)
            {
                _value7 = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Gets or sets value 8.</summary>
    public string Value8
    {
        get => _value8;
        set
        {
            if (_value8 != value)
            {
                _value8 = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Gets or sets value 9.</summary>
    public string Value9
    {
        get => _value9;
        set
        {
            if (_value9 != value)
            {
                _value9 = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Gets or sets value 10.</summary>
    public string Value10
    {
        get => _value10;
        set
        {
            if (_value10 != value)
            {
                _value10 = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Gets or sets value 11.</summary>
    public string Value11
    {
        get => _value11;
        set
        {
            if (_value11 != value)
            {
                _value11 = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Gets or sets value 12.</summary>
    public string Value12
    {
        get => _value12;
        set
        {
            if (_value12 != value)
            {
                _value12 = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

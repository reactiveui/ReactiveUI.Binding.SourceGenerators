// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A fixture with 12 string properties for testing multi-property WhenAnyValue overloads.
/// Implements INotifyPropertyChanged.
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

    /// <summary>
    /// Gets or sets value1.
    /// </summary>
    public string Value1
    {
        get => _value1;
        set
        {
            if (_value1 != value)
            {
                _value1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value1)));
            }
        }
    }

    /// <summary>
    /// Gets or sets value2.
    /// </summary>
    public string Value2
    {
        get => _value2;
        set
        {
            if (_value2 != value)
            {
                _value2 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value2)));
            }
        }
    }

    /// <summary>
    /// Gets or sets value3.
    /// </summary>
    public string Value3
    {
        get => _value3;
        set
        {
            if (_value3 != value)
            {
                _value3 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value3)));
            }
        }
    }

    /// <summary>
    /// Gets or sets value4.
    /// </summary>
    public string Value4
    {
        get => _value4;
        set
        {
            if (_value4 != value)
            {
                _value4 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value4)));
            }
        }
    }

    /// <summary>
    /// Gets or sets value5.
    /// </summary>
    public string Value5
    {
        get => _value5;
        set
        {
            if (_value5 != value)
            {
                _value5 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value5)));
            }
        }
    }

    /// <summary>
    /// Gets or sets value6.
    /// </summary>
    public string Value6
    {
        get => _value6;
        set
        {
            if (_value6 != value)
            {
                _value6 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value6)));
            }
        }
    }

    /// <summary>
    /// Gets or sets value7.
    /// </summary>
    public string Value7
    {
        get => _value7;
        set
        {
            if (_value7 != value)
            {
                _value7 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value7)));
            }
        }
    }

    /// <summary>
    /// Gets or sets value8.
    /// </summary>
    public string Value8
    {
        get => _value8;
        set
        {
            if (_value8 != value)
            {
                _value8 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value8)));
            }
        }
    }

    /// <summary>
    /// Gets or sets value9.
    /// </summary>
    public string Value9
    {
        get => _value9;
        set
        {
            if (_value9 != value)
            {
                _value9 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value9)));
            }
        }
    }

    /// <summary>
    /// Gets or sets value10.
    /// </summary>
    public string Value10
    {
        get => _value10;
        set
        {
            if (_value10 != value)
            {
                _value10 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value10)));
            }
        }
    }

    /// <summary>
    /// Gets or sets value11.
    /// </summary>
    public string Value11
    {
        get => _value11;
        set
        {
            if (_value11 != value)
            {
                _value11 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value11)));
            }
        }
    }

    /// <summary>
    /// Gets or sets value12.
    /// </summary>
    public string Value12
    {
        get => _value12;
        set
        {
            if (_value12 != value)
            {
                _value12 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value12)));
            }
        }
    }
}

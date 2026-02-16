// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A view model with 16 properties of mixed types plus a nested Address for testing
/// multi-property overloads and deep property chains.
/// Implements both INotifyPropertyChanged and INotifyPropertyChanging.
/// </summary>
public class BigViewModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    private string _prop1 = string.Empty;
    private int _prop2;
    private double _prop3;
    private bool _prop4;
    private string _prop5 = string.Empty;
    private int _prop6;
    private double _prop7;
    private bool _prop8;
    private string _prop9 = string.Empty;
    private int _prop10;
    private double _prop11;
    private bool _prop12;
    private string _prop13 = string.Empty;
    private int _prop14;
    private double _prop15;
    private bool _prop16;
    private Address _address = new();

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Gets or sets prop1 (string).
    /// </summary>
    public string Prop1
    {
        get => _prop1;
        set
        {
            if (_prop1 != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop1)));
                _prop1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop1)));
            }
        }
    }

    /// <summary>
    /// Gets or sets prop2 (int).
    /// </summary>
    public int Prop2
    {
        get => _prop2;
        set
        {
            if (_prop2 != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop2)));
                _prop2 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop2)));
            }
        }
    }

    /// <summary>
    /// Gets or sets prop3 (double).
    /// </summary>
    public double Prop3
    {
        get => _prop3;
        set
        {
            if (Math.Abs(_prop3 - value) > double.Epsilon)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop3)));
                _prop3 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop3)));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether prop4 is true.
    /// </summary>
    public bool Prop4
    {
        get => _prop4;
        set
        {
            if (_prop4 != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop4)));
                _prop4 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop4)));
            }
        }
    }

    /// <summary>
    /// Gets or sets prop5 (string).
    /// </summary>
    public string Prop5
    {
        get => _prop5;
        set
        {
            if (_prop5 != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop5)));
                _prop5 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop5)));
            }
        }
    }

    /// <summary>
    /// Gets or sets prop6 (int).
    /// </summary>
    public int Prop6
    {
        get => _prop6;
        set
        {
            if (_prop6 != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop6)));
                _prop6 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop6)));
            }
        }
    }

    /// <summary>
    /// Gets or sets prop7 (double).
    /// </summary>
    public double Prop7
    {
        get => _prop7;
        set
        {
            if (Math.Abs(_prop7 - value) > double.Epsilon)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop7)));
                _prop7 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop7)));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether prop8 is true.
    /// </summary>
    public bool Prop8
    {
        get => _prop8;
        set
        {
            if (_prop8 != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop8)));
                _prop8 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop8)));
            }
        }
    }

    /// <summary>
    /// Gets or sets prop9 (string).
    /// </summary>
    public string Prop9
    {
        get => _prop9;
        set
        {
            if (_prop9 != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop9)));
                _prop9 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop9)));
            }
        }
    }

    /// <summary>
    /// Gets or sets prop10 (int).
    /// </summary>
    public int Prop10
    {
        get => _prop10;
        set
        {
            if (_prop10 != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop10)));
                _prop10 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop10)));
            }
        }
    }

    /// <summary>
    /// Gets or sets prop11 (double).
    /// </summary>
    public double Prop11
    {
        get => _prop11;
        set
        {
            if (Math.Abs(_prop11 - value) > double.Epsilon)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop11)));
                _prop11 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop11)));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether prop12 is true.
    /// </summary>
    public bool Prop12
    {
        get => _prop12;
        set
        {
            if (_prop12 != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop12)));
                _prop12 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop12)));
            }
        }
    }

    /// <summary>
    /// Gets or sets prop13 (string).
    /// </summary>
    public string Prop13
    {
        get => _prop13;
        set
        {
            if (_prop13 != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop13)));
                _prop13 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop13)));
            }
        }
    }

    /// <summary>
    /// Gets or sets prop14 (int).
    /// </summary>
    public int Prop14
    {
        get => _prop14;
        set
        {
            if (_prop14 != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop14)));
                _prop14 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop14)));
            }
        }
    }

    /// <summary>
    /// Gets or sets prop15 (double).
    /// </summary>
    public double Prop15
    {
        get => _prop15;
        set
        {
            if (Math.Abs(_prop15 - value) > double.Epsilon)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop15)));
                _prop15 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop15)));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether prop16 is true.
    /// </summary>
    public bool Prop16
    {
        get => _prop16;
        set
        {
            if (_prop16 != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Prop16)));
                _prop16 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prop16)));
            }
        }
    }

    /// <summary>
    /// Gets or sets the address for deep property chain testing.
    /// </summary>
    public Address Address
    {
        get => _address;
        set
        {
            if (_address != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Address)));
                _address = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Address)));
            }
        }
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A view with 16 properties matching BigViewModel types for testing bindings.
/// Implements INotifyPropertyChanged only (views don't need INotifyPropertyChanging).
/// </summary>
public class BigView : INotifyPropertyChanged
{
    private string _viewProp1 = string.Empty;
    private int _viewProp2;
    private double _viewProp3;
    private bool _viewProp4;
    private string _viewProp5 = string.Empty;
    private int _viewProp6;
    private double _viewProp7;
    private bool _viewProp8;
    private string _viewProp9 = string.Empty;
    private int _viewProp10;
    private double _viewProp11;
    private bool _viewProp12;
    private string _viewProp13 = string.Empty;
    private int _viewProp14;
    private double _viewProp15;
    private bool _viewProp16;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets view prop1 (string).
    /// </summary>
    public string ViewProp1
    {
        get => _viewProp1;
        set
        {
            if (_viewProp1 != value)
            {
                _viewProp1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp1)));
            }
        }
    }

    /// <summary>
    /// Gets or sets view prop2 (int).
    /// </summary>
    public int ViewProp2
    {
        get => _viewProp2;
        set
        {
            if (_viewProp2 != value)
            {
                _viewProp2 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp2)));
            }
        }
    }

    /// <summary>
    /// Gets or sets view prop3 (double).
    /// </summary>
    public double ViewProp3
    {
        get => _viewProp3;
        set
        {
            if (Math.Abs(_viewProp3 - value) > double.Epsilon)
            {
                _viewProp3 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp3)));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether view prop4 is true.
    /// </summary>
    public bool ViewProp4
    {
        get => _viewProp4;
        set
        {
            if (_viewProp4 != value)
            {
                _viewProp4 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp4)));
            }
        }
    }

    /// <summary>
    /// Gets or sets view prop5 (string).
    /// </summary>
    public string ViewProp5
    {
        get => _viewProp5;
        set
        {
            if (_viewProp5 != value)
            {
                _viewProp5 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp5)));
            }
        }
    }

    /// <summary>
    /// Gets or sets view prop6 (int).
    /// </summary>
    public int ViewProp6
    {
        get => _viewProp6;
        set
        {
            if (_viewProp6 != value)
            {
                _viewProp6 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp6)));
            }
        }
    }

    /// <summary>
    /// Gets or sets view prop7 (double).
    /// </summary>
    public double ViewProp7
    {
        get => _viewProp7;
        set
        {
            if (Math.Abs(_viewProp7 - value) > double.Epsilon)
            {
                _viewProp7 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp7)));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether view prop8 is true.
    /// </summary>
    public bool ViewProp8
    {
        get => _viewProp8;
        set
        {
            if (_viewProp8 != value)
            {
                _viewProp8 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp8)));
            }
        }
    }

    /// <summary>
    /// Gets or sets view prop9 (string).
    /// </summary>
    public string ViewProp9
    {
        get => _viewProp9;
        set
        {
            if (_viewProp9 != value)
            {
                _viewProp9 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp9)));
            }
        }
    }

    /// <summary>
    /// Gets or sets view prop10 (int).
    /// </summary>
    public int ViewProp10
    {
        get => _viewProp10;
        set
        {
            if (_viewProp10 != value)
            {
                _viewProp10 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp10)));
            }
        }
    }

    /// <summary>
    /// Gets or sets view prop11 (double).
    /// </summary>
    public double ViewProp11
    {
        get => _viewProp11;
        set
        {
            if (Math.Abs(_viewProp11 - value) > double.Epsilon)
            {
                _viewProp11 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp11)));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether view prop12 is true.
    /// </summary>
    public bool ViewProp12
    {
        get => _viewProp12;
        set
        {
            if (_viewProp12 != value)
            {
                _viewProp12 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp12)));
            }
        }
    }

    /// <summary>
    /// Gets or sets view prop13 (string).
    /// </summary>
    public string ViewProp13
    {
        get => _viewProp13;
        set
        {
            if (_viewProp13 != value)
            {
                _viewProp13 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp13)));
            }
        }
    }

    /// <summary>
    /// Gets or sets view prop14 (int).
    /// </summary>
    public int ViewProp14
    {
        get => _viewProp14;
        set
        {
            if (_viewProp14 != value)
            {
                _viewProp14 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp14)));
            }
        }
    }

    /// <summary>
    /// Gets or sets view prop15 (double).
    /// </summary>
    public double ViewProp15
    {
        get => _viewProp15;
        set
        {
            if (Math.Abs(_viewProp15 - value) > double.Epsilon)
            {
                _viewProp15 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp15)));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether view prop16 is true.
    /// </summary>
    public bool ViewProp16
    {
        get => _viewProp16;
        set
        {
            if (_viewProp16 != value)
            {
                _viewProp16 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewProp16)));
            }
        }
    }
}

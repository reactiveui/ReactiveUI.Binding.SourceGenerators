// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A view with 16 properties matching BigViewModel types for testing bindings.
/// Implements INotifyPropertyChanged only (views don't need INotifyPropertyChanging).
/// </summary>
public class BigView : INotifyPropertyChanged
{
    /// <summary>
    /// The backer for ViewProp1.
    /// </summary>
    private string _viewProp1 = string.Empty;

    /// <summary>
    /// The backer for ViewProp2.
    /// </summary>
    private int _viewProp2;

    /// <summary>
    /// The backer for ViewProp3.
    /// </summary>
    private double _viewProp3;

    /// <summary>
    /// The backer for ViewProp4.
    /// </summary>
    private bool _viewProp4;

    /// <summary>
    /// The backer for ViewProp5.
    /// </summary>
    private string _viewProp5 = string.Empty;

    /// <summary>
    /// The backer for ViewProp6.
    /// </summary>
    private int _viewProp6;

    /// <summary>
    /// The backer for ViewProp7.
    /// </summary>
    private double _viewProp7;

    /// <summary>
    /// The backer for ViewProp8.
    /// </summary>
    private bool _viewProp8;

    /// <summary>
    /// The backer for ViewProp9.
    /// </summary>
    private string _viewProp9 = string.Empty;

    /// <summary>
    /// The backer for ViewProp10.
    /// </summary>
    private int _viewProp10;

    /// <summary>
    /// The backer for ViewProp11.
    /// </summary>
    private double _viewProp11;

    /// <summary>
    /// The backer for ViewProp12.
    /// </summary>
    private bool _viewProp12;

    /// <summary>
    /// The backer for ViewProp13.
    /// </summary>
    private string _viewProp13 = string.Empty;

    /// <summary>
    /// The backer for ViewProp14.
    /// </summary>
    private int _viewProp14;

    /// <summary>
    /// The backer for ViewProp15.
    /// </summary>
    private double _viewProp15;

    /// <summary>
    /// The backer for ViewProp16.
    /// </summary>
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
            if (_viewProp1 == value)
            {
                return;
            }

            _viewProp1 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp1)));
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
            if (_viewProp2 == value)
            {
                return;
            }

            _viewProp2 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp2)));
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
            if (Math.Abs(_viewProp3 - value) <= double.Epsilon)
            {
                return;
            }

            _viewProp3 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp3)));
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
            if (_viewProp4 == value)
            {
                return;
            }

            _viewProp4 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp4)));
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
            if (_viewProp5 == value)
            {
                return;
            }

            _viewProp5 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp5)));
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
            if (_viewProp6 == value)
            {
                return;
            }

            _viewProp6 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp6)));
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
            if (Math.Abs(_viewProp7 - value) <= double.Epsilon)
            {
                return;
            }

            _viewProp7 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp7)));
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
            if (_viewProp8 == value)
            {
                return;
            }

            _viewProp8 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp8)));
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
            if (_viewProp9 == value)
            {
                return;
            }

            _viewProp9 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp9)));
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
            if (_viewProp10 == value)
            {
                return;
            }

            _viewProp10 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp10)));
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
            if (Math.Abs(_viewProp11 - value) <= double.Epsilon)
            {
                return;
            }

            _viewProp11 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp11)));
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
            if (_viewProp12 == value)
            {
                return;
            }

            _viewProp12 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp12)));
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
            if (_viewProp13 == value)
            {
                return;
            }

            _viewProp13 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp13)));
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
            if (_viewProp14 == value)
            {
                return;
            }

            _viewProp14 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp14)));
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
            if (Math.Abs(_viewProp15 - value) <= double.Epsilon)
            {
                return;
            }

            _viewProp15 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp15)));
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
            if (_viewProp16 == value)
            {
                return;
            }

            _viewProp16 = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp16)));
        }
    }
}

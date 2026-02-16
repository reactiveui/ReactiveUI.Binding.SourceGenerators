// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.AotValidation;

/// <summary>
/// A view model used for AOT validation of source-generated property observation and binding.
/// </summary>
public class AotViewModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    private string _name = string.Empty;
    private int _age;
    private AotChildViewModel _child = new();

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

    /// <summary>
    /// Gets or sets the child view model.
    /// </summary>
    public AotChildViewModel Child
    {
        get => _child;
        set
        {
            if (_child != value)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Child)));
                _child = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Child)));
            }
        }
    }
}

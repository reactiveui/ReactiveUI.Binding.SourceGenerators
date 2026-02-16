// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

using ReactiveUI.Binding;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A simple view with DisplayName and DisplayAge properties for testing bindings.
/// Implements INotifyPropertyChanged and IViewFor for view-first binding compat aliases.
/// </summary>
public class TestView : INotifyPropertyChanged, IViewFor
{
    private string _displayName = string.Empty;
    private int _displayAge;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public object? ViewModel { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName
    {
        get => _displayName;
        set
        {
            if (_displayName != value)
            {
                _displayName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
            }
        }
    }

    /// <summary>
    /// Gets or sets the display age.
    /// </summary>
    public int DisplayAge
    {
        get => _displayAge;
        set
        {
            if (_displayAge != value)
            {
                _displayAge = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayAge)));
            }
        }
    }
}

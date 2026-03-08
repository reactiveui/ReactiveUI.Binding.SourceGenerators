// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>
/// A view used for binding benchmarks.
/// Implements <see cref="IViewFor{T}"/> to support ReactiveUI's expression-tree-based binding APIs.
/// </summary>
public class BenchmarkView : IViewFor<BenchmarkViewModel>, INotifyPropertyChanged
{
    /// <summary>
    /// The backing field for the <see cref="DisplayName"/> property.
    /// </summary>
    private string _displayName = string.Empty;

    /// <summary>
    /// The backing field for the <see cref="DisplayAge"/> property.
    /// </summary>
    private int _displayAge;

    /// <summary>
    /// The backing field for the <see cref="ViewModel"/> property.
    /// </summary>
    private BenchmarkViewModel? _viewModel;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

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

    /// <inheritdoc/>
    public BenchmarkViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            if (_viewModel != value)
            {
                _viewModel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewModel)));
            }
        }
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (BenchmarkViewModel?)value;
    }
}

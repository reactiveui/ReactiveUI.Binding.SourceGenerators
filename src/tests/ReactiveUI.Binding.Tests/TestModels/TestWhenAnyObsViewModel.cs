// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A view model for testing WhenAnyObservable with observable properties.
/// </summary>
public class TestWhenAnyObsViewModel : INotifyPropertyChanged
{
    private IObservable<int>? _command1;
    private IObservable<int>? _command2;
    private IObservable<int>? _command3;
    private IObservable<int>? _changes;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the first command observable.
    /// </summary>
    public IObservable<int>? Command1
    {
        get => _command1;
        set
        {
            if (ReferenceEquals(_command1, value))
            {
                return;
            }

            _command1 = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the second command observable.
    /// </summary>
    public IObservable<int>? Command2
    {
        get => _command2;
        set
        {
            if (ReferenceEquals(_command2, value))
            {
                return;
            }

            _command2 = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the third command observable.
    /// </summary>
    public IObservable<int>? Command3
    {
        get => _command3;
        set
        {
            if (ReferenceEquals(_command3, value))
            {
                return;
            }

            _command3 = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the changes observable.
    /// </summary>
    public IObservable<int>? Changes
    {
        get => _changes;
        set
        {
            if (ReferenceEquals(_changes, value))
            {
                return;
            }

            _changes = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

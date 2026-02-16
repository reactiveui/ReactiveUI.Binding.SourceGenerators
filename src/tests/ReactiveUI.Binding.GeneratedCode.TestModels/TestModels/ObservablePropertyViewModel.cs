// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A view model with properties of type IObservable for testing WhenAnyObservable.
/// Implements INotifyPropertyChanged for property change observation.
/// </summary>
public class ObservablePropertyViewModel : INotifyPropertyChanged
{
    private IObservable<string>? _myCommand;
    private IObservable<string>? _otherCommand;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the command observable.
    /// </summary>
    public IObservable<string>? MyCommand
    {
        get => _myCommand;
        set
        {
            if (_myCommand != value)
            {
                _myCommand = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MyCommand)));
            }
        }
    }

    /// <summary>
    /// Gets or sets another command observable.
    /// </summary>
    public IObservable<string>? OtherCommand
    {
        get => _otherCommand;
        set
        {
            if (_otherCommand != value)
            {
                _otherCommand = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OtherCommand)));
            }
        }
    }
}

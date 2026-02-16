// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A host fixture with a Child property for testing deep property chain observation.
/// Implements both INotifyPropertyChanged and INotifyPropertyChanging.
/// </summary>
public class HostTestFixture : INotifyPropertyChanged, INotifyPropertyChanging
{
    private TestViewModel? _child;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Gets or sets the child view model for deep chain testing.
    /// </summary>
    public TestViewModel? Child
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

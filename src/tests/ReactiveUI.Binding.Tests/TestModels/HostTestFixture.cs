// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A host test fixture that contains nested observable and non-observable child objects.
/// Used for testing deep property chains.
/// </summary>
public class HostTestFixture : INotifyPropertyChanged, INotifyPropertyChanging
{
    private TestFixture? _child;
    private int _someOtherParam;
    private NonObservableTestFixture? _pocoChild;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Gets or sets the child test fixture.
    /// </summary>
    public TestFixture? Child
    {
        get => _child;
        set
        {
            if (ReferenceEquals(_child, value))
            {
                return;
            }

            OnPropertyChanging();
            _child = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets some other parameter.
    /// </summary>
    public int SomeOtherParam
    {
        get => _someOtherParam;
        set
        {
            if (_someOtherParam == value)
            {
                return;
            }

            OnPropertyChanging();
            _someOtherParam = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the non-observable child.
    /// </summary>
    public NonObservableTestFixture? PocoChild
    {
        get => _pocoChild;
        set
        {
            if (ReferenceEquals(_pocoChild, value))
            {
                return;
            }

            OnPropertyChanging();
            _pocoChild = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Raises the PropertyChanging event.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    protected void OnPropertyChanging([CallerMemberName] string? propertyName = null) =>
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

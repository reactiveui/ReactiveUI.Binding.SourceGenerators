// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A plain INPC object that does not extend ReactiveObject.
/// Used to verify that the runtime fallback works with any INPC implementation.
/// </summary>
public class NonReactiveINPCObject : INotifyPropertyChanged
{
    private string _inpcProperty = string.Empty;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the INPC property.
    /// </summary>
    public string InpcProperty
    {
        get => _inpcProperty;
        set
        {
            if (_inpcProperty == value)
            {
                return;
            }

            _inpcProperty = value;
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

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A minimal observable object exposing a single value. Its own type is what keys the notification
/// factory lookup, so tests that need a specific notifier resolved for them observe this rather than a
/// fixture other tests already share.
/// </summary>
public class ObservedValueFixture : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the observed value.</summary>
    public string Value
    {
        get => field;
        set
        {
            if (string.Equals(field, value, StringComparison.Ordinal))
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value)));
        }
    }

    = string.Empty;

    /// <summary>Raises a change notification for <see cref="Value"/> without altering it.</summary>
    public void RaiseValueChanged() => PropertyChanged?.Invoke(this, new(nameof(Value)));
}

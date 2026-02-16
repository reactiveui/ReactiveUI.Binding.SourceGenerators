// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// Third-level object in a 4-level deep chain: ObjChain1 → ObjChain2 → ObjChain3 → HostTestFixture.
/// </summary>
public class ObjChain3 : INotifyPropertyChanged
{
    private HostTestFixture? _host;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the host test fixture at the end of the chain.
    /// </summary>
    public HostTestFixture? Host
    {
        get => _host;
        set
        {
            if (ReferenceEquals(_host, value))
            {
                return;
            }

            _host = value;
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

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// Top-level object in a 4-level deep chain: ObjChain1 → ObjChain2 → ObjChain3 → HostTestFixture.
/// </summary>
public class ObjChain1 : INotifyPropertyChanged
{
    private ObjChain2? _chain2;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the next link in the chain.
    /// </summary>
    public ObjChain2? Chain2
    {
        get => _chain2;
        set
        {
            if (ReferenceEquals(_chain2, value))
            {
                return;
            }

            _chain2 = value;
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

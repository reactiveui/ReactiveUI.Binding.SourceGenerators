// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A test fixture implementing INotifyPropertyChanged and INotifyPropertyChanging
/// with various property types for comprehensive testing.
/// </summary>
public class TestFixture : INotifyPropertyChanged, INotifyPropertyChanging
{
    private string _isNotNullString = "Foo";
    private string _isOnlyOneWord = "Baz";
    private string _pocoProperty = string.Empty;
    private int? _nullableInt;
    private string _usesExprRaiseSet = string.Empty;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Gets or sets a non-null string property.
    /// </summary>
    public string IsNotNullString
    {
        get => _isNotNullString;
        set
        {
            if (_isNotNullString == value)
            {
                return;
            }

            OnPropertyChanging();
            _isNotNullString = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a single-word string property.
    /// </summary>
    public string IsOnlyOneWord
    {
        get => _isOnlyOneWord;
        set
        {
            if (_isOnlyOneWord == value)
            {
                return;
            }

            OnPropertyChanging();
            _isOnlyOneWord = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a property without change notification behavior.
    /// </summary>
    public string PocoProperty
    {
        get => _pocoProperty;
        set
        {
            if (_pocoProperty == value)
            {
                return;
            }

            OnPropertyChanging();
            _pocoProperty = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a nullable integer property.
    /// </summary>
    public int? NullableInt
    {
        get => _nullableInt;
        set
        {
            if (_nullableInt == value)
            {
                return;
            }

            OnPropertyChanging();
            _nullableInt = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a property that uses expression-based raise and set.
    /// </summary>
    public string UsesExprRaiseSet
    {
        get => _usesExprRaiseSet;
        set
        {
            if (_usesExprRaiseSet == value)
            {
                return;
            }

            OnPropertyChanging();
            _usesExprRaiseSet = value;
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

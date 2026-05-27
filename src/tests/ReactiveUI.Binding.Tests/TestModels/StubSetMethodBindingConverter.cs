// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A stub implementation of <see cref="ISetMethodBindingConverter"/> for testing.
/// </summary>
public class StubSetMethodBindingConverter : ISetMethodBindingConverter
{
    /// <summary>
    /// The default affinity score used when none is supplied.
    /// </summary>
    private const int DefaultAffinity = 10;

    /// <summary>
    /// The affinity score to return.
    /// </summary>
    private readonly int _affinity;

    /// <summary>
    /// Initializes a new instance of the <see cref="StubSetMethodBindingConverter"/> class
    /// using the default affinity score.
    /// </summary>
    public StubSetMethodBindingConverter()
        : this(DefaultAffinity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StubSetMethodBindingConverter"/> class.
    /// </summary>
    /// <param name="affinity">The affinity score to return.</param>
    public StubSetMethodBindingConverter(int affinity) => _affinity = affinity;

    /// <inheritdoc/>
    public int GetAffinityForObjects(Type? fromType, Type? toType) => _affinity;

    /// <inheritdoc/>
    public object? PerformSet(object? toTarget, object? newValue, object?[]? arguments) => newValue;
}

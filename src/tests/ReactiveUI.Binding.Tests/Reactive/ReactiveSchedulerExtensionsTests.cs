// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reactive.Concurrency;
using ReactiveUI.Binding.Reactive;

namespace ReactiveUI.Binding.Tests.Reactive;

/// <summary>
///     Tests for the System.Reactive flavour of <see cref="ReactiveSchedulerExtensions"/>, which takes
///     an <see cref="IScheduler"/>. All extension methods are CallerInfo stubs that throw
///     InvalidOperationException unless a source generator provides the implementation.
/// </summary>
public class ReactiveSchedulerExtensionsTests
{
    /// <summary>Verifies that BindOneWay throws InvalidOperationException (no generated binding).</summary>
    [Test]
    public void BindOneWay_ThrowsInvalidOperationException()
    {
        var source = new TestModel();
        var target = new TestModel();

        _ = Assert.Throws<InvalidOperationException>(() =>
            source.BindOneWay(
                target,
                s => s.Name,
                t => t.Name,
                (IScheduler?)null));
    }

    /// <summary>Verifies that BindOneWay with conversion throws InvalidOperationException (no generated binding).</summary>
    [Test]
    public void BindOneWay_WithConversion_ThrowsInvalidOperationException()
    {
        var source = new TestModel();
        var target = new TestModel();

        _ = Assert.Throws<InvalidOperationException>(() =>
            source.BindOneWay(
                target,
                s => s.Name,
                t => t.Name,
                static v => v,
                (IScheduler?)null));
    }

    /// <summary>Verifies that BindTwoWay throws InvalidOperationException (no generated binding).</summary>
    [Test]
    public void BindTwoWay_ThrowsInvalidOperationException()
    {
        var source = new TestModel();
        var target = new TestModel();

        _ = Assert.Throws<InvalidOperationException>(() =>
            source.BindTwoWay(
                target,
                s => s.Name,
                t => t.Name,
                (IScheduler?)null));
    }

    /// <summary>Verifies that BindTwoWay with conversion throws InvalidOperationException (no generated binding).</summary>
    [Test]
    public void BindTwoWay_WithConversion_ThrowsInvalidOperationException()
    {
        var source = new TestModel();
        var target = new TestModel();

        _ = Assert.Throws<InvalidOperationException>(() =>
            source.BindTwoWay(
                target,
                s => s.Name,
                t => t.Name,
                static v => v,
                static v => v,
                (IScheduler?)null));
    }

    /// <summary>A simple test model implementing <see cref="INotifyPropertyChanged"/>.</summary>
    private sealed class TestModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets or sets the name.</summary>
        public string? Name
        {
            get => field;
            set
            {
                if (string.Equals(field, value, StringComparison.Ordinal))
                {
                    return;
                }

                field = value;
                PropertyChanged?.Invoke(this, new(nameof(Name)));
            }
        }
    }
}

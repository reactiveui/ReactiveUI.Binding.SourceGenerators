// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reactive.Concurrency;

namespace ReactiveUI.Binding.Tests.Reactive;

/// <summary>
///     Tests for the <see cref="ReactiveSchedulerExtensions"/> class.
///     All extension methods are CallerInfo stubs that throw InvalidOperationException
///     unless a source generator provides the implementation.
/// </summary>
public class ReactiveSchedulerExtensionsTests
{
    /// <summary>
    ///     Verifies that BindOneWay throws InvalidOperationException (no generated binding).
    /// </summary>
    [Test]
    public void BindOneWay_ThrowsInvalidOperationException()
    {
        var source = new TestModel();
        var target = new TestModel();

        Assert.Throws<InvalidOperationException>(() =>
            source.BindOneWay(
                target,
                s => s.Name,
                t => t.Name,
                (IScheduler?)null));
    }

    /// <summary>
    ///     Verifies that BindOneWay with conversion throws InvalidOperationException (no generated binding).
    /// </summary>
    [Test]
    public void BindOneWay_WithConversion_ThrowsInvalidOperationException()
    {
        var source = new TestModel();
        var target = new TestModel();

        Assert.Throws<InvalidOperationException>(() =>
            source.BindOneWay(
                target,
                s => s.Name,
                t => t.Name,
                v => v,
                (IScheduler?)null));
    }

    /// <summary>
    ///     Verifies that BindTwoWay throws InvalidOperationException (no generated binding).
    /// </summary>
    [Test]
    public void BindTwoWay_ThrowsInvalidOperationException()
    {
        var source = new TestModel();
        var target = new TestModel();

        Assert.Throws<InvalidOperationException>(() =>
            source.BindTwoWay(
                target,
                s => s.Name,
                t => t.Name,
                (IScheduler?)null));
    }

    /// <summary>
    ///     Verifies that BindTwoWay with conversion throws InvalidOperationException (no generated binding).
    /// </summary>
    [Test]
    public void BindTwoWay_WithConversion_ThrowsInvalidOperationException()
    {
        var source = new TestModel();
        var target = new TestModel();

        Assert.Throws<InvalidOperationException>(() =>
            source.BindTwoWay(
                target,
                s => s.Name,
                t => t.Name,
                v => v,
                v => v,
                (IScheduler?)null));
    }

    /// <summary>
    /// A simple test model implementing <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    private sealed class TestModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [SuppressMessage(
            "Major Code Smell",
            "S3459:Unassigned members should be removed",
            Justification = "Assigned indirectly by the two-way binding under test via the property selector; no literal assignment is visible to the analyzer.")]
        public string? Name { get; set; }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new(propertyName));
    }
}

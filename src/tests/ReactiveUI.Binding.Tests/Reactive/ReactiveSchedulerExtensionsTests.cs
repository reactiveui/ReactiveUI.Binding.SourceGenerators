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

    private class TestModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string? Name { get; set; }

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

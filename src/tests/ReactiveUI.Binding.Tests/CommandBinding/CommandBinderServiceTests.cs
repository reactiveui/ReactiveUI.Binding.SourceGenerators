// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.CommandBinding;

namespace ReactiveUI.Binding.Tests.CommandBinding;

/// <summary>
/// Tests for the <see cref="CommandBinderService"/> class.
/// </summary>
public class CommandBinderServiceTests
{
    /// <summary>
    /// Verifies that GetBinder returns null when only core services are registered
    /// (no runtime command binders — known patterns handled by source generator).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBinder_CoreServicesOnly_ReturnsNull()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        builder.WithCoreServices();
        builder.BuildApp();

        var binder = CommandBinderService.GetBinder<ClickableButton>(false);

        await Assert.That(binder).IsNull();
    }

    /// <summary>
    /// Verifies that GetBinder returns null when no matching binder supports the type.
    /// Uses a fresh resolver to avoid state from other tests.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBinder_NoMatchingBinders_ReturnsNull()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        builder.WithCoreServices();
        builder.BuildApp();

        // PlainObject has no default events and no Command/CommandParameter properties
        var binder = CommandBinderService.GetBinder<PlainObject>(false);

        await Assert.That(binder).IsNull();
    }

    /// <summary>
    /// Verifies that GetBinder prefers higher affinity when custom binder is registered.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBinder_CustomBinder_PrefersHigherAffinity()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        builder.WithCoreServices();

        var customBinder = new HighAffinityBinder();
        builder.WithCommandBinder(customBinder);
        builder.BuildApp();

        var binder = CommandBinderService.GetBinder<ClickableButton>(false);

        await Assert.That(binder).IsNotNull();
        await Assert.That(ReferenceEquals(binder, customBinder)).IsTrue();
    }

    /// <summary>
    /// A mock control with a Click event for testing event-based command binder selection.
    /// </summary>
    [SuppressMessage("Minor Code Smell", "S2094:Classes should not be empty", Justification = "Used for testing")]
    [SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Used only as a generic type argument for command-binder metadata dispatch; never constructed by design.")]
    private sealed class ClickableButton
    {
#pragma warning disable CS0067
        /// <summary>
        /// Occurs when the button is clicked.
        /// </summary>
        [SuppressMessage("Major Code Smell", "S3264:Events should be invoked", Justification = "Test model")]
        public event EventHandler? Click;
#pragma warning restore CS0067
    }

    /// <summary>
    /// A plain object with no events or command properties for testing null binder scenarios.
    /// </summary>
    [SuppressMessage("Minor Code Smell", "S2094:Classes should not be empty", Justification = "Used for testing")]
    [SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Used only as a generic type argument for the null-binder dispatch path; never constructed by design.")]
    private sealed class PlainObject;

    /// <summary>
    /// A custom command binder with high affinity (10) for testing binder priority selection.
    /// </summary>
    private sealed class HighAffinityBinder : ICreatesCommandBinding
    {
        /// <summary>
        /// The high affinity score reported by this binder.
        /// </summary>
        private const int HighAffinity = 10;

        /// <inheritdoc/>
        [SuppressMessage(
            "Major Code Smell",
            "S4018:Generic methods should provide type parameter for type inference",
            Justification = "Type parameter is dictated by the implemented interface and is not inferable from the arguments.")]
        public int GetAffinityForObject<T>(bool hasEventTarget) => HighAffinity;

        /// <inheritdoc/>
        [RequiresUnreferencedCode("Test stub")]
        public IDisposable? BindCommandToObject<T>(ICommand? command, T? target, IObservable<object?> commandParameter)
            where T : class => null;

        /// <inheritdoc/>
        [RequiresUnreferencedCode("Test stub")]
        [SuppressMessage(
            "Major Code Smell",
            "S4018:Generic methods should provide type parameter for type inference",
            Justification = "Type parameter is dictated by the implemented interface and is not inferable from the arguments.")]
        public IDisposable? BindCommandToObject<T, TEventArgs>(
            ICommand? command,
            T? target,
            IObservable<object?> commandParameter,
            string eventName)
            where T : class => null;

        /// <inheritdoc/>
        public IDisposable? BindCommandToObject<T, TEventArgs>(
            ICommand? command,
            T? target,
            IObservable<object?> commandParameter,
            Action<EventHandler<TEventArgs>> addHandler,
            Action<EventHandler<TEventArgs>> removeHandler)
            where T : class
            where TEventArgs : EventArgs => null;
    }
}

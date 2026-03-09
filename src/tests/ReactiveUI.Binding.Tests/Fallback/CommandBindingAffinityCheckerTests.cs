// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;

using ReactiveUI.Binding.Fallback;

using Splat;

namespace ReactiveUI.Binding.Tests.Fallback;

/// <summary>
/// Tests for the <see cref="CommandBindingAffinityChecker"/> class.
/// </summary>
public class CommandBindingAffinityCheckerTests
{
    /// <summary>
    /// Verifies that when no plugins are registered, the method returns false.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_NoPluginsRegistered_ReturnsFalse()
    {
        AppLocator.UnregisterAll<ICreatesCommandBinding>();
        try
        {
            var result = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(10, false);

            await Assert.That(result).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>
    /// Verifies that when a registered plugin has lower affinity than the generated affinity,
    /// the method returns false.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_PluginWithLowerAffinity_ReturnsFalse()
    {
        AppLocator.UnregisterAll<ICreatesCommandBinding>();
        try
        {
            AppLocator.Register<ICreatesCommandBinding>(() => new StubCommandBinding(5));

            var result = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(10, false);

            await Assert.That(result).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>
    /// Verifies that when a registered plugin has equal affinity to the generated affinity,
    /// the method returns false (only strictly higher affinity wins).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_PluginWithEqualAffinity_ReturnsFalse()
    {
        AppLocator.UnregisterAll<ICreatesCommandBinding>();
        try
        {
            AppLocator.Register<ICreatesCommandBinding>(() => new StubCommandBinding(10));

            var result = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(10, false);

            await Assert.That(result).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>
    /// Verifies that when a registered plugin has higher affinity than the generated affinity,
    /// the method returns true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_PluginWithHigherAffinity_ReturnsTrue()
    {
        AppLocator.UnregisterAll<ICreatesCommandBinding>();
        try
        {
            AppLocator.Register<ICreatesCommandBinding>(() => new StubCommandBinding(20));

            var result = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(10, false);

            await Assert.That(result).IsTrue();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>
    /// Verifies that the hasEventTarget parameter is correctly passed through to the plugin.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_HasEventTargetTrue_PassesThroughToPlugin()
    {
        AppLocator.UnregisterAll<ICreatesCommandBinding>();
        try
        {
            var plugin = new StubCommandBinding(hasEventAffinity: 20, noEventAffinity: 0);
            AppLocator.Register<ICreatesCommandBinding>(() => plugin);

            var resultWithEvent = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(10, true);
            var resultWithoutEvent = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(10, false);

            await Assert.That(resultWithEvent).IsTrue();
            await Assert.That(resultWithoutEvent).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>
    /// Verifies that when multiple plugins are registered and only one has higher affinity,
    /// the method returns true (short-circuits on first match).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_MultiplePlugins_OnlyOneHigher_ReturnsTrue()
    {
        AppLocator.UnregisterAll<ICreatesCommandBinding>();
        try
        {
            AppLocator.Register<ICreatesCommandBinding>(() => new StubCommandBinding(5));
            AppLocator.Register<ICreatesCommandBinding>(() => new StubCommandBinding(20));

            var result = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(10, false);

            await Assert.That(result).IsTrue();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>
    /// Verifies that when multiple plugins are registered and none has higher affinity,
    /// the method returns false.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_MultiplePlugins_NoneHigher_ReturnsFalse()
    {
        AppLocator.UnregisterAll<ICreatesCommandBinding>();
        try
        {
            AppLocator.Register<ICreatesCommandBinding>(() => new StubCommandBinding(3));
            AppLocator.Register<ICreatesCommandBinding>(() => new StubCommandBinding(7));

            var result = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(10, false);

            await Assert.That(result).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>
    /// Restores default plugins by re-initializing the binding infrastructure.
    /// </summary>
    private static void RestoreDefaultPlugins() =>
        RuntimeObservationFallbackTests.EnsureInitialized();

    /// <summary>
    /// A stub control type used as a generic type argument in tests.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used as type parameter for HasHigherAffinityPlugin<T>.")]
    private sealed class StubControl
    {
    }

    /// <summary>
    /// A stub implementation of <see cref="ICreatesCommandBinding"/> for testing.
    /// </summary>
    private sealed class StubCommandBinding : ICreatesCommandBinding
    {
        /// <summary>
        /// The affinity to return when hasEventTarget is true.
        /// </summary>
        private readonly int _hasEventAffinity;

        /// <summary>
        /// The affinity to return when hasEventTarget is false.
        /// </summary>
        private readonly int _noEventAffinity;

        /// <summary>
        /// Initializes a new instance of the <see cref="StubCommandBinding"/> class
        /// with the same affinity for both event and non-event targets.
        /// </summary>
        /// <param name="affinity">The affinity to return for all calls.</param>
        public StubCommandBinding(int affinity)
        {
            _hasEventAffinity = affinity;
            _noEventAffinity = affinity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StubCommandBinding"/> class
        /// with different affinities for event and non-event targets.
        /// </summary>
        /// <param name="hasEventAffinity">The affinity to return when hasEventTarget is true.</param>
        /// <param name="noEventAffinity">The affinity to return when hasEventTarget is false.</param>
        public StubCommandBinding(int hasEventAffinity, int noEventAffinity)
        {
            _hasEventAffinity = hasEventAffinity;
            _noEventAffinity = noEventAffinity;
        }

        /// <inheritdoc/>
        public int GetAffinityForObject<T>(bool hasEventTarget) =>
            hasEventTarget ? _hasEventAffinity : _noEventAffinity;

        /// <inheritdoc/>
        public IDisposable? BindCommandToObject<T>(
            ICommand? command,
            T? target,
            IObservable<object?> commandParameter)
            where T : class =>
            throw new NotSupportedException("Not needed for affinity tests.");

        /// <inheritdoc/>
        public IDisposable? BindCommandToObject<T, TEventArgs>(
            ICommand? command,
            T? target,
            IObservable<object?> commandParameter,
            string eventName)
            where T : class =>
            throw new NotSupportedException("Not needed for affinity tests.");

        /// <inheritdoc/>
        public IDisposable? BindCommandToObject<T, TEventArgs>(
            ICommand? command,
            T? target,
            IObservable<object?> commandParameter,
            Action<EventHandler<TEventArgs>> addHandler,
            Action<EventHandler<TEventArgs>> removeHandler)
            where T : class
            where TEventArgs : EventArgs =>
            throw new NotSupportedException("Not needed for affinity tests.");
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Fallback;
using Splat;

namespace ReactiveUI.Binding.Tests.Fallback;

/// <summary>Tests for the <see cref="ObservationAffinityChecker"/> class.</summary>
public class ObservationAffinityCheckerTests
{
    /// <summary>The affinity the source generator's own selection is assumed to have.</summary>
    private const int GeneratedAffinity = 10;

    /// <summary>A plugin affinity above <see cref="GeneratedAffinity"/>, so the plugin wins.</summary>
    private const int HigherPluginAffinity = 20;

    /// <summary>A plugin affinity below <see cref="GeneratedAffinity"/>, so the generated binding wins.</summary>
    private const int LowerPluginAffinity = 5;

    /// <summary>A second, still-losing plugin affinity used when two plugins are registered.</summary>
    private const int MinorPluginAffinity = 3;

    /// <summary>A second plugin affinity used to check which of two plugins is selected.</summary>
    private const int AlternatePluginAffinity = 7;

    /// <summary>Verifies that passing a null type throws <see cref="ArgumentNullException"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_NullType_ThrowsArgumentNullException()
    {
        var action = static () => ObservationAffinityChecker.HasHigherAffinityPlugin(null!, GeneratedAffinity, false);
        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that when no plugins are registered, the method returns false.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_NoPluginsRegistered_ReturnsFalse()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            var result = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), GeneratedAffinity, false);

            await Assert.That(result).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Verifies that when all registered plugins have lower affinity than the generated affinity, the method returns false.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_PluginWithLowerAffinity_ReturnsFalse()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(LowerPluginAffinity));

            var result = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), GeneratedAffinity, false);

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
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(GeneratedAffinity));

            var result = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), GeneratedAffinity, false);

            await Assert.That(result).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Verifies that when a registered plugin has higher affinity than the generated affinity, the method returns true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_PluginWithHigherAffinity_ReturnsTrue()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(HigherPluginAffinity));

            var result = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), GeneratedAffinity, false);

            await Assert.That(result).IsTrue();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Verifies that the beforeChanged parameter is correctly passed through to the plugin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_BeforeChangedTrue_PassesThroughToPlugin()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            var plugin = new StubObservableForProperty(HigherPluginAffinity, 0);
            AppLocator.Register<ICreatesObservableForProperty>(() => plugin);

            var resultBeforeChanged = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), GeneratedAffinity, true);
            var resultAfterChanged = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), GeneratedAffinity, false);

            await Assert.That(resultBeforeChanged).IsTrue();
            await Assert.That(resultAfterChanged).IsFalse();
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
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(LowerPluginAffinity));
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(HigherPluginAffinity));

            var result = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), GeneratedAffinity, false);

            await Assert.That(result).IsTrue();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Verifies that when multiple plugins are registered and none has higher affinity, the method returns false.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_MultiplePlugins_NoneHigher_ReturnsFalse()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(MinorPluginAffinity));
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(AlternatePluginAffinity));

            var result = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), GeneratedAffinity, false);

            await Assert.That(result).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Restores default plugins by re-initializing the binding infrastructure.</summary>
    private static void RestoreDefaultPlugins() =>
        RuntimeObservationFallbackTests.EnsureInitialized();

    /// <summary>A stub implementation of <see cref="ICreatesObservableForProperty"/> for testing.</summary>
    private sealed class StubObservableForProperty : ICreatesObservableForProperty
    {
        /// <summary>The affinity to return when beforeChanged is true.</summary>
        private readonly int _beforeChangedAffinity;

        /// <summary>The affinity to return when beforeChanged is false.</summary>
        private readonly int _afterChangedAffinity;

        /// <summary>Initializes a new instance of the <see cref="StubObservableForProperty"/> class with the same affinity for both before and after change.</summary>
        /// <param name="affinity">The affinity to return for all calls.</param>
        public StubObservableForProperty(int affinity)
        {
            _beforeChangedAffinity = affinity;
            _afterChangedAffinity = affinity;
        }

        /// <summary>Initializes a new instance of the <see cref="StubObservableForProperty"/> class with different affinities for before and after change.</summary>
        /// <param name="beforeChangedAffinity">The affinity to return when beforeChanged is true.</param>
        /// <param name="afterChangedAffinity">The affinity to return when beforeChanged is false.</param>
        public StubObservableForProperty(int beforeChangedAffinity, int afterChangedAffinity)
        {
            _beforeChangedAffinity = beforeChangedAffinity;
            _afterChangedAffinity = afterChangedAffinity;
        }

        /// <inheritdoc/>
        public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged) =>
            beforeChanged ? _beforeChangedAffinity : _afterChangedAffinity;

        /// <inheritdoc/>
        public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
            object sender,
            System.Linq.Expressions.Expression expression,
            string propertyName,
            bool beforeChanged,
            bool suppressWarnings) =>
            throw new NotSupportedException("Not needed for affinity tests.");
    }
}

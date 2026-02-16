// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

using ReactiveUI.Binding.Helpers;

namespace ReactiveUI.Binding;

/// <summary>
/// Thread-safe registry for set-method binding converters using a lock-free snapshot pattern.
/// </summary>
/// <remarks>
/// <para>
/// This registry uses a copy-on-write snapshot pattern optimized for read-heavy workloads:
/// </para>
/// <list type="bullet">
/// <item><description>
/// <strong>Reads:</strong> Lock-free via a volatile read of the snapshot reference.
/// Multiple readers can access the registry concurrently without contention.
/// </description></item>
/// <item><description>
/// <strong>Writes:</strong> Serialized under a lock. Writes clone the converter list,
/// mutate the clone, and publish a new snapshot atomically.
/// </description></item>
/// <item><description>
/// <strong>Selection:</strong> Set-method converters are stored in a simple list (no type-pair grouping).
/// When looking up a converter, each converter's runtime affinity is checked via
/// <see cref="ISetMethodBindingConverter.GetAffinityForObjects(Type?, Type?)"/>.
/// The converter with the highest affinity (&gt; 0) is selected.
/// </description></item>
/// </list>
/// <para>
/// Set-method converters are used for specialized binding operations that require custom
/// set behavior, such as populating collections or handling platform-specific controls.
/// </para>
/// </remarks>
public sealed class SetMethodBindingConverterRegistry
{
#if NET9_0_OR_GREATER
    private readonly Lock _gate = new();
#else
    private readonly object _gate = new();
#endif

    private Snapshot? _snapshot;

    /// <summary>
    /// Registers a set-method binding converter.
    /// </summary>
    /// <param name="converter">The converter to register. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="converter"/> is null.</exception>
    public void Register(ISetMethodBindingConverter converter)
    {
        ArgumentExceptionHelper.ThrowIfNull(converter);

        lock (_gate)
        {
            var snap = _snapshot ?? new Snapshot(new List<ISetMethodBindingConverter>(8));

            // Copy-on-write update: clone the list
            var newList = new List<ISetMethodBindingConverter>(snap.Converters) { converter };

            // Publish the new snapshot (atomic via reference assignment)
            _snapshot = new Snapshot(newList);
        }
    }

    /// <summary>
    /// Attempts to retrieve the best set-method converter for the specified type pair.
    /// </summary>
    /// <param name="fromType">The source type to convert from. May be null.</param>
    /// <param name="toType">The target type to convert to. May be null.</param>
    /// <returns>
    /// The converter with the highest affinity for the type pair, or <see langword="null"/> if no converter supports the conversion.
    /// </returns>
    public ISetMethodBindingConverter? TryGetConverter(
#if NET
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        Type? fromType,
#if NET
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        Type? toType)
    {
        var snap = Volatile.Read(ref _snapshot);
        if (snap is null)
        {
            return null;
        }

        // Find the converter with the highest affinity
        ISetMethodBindingConverter? best = null;
        var bestScore = -1;

        var converters = snap.Converters;
        for (var i = 0; i < converters.Count; i++)
        {
            var converter = converters[i];
            var score = converter.GetAffinityForObjects(fromType, toType);
            if (score > bestScore && score > 0)
            {
                bestScore = score;
                best = converter;
            }
        }

        return best;
    }

    /// <summary>
    /// Returns all registered set-method converters.
    /// </summary>
    /// <returns>
    /// A sequence of all set-method converters currently registered in the registry.
    /// Returns an empty sequence if no converters are registered.
    /// </returns>
    public IEnumerable<ISetMethodBindingConverter> GetAllConverters()
    {
        var snap = Volatile.Read(ref _snapshot);
        if (snap is null)
        {
            return [];
        }

        return [.. snap.Converters];
    }

    /// <summary>
    /// Immutable snapshot of the registry state for lock-free reads.
    /// </summary>
    /// <param name="Converters">The registered set-method converters.</param>
    private sealed record Snapshot(List<ISetMethodBindingConverter> Converters);
}

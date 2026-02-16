// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

using ReactiveUI.Binding.Helpers;

namespace ReactiveUI.Binding;

/// <summary>
/// Thread-safe registry for fallback binding converters using a lock-free snapshot pattern.
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
/// <strong>Selection:</strong> Fallback converters are stored in a simple list (no type-pair grouping).
/// When looking up a converter, each converter's runtime affinity is checked via
/// <see cref="IBindingFallbackConverter.GetAffinityForObjects(Type, Type)"/>.
/// The converter with the highest affinity (&gt; 0) is selected.
/// </description></item>
/// </list>
/// <para>
/// Fallback converters are used when no exact type-pair match is found in the typed converter registry.
/// They provide runtime type checking and conversion using techniques like reflection or type descriptors.
/// </para>
/// </remarks>
public sealed class BindingFallbackConverterRegistry
{
#if NET9_0_OR_GREATER
    private readonly Lock _gate = new();
#else
    private readonly object _gate = new();
#endif

    private Snapshot? _snapshot;

    /// <summary>
    /// Registers a fallback binding converter.
    /// </summary>
    /// <param name="converter">The converter to register. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="converter"/> is null.</exception>
    /// <remarks>
    /// <para>
    /// Fallback converters are consulted when no exact type-pair converter is found.
    /// Multiple fallback converters can be registered; when retrieved, the converter with
    /// the highest affinity for the requested type pair will be selected.
    /// </para>
    /// </remarks>
    public void Register(IBindingFallbackConverter converter)
    {
        ArgumentExceptionHelper.ThrowIfNull(converter);

        lock (_gate)
        {
            var snap = _snapshot ?? new Snapshot(new List<IBindingFallbackConverter>(8));

            // Copy-on-write update: clone the list
            var newList = new List<IBindingFallbackConverter>(snap.Converters) { converter };

            // Publish the new snapshot (atomic via reference assignment)
            _snapshot = new Snapshot(newList);
        }
    }

    /// <summary>
    /// Attempts to retrieve the best fallback converter for the specified type pair.
    /// </summary>
    /// <param name="fromType">The source type to convert from.</param>
    /// <param name="toType">The target type to convert to.</param>
    /// <returns>
    /// The converter with the highest affinity for the type pair, or <see langword="null"/> if no converter supports the conversion.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="fromType"/> or <paramref name="toType"/> is null.
    /// </exception>
    public IBindingFallbackConverter? TryGetConverter(
#if NET
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        Type fromType,
#if NET
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        Type toType)
    {
        ArgumentExceptionHelper.ThrowIfNull(fromType);
        ArgumentExceptionHelper.ThrowIfNull(toType);

        var snap = Volatile.Read(ref _snapshot);
        if (snap is null)
        {
            return null;
        }

        // Find the converter with the highest affinity
        IBindingFallbackConverter? best = null;
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
    /// Returns all registered fallback converters.
    /// </summary>
    /// <returns>
    /// A sequence of all fallback converters currently registered in the registry.
    /// Returns an empty sequence if no converters are registered.
    /// </returns>
    public IEnumerable<IBindingFallbackConverter> GetAllConverters()
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
    /// <param name="Converters">The registered fallback converters.</param>
    private sealed record Snapshot(List<IBindingFallbackConverter> Converters);
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Asks the registered resolvers which thread owns the object a binding is writing to.</summary>
/// <remarks>
/// The registered set is resolved once and kept, because every binding an application creates asks this and
/// re-reading the locator allocates an enumeration per call. <see cref="Refresh"/> drops the resolved set for
/// a host that registers a resolver after its first binding.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ViewThreadResolvers
{
    /// <summary>The resolved resolvers, or null while none have been resolved yet.</summary>
    private static IViewThreadResolver[]? _resolvers;

    /// <summary>Re-reads the registered resolvers, for a host that registers one after its first binding.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Refresh() => Interlocked.Exchange(ref _resolvers, null);

    /// <summary>Names the thread that owns <paramref name="target"/>.</summary>
    /// <param name="target">The object a binding is about to write to.</param>
    /// <returns>The context owning it, or null when nothing claims it.</returns>
    /// <remarks>
    /// The first resolver to claim the target wins. A target nothing claims has no thread affinity anybody
    /// declared, so the write is left where the caller put it.
    /// </remarks>
    public static SynchronizationContext? ForTarget(object? target)
    {
        if (target is null)
        {
            return null;
        }

        var resolvers = Resolve();
        for (var i = 0; i < resolvers.Length; i++)
        {
            var context = resolvers[i].ContextFor(target);
            if (context is not null)
            {
                return context;
            }
        }

        return null;
    }

    /// <summary>Resolves the registered resolvers once and keeps them.</summary>
    /// <returns>The registered resolvers, empty when none is registered.</returns>
    /// <remarks>
    /// Publishing with a compare-exchange rather than a lock means the read that every binding makes is a
    /// plain field read. Two threads racing the first resolve both ask the locator and one array is discarded,
    /// which costs less than making every later caller take a lock to avoid it.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IViewThreadResolver[] Resolve()
    {
        var resolved = Volatile.Read(ref _resolvers);
        if (resolved is not null)
        {
            return resolved;
        }

        IViewThreadResolver[] built = [.. AppLocator.Current.GetServices<IViewThreadResolver>()];

        return Interlocked.CompareExchange(ref _resolvers, built, null) ?? built;
    }
}

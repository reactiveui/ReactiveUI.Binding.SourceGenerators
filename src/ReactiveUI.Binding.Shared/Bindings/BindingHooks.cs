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

/// <summary>Lets a registered <see cref="IPropertyBindingHook"/> inspect, or refuse, a binding as it is created.</summary>
/// <remarks>
/// <para>
/// A hook is consulted once per binding, before anything is wired, and a hook that returns false cancels the
/// binding outright. Registration is by the service locator, so the set is only known at run time even though
/// the binding itself was resolved at compile time.
/// </para>
/// <para>
/// Nothing is asked of the locator until a hook is actually registered. Almost no application registers one,
/// and a binding is created per control per view, so paying for a service lookup and two property-chain
/// closures on every one of them would be a cost the overwhelming majority never gets anything back for.
/// <see cref="Any"/> is the guard a caller tests first, and it stays false until <see cref="Refresh"/> is
/// told the registrations changed.
/// </para>
/// </remarks>
public static class BindingHooks
{
    /// <summary>Guards <see cref="_hooks"/> while it is (re)resolved.</summary>
    private static readonly Lock Gate = new();

    /// <summary>The resolved hooks, or null while none has been resolved yet.</summary>
    private static IPropertyBindingHook[]? _hooks;

    /// <summary>Gets a value indicating whether any hook is registered.</summary>
    /// <remarks>
    /// Test this before building the arguments to <see cref="ShouldBind"/>: the closures they need cost more
    /// than the check, and are wasted whenever the answer is false.
    /// </remarks>
    public static bool Any => Resolve().Length > 0;

    /// <summary>Re-reads the registered hooks, for a host that registers them after the first binding.</summary>
    public static void Refresh()
    {
        lock (Gate)
        {
            _hooks = null;
        }
    }

    /// <summary>Asks every registered hook whether this binding may be created.</summary>
    /// <param name="source">The source object, typically the view model.</param>
    /// <param name="target">The target object, typically the view.</param>
    /// <param name="getSourceProperties">Reads the current source-side values.</param>
    /// <param name="getTargetProperties">Reads the current target-side values.</param>
    /// <param name="direction">Which way the binding runs.</param>
    /// <returns><see langword="true"/> when the binding may proceed; <see langword="false"/> when a hook refused it.</returns>
    /// <exception cref="ArgumentNullException">A required argument is null.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static bool ShouldBind(
        object? source,
        object target,
        Func<IObservedChange<object, object>[]> getSourceProperties,
        Func<IObservedChange<object, object>[]> getTargetProperties,
        BindingDirection direction)
    {
        ArgumentExceptionHelper.ThrowIfNull(target);
        ArgumentExceptionHelper.ThrowIfNull(getSourceProperties);
        ArgumentExceptionHelper.ThrowIfNull(getTargetProperties);

        var hooks = Resolve();
        for (var i = 0; i < hooks.Length; i++)
        {
            if (!hooks[i].ExecuteHook(source, target, getSourceProperties, getTargetProperties, direction))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Resolves the registered hooks once and keeps them.</summary>
    /// <returns>The registered hooks, empty when none is registered.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IPropertyBindingHook[] Resolve()
    {
        var resolved = _hooks;
        if (resolved is not null)
        {
            return resolved;
        }

        lock (Gate)
        {
            _hooks ??= [.. AppLocator.Current.GetServices<IPropertyBindingHook>()];
            return _hooks;
        }
    }
}

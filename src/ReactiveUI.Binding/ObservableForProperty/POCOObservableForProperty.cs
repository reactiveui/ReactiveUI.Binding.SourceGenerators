// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.ObservableForProperty;

/// <summary>
/// Final fallback implementation for observation when no observable mechanism is available.
/// Emits exactly one value (the current value at subscription time) and then never emits again.
/// </summary>
[SuppressMessage(
    "Minor Code Smell",
    "S101:Types should be named in PascalCase",
    Justification = "POCO is an established acronym (Plain Old CLR Object) and matches the ReactiveUI public API name.")]
public sealed class POCOObservableForProperty : ICreatesObservableForProperty
{
    /// <summary>
    /// Tracks which (type, property) pairs have already emitted a POCO warning to avoid duplicate messages.
    /// </summary>
    private static readonly ConcurrentDictionary<(Type Type, string PropertyName), byte> HasWarned = new();

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection over runtime types which is not trim- or AOT-safe.")]
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged)
    {
        ArgumentExceptionHelper.ThrowIfNull(type);
        ArgumentExceptionHelper.ThrowIfNull(propertyName);

        return BindingAffinity.Fallback;
    }

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection over runtime types which is not trim- or AOT-safe.")]
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        Expression expression,
        string propertyName,
        bool beforeChanged,
        bool suppressWarnings)
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(expression);
        ArgumentExceptionHelper.ThrowIfNull(propertyName);

        if (!suppressWarnings)
        {
            WarnOnce(sender, propertyName);
        }

        // Emit the current value once, then never complete (so the binding stays alive).
        return new StartWithObservable<IObservedChange<object, object?>>(
            NeverObservable<IObservedChange<object, object?>>.Instance,
            new ObservedChange<object, object?>(sender, expression, default));
    }

    /// <summary>
    /// Emits a debug warning the first time a POCO property is observed, indicating that
    /// no change notifications will be sent after the initial value.
    /// </summary>
    /// <param name="sender">The object being observed.</param>
    /// <param name="propertyName">The name of the property being observed.</param>
#if NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static void WarnOnce(object sender, string propertyName)
    {
        var type = sender.GetType();
        if (!HasWarned.TryAdd((type, propertyName), 0))
        {
            return;
        }

        Debug.WriteLine(
            $"[ReactiveUI.Binding] Warning: The class {type.FullName} property {propertyName} is a POCO type and won't send change notifications, WhenAny will only return a single value!");
    }
}

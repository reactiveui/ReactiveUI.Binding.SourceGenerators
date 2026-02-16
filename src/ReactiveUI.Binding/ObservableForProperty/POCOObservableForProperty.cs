// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace ReactiveUI.Binding.ObservableForProperty;

/// <summary>
/// Final fallback implementation for observation when no observable mechanism is available.
/// Emits exactly one value (the current value at subscription time) and then never emits again.
/// </summary>
public sealed class POCOObservableForProperty : ICreatesObservableForProperty
{
    private static readonly ConcurrentDictionary<(Type Type, string PropertyName), byte> HasWarned = new();

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection over runtime types which is not trim- or AOT-safe.")]
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged)
    {
        ArgumentExceptionHelper.ThrowIfNull(type);
        ArgumentExceptionHelper.ThrowIfNull(propertyName);

        return 1;
    }

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection over runtime types which is not trim- or AOT-safe.")]
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(object sender, System.Linq.Expressions.Expression expression, string propertyName, bool beforeChanged = false, bool suppressWarnings = false)
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(expression);
        ArgumentExceptionHelper.ThrowIfNull(propertyName);

        if (!suppressWarnings)
        {
            WarnOnce(sender, propertyName);
        }

        return Observable
            .Return(new ObservedChange<object, object?>(sender, expression, default), CurrentThreadScheduler.Instance)
            .Concat(Observable.Never<IObservedChange<object, object?>>());
    }

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

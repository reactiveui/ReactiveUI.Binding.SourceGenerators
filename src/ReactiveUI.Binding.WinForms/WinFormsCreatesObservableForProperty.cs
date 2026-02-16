// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;
using System.Windows.Forms;

namespace ReactiveUI.Binding.WinForms;

/// <summary>
/// Creates observables for WinForms component properties by subscribing to
/// {PropertyName}Changed events via reflection.
/// </summary>
[RequiresUnreferencedCode("Uses reflection to find and subscribe to {PropertyName}Changed events on WinForms components.")]
public class WinFormsCreatesObservableForProperty : ICreatesObservableForProperty
{
    private static readonly ConcurrentDictionary<(Type Type, string PropertyName), EventInfo?> EventInfoCache = new();

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection to find {PropertyName}Changed events.")]
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged = false)
    {
        if (beforeChanged)
        {
            return 0;
        }

        if (!typeof(Component).IsAssignableFrom(type))
        {
            return 0;
        }

        return GetEventInfo(type, propertyName) is not null ? 8 : 0;
    }

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection to subscribe to {PropertyName}Changed events.")]
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        System.Linq.Expressions.Expression expression,
        string propertyName,
        bool beforeChanged = false,
        bool suppressWarnings = false)
    {
        if (sender is null)
        {
            throw new ArgumentNullException(nameof(sender));
        }

        var ei = GetEventInfo(sender.GetType(), propertyName) ?? throw new ArgumentException(
            $"Could not find event {propertyName}Changed on type {sender.GetType().Name}",
            nameof(propertyName));

        return Observable.Create<IObservedChange<object, object?>>(subj =>
        {
            var handler = new EventHandler((_, _) =>
                subj.OnNext(new ObservedChange<object, object?>(sender, expression, default)));

            ei.AddEventHandler(sender, handler);
            return Disposable.Create(() => ei.RemoveEventHandler(sender, handler));
        });
    }

    internal static EventInfo? GetEventInfo(Type type, string propertyName) =>
        EventInfoCache.GetOrAdd(
            (type, propertyName),
            key => key.Type.GetEvent(key.PropertyName + "Changed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy));
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;

namespace ReactiveUI.Binding.WinForms;

/// <summary>
/// Creates observables for WinForms component properties by subscribing to
/// {PropertyName}Changed events via reflection.
/// </summary>
[RequiresUnreferencedCode("Uses reflection to find and subscribe to {PropertyName}Changed events on WinForms components.")]
public class WinFormsCreatesObservableForProperty : ICreatesObservableForProperty
{
    /// <summary>
    /// A concurrent dictionary cache used to store information about events corresponding
    /// to property changes on WinForms components.
    /// The key is a tuple consisting of a type and property name, and the value is the associated <see cref="EventInfo"/>
    /// if the {PropertyName}Changed event exists, or null if it does not.
    /// </summary>
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
        ArgumentExceptionHelper.ThrowIfNull(sender);

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

    /// <summary>
    /// Retrieves the <see cref="EventInfo"/> for the specified {PropertyName}Changed event
    /// on the given type.
    /// </summary>
    /// <param name="type">The type to be inspected for the event.</param>
    /// <param name="propertyName">The name of the property whose corresponding event information is to be retrieved.</param>
    /// <returns>
    /// An <see cref="EventInfo"/> object if the {PropertyName}Changed event is found; otherwise, null.
    /// </returns>
    internal static EventInfo? GetEventInfo(Type type, string propertyName) =>
        EventInfoCache.GetOrAdd(
            (type, propertyName),
            key => key.Type.GetEvent(key.PropertyName + "Changed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy));
}

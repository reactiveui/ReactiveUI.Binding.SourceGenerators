// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reflection;
using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.ObservableForProperty;

/// <summary>
/// Generates Observables based on observing INotifyPropertyChanged objects.
/// </summary>
[SuppressMessage(
    "Minor Code Smell",
    "S101:Types should be named in PascalCase",
    Justification = "INPC is an established acronym for INotifyPropertyChanged and matches the ReactiveUI public API name.")]
public class INPCObservableForProperty : ICreatesObservableForProperty
{
    /// <summary>
    /// The affinity returned when the target type implements the relevant notification interface.
    /// </summary>
    private static readonly int SupportedAffinity = BindingAffinity.Explicit;

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection over runtime types which is not trim- or AOT-safe.")]
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged)
    {
        var target = beforeChanged ? typeof(INotifyPropertyChanging) : typeof(INotifyPropertyChanged);
        return target.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()) ? SupportedAffinity : 0;
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
        ArgumentExceptionHelper.ThrowIfNull(expression);

        // Before-change uses INotifyPropertyChanging; after-change (and the before-change fallback for
        // types that only implement INotifyPropertyChanged) uses PropertyChanged. A single fused sink
        // wires the event, filters by name, and emits the observed change directly.
        if ((beforeChanged && sender is INotifyPropertyChanging) || sender is INotifyPropertyChanged)
        {
            var expectedName = expression.NodeType == ExpressionType.Index ? propertyName + "[]" : propertyName;
            return new NotifyPropertyChangedObservable(sender, expression, expectedName, beforeChanged);
        }

        return NeverObservable<IObservedChange<object, object?>>.Instance;
    }
}

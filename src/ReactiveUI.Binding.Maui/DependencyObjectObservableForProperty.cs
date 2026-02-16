// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if WINUI_TARGET
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.UI.Xaml;

namespace ReactiveUI.Binding.Maui;

/// <summary>
/// Creates an observable for a property if available that is based on a WinUI DependencyProperty.
/// </summary>
[RequiresUnreferencedCode("Uses reflection to find DependencyProperty static fields/properties.")]
public class DependencyObjectObservableForProperty : ICreatesObservableForProperty
{
    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection to find DependencyProperty.")]
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged = false)
    {
        if (!typeof(DependencyObject).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
        {
            return 0;
        }

        if (GetDependencyPropertyFetcher(type, propertyName) is null)
        {
            return 0;
        }

        return 6;
    }

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection to find DependencyProperty.")]
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        Expression expression,
        string propertyName,
        bool beforeChanged = false,
        bool suppressWarnings = false)
    {
        if (sender is null)
        {
            throw new ArgumentNullException(nameof(sender));
        }

        if (sender is not DependencyObject depSender)
        {
            throw new ArgumentException("The sender must be a DependencyObject", nameof(sender));
        }

        var type = sender.GetType();

        if (beforeChanged)
        {
            Debug.WriteLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "[ReactiveUI.Binding.Maui] Tried to bind DO {0}.{1}, but DPs can't do beforeChanged. Binding as POCO object",
                    type.FullName,
                    propertyName));

            var ret = new POCOObservableForProperty();
            return ret.GetNotificationForProperty(sender, expression, propertyName, beforeChanged, suppressWarnings);
        }

        var dpFetcher = GetDependencyPropertyFetcher(type, propertyName);
        if (dpFetcher is null)
        {
            Debug.WriteLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "[ReactiveUI.Binding.Maui] Tried to bind DO {0}.{1}, but DP doesn't exist. Binding as POCO object",
                    type.FullName,
                    propertyName));

            var ret = new POCOObservableForProperty();
            return ret.GetNotificationForProperty(sender, expression, propertyName, beforeChanged, suppressWarnings);
        }

        return Observable.Create<IObservedChange<object, object?>>(subj =>
        {
            var handler = new DependencyPropertyChangedCallback((_, _) =>
                subj.OnNext(new ObservedChange<object, object?>(sender, expression, default)));

            var dependencyProperty = dpFetcher();
            var token = depSender.RegisterPropertyChangedCallback(dependencyProperty, handler);
            return Disposable.Create(() => depSender.UnregisterPropertyChangedCallback(dependencyProperty, token));
        });
    }

    [RequiresUnreferencedCode("Uses reflection to walk type hierarchy.")]
    internal static PropertyInfo? ActuallyGetProperty(TypeInfo typeInfo, string propertyName)
    {
        var current = typeInfo;
        while (current is not null)
        {
            var ret = current.GetDeclaredProperty(propertyName);
            if (ret is not null && ret.GetMethod is not null && ret.GetMethod.IsStatic)
            {
                return ret;
            }

            current = current.BaseType?.GetTypeInfo();
        }

        return null;
    }

    [RequiresUnreferencedCode("Uses reflection to walk type hierarchy.")]
    internal static FieldInfo? ActuallyGetField(TypeInfo typeInfo, string propertyName)
    {
        var current = typeInfo;
        while (current is not null)
        {
            var ret = current.GetDeclaredField(propertyName);
            if (ret?.IsStatic == true)
            {
                return ret;
            }

            current = current.BaseType?.GetTypeInfo();
        }

        return null;
    }

    [RequiresUnreferencedCode("Uses reflection to find DependencyProperty static fields/properties.")]
    internal static Func<DependencyProperty>? GetDependencyPropertyFetcher(Type type, string propertyName)
    {
        var typeInfo = type.GetTypeInfo();

        // Look for the DependencyProperty attached to this property name
        var pi = ActuallyGetProperty(typeInfo, propertyName + "Property");
        if (pi is not null)
        {
            var value = pi.GetValue(null);

            if (value is null)
            {
                return null;
            }

            return () => (DependencyProperty)value;
        }

        var fi = ActuallyGetField(typeInfo, propertyName + "Property");
        if (fi is not null)
        {
            var value = fi.GetValue(null);

            if (value is null)
            {
                return null;
            }

            return () => (DependencyProperty)value;
        }

        return null;
    }
}
#endif

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;

using ReactiveUI.Binding.Expressions;

namespace ReactiveUI.Binding.ObservableForProperty;

/// <summary>
/// Extension methods for property change observation using expression chains.
/// This is a runtime fallback bridge class — the source generator produces optimized
/// code that bypasses this entirely at compile time.
/// </summary>
[ExcludeFromCodeCoverage]
[EditorBrowsable(EditorBrowsableState.Never)]
[RequiresUnreferencedCode(
    "Creating Expressions requires unreferenced code because the members being referenced by the Expression may be trimmed.")]
public static class ReactiveNotifyPropertyChangedMixin
{
    private static readonly MemoizingMRUCache<(Type senderType, string propertyName, bool beforeChange), ICreatesObservableForProperty?>
        NotifyFactoryCache =
            new(
                (t, _) => AppLocator.Current.GetServices<ICreatesObservableForProperty>()
                    .Aggregate(
                        (score: 0, binding: (ICreatesObservableForProperty?)null),
                        (acc, x) =>
                        {
                            var score = x.GetAffinityForObject(t.senderType, t.propertyName, t.beforeChange);
                            return score > acc.score ? (score, x) : acc;
                        }).binding,
                64);

    /// <summary>
    /// ObservableForProperty returns an Observable representing the
    /// property change notifications for a specific property on an object.
    /// This overload avoids expression tree analysis by using a property name string.
    /// </summary>
    /// <typeparam name="TSender">The sender type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="item">The source object to observe properties of.</param>
    /// <param name="propertyName">The property name to observe.</param>
    /// <param name="beforeChange">If true, the Observable will notify immediately before a property is going to change.</param>
    /// <param name="skipInitial">If true, the Observable will not notify with the initial value.</param>
    /// <param name="isDistinct">If set to true, values are filtered with DistinctUntilChanged.</param>
    /// <returns>An Observable representing the property change notifications for the given property name.</returns>
    [RequiresUnreferencedCode(
        "Creating Expressions requires unreferenced code because the members being referenced by the Expression may be trimmed.")]
    public static IObservable<IObservedChange<TSender, TValue>> ObservableForProperty<TSender, TValue>(
        this TSender? item,
        string propertyName,
        bool beforeChange = false,
        bool skipInitial = true,
        bool isDistinct = true)
    {
        ArgumentExceptionHelper.ThrowIfNull(item);
        ArgumentExceptionHelper.ThrowIfNull(propertyName);

        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TSender), "x");
        System.Linq.Expressions.Expression expr;
        try
        {
            expr = System.Linq.Expressions.Expression.Property(parameter, propertyName);
        }
        catch
        {
            expr = parameter;
        }

        var factory = NotifyFactoryCache.Get((item!.GetType(), propertyName, beforeChange))
                      ?? throw new InvalidOperationException(
                          $"Could not find a ICreatesObservableForProperty for {item.GetType()} property {propertyName}. " +
                          "This should never happen, your service locator is probably broken. " +
                          "Please make sure you have registered ICreatesObservableForProperty implementations.");

        static TValue GetCurrentValue(object sender, string name)
        {
            var t = sender.GetType();
            var prop = t.GetProperty(
                name,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
            if (prop is null)
            {
                return default!;
            }

            var val = prop.GetValue(sender);
            if (val is null)
            {
                return default!;
            }

            return val is TValue tv ? tv : (TValue)val;
        }

        var core = Observable.Create<IObservedChange<TSender, TValue>>(obs =>
        {
            if (!skipInitial)
            {
                try
                {
                    var initial = GetCurrentValue(item!, propertyName);
                    obs.OnNext(new ObservedChange<TSender, TValue>(item!, expr, initial));
                }
                catch (Exception ex)
                {
                    obs.OnError(ex);
                }
            }

            var subscription = factory
                .GetNotificationForProperty(item!, expr, propertyName, beforeChange, suppressWarnings: false)
                .Subscribe(
                    _ =>
                    {
                        try
                        {
                            var current = GetCurrentValue(item!, propertyName);
                            obs.OnNext(new ObservedChange<TSender, TValue>(item!, expr, current));
                        }
                        catch (Exception ex)
                        {
                            obs.OnError(ex);
                        }
                    },
                    obs.OnError,
                    obs.OnCompleted);

            return subscription;
        });

        if (isDistinct)
        {
            return core.DistinctUntilChanged(x => x.Value);
        }

        return core;
    }

    /// <summary>
    /// ObservableForProperty returns an Observable representing the
    /// property change notifications for a specific property on an object.
    /// This method uses expression trees to identify the property.
    /// </summary>
    /// <typeparam name="TSender">The sender type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="item">The source object to observe properties of.</param>
    /// <param name="property">An Expression representing the property.</param>
    /// <param name="beforeChange">If true, the Observable will notify immediately before a property is going to change.</param>
    /// <param name="skipInitial">If true, the Observable will not notify with the initial value.</param>
    /// <param name="isDistinct">If set to true, values are filtered with DistinctUntilChanged.</param>
    /// <returns>An Observable representing the property change notifications for the given property.</returns>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    public static IObservable<IObservedChange<TSender, TValue>> ObservableForProperty<TSender, TValue>(
        this TSender? item,
        System.Linq.Expressions.Expression<Func<TSender, TValue>> property,
        bool beforeChange = false,
        bool skipInitial = true,
        bool isDistinct = true)
    {
        ArgumentExceptionHelper.ThrowIfNull(property);

        return SubscribeToExpressionChain<TSender, TValue>(
            item,
            property.Body,
            beforeChange,
            skipInitial,
            isDistinct);
    }

    /// <summary>
    /// Creates an observable which will subscribe to each property and sub-property
    /// specified in the Expression, providing updates to the last value in the chain.
    /// </summary>
    /// <typeparam name="TSender">The type of the origin of the expression chain.</typeparam>
    /// <typeparam name="TValue">The end value we want to subscribe to.</typeparam>
    /// <param name="source">The object where we start the chain.</param>
    /// <param name="expression">An expression which will point towards the property.</param>
    /// <param name="beforeChange">If we are interested in notifications before the property value is changed.</param>
    /// <param name="skipInitial">If we don't want to get a notification about the default value of the property.</param>
    /// <param name="isDistinct">If set to true, values are filtered with DistinctUntilChanged.</param>
    /// <returns>An observable which notifies about observed changes.</returns>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    public static IObservable<IObservedChange<TSender, TValue>> SubscribeToExpressionChain<TSender, TValue>(
        this TSender? source,
        System.Linq.Expressions.Expression? expression,
        bool beforeChange = false,
        bool skipInitial = true,
        bool isDistinct = true)
    {
        IObservable<IObservedChange<object?, object?>> notifier =
            Observable.Return(new ObservedChange<object?, object?>(null, null, source));

        var chain = Reflection.Rewrite(expression).GetExpressionChain();
        notifier = chain.Aggregate(
            notifier,
            (n, expr) => n
                .Select(y => NestedObservedChanges(expr, y, beforeChange))
                .Switch());

        if (skipInitial)
        {
            notifier = notifier.Skip(1);
        }

        notifier = notifier.Where(x => x.Sender is not null);

        var r = notifier.Select(x =>
        {
            var val = x.GetValueOrDefault();
            if (val is not null && val is not TValue)
            {
                throw new InvalidCastException($"Unable to cast from {val.GetType()} to {typeof(TValue)}.");
            }

            return new ObservedChange<TSender, TValue>(source!, expression, (TValue)val!);
        });

        return isDistinct ? r.DistinctUntilChanged(x => x.Value) : r;
    }

    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    internal static IObservable<IObservedChange<object?, object?>> NestedObservedChanges(
        System.Linq.Expressions.Expression expression,
        IObservedChange<object?, object?> sourceChange,
        bool beforeChange)
    {
        var kicker = new ObservedChange<object?, object?>(sourceChange.Value, expression, default);

        if (sourceChange.Value is null)
        {
            return Observable.Return(kicker);
        }

        return NotifyForProperty(sourceChange.Value, expression, beforeChange)
            .StartWith(kicker)
            .Select(static x => new ObservedChange<object?, object?>(x.Sender, x.Expression, x.GetValueOrDefault()));
    }

    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    internal static IObservable<IObservedChange<object?, object?>> NotifyForProperty(
        object sender,
        System.Linq.Expressions.Expression expression,
        bool beforeChange)
    {
        ArgumentExceptionHelper.ThrowIfNull(expression);

        var memberInfo = expression.GetMemberInfo() ?? throw new ArgumentException(
            "The expression does not have valid member info",
            nameof(expression));
        var propertyName = memberInfo.Name;
        var result = NotifyFactoryCache.Get((sender.GetType(), propertyName, beforeChange));

        return result switch
        {
            null => throw new InvalidOperationException(
                $"Could not find a ICreatesObservableForProperty for {sender.GetType()} property {propertyName}. " +
                "This should never happen, your service locator is probably broken. " +
                "Please make sure you have registered ICreatesObservableForProperty implementations."),
            _ => result.GetNotificationForProperty(sender, expression, propertyName, beforeChange, suppressWarnings: false)
        };
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.ObservableForProperty;
#else
namespace ReactiveUI.Binding.ObservableForProperty;
#endif

/// <summary>
/// Extension methods for property change observation using expression chains.
/// This is a runtime fallback bridge class — the source generator produces optimized
/// code that bypasses this entirely at compile time.
/// </summary>
[ExcludeFromCodeCoverage]
[EditorBrowsable(EditorBrowsableState.Never)]
[RequiresUnreferencedCode(
    "Creating Expressions requires unreferenced code because the members being referenced by the Expression may be trimmed.")]
public static class ReactiveNotifyPropertyChangedMixins
{
    /// <summary>Number of resolved factories kept in the MRU cache.</summary>
    private const int NotifyFactoryCacheSize = 64;

    /// <summary>Appended to the "no factory found" message; the only way to hit it is a misconfigured locator.</summary>
    private const string BrokenLocatorAdvice =
        "This should never happen, your service locator is probably broken. Please make sure you have registered ICreatesObservableForProperty implementations.";

    /// <summary>MRU cache that maps (sender type, property name, before-change flag) to the best <see cref="ICreatesObservableForProperty"/> implementation for that combination.</summary>
    private static readonly MemoizingMRUCache<
        (Type senderType, string propertyName, bool beforeChange),
        ICreatesObservableForProperty?>
        NotifyFactoryCache =
            new(
                static (t, _) =>
                {
                    var bestScore = 0;
                    ICreatesObservableForProperty? best = null;
                    foreach (var candidate in AppLocator.Current.GetServices<ICreatesObservableForProperty>())
                    {
                        var score = candidate.GetAffinityForObject(t.senderType, t.propertyName, t.beforeChange);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            best = candidate;
                        }
                    }

                    return best;
                },
                NotifyFactoryCacheSize);

    /// <summary>Provides ObservableForProperty extension members for <paramref name="item"/>.</summary>
    /// <typeparam name="TSender">The sender type.</typeparam>
    /// <param name="item">The source object to observe properties of.</param>
    extension<TSender>(TSender? item)
    {
        /// <summary>ObservableForProperty by name, observing after-change, emitting the initial value, with distinct filtering.</summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="propertyName">The property name to observe.</param>
        /// <returns>An Observable representing the property change notifications for the given property name.</returns>
        [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the observed shape.")]
        [RequiresUnreferencedCode(
            "Creating Expressions requires unreferenced code because the members being referenced by the Expression may be trimmed.")]
        public IObservable<IObservedChange<TSender, TValue>> ObservableForProperty<TValue>(
            string propertyName) =>
            ObservableForProperty<TSender, TValue>(item, propertyName, false, true, true);

        /// <summary>ObservableForProperty by name, observing after-change with distinct filtering and a configurable initial value.</summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="propertyName">The property name to observe.</param>
        /// <param name="skipInitial">If true, the Observable will not notify with the initial value.</param>
        /// <returns>An Observable representing the property change notifications for the given property name.</returns>
        [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the observed shape.")]
        [RequiresUnreferencedCode(
            "Creating Expressions requires unreferenced code because the members being referenced by the Expression may be trimmed.")]
        public IObservable<IObservedChange<TSender, TValue>> ObservableForProperty<TValue>(
            string propertyName,
            bool skipInitial) =>
            ObservableForProperty<TSender, TValue>(item, propertyName, false, skipInitial, true);

        /// <summary>
        /// ObservableForProperty returns an Observable representing the
        /// property change notifications for a specific property on an object.
        /// This overload avoids expression tree analysis by using a property name string.
        /// </summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="propertyName">The property name to observe.</param>
        /// <param name="beforeChange">If true, the Observable will notify immediately before a property is going to change.</param>
        /// <param name="skipInitial">If true, the Observable will not notify with the initial value.</param>
        /// <param name="isDistinct">If set to true, values are filtered with DistinctUntilChanged.</param>
        /// <returns>An Observable representing the property change notifications for the given property name.</returns>
        [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the observed shape.")]
        [RequiresUnreferencedCode(
            "Creating Expressions requires unreferenced code because the members being referenced by the Expression may be trimmed.")]
        public IObservable<IObservedChange<TSender, TValue>> ObservableForProperty<TValue>(
            string propertyName,
            bool beforeChange,
            bool skipInitial,
            bool isDistinct)
        {
            ArgumentExceptionHelper.ThrowIfNull(item);
            ArgumentExceptionHelper.ThrowIfNull(propertyName);

            var parameter = Expression.Parameter(typeof(TSender), "x");
            Expression expr;
            try
            {
                expr = Expression.Property(parameter, propertyName);
            }
            catch
            {
                expr = parameter;
            }

            var factory = NotifyFactoryCache.Get((item!.GetType(), propertyName, beforeChange))
                          ?? throw new InvalidOperationException(
                              $"Could not find a ICreatesObservableForProperty for {item.GetType()} property {propertyName}. {BrokenLocatorAdvice}");

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

            // Single fused sink: emits the initial value (unless skipped), then re-reads and emits on each
            // notification, applying the distinct gate inline.
            var notifications = factory.GetNotificationForProperty(item!, expr, propertyName, beforeChange);
            return new ObservableForPropertySink<TSender, TValue>(
                item!,
                expr,
                notifications,
                () => GetCurrentValue(item!, propertyName),
                skipInitial,
                isDistinct);
        }

        /// <summary>ObservableForProperty by expression, observing after-change, emitting the initial value, with distinct filtering.</summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="property">An Expression representing the property.</param>
        /// <returns>An Observable representing the property change notifications for the given property.</returns>
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        public IObservable<IObservedChange<TSender, TValue>> ObservableForProperty<TValue>(
            Expression<Func<TSender, TValue>> property) =>
            ObservableForProperty(item, property, false, true, true);

        /// <summary>ObservableForProperty by expression, observing after-change with distinct filtering and a configurable initial value.</summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="property">An Expression representing the property.</param>
        /// <param name="skipInitial">If true, the Observable will not notify with the initial value.</param>
        /// <returns>An Observable representing the property change notifications for the given property.</returns>
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        public IObservable<IObservedChange<TSender, TValue>> ObservableForProperty<TValue>(
            Expression<Func<TSender, TValue>> property,
            bool skipInitial) =>
            ObservableForProperty(item, property, false, skipInitial, true);

        /// <summary>
        /// ObservableForProperty returns an Observable representing the
        /// property change notifications for a specific property on an object.
        /// This method uses expression trees to identify the property.
        /// </summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="property">An Expression representing the property.</param>
        /// <param name="beforeChange">If true, the Observable will notify immediately before a property is going to change.</param>
        /// <param name="skipInitial">If true, the Observable will not notify with the initial value.</param>
        /// <param name="isDistinct">If set to true, values are filtered with DistinctUntilChanged.</param>
        /// <returns>An Observable representing the property change notifications for the given property.</returns>
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        public IObservable<IObservedChange<TSender, TValue>> ObservableForProperty<TValue>(
            Expression<Func<TSender, TValue>> property,
            bool beforeChange,
            bool skipInitial,
            bool isDistinct)
        {
            ArgumentExceptionHelper.ThrowIfNull(property);

            return SubscribeToExpressionChain<TSender, TValue>(
                item,
                property.Body,
                beforeChange,
                skipInitial,
                isDistinct);
        }

        /// <summary>Subscribes to an expression chain, observing after-change, emitting the initial value, with distinct filtering.</summary>
        /// <typeparam name="TValue">The end value we want to subscribe to.</typeparam>
        /// <param name="expression">An expression which will point towards the property.</param>
        /// <returns>An observable which notifies about observed changes.</returns>
        [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the observed shape.")]
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        public IObservable<IObservedChange<TSender, TValue>> SubscribeToExpressionChain<TValue>(
            Expression? expression) =>
            SubscribeToExpressionChain<TSender, TValue>(item, expression, false, true, true);

        /// <summary>Subscribes to an expression chain, observing after-change with distinct filtering and a configurable initial value.</summary>
        /// <typeparam name="TValue">The end value we want to subscribe to.</typeparam>
        /// <param name="expression">An expression which will point towards the property.</param>
        /// <param name="skipInitial">If we don't want to get a notification about the default value of the property.</param>
        /// <returns>An observable which notifies about observed changes.</returns>
        [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the observed shape.")]
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        public IObservable<IObservedChange<TSender, TValue>> SubscribeToExpressionChain<TValue>(
            Expression? expression,
            bool skipInitial) =>
            SubscribeToExpressionChain<TSender, TValue>(item, expression, false, skipInitial, true);

        /// <summary>
        /// Creates an observable which will subscribe to each property and sub-property
        /// specified in the Expression, providing updates to the last value in the chain.
        /// </summary>
        /// <typeparam name="TValue">The end value we want to subscribe to.</typeparam>
        /// <param name="expression">An expression which will point towards the property.</param>
        /// <param name="beforeChange">If we are interested in notifications before the property value is changed.</param>
        /// <param name="skipInitial">If we don't want to get a notification about the default value of the property.</param>
        /// <param name="isDistinct">If set to true, values are filtered with DistinctUntilChanged.</param>
        /// <returns>An observable which notifies about observed changes.</returns>
        [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the observed shape.")]
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        public IObservable<IObservedChange<TSender, TValue>> SubscribeToExpressionChain<TValue>(
            Expression? expression,
            bool beforeChange,
            bool skipInitial,
            bool isDistinct)
        {
            // Single fused switching engine: one watcher per link, re-subscribing deeper links when an
            // intermediate value changes, with skip-initial, the non-null-parent filter, the cast to TValue,
            // and the distinct gate applied inline.
            var links = new List<Expression>(Reflection.Rewrite(expression).GetExpressionChain()).ToArray();
            return new ExpressionChainSink<TSender, TValue>(item, expression, links, beforeChange, skipInitial, isDistinct);
        }

    }

    /// <summary>Creates an observable that tracks nested property changes for a single link in an expression chain.</summary>
    /// <param name="expression">The expression representing the current property in the chain.</param>
    /// <param name="sourceChange">The observed change from the previous link in the chain.</param>
    /// <param name="beforeChange">If <see langword="true"/>, subscribes to before-change notifications.</param>
    /// <returns>An observable of observed changes for the current property link.</returns>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    internal static IObservable<IObservedChange<object?, object?>> NestedObservedChanges(
        Expression expression,
        IObservedChange<object?, object?> sourceChange,
        bool beforeChange)
    {
        var kicker = new ObservedChange<object?, object?>(sourceChange.Value, expression, default);

        return sourceChange.Value is null
            ? new ReturnObservable<IObservedChange<object?, object?>>(kicker)
            : new SelectObservable<IObservedChange<object?, object?>, IObservedChange<object?, object?>>(
            new StartWithObservable<IObservedChange<object?, object?>>(
                NotifyForProperty(sourceChange.Value, expression, beforeChange),
                kicker),
            static x => new ObservedChange<object?, object?>(x.Sender, x.Expression, x.GetValueOrDefault()));
    }

    /// <summary>
    /// Gets property change notifications for a single property on an object by resolving the
    /// best <see cref="ICreatesObservableForProperty"/> implementation from the service locator.
    /// </summary>
    /// <param name="sender">The object to observe.</param>
    /// <param name="expression">The expression identifying the property to observe.</param>
    /// <param name="beforeChange">If <see langword="true"/>, subscribes to before-change notifications.</param>
    /// <returns>An observable of observed changes for the property.</returns>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    internal static IObservable<IObservedChange<object?, object?>> NotifyForProperty(
        object sender,
        Expression expression,
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
                $"Could not find a ICreatesObservableForProperty for {sender.GetType()} property {propertyName}. {BrokenLocatorAdvice}"),
            _ => result.GetNotificationForProperty(sender, expression, propertyName, beforeChange)
        };
    }
}

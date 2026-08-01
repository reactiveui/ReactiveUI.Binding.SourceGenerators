// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.ObservableForProperty;
#else
namespace ReactiveUI.Binding.ObservableForProperty;
#endif

/// <summary>A collection of helpers for <see cref="IObservedChange{TSender, TValue}"/>.</summary>
public static class ObservedChangedMixins
{
    /// <summary>Provides Value extension members for <paramref name="stream"/>.</summary>
    /// <typeparam name="TSender">The sender type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="stream">The change notification stream to get the values of.</param>
    extension<TSender, TValue>(IObservable<IObservedChange<TSender, TValue>> stream)
    {
        /// <summary>
        /// Given a stream of notification changes, this method will convert
        /// the property changes to the current value of the property.
        /// </summary>
        /// <returns>An Observable representing the stream of current values.</returns>
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        public IObservable<TValue> Value() =>
            new SelectObservable<IObservedChange<TSender, TValue>, TValue>(stream, GetValue);
    }

    /// <summary>Provides value-access extension members for <paramref name="item"/>.</summary>
    /// <typeparam name="TSender">The sender type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="item">The observed change.</param>
    extension<TSender, TValue>(IObservedChange<TSender, TValue> item)
    {
        /// <summary>Returns the name of a property which has been changed.</summary>
        /// <returns>The name of the property which has changed.</returns>
        public string GetPropertyName()
        {
            ArgumentExceptionHelper.ThrowIfNull(item);
            return Reflection.ExpressionToPropertyNames(item.Expression);
        }

        /// <summary>Returns the current value of a property given a notification that it has changed.</summary>
        /// <returns>The current value of the property.</returns>
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        public TValue GetValue()
        {
            ArgumentExceptionHelper.ThrowIfNull(item);

            if (!item.TryGetValue(out var returnValue))
            {
                throw new InvalidOperationException($"One of the properties in the expression '{item.GetPropertyName()}' was null");
            }

            return returnValue;
        }

        /// <summary>
        /// Returns the current value of a property given a notification that it has changed,
        /// or the default value if the chain cannot be resolved.
        /// </summary>
        /// <returns>The current value of the property, or default.</returns>
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        public TValue? GetValueOrDefault()
        {
            ArgumentExceptionHelper.ThrowIfNull(item);
            return !item.TryGetValue(out var returnValue) ? default : returnValue;
        }

        /// <summary>Attempts to return the current value of a property given a notification that it has changed.</summary>
        /// <param name="changeValue">The value of the property expression.</param>
        /// <returns>True if the entire expression was able to be followed, false otherwise.</returns>
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        internal bool TryGetValue(
            out TValue changeValue)
        {
            // TValue is unconstrained, so default(TValue) is maybe-null while the comparer's parameter is not
            // annotated; the comparer handles a null operand, so the state is suppressed rather than guarded.
            if (EqualityComparer<TValue>.Default.Equals(item.Value, default!))
            {
                return Reflection.TryGetValueForPropertyChain(
                    out changeValue,
                    item.Sender,
                    item.Expression!.GetExpressionChain());
            }

            changeValue = item.Value;
            return true;
        }

        /// <summary>Given a fully filled-out IObservedChange object, SetValueToProperty will apply it to the specified object.</summary>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="target">The target object to apply the change to.</param>
        /// <param name="property">The target property to apply the change to.</param>
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        internal void SetValueToProperty<TTarget>(
            TTarget target,
            Expression<Func<TTarget, TValue>> property)
        {
            if (target is null)
            {
                return;
            }

            _ = Reflection.TrySetValueToPropertyChain(
                target,
                Reflection.Rewrite(property.Body).GetExpressionChain(),
                item.GetValue());
        }
    }
}

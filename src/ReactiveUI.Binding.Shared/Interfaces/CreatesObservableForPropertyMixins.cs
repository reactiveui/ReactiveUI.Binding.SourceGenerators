// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Convenience overloads for <see cref="ICreatesObservableForProperty"/> that supply the common
/// defaults. They are provided as overloads (rather than optional parameters on the interface) so the
/// interface stays free of optional parameters and works on target frameworks without default interface methods.
/// </summary>
public static class CreatesObservableForPropertyMixins
{
    /// <summary>Provides GetAffinityForObject extension members for <paramref name="factory"/>.</summary>
    /// <param name="factory">The observation factory.</param>
    extension(ICreatesObservableForProperty factory)
    {
        /// <summary>Returns the affinity for after-change observation of the specified property.</summary>
        /// <param name="type">The type being observed.</param>
        /// <param name="propertyName">The property name being observed.</param>
        /// <returns>The affinity score. Positive means supported.</returns>
        public int GetAffinityForObject(Type type, string propertyName)
        {
            ArgumentExceptionHelper.ThrowIfNull(factory);
            return factory.GetAffinityForObject(type, propertyName, false);
        }

        /// <summary>Creates an observable that fires after the specified property changes, without suppressing warnings.</summary>
        /// <param name="sender">The object to observe.</param>
        /// <param name="expression">The expression identifying the property.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>An observable of observed changes.</returns>
        public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
            object sender,
            Expression expression,
            string propertyName)
        {
            ArgumentExceptionHelper.ThrowIfNull(factory);
            return factory.GetNotificationForProperty(sender, expression, propertyName, false, false);
        }

        /// <summary>Creates an observable that fires when the specified property changes, without suppressing warnings.</summary>
        /// <param name="sender">The object to observe.</param>
        /// <param name="expression">The expression identifying the property.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="beforeChanged">Whether to observe before-change events.</param>
        /// <returns>An observable of observed changes.</returns>
        public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
            object sender,
            Expression expression,
            string propertyName,
            bool beforeChanged)
        {
            ArgumentExceptionHelper.ThrowIfNull(factory);
            return factory.GetNotificationForProperty(sender, expression, propertyName, beforeChanged, false);
        }
    }
}

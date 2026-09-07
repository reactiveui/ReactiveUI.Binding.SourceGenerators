// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>
/// Chooses between the observation the generator emitted for one link of a property chain and a registered
/// <see cref="ICreatesObservableForProperty"/> that outranks the mechanism it was built from.
/// </summary>
/// <remarks>
/// A link past the first is observed inside the switch that follows its parent, which is an expression rather
/// than a statement, so the choice has to be a single call the generated code can sit in that position. Every
/// argument is something the generator already knew - the declaring type, the property name, the accessor and
/// the expression are all fixed at compile time - so a chain honours a registration link by link without
/// anything being resolved by name, and an ahead-of-time consumer carries no expression engine for it.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class PluginObservationSource
{
    /// <summary>Returns the registration's observation of a property, or the generated one when none outranks it.</summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="source">The object the property is read from.</param>
    /// <param name="expression">The property as the generated code names it.</param>
    /// <param name="propertyName">The name of the property being observed.</param>
    /// <param name="beforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="generatedAffinity">The affinity of the mechanism the generator picked for this link.</param>
    /// <param name="getter">Reads the current property value from the source.</param>
    /// <param name="generated">The observation the generator emitted for this link.</param>
    /// <returns>Whichever of the two should drive this link.</returns>
    /// <remarks>
    /// The link does not filter consecutive equal values: a chain applies that once over its whole result, and
    /// filtering per link as well would drop a value a later link still has to re-root on.
    /// </remarks>
    public static IObservable<T> Choose<T>(
        object source,
        Expression expression,
        string propertyName,
        bool beforeChange,
        int generatedAffinity,
        Func<object, T?> getter,
        IObservable<T> generated)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(generated);

        var plugin = ObservationAffinityChecker.FindHigherAffinityPlugin(
            source.GetType(),
            propertyName,
            generatedAffinity,
            beforeChange);

        return plugin is null
            ? generated
            : new PluginPropertyObservable<T>(plugin, source, expression, propertyName, getter, beforeChange, false);
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>Observes a property that has no change notification: its value, once, and then silence.</summary>
/// <typeparam name="T">The type of the property value.</typeparam>
/// <remarks>
/// <para>
/// Deliberately never completes. A property with no mechanism behind it has one value and no further news,
/// which is not the same as a sequence that has ended - and downstream operators read the difference. A
/// completing source ends a binding's subscription, and inside a chain it lets the stage below tear down a
/// subtree that is still live. The runtime engine's own fallback stays open for exactly this reason.
/// </para>
/// <para>
/// Hand-rolled rather than composed. The general spelling is a return followed by a never, concatenated,
/// which is three objects where this is one - on a path taken once per binding of every unobservable
/// property in a view.
/// </para>
/// </remarks>
[DebuggerDisplay("UnchangingProperty: {_value}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class UnchangingPropertyObservable<T> : IObservable<T>
{
    /// <summary>The only value this observation will ever carry.</summary>
    private readonly T _value;

    /// <summary>Initializes a new instance of the <see cref="UnchangingPropertyObservable{T}"/> class.</summary>
    /// <param name="value">The property's current value, which is also its final one.</param>
    public UnchangingPropertyObservable(T value) => _value = value;

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        observer.OnNext(_value);

        return new Subscription();
    }

    /// <summary>
    /// The teardown for a subscription that holds nothing. Disposing it is what a caller does with the
    /// handle it was given; there is no handler to unhook and no completion to suppress.
    /// </summary>
    private sealed class Subscription : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}

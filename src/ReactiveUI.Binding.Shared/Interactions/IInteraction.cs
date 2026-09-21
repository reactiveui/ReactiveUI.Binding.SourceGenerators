// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Represents an interaction between collaborating application components.</summary>
/// <typeparam name="TInput">The interaction's input type.</typeparam>
/// <typeparam name="TOutput">The interaction's output type.</typeparam>
/// <remarks>
/// <para>
/// Interactions allow collaborating components in an application to ask each other questions. Typically,
/// interactions allow a view model to get the user's confirmation from the view before proceeding with
/// some operation. The view provides the interaction's confirmation interface in a handler registered
/// for the interaction.
/// </para>
/// <para>
/// Interactions have both an input and an output. The interaction's input provides handlers the information
/// they require to ask a question. The handler then provides the interaction with an output as the answer.
/// </para>
/// <para>
/// This contract covers handler registration only, which is all a binding needs. Asking the question is the
/// caller's own concern, so the method that does it belongs to the implementation:
/// <see cref="Interaction{TInput, TOutput}.Handle(TInput)"/> returns a task, while a host framework that
/// prefers observables can return one from its own type without conflicting with this interface.
/// </para>
/// </remarks>
public interface IInteraction<TInput, TOutput>
{
    /// <summary>Registers a synchronous interaction handler.</summary>
    /// <param name="handler">The handler.</param>
    /// <returns>A disposable which, when disposed, will unregister the handler.</returns>
    IDisposable RegisterHandler(Action<IInteractionContext<TInput, TOutput>> handler);

    /// <summary>Registers a task-based asynchronous interaction handler.</summary>
    /// <param name="handler">The handler.</param>
    /// <returns>A disposable which, when disposed, will unregister the handler.</returns>
    IDisposable RegisterHandler(Func<IInteractionContext<TInput, TOutput>, Task> handler);

    /// <summary>Registers an observable-based asynchronous interaction handler.</summary>
    /// <typeparam name="TDontCare">The signal type.</typeparam>
    /// <param name="handler">The handler.</param>
    /// <returns>A disposable which, when disposed, will unregister the handler.</returns>
    IDisposable RegisterHandler<TDontCare>(Func<IInteractionContext<TInput, TOutput>, IObservable<TDontCare>> handler);
}

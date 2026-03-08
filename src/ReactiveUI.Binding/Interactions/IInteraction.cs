// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Represents an interaction between collaborating application components.
/// </summary>
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
/// </remarks>
/// <typeparam name="TInput">The interaction's input type.</typeparam>
/// <typeparam name="TOutput">The interaction's output type.</typeparam>
public interface IInteraction<TInput, TOutput>
{
    /// <summary>
    /// Registers a synchronous interaction handler.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <returns>A disposable which, when disposed, will unregister the handler.</returns>
    IDisposable RegisterHandler(Action<IInteractionContext<TInput, TOutput>> handler);

    /// <summary>
    /// Registers a task-based asynchronous interaction handler.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <returns>A disposable which, when disposed, will unregister the handler.</returns>
    IDisposable RegisterHandler(Func<IInteractionContext<TInput, TOutput>, Task> handler);

    /// <summary>
    /// Registers an observable-based asynchronous interaction handler.
    /// </summary>
    /// <typeparam name="TDontCare">The signal type.</typeparam>
    /// <param name="handler">The handler.</param>
    /// <returns>A disposable which, when disposed, will unregister the handler.</returns>
    IDisposable RegisterHandler<TDontCare>(Func<IInteractionContext<TInput, TOutput>, IObservable<TDontCare>> handler);

    /// <summary>
    /// Handles an interaction and asynchronously returns the result.
    /// </summary>
    /// <param name="input">The input for the interaction.</param>
    /// <returns>A task that completes with the output when the interaction is handled.</returns>
    Task<TOutput> Handle(TInput input);
}

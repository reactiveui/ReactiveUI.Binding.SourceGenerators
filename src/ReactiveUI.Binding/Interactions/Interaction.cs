// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding;

/// <summary>
/// Represents an interaction between collaborating application components.
/// </summary>
/// <remarks>
/// <para>
/// Interactions allow collaborating components in an application to ask each other questions. Typically,
/// interactions allow a view model to get the user's confirmation from the view before proceeding with
/// some operation.
/// </para>
/// <para>
/// By default, handlers are invoked in reverse order of registration. That is, handlers registered later
/// are given the opportunity to handle interactions before handlers that were registered earlier.
/// </para>
/// <para>
/// Note that handlers are not required to handle an interaction. They can choose to ignore it, leaving it
/// for some other handler to handle. The interaction's <see cref="Handle"/> method will throw an
/// <see cref="UnhandledInteractionException{TInput, TOutput}"/> if no handler handles the interaction.
/// </para>
/// </remarks>
/// <typeparam name="TInput">The interaction's input type.</typeparam>
/// <typeparam name="TOutput">The interaction's output type.</typeparam>
public class Interaction<TInput, TOutput> : IInteraction<TInput, TOutput>
{
    /// <summary>
    /// The list of registered interaction handlers, invoked in reverse order during <see cref="Handle"/>.
    /// </summary>
    private readonly List<Func<IInteractionContext<TInput, TOutput>, Task>> _handlers = [];

    /// <summary>
    /// Synchronization gate for thread-safe handler registration and removal.
    /// </summary>
    private readonly object _sync = new();

    /// <inheritdoc/>
    public IDisposable RegisterHandler(Action<IInteractionContext<TInput, TOutput>> handler)
    {
        ArgumentExceptionHelper.ThrowIfNull(handler);

        return RegisterHandler(context =>
        {
            handler(context);
            return Task.CompletedTask;
        });
    }

    /// <inheritdoc />
    public IDisposable RegisterHandler(Func<IInteractionContext<TInput, TOutput>, Task> handler)
    {
        ArgumentExceptionHelper.ThrowIfNull(handler);

        AddHandler(handler);
        return new ActionDisposable(() => RemoveHandler(handler));
    }

    /// <inheritdoc />
    public IDisposable RegisterHandler<TDontCare>(Func<IInteractionContext<TInput, TOutput>, IObservable<TDontCare>> handler)
    {
        ArgumentExceptionHelper.ThrowIfNull(handler);

        Task ContentHandler(IInteractionContext<TInput, TOutput> context)
        {
            var tcs = new TaskCompletionSource<bool>();
            handler(context).Subscribe(new ObservableToTaskObserver<TDontCare>(tcs));
            return tcs.Task;
        }

        AddHandler(ContentHandler);
        return new ActionDisposable(() => RemoveHandler(ContentHandler));
    }

    /// <inheritdoc />
    public virtual async Task<TOutput> Handle(TInput input)
    {
        var context = GenerateContext(input);
        var handlers = GetHandlers();

        for (var i = handlers.Length - 1; i >= 0; i--)
        {
            await handlers[i](context).ConfigureAwait(false);
            if (context.IsHandled)
            {
                return context.GetOutput();
            }
        }

        throw new UnhandledInteractionException<TInput, TOutput>(this, input);
    }

    /// <summary>
    /// Gets all registered handlers by order of registration.
    /// </summary>
    /// <returns>All registered handlers.</returns>
    protected Func<IInteractionContext<TInput, TOutput>, Task>[] GetHandlers()
    {
        lock (_sync)
        {
            return _handlers.ToArray();
        }
    }

    /// <summary>
    /// Gets an interaction context which is used to provide information about the interaction.
    /// </summary>
    /// <param name="input">The input that is being passed in.</param>
    /// <returns>The interaction context.</returns>
    protected virtual IOutputContext<TInput, TOutput> GenerateContext(TInput input)
        => new InteractionContext<TInput, TOutput>(input);

    /// <summary>
    /// Adds a handler to the internal handler list under the synchronization gate.
    /// </summary>
    /// <param name="handler">The handler to add.</param>
    private void AddHandler(Func<IInteractionContext<TInput, TOutput>, Task> handler)
    {
        lock (_sync)
        {
            _handlers.Add(handler);
        }
    }

    /// <summary>
    /// Removes a handler from the internal handler list under the synchronization gate.
    /// </summary>
    /// <param name="handler">The handler to remove.</param>
    private void RemoveHandler(Func<IInteractionContext<TInput, TOutput>, Task> handler)
    {
        lock (_sync)
        {
            _handlers.Remove(handler);
        }
    }

    /// <summary>
    /// An observer that bridges an observable sequence to a <see cref="TaskCompletionSource{TResult}"/>,
    /// completing the task when the observable completes or faults.
    /// </summary>
    /// <typeparam name="T">The element type of the observable sequence.</typeparam>
    private sealed class ObservableToTaskObserver<T> : IObserver<T>
    {
        /// <summary>
        /// The task completion source that is signaled when the observable completes or errors.
        /// </summary>
        private readonly TaskCompletionSource<bool> _tcs;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableToTaskObserver{T}"/> class.
        /// </summary>
        /// <param name="tcs">The task completion source to signal.</param>
        public ObservableToTaskObserver(TaskCompletionSource<bool> tcs) => _tcs = tcs;

        /// <inheritdoc/>
        public void OnCompleted() => _tcs.TrySetResult(true);

        /// <inheritdoc/>
        public void OnError(Exception error) => _tcs.TrySetException(error);

        /// <inheritdoc/>
        public void OnNext(T value)
        {
            // Intentionally empty — we only care about completion/error.
        }
    }
}

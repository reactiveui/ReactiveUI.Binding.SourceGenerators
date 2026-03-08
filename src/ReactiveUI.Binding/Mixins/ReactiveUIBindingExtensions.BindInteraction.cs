// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Extension methods for binding interactions between a view and a view model.
/// </summary>
public static partial class ReactiveUIBindingExtensions
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// Binds a task-based handler to an interaction exposed by the view model.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TInput">The type of the interaction's input.</typeparam>
    /// <typeparam name="TOutput">The type of the interaction's output.</typeparam>
    /// <param name="view">The view that provides the handler.</param>
    /// <param name="viewModel">The view model that exposes the interaction.</param>
    /// <param name="propertyName">An expression that selects the interaction property on the view model.</param>
    /// <param name="handler">A task-based handler for the interaction.</param>
    /// <param name="propertyNameExpression">The caller argument expression for <paramref name="propertyName"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindInteraction<TViewModel, TView, TInput, TOutput>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, IInteraction<TInput, TOutput>>> propertyName,
        Func<IInteractionContext<TInput, TOutput>, Task> handler,
        [CallerArgumentExpression("propertyName")] string propertyNameExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
#else
    /// <summary>
    /// Binds a task-based handler to an interaction exposed by the view model.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TInput">The type of the interaction's input.</typeparam>
    /// <typeparam name="TOutput">The type of the interaction's output.</typeparam>
    /// <param name="view">The view that provides the handler.</param>
    /// <param name="viewModel">The view model that exposes the interaction.</param>
    /// <param name="propertyName">An expression that selects the interaction property on the view model.</param>
    /// <param name="handler">A task-based handler for the interaction.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindInteraction<TViewModel, TView, TInput, TOutput>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, IInteraction<TInput, TOutput>>> propertyName,
        Func<IInteractionContext<TInput, TOutput>, Task> handler,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
#endif
    {
        throw new InvalidOperationException(NoGeneratedBindingMessage);
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Binds an observable-based handler to an interaction exposed by the view model.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TInput">The type of the interaction's input.</typeparam>
    /// <typeparam name="TOutput">The type of the interaction's output.</typeparam>
    /// <typeparam name="TDontCare">The signal type of the observable handler.</typeparam>
    /// <param name="view">The view that provides the handler.</param>
    /// <param name="viewModel">The view model that exposes the interaction.</param>
    /// <param name="propertyName">An expression that selects the interaction property on the view model.</param>
    /// <param name="handler">An observable-based handler for the interaction.</param>
    /// <param name="propertyNameExpression">The caller argument expression for <paramref name="propertyName"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindInteraction<TViewModel, TView, TInput, TOutput, TDontCare>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, IInteraction<TInput, TOutput>>> propertyName,
        Func<IInteractionContext<TInput, TOutput>, IObservable<TDontCare>> handler,
        [CallerArgumentExpression("propertyName")] string propertyNameExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
#else
    /// <summary>
    /// Binds an observable-based handler to an interaction exposed by the view model.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TInput">The type of the interaction's input.</typeparam>
    /// <typeparam name="TOutput">The type of the interaction's output.</typeparam>
    /// <typeparam name="TDontCare">The signal type of the observable handler.</typeparam>
    /// <param name="view">The view that provides the handler.</param>
    /// <param name="viewModel">The view model that exposes the interaction.</param>
    /// <param name="propertyName">An expression that selects the interaction property on the view model.</param>
    /// <param name="handler">An observable-based handler for the interaction.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindInteraction<TViewModel, TView, TInput, TOutput, TDontCare>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, IInteraction<TInput, TOutput>>> propertyName,
        Func<IInteractionContext<TInput, TOutput>, IObservable<TDontCare>> handler,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
#endif
    {
        throw new InvalidOperationException(NoGeneratedBindingMessage);
    }
}

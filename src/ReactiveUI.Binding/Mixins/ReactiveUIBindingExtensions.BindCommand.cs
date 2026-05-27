// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;

namespace ReactiveUI.Binding;

/// <summary>
/// Extension methods for binding commands from a view model to controls on a view.
/// </summary>
public static partial class ReactiveUIBindingExtensions
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// Binds a command from a view model to a control on a view.
    /// </summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TProp">The type of the command property.</typeparam>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model containing the command.</param>
    /// <param name="propertyName">An expression that selects the command property on the view model.</param>
    /// <param name="controlName">An expression that selects the control on the view.</param>
    /// <param name="toEvent">The event name to bind to. If null, a default event is selected.</param>
    /// <param name="propertyNameExpression">The caller argument expression for <paramref name="propertyName"/>. Auto-populated by the compiler.</param>
    /// <param name="controlNameExpression">The caller argument expression for <paramref name="controlName"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindCommand<TView, TViewModel, TProp, TControl>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp?>> propertyName,
        Expression<Func<TView, TControl>> controlName,
        string? toEvent = null,
        [CallerArgumentExpression("propertyName")] string propertyNameExpression = "",
        [CallerArgumentExpression("controlName")] string controlNameExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TView : class, IViewFor
        where TViewModel : class
        where TProp : ICommand
        where TControl : class
#else
    /// <summary>
    /// Binds a command from a view model to a control on a view.
    /// </summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TProp">The type of the command property.</typeparam>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model containing the command.</param>
    /// <param name="propertyName">An expression that selects the command property on the view model.</param>
    /// <param name="controlName">An expression that selects the control on the view.</param>
    /// <param name="toEvent">The event name to bind to. If null, a default event is selected.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindCommand<TView, TViewModel, TProp, TControl>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp?>> propertyName,
        Expression<Func<TView, TControl>> controlName,
        string? toEvent = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TView : class, IViewFor
        where TViewModel : class
        where TProp : ICommand
        where TControl : class
#endif
    {
        throw new InvalidOperationException(NoGeneratedBindingMessage);
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Binds a command from a view model to a control on a view with an observable parameter.
    /// </summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TProp">The type of the command property.</typeparam>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <typeparam name="TParam">The type of the command parameter.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model containing the command.</param>
    /// <param name="propertyName">An expression that selects the command property on the view model.</param>
    /// <param name="controlName">An expression that selects the control on the view.</param>
    /// <param name="withParameter">An observable that provides the command parameter.</param>
    /// <param name="toEvent">The event name to bind to. If null, a default event is selected.</param>
    /// <param name="propertyNameExpression">The caller argument expression for <paramref name="propertyName"/>. Auto-populated by the compiler.</param>
    /// <param name="controlNameExpression">The caller argument expression for <paramref name="controlName"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindCommand<TView, TViewModel, TProp, TControl, TParam>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp?>> propertyName,
        Expression<Func<TView, TControl>> controlName,
        IObservable<TParam?> withParameter,
        string? toEvent = null,
        [CallerArgumentExpression("propertyName")] string propertyNameExpression = "",
        [CallerArgumentExpression("controlName")] string controlNameExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TView : class, IViewFor
        where TViewModel : class
        where TProp : ICommand
        where TControl : class
#else
    /// <summary>
    /// Binds a command from a view model to a control on a view with an observable parameter.
    /// </summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TProp">The type of the command property.</typeparam>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <typeparam name="TParam">The type of the command parameter.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model containing the command.</param>
    /// <param name="propertyName">An expression that selects the command property on the view model.</param>
    /// <param name="controlName">An expression that selects the control on the view.</param>
    /// <param name="withParameter">An observable that provides the command parameter.</param>
    /// <param name="toEvent">The event name to bind to. If null, a default event is selected.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindCommand<TView, TViewModel, TProp, TControl, TParam>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp?>> propertyName,
        Expression<Func<TView, TControl>> controlName,
        IObservable<TParam?> withParameter,
        string? toEvent = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TView : class, IViewFor
        where TViewModel : class
        where TProp : ICommand
        where TControl : class
#endif
    {
        throw new InvalidOperationException(NoGeneratedBindingMessage);
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Binds a command from a view model to a control on a view with a parameter expression.
    /// </summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TProp">The type of the command property.</typeparam>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <typeparam name="TParam">The type of the command parameter.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model containing the command.</param>
    /// <param name="propertyName">An expression that selects the command property on the view model.</param>
    /// <param name="controlName">An expression that selects the control on the view.</param>
    /// <param name="withParameter">An expression that selects the command parameter property on the view model.</param>
    /// <param name="toEvent">The event name to bind to. If null, a default event is selected.</param>
    /// <param name="propertyNameExpression">The caller argument expression for <paramref name="propertyName"/>. Auto-populated by the compiler.</param>
    /// <param name="controlNameExpression">The caller argument expression for <paramref name="controlName"/>. Auto-populated by the compiler.</param>
    /// <param name="withParameterExpression">The caller argument expression for <paramref name="withParameter"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindCommand<TView, TViewModel, TProp, TControl, TParam>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp?>> propertyName,
        Expression<Func<TView, TControl>> controlName,
        Expression<Func<TViewModel, TParam?>> withParameter,
        string? toEvent = null,
        [CallerArgumentExpression("propertyName")] string propertyNameExpression = "",
        [CallerArgumentExpression("controlName")] string controlNameExpression = "",
        [CallerArgumentExpression("withParameter")] string withParameterExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TView : class, IViewFor
        where TViewModel : class
        where TProp : ICommand
        where TControl : class
#else
    /// <summary>
    /// Binds a command from a view model to a control on a view with a parameter expression.
    /// </summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TProp">The type of the command property.</typeparam>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <typeparam name="TParam">The type of the command parameter.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model containing the command.</param>
    /// <param name="propertyName">An expression that selects the command property on the view model.</param>
    /// <param name="controlName">An expression that selects the control on the view.</param>
    /// <param name="withParameter">An expression that selects the command parameter property on the view model.</param>
    /// <param name="toEvent">The event name to bind to. If null, a default event is selected.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindCommand<TView, TViewModel, TProp, TControl, TParam>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp?>> propertyName,
        Expression<Func<TView, TControl>> controlName,
        Expression<Func<TViewModel, TParam?>> withParameter,
        string? toEvent = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TView : class, IViewFor
        where TViewModel : class
        where TProp : ICommand
        where TControl : class
#endif
    {
        throw new InvalidOperationException(NoGeneratedBindingMessage);
    }
}

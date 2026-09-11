// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows.Input;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Resolves a BindCommand expression by reflection, for a call site the generator could not read.</summary>
/// <remarks>
/// The opt-in half of <c>BindCommand</c>. The unsuffixed overloads resolve at compile time and throw when no generated
/// dispatch claimed the call site; these walk the chain by reflection instead, and are annotated so a consumer
/// publishing trimmed or ahead-of-time sees the requirement at their own call site.
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Binds a command from a view model to a control on a view.</summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TProp">The type of the command property.</typeparam>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model containing the command.</param>
    /// <param name="propertyName">An expression that selects the command property on the view model.</param>
    /// <param name="controlName">An expression that selects the control on the view.</param>
    /// <param name="toEvent">The event name to bind to. If null, a default event is selected.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST2309", Justification = "the CallerInfo and toEvent defaults must stay optional; overloads would shadow the generated overloads")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindCommandUnsafe<TView, TViewModel, TProp, TControl>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp?>> propertyName,
        Expression<Func<TView, TControl>> controlName,
        string? toEvent)
        where TView : class, IViewFor
        where TViewModel : class
        where TProp : ICommand
        where TControl : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(propertyName.Body);

        return RuntimeCommandBindingFallback.BindCommand(
            view,
            viewModel,
            propertyName,
            controlName,
            Signal.Return<object?>(null),
            toEvent,
            bindingExpression);
    }

    /// <summary>Binds a command from a view model to a control on a view with an observable parameter.</summary>
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
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST2309", Justification = "the CallerInfo and toEvent defaults must stay optional; overloads would shadow the generated overloads")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindCommandUnsafe<TView, TViewModel, TProp, TControl, TParam>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp?>> propertyName,
        Expression<Func<TView, TControl>> controlName,
        IObservable<TParam?> withParameter,
        string? toEvent)
        where TView : class, IViewFor
        where TViewModel : class
        where TProp : ICommand
        where TControl : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(propertyName.Body);

        return RuntimeCommandBindingFallback.BindCommand(
            view,
            viewModel,
            propertyName,
            controlName,
            LinqExtensions.Map(withParameter, static value => (object?)value),
            toEvent,
            bindingExpression);
    }

    /// <summary>Binds a command from a view model to a control on a view with a parameter expression.</summary>
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
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST2309", Justification = "the CallerInfo and toEvent defaults must stay optional; overloads would shadow the generated overloads")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindCommandUnsafe<TView, TViewModel, TProp, TControl, TParam>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp?>> propertyName,
        Expression<Func<TView, TControl>> controlName,
        Expression<Func<TViewModel, TParam?>> withParameter,
        string? toEvent)
        where TView : class, IViewFor
        where TViewModel : class
        where TProp : ICommand
        where TControl : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(propertyName.Body);

        // The parameter property lives on the same view model, so it is only observable once there is one.
        var parameters = viewModel is null
            ? Signal.Never<object?>()
            : LinqExtensions.Map(
                RuntimeObservationFallback.WhenAnyValue(viewModel, withParameter),
                static value => (object?)value);

        return RuntimeCommandBindingFallback.BindCommand(
            view,
            viewModel,
            propertyName,
            controlName,
            parameters,
            toEvent,
            bindingExpression);
    }
}

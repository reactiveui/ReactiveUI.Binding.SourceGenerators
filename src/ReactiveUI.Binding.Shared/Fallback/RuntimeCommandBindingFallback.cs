// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>Binds a command to a control through the runtime expression engine.</summary>
/// <remarks>
/// Reached where the generator could not serve the call site - a view or view model it cannot name, or a selector
/// that is not an inline lambda. Which control mechanism carries the execution is the generated path's decision
/// too: both ask <c>CommandBinderService</c> for the binder with the highest affinity for the control, and
/// only how the command and the control are found differs.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RuntimeCommandBindingFallback
{
    /// <summary>Keeps the command an observed property holds bound to the control another one holds.</summary>
    /// <typeparam name="TView">The type declaring the control property.</typeparam>
    /// <typeparam name="TViewModel">The type declaring the command property.</typeparam>
    /// <typeparam name="TProp">The type of the command property.</typeparam>
    /// <typeparam name="TControl">The type of the control the command is bound to.</typeparam>
    /// <param name="view">The object declaring the control property.</param>
    /// <param name="viewModel">The object declaring the command property.</param>
    /// <param name="commandProperty">The property holding the command to bind.</param>
    /// <param name="controlProperty">The property holding the control to bind it to.</param>
    /// <param name="commandParameter">The values offered as the command parameter.</param>
    /// <param name="toEvent">The control event that executes the command, or null for the control's default.</param>
    /// <param name="bindingExpression">The bound expression, named when the observation faults.</param>
    /// <returns>A disposable that, when disposed, unbinds the command and stops observing.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="commandProperty"/> or <paramref name="controlProperty"/> is null.</exception>
    /// <remarks>
    /// Both sides are followed, so replacing either the command or the control rebinds and drops the binding it
    /// held before. A null view model holds no command property to observe, so nothing is bound rather than the
    /// call faulting - which is the ordinary state before a view model is assigned.
    /// </remarks>
    [RequiresUnreferencedCode("Runtime command binding resolves the property chain by reflection.")]
    public static IDisposable BindCommand<
        TView,
        TViewModel,
        TProp,
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.PublicEvents
            | DynamicallyAccessedMemberTypes.NonPublicEvents)]
        TControl>(
        TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp?>> commandProperty,
        Expression<Func<TView, TControl>> controlProperty,
        IObservable<object?> commandParameter,
        string? toEvent,
        string bindingExpression)
        where TView : class
        where TViewModel : class
        where TProp : ICommand
        where TControl : class
    {
        ArgumentExceptionHelper.ThrowIfNull(commandProperty);
        ArgumentExceptionHelper.ThrowIfNull(controlProperty);

        if (viewModel is null)
        {
            return EmptyDisposable.Instance;
        }

        var binding = new MutableDisposable();
        var commands = RuntimeObservationFallback.WhenAnyValue(viewModel, commandProperty);
        var controls = RuntimeObservationFallback.WhenAnyValue(view, controlProperty);

        var observation = BindingErrors.Subscribe(
            LinqExtensions.CombineLatest(commands, controls, static (command, control) => (command, control)),
            pair => binding.Disposable = Bind(pair.command, pair.control, commandParameter, toEvent),
            bindingExpression);

        return new MultipleDisposable(observation, binding);
    }

    /// <summary>Hands one command and one control to the binder with the highest affinity for that control.</summary>
    /// <typeparam name="TProp">The type of the command.</typeparam>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <param name="command">The command to execute, or null while the property holds none.</param>
    /// <param name="control">The control that executes it, or null while the property holds none.</param>
    /// <param name="commandParameter">The values offered as the command parameter.</param>
    /// <param name="toEvent">The control event that executes the command, or null for the control's default.</param>
    /// <returns>The binding, or an empty disposable when there is nothing to bind.</returns>
    [RequiresUnreferencedCode("Runtime command binding resolves the control's event by reflection.")]
    private static IDisposable Bind<
        TProp,
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.PublicEvents
            | DynamicallyAccessedMemberTypes.NonPublicEvents)]
        TControl>(
        TProp? command,
        TControl? control,
        IObservable<object?> commandParameter,
        string? toEvent)
        where TProp : ICommand
        where TControl : class
    {
        if (command is null || control is null)
        {
            return EmptyDisposable.Instance;
        }

        var binder = CommandBinding.CommandBinderService.GetBinder<TControl>(toEvent is not null);

        var bound = toEvent is null
            ? binder?.BindCommandToObject(command, control, commandParameter)
            : binder?.BindCommandToObject<TControl, EventArgs>(command, control, commandParameter, toEvent);

        return bound ?? EmptyDisposable.Instance;
    }
}

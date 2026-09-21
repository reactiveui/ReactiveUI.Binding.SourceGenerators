// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;
using Microsoft.Maui.Controls;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Mechanisms;

/// <summary>
/// Runs a command when a <see cref="Button"/> raises <see cref="Button.Clicked"/>. The binder passes the latest
/// command parameter to the command, enables the button only while the command can run, and never touches the
/// button's own <see cref="Button.Command"/> property. It prints a line each time it attaches to a button.
/// </summary>
public sealed class ClickCommandBinder : ICreatesCommandBinding
{
    /// <inheritdoc/>
    public int GetAffinityForObject<T>(bool hasEventTarget) =>
        typeof(Button).IsAssignableFrom(typeof(T)) ? BindingAffinity.ExactType : 0;

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T>(ICommand? command, T? target, IObservable<object?> commandParameter)
        where T : class =>
        AttachToClick(command, target, commandParameter);

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T, TEventArgs>(ICommand? command, T? target, IObservable<object?> commandParameter, string eventName)
        where T : class
    {
        if (eventName != nameof(Button.Clicked))
        {
            return null;
        }

        Console.WriteLine($"The click binder is asked for the {eventName} event, which carries {typeof(TEventArgs).Name}");
        return AttachToClick(command, target, commandParameter);
    }

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T, TEventArgs>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter,
        Action<EventHandler<TEventArgs>> addHandler,
        Action<EventHandler<TEventArgs>> removeHandler)
        where T : class
        where TEventArgs : EventArgs
    {
        if (command is null || target is not Button button)
        {
            return null;
        }

        Console.WriteLine($"The click binder is attached to the {button.Text} button");

        ClickBinding binding = new(button, command, commandParameter);
        EventHandler<TEventArgs> handler = binding.OnClick;
        addHandler(handler);
        binding.WhenDisposed(() => removeHandler(handler));
        return binding;
    }

    /// <summary>Attaches a command to the click of a button.</summary>
    /// <param name="command">The command to run.</param>
    /// <param name="target">The object to attach to; only a button is accepted.</param>
    /// <param name="commandParameter">The stream of parameters the command receives.</param>
    /// <returns>The attachment, or <see langword="null"/> when there is no command or the target is not a button.</returns>
    private static ClickBinding? AttachToClick(ICommand? command, object? target, IObservable<object?> commandParameter)
    {
        if (command is null || target is not Button button)
        {
            return null;
        }

        Console.WriteLine($"The click binder is attached to the {button.Text} button");

        ClickBinding binding = new(button, command, commandParameter);
        button.Clicked += binding.OnClick;
        binding.WhenDisposed(() => button.Clicked -= binding.OnClick);
        return binding;
    }

    /// <summary>The link between a button click and a command; disposing it detaches both.</summary>
    [System.Diagnostics.DebuggerDisplay("Parameter = {_parameter}")]
    private sealed class ClickBinding : IDisposable
    {
        /// <summary>The button whose enabled state follows the command.</summary>
        private readonly Button _button;

        /// <summary>The command to run.</summary>
        private readonly ICommand _command;

        /// <summary>The subscription to the parameter stream.</summary>
        private readonly IDisposable _parameterSubscription;

        /// <summary>Whether the button was enabled before the binding.</summary>
        private readonly bool _wasEnabled;

        /// <summary>Detaches the click handler.</summary>
        private Action? _detach;

        /// <summary>The latest parameter the stream produced.</summary>
        private object? _parameter;

        /// <summary>Initializes a new instance of the <see cref="ClickBinding"/> class.</summary>
        /// <param name="button">The button whose enabled state follows the command.</param>
        /// <param name="command">The command to run.</param>
        /// <param name="parameters">The stream of parameters the command receives.</param>
        public ClickBinding(Button button, ICommand command, IObservable<object?> parameters)
        {
            _button = button;
            _command = command;
            _wasEnabled = button.IsEnabled;
            _parameterSubscription = parameters.Subscribe(OnParameter);
            command.CanExecuteChanged += OnCanExecuteChanged;
            RefreshEnabled();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _detach?.Invoke();
            _command.CanExecuteChanged -= OnCanExecuteChanged;
            _parameterSubscription.Dispose();
            _button.IsEnabled = _wasEnabled;
        }

        /// <summary>Sets what to do to detach the click handler when the binding is disposed.</summary>
        /// <param name="detach">Detaches the click handler.</param>
        public void WhenDisposed(Action detach) => _detach = detach;

        /// <summary>Runs the command with the latest parameter when it can run.</summary>
        /// <param name="sender">The button.</param>
        /// <param name="e">The event data.</param>
        public void OnClick(object? sender, EventArgs e)
        {
            if (_command.CanExecute(_parameter))
            {
                _command.Execute(_parameter);
            }
        }

        /// <summary>Keeps the latest parameter and asks the command about it again.</summary>
        /// <param name="parameter">The new parameter.</param>
        private void OnParameter(object? parameter)
        {
            _parameter = parameter;
            RefreshEnabled();
        }

        /// <summary>Asks the command again whether it can run.</summary>
        /// <param name="sender">The command.</param>
        /// <param name="e">The event data.</param>
        private void OnCanExecuteChanged(object? sender, EventArgs e) => RefreshEnabled();

        /// <summary>Enables the button only while the command can run with the latest parameter.</summary>
        private void RefreshEnabled() => _button.IsEnabled = _command.CanExecute(_parameter);
    }
}

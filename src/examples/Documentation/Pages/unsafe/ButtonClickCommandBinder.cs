// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;
using Microsoft.Maui.Controls;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>Runs a command each time a <see cref="Button"/> is clicked. The runtime path finds a control's binder here.</summary>
public sealed class ButtonClickCommandBinder : ICreatesCommandBinding
{
    /// <summary>The score that makes this binder win over any other binder for a button.</summary>
    private const int ButtonAffinity = 100;

    /// <summary>The name of the only event this binder understands.</summary>
    private const string ClickedEventName = nameof(Button.Clicked);

    /// <inheritdoc/>
    public int GetAffinityForObject<T>(bool hasEventTarget) => typeof(T) == typeof(Button) ? ButtonAffinity : 0;

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T>(ICommand? command, T? target, IObservable<object?> commandParameter)
        where T : class => Attach(command, target as Button, commandParameter);

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T, TEventArgs>(ICommand? command, T? target, IObservable<object?> commandParameter, string eventName)
        where T : class =>
        eventName == ClickedEventName
            ? Attach(command, target as Button, commandParameter)
            : throw new NotSupportedException($"A button has no {eventName} event ({typeof(TEventArgs).Name}).");

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T, TEventArgs>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter,
        Action<EventHandler<TEventArgs>> addHandler,
        Action<EventHandler<TEventArgs>> removeHandler)
        where T : class
        where TEventArgs : EventArgs => null;

    /// <summary>Runs the command with the latest parameter each time the button is clicked.</summary>
    /// <param name="command">The command to run.</param>
    /// <param name="button">The button to follow.</param>
    /// <param name="commandParameter">The values offered as the command parameter.</param>
    /// <returns>A disposable that stops following the button, or <see langword="null"/> when there is nothing to attach.</returns>
    private static ClickAttachment? Attach(ICommand? command, Button? button, IObservable<object?> commandParameter) =>
        command is null || button is null ? null : new ClickAttachment(command, button, commandParameter);

    /// <summary>The link between one button and one command.</summary>
    [System.Diagnostics.DebuggerDisplay("Parameter = {_parameter}")]
    private sealed class ClickAttachment : IDisposable
    {
        /// <summary>The command a click runs.</summary>
        private readonly ICommand _command;

        /// <summary>The button being followed.</summary>
        private readonly Button _button;

        /// <summary>The subscription that keeps the latest parameter.</summary>
        private readonly IDisposable _parameterSubscription;

        /// <summary>The parameter the next click passes to the command.</summary>
        private object? _parameter;

        /// <summary>Initializes a new instance of the <see cref="ClickAttachment"/> class.</summary>
        /// <param name="command">The command a click runs.</param>
        /// <param name="button">The button to follow.</param>
        /// <param name="commandParameter">The values offered as the command parameter.</param>
        public ClickAttachment(ICommand command, Button button, IObservable<object?> commandParameter)
        {
            _command = command;
            _button = button;
            _parameterSubscription = commandParameter.Subscribe(parameter => _parameter = parameter);
            _button.Clicked += OnClicked;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _button.Clicked -= OnClicked;
            _parameterSubscription.Dispose();
        }

        /// <summary>Runs the command when it can run.</summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">The event data.</param>
        private void OnClicked(object? sender, EventArgs e)
        {
            if (_command.CanExecute(_parameter))
            {
                _command.Execute(_parameter);
            }
        }
    }
}

// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Indicates that an interaction has gone unhandled.
/// </summary>
/// <typeparam name="TInput">The type of the interaction's input.</typeparam>
/// <typeparam name="TOutput">The type of the interaction's output.</typeparam>
public class UnhandledInteractionException<TInput, TOutput> : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnhandledInteractionException{TInput, TOutput}"/> class.
    /// </summary>
    /// <param name="interaction">The interaction that doesn't have an input handler.</param>
    /// <param name="input">The input into the interaction.</param>
    public UnhandledInteractionException(Interaction<TInput, TOutput> interaction, TInput input)
        : this("Failed to find a registration for an Interaction.")
    {
        Interaction = interaction;
        Input = input;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnhandledInteractionException{TInput, TOutput}"/> class.
    /// </summary>
    public UnhandledInteractionException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnhandledInteractionException{TInput, TOutput}"/> class.
    /// </summary>
    /// <param name="message">A message about the exception.</param>
    public UnhandledInteractionException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnhandledInteractionException{TInput, TOutput}"/> class.
    /// </summary>
    /// <param name="message">A message about the exception.</param>
    /// <param name="innerException">Any other exception that caused the issue.</param>
    public UnhandledInteractionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets the interaction that was not handled.
    /// </summary>
    public Interaction<TInput, TOutput>? Interaction { get; }

    /// <summary>
    /// Gets the input for the interaction that was not handled.
    /// </summary>
    public TInput Input { get; } = default!;
}

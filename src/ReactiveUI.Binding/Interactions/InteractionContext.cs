// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Contains contextual information for an interaction.
/// </summary>
/// <remarks>
/// <para>
/// Instances of this class are passed into interaction handlers. The <see cref="Input"/> property exposes
/// the input to the interaction, whilst the <see cref="SetOutput"/> method allows a handler to provide the
/// output.
/// </para>
/// <para>
/// Calling <see cref="SetOutput"/> more than once throws an <see cref="InvalidOperationException"/>, ensuring the
/// handler's reply remains deterministic even when multiple handlers run concurrently. Use <see cref="IsHandled"/>
/// to guard logic that should only execute once.
/// </para>
/// </remarks>
/// <typeparam name="TInput">The type of the interaction's input.</typeparam>
/// <typeparam name="TOutput">The type of the interaction's output.</typeparam>
public sealed class InteractionContext<TInput, TOutput> : IOutputContext<TInput, TOutput>
{
    /// <summary>
    /// The output value set by a handler via <see cref="SetOutput"/>.
    /// </summary>
    private TOutput _output = default!;

    /// <summary>
    /// Tracks whether the output has been set (0 = not set, 1 = set).
    /// </summary>
    private int _outputSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionContext{TInput, TOutput}"/> class.
    /// </summary>
    /// <param name="input">The input value for this interaction.</param>
    internal InteractionContext(TInput input) => Input = input;

    /// <inheritdoc />
    public TInput Input { get; }

    /// <inheritdoc />
    public bool IsHandled => _outputSet == 1;

    /// <inheritdoc />
    public void SetOutput(TOutput output)
    {
        if (!TryClaimOutput())
        {
            throw new InvalidOperationException("Output has already been set.");
        }

        _output = output;
    }

    /// <inheritdoc />
    public TOutput GetOutput()
    {
        if (_outputSet == 0)
        {
            throw new InvalidOperationException("Output has not been set.");
        }

        return _output;
    }

    /// <summary>
    /// Atomically claims the output slot.
    /// </summary>
    /// <returns><see langword="true"/> if the claim was successful; <see langword="false"/> if already set.</returns>
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryClaimOutput() => Interlocked.CompareExchange(ref _outputSet, 1, 0) == 0;
}

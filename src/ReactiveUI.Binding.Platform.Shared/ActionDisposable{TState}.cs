// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>
/// Runs a teardown action against a state value exactly once. Carrying the state lets the platform
/// observers unhook with a non-capturing lambda, so no closure is allocated per subscription.
/// </summary>
/// <remarks>Each platform assembly compiles its own internal copy; nothing here touches the seam.</remarks>
/// <typeparam name="TState">The type of the state handed to the teardown action.</typeparam>
internal sealed class ActionDisposable<TState> : IDisposable
{
    /// <summary>The state handed to <see cref="_action"/> on disposal.</summary>
    private readonly TState _state;

    /// <summary>The teardown action.</summary>
    private readonly Action<TState> _action;

    /// <summary>Guard flag to ensure disposal occurs exactly once (0 = not disposed, 1 = disposed).</summary>
    private int _disposed;

    /// <summary>Initializes a new instance of the <see cref="ActionDisposable{TState}"/> class.</summary>
    /// <param name="state">The state to hand to <paramref name="action"/> on disposal.</param>
    /// <param name="action">The teardown action.</param>
    public ActionDisposable(TState state, Action<TState> action)
    {
        ArgumentExceptionHelper.ThrowIfNull(action);
        _state = state;
        _action = action;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _action(_state);
    }
}

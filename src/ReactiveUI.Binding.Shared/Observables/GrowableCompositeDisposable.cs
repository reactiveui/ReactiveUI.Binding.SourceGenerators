// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>
/// A composite disposable that accepts resources after construction. Additions made once the
/// composite is already disposed are disposed immediately rather than retained, so a producer
/// racing disposal cannot leak the resource it was mid-way through handing over.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GrowableCompositeDisposable : IDisposable
{
    /// <summary>Guards <see cref="_disposables"/> against concurrent additions and disposal.</summary>
    private readonly Lock _gate = new();

    /// <summary>The retained resources, or <see langword="null"/> once disposal has run.</summary>
    private List<IDisposable>? _disposables = [];

    /// <summary>Adds a resource, disposing it immediately if this composite is already disposed.</summary>
    /// <param name="disposable">The resource to take ownership of.</param>
    public void Add(IDisposable disposable)
    {
        ArgumentExceptionHelper.ThrowIfNull(disposable);

        lock (_gate)
        {
            if (_disposables is not null)
            {
                _disposables.Add(disposable);
                return;
            }
        }

        disposable.Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        List<IDisposable>? toDispose;

        lock (_gate)
        {
            toDispose = _disposables;
            _disposables = null;
        }

        if (toDispose is null)
        {
            return;
        }

        for (var i = 0; i < toDispose.Count; i++)
        {
            toDispose[i].Dispose();
        }
    }
}

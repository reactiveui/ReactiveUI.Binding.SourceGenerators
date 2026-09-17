// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Maui;
#else
namespace ReactiveUI.Binding.Maui;
#endif

/// <summary>Writes to a MAUI object through the dispatcher it carries.</summary>
public sealed class DispatcherViewThreadInvoker : IViewThreadInvoker
{
    /// <inheritdoc/>
    public bool Claims(object target) => target is BindableObject;

    /// <inheritdoc/>
    public bool CheckAccess(object target) => FindDispatcher((BindableObject)target) is not { IsDispatchRequired: true };

    /// <inheritdoc/>
    public void Post(object target, Action<object?> callback, object? state)
    {
        ArgumentExceptionHelper.ThrowIfNull(callback);

        var dispatcher = FindDispatcher((BindableObject)target);
        if (dispatcher is null)
        {
            callback(state);
            return;
        }

        _ = dispatcher.Dispatch(() => callback(state));
    }

    /// <summary>Reads the dispatcher an object carries.</summary>
    /// <param name="owner">The object to read.</param>
    /// <returns>The dispatcher, or null when neither the object, its thread nor the application has one.</returns>
    private static IDispatcher? FindDispatcher(BindableObject owner)
    {
        try
        {
            return owner.Dispatcher;
        }
        catch (InvalidOperationException)
        {
            // MAUI throws rather than answering null when it finds no dispatcher, as in a view's unit test.
            return null;
        }
    }
}

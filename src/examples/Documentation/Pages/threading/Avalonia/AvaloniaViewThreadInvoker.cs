// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Threading;
using Dispatcher = Avalonia.Threading.Dispatcher;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>Tells the binding engine that Avalonia's UI thread owns every Avalonia object.</summary>
public sealed class AvaloniaViewThreadInvoker : IViewThreadInvoker
{
    /// <inheritdoc/>
    public bool Claims(object target) => target is AvaloniaObject;

    /// <inheritdoc/>
    public bool CheckAccess(object target) => Dispatcher.UIThread.CheckAccess();

    /// <inheritdoc/>
    public void Post(object target, Action<object?> callback, object? state) =>
        Dispatcher.UIThread.Post(callback.Invoke, state, DispatcherPriority.Default);
}

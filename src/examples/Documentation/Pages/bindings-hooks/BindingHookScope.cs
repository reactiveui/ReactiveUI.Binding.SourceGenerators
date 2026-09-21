// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat;

namespace ReactiveUI.Binding.Documentation.BindingsHooks;

/// <summary>Keeps hooks registered until it is disposed, then leaves no hook registered.</summary>
[System.Diagnostics.DebuggerDisplay("BindingHookScope")]
public sealed class BindingHookScope : IDisposable
{
    /// <summary>Initializes a new instance of the <see cref="BindingHookScope"/> class.</summary>
    private BindingHookScope()
    {
    }

    /// <summary>Registers hooks with the service locator and tells <see cref="BindingHooks"/> to read the registrations again.</summary>
    /// <param name="hooks">The hooks to register, in the order they are asked.</param>
    /// <returns>A scope that removes the hooks when disposed.</returns>
    public static BindingHookScope Register(params IPropertyBindingHook[] hooks)
    {
        foreach (var hook in hooks)
        {
            AppLocator.CurrentMutable.RegisterConstant(hook);
        }

        BindingHooks.Refresh();
        return new();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        AppLocator.CurrentMutable.UnregisterAll<IPropertyBindingHook>();
        BindingHooks.Refresh();
    }
}

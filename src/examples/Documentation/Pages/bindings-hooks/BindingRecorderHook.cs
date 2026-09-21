// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.BindingsHooks;

/// <summary>Writes down every binding that is created and lets each one through.</summary>
[System.Diagnostics.DebuggerDisplay("Recorded = {Recorded.Count}")]
public sealed class BindingRecorderHook : IPropertyBindingHook
{
    /// <summary>Gets the bindings seen so far, each as its direction, its source type and its target type.</summary>
    public List<string> Recorded { get; } = [];

    /// <inheritdoc/>
    public bool ExecuteHook(
        object? source,
        object target,
        Func<IObservedChange<object, object>[]> getCurrentViewModelProperties,
        Func<IObservedChange<object, object>[]> getCurrentViewProperties,
        BindingDirection direction)
    {
        var sourceType = getCurrentViewModelProperties()[0].Sender.GetType().Name;
        var targetType = getCurrentViewProperties()[0].Sender.GetType().Name;

        Recorded.Add($"{direction}: {sourceType} -> {targetType}");
        return true;
    }
}

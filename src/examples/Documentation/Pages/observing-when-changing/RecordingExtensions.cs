// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.ObservingWhenChanging;

/// <summary>Records what a stream delivers.</summary>
public static class RecordingExtensions
{
    /// <summary>Adds recording to every stream.</summary>
    /// <typeparam name="T">The type of the delivered values.</typeparam>
    /// <param name="source">The stream to record.</param>
    extension<T>(IObservable<T> source)
    {
        /// <summary>Subscribes to the stream and keeps every value it delivers.</summary>
        /// <returns>The recording; dispose it to unsubscribe.</returns>
        public Recording<T> Record() => new(source);
    }
}

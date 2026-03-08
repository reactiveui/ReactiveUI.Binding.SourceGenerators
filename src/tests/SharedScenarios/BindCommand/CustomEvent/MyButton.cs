// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace SharedScenarios.BindCommand.CustomEvent
{
    /// <summary>
    /// A button control with a custom MouseUp event.
    /// </summary>
    public class MyButton
    {
        /// <summary>
        /// Occurs when the mouse button is released.
        /// </summary>
        public event EventHandler? MouseUp;

        /// <summary>
        /// Simulates a mouse up event.
        /// </summary>
        public void PerformMouseUp() => MouseUp?.Invoke(this, EventArgs.Empty);
    }
}

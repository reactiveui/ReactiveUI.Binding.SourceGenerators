// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace SharedScenarios.BindCommand.DeepCommandPath
{
    /// <summary>
    /// A simple button control with a Click event.
    /// </summary>
    public class MyButton
    {
        /// <summary>
        /// Occurs when the button is clicked.
        /// </summary>
        public event EventHandler? Click;

        /// <summary>
        /// Simulates a button click.
        /// </summary>
        public void PerformClick() => Click?.Invoke(this, EventArgs.Empty);
    }
}

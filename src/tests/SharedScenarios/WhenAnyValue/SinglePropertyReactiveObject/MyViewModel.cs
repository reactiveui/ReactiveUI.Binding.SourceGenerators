// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

using ReactiveUI;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyValue.SinglePropertyReactiveObject
{
    /// <summary>
    /// ViewModel extending ReactiveObject with a single string property.
    /// </summary>
    public class MyViewModel : ReactiveObject
    {
        /// <summary>
        /// The backing field for <see cref="Name"/>.
        /// </summary>
        private string _name = string.Empty;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
    }
}

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

namespace ReactiveUI.Binding.SourceGenerators.Plugins;

/// <summary>Declares a native bridge required by a selected command mechanism.</summary>
internal interface ICommandBindingHelperPlugin : ICommandBindingPlugin
{
    /// <summary>Emits the bridge once for all call sites selecting this plugin.</summary>
    /// <param name="sb">The output builder.</param>
    void EmitHelper(StringBuilder sb);
}

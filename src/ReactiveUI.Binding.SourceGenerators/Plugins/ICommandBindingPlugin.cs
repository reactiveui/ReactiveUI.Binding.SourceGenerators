// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins;

/// <summary>
/// Compile-time plugin interface for command binding code generation.
/// Implementations determine how a command is bound to a control
/// (via Command property, event + Enabled, or event only).
/// </summary>
internal interface ICommandBindingPlugin
{
    /// <summary>
    /// Gets the affinity score for this plugin.
    /// Higher values win when multiple plugins can handle the same invocation.
    /// </summary>
    int Affinity { get; }

    /// <summary>
    /// Determines whether this plugin can handle the given BindCommand invocation.
    /// </summary>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <returns>True if this plugin can generate binding code for this invocation.</returns>
    bool CanHandle(BindCommandInvocationInfo inv);

    /// <summary>
    /// Emits the command binding code into the string builder.
    /// </summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain expression.</param>
    void EmitBinding(StringBuilder sb, BindCommandInvocationInfo inv, string controlAccess);
}

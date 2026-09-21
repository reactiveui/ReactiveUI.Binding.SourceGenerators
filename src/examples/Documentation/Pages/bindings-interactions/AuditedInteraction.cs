// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.BindingsInteractions;

/// <summary>An interaction that keeps an audit trail of each question and answer, as a bank does for an approval.</summary>
/// <typeparam name="TInput">The type of the interaction's input.</typeparam>
/// <typeparam name="TOutput">The type of the interaction's output.</typeparam>
[System.Diagnostics.DebuggerDisplay("Audit = {Audit.Count}")]
public sealed class AuditedInteraction<TInput, TOutput> : Interaction<TInput, TOutput>
{
    /// <summary>Gets the questions asked and the answers given, in order.</summary>
    public List<string> Audit { get; } = [];

    /// <inheritdoc/>
    public override async Task<TOutput> Handle(TInput input)
    {
        Audit.Add($"ask {input} of {GetHandlers().Length} handlers");
        return await base.Handle(input).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override IOutputContext<TInput, TOutput> GenerateContext(TInput input) => new AuditingOutputContext<TInput, TOutput>(base.GenerateContext(input), Audit);
}

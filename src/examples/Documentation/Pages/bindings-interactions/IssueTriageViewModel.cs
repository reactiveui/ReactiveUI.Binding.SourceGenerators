// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.BindingsInteractions;

/// <summary>
/// The triage panel of the issue board. It exposes the board's close confirmation through the
/// <see cref="IInteraction{TInput, TOutput}"/> interface, so the panel's view never sees the concrete interaction.
/// </summary>
/// <param name="board">The issue board the panel triages.</param>
[System.Diagnostics.DebuggerDisplay("Board = {Board}")]
public sealed class IssueTriageViewModel(IssueBoardViewModel board) : ObservableObject
{
    /// <summary>Gets the issue board the panel triages.</summary>
    public IssueBoardViewModel Board { get; } = board;

    /// <summary>Gets the question the view answers before the board closes an issue.</summary>
    public IInteraction<Issue, bool> ConfirmClose { get; } = board.ConfirmClose;
}

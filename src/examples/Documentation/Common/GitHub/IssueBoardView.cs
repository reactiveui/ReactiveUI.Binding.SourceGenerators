// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.GitHub;

/// <summary>
/// The issue board screen: a sign-in strip, a repository list, an issue list and a detail area for the selected
/// issue. A real UI framework builds these controls from markup; here the view creates them in code.
/// </summary>
[System.Diagnostics.DebuggerDisplay("ViewModel = {ViewModel}")]
public sealed class IssueBoardView : ObservableObject, IViewFor<IssueBoardViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public IssueBoardViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the box where the user types the access token.</summary>
    public TextBoxControl TokenTextBox { get; } = new() { Placeholder = "Access token" };

    /// <summary>Gets the button that signs in.</summary>
    public ButtonControl SignInButton { get; } = new() { Content = "Sign in" };

    /// <summary>Gets the button that signs out.</summary>
    public ButtonControl SignOutButton { get; } = new() { Content = "Sign out" };

    /// <summary>Gets the label that shows who is signed in.</summary>
    public LabelControl UserLabel { get; } = new();

    /// <summary>Gets the label that shows how many requests are left in the quota.</summary>
    public LabelControl RateLimitLabel { get; } = new();

    /// <summary>Gets the label that shows the last error.</summary>
    public LabelControl ErrorLabel { get; } = new();

    /// <summary>Gets the list of repositories.</summary>
    public ItemsListControl<Repository> RepositoryList { get; } = new();

    /// <summary>Gets the button that loads the issues of the selected repository.</summary>
    public ButtonControl LoadIssuesButton { get; } = new() { Content = "Load issues" };

    /// <summary>Gets the list of open issues.</summary>
    public ItemsListControl<Issue> IssueList { get; } = new();

    /// <summary>Gets the label that shows the title of the selected issue.</summary>
    public LabelControl IssueTitleLabel { get; } = new();

    /// <summary>Gets the label that shows whether the selected issue is open or closed.</summary>
    public LabelControl StateLabel { get; } = new();

    /// <summary>Gets the label that shows the account name of the selected issue's assignee.</summary>
    public LabelControl AssigneeLabel { get; } = new();

    /// <summary>Gets the label that shows the display name of the selected issue's author.</summary>
    public LabelControl AuthorLabel { get; } = new();

    /// <summary>Gets the button that closes the selected issue.</summary>
    public ButtonControl CloseIssueButton { get; } = new() { Content = "Close issue" };

    /// <summary>Gets the box where the user writes a comment.</summary>
    public TextBoxControl CommentTextBox { get; } = new() { Placeholder = "Leave a comment" };

    /// <summary>Gets the button that posts the comment.</summary>
    public ButtonControl AddCommentButton { get; } = new() { Content = "Comment" };

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IssueBoardViewModel?)value;
    }
}

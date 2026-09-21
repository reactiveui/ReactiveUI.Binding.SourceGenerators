// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Net;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.GitHub;

/// <summary>
/// The view model behind the issue board. It signs in, lists repositories and their open issues, and closes or
/// comments on the selected issue. Each command starts its work and returns; wait for it through the command's
/// <see cref="AsyncDelegateCommand.Completion"/>. A failed request never throws from a command. It sets
/// <see cref="ErrorMessage"/>, and a 401 signs the user out.
/// </summary>
[System.Diagnostics.DebuggerDisplay("IsSignedIn = {IsSignedIn}, Issues = {Issues.Count}")]
public sealed class IssueBoardViewModel : ObservableObject
{
    /// <summary>The server the view model calls.</summary>
    private readonly IGitHubApi _api;

    /// <summary>Initializes a new instance of the <see cref="IssueBoardViewModel"/> class.</summary>
    /// <param name="api">The server to call.</param>
    public IssueBoardViewModel(IGitHubApi api)
    {
        _api = api;
        RateLimitRemaining = api.RateLimitRemaining;
        SignInCommand = new(_ => SignInAsync(), _ => !IsSignedIn && !string.IsNullOrWhiteSpace(Token));
        SignOutCommand = new(_ => SignOut(), _ => IsSignedIn);
        LoadIssuesCommand = new(_ => LoadIssuesAsync(), _ => IsSignedIn && SelectedRepository is not null);
        CloseIssueCommand = new(_ => CloseIssueAsync(), _ => SelectedIssue is { State: IssueState.Open });
        AddCommentCommand = new(_ => AddCommentAsync(), _ => SelectedIssue is not null && !string.IsNullOrWhiteSpace(NewCommentText));
    }

    /// <summary>Gets the question the view answers before an issue closes. The input is the issue; the answer is <see langword="true"/> to close it.</summary>
    public Interaction<Issue, bool> ConfirmClose { get; } = new();

    /// <summary>Gets the command that signs in with <see cref="Token"/> and then loads the repositories.</summary>
    public AsyncDelegateCommand SignInCommand { get; }

    /// <summary>Gets the command that signs out and clears the board.</summary>
    public DelegateCommand SignOutCommand { get; }

    /// <summary>Gets the command that loads the open issues of <see cref="SelectedRepository"/>.</summary>
    public AsyncDelegateCommand LoadIssuesCommand { get; }

    /// <summary>Gets the command that closes <see cref="SelectedIssue"/> once <see cref="ConfirmClose"/> answers <see langword="true"/>.</summary>
    public AsyncDelegateCommand CloseIssueCommand { get; }

    /// <summary>Gets the command that posts <see cref="NewCommentText"/> on <see cref="SelectedIssue"/>.</summary>
    public AsyncDelegateCommand AddCommentCommand { get; }

    /// <summary>Gets or sets the access token the user typed.</summary>
    public string Token
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                SignInCommand.RaiseCanExecuteChanged();
            }
        }
    } = string.Empty;

    /// <summary>Gets a value indicating whether the server accepted the token.</summary>
    public bool IsSignedIn
    {
        get;
        private set
        {
            if (!SetProperty(ref field, value))
            {
                return;
            }

            SignInCommand.RaiseCanExecuteChanged();
            SignOutCommand.RaiseCanExecuteChanged();
            LoadIssuesCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>Gets the signed-in account, or <see langword="null"/> when nobody is signed in.</summary>
    public User? CurrentUser
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the repositories the account can see.</summary>
    public IReadOnlyList<Repository> Repositories
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets the repository whose issues the board shows.</summary>
    public Repository? SelectedRepository
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                LoadIssuesCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>Gets the open issues of <see cref="SelectedRepository"/>.</summary>
    public IReadOnlyList<Issue> Issues
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets the issue the user picked. A path such as <c>SelectedIssue.Assignee.Login</c> is empty while nothing is picked or the issue has no assignee.</summary>
    public Issue? SelectedIssue
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
            {
                return;
            }

            CloseIssueCommand.RaiseCanExecuteChanged();
            AddCommentCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>Gets or sets the text of the comment the user is writing.</summary>
    public string NewCommentText
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                AddCommentCommand.RaiseCanExecuteChanged();
            }
        }
    } = string.Empty;

    /// <summary>Gets how many requests the account may still make before the quota renews.</summary>
    public int RateLimitRemaining
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the message from the last failed request, or an empty string.</summary>
    public string ErrorMessage
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Signs in and loads the repositories.</summary>
    /// <returns>A task that completes when the repositories are loaded or the request failed.</returns>
    private async Task SignInAsync()
    {
        try
        {
            var user = await _api.SignInAsync(Token).ConfigureAwait(false);
            CurrentUser = user;
            IsSignedIn = true;
            ErrorMessage = string.Empty;
            Repositories = await _api.ListRepositoriesAsync().ConfigureAwait(false);
        }
        catch (GitHubApiException ex)
        {
            Fail(ex);
        }
        finally
        {
            RateLimitRemaining = _api.RateLimitRemaining;
        }
    }

    /// <summary>Signs out and clears everything the account could see.</summary>
    private void SignOut()
    {
        _api.SignOut();
        Reset();
    }

    /// <summary>Loads the open issues of the selected repository.</summary>
    /// <returns>A task that completes when the issues are loaded or the request failed.</returns>
    private async Task LoadIssuesAsync()
    {
        if (SelectedRepository is not { } repository)
        {
            return;
        }

        try
        {
            var issues = await _api.ListIssuesAsync(repository.FullName, IssueState.Open).ConfigureAwait(false);
            ErrorMessage = string.Empty;
            Issues = issues;
            SelectedIssue = null;
        }
        catch (GitHubApiException ex)
        {
            Fail(ex);
        }
        finally
        {
            RateLimitRemaining = _api.RateLimitRemaining;
        }
    }

    /// <summary>Asks the view to confirm, then closes the selected issue.</summary>
    /// <returns>A task that completes when the issue is closed, the user declined or the request failed.</returns>
    private async Task CloseIssueAsync()
    {
        if (SelectedIssue is not { } issue || SelectedRepository is not { } repository)
        {
            return;
        }

        if (!await ConfirmClose.Handle(issue).ConfigureAwait(false))
        {
            return;
        }

        try
        {
            var closed = await _api.CloseIssueAsync(repository.FullName, issue.Number).ConfigureAwait(false);
            ErrorMessage = string.Empty;
            issue.UpdateFrom(closed);
            CloseIssueCommand.RaiseCanExecuteChanged();
        }
        catch (GitHubApiException ex)
        {
            Fail(ex);
        }
        finally
        {
            RateLimitRemaining = _api.RateLimitRemaining;
        }
    }

    /// <summary>Posts the comment the user wrote on the selected issue.</summary>
    /// <returns>A task that completes when the comment is posted or the request failed.</returns>
    private async Task AddCommentAsync()
    {
        if (SelectedIssue is not { } issue || SelectedRepository is not { } repository)
        {
            return;
        }

        try
        {
            var updated = await _api.AddCommentAsync(repository.FullName, issue.Number, NewCommentText).ConfigureAwait(false);
            ErrorMessage = string.Empty;
            issue.UpdateFrom(updated);
            NewCommentText = string.Empty;
        }
        catch (GitHubApiException ex)
        {
            Fail(ex);
        }
        finally
        {
            RateLimitRemaining = _api.RateLimitRemaining;
        }
    }

    /// <summary>Reports a failed request, and signs out when the server no longer accepts the token.</summary>
    /// <param name="ex">The failure.</param>
    private void Fail(GitHubApiException ex)
    {
        ErrorMessage = ex.RateLimitResetsAt is { } resetsAt
            ? $"{ex.Message} Try again after {resetsAt.ToString("HH:mm", CultureInfo.InvariantCulture)} UTC."
            : ex.Message;

        if (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            Reset();
        }
    }

    /// <summary>Clears the account and everything loaded for it.</summary>
    private void Reset()
    {
        IsSignedIn = false;
        CurrentUser = null;
        Repositories = [];
        SelectedRepository = null;
        Issues = [];
        SelectedIssue = null;
    }
}

// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Net;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.GitHub;

/// <summary>
/// A GitHub-style issue tracker that lives in memory. It answers with the same statuses a real server does:
/// 401 without a valid token, 403 once the hourly request quota is used up, 404 for an unknown repository or
/// issue, and 422 for a request it cannot accept. Time comes from a <see cref="ManualClock"/>, and the
/// <see cref="Gate"/> decides when each response arrives.
/// </summary>
/// <param name="clock">The clock that stamps changes and renews the request quota.</param>
[System.Diagnostics.DebuggerDisplay("RateLimitRemaining = {RateLimitRemaining}")]
public sealed class InMemoryGitHubServer(ManualClock clock) : IGitHubApi
{
    /// <summary>The number the first issue of each repository receives.</summary>
    private const int FirstIssueNumber = 101;

    /// <summary>The account name of Priya Nair.</summary>
    private const string PriyaLogin = "priya-nair";

    /// <summary>The account name of Tomas Berg.</summary>
    private const string TomasLogin = "tomas-berg";

    /// <summary>The account name of Maria Santos.</summary>
    private const string MariaLogin = "maria-santos";

    /// <summary>The name of the storefront repository.</summary>
    private const string Webshop = "acme/webshop";

    /// <summary>The name of the payments repository.</summary>
    private const string PaymentsApi = "acme/payments-api";

    /// <summary>How long a quota window lasts.</summary>
    private static readonly TimeSpan _quotaWindow = TimeSpan.FromHours(1);

    /// <summary>The known accounts.</summary>
    private readonly List<User> _users = [];

    /// <summary>The access token of each account, by token.</summary>
    private readonly Dictionary<string, string> _tokens = [];

    /// <summary>The repositories the server hosts.</summary>
    private readonly List<Repository> _repositories = [];

    /// <summary>The issues of each repository, by repository name, oldest first.</summary>
    private readonly Dictionary<string, List<Issue>> _issues = [];

    /// <summary>When the current quota window ends.</summary>
    private DateTimeOffset _windowEnds = clock.GetUtcNow() + _quotaWindow;

    /// <summary>The number of requests made in the current quota window.</summary>
    private int _used;

    /// <summary>The account the current token belongs to, or <see langword="null"/> when nobody is signed in.</summary>
    private User? _signedIn;

    /// <summary>Gets the access token of the account <c>priya-nair</c>.</summary>
    public static string PriyaToken { get; } = "ghp_priya_2f9c1d";

    /// <summary>Gets the access token of the account <c>tomas-berg</c>.</summary>
    public static string TomasToken { get; } = "ghp_tomas_8b41e7";

    /// <summary>Gets the access token of the account <c>maria-santos</c>.</summary>
    public static string MariaToken { get; } = "ghp_maria_c07a55";

    /// <summary>Gets the number of requests an account may make in each quota window.</summary>
    public static int HourlyQuota { get; } = 60;

    /// <summary>Gets the gate that decides when each response arrives.</summary>
    public ResponseGate Gate { get; } = new();

    /// <inheritdoc/>
    public int RateLimitRemaining
    {
        get
        {
            RenewQuota();
            return HourlyQuota - _used;
        }
    }

    /// <summary>Creates a server with three accounts, two repositories and five issues.</summary>
    /// <param name="clock">The clock that stamps changes and renews the request quota.</param>
    /// <returns>A new server; sign in with <see cref="PriyaToken"/> or <see cref="TomasToken"/>.</returns>
    public static InMemoryGitHubServer CreateSeeded(ManualClock clock)
    {
        InMemoryGitHubServer server = new(clock);
        server.AddUser(PriyaLogin, "Priya Nair", PriyaToken);
        server.AddUser(TomasLogin, "Tomas Berg", TomasToken);
        server.AddUser(MariaLogin, "Maria Santos", MariaToken);

        server.AddRepository("acme", "webshop", "Storefront and checkout");
        var safari = server.Seed(Webshop, "Checkout button unresponsive on Safari", IssueState.Open, MariaLogin, PriyaLogin, "bug", "checkout");
        safari.Comments =
        [
            new IssueComment(TomasLogin, "Reproduced on Safari 17.", SeedData.Instant("2026-03-02T14:20:00Z")),
            new IssueComment(PriyaLogin, "Looking into the click handler.", SeedData.Instant("2026-03-02T15:05:00Z")),
        ];
        _ = server.Seed(Webshop, "Add gift-card support", IssueState.Open, TomasLogin, null, "enhancement");
        _ = server.Seed(Webshop, "Order confirmation email shows the wrong currency", IssueState.Closed, PriyaLogin, TomasLogin, "bug", "email");

        server.AddRepository("acme", "payments-api", "Card and wallet payments");
        _ = server.Seed(PaymentsApi, "Card gateway times out under load", IssueState.Open, PriyaLogin, MariaLogin, "reliability");
        _ = server.Seed(PaymentsApi, "Document the refund endpoint", IssueState.Open, MariaLogin, null, "documentation");
        return server;
    }

    /// <inheritdoc/>
    public async Task<User> SignInAsync(string token)
    {
        await EnterAsync(requiresSignIn: false).ConfigureAwait(false);

        if (!_tokens.TryGetValue(token, out var login))
        {
            throw new GitHubApiException(HttpStatusCode.Unauthorized, "Bad credentials.", null);
        }

        _signedIn = FindUser(login);
        return _signedIn.Clone();
    }

    /// <inheritdoc/>
    public void SignOut() => _signedIn = null;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Repository>> ListRepositoriesAsync()
    {
        await EnterAsync(requiresSignIn: true).ConfigureAwait(false);

        List<Repository> repositories = [];
        foreach (var repository in _repositories)
        {
            repositories.Add(repository with { OpenIssueCount = CountOpen(repository.FullName) });
        }

        return repositories;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Issue>> ListIssuesAsync(string repository, IssueState state)
    {
        await EnterAsync(requiresSignIn: true).ConfigureAwait(false);

        var stored = IssuesOf(repository);
        List<Issue> matches = [];
        for (var index = stored.Count - 1; index >= 0; index--)
        {
            if (stored[index].State == state)
            {
                matches.Add(stored[index].Clone());
            }
        }

        return matches;
    }

    /// <inheritdoc/>
    public async Task<Issue> CreateIssueAsync(string repository, string title, string? assigneeLogin)
    {
        await EnterAsync(requiresSignIn: true).ConfigureAwait(false);

        var stored = IssuesOf(repository);
        var assignee = assigneeLogin is null ? null : FindUser(assigneeLogin);

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new GitHubApiException(HttpStatusCode.UnprocessableEntity, "Validation failed: the title is required.", null);
        }

        Issue issue = new()
        {
            Number = FirstIssueNumber + stored.Count,
            Title = title.Trim(),
            State = IssueState.Open,
            Author = _signedIn!.Clone(),
            Assignee = assignee?.Clone(),
            UpdatedAt = clock.GetUtcNow(),
        };

        stored.Add(issue);
        return issue.Clone();
    }

    /// <inheritdoc/>
    public async Task<Issue> CloseIssueAsync(string repository, int number)
    {
        await EnterAsync(requiresSignIn: true).ConfigureAwait(false);

        var issue = FindIssue(repository, number);
        if (issue.State == IssueState.Closed)
        {
            throw new GitHubApiException(HttpStatusCode.UnprocessableEntity, $"Issue {number} is already closed.", null);
        }

        issue.State = IssueState.Closed;
        issue.UpdatedAt = clock.GetUtcNow();
        return issue.Clone();
    }

    /// <inheritdoc/>
    public async Task<Issue> AddCommentAsync(string repository, int number, string body)
    {
        await EnterAsync(requiresSignIn: true).ConfigureAwait(false);

        var issue = FindIssue(repository, number);
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new GitHubApiException(HttpStatusCode.UnprocessableEntity, "Validation failed: the comment body is required.", null);
        }

        issue.Comments = [.. issue.Comments, new IssueComment(_signedIn!.Login, body.Trim(), clock.GetUtcNow())];
        issue.UpdatedAt = clock.GetUtcNow();
        return issue.Clone();
    }

    /// <summary>Uses up the request quota, so the next request fails with 403 until the clock passes the end of the window.</summary>
    public void ExhaustQuota() => _used = HourlyQuota;

    /// <summary>Makes the server forget the signed-in account, as an expired token does. The next request fails with 401.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExpireSession() => SignOut();

    /// <summary>Waits for the gate, renews the quota when its window ended, then charges one request.</summary>
    /// <param name="requiresSignIn">Whether the request needs a signed-in account.</param>
    /// <returns>A task that completes when the request may proceed.</returns>
    /// <exception cref="GitHubApiException">The quota is used up (403) or the caller is not signed in (401).</exception>
    private async Task EnterAsync(bool requiresSignIn)
    {
        await Gate.WaitAsync().ConfigureAwait(false);
        RenewQuota();

        if (_used >= HourlyQuota)
        {
            throw new GitHubApiException(HttpStatusCode.Forbidden, "API rate limit exceeded.", _windowEnds);
        }

        _used++;

        if (requiresSignIn && _signedIn is null)
        {
            throw new GitHubApiException(HttpStatusCode.Unauthorized, "Requires authentication.", null);
        }
    }

    /// <summary>Starts a new quota window when the clock has passed the end of the current one.</summary>
    private void RenewQuota()
    {
        if (clock.GetUtcNow() < _windowEnds)
        {
            return;
        }

        _used = 0;
        _windowEnds = clock.GetUtcNow() + _quotaWindow;
    }

    /// <summary>Counts the open issues of a repository.</summary>
    /// <param name="repository">The repository name.</param>
    /// <returns>The number of open issues.</returns>
    private int CountOpen(string repository)
    {
        var open = 0;
        foreach (var issue in _issues[repository])
        {
            if (issue.State == IssueState.Open)
            {
                open++;
            }
        }

        return open;
    }

    /// <summary>Finds the issues of a repository.</summary>
    /// <param name="repository">The repository name.</param>
    /// <returns>The stored issues.</returns>
    /// <exception cref="GitHubApiException">The repository does not exist (404).</exception>
    private List<Issue> IssuesOf(string repository) =>
        _issues.TryGetValue(repository, out var issues)
            ? issues
            : throw new GitHubApiException(HttpStatusCode.NotFound, $"Repository '{repository}' was not found.", null);

    /// <summary>Finds an issue.</summary>
    /// <param name="repository">The repository name.</param>
    /// <param name="number">The number of the issue.</param>
    /// <returns>The stored issue.</returns>
    /// <exception cref="GitHubApiException">The repository or the issue does not exist (404).</exception>
    private Issue FindIssue(string repository, int number)
    {
        foreach (var issue in IssuesOf(repository))
        {
            if (issue.Number == number)
            {
                return issue;
            }
        }

        throw new GitHubApiException(HttpStatusCode.NotFound, $"Issue {number} was not found in '{repository}'.", null);
    }

    /// <summary>Finds an account.</summary>
    /// <param name="login">The account name.</param>
    /// <returns>The stored account.</returns>
    /// <exception cref="GitHubApiException">The account does not exist (404).</exception>
    private User FindUser(string login)
    {
        foreach (var user in _users)
        {
            if (user.Login == login)
            {
                return user;
            }
        }

        throw new GitHubApiException(HttpStatusCode.NotFound, $"User '{login}' was not found.", null);
    }

    /// <summary>Adds an account without going through the gate.</summary>
    /// <param name="login">The account name.</param>
    /// <param name="displayName">The name the person goes by.</param>
    /// <param name="token">The access token that signs in as the account.</param>
    private void AddUser(string login, string displayName, string token)
    {
        _users.Add(new User { Login = login, DisplayName = displayName });
        _tokens[token] = login;
    }

    /// <summary>Adds a repository without going through the gate.</summary>
    /// <param name="owner">The account or organisation that owns the repository.</param>
    /// <param name="name">The name of the repository.</param>
    /// <param name="description">What the repository is for.</param>
    private void AddRepository(string owner, string name, string description)
    {
        Repository repository = new(owner, name, description, 0);
        _repositories.Add(repository);
        _issues[repository.FullName] = [];
    }

    /// <summary>Adds an issue without going through the gate.</summary>
    /// <param name="repository">The repository name.</param>
    /// <param name="title">The title of the issue.</param>
    /// <param name="state">Whether the issue is open or closed.</param>
    /// <param name="author">The account name of the reporter.</param>
    /// <param name="assignee">The account name of the assignee, or <see langword="null"/>.</param>
    /// <param name="labels">The labels of the issue.</param>
    /// <returns>The stored issue, so the caller can add comments.</returns>
    private Issue Seed(string repository, string title, IssueState state, string author, string? assignee, params string[] labels)
    {
        var stored = _issues[repository];
        Issue issue = new()
        {
            Number = FirstIssueNumber + stored.Count,
            Title = title,
            State = state,
            Author = FindUser(author).Clone(),
            Assignee = assignee is null ? null : FindUser(assignee).Clone(),
            Labels = labels,
            UpdatedAt = clock.GetUtcNow(),
        };

        stored.Add(issue);
        return issue;
    }
}

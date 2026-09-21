// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Documentation.ObservingSearchAsYouType;

/// <summary>Shows a search box that reacts to typing: observe the text, wait for a pause, and show the newest response.</summary>
public static class SearchAsYouTypeExamples
{
    /// <summary>The repository the search box looks in.</summary>
    private const string Webshop = "acme/webshop";

    /// <summary>The title of the open issue about the checkout button.</summary>
    private const string CheckoutTitle = "Checkout button unresponsive on Safari";

    /// <summary>The title of the open issue about gift cards.</summary>
    private const string GiftCardTitle = "Add gift-card support";

    /// <summary>The first term the user searches for.</summary>
    private const string CheckoutTerm = "checkout";

    /// <summary>The second term the user searches for.</summary>
    private const string GiftTerm = "gift";

    /// <summary>The term as far as the user has typed it when they pause.</summary>
    private const string CraTerm = "cra";

    /// <summary>The term the user finishes typing.</summary>
    private const string CrashTerm = "crash";

    /// <summary>The number of searches that reach the server once the user has searched for two different terms.</summary>
    private const int TwoSearches = 2;

    /// <summary>How long the user must stop typing before a search starts, in milliseconds.</summary>
    private const int QuietPeriodMilliseconds = 800;

    /// <summary>How long the user takes between two key presses, in milliseconds.</summary>
    private const int KeyGapMilliseconds = 100;

    /// <summary>How long the user must stop typing before a search starts.</summary>
    private static readonly TimeSpan _quietPeriod = TimeSpan.FromMilliseconds(QuietPeriodMilliseconds);

    /// <summary>How long the user takes between two key presses.</summary>
    private static readonly TimeSpan _keyGap = TimeSpan.FromMilliseconds(KeyGapMilliseconds);

    /// <summary>Reacts to each distinct, trimmed, non-blank term the user types.</summary>
    public static void ObserveDistinctTrimmedSearchTerms()
    {
        const string PaddedCheckout = "  checkout ";

        var server = InMemoryGitHubServer.CreateSeeded(ManualClock.StartOfWorkingDay());
        IssueSearchViewModel viewModel = new(server, Webshop);

        List<string> terms = [];

        using (viewModel.WhenChanged(x => x.SearchTerm)
            .Select(static term => term.Trim())
            .DistinctUntilChanged()
            .Where(static term => term.Length > 0)
            .Subscribe(terms.Add))
        {
            viewModel.SearchTerm = PaddedCheckout;
            viewModel.SearchTerm = CheckoutTerm;
            viewModel.SearchTerm = "   ";
            viewModel.SearchTerm = GiftTerm;
        }

        // The empty starting text, the repeated term and the blank text start no search.
        SampleCheck.SequenceEqual([CheckoutTerm, GiftTerm], terms);
    }

    /// <summary>Waits for a pause in typing before a term goes on. A virtual clock decides when the pause has passed.</summary>
    public static void ThrottleTypingUntilPause()
    {
        var server = InMemoryGitHubServer.CreateSeeded(ManualClock.StartOfWorkingDay());
        IssueSearchViewModel viewModel = new(server, Webshop);
        VirtualClock clock = new();

        List<string> settled = [];

        using var subscription = viewModel.WhenAnyValue(x => x.SearchTerm)
            .Throttle(_quietPeriod, clock)
            .Subscribe(settled.Add);

        viewModel.SearchTerm = "c";
        clock.AdvanceBy(_keyGap);
        viewModel.SearchTerm = "cr";
        clock.AdvanceBy(_keyGap);
        viewModel.SearchTerm = CraTerm;

        // Every key press restarts the wait, so nothing has settled yet.
        clock.AdvanceBy(_quietPeriod - _keyGap);
        SampleCheck.Equal(0, settled.Count);

        clock.AdvanceBy(_keyGap);
        SampleCheck.SequenceEqual([CraTerm], settled);

        viewModel.SearchTerm = CrashTerm;
        clock.AdvanceBy(_quietPeriod);
        SampleCheck.SequenceEqual([CraTerm, CrashTerm], settled);
    }

    /// <summary>Searches once per settled term and shows the matches.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task SearchWhenTypingPauses()
    {
        var server = await CreateSignedInServerAsync();
        IssueSearchViewModel viewModel = new(server, Webshop);
        VirtualClock clock = new();

        using var subscription = viewModel.StartSearching(clock, _quietPeriod);

        var quotaAtStart = server.RateLimitRemaining;

        viewModel.SearchTerm = "  checkout ";
        clock.AdvanceBy(_quietPeriod);

        SampleCheck.SequenceEqual([CheckoutTitle], TitlesOf(viewModel.Results));
        SampleCheck.Equal(false, viewModel.IsSearching);
        SampleCheck.Equal(quotaAtStart - 1, server.RateLimitRemaining);

        // The same term without its padding trims to the term already searched, so no request goes out.
        viewModel.SearchTerm = CheckoutTerm;
        clock.AdvanceBy(_quietPeriod);
        SampleCheck.Equal(quotaAtStart - 1, server.RateLimitRemaining);

        viewModel.SearchTerm = "GIFT";
        clock.AdvanceBy(_quietPeriod);

        SampleCheck.SequenceEqual([GiftCardTitle], TitlesOf(viewModel.Results));
        SampleCheck.Equal(quotaAtStart - TwoSearches, server.RateLimitRemaining);
    }

    /// <summary>Binds the busy flag to a button, a progress bar and the results to a list while a search waits for its response.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task BindSearchStateToView()
    {
        var server = await CreateSignedInServerAsync();
        IssueSearchViewModel viewModel = new(server, Webshop);
        IssueSearchView view = new() { ViewModel = viewModel };
        VirtualClock clock = new();

        using var search = viewModel.StartSearching(clock, _quietPeriod);
        using var typing = viewModel.BindTwoWay(view, x => x.SearchTerm, v => v.SearchBox.Text);
        using var buttonBinding = viewModel.BindOneWay(view, x => x.IsSearching, v => v.SearchButton.IsEnabled, static searching => !searching);
        using var barBinding = viewModel.BindOneWay(view, x => x.IsSearching, v => v.BusyBar.IsVisible);
        using var listBinding = viewModel.BindOneWay(view, x => x.Results, v => v.ResultList.Items);

        SampleCheck.Equal(true, view.SearchButton.IsEnabled);
        SampleCheck.Equal(false, view.BusyBar.IsVisible);

        server.Gate.Hold();

        view.SearchBox.Text = CheckoutTerm;
        clock.AdvanceBy(_quietPeriod);

        SampleCheck.Equal(1, server.Gate.PendingCount);
        SampleCheck.Equal(false, view.SearchButton.IsEnabled);
        SampleCheck.Equal(true, view.BusyBar.IsVisible);
        SampleCheck.Equal(0, view.ResultList.Items.Count);

        server.Gate.ReleaseAll();

        SampleCheck.Equal(true, view.SearchButton.IsEnabled);
        SampleCheck.Equal(false, view.BusyBar.IsVisible);
        SampleCheck.SequenceEqual([CheckoutTitle], TitlesOf(view.ResultList.Items));
    }

    /// <summary>Drops the response of a search that a newer search has replaced.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task IgnoreStaleSearchResponse()
    {
        var server = await CreateSignedInServerAsync();
        IssueSearchViewModel viewModel = new(server, Webshop);
        VirtualClock clock = new();

        using var subscription = viewModel.StartSearching(clock, _quietPeriod);

        server.Gate.Hold();

        viewModel.SearchTerm = CheckoutTerm;
        clock.AdvanceBy(_quietPeriod);
        viewModel.SearchTerm = GiftTerm;
        clock.AdvanceBy(_quietPeriod);

        SampleCheck.Equal(TwoSearches, server.Gate.PendingCount);

        // The gate answers the oldest request first: the response for "checkout" arrives after "gift" has started.
        server.Gate.ReleaseNext();

        SampleCheck.Equal(0, viewModel.Results.Count);
        SampleCheck.Equal(true, viewModel.IsSearching);

        server.Gate.ReleaseNext();

        SampleCheck.SequenceEqual([GiftCardTitle], TitlesOf(viewModel.Results));
        SampleCheck.Equal(false, viewModel.IsSearching);
    }

    /// <summary>Creates a seeded server with Priya signed in.</summary>
    /// <returns>A task whose result is the signed-in server.</returns>
    private static async Task<InMemoryGitHubServer> CreateSignedInServerAsync()
    {
        var server = InMemoryGitHubServer.CreateSeeded(ManualClock.StartOfWorkingDay());
        _ = await server.SignInAsync(InMemoryGitHubServer.PriyaToken);
        return server;
    }

    /// <summary>Reads the titles of a list of issues.</summary>
    /// <param name="issues">The issues.</param>
    /// <returns>The titles, in the order of the issues.</returns>
    private static List<string> TitlesOf(IReadOnlyList<Issue> issues)
    {
        List<string> titles = [];
        foreach (var issue in issues)
        {
            titles.Add(issue.Title);
        }

        return titles;
    }
}

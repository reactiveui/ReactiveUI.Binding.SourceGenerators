// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Education;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.BindingsCommands;

/// <summary>Demonstrates <c>BindCommand</c> and <c>InvokeCommand</c> with every overload they offer.</summary>
public static class BindingsCommandsExamples
{
    /// <summary>The name of the event a button raises when the user presses it.</summary>
    private const string ClickEventName = "Click";

    /// <summary>The number of exports after one press with both bindings attached.</summary>
    private const int TwoBindingRuns = 2;

    /// <summary>The number of exports after a second press with only the plain binding attached.</summary>
    private const int ThreeBindingRuns = 3;

    /// <summary>The title of the to-do item the add button creates.</summary>
    private const string NewTodoTitle = "Book electrician";

    /// <summary>The number of to-do items after the add button stores one more.</summary>
    private const int TodoCountAfterAdd = 5;

    /// <summary>The number of unfinished seeded to-do items.</summary>
    private const int SeededRemainingCount = 3;

    /// <summary>The number of unfinished to-do items after one is completed.</summary>
    private const int RemainingCountAfterComplete = 2;

    /// <summary>The amount the customer sends to the utility.</summary>
    private const decimal UtilityAmount = 250M;

    /// <summary>The balance of the everyday account after the utility is paid.</summary>
    private const decimal EverydayBalanceAfterPayment = 2200.75M;

    /// <summary>The number of open issues in the webshop repository.</summary>
    private const int OpenWebshopIssueCount = 2;

    /// <summary>The position of Diego Alvarez in the list of students who could join Linear Algebra.</summary>
    private const int DiegoCandidateIndex = 2;

    /// <summary>The number of students in Linear Algebra after Diego joins.</summary>
    private const int MathRosterCountAfterEnrol = 3;

    /// <summary>The position of Linear Algebra in the list of courses.</summary>
    private const int MathCourseIndex = 1;

    /// <summary>The name of the file the user picks in the upload dialog.</summary>
    private const string SpringCampaignFileName = "spring-campaign.png";

    /// <summary>The size in bytes of the picked file.</summary>
    private const long SpringCampaignSize = 3_145_728;

    /// <summary>The number of objects in the photos folder before the picked file is stored.</summary>
    private const int SeededPhotosCount = 2;

    /// <summary>The number of objects in the photos folder once the picked file is stored.</summary>
    private const int PhotosCountAfterUpload = 3;

    /// <summary>The position of the uploaded file among the objects of the photos folder.</summary>
    private const int SpringCampaignPosition = 1;

    /// <summary>Binds the add button of the to-do screen to the view model's add command; the button follows the command's <c>CanExecute</c>.</summary>
    /// <returns>A task that completes when the item is stored.</returns>
    public static async Task BindAddButton()
    {
        var store = InMemoryTodoStore.CreateSeeded();
        TodoListViewModel viewModel = new(store);
        viewModel.LoadCommand.Execute(null);
        TodoView view = new() { ViewModel = viewModel };

        using var binding = view.BindCommand(viewModel, x => x.AddCommand, v => v.AddButton);

        SampleCheck.Equal(false, view.AddButton.IsEnabled);

        viewModel.NewTitle = NewTodoTitle;

        SampleCheck.Equal(true, view.AddButton.IsEnabled);

        view.AddButton.Press();
        await viewModel.AddCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal(TodoCountAfterAdd, viewModel.Items.Count);
        SampleCheck.Equal(NewTodoTitle, viewModel.SelectedItem!.Title);
        SampleCheck.Equal(false, view.AddButton.IsEnabled);
    }

    /// <summary>Binds the complete button through its <c>Click</c> event; the binding runs the command on a click and leaves the button's enabled state alone.</summary>
    /// <returns>A task that completes when the item is finished.</returns>
    public static async Task BindCompleteButtonToNamedEvent()
    {
        var store = InMemoryTodoStore.CreateSeeded();
        TodoListViewModel viewModel = new(store);
        viewModel.LoadCommand.Execute(null);
        TodoView view = new() { ViewModel = viewModel };

        using var binding = view.BindCommand(viewModel, x => x.CompleteCommand, v => v.CompleteButton, toEvent: ClickEventName);

        view.CompleteButton.Press();

        SampleCheck.Equal(SeededRemainingCount, viewModel.RemainingCount);

        viewModel.SelectedItem = viewModel.Items[0];
        view.CompleteButton.Press();
        await viewModel.CompleteCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal(RemainingCountAfterComplete, viewModel.RemainingCount);
        SampleCheck.Equal(true, view.CompleteButton.IsEnabled);
    }

    /// <summary>
    /// Binds one button to one command twice, with and without <c>toEvent</c>. Each call is its own binding: the plain one
    /// attaches the command to the button and follows its <c>CanExecute</c>, and the event one runs the command on each click.
    /// </summary>
    public static void BindExportButtonWithAndWithoutEvent()
    {
        StatementExportViewModel viewModel = new() { HasStatement = true };
        StatementExportView view = new() { ViewModel = viewModel };

        var attached = view.BindCommand(viewModel, x => x.ExportCommand, v => v.ExportButton);
        var clicked = view.BindCommand(viewModel, x => x.ExportCommand, v => v.ExportButton, toEvent: ClickEventName);

        SampleCheck.Equal(true, ReferenceEquals(viewModel.ExportCommand, view.ExportButton.Command));

        // One press reaches the command through both bindings.
        view.ExportButton.Press();

        SampleCheck.Equal(TwoBindingRuns, viewModel.ExportCount);

        clicked.Dispose();
        view.ExportButton.Press();

        SampleCheck.Equal(ThreeBindingRuns, viewModel.ExportCount);

        // Only the plain binding follows CanExecute, so the button goes dark while nothing is chosen.
        viewModel.HasStatement = false;

        SampleCheck.Equal(false, view.ExportButton.IsEnabled);

        attached.Dispose();
    }

    /// <summary>Binds the sign-in button; it stays disabled until the user has typed an access token.</summary>
    /// <returns>A task that completes when the sign-in has finished.</returns>
    public static async Task BindSignInButton()
    {
        var server = InMemoryGitHubServer.CreateSeeded(ManualClock.StartOfWorkingDay());
        IssueBoardViewModel viewModel = new(server);
        IssueBoardView view = new() { ViewModel = viewModel };

        using var binding = view.BindCommand(viewModel, x => x.SignInCommand, v => v.SignInButton);

        SampleCheck.Equal(false, view.SignInButton.IsEnabled);

        viewModel.Token = InMemoryGitHubServer.PriyaToken;

        SampleCheck.Equal(true, view.SignInButton.IsEnabled);

        view.SignInButton.Press();
        await viewModel.SignInCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal("priya-nair", viewModel.CurrentUser!.Login);
        SampleCheck.Equal(false, view.SignInButton.IsEnabled);
    }

    /// <summary>Binds the close-issue button; the view model asks a confirmation handler before it closes the issue.</summary>
    /// <returns>A task that completes when the issue is closed.</returns>
    public static async Task BindCloseIssueButton()
    {
        var viewModel = await OpenWebshopIssuesAsync().ConfigureAwait(false);
        IssueBoardView view = new() { ViewModel = viewModel };
        using var confirmation = viewModel.ConfirmClose.RegisterHandler(static context => context.SetOutput(true));

        using var binding = view.BindCommand(viewModel, x => x.CloseIssueCommand, v => v.CloseIssueButton);

        SampleCheck.Equal(false, view.CloseIssueButton.IsEnabled);

        var issue = viewModel.Issues[0];
        viewModel.SelectedIssue = issue;

        SampleCheck.Equal(true, view.CloseIssueButton.IsEnabled);

        view.CloseIssueButton.Press();
        await viewModel.CloseIssueCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal(IssueState.Closed, issue.State);
        SampleCheck.Equal(false, view.CloseIssueButton.IsEnabled);
    }

    /// <summary>Binds the transfer button; it is disabled until the draft breaks none of the view model's rules.</summary>
    /// <returns>A task that completes when the transfer has been sent.</returns>
    public static async Task BindTransferButton()
    {
        InMemoryBankingBackend backend = new(ManualClock.StartOfWorkingDay());
        TransferViewModel viewModel = new(backend);
        viewModel.LoadCommand.Execute(null);
        await viewModel.LoadCommand.Completion.ConfigureAwait(false);
        using var confirmation = viewModel.ConfirmTransfer.RegisterHandler(static context => context.SetOutput(true));
        TransferView view = new() { ViewModel = viewModel };

        using var binding = view.BindCommand(viewModel, x => x.TransferCommand, v => v.TransferButton);

        SampleCheck.Equal(false, view.TransferButton.IsEnabled);

        viewModel.Draft.Source = viewModel.Accounts[0];
        viewModel.Draft.Payee = viewModel.Payees[0];

        SampleCheck.Equal(false, view.TransferButton.IsEnabled);

        viewModel.Draft.Amount = UtilityAmount;

        SampleCheck.Equal(true, view.TransferButton.IsEnabled);

        view.TransferButton.Press();
        await viewModel.TransferCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal(EverydayBalanceAfterPayment, viewModel.LastReceipt!.NewBalance);
        SampleCheck.Equal(false, view.TransferButton.IsEnabled);
    }

    /// <summary>Binds the enrol button with an observable command parameter: the student selected in the candidate list.</summary>
    /// <returns>A task that completes when the student is on the roster.</returns>
    public static async Task BindEnrolButtonWithObservableParameter()
    {
        var records = InMemoryStudentRecords.CreateSeeded(ManualClock.StartOfWorkingDay());
        GradebookViewModel viewModel = new(records);
        viewModel.LoadCoursesCommand.Execute(null);
        await viewModel.LoadCoursesCommand.Completion.ConfigureAwait(false);
        viewModel.SelectedCourse = viewModel.Courses[MathCourseIndex];
        viewModel.OpenCourseCommand.Execute(null);
        await viewModel.OpenCourseCommand.Completion.ConfigureAwait(false);
        GradebookView view = new() { ViewModel = viewModel };
        view.CandidateList.Items = viewModel.Candidates;

        using var binding = view.BindCommand(viewModel, x => x.EnrolCommand, v => v.EnrolButton, view.CandidateList.WhenChanged(x => x.SelectedItem!));

        SampleCheck.Equal(false, view.EnrolButton.IsEnabled);

        view.CandidateList.SelectedItem = viewModel.Candidates[DiegoCandidateIndex];

        SampleCheck.Equal(true, view.EnrolButton.IsEnabled);

        view.EnrolButton.Press();
        await viewModel.EnrolCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal(MathRosterCountAfterEnrol, viewModel.Roster.Count);
    }

    /// <summary>Binds the upload button of the browser through its <c>Click</c> event with an observable command parameter: each file the user picks.</summary>
    /// <returns>A task that completes when the file is stored.</returns>
    public static async Task BindUploadButtonWithObservableParameterOnNamedEvent()
    {
        var browser = await OpenPhotosFolderAsync().ConfigureAwait(false);
        StorageBrowserView view = new() { ViewModel = browser };
        Signal<UploadRequest> pickedFiles = new();

        using var binding = view.BindCommand(browser, x => x.UploadCommand, v => v.UploadButton, pickedFiles, toEvent: ClickEventName);

        pickedFiles.OnNext(SpringCampaign());
        view.UploadButton.Press();
        await browser.UploadCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal(PhotosCountAfterUpload, browser.Objects.Count);
    }

    /// <summary>Binds the upload button through its <c>Click</c> event with a parameter expression.</summary>
    /// <returns>A task that completes when the file is stored.</returns>
    public static async Task BindUploadButtonWithParameterExpressionOnNamedEvent()
    {
        var browser = await OpenPhotosFolderAsync().ConfigureAwait(false);
        UploadPanelViewModel viewModel = new(browser);
        UploadPanelView view = new() { ViewModel = viewModel };

        using var binding = view.BindCommand(viewModel, panel => panel.UploadCommand, v => v.UploadButton, panel => panel.PendingUpload, toEvent: ClickEventName);

        viewModel.PendingUpload = SpringCampaign();
        view.UploadButton.Press();
        await browser.UploadCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal(PhotosCountAfterUpload, browser.Objects.Count);
    }

    /// <summary>Binds the upload button with a parameter expression: the file the user picked, read from the view model.</summary>
    /// <returns>A task that completes when the file is stored.</returns>
    public static async Task BindUploadButtonWithParameterExpression()
    {
        var browser = await OpenPhotosFolderAsync().ConfigureAwait(false);
        UploadPanelViewModel viewModel = new(browser);
        UploadPanelView view = new() { ViewModel = viewModel };

        using var binding = view.BindCommand(viewModel, x => x.UploadCommand, v => v.UploadButton, x => x.PendingUpload);

        SampleCheck.Equal(false, view.UploadButton.IsEnabled);

        viewModel.PendingUpload = SpringCampaign();

        SampleCheck.Equal(true, view.UploadButton.IsEnabled);

        view.UploadButton.Press();
        await browser.UploadCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal(PhotosCountAfterUpload, browser.Objects.Count);
        SampleCheck.Equal($"photos/2026/{SpringCampaignFileName}", browser.Objects[SpringCampaignPosition].Key);
    }

    /// <summary>Executes the sign-in command each time the access token changes; a blank token is refused by the command and dropped.</summary>
    /// <returns>A task that completes when the sign-in has finished.</returns>
    public static async Task InvokeSignInWhenTokenChanges()
    {
        var server = InMemoryGitHubServer.CreateSeeded(ManualClock.StartOfWorkingDay());
        IssueBoardViewModel viewModel = new(server);

        using var subscription = viewModel.WhenChanged(x => x.Token).InvokeCommand(viewModel.SignInCommand);

        viewModel.Token = "   ";

        SampleCheck.Equal(false, viewModel.IsSignedIn);
        SampleCheck.Equal(InMemoryGitHubServer.HourlyQuota, viewModel.RateLimitRemaining);

        viewModel.Token = InMemoryGitHubServer.PriyaToken;
        await viewModel.SignInCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal(true, viewModel.IsSignedIn);
    }

    /// <summary>Executes the browser's upload command with each file the user picks, so the picked file reaches the command as its parameter.</summary>
    /// <returns>A task that completes when the file is stored.</returns>
    public static async Task InvokeUploadWithPickedFile()
    {
        var browser = await OpenPhotosFolderAsync().ConfigureAwait(false);
        UploadPanelViewModel viewModel = new(browser);

        using var subscription = viewModel.WhenChanged(x => x.PendingUpload!).InvokeCommand(browser.UploadCommand);

        SampleCheck.Equal(SeededPhotosCount, browser.Objects.Count);

        viewModel.PendingUpload = SpringCampaign();
        await browser.UploadCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal(PhotosCountAfterUpload, browser.Objects.Count);
    }

    /// <summary>Executes the command a property holds each time the selected repository changes; the property is read from the view model.</summary>
    /// <returns>A task that completes when the issues are loaded.</returns>
    public static async Task InvokeLoadIssuesWhenRepositoryChanges()
    {
        var server = InMemoryGitHubServer.CreateSeeded(ManualClock.StartOfWorkingDay());
        IssueBoardViewModel viewModel = new(server) { Token = InMemoryGitHubServer.PriyaToken };
        viewModel.SignInCommand.Execute(null);
        await viewModel.SignInCommand.Completion.ConfigureAwait(false);

        using var subscription = viewModel.WhenChanged(x => x.SelectedRepository!).InvokeCommand(viewModel, x => x.LoadIssuesCommand);

        SampleCheck.Equal(0, viewModel.Issues.Count);

        viewModel.SelectedRepository = viewModel.Repositories[0];
        await viewModel.LoadIssuesCommand.Completion.ConfigureAwait(false);

        SampleCheck.Equal(OpenWebshopIssueCount, viewModel.Issues.Count);
    }

    /// <summary>Creates the file the user picks in the upload dialog.</summary>
    /// <returns>The upload request.</returns>
    private static UploadRequest SpringCampaign() => new(SpringCampaignFileName, SpringCampaignSize, "image/png");

    /// <summary>Signs in as Priya Nair and loads the open issues of the webshop repository.</summary>
    /// <returns>A task that completes with the view model once the issues are loaded.</returns>
    private static async Task<IssueBoardViewModel> OpenWebshopIssuesAsync()
    {
        var server = InMemoryGitHubServer.CreateSeeded(ManualClock.StartOfWorkingDay());
        IssueBoardViewModel viewModel = new(server) { Token = InMemoryGitHubServer.PriyaToken };
        viewModel.SignInCommand.Execute(null);
        await viewModel.SignInCommand.Completion.ConfigureAwait(false);
        viewModel.SelectedRepository = viewModel.Repositories[0];
        viewModel.LoadIssuesCommand.Execute(null);
        await viewModel.LoadIssuesCommand.Completion.ConfigureAwait(false);
        return viewModel;
    }

    /// <summary>Opens the 2026 photos folder of the media bucket.</summary>
    /// <returns>A task that completes with the browser once the folder is listed.</returns>
    private static async Task<StorageBrowserViewModel> OpenPhotosFolderAsync()
    {
        var storage = InMemoryObjectStorage.CreateSeeded(ManualClock.StartOfWorkingDay());
        StorageBrowserViewModel browser = new(storage);
        browser.LoadBucketsCommand.Execute(null);
        await browser.LoadBucketsCommand.Completion.ConfigureAwait(false);
        browser.SelectedBucket = browser.Buckets[0];
        browser.CurrentPrefix = "photos/2026/";
        browser.RefreshCommand.Execute(null);
        await browser.RefreshCommand.Completion.ConfigureAwait(false);
        return browser;
    }
}

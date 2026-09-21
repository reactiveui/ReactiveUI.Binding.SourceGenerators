// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.CloudStorage;

/// <summary>
/// The view model behind the storage browser. It lists buckets and the objects under a prefix, uploads files
/// and follows the state of the link. Each command starts its work and returns; wait for it through the
/// command's <see cref="AsyncDelegateCommand.Completion"/>. A failed request never throws from a command. It
/// sets <see cref="ErrorMessage"/>.
/// </summary>
[System.Diagnostics.DebuggerDisplay("Bucket = {SelectedBucket}, Objects = {Objects.Count}, UploadPercent = {UploadPercent}")]
public sealed class StorageBrowserViewModel : ObservableObject
{
    /// <summary>The value of <see cref="UploadPercent"/> when the upload is complete.</summary>
    private const double FullPercent = 100;

    /// <summary>The service the view model calls.</summary>
    private readonly IObjectStorage _storage;

    /// <summary>Initializes a new instance of the <see cref="StorageBrowserViewModel"/> class.</summary>
    /// <param name="storage">The service to call.</param>
    public StorageBrowserViewModel(IObjectStorage storage)
    {
        _storage = storage;
        LoadBucketsCommand = new(_ => LoadBucketsAsync());
        RefreshCommand = new(_ => LoadObjectsAsync(), _ => SelectedBucket is not null);
        UploadCommand = new(parameter => UploadAsync((UploadRequest)parameter!), parameter => parameter is UploadRequest && SelectedBucket is not null);
        ConnectCommand = new(_ => _storage.ConnectAsync(), _ => ConnectionStatus == ConnectionState.Disconnected);
        ConnectionStatus = storage.Connection.State;
        storage.Connection.StateChanged += OnConnectionStateChanged;
    }

    /// <summary>Gets the command that lists the buckets.</summary>
    public AsyncDelegateCommand LoadBucketsCommand { get; }

    /// <summary>Gets the command that lists the objects of <see cref="SelectedBucket"/> under <see cref="CurrentPrefix"/>.</summary>
    public AsyncDelegateCommand RefreshCommand { get; }

    /// <summary>Gets the command that uploads a file. Its parameter is an <see cref="UploadRequest"/>; the file is stored under <see cref="CurrentPrefix"/>.</summary>
    public AsyncDelegateCommand UploadCommand { get; }

    /// <summary>Gets the command that reopens the link while <see cref="ConnectionStatus"/> is <see cref="ConnectionState.Disconnected"/>.</summary>
    public AsyncDelegateCommand ConnectCommand { get; }

    /// <summary>Gets the link to the service. It is a plain class that raises only <see cref="StorageConnection.StateChanged"/>.</summary>
    public StorageConnection Connection => _storage.Connection;

    /// <summary>Gets the state of the link, kept in step with <see cref="Connection"/>.</summary>
    public ConnectionState ConnectionStatus
    {
        get;
        private set
        {
            if (!SetProperty(ref field, value))
            {
                return;
            }

            ConnectCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>Gets the buckets of the account.</summary>
    public IReadOnlyList<Bucket> Buckets
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets the bucket the browser shows.</summary>
    public Bucket? SelectedBucket
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
            {
                return;
            }

            RefreshCommand.RaiseCanExecuteChanged();
            UploadCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>Gets or sets the folder the browser lists, such as <c>photos/2026/</c>. Empty lists the whole bucket.</summary>
    public string CurrentPrefix
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets the objects of the selected bucket under <see cref="CurrentPrefix"/>.</summary>
    public IReadOnlyList<StorageObject> Objects
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets the object the user picked. <see cref="StorageObject"/> raises no change notifications.</summary>
    public StorageObject? SelectedObject
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the combined size in bytes of <see cref="Objects"/>.</summary>
    public long TotalSize
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets a value indicating whether an upload is running.</summary>
    public bool IsUploading
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets how much of the running upload the service has received, from 0 to 100.</summary>
    public double UploadPercent
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

    /// <summary>Adds up the sizes of objects.</summary>
    /// <param name="objects">The objects to add up.</param>
    /// <returns>The combined size in bytes.</returns>
    private static long SumSizes(IReadOnlyList<StorageObject> objects)
    {
        long total = 0;
        foreach (var stored in objects)
        {
            total += stored.Size;
        }

        return total;
    }

    /// <summary>Lists the buckets.</summary>
    /// <returns>A task that completes when the buckets are listed or the request failed.</returns>
    private async Task LoadBucketsAsync()
    {
        try
        {
            Buckets = await _storage.ListBucketsAsync().ConfigureAwait(false);
            ErrorMessage = string.Empty;
        }
        catch (StorageException ex)
        {
            ErrorMessage = $"{ex.Failure}: {ex.Message}";
        }
    }

    /// <summary>Lists the objects of the selected bucket under the current prefix.</summary>
    /// <returns>A task that completes when the objects are listed or the request failed.</returns>
    private async Task LoadObjectsAsync()
    {
        if (SelectedBucket is not { } bucket)
        {
            return;
        }

        try
        {
            var objects = await _storage.ListObjectsAsync(bucket.Name, CurrentPrefix).ConfigureAwait(false);
            ErrorMessage = string.Empty;
            Objects = objects;
            SelectedObject = null;
            TotalSize = SumSizes(objects);
        }
        catch (StorageException ex)
        {
            ErrorMessage = $"{ex.Failure}: {ex.Message}";
        }
    }

    /// <summary>Uploads a file to the selected bucket, then lists the objects again.</summary>
    /// <param name="request">The file to upload.</param>
    /// <returns>A task that completes when the file is stored or the request failed.</returns>
    private async Task UploadAsync(UploadRequest request)
    {
        if (SelectedBucket is not { } bucket)
        {
            return;
        }

        UploadPercent = 0;
        IsUploading = true;

        try
        {
            ImmediateProgress<UploadProgress> progress = new(OnUploadProgress);
            await _storage.UploadAsync(bucket.Name, CurrentPrefix + request.FileName, request.SizeBytes, request.ContentType, progress).ConfigureAwait(false);
            ErrorMessage = string.Empty;
            await LoadObjectsAsync().ConfigureAwait(false);
        }
        catch (StorageException ex)
        {
            ErrorMessage = $"{ex.Failure}: {ex.Message}";
        }
        finally
        {
            IsUploading = false;
        }
    }

    /// <summary>Shows how much of the upload the service has received.</summary>
    /// <param name="report">The progress report.</param>
    private void OnUploadProgress(UploadProgress report) => UploadPercent = report.Fraction * FullPercent;

    /// <summary>Mirrors the state of the link.</summary>
    /// <param name="sender">The connection.</param>
    /// <param name="e">The event data.</param>
    private void OnConnectionStateChanged(object? sender, EventArgs e) => ConnectionStatus = _storage.Connection.State;
}

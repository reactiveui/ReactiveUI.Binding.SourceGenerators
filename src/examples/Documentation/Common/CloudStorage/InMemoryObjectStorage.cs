// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.CloudStorage;

/// <summary>
/// An S3-style storage service that lives in memory. Time comes from a <see cref="ManualClock"/>. A held
/// <see cref="Gate"/> makes every request wait once to start, and an upload wait once more for each part, so an
/// example decides when each progress report arrives. <see cref="ThrottleNext"/> and <see cref="Disconnect"/>
/// make the service refuse requests.
/// </summary>
/// <param name="clock">The clock that stamps stored objects.</param>
[System.Diagnostics.DebuggerDisplay("Connection = {Connection.State}, Throttled = {_throttled}")]
public sealed class InMemoryObjectStorage(ManualClock clock) : IObjectStorage
{
    /// <summary>The size of one part of an upload, 4 MiB.</summary>
    private const long PartSizeBytes = 4_194_304;

    /// <summary>The size of the launch banner, about 2.3 MiB.</summary>
    private const long BannerSize = 2_411_724;

    /// <summary>The size of the offsite photo, about 5.3 MiB.</summary>
    private const long OffsitePhotoSize = 5_562_368;

    /// <summary>The size of the warehouse photo, about 1.6 MiB.</summary>
    private const long WarehousePhotoSize = 1_677_722;

    /// <summary>The size of the product tour video, about 148 MiB.</summary>
    private const long ProductTourSize = 155_189_248;

    /// <summary>The size of the landing page, 12 KiB.</summary>
    private const long LandingPageSize = 12_288;

    /// <summary>The size of a nightly database dump, about 96 MiB.</summary>
    private const long DatabaseDumpSize = 100_663_296;

    /// <summary>The media type of a JPEG photo.</summary>
    private const string JpegType = "image/jpeg";

    /// <summary>The media type of a gzip archive.</summary>
    private const string GzipType = "application/gzip";

    /// <summary>The name of the bucket that holds the marketing files.</summary>
    private const string MediaBucket = "acme-media";

    /// <summary>The name of the bucket that holds the backups.</summary>
    private const string BackupsBucket = "acme-backups";

    /// <summary>The buckets of the account.</summary>
    private readonly List<Bucket> _buckets = [];

    /// <summary>The objects of each bucket, by bucket name, ordered by key.</summary>
    private readonly Dictionary<string, List<StorageObject>> _objects = [];

    /// <summary>The number of following requests the service refuses.</summary>
    private int _throttled;

    /// <summary>The number that makes each stored object's tag unique.</summary>
    private int _version;

    /// <summary>Gets the gate that decides when each request and each upload part completes.</summary>
    public ResponseGate Gate { get; } = new();

    /// <inheritdoc/>
    public StorageConnection Connection { get; } = new() { Endpoint = "https://s3.ap-southeast-2.storage.example", State = ConnectionState.Connected };

    /// <summary>Creates a service with two buckets and seven objects, already connected.</summary>
    /// <param name="clock">The clock that stamps stored objects.</param>
    /// <returns>A new service.</returns>
    public static InMemoryObjectStorage CreateSeeded(ManualClock clock)
    {
        InMemoryObjectStorage storage = new(clock);
        storage.AddBucket(MediaBucket, "ap-southeast-2", "2025-06-12T08:30:00Z");
        storage.Seed(MediaBucket, "index.html", LandingPageSize, "text/html", "2026-01-15T10:00:00Z");
        storage.Seed(MediaBucket, "photos/2025/warehouse.jpg", WarehousePhotoSize, JpegType, "2025-11-20T16:45:00Z");
        storage.Seed(MediaBucket, "photos/2026/launch-banner.png", BannerSize, "image/png", "2026-02-10T09:12:00Z");
        storage.Seed(MediaBucket, "photos/2026/team-offsite.jpg", OffsitePhotoSize, JpegType, "2026-02-27T18:30:00Z");
        storage.Seed(MediaBucket, "videos/product-tour.mp4", ProductTourSize, "video/mp4", "2026-01-30T13:05:00Z");
        storage.AddBucket(BackupsBucket, "us-east-1", "2024-09-01T00:00:00Z");
        storage.Seed(BackupsBucket, "db/2026-03-01.sql.gz", DatabaseDumpSize, GzipType, "2026-03-01T02:00:00Z");
        storage.Seed(BackupsBucket, "db/2026-03-02.sql.gz", DatabaseDumpSize, GzipType, "2026-03-02T02:00:00Z");
        return storage;
    }

    /// <inheritdoc/>
    public async Task ConnectAsync()
    {
        Connection.State = ConnectionState.Connecting;
        await Gate.WaitAsync().ConfigureAwait(false);
        Connection.State = ConnectionState.Connected;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Bucket>> ListBucketsAsync()
    {
        await EnterAsync().ConfigureAwait(false);

        return [.. _buckets];
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StorageObject>> ListObjectsAsync(string bucket, string prefix)
    {
        await EnterAsync().ConfigureAwait(false);

        List<StorageObject> matches = [];
        foreach (var stored in ObjectsOf(bucket))
        {
            if (stored.Key.StartsWith(prefix, StringComparison.Ordinal))
            {
                matches.Add(stored.Clone());
            }
        }

        return matches;
    }

    /// <inheritdoc/>
    public async Task<StorageObject> UploadAsync(string bucket, string key, long sizeBytes, string contentType, IProgress<UploadProgress> progress)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sizeBytes);
        await EnterAsync().ConfigureAwait(false);

        var stored = ObjectsOf(bucket);
        long sent = 0;
        while (sent < sizeBytes)
        {
            await Gate.WaitAsync().ConfigureAwait(false);
            EnsureConnected();
            sent = Math.Min(sent + PartSizeBytes, sizeBytes);
            progress.Report(new(sent, sizeBytes));
        }

        StorageObject uploaded = new() { Key = key, Size = sizeBytes, LastModified = clock.GetUtcNow(), ContentType = contentType, ETag = NextETag() };

        _ = stored.RemoveAll(existing => existing.Key == key);
        stored.Add(uploaded);
        stored.Sort(static (left, right) => string.CompareOrdinal(left.Key, right.Key));
        return uploaded.Clone();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string bucket, string key)
    {
        await EnterAsync().ConfigureAwait(false);

        if (ObjectsOf(bucket).RemoveAll(existing => existing.Key == key) == 0)
        {
            throw new StorageException(StorageFailure.NoSuchKey, $"The key '{key}' does not exist in bucket '{bucket}'.");
        }
    }

    /// <summary>Makes the service refuse the next requests with <see cref="StorageFailure.SlowDown"/>.</summary>
    /// <param name="requests">How many requests to refuse.</param>
    public void ThrottleNext(int requests) => _throttled = requests;

    /// <summary>Drops the link. Requests fail with <see cref="StorageFailure.ConnectionLost"/> until <see cref="ConnectAsync"/> completes.</summary>
    public void Disconnect() => Connection.State = ConnectionState.Disconnected;

    /// <summary>Waits for the gate, then refuses the request when the link is down or the service is throttling.</summary>
    /// <returns>A task that completes when the request may proceed.</returns>
    /// <exception cref="StorageException">The link is down or the service is throttling.</exception>
    private async Task EnterAsync()
    {
        await Gate.WaitAsync().ConfigureAwait(false);
        EnsureConnected();

        if (_throttled == 0)
        {
            return;
        }

        _throttled--;
        throw new StorageException(StorageFailure.SlowDown, "Please reduce your request rate.");
    }

    /// <summary>Refuses the request when the link is down.</summary>
    /// <exception cref="StorageException">The link is down.</exception>
    private void EnsureConnected()
    {
        if (Connection.State != ConnectionState.Connected)
        {
            throw new StorageException(StorageFailure.ConnectionLost, "The connection to the storage service was lost.");
        }
    }

    /// <summary>Finds the objects of a bucket.</summary>
    /// <param name="bucket">The name of the bucket.</param>
    /// <returns>The stored objects, ordered by key.</returns>
    /// <exception cref="StorageException">The bucket does not exist.</exception>
    private List<StorageObject> ObjectsOf(string bucket) =>
        _objects.TryGetValue(bucket, out var stored)
            ? stored
            : throw new StorageException(StorageFailure.NoSuchBucket, $"The bucket '{bucket}' does not exist.");

    /// <summary>Creates the tag of the next stored object.</summary>
    /// <returns>A tag that no other stored object has.</returns>
    private string NextETag()
    {
        _version++;
        return $"etag-{_version.ToString(CultureInfo.InvariantCulture)}";
    }

    /// <summary>Adds a bucket without going through the gate.</summary>
    /// <param name="name">The name of the bucket.</param>
    /// <param name="region">The region of the bucket.</param>
    /// <param name="createdAt">When the bucket was created, as an ISO 8601 text.</param>
    private void AddBucket(string name, string region, string createdAt)
    {
        _buckets.Add(new(name, region, SeedData.Instant(createdAt)));
        _objects[name] = [];
    }

    /// <summary>Adds an object without going through the gate.</summary>
    /// <param name="bucket">The name of the bucket.</param>
    /// <param name="key">The key of the object.</param>
    /// <param name="size">The size of the object in bytes.</param>
    /// <param name="contentType">The media type of the object.</param>
    /// <param name="lastModified">When the object was written, as an ISO 8601 text.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Seed(string bucket, string key, long size, string contentType, string lastModified) =>
        _objects[bucket].Add(new() { Key = key, Size = size, LastModified = SeedData.Instant(lastModified), ContentType = contentType, ETag = NextETag() });
}

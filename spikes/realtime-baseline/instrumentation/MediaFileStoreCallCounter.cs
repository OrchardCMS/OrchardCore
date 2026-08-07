// SPIKE INSTRUMENTATION — Step 1 of media-gallery-realtime-transport-plan.md §3.
// Counts real IMediaFileStore operations so the amplification factor is measured, not derived.
// DO NOT MERGE. See ../README.md for wiring instructions.
//
// Drop this file into src/OrchardCore.Modules/OrchardCore.Media/ temporarily.

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.FileStorage;

namespace OrchardCore.Media.Spike;

/// <summary>
/// Process-wide call counters, keyed by "Method" and "Method:path".
/// </summary>
public sealed class MediaFileStoreCounters
{
    private readonly ConcurrentDictionary<string, long> _counts = new();

    public void Increment(string key)
        => _counts.AddOrUpdate(key, 1, static (_, current) => current + 1);

    public IReadOnlyDictionary<string, long> Snapshot()
        => _counts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public void Reset() => _counts.Clear();

    public long Total()
    {
        long total = 0;
        foreach (var kvp in _counts)
        {
            // Only the top-level method keys, so path-scoped keys aren't double counted.
            if (!kvp.Key.Contains(':'))
            {
                total += kvp.Value;
            }
        }

        return total;
    }
}

/// <summary>
/// Decorates <see cref="IMediaFileStore"/> and counts every call. Note that GetFilesAsync and
/// GetDirectoriesAsync are default interface implementations over GetDirectoryContentAsync, so
/// GetDirectoryContentAsync is the one that reflects real storage round-trips.
/// </summary>
public sealed class CountingMediaFileStore : IMediaFileStore
{
    private readonly IMediaFileStore _inner;
    private readonly MediaFileStoreCounters _counters;

    public CountingMediaFileStore(IMediaFileStore inner, MediaFileStoreCounters counters)
    {
        _inner = inner;
        _counters = counters;
    }

    private void Count(string method, string path)
    {
        _counters.Increment(method);
        _counters.Increment($"{method}:{path ?? string.Empty}");
    }

    public Task<IFileStoreEntry> GetFileInfoAsync(string path)
    {
        Count(nameof(GetFileInfoAsync), path);
        return _inner.GetFileInfoAsync(path);
    }

    public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
    {
        Count(nameof(GetDirectoryInfoAsync), path);
        return _inner.GetDirectoryInfoAsync(path);
    }

    public IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
    {
        Count(nameof(GetDirectoryContentAsync), path);
        return _inner.GetDirectoryContentAsync(path, includeSubDirectories);
    }

    // GetFilesAsync/GetDirectoriesAsync are default interface implementations, but DefaultMediaFileStore
    // overrides both to forward to the underlying store. Implement them here too, otherwise the
    // decorator would silently route them through the default implementation instead.
    public IAsyncEnumerable<IFileStoreEntry> GetFilesAsync(string path = null)
    {
        Count(nameof(GetFilesAsync), path);
        return _inner.GetFilesAsync(path);
    }

    public IAsyncEnumerable<IFileStoreEntry> GetDirectoriesAsync(string path = null)
    {
        Count(nameof(GetDirectoriesAsync), path);
        return _inner.GetDirectoriesAsync(path);
    }

    public Task<bool> TryCreateDirectoryAsync(string path)
    {
        Count(nameof(TryCreateDirectoryAsync), path);
        return _inner.TryCreateDirectoryAsync(path);
    }

    public Task<bool> TryDeleteFileAsync(string path)
    {
        Count(nameof(TryDeleteFileAsync), path);
        return _inner.TryDeleteFileAsync(path);
    }

    public Task<bool> TryDeleteDirectoryAsync(string path)
    {
        Count(nameof(TryDeleteDirectoryAsync), path);
        return _inner.TryDeleteDirectoryAsync(path);
    }

    public Task MoveFileAsync(string oldPath, string newPath)
    {
        Count(nameof(MoveFileAsync), oldPath);
        return _inner.MoveFileAsync(oldPath, newPath);
    }

    public Task CopyFileAsync(string srcPath, string dstPath)
    {
        Count(nameof(CopyFileAsync), srcPath);
        return _inner.CopyFileAsync(srcPath, dstPath);
    }

    public Task<Stream> GetFileStreamAsync(string path)
    {
        Count(nameof(GetFileStreamAsync), path);
        return _inner.GetFileStreamAsync(path);
    }

    public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry)
    {
        Count(nameof(GetFileStreamAsync), fileStoreEntry?.Path);
        return _inner.GetFileStreamAsync(fileStoreEntry);
    }

    public Task<string> CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
    {
        Count(nameof(CreateFileFromStreamAsync), path);
        return _inner.CreateFileFromStreamAsync(path, inputStream, overwrite);
    }

    public Task<long?> GetPermittedStorageAsync()
    {
        Count(nameof(GetPermittedStorageAsync), null);
        return _inner.GetPermittedStorageAsync();
    }

    public string StorageName => _inner.StorageName;

    public IFileStoreCapabilities Capabilities => _inner.Capabilities;

    public string MapPathToPublicUrl(string path) => _inner.MapPathToPublicUrl(path);
}

public static class MediaFileStoreCounterExtensions
{
    /// <summary>
    /// Wraps whatever <see cref="IMediaFileStore"/> descriptor is already registered.
    /// Call this AFTER the module's own registration (i.e. at the end of ConfigureServices).
    /// </summary>
    public static IServiceCollection AddMediaFileStoreCallCounter(this IServiceCollection services)
    {
        services.AddSingleton<MediaFileStoreCounters>();

        var descriptor = services.LastOrDefault(d => d.ServiceType == typeof(IMediaFileStore));

        if (descriptor is null)
        {
            return services;
        }

        services.Remove(descriptor);

        services.AddSingleton<IMediaFileStore>(sp =>
        {
            var inner = (IMediaFileStore)(descriptor.ImplementationInstance
                ?? descriptor.ImplementationFactory?.Invoke(sp)
                ?? ActivatorUtilities.CreateInstance(sp, descriptor.ImplementationType!));

            return new CountingMediaFileStore(inner, sp.GetRequiredService<MediaFileStoreCounters>());
        });

        return services;
    }

    /// <summary>
    /// GET  api/media/_spike/counters          -> JSON snapshot
    /// POST api/media/_spike/counters/reset    -> zero the counters
    /// Anonymous on purpose: this is a local dev spike endpoint. Never ship it.
    /// </summary>
    public static IEndpointRouteBuilder MapMediaFileStoreCounters(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("api/media/_spike/counters", (MediaFileStoreCounters counters) =>
            Results.Ok(new
            {
                total = counters.Total(),
                counts = counters.Snapshot().OrderByDescending(kvp => kvp.Value).ToDictionary(k => k.Key, v => v.Value),
            })).AllowAnonymous();

        routes.MapPost("api/media/_spike/counters/reset", (MediaFileStoreCounters counters) =>
        {
            counters.Reset();
            return Results.Ok(new { reset = true });
        }).AllowAnonymous();

        return routes;
    }
}

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Routing;

namespace OrchardCore.Media.Services
{
    /// <summary>
    /// Provides local caching for remote file store files.
    /// </summary>
    public class MediaFileStoreResolverMiddleware
    {
        private readonly ConcurrentDictionary<string, Lazy<Task>> _workers = new(StringComparer.OrdinalIgnoreCase);

        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IMediaFileStoreCache _mediaFileStoreCache;
        private readonly IMediaFileStore _mediaFileStore;

        private readonly PathString _assetsRequestPath;

        public MediaFileStoreResolverMiddleware(
            RequestDelegate next,
            ILogger<MediaFileStoreResolverMiddleware> logger,
            IMediaFileStoreCache mediaFileStoreCache,
            IMediaFileStore mediaFileStore,
            IOptions<MediaOptions> mediaOptions
            )
        {
            _next = next;
            _logger = logger;
            _mediaFileStoreCache = mediaFileStoreCache;
            _mediaFileStore = mediaFileStore;

            _assetsRequestPath = mediaOptions.Value.AssetsRequestPath;
        }

        /// <summary>
        /// Processes a request to determine if it matches a known file from the media file store, and cache it locally.
        /// </summary>
        /// <param name="context"></param>
        public async Task Invoke(HttpContext context)
        {
            // Support only Head requests or Get Requests.
            if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            {
                await _next(context);
                return;
            }

            var validatePath = context.Request.Path.StartsWithNormalizedSegments(_assetsRequestPath, StringComparison.OrdinalIgnoreCase, out var subPath);
            if (!validatePath)
            {
                _logger.LogDebug("Request path {Path} does not match the assets request path {RequestPath}", subPath, _assetsRequestPath);
                await _next(context);
                return;
            }

            // subpath.Value returns an unescaped path value, subPath returns an escaped path value.
            var subPathValue = subPath.Value;

            var isFileCached = await _mediaFileStoreCache.IsCachedAsync(subPathValue);
            if (isFileCached)
            {
                // When multiple requests occur for the same file the download
                // may already be in progress so we wait for it to complete.
                if (_workers.TryGetValue(subPathValue, out var writeTask))
                {
                    await writeTask.Value;
                }

                await _next(context);
                return;
            }

            // When multiple requests occur for the same file we use a Lazy<Task>
            // to initialize the file store request once.
            await _workers.GetOrAdd(subPathValue, x => new Lazy<Task>(async () =>
            {
                try
                {
                    var fileStoreEntry = await _mediaFileStore.GetFileInfoAsync(subPathValue);

                    if (fileStoreEntry != null)
                    {
                        using var stream = await _mediaFileStore.GetFileStreamAsync(fileStoreEntry);
                        await _mediaFileStoreCache.SetCacheAsync(stream, fileStoreEntry, context.RequestAborted);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error, and pass to pipeline to handle as 404.
                    // Multiple requests at the same time will all recieve the same 404
                    // as we use LazyThreadSafetyMode.ExecutionAndPublication.
                    _logger.LogError(ex, "Error retrieving file from media file store for request path {Path}", subPathValue);
                }
                finally
                {
                    _workers.TryRemove(subPathValue, out var writeTask);
                }
            }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;

            // Always call next, this middleware always passes.
            await _next(context);
            return;
        }
    }
}
